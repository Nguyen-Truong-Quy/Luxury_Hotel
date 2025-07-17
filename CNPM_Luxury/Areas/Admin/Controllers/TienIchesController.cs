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
    public class TienIchesController : Controller
    {
        private Luxury_HotelEntities db = new Luxury_HotelEntities();

        // GET: Admin/TienIches
        public ActionResult Index()
        {
            return View(db.TienIches.ToList());
        }

        // GET: Admin/TienIches/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            TienIch tienIch = db.TienIches.Find(id);
            if (tienIch == null)
            {
                return HttpNotFound();
            }
            return View(tienIch);
        }

        // GET: Admin/TienIches/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Admin/TienIches/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ID_TienIch,TenTienIch")] TienIch tienIch)
        {
            if (ModelState.IsValid)
            {
                db.TienIches.Add(tienIch);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(tienIch);
        }

        // GET: Admin/TienIches/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            TienIch tienIch = db.TienIches.Find(id);
            if (tienIch == null)
            {
                return HttpNotFound();
            }
            return View(tienIch);
        }

        // POST: Admin/TienIches/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ID_TienIch,TenTienIch")] TienIch tienIch)
        {
            if (ModelState.IsValid)
            {
                db.Entry(tienIch).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(tienIch);
        }

        // GET: Admin/TienIches/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            TienIch tienIch = db.TienIches.Find(id);
            if (tienIch == null)
            {
                return HttpNotFound();
            }
            return View(tienIch);
        }

        // POST: Admin/TienIches/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            TienIch tienIch = db.TienIches.Find(id);
            db.TienIches.Remove(tienIch);
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
