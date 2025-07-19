using CNPM_Luxury.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;


namespace CNPM_Luxury.ViewModel
{
    public class PhongTienIchViewModel
    {
        public string Ma_Phong { get; set; }

        public List<int> SelectedTienIchIds { get; set; } = new List<int>();

        public List<SelectListItem> AvailableTienIch { get; set; }
    }
}