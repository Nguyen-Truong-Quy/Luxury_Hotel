using System;
using System.Net;
using System.Net.Mail;
using CNPM_Luxury.Model;

namespace CNPM_Project_web.Helpers
{
    public class EmailService
    {
        public static void SendBookingConfirmation(string toEmail, Booking booking)
        {
            string fromEmail = "quy1chatgpt@gmail.com";
            string fromPassword = "ctcb wbes csrj ybtv"; // App password

            try
            {
                if (booking != null && booking.User != null && booking.Room != null)
                {
                    string subject = "Xác nhận đặt phòng tại Luxury Hotel";

                    string body = $@"
                        <div style='font-family:Arial,sans-serif;padding:20px;border:1px solid #ccc;border-radius:10px;background:#f9f9f9;max-width:600px;margin:auto'>
        <h2 style='color:#007BFF;text-align:center;'>XÁC NHẬN ĐẶT PHÒNG</h2>
        <p>Chào <strong>{booking.User.HO_TEN_KH}</strong>,</p>
        <p>Bạn đã đặt phòng thành công tại <strong>Luxury Hotel</strong>. Dưới đây là thông tin chi tiết:</p>
        <table style='width:100%;border-collapse:collapse'>
            <tr><td><strong>Email:</strong></td><td>{booking.User.Email}</td></tr>
            <tr><td><strong>SĐT:</strong></td><td>{booking.User.SDT_KH}</td></tr>
            <tr><td><strong>Ngày nhận phòng:</strong></td><td>{booking.CheckInDate?.ToString("dd/MM/yyyy")}</td></tr>
            <tr><td><strong>Ngày trả phòng:</strong></td><td>{booking.CheckOutDate?.ToString("dd/MM/yyyy")}</td></tr>
            <tr><td><strong>Phòng:</strong></td><td>{booking.Room.Ten_Phong} (Mã: {booking.Room.Ma_Phong})</td></tr>
            <tr><td><strong>Giá phòng:</strong></td><td>{booking.Room.Gia_Phong} VND</td></tr>
            <tr><td><strong>Trạng thái:</strong></td><td>{booking.Trang_Thai?.Ten_Trang_Thai}</td></tr>
        </table>
        <br />
        <p style='color:#333'>Cảm ơn bạn đã tin tưởng và sử dụng dịch vụ của chúng tôi.</p>
        <p style='text-align:right;'>-- Luxury Hotel --</p>
    </div>";

                    var fromAddress = new MailAddress(fromEmail, "Hệ thống đặt phòng - Luxury Hotel");
                    var toAddress = new MailAddress(toEmail);

                    var smtp = new SmtpClient
                    {
                        Host = "smtp.gmail.com",
                        Port = 587,
                        EnableSsl = true,
                        Credentials = new NetworkCredential(fromEmail, fromPassword)
                    };

                    using (var message = new MailMessage(fromAddress, toAddress)
                    {
                        Subject = subject,
                        Body = body,
                        IsBodyHtml = true
                    })
                    {
                        smtp.Send(message);
                    }
                }
                else
                {
                    Console.WriteLine("Booking hoặc thông tin liên quan bị null.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Lỗi khi gửi email xác nhận đặt phòng: " + ex.Message);
                throw;
            }
        }
    }
}
