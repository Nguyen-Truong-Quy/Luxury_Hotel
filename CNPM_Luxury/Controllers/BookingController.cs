using CNPM_Luxury.Model;
using System;
using System.Data.Entity.Validation;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using System.Data.Entity; // Đảm bảo có dòng này!
using CNPM_Luxury.ViewModel; // Vẫn cần nếu bạn dùng BookingDetailViewModel cho XacNhan
using System.Collections.Generic;
using System.Net;
using System.Web;
using System.IO;


using BCrypt.Net; // THÊM DÒNG NÀY nếu bạn dùng BCrypt để băm mật khẩu


namespace CNPM_Luxury.Controllers
{
    public class BookingController : Controller
    {
        private Luxury_HotelEntities db = new Luxury_HotelEntities();

        // Hàm tạo mật khẩu ngẫu nhiên
        private string GenerateRandomPassword(int length = 8)
        {
            const string validChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890!@#$%^&*";
            Random random = new Random();
            StringBuilder password = new StringBuilder();
            for (int i = 0; i < length; i++)
            {
                password.Append(validChars[random.Next(validChars.Length)]);
            }
            return password.ToString();
        }

        [HttpPost]
        public ActionResult BookRoom(string Email, string HO_TEN_KH, string SDT_KH, string Ma_Phong, DateTime CheckInDate, DateTime CheckOutDate)
        {
            System.Diagnostics.Debug.WriteLine($"Email: {Email}, HO_TEN_KH: {HO_TEN_KH}, SDT_KH: {SDT_KH}, Ma_Phong: {Ma_Phong}, CheckInDate: {CheckInDate}, CheckOutDate: {CheckOutDate}");

            if (string.IsNullOrEmpty(Email) || string.IsNullOrEmpty(HO_TEN_KH) || string.IsNullOrEmpty(SDT_KH) || string.IsNullOrEmpty(Ma_Phong))
            {
                TempData["Error"] = "Vui lòng điền đầy đủ thông tin.";
                return RedirectToAction("SearchingRoom", "Home");
            }

            if (!Email.Contains("@") || !Email.Contains("."))
            {
                TempData["Error"] = "Email không hợp lệ.";
                return RedirectToAction("SearchingRoom", "Home");
            }

            if (!System.Text.RegularExpressions.Regex.IsMatch(SDT_KH, @"^[0-9]{10}$"))
            {
                TempData["Error"] = "Số điện thoại phải có đúng 10 chữ số.";
                return RedirectToAction("SearchingRoom", "Home");
            }

            // Ngày hiện tại được lấy tại múi giờ của máy chủ, cần đảm bảo đồng bộ nếu ứng dụng chạy trên máy chủ khác múi giờ người dùng.
            // Để an toàn hơn cho các ứng dụng thực tế, nên dùng DateTimeOffset hoặc UTC và chuyển đổi khi hiển thị.
            // Hiện tại, dùng DateTime.Now.Date là ổn cho logic này.
            if (CheckInDate < DateTime.Now.Date)
            {
                TempData["Error"] = "Ngày nhận phòng phải lớn hơn hoặc bằng ngày hiện tại.";
                return RedirectToAction("SearchingRoom", "Home");
            }
            if (CheckOutDate <= CheckInDate)
            {
                TempData["Error"] = "Ngày trả phòng phải lớn hơn ngày nhận phòng.";
                return RedirectToAction("SearchingRoom", "Home");
            }

            var user = db.Users.FirstOrDefault(u => u.Email == Email);

            if (user == null)
            {
                int userCount = db.Users.Count() + 1;
                string newUserId = $"User_{userCount:D3}";
                // Vòng lặp để đảm bảo ID không trùng lặp, tăng số chữ số nếu cần
                while (db.Users.Any(u => u.ID_User == newUserId))
                {
                    userCount++;
                    // Tăng số chữ số để tránh trùng lặp nếu số lượng user lớn hơn 999
                    newUserId = $"User_{{userCount:D" + (userCount.ToString().Length > 3 ? userCount.ToString().Length : 3) + "}}";
                }

                string defaultPassword = GenerateRandomPassword(8);

                user = new User
                {
                    ID_User = newUserId,
                    Email = Email,
                    Password = BCrypt.Net.BCrypt.HashPassword(defaultPassword), // Đảm bảo BCrypt.Net đã được cài đặt qua NuGet
                    HO_TEN_KH = HO_TEN_KH,
                    SDT_KH = SDT_KH,
                    ID_Role = 1 // Giả sử 1 là role khách hàng
                };

                db.Users.Add(user);
                try
                {
                    db.SaveChanges();
                    System.Diagnostics.Debug.WriteLine($"User {newUserId} created successfully.");
                }
                catch (DbEntityValidationException ex)
                {
                    var errorMessages = ex.EntityValidationErrors
                        .SelectMany(x => x.ValidationErrors)
                        .Select(x => x.ErrorMessage);
                    var fullErrorMessage = string.Join("; ", errorMessages);
                    TempData["Error"] = "Lỗi xác thực khi tạo người dùng: " + fullErrorMessage;
                    return RedirectToAction("SearchingRoom", "Home");
                }
                catch (Exception ex)
                {
                    TempData["Error"] = "Lỗi không xác định khi tạo người dùng: " + ex.Message;
                    return RedirectToAction("SearchingRoom", "Home");
                }
            }

            var room = db.Rooms.FirstOrDefault(r => r.Ma_Phong == Ma_Phong);
            if (room == null)
            {
                TempData["Error"] = "Mã phòng không tồn tại trong hệ thống.";
                return RedirectToAction("SearchingRoom", "Home");
            }

            // Kiểm tra trạng thái phòng trước khi đặt
            // Giả sử ID_Trang_Thai = 2 là "Trống", 4 là "Đang bảo trì" (có thể đặt trước, tùy logic)
            if (room.ID_Trang_Thai != 2 && room.ID_Trang_Thai != 4) // Ví dụ: chỉ phòng trống hoặc đang bảo trì mới cho phép đặt
            {
                TempData["Error"] = $"Phòng {room.Ten_Phong} hiện không khả dụng để đặt. Trạng thái: {room.Trang_Thai?.Ten_Trang_Thai}";
                return RedirectToAction("SearchingRoom", "Home");
            }

            // Kiểm tra trùng lịch đặt
            var conflictingBookings = db.Bookings
                .Where(b => b.Ma_Phong == Ma_Phong && b.ID_Trang_Thai != 3 && // Không tính các booking đã hủy (ID_Trang_Thai = 3)
                             ((CheckInDate >= b.CheckInDate && CheckInDate < b.CheckOutDate) || // Bắt đầu trong booking khác
                              (CheckOutDate > b.CheckInDate && CheckOutDate <= b.CheckOutDate) || // Kết thúc trong booking khác
                              (CheckInDate <= b.CheckInDate && CheckOutDate >= b.CheckOutDate))) // Bao trùm booking khác
                .Any();
            if (conflictingBookings)
            {
                TempData["Error"] = "Phòng đã được đặt trong khoảng thời gian này.";
                return RedirectToAction("SearchingRoom", "Home");
            }

            int bookingCount = db.Bookings.Count() + 1;
            string newBookingId = $"booking_{bookingCount:D5}";
            // Vòng lặp để đảm bảo ID không trùng lặp, tăng số chữ số nếu cần
            while (db.Bookings.Any(b => b.BookingID == newBookingId))
            {
                bookingCount++;
                newBookingId = $"booking{{bookingCount:D" + (bookingCount.ToString().Length > 5 ? bookingCount.ToString().Length : 5) + "}}";
            }

            var booking = new Booking
            {
                BookingID = newBookingId,
                ID_User = user.ID_User,
                Ma_Phong = Ma_Phong,
                CheckInDate = CheckInDate,
                CheckOutDate = CheckOutDate,
                Ngay_Tao_Don = DateTime.Now,
                ID_Trang_Thai = 1 // Giả sử 1 là trạng thái "Đã đặt" hoặc "Chờ xác nhận"
            };

            db.Bookings.Add(booking);
            try
            {
                System.Diagnostics.Debug.WriteLine($"Tạo booking: ID={booking.BookingID}, User={booking.ID_User}, Phong={booking.Ma_Phong}, CheckIn={booking.CheckInDate}, CheckOut={booking.CheckOutDate}, TrangThai={booking.ID_Trang_Thai}");
                db.SaveChanges();
                System.Diagnostics.Debug.WriteLine($"Booking {booking.BookingID} created successfully.");
            }
            catch (DbEntityValidationException ex)
            {
                var errorMessages = ex.EntityValidationErrors
                    .SelectMany(x => x.ValidationErrors)
                    .Select(x => x.ErrorMessage);
                var fullErrorMessage = string.Join("; ", errorMessages);
                TempData["Error"] = "Lỗi xác thực khi tạo booking: " + fullErrorMessage;
                return RedirectToAction("SearchingRoom", "Home");
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Lỗi không xác định khi tạo booking: " + ex.Message;
                return RedirectToAction("SearchingRoom", "Home");
            }

            // Chuyển hướng đến trang xác nhận chi tiết booking
            return RedirectToAction("XacNhan", new { id = booking.BookingID });
        }

        public ActionResult XacNhan(string id)
        {
            if (string.IsNullOrEmpty(id))
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            // Eager loading tất cả các thông tin cần thiết cho trang xác nhận
            var booking = db.Bookings
                .Include(b => b.Room.Trang_Thai) // Eager load Room và Trang_Thai của Room
                .Include(b => b.Trang_Thai) // Eager load Trang_Thai của Booking
                .Include(b => b.Room.Phong_TienIch.Select(pti => pti.TienIch)) // Eager load Tiện ích của Phòng
                .FirstOrDefault(b => b.BookingID == id);

            System.Diagnostics.Debug.WriteLine($"Booking tìm thấy: {booking?.BookingID}, Room: {booking?.Room?.Ma_Phong}, TrangThaiBooking: {booking?.Trang_Thai?.Ten_Trang_Thai}");

            if (booking == null)
                return HttpNotFound();

            // Khởi tạo BookingDetailViewModel từ đối tượng Booking
            var viewModel = new BookingDetailViewModel
            {
                BookingID = booking.BookingID,
                ID_User = booking.ID_User,
                CheckInDate = booking.CheckInDate,
                CheckOutDate = booking.CheckOutDate,
                Ngay_Tao_Don = booking.Ngay_Tao_Don,
                TrangThaiBooking = booking.Trang_Thai?.Ten_Trang_Thai,
                Ma_Phong = booking.Room?.Ma_Phong,
                Ten_Phong = booking.Room?.Ten_Phong,
                Mo_Ta = booking.Room?.Mo_Ta,
                Gia_Phong = booking.Room?.Gia_Phong,
                So_Nguoi = booking.Room?.So_Nguoi,
                Dia_Diem = booking.Room?.Dia_Diem,
                Anh_Phong = booking.Room?.Anh_Phong,
                TrangThaiPhong = booking.Room?.Trang_Thai?.Ten_Trang_Thai,
                // Lấy danh sách tiện ích, đảm bảo không null
                TienIchList = booking.Room?.Phong_TienIch?
                                     .Select(pti => pti.TienIch?.TenTienIch)
                                     .Where(ti => !string.IsNullOrEmpty(ti))
                                     .ToList() ?? new List<string>()
            };

            return View("XacNhan", viewModel);
        }

        // GET: Booking/TimKiemBooking (Trang để người dùng ẩn danh nhập thông tin)
        public ActionResult TimKiemBooking()
        {
            return View();
        }

        // POST: Booking/KetQuaTimKiemBooking (Xử lý khi người dùng gửi form tìm kiếm)
        [HttpPost]
        [ValidateAntiForgeryToken] // Nên có để tăng bảo mật
        public ActionResult KetQuaTimKiemBooking(string emailHoacSdtHoacBookingId)
        {
            if (string.IsNullOrWhiteSpace(emailHoacSdtHoacBookingId))
            {
                TempData["Error"] = "Vui lòng nhập Email, Số điện thoại hoặc Mã Booking.";
                return RedirectToAction("TimKiemBooking");
            }

            emailHoacSdtHoacBookingId = emailHoacSdtHoacBookingId.Trim();

            // 1. Tìm kiếm theo BookingID chính xác
            var bookingById = db.Bookings
                                .Include(b => b.Room)
                                .Include(b => b.Trang_Thai)
                                .FirstOrDefault(b => b.BookingID == emailHoacSdtHoacBookingId);

            if (bookingById != null)
            {
                // Nếu tìm thấy theo BookingID, chuyển hướng đến trang xác nhận chi tiết booking đó
                return RedirectToAction("XacNhan", new { id = bookingById.BookingID });
            }

            // 2. Tìm kiếm theo Email hoặc SĐT của người dùng
            // Tìm người dùng dựa trên Email hoặc SDT
            var userByContact = db.Users
                                 .FirstOrDefault(u => u.Email == emailHoacSdtHoacBookingId || u.SDT_KH == emailHoacSdtHoacBookingId);

            if (userByContact != null)
            {
                // Nếu tìm thấy người dùng, chuyển hướng đến DanhSachBookingTheoUser với ID của người dùng đó
                return RedirectToAction("DanhSachBookingTheoUser", new { userId = userByContact.ID_User });
            }
            else
            {
                // Nếu không tìm thấy cả BookingID lẫn User ID
                TempData["Error"] = "Không tìm thấy đơn đặt phòng nào hoặc thông tin nhập vào không chính xác.";
                return RedirectToAction("TimKiemBooking");
            }
        }


        // Action để hiển thị danh sách booking cho một userId cụ thể (cho người dùng ẩn danh đã nhập Email/SĐT)
        public ActionResult DanhSachBookingTheoUser(string userId)
        {
            System.Diagnostics.Debug.WriteLine($"DanhSachBookingTheoUser được gọi với userId: '{userId}'");

            if (string.IsNullOrEmpty(userId))
            {
                TempData["Error"] = "⚠️ Không có thông tin người dùng được truyền để xem lịch sử đặt phòng.";
                return RedirectToAction("TimKiemBooking");
            }

            var bookings = db.Bookings
                             .Include(b => b.Room)
                             .Include(b => b.Trang_Thai)
                             .Where(b => b.ID_User == userId)
                             .OrderByDescending(b => b.Ngay_Tao_Don)
                             .ToList();

            System.Diagnostics.Debug.WriteLine($"Số lượng booking tìm thấy cho user '{userId}': {bookings.Count}");

            ViewBag.Debug_UserId = userId;

            if (bookings == null || !bookings.Any())
            {
                TempData["Error"] = "📭 Không có đơn đặt phòng nào được tìm thấy cho thông tin bạn cung cấp.";
                return View("DanhSachBooking", new BookingListViewModel { Bookings = new List<Booking>() });
            }

            return View("DanhSachBooking", new BookingListViewModel { Bookings = bookings });
        }




        [HttpGet]
        public ActionResult ThanhToan(string BookingID)
        {
            if (string.IsNullOrEmpty(BookingID))
            {
                TempData["Error"] = "Thiếu mã booking.";
                return RedirectToAction("TimKiemBooking");
            }

            var booking = db.Bookings.FirstOrDefault(b => b.BookingID == BookingID);

            if (booking == null)
            {
                return HttpNotFound("Không tìm thấy booking.");
            }

            var room = db.Rooms.FirstOrDefault(r => r.Ma_Phong == booking.Ma_Phong);

            if (room == null)
            {
                return HttpNotFound("Không tìm thấy phòng.");
            }

            var viewModel = new ThanhToanViewModel
            {
                BookingID = booking.BookingID,
                SoTien = room.Gia_Phong,
                NoiDungThanhToan = $"Thanh toán cho phòng {room.Ten_Phong}",
                ID_User = booking.ID_User
            };
            return View("ThanhToanForm", viewModel);



        }


        [HttpPost]
        public ActionResult ThanhToan(ThanhToanViewModel model)
        {
            if (string.IsNullOrEmpty(model.BookingID) || model.ImageUpload == null || model.ImageUpload.ContentLength == 0)
            {
                TempData["Error"] = "Vui lòng chọn ảnh thanh toán và kiểm tra mã booking.";
                return RedirectToAction("ThanhToan", new { BookingID = model.BookingID });
            }

            if (!model.SoTien.HasValue || model.SoTien <= 0)
            {
                TempData["Error"] = "Vui lòng nhập số tiền hợp lệ.";
                return RedirectToAction("ThanhToan", new { BookingID = model.BookingID });
            }

            var booking = db.Bookings.FirstOrDefault(b => b.BookingID == model.BookingID);
            if (booking == null)
            {
                TempData["Error"] = "Không tìm thấy đơn đặt phòng.";
                return RedirectToAction("TimKiemBooking");
            }

            try
            {
                string folderPath = Server.MapPath("/Image/ThanhToan");
                if (!Directory.Exists(folderPath))
                    Directory.CreateDirectory(folderPath);

                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(model.ImageUpload.FileName);
                string fullPath = Path.Combine(folderPath, fileName);
                model.ImageUpload.SaveAs(fullPath);

                var thanhToan = new ThanhToan
                {

                    BookingID = model.BookingID,
                    SoTien = model.SoTien, // Đảm bảo giá trị nullable được gán
                    NoiDungThanhToan = model.NoiDungThanhToan,
                    ThoiGianThanhToan = DateTime.Now, // Được gán mặc định, nhưng vẫn có thể null trong model
                    ID_Trang_Thai = 1, // Giả sử 1 là trạng thái "Chờ xác nhận"
                    AnhThanhToan = "~/Image/ThanhToan/" + fileName
                };


                db.ThanhToans.Add(thanhToan);

                // ✅ Cập nhật trạng thái booking sang "Chờ xác nhận" (ID = 6)
                booking.ID_Trang_Thai = 6;

                db.SaveChanges();

           

                TempData["Success"] = "Thanh toán đã được gửi thành công. Chờ xác nhận!";
                return RedirectToAction("ThanhToan", new { BookingID = model.BookingID });
            }
            catch (Exception ex)
            {

                TempData["Error"] = "Đã xảy ra lỗi khi xử lý thanh toán. Vui lòng thử lại.";
                return RedirectToAction("ThanhToan", new { BookingID = model.BookingID });
            }
        }
        //public ActionResult ThongTinChiTiet(string bookingId)
        //{
        //    if (string.IsNullOrEmpty(bookingId))
        //        return View();

        //    var booking = db.Bookings
        //        .Include("User")
        //        .Include("Room")
        //        .Include("Trang_Thai")
        //        .FirstOrDefault(b => b.BookingID == bookingId);

        //    if (booking == null)
        //    {
        //        ViewBag.ErrorMessage = "Không tìm thấy mã booking.";
        //        return View();
        //    }

        //    var danhSachTrangThai = db.Trang_Thai
        //        .Where(t => t.LoaiTrangThai == "Booking")
        //        .Select(t => new SelectListItem
        //        {
        //            Value = t.ID_Trang_Thai.ToString(),
        //            Text = t.Ten_Trang_Thai
        //        }).ToList();

        //    var model = new BookingDetailViewModel
        //    {
        //        BookingID = booking.BookingID,
        //        ID_User = booking.User.ID_User,
        //        HO_TEN_KH = booking.User.HO_TEN_KH,
        //        Ten_Phong = booking.Room.Ten_Phong,
        //        Ma_Phong = booking.Ma_Phong,
        //        CheckInDate = booking.CheckInDate,
        //        CheckOutDate = booking.CheckOutDate,
        //        Ngay_Tao_Don = booking.Ngay_Tao_Don,
        //        So_Nguoi = booking.Room.So_Nguoi,
        //        ID_Trang_Thai = booking.ID_Trang_Thai ?? 0,
        //        TrangThaiList = danhSachTrangThai
        //    };

        //    return View(model);
        //}
        //[HttpPost]
        //public ActionResult CapNhatTrangThai(BookingDisplayViewModel model)
        //{
        //    var booking = db.Bookings.FirstOrDefault(b => b.BookingID == model.BookingID);
        //    if (booking == null)
        //    {
        //        ViewBag.ErrorMessage = "Không tìm thấy mã booking để cập nhật.";
        //        return RedirectToAction("ThongTinChiTiet", new { bookingId = model.BookingID });
        //    }

        //    booking.ID_Trang_Thai = model.ID_Trang_Thai;
        //    db.SaveChanges();

        //    TempData["Success"] = "Cập nhật trạng thái thành công!";
        //    return RedirectToAction("ThongTinChiTiet", new { bookingId = model.BookingID });
        //}








    }
}