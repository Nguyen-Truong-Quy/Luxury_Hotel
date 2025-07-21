using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using CNPM_Luxury.Model;
using CNPM_Project_web.Helpers;

namespace CNPM_Luxury.Areas.Admin.Controllers
{
    public class Trang_ThaiController : Controller
    {
        private Luxury_HotelEntities db = new Luxury_HotelEntities();

        // GET: Admin/Trang_Thai
        public ActionResult Index()
        {
            return View(db.Trang_Thai.ToList());
        }

        // GET: Admin/Trang_Thai/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Trang_Thai trang_Thai = db.Trang_Thai.Find(id);
            if (trang_Thai == null)
            {
                return HttpNotFound();
            }
            return View(trang_Thai);
        }

        // GET: Admin/Trang_Thai/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Admin/Trang_Thai/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ID_Trang_Thai,Ten_Trang_Thai,LoaiTrangThai")] Trang_Thai trang_Thai)
        {
            // Kiểm tra trùng tên trạng thái (không phân biệt hoa thường)
            bool tenBiTrung = db.Trang_Thai
                                .Any(t => t.Ten_Trang_Thai.Trim().ToLower() == trang_Thai.Ten_Trang_Thai.Trim().ToLower());

            if (tenBiTrung)
            {
                ModelState.AddModelError("Ten_Trang_Thai", "Tên trạng thái đã tồn tại.");
            }

            if (ModelState.IsValid)
            {
                db.Trang_Thai.Add(trang_Thai);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(trang_Thai);
        }


        // GET: Admin/Trang_Thai/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Trang_Thai trang_Thai = db.Trang_Thai.Find(id);
            if (trang_Thai == null)
            {
                return HttpNotFound();
            }
            return View(trang_Thai);
        }

        // POST: Admin/Trang_Thai/Edit/5
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


        // GET: Admin/Trang_Thai/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Trang_Thai trang_Thai = db.Trang_Thai.Find(id);
            if (trang_Thai == null)
            {
                return HttpNotFound();
            }
            return View(trang_Thai);
        }

        // POST: Admin/Trang_Thai/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Trang_Thai trang_Thai = db.Trang_Thai.Find(id);
            db.Trang_Thai.Remove(trang_Thai);
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
    }
}
