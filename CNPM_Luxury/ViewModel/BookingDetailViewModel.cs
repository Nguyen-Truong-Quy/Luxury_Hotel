using System;
using System.Collections.Generic;

namespace CNPM_Luxury.ViewModel
{
    public class BookingDetailViewModel
    {
        // Booking
        public string BookingID { get; set; }
        public string ID_User { get; set; }
        public DateTime? CheckInDate { get; set; }
        public DateTime? CheckOutDate { get; set; }
        public DateTime? Ngay_Tao_Don { get; set; }
        public string TrangThaiBooking { get; set; }

        // Room
        public string Ma_Phong { get; set; }
        public string Ten_Phong { get; set; }
        public string Mo_Ta { get; set; }
        public decimal? Gia_Phong { get; set; }
        public int? So_Nguoi { get; set; }
        public string Dia_Diem { get; set; }
        public string Anh_Phong { get; set; }
        public string TrangThaiPhong { get; set; }

        // Danh sách tiện ích
        public List<string> TienIchList { get; set; } = new List<string>();
    }
}
