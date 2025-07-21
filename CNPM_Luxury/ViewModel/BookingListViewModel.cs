using CNPM_Luxury.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CNPM_Luxury.ViewModel
{
    public class BookingListViewModel
    {
        public List<Booking> Bookings { get; set; }
        public string UserId { get; set; } // Có thể thêm các thuộc tính khác nếu cần
        // Public string Message { get; set; }
    }
}