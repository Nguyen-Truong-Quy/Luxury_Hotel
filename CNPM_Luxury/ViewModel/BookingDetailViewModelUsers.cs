using CNPM_Luxury.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CNPM_Luxury.ViewModel
{
    public class BookingDetailViewModelUsers
    {
        public string BookingID { get; set; }
        public string ID_User { get; set; }
        public string HO_TEN_KH { get; set; }
        public string Ten_Phong { get; set; }
        public string Ma_Phong { get; set; }
        public DateTime? CheckInDate { get; set; }
        public DateTime? CheckOutDate { get; set; }
        public DateTime? Ngay_Tao_Don { get; set; }
        public int So_Nguoi { get; set; }

   
    }
}