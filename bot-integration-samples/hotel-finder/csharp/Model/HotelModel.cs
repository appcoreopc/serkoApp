using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LuisBot.Model
{
    public class HotelElement
    {
        public string Formatted_address { get; set; }

        public string Name { get; set; }

        public float? Rating { get; set; }
             
    }
    
    public class HotelResult
    {

        public HotelElement[] Results { get; set; }
    }
}