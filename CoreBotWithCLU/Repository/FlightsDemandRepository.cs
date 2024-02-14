using System;
using System.Collections.Generic;
using System.Text;
using CoreBotCLU.Models;
using System.Linq;
using System.Globalization;

namespace CoreBotCLU.Repository
{
    

    public class FlightsDemandRepository 
    {
        
        
        TravelbotDbContext context;

        public TravelbotDbContext Context { get { return context; } }

        public FlightsDemandRepository()
        {
           context = new TravelbotDbContext();
        }

        public bool EmailExists(string email)
        {

            string flightsEmail;
            DateTime today = DateTime.Today;
            try
            {
                flightsEmail = (from f in Context.FlightsDemands
                                where f.Email == email
                                select f.Email
                                ).FirstOrDefault();

            }
            catch (Exception)
            {
                return false;
            }
            if (flightsEmail !=null)
            {
                return true;
            }

            return false;
        }

        public void RemoveEmail(string email)
        {
            try
            {
                var flightsToUpdate = Context.FlightsDemands
                    .Where(f => f.Email == email)
                    .ToList();

                if (flightsToUpdate.Any())
                {
                    foreach (var flight in flightsToUpdate)
                    {
                        flight.Email =" ";
                        flight.Notify = "N";
                       
                    }

                    Context.SaveChanges();
                }
                else
                {
                    // Non ci sono voli da aggiornare, esci dal metodo
                    return;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Errore rimozione email", ex);
            }
        }


        public void SwitchPrice()
        {
            DateTime today = DateTime.Today;
            try
            {
                var flightsToUpdate = Context.FlightsDemands
                    .Where(f => f.NewPrice != 0 )
                    .ToList().Where(f => DateTime.ParseExact(f.DepartureDate, "yyyy-MM-dd", CultureInfo.InvariantCulture) > today).ToList();

                if (flightsToUpdate.Any())
                {
                    foreach (var flight in flightsToUpdate)
                    {
                        flight.CurrentPrice = flight.NewPrice;
                        flight.NewPrice = 0;
                    }

                    Context.SaveChanges();
                }
                else
                {
                    // Non ci sono voli da aggiornare, esci dal metodo
                    return;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Errore nell'aggiornamento del prezzo", ex);
            }
        }


        public List<FlightsDemand> GetFlightsToNotify()
        {
            List<FlightsDemand> flightsEmail;
             DateTime today = DateTime.Today;
            try
            {
                flightsEmail = (from f in Context.FlightsDemands
                                where f.CurrentPrice != f.NewPrice && f.NewPrice != 0 && f.Notify == "Y" 
                                select f
                                ).ToList().Where(f => DateTime.ParseExact(f.DepartureDate, "yyyy-MM-dd", CultureInfo.InvariantCulture) > today).ToList();

            }
            catch (Exception)
            {
                throw;
            }

            return flightsEmail;
        }

        

        public void SaveFlightDemand(FlightsDemand flight)
        {
            try
            {
                Context.FlightsDemands.Add(flight);
                Context.SaveChanges();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void UpdateFlightNewPrice(int flightId, double newPrice)
        {
            try
            {
                var flightToUpdate = Context.FlightsDemands.FirstOrDefault(f => f.DemandId == flightId);

                if (flightToUpdate != null)
                {
                    flightToUpdate.NewPrice = newPrice;
                    Context.SaveChanges();
                }
                else
                {
                    throw new InvalidOperationException("Volo non trovato.");
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Errorore nell'aggiornameto del prezzo", ex);
            }
        }

        public List<FlightsDemand> GetFlightsToResearch()
        {
            List<FlightsDemand> flightsEmail;
            DateTime today = DateTime.Today;
            try
            {
                flightsEmail = (from f in Context.FlightsDemands
                                where f.Notify == "Y" 
                                select f
                                ).ToList().Where(f => DateTime.ParseExact(f.DepartureDate, "yyyy-MM-dd", CultureInfo.InvariantCulture) > today).ToList(); ;

            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return null;
            }

            return flightsEmail;
        }

    }

}