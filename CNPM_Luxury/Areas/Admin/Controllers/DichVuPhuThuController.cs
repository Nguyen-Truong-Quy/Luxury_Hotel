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
    public class DichVuPhuThuController : Controller
    {
        private Luxury_HotelEntities db = new Luxury_HotelEntities();

        // GET: Admin/DichVuPhuThu
        public ActionResult Index()
        {
            var dichVuPhuThus = db.DichVuPhuThus.Include(d => d.Trang_Thai);
            return View(dichVuPhuThus.ToList());
        }

        // GET: Admin/DichVuPhuThu/Details/5
        public ActionResult Details(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            DichVuPhuThu dichVuPhuThu = db.DichVuPhuThus.Find(id);
            if (dichVuPhuThu == null)
            {
                return HttpNotFound();
            }
            return View(dichVuPhuThu);
        }

        // GET: Admin/DichVuPhuThu/Create
        public ActionResult Create()
        {
            ViewBag.ID_Trang_Thai = new SelectList(db.Trang_Thai, "ID_Trang_Thai", "Ten_Trang_Thai");
            return View();
        }

        // POST: Admin/DichVuPhuThu/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ID_DichVu,Ten_DichVu,Gia_DichVu,ThoiGianMo,ThoiGianKetThuc,ID_Trang_Thai")] DichVuPhuThu dichVuPhuThu)
        {
            if (ModelState.IsValid)
            {
                db.DichVuPhuThus.Add(dichVuPhuThu);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.ID_Trang_Thai = new SelectList(db.Trang_Thai, "ID_Trang_Thai", "Ten_Trang_Thai", dichVuPhuThu.ID_Trang_Thai);
            return View(dichVuPhuThu);
        }

        // GET: Admin/DichVuPhuThu/Edit/5
        public ActionResult Edit(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            DichVuPhuThu dichVuPhuThu = db.DichVuPhuThus.Find(id);
            if (dichVuPhuThu == null)
            {
                return HttpNotFound();
            }
            ViewBag.ID_Trang_Thai = new SelectList(db.Trang_Thai, "ID_Trang_Thai", "Ten_Trang_Thai", dichVuPhuThu.ID_Trang_Thai);
            return View(dichVuPhuThu);
        }

        // POST: Admin/DichVuPhuThu/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ID_DichVu,Ten_DichVu,Gia_DichVu,ThoiGianMo,ThoiGianKetThuc,ID_Trang_Thai")] DichVuPhuThu dichVuPhuThu)
        {
            if (ModelState.IsValid)
            {
                db.Entry(dichVuPhuThu).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.ID_Trang_Thai = new SelectList(db.Trang_Thai, "ID_Trang_Thai", "Ten_Trang_Thai", dichVuPhuThu.ID_Trang_Thai);
            return View(dichVuPhuThu);
        }

        // GET: Admin/DichVuPhuThu/Delete/5
        public ActionResult Delete(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            DichVuPhuThu dichVuPhuThu = db.DichVuPhuThus.Find(id);
            if (dichVuPhuThu == null)
            {
                return HttpNotFound();
            }
            return View(dichVuPhuThu);
        }

        // POST: Admin/DichVuPhuThu/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(string id)
        {
            DichVuPhuThu dichVuPhuThu = db.DichVuPhuThus.Find(id);
            db.DichVuPhuThus.Remove(dichVuPhuThu);
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
