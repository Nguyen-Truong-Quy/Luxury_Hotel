using CNPM_Luxury.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CNPM_Luxury.ViewModel
{
    public class RoomDetailViewModel
    {
        public Room Room { get; set; }
        public List<TienIch> TienIchList { get; set; }
    }
}