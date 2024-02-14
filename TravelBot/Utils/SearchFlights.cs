using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;
using Newtonsoft.Json.Linq;
using SerpApi;
using System.Net.Http;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Configuration;
using System.IO;
using Microsoft.IdentityModel.Tokens;




namespace TravelBot.Utils
{
    public class SearchFlights
    {

        private Hashtable ht = new Hashtable();
        private string apiKey;

        public int passengers {  get; set; }

        public SearchFlights(string departureIATA, string arrivalIATA,string departureDate, string returnDate, int passengers, double maxPrice) {

            this.passengers = passengers;

            var builder = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("appsettings.json");

            var config = builder.Build();

            // Ottieni la connection string e assegnala a una variabile di istanza
            apiKey = config.GetConnectionString("SEARCH_FLIGHT_API_KEY");

            // apiKey =  Environment.GetEnvironmentVariable("SEARCH_FLIGHT_API_KEY");

            ht.Add("engine", "google_flights");
            ht.Add("hl", "it");
            ht.Add("gl", "it");
            ht.Add("currency", "EUR");
            ht.Add("departure_id", departureIATA);
            ht.Add("arrival_id", arrivalIATA);
            ht.Add("outbound_date", departureDate);
            

            if (passengers > 0)
            {
                ht.Add("adults", passengers.ToString());
            }
            
            if (returnDate != null)
            {
                ht.Add("type", "1");
                ht.Add("return_date", returnDate.ToString());
            }
            else { ht.Add("type", "2");}

            if (maxPrice > 0)
            {
                ht.Add("max_price", maxPrice.ToString());
            }
           
        }

       
        public List<FlightInfo> StartSearch()
        {
            List<FlightInfo> flightInfoList = new List<FlightInfo>();
            
            try
            {
                GoogleSearch search = new GoogleSearch(ht, apiKey);
                JObject data = search.GetJson();
                // Nuovo blocco di codice per la deserializzazione del JSON
                if (data["best_flights"] != null && data["best_flights"].HasValues)
                {
                    JArray bestFlights = (JArray)data["best_flights"];

                    foreach (JObject flight_Info in bestFlights)
                    {
                        flightInfoList.Add(ExtractFlightInfo(flight_Info));
                    }
                }
                else if (data["other_flights"] != null && data["other_flights"].HasValues)
                {
                    JArray otherFlights = (JArray)data["other_flights"];

                    foreach (JObject flight_Info in otherFlights)
                    {
                        flightInfoList.Add(ExtractFlightInfo(flight_Info));
                    }
                } 
            }
            catch (SerpApiSearchException ex)
            {
                Console.WriteLine("Exception:");
                Console.WriteLine(ex.ToString());
                return flightInfoList;
            }


            return flightInfoList;

        }


        private FlightInfo ExtractFlightInfo(JObject flight_Info)
        {
            FlightInfo flightInfo = new FlightInfo();

            try {

                JArray flights = (JArray)flight_Info["flights"];
                flightInfo.totalDuration = (int)flight_Info["total_duration"];
                flightInfo.price = (double)flight_Info["price"];
                flightInfo.type = (string)flight_Info["type"];

                Console.WriteLine($"Tipo: {flightInfo.type}");
                Console.WriteLine($"Prezzo: {flightInfo.price} EUR");

                List<Flight> flightList = new List<Flight>();

                foreach (JObject flight in flights)
                {
                    Flight flt = new Flight();

                    flt.departureAirport = (string)flight["departure_airport"]["name"];
                    flt.arrivalAirport = (string)flight["arrival_airport"]["name"];
                    flt.departureAirportIATA = (string)flight["departure_airport"]["id"];
                    flt.arrivalAirportIATA = (string)flight["arrival_airport"]["id"];
                    flt.airline = (string)flight["airline"];
                    flt.airlineLogo = (string)flight["airline_logo"];
                    flt.flightNumber = (string)flight["flight_number"];
                    flt.duration = (int)flight["duration"];

                    flt.departureTime = (string)flight["departure_airport"]["time"];
                    flt.arrivalTime = (string)flight["arrival_airport"]["time"];

                    Console.WriteLine($"Volo da {flt.departureAirport} a {flt.arrivalAirport}");
                    Console.WriteLine($"Compagnia aerea: {flt.airline}");
                    Console.WriteLine($"Numero di volo: {flt.flightNumber}");
                    Console.WriteLine($"Data e orario di partenza: {flt.departureTime}");
                    Console.WriteLine($"Data e orario di arrivo: {flt.arrivalTime}");
                    Console.WriteLine($"Durata del volo: {flt.duration} minuti");
                    Console.WriteLine();

                    // Se c'è un'informazione sugli scali, la estrai e la stampi
                    if (flight["layovers"] != null && flight["layovers"].HasValues)
                    {
                        JArray layovers = (JArray)flight["layovers"];
                        foreach (JObject layover in layovers)
                        {
                            flt.layoverDuration = (int)layover["duration"];
                            flt.layoverAirport = (string)layover["name"];

                            Console.WriteLine($"Scalo a {flt.layoverAirport} con durata di {flt.layoverDuration} minuti");
                        }
                    }

                    flightList.Add(flt);
                }

                Console.WriteLine("-------------");
                flightInfo.flights = flightList;
            } catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                return flightInfo;
            }


                return flightInfo;

           }
        
        
    }
}
