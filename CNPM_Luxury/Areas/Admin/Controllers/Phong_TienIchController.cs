using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using CNPM_Luxury.Model;
using CNPM_Luxury.ViewModel;

namespace CNPM_Luxury.Areas.Admin.Controllers
{
    public class Phong_TienIchController : Controller
    {
        private Luxury_HotelEntities db = new Luxury_HotelEntities();

        // GET: Admin/Phong_TienIch
        public ActionResult Index()
        {
            var phong_TienIch = db.Phong_TienIch.Include(p => p.TienIch).Include(p => p.Room).Include(p => p.Room);
            return View(phong_TienIch.ToList());
        }

        // GET: Admin/Phong_TienIch/Details/5
        public ActionResult Details(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Phong_TienIch phong_TienIch = db.Phong_TienIch.Find(id);
            if (phong_TienIch == null)
            {
                return HttpNotFound();
            }
            return View(phong_TienIch);
        }

        // GET: Admin/Phong_TienIch/Create
        public ActionResult Create()
        {
            ViewBag.ID_TienIch = new SelectList(db.TienIches, "ID_TienIch", "TenTienIch");
            ViewBag.Ma_Phong = new SelectList(db.Rooms, "Ma_Phong", "Ten_Phong");
            ViewBag.Ma_Phong = new SelectList(db.Rooms, "Ma_Phong", "Ten_Phong");
            return View();
        }

        // POST: Admin/Phong_TienIch/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Ma_Phong,ID_TienIch,Ngay_Them_Tien_Ich_Phong")] Phong_TienIch phong_TienIch)
        {
            if (ModelState.IsValid)
            {
                db.Phong_TienIch.Add(phong_TienIch);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.ID_TienIch = new SelectList(db.TienIches, "ID_TienIch", "TenTienIch", phong_TienIch.ID_TienIch);
            ViewBag.Ma_Phong = new SelectList(db.Rooms, "Ma_Phong", "Ten_Phong", phong_TienIch.Ma_Phong);
            ViewBag.Ma_Phong = new SelectList(db.Rooms, "Ma_Phong", "Ten_Phong", phong_TienIch.Ma_Phong);
            return View(phong_TienIch);
        }

        // GET: Admin/Phong_TienIch/Edit/5
        public ActionResult Edit(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Phong_TienIch phong_TienIch = db.Phong_TienIch.Find(id);
            if (phong_TienIch == null)
            {
                return HttpNotFound();
            }
            ViewBag.ID_TienIch = new SelectList(db.TienIches, "ID_TienIch", "TenTienIch", phong_TienIch.ID_TienIch);
            ViewBag.Ma_Phong = new SelectList(db.Rooms, "Ma_Phong", "Ten_Phong", phong_TienIch.Ma_Phong);
            ViewBag.Ma_Phong = new SelectList(db.Rooms, "Ma_Phong", "Ten_Phong", phong_TienIch.Ma_Phong);
            return View(phong_TienIch);
        }

        // POST: Admin/Phong_TienIch/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Ma_Phong,ID_TienIch,Ngay_Them_Tien_Ich_Phong")] Phong_TienIch phong_TienIch)
        {
            if (ModelState.IsValid)
            {
                db.Entry(phong_TienIch).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.ID_TienIch = new SelectList(db.TienIches, "ID_TienIch", "TenTienIch", phong_TienIch.ID_TienIch);
            ViewBag.Ma_Phong = new SelectList(db.Rooms, "Ma_Phong", "Ten_Phong", phong_TienIch.Ma_Phong);
            ViewBag.Ma_Phong = new SelectList(db.Rooms, "Ma_Phong", "Ten_Phong", phong_TienIch.Ma_Phong);
            return View(phong_TienIch);
        }

        // GET: Admin/Phong_TienIch/Delete/5
        public ActionResult Delete(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Phong_TienIch phong_TienIch = db.Phong_TienIch.Find(id);
            if (phong_TienIch == null)
            {
                return HttpNotFound();
            }
            return View(phong_TienIch);
        }

        // POST: Admin/Phong_TienIch/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(string id)
        {
            Phong_TienIch phong_TienIch = db.Phong_TienIch.Find(id);
            db.Phong_TienIch.Remove(phong_TienIch);
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
        public ActionResult GanTienIch(string maPhong)
        {
            var allTienIch = db.TienIches.ToList();
            var model = new PhongTienIchViewModel
            {
                Ma_Phong = maPhong,
                AvailableTienIch = allTienIch.Select(t => new SelectListItem
                {
                    Value = t.ID_TienIch.ToString(),
                    Text = t.TenTienIch
                }).ToList()
            };
            return View(model);
        }

        [HttpPost]
        public ActionResult GanTienIch(PhongTienIchViewModel model)
        {
            foreach (var idTienIch in model.SelectedTienIchIds)
            {
                var record = new Phong_TienIch
                {
                    Ma_Phong = model.Ma_Phong,
                    ID_TienIch = idTienIch,
                    Ngay_Them_Tien_Ich_Phong = DateTime.Now
                };
                db.Phong_TienIch.Add(record);
            }
            db.SaveChanges();
            return RedirectToAction("Index", "Rooms");
        }

    }
}
