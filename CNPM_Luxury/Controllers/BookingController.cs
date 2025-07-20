using System;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using CNPM_Luxury.Model;
using BCrypt.Net;
using System.Data.Entity.Validation;

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
                    newUserId = $"User_{userCount:D3}";
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
                TempData["Error"] = "Mã phòng không tồn tại.";
                return RedirectToAction("SearchingRoom", "Home");
            }

            // Tạo đơn đặt phòng
            var booking = new Booking
            {
                BookingID = Guid.NewGuid().ToString(),
                ID_User = user.ID_User,
                Ma_Phong = Ma_Phong,
                CheckInDate = CheckInDate,
                CheckOutDate = CheckOutDate,
                Ngay_Tao_Don = DateTime.Now,
                ID_Trang_Thai = 1 // Trạng thái "Chờ xác nhận"
            };

            db.Bookings.Add(booking);
            try
            {
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
            var booking = db.Bookings.Find(id);
            if (booking == null)
            {
                return HttpNotFound();
            }
            return View(booking);
        }
    }
}