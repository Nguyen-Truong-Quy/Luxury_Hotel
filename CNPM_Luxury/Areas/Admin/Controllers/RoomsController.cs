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
            ViewBag.ID_Trang_Thai = new SelectList(db.Trang_Thai, "ID_Trang_Thai", "Ten_Trang_Thai");
            return View();
        }

        // POST: Admin/Rooms/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Ten_Phong,Mo_Ta,Gia_Phong,Dia_Diem,So_Nguoi,Anh_Phong,ID_Trang_Thai")] Room room)
        {
            if (ModelState.IsValid)
            {
                // Sinh mã tự động
                var lastRoom = db.Rooms
                                 .OrderByDescending(r => r.Ma_Phong)
                                 .FirstOrDefault();

                string newCode = "MaPH001"; // mặc định nếu chưa có phòng

                if (lastRoom != null)
                {
                    // Cắt số cuối + tăng lên
                    string lastCode = lastRoom.Ma_Phong;
                    int numberPart = int.Parse(lastCode.Substring(4)); // "MaPH005" -> 5
                    newCode = "MaPH" + (numberPart + 1).ToString("D3"); // -> MaPH006
                }

                room.Ma_Phong = newCode;

                db.Rooms.Add(room);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.ID_Trang_Thai = new SelectList(db.Trang_Thai, "ID_Trang_Thai", "Ten_Trang_Thai", room.ID_Trang_Thai);
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
        public ActionResult Edit([Bind(Include = "Ma_Phong,Ten_Phong,Mo_Ta,Gia_Phong,Dia_Diem,So_Nguoi,Anh_Phong,ID_Trang_Thai")] Room room)
        {
            if (ModelState.IsValid)
            {
                db.Entry(room).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.ID_Trang_Thai = new SelectList(db.Trang_Thai, "ID_Trang_Thai", "Ten_Trang_Thai", room.ID_Trang_Thai);
            return View(room);
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
    }
}
