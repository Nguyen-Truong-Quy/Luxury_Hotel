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
    public class UsersController : Controller
    {
        private Luxury_HotelEntities db = new Luxury_HotelEntities();

        // GET: Admin/Users
        public ActionResult Index()
        {
            var users = db.Users.Include(u => u.Role);
            return View(users.ToList());
        }

        // GET: Admin/Users/Details/5
        public ActionResult Details(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            User user = db.Users.Find(id);
            if (user == null)
            {
                return HttpNotFound();
            }
            return View(user);
        }

        // GET: Admin/Users/Create
        public ActionResult Create()
        {
            ViewBag.ID_Role = new SelectList(db.Roles, "ID_Role", "TEN_ROLE");
            return View();
        }

        // POST: Admin/Users/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Email,Password,HO_TEN_KH,SDT_KH,ID_Role")] User user)
        {
            if (ModelState.IsValid)
            {
                // Tự động sinh ID_User
                int userCount = db.Users.Count() + 1; // Số lượng bản ghi hiện tại + 1
                string newUserId = $"User_{userCount:D3}"; // Định dạng: User_001, User_002, ...

                // Kiểm tra xem ID đã tồn tại chưa (tránh trùng lặp)
                while (db.Users.Any(u => u.ID_User == newUserId))
                {
                    userCount++;
                    newUserId = $"User_{userCount:D3}";
                }

                user.ID_User = newUserId; // Gán ID tự sinh
                db.Users.Add(user);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.ID_Role = new SelectList(db.Roles, "ID_Role", "TEN_ROLE", user.ID_Role);
            return View(user);
        }

        // GET: Admin/Users/Edit/5
        public ActionResult Edit(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            User user = db.Users.Find(id);
            if (user == null)
            {
                return HttpNotFound();
            }
            ViewBag.ID_Role = new SelectList(db.Roles, "ID_Role", "TEN_ROLE", user.ID_Role);
            return View(user);
        }

        // POST: Admin/Users/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ID_User,Email,Password,HO_TEN_KH,SDT_KH,ID_Role")] User user)
        {
            if (ModelState.IsValid)
            {
                db.Entry(user).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.ID_Role = new SelectList(db.Roles, "ID_Role", "TEN_ROLE", user.ID_Role);
            return View(user);
        }

        // GET: Admin/Users/Delete/5
        public ActionResult Delete(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            User user = db.Users.Find(id);
            if (user == null)
            {
                return HttpNotFound();
            }
            return View(user);
        }

        // POST: Admin/Users/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(string id)
        {
            User user = db.Users.Find(id);
            db.Users.Remove(user);
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