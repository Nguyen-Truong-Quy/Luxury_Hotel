using CNPM_Luxury.Model;
using System;
using System.Data.Entity.Validation;
using System.Linq;
using System.Text;
using System.Web.Mvc;
     using System.Data.Entity;
using CNPM_Luxury.ViewModels;
using System.Collections.Generic;
using System.Net;
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
            // Log dữ liệu nhận được
            System.Diagnostics.Debug.WriteLine($"Email: {Email}, HO_TEN_KH: {HO_TEN_KH}, SDT_KH: {SDT_KH}, Ma_Phong: {Ma_Phong}, CheckInDate: {CheckInDate}, CheckOutDate: {CheckOutDate}");

            // Kiểm tra đầu vào
            if (string.IsNullOrEmpty(Email) || string.IsNullOrEmpty(HO_TEN_KH) || string.IsNullOrEmpty(SDT_KH) || string.IsNullOrEmpty(Ma_Phong))
            {
                TempData["Error"] = "Vui lòng điền đầy đủ thông tin.";
                return RedirectToAction("SearchingRoom", "Home");
            }

            // Kiểm tra định dạng email
            if (!Email.Contains("@") || !Email.Contains("."))
            {
                TempData["Error"] = "Email không hợp lệ.";
                return RedirectToAction("SearchingRoom", "Home");
            }

            // Kiểm tra định dạng số điện thoại
            if (!System.Text.RegularExpressions.Regex.IsMatch(SDT_KH, @"^[0-9]{10}$"))
            {
                TempData["Error"] = "Số điện thoại phải có đúng 10 chữ số.";
                return RedirectToAction("SearchingRoom", "Home");
            }

            // Kiểm tra ngày (đảm bảo phù hợp với ngày hiện tại 21/07/2025)
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
                // Sinh ID_User mới
                int userCount = db.Users.Count() + 1;
                string newUserId = $"User_{userCount:D3}";
                while (db.Users.Any(u => u.ID_User == newUserId))
                {
                    userCount++;
                    newUserId = $"User_{userCount:D4}";
                }

                // Gán mật khẩu mặc định
                string defaultPassword = GenerateRandomPassword(8);

                // Tạo user mới
                user = new User
                {
                    ID_User = newUserId,
                    Email = Email,
                    Password = BCrypt.Net.BCrypt.HashPassword(defaultPassword),
                    HO_TEN_KH = HO_TEN_KH,
                    SDT_KH = SDT_KH,
                    ID_Role = 1
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
            }

            // Kiểm tra Ma_Phong tồn tại trong Room
            var room = db.Rooms.FirstOrDefault(r => r.Ma_Phong == Ma_Phong);
            if (room == null)
            {
                TempData["Error"] = "Mã phòng không tồn tại trong hệ thống.";
                return RedirectToAction("SearchingRoom", "Home");
            }

            // Kiểm tra trạng thái phòng (chỉ cho phép đặt nếu trạng thái là "Bình Thường" hoặc "Chưa nhận phòng")
            if (room.ID_Trang_Thai != 2 && room.ID_Trang_Thai != 4) // 2: Bình Thường, 4: Chưa nhận phòng
            {
                TempData["Error"] = "Phòng hiện không khả dụng để đặt.";
                return RedirectToAction("SearchingRoom", "Home");
            }

            // Kiểm tra xung đột đặt phòng
            var conflictingBookings = db.Bookings
                .Where(b => b.Ma_Phong == Ma_Phong &&
                            ((CheckInDate >= b.CheckInDate && CheckInDate < b.CheckOutDate) ||
                             (CheckOutDate > b.CheckInDate && CheckOutDate <= b.CheckOutDate) ||
                             (CheckInDate <= b.CheckInDate && CheckOutDate >= b.CheckOutDate)))
                .Any();
            if (conflictingBookings)
            {
                TempData["Error"] = "Phòng đã được đặt trong khoảng thời gian này.";
                return RedirectToAction("SearchingRoom", "Home");
            }
            System.Diagnostics.Debug.WriteLine("CheckInDate: " + CheckInDate);
            System.Diagnostics.Debug.WriteLine("CheckOutDate: " + CheckOutDate);
            System.Diagnostics.Debug.WriteLine("Ma_Phong: " + Ma_Phong);

            int bookingCount = db.Bookings.Count() + 1;
            string newBookingId = $"booking_{bookingCount:D5}";

            // Kiểm tra trùng (an toàn hơn nếu có xoá dữ liệu hoặc rollback)
            while (db.Bookings.Any(b => b.BookingID == newBookingId))
            {
                bookingCount++;
                newBookingId = $"booking_{bookingCount:D5}";
            }

            // Tạo đơn đặt phòng với ID_Trang_Thai phù hợp (6: Chưa thanh toán)
            var booking = new Booking
            {
                BookingID = newBookingId,
                ID_User = user.ID_User,
                Ma_Phong = Ma_Phong,
                CheckInDate = CheckInDate,
                CheckOutDate = CheckOutDate,
                Ngay_Tao_Don = DateTime.Now,
                ID_Trang_Thai = 1 // Sử dụng "Chưa thanh toán" cho booking
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

            return RedirectToAction("XacNhan", new { id = booking.BookingID });
        }

        public ActionResult XacNhan(string id)
        {
            if (string.IsNullOrEmpty(id))
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var booking = db.Bookings
                .Include("Room.Trang_Thai")
                .Include("Trang_Thai")
                .Include("Room.Phong_TienIch.TienIch")
                .FirstOrDefault(b => b.BookingID == id);

            if (booking == null)
                return HttpNotFound();

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

                TienIchList = booking.Room?.Phong_TienIch?
                                  .Select(pti => pti.TienIch?.TenTienIch)
                                  .Where(ti => !string.IsNullOrEmpty(ti))
                                  .ToList() ?? new List<string>()
            };

            return View("XacNhan", viewModel);
        }


        public ActionResult DanhSachBooking(string userId)
        {
            ViewBag.Debug_UserId = userId;
            if (string.IsNullOrEmpty(userId))
            {
                return HttpNotFound("Không tìm thấy ID người dùng.");
            }

            var bookings = db.Bookings
                             .Where(b => b.ID_User == userId)
                             .OrderByDescending(b => b.Ngay_Tao_Don)
                             .ToList();

            return View(bookings);
        }



    }
}