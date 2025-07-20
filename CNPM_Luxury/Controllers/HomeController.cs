using CNPM_Luxury.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using CNPM_Luxury.ViewModel;

namespace CNPM_Luxury.Controllers
{
    public class HomeController : Controller
    {
        private Luxury_HotelEntities db = new Luxury_HotelEntities();

        public ActionResult Index()
        {
            var rooms = db.Rooms.Include(r => r.Trang_Thai).ToList();
            return View(rooms);
        }
        public ActionResult _LayoutHome()
        {
            return View();
        }

        public ActionResult Index2()
        {
            return View();
        }
        public ActionResult Login()
        {
            return View();
        }
        public ActionResult Register()
        {
            return View();
        }
        public ActionResult InfoUser()
        {
            return View();
        }
        public ActionResult Product()
        {

            return View();
        }
        public ActionResult Header()
        {
            return View();
        }
        public ActionResult ForgotPass()
        {
            return View();
        }

        public ActionResult DichVu()
        {
            return View();
        }

        public ActionResult ChiTietPhong(string id)
        {
            var room = db.Rooms.FirstOrDefault(r => r.Ma_Phong == id);
            if (room == null)
            {
                return HttpNotFound();
            }

            var tienIchList = db.Phong_TienIch
                                .Where(pt => pt.Ma_Phong == id)
                                .Select(pt => pt.TienIch)
                                .ToList();

            var viewModel = new RoomDetailViewModel
            {
                Room = room,
                TienIchList = tienIchList
            };

            return View(viewModel);
        }


        public ActionResult DanhSachPhong()
        {
            return View();
        }



    }
}