using System;
using System.Collections.Generic;

namespace TimeTriggerEmail.Models;

public partial class Airport
{
    public int Id { get; set; }

    public string IcaoCode { get; set; }

    public string IataCode { get; set; }

    public string Name { get; set; }

    public string City { get; set; }

    public string Country { get; set; }

    public string CityIt { get; set; }
}
