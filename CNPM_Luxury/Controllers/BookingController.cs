using CNPM_Luxury.Model;
using System;
using System.Data.Entity.Validation;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using System.Data.Entity; // Đảm bảo có dòng này!
using CNPM_Luxury.ViewModels; // Vẫn cần nếu bạn dùng BookingDetailViewModel cho XacNhan
using System.Collections.Generic;
using System.Net;
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
                    newUserId = $"User_{userCount:D" + (userCount.ToString().Length > 3 ? userCount.ToString().Length : 3) + "}";
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
                newBookingId = $"booking_{bookingCount:D" + (bookingCount.ToString().Length > 5 ? bookingCount.ToString().Length : 5) + "}";
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
                Gia_Phong = booking.Room?.Gia_Phong ?? 0,
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
                return RedirectToAction("TimKiemBooking"); // Chuyển hướng lại trang tìm kiếm nếu không có userId
            }

            // Eager loading Room và Trang_Thai để tránh lỗi DynamicProxies trên View
            var bookings = db.Bookings
                             .Include(b => b.Room) // Tải thông tin phòng
                             .Include(b => b.Trang_Thai) // Tải thông tin trạng thái
                             .Where(b => b.ID_User == userId)
                             .OrderByDescending(b => b.Ngay_Tao_Don)
                             .ToList();

            System.Diagnostics.Debug.WriteLine($"Số lượng booking tìm thấy cho user '{userId}': {bookings.Count}");

            ViewBag.Debug_UserId = userId; // Vẫn giữ để debug trên View

            if (bookings == null || !bookings.Any())
            {
                TempData["Error"] = "📭 Không có đơn đặt phòng nào được tìm thấy cho thông tin bạn cung cấp.";
                // Vẫn trả về view DanhSachBooking nhưng với danh sách rỗng, để thông báo hiển thị
                return View("DanhSachBooking", new List<Booking>());
            }

            // Trả về view DanhSachBooking với danh sách các đối tượng Booking
            return View("DanhSachBooking", bookings);
        }
    }
}