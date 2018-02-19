using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LuisBot.Model
{


    public class BookingInfo
    {
        
        public string FromLocation { get; set; }

        public string ToLocation { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }
        
    }
}