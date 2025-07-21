using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using CNPM_Luxury.Model;
using CNPM_Project_web.Helpers;

namespace CNPM_Luxury.Areas.Admin.Controllers
{
    public class RoomsController : Controller
    {
        private Luxury_HotelEntities db = new Luxury_HotelEntities();
        //[Authorize(Roles = "Admin")]  // Chặn không phải Admin
        // GET: Admin/Rooms
        public ActionResult Index()
        {
            var rooms = db.Rooms.Include(r => r.Trang_Thai);
            return View(rooms.ToList());
        }

        // GET: Admin/Rooms/Details/5
        public ActionResult Details(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Room room = db.Rooms.Find(id);
            if (room == null)
            {
                return HttpNotFound();
            }
            return View(room);
        }

        // GET: Admin/Rooms/Create
        public ActionResult Create()
        {
            ViewBag.ID_Trang_Thai = new SelectList(db.Trang_Thai.Where(t => t.LoaiTrangThai == "Phòng"), "ID_Trang_Thai", "Ten_Trang_Thai");
            return View();
        }


        // POST: Admin/Rooms/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Ten_Phong,Mo_Ta,Gia_Phong,Dia_Diem,So_Nguoi,ID_Trang_Thai")] Room room, HttpPostedFileBase ImageUpload)
        {
            // Lấy mã phòng cuối cùng trong CSDL theo thứ tự giảm dần
            var lastRoom = db.Rooms
                .OrderByDescending(r => r.Ma_Phong)
                .FirstOrDefault();

            // Tạo số thứ tự mới
            int nextIndex = 1; // mặc định nếu chưa có phòng nào

            if (lastRoom != null && lastRoom.Ma_Phong.StartsWith("Phong"))
            {
                string lastNumber = lastRoom.Ma_Phong.Substring(5); // lấy phần số, sau "Phong"
                if (int.TryParse(lastNumber, out int num))
                    nextIndex = num + 1;
            }

            // Tạo mã mới với định dạng Phong001
            room.Ma_Phong = "Phong" + nextIndex.ToString("D3"); // D3 = 3 chữ số
            if (room.So_Nguoi <= 0)
                ModelState.AddModelError("So_Nguoi", "Số người phải lớn hơn 0");
            if (ModelState.IsValid)
            {
                try
                {
                    if (ImageUpload != null && ImageUpload.ContentLength > 0)
                    {
                        var folderPath = Server.MapPath("~/Image/Room");
                        if (!Directory.Exists(folderPath))
                            Directory.CreateDirectory(folderPath);

                        var fileName = Path.GetFileName(ImageUpload.FileName);
                        var uniqueName = Guid.NewGuid().ToString() + "_" + fileName;
                        var fullPath = Path.Combine(folderPath, uniqueName);
                        ImageUpload.SaveAs(fullPath);

                        room.Anh_Phong = "~/Image/Room/" + uniqueName;
                    }
                    else
                    {
                        room.Anh_Phong = "~/Image/Room/default.png"; // ảnh mặc định nếu không upload
                    }


                    db.Rooms.Add(room);
                    db.SaveChanges();
                    return RedirectToAction("GanTienIch", "Phong_TienIch", new { maPhong = room.Ma_Phong });

                }
                catch (DbEntityValidationException ex)
                {
                    foreach (var errorSet in ex.EntityValidationErrors)
                    {
                        foreach (var error in errorSet.ValidationErrors)
                        {
                            ModelState.AddModelError(error.PropertyName, error.ErrorMessage);
                        }
                    }
                }
            }

            ViewBag.ID_Trang_Thai = new SelectList(db.Trang_Thai.Where(t => t.LoaiTrangThai == "Phòng"), "ID_Trang_Thai", "Ten_Trang_Thai", room.ID_Trang_Thai);
            return View(room);
        }


        // GET: Admin/Rooms/Edit/5
        public ActionResult Edit(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Room room = db.Rooms.Find(id);
            if (room == null)
            {
                return HttpNotFound();
            }
            ViewBag.ID_Trang_Thai = new SelectList(db.Trang_Thai, "ID_Trang_Thai", "Ten_Trang_Thai", room.ID_Trang_Thai);
            return View(room);
        }

        // POST: Admin/Rooms/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ID_ThanhToan,BookingID,SoTien,ThoiGianThanhToan,NoiDungThanhToan,ID_Trang_Thai,AnhThanhToan")] ThanhToan thanhToan)
        {
            if (ModelState.IsValid)
            {
                db.Entry(thanhToan).State = EntityState.Modified;
                db.SaveChanges();

                // Gửi email + cập nhật trạng thái Booking nếu đã thanh toán
                if (thanhToan.ID_Trang_Thai == 3)
                {
                    var booking = db.Bookings
                        .Include("User")
                        .Include("Room")
                        .Include("Trang_Thai")
                        .FirstOrDefault(b => b.BookingID == thanhToan.BookingID);

                    if (booking != null)
                    {
                        // Cập nhật trạng thái nhận phòng (ID = 4 là "Chưa nhận phòng")
                        booking.ID_Trang_Thai = 4;
                        db.Entry(booking).State = EntityState.Modified;
                        db.SaveChanges();

                        // Gửi email xác nhận
                        try
                        {
                            EmailService.SendBookingConfirmation(booking.User.Email, booking);
                        }
                        catch (Exception ex)
                        {
                            // Ghi log nếu cần
                            Console.WriteLine("Lỗi khi gửi email xác nhận: " + ex.Message);
                        }
                    }
                }

                return RedirectToAction("Index");
            }

            ViewBag.BookingID = new SelectList(db.Bookings, "BookingID", "ID_User", thanhToan.BookingID);
            ViewBag.ID_Trang_Thai = new SelectList(db.Trang_Thai, "ID_Trang_Thai", "Ten_Trang_Thai", thanhToan.ID_Trang_Thai);
            return View(thanhToan);
        }



        // GET: Admin/Rooms/Delete/5
        public ActionResult Delete(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Room room = db.Rooms.Find(id);
            if (room == null)
            {
                return HttpNotFound();
            }
            return View(room);
        }

        // POST: Admin/Rooms/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(string id)
        {
            Room room = db.Rooms.Find(id);
            db.Rooms.Remove(room);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
        [HttpPost]
        public ActionResult SearchResult(string Dia_Diem, DateTime checkIn, DateTime checkOut, int So_Nguoi)
        {
            // Giả sử bạn có DbContext là db
            var rooms = db.Rooms.Where(r =>
                r.Dia_Diem == Dia_Diem &&
                r.So_Nguoi >= So_Nguoi 
              
                     )
            .ToList();

            ViewBag.DiaDiem = Dia_Diem;
            ViewBag.SoNguoi = So_Nguoi;

            return View(rooms); // Trả về view kết quả với danh sách phòng phù hợp
        }

    }
}
