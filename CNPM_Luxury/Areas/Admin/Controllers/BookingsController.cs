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
    public class BookingsController : Controller
    {
        private Luxury_HotelEntities db = new Luxury_HotelEntities();

        // GET: Admin/Bookings
        public ActionResult Index()
        {
            var bookings = db.Bookings.Include(b => b.Trang_Thai).Include(b => b.User).Include(b => b.Room);
            return View(bookings.ToList());
        }

        // GET: Admin/Bookings/Details/5
        public ActionResult Details(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Booking booking = db.Bookings.Find(id);
            if (booking == null)
            {
                return HttpNotFound();
            }
            return View(booking);
        }

        // GET: Admin/Bookings/Create
        public ActionResult Create()
        {
            ViewBag.ID_Trang_Thai = new SelectList(db.Trang_Thai, "ID_Trang_Thai", "Ten_Trang_Thai");
            ViewBag.ID_User = new SelectList(db.Users, "ID_User", "Email");
            ViewBag.Ma_Phong = new SelectList(db.Rooms, "Ma_Phong", "Ten_Phong");
            return View();
        }

        // POST: Admin/Bookings/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "BookingID,ID_User,Ma_Phong,CheckInDate,CheckOutDate,Ngay_Tao_Don,ID_Trang_Thai")] Booking booking)
        {
            if (ModelState.IsValid)
            {
                db.Bookings.Add(booking);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.ID_Trang_Thai = new SelectList(db.Trang_Thai, "ID_Trang_Thai", "Ten_Trang_Thai", booking.ID_Trang_Thai);
            ViewBag.ID_User = new SelectList(db.Users, "ID_User", "Email", booking.ID_User);
            ViewBag.Ma_Phong = new SelectList(db.Rooms, "Ma_Phong", "Ten_Phong", booking.Ma_Phong);
            return View(booking);
        }

        // GET: Admin/Bookings/Edit/5
        public ActionResult Edit(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Booking booking = db.Bookings.Find(id);
            if (booking == null)
            {
                return HttpNotFound();
            }
            ViewBag.ID_Trang_Thai = new SelectList(db.Trang_Thai, "ID_Trang_Thai", "Ten_Trang_Thai", booking.ID_Trang_Thai);
            ViewBag.ID_User = new SelectList(db.Users, "ID_User", "Email", booking.ID_User);
            ViewBag.Ma_Phong = new SelectList(db.Rooms, "Ma_Phong", "Ten_Phong", booking.Ma_Phong);
            return View(booking);
        }

        // POST: Admin/Bookings/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "BookingID,ID_User,Ma_Phong,CheckInDate,CheckOutDate,Ngay_Tao_Don,ID_Trang_Thai")] Booking booking)
        {
            if (ModelState.IsValid)
            {
                db.Entry(booking).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.ID_Trang_Thai = new SelectList(db.Trang_Thai, "ID_Trang_Thai", "Ten_Trang_Thai", booking.ID_Trang_Thai);
            ViewBag.ID_User = new SelectList(db.Users, "ID_User", "Email", booking.ID_User);
            ViewBag.Ma_Phong = new SelectList(db.Rooms, "Ma_Phong", "Ten_Phong", booking.Ma_Phong);
            return View(booking);
        }

        // GET: Admin/Bookings/Delete/5
        public ActionResult Delete(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Booking booking = db.Bookings.Find(id);
            if (booking == null)
            {
                return HttpNotFound();
            }
            return View(booking);
        }

        // POST: Admin/Bookings/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(string id)
        {
            Booking booking = db.Bookings.Find(id);
            db.Bookings.Remove(booking);
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
