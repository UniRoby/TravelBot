namespace TravelBot.Utils
{
    public class CityDialogResult
    {
        public IataInfo iataInfo { get; set; }
        public BookingDetails bookingDetails { get; set; }

        public string promtResult { get; set; }

        public bool isFake { get; set; }
    }
}
