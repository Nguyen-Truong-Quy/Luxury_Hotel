using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CNPM_Luxury.ViewModel
{
    public class ThanhToanViewModel
    {
        public string BookingID { get; set; }
        public decimal? SoTien { get; set; } // Thay đổi thành nullable để khớp với model
        public string NoiDungThanhToan { get; set; }
        public HttpPostedFileBase ImageUpload { get; set; }
        public string ID_User { get; set; }
        
    }
}