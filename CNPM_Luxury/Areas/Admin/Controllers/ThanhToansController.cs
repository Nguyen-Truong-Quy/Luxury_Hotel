using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using CNPM_Luxury.Model;

namespace CNPM_Luxury.Areas.Admin.Controllers
{
    public class ThanhToansController : Controller
    {
        private Luxury_HotelEntities db = new Luxury_HotelEntities();

        // GET: Admin/ThanhToans
        public ActionResult Index()
        {
            var thanhToans = db.ThanhToans.Include(t => t.Booking).Include(t => t.Trang_Thai);
            return View(thanhToans.ToList());
        }

        // GET: Admin/ThanhToans/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ThanhToan thanhToan = db.ThanhToans.Find(id);
            if (thanhToan == null)
            {
                return HttpNotFound();
            }
            return View(thanhToan);
        }

        // GET: Admin/ThanhToans/Create
        public ActionResult Create()
        {
            ViewBag.BookingID = new SelectList(db.Bookings, "BookingID", "ID_User");
            ViewBag.ID_Trang_Thai = new SelectList(db.Trang_Thai, "ID_Trang_Thai", "Ten_Trang_Thai");
            return View();
        }

        // POST: Admin/ThanhToans/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ID_ThanhToan,BookingID,SoTien,ThoiGianThanhToan,NoiDungThanhToan,ID_Trang_Thai,AnhThanhToan")] ThanhToan thanhToan)
        {
            if (ModelState.IsValid)
            {
                db.ThanhToans.Add(thanhToan);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.BookingID = new SelectList(db.Bookings, "BookingID", "ID_User", thanhToan.BookingID);
            ViewBag.ID_Trang_Thai = new SelectList(db.Trang_Thai, "ID_Trang_Thai", "Ten_Trang_Thai", thanhToan.ID_Trang_Thai);
            return View(thanhToan);
        }

        // GET: Admin/ThanhToans/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ThanhToan thanhToan = db.ThanhToans.Find(id);
            if (thanhToan == null)
            {
                return HttpNotFound();
            }
            ViewBag.BookingID = new SelectList(db.Bookings, "BookingID", "ID_User", thanhToan.BookingID);
            ViewBag.ID_Trang_Thai = new SelectList(db.Trang_Thai, "ID_Trang_Thai", "Ten_Trang_Thai", thanhToan.ID_Trang_Thai);
            return View(thanhToan);
        }

        // POST: Admin/ThanhToans/Edit/5
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
                return RedirectToAction("Index");
            }
            ViewBag.BookingID = new SelectList(db.Bookings, "BookingID", "ID_User", thanhToan.BookingID);
            ViewBag.ID_Trang_Thai = new SelectList(db.Trang_Thai, "ID_Trang_Thai", "Ten_Trang_Thai", thanhToan.ID_Trang_Thai);
            return View(thanhToan);
        }

        // GET: Admin/ThanhToans/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ThanhToan thanhToan = db.ThanhToans.Find(id);
            if (thanhToan == null)
            {
                return HttpNotFound();
            }
            return View(thanhToan);
        }

        // POST: Admin/ThanhToans/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            ThanhToan thanhToan = db.ThanhToans.Find(id);
            db.ThanhToans.Remove(thanhToan);
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
