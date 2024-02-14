using System;
using System.Collections.Generic;

namespace TravelBot.Utils
{
    public class FlightInfo
    {



        public int totalDuration { get; set; }
        public double price { get; set; }
        public string type { get; set; }
        public List<Flight> flights { get; set; }

        public FlightInfo() { }
    }
}