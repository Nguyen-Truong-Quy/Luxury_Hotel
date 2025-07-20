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
        // GET: Home/SearchingRoom
        public ActionResult SearchingRoom()
        {
            // Chuẩn bị dữ liệu cho dropdown
            ViewBag.DiaDiems = new SelectList(db.Rooms.Select(r => r.Dia_Diem).Distinct(), "Chọn địa điểm");
            ViewBag.SoKhachList = new SelectList(db.Rooms.Select(r => r.So_Nguoi).Distinct(), "Chọn số khách");
            ViewBag.CheckIn = DateTime.Now.AddDays(1).ToString("yyyy-MM-dd");
            ViewBag.CheckOut = DateTime.Now.AddDays(2).ToString("yyyy-MM-dd");
            return View();
        }

        // POST: Home/SearchResult
        [HttpPost]
        public ActionResult SearchResult(string Dia_Diem, int? So_Nguoi, DateTime checkIn, DateTime checkOut)
        {
            // Kiểm tra ngày
            if (checkIn < DateTime.Now.Date)
            {
                ModelState.AddModelError("checkIn", "Ngày nhận phòng phải lớn hơn hoặc bằng ngày hiện tại.");
            }
            if (checkOut <= checkIn)
            {
                ModelState.AddModelError("checkOut", "Ngày trả phòng phải lớn hơn ngày nhận phòng.");
            }
            if (!ModelState.IsValid)
            {
                ViewBag.DiaDiems = new SelectList(db.Rooms.Select(r => r.Dia_Diem).Distinct(), Dia_Diem);
                ViewBag.SoKhachList = new SelectList(db.Rooms.Select(r => r.So_Nguoi).Distinct(), So_Nguoi.ToString());
                ViewBag.CheckIn = checkIn.ToString("yyyy-MM-dd");
                ViewBag.CheckOut = checkOut.ToString("yyyy-MM-dd");
                return View("SearchingRoom");
            }

            // Lưu vào TempData
            TempData["CheckInDate"] = checkIn;
            TempData["CheckOutDate"] = checkOut;
            TempData["Dia_Diem"] = Dia_Diem;
            TempData["So_Nguoi"] = So_Nguoi;

            // Lọc phòng theo Dia_Diem và So_Nguoi
            var rooms = db.Rooms
                .Where(r => (string.IsNullOrEmpty(Dia_Diem) || r.Dia_Diem == Dia_Diem) &&
                            (!So_Nguoi.HasValue || r.So_Nguoi >= So_Nguoi))
                .ToList();

            return View("SearchResult", rooms);
        }



    }
}