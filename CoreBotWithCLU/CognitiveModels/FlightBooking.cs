﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Linq;
using Microsoft.Bot.Builder;
using TravelBot.Clu;
using Newtonsoft.Json;

namespace TravelBot
{
    /// <summary>
    /// An <see cref="IRecognizerConvert"/> implementation that provides helper methods and properties to interact with
    /// the CLU recognizer results.
    /// </summary>
    public class FlightBooking : IRecognizerConvert
    {
        public enum Intent
        {
            BookFlight,
            GetEmail,
            Cancel,
            None
        }

        public string Text { get; set; }

        public string AlteredText { get; set; }

        public Dictionary<Intent, IntentScore> Intents { get; set; }

        public CluEntities Entities { get; set; }

        public IDictionary<string, object> Properties { get; set; }

        public void Convert(dynamic result)
        {
            var jsonResult = JsonConvert.SerializeObject(result, new JsonSerializerSettings {NullValueHandling = NullValueHandling.Ignore});
            var app = JsonConvert.DeserializeObject<FlightBooking>(jsonResult);

            Text = app.Text;
            AlteredText = app.AlteredText;
            Intents = app.Intents;
            Entities = app.Entities;
            Properties = app.Properties;
        }

        public (Intent intent, double score) GetTopIntent()
        {
            var maxIntent = Intent.None;
            var max = 0.0;
            foreach (var entry in Intents)
            {
                if (entry.Value.Score > max)
                {
                    maxIntent = entry.Key;
                    max = entry.Value.Score.Value;
                }
            }

            return (maxIntent, max);
        }

        public class CluEntities
        {
            public CluEntity[] Entities;

            public CluEntity[] GetFromCityList() => Entities.Where(e => e.Category == "Partenza").ToArray();

            public CluEntity[] GetToCityList() => Entities.Where(e => e.Category == "Destinazione").ToArray();

            public CluEntity[] GetFlightDateList() => Entities.Where(e => e.Category == "DataPartenza").ToArray();

            public CluEntity[] GetReturnFlightDateList() => Entities.Where(e => e.Category == "DataRitorno").ToArray();

            public CluEntity[] GetPassengersNumberList() => Entities.Where(e => e.Category == "NumeroPasseggeri").ToArray();

            public CluEntity[] GetBudgetList() => Entities.Where(e => e.Category == "Budget").ToArray();

            public CluEntity[] GetEmailList() => Entities.Where(e => e.Category == "email").ToArray();

            public string GetFromCity() => GetFromCityList().FirstOrDefault()?.Text;

            public string GetToCity() => GetToCityList().FirstOrDefault()?.Text;

            public string GetFlightDate() => GetFlightDateList().FirstOrDefault()?.Text;

            public string GetReturnFlightDate() => GetReturnFlightDateList().FirstOrDefault()?.Text;

            public string GetPassengersNumber() => GetPassengersNumberList().FirstOrDefault()?.Text;

            public string GetBudget() => GetBudgetList().FirstOrDefault()?.Text;

            public string GetEmail() => GetEmailList().FirstOrDefault()?.Text;

        }
    }
}
