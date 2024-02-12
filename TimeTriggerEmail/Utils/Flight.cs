using System;


namespace TimeTriggerEmail.Utils
{
    public class Flight
    {
       

        public string departureAirport { get; set; }
        public string arrivalAirport { get; set; }
        public string departureAirportIATA { get; set; }
        public string arrivalAirportIATA { get; set; }
        public string airline { get; set; }
        public string flightNumber { get; set; }
        public int duration { get; set; }
        public string departureTime { get; set; }
        public string arrivalTime { get; set; }
        public int layoverDuration { get; set; }
        public string layoverAirport { get; set; }
        public string airlineLogo { get; set; }
  

        public Flight() { }
    }
}
