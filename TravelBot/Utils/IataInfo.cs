using System.Collections.Generic;
namespace TravelBot.Utils
{
    public class IataInfo
    {
        public List<string> IATAlistOrigin   { get; set; }
        public List<string> IATAlistDestination  { get; set; }
        public int originIATAIndex  { get; set; }
        public int destinationIATAIndex  { get; set; }
        public bool allAirportsOrigin    { get; set; }
        public bool allAirportsDestination   { get; set; }

        public IataInfo()
        {
            allAirportsOrigin = true;
            allAirportsDestination = true;
        }
    }
}
