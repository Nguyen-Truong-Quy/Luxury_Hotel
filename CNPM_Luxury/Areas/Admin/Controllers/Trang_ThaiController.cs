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
        public ActionResult Create([Bind(Include = "ID_Trang_Thai,Ten_Trang_Thai")] Trang_Thai trang_Thai)
        {
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
        public ActionResult Edit([Bind(Include = "ID_Trang_Thai,Ten_Trang_Thai")] Trang_Thai trang_Thai)
        {
            if (ModelState.IsValid)
            {
                db.Entry(trang_Thai).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(trang_Thai);
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
