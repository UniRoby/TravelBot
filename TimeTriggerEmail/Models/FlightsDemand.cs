using System;
using System.Collections.Generic;

namespace TimeTriggerEmail.Models;

public partial class FlightsDemand
{
    public int DemandId { get; set; }

    public string Email { get; set; }

    public string Origin { get; set; }

    public string Destination { get; set; }

    public string DepartureDate { get; set; }

    public string ReturnDate { get; set; }

    public double CurrentPrice { get; set; }

    public double NewPrice { get; set; }

    public int Passengers { get; set; }

    public string Notify {  get; set; }
}
