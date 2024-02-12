using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TimeTriggerEmail.Repository;
using TimeTriggerEmail.Models;

using Microsoft.Extensions.Logging;

namespace TimeTriggerEmail.Utils
{
    public class UpdatePriceService
    {
        private FlightsDemandRepository flightsDemandRepository= new FlightsDemandRepository();

        

        public UpdatePriceService() { }

       
        public void CheckAndUpdateBestPrice(ILogger _logger)
        {
            List<FlightsDemand> flightsDemands = flightsDemandRepository.GetFlightsToResearch();

            _logger.LogInformation($"First FlightsDemand Email: {flightsDemands[0].Email}");

            foreach (var flightDemand in flightsDemands)
            {
                //chiama Api e prenditi il prezzo migliore
                //SearchFlights searchFlights = new SearchFlights(flightDemand.Origin, flightDemand.Destination, flightDemand.DepartureDate, flightDemand.ReturnDate, flightDemand.Passengers,0);

                List<FlightInfo> flightInfoList = GetRandomFlights();     //searchFlights.StartSearch();
                _logger.LogInformation($"First FlightsINFO price: {flightInfoList[0].price}");
                List<double> prices = new List<double>();
                foreach (var flightInfo in  flightInfoList)
                {
                    prices.Add(flightInfo.price);
                }
                double bestPrice=GetBestPrice(prices);
                _logger.LogInformation($"best price: {bestPrice}");
                //aggorna il prezzo a DB
                flightsDemandRepository.UpdateFlightNewPrice(flightDemand.DemandId,bestPrice);
            }
          
        }
       

        public double GetBestPrice(List<double> prices)
        {
            // Verifica se la lista dei prezzi è vuota
            if (prices == null || prices.Count == 0)
            {
                throw new ArgumentException("La lista dei prezzi è vuota o nulla.");
            }

            // Trova il prezzo più basso usando il metodo Min() di LINQ
            double bestPrice = prices.Min();

            return bestPrice;
        }





        private List<FlightInfo> GetRandomFlights()
        {
            List<FlightInfo> list = new List<FlightInfo>();

            // Creazione degli oggetti Flight
            Flight flight1 = new Flight
            {
                departureAirport = "Airport1",
                arrivalAirport = "Airport2",
                departureAirportIATA = "IATA1",
                arrivalAirportIATA = "IATA2",
                airline = "Airline1",
                airlineLogo = "https://www.gstatic.com/flights/airline_logos/70px/NH.png",
                flightNumber = "FL1",
                duration = 120,
                departureTime = "2023-10-03 17:10",
                arrivalTime = "2023-10-03 21:00",
                layoverDuration = 0,
                layoverAirport = ""
            };

            Flight flight2 = new Flight
            {
                departureAirport = "Airport2",
                arrivalAirport = "Airport3",
                departureAirportIATA = "IATA12",
                arrivalAirportIATA = "IATA13",
                airline = "Airline2",
                airlineLogo = "https://www.gstatic.com/flights/airline_logos/70px/UA.png",
                flightNumber = "FL2",
                duration = 180,
                departureTime = "2023-10-03 15:10",
                arrivalTime = "2023-10-03 17:00",
                layoverDuration = 0,
                layoverAirport = ""
            };

            // Creazione degli oggetti FlightInfo
            FlightInfo flightInfo1 = new FlightInfo
            {
                totalDuration = flight1.duration + flight2.duration,
                price = 300.50,
                type = "Business",
                flights = new List<Flight> { flight1, flight2 }
            };

            FlightInfo flightInfo2 = new FlightInfo
            {
                totalDuration = flight1.duration,
                price = 100.75,
                type = "Economy",
                flights = new List<Flight> { flight1 }
            };

            FlightInfo flightInfo3 = new FlightInfo
            {
                totalDuration = flight2.duration,
                price = 700.25,
                type = "Economy",
                flights = new List<Flight> { flight2 }
            };

            list.Add(flightInfo1);
            list.Add(flightInfo2);
            list.Add(flightInfo3);


            return list;
        }
       

    }


}
