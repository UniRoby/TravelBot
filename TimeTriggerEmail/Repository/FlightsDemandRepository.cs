using System;
using System.Collections.Generic;
using System.Text;
using TimeTriggerEmail.Models;
using System.Linq;
using System.Globalization;
using TimeTriggerEmail.Utils;
using Microsoft.Extensions.Logging;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace TimeTriggerEmail.Repository
{

  public class FlightsDemandRepository
  {


        private string connectionString;

       

        public FlightsDemandRepository()
          {
            connectionString = Environment.GetEnvironmentVariable("sqldb_connection");
        }

 

      public void SwitchPrice()
      {
            try
            {
               
                string query = " UPDATE flights_demands SET currentPrice = newPrice, newPrice = 0 WHERE NewPrice != 0 AND CONVERT(datetime, DepartureDate, 20) > GETDATE();";

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {

                        int rowsAffected = cmd.ExecuteNonQuery();

                        if (rowsAffected == 0)
                        {
                            // Non ci sono voli da aggiornare, esci dal metodo
                            return;
                        }
                       
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Errore nell'aggiornamento del prezzo", ex);
            }

        }


      public List<FlightsDemand> GetFlightsToNotify()
        {
            List<FlightsDemand> flightsEmail = new List<FlightsDemand>();
            try
            {
               
                string query =" SELECT * FROM flights_demands WHERE CurrentPrice <> NewPrice AND NewPrice <> 0 AND Notify = 'Y' AND CONVERT(datetime, DepartureDate, 20) >  GETDATE();";

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                     

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                FlightsDemand flight = new FlightsDemand();
                                flight.DemandId = reader.GetInt32(reader.GetOrdinal("DemandId"));
                                flight.Email = reader.GetString(reader.GetOrdinal("Email"));
                                flight.Origin = reader.GetString(reader.GetOrdinal("Origin"));
                                flight.Destination = reader.GetString(reader.GetOrdinal("Destination"));
                                flight.DepartureDate = reader.GetString(reader.GetOrdinal("DepartureDate"));
                                if (!reader.IsDBNull(reader.GetOrdinal("returnDate")))
                                {
                                    flight.ReturnDate = reader.GetString(reader.GetOrdinal("returnDate"));
                                }
                                flight.CurrentPrice = reader.GetDouble(reader.GetOrdinal("CurrentPrice"));
                                flight.NewPrice = reader.GetDouble(reader.GetOrdinal("NewPrice"));
                                flight.Passengers = reader.GetInt32(reader.GetOrdinal("Passengers"));
                                flight.Notify = reader.GetString(reader.GetOrdinal("Notify"));

                                flightsEmail.Add(flight);
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.StackTrace);
                throw;
            }

            return flightsEmail;
        }



  

      public void UpdateFlightNewPrice(int flightId, double newPrice)
      {
            try
            {
               
                string query = @"
            UPDATE flights_demands
            SET newPrice = @NewPrice
            WHERE demandId = @FlightId;";

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@NewPrice", newPrice);
                        cmd.Parameters.AddWithValue("@FlightId", flightId);

                        int rowsAffected = cmd.ExecuteNonQuery();

                        if (rowsAffected == 0)
                        {
                            throw new InvalidOperationException("Volo non trovato.");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Errore nell'aggiornamento del prezzo", ex);
            }

        }
     
      public List<FlightsDemand> GetFlightsToResearch()
      {
          List<FlightsDemand> flightsEmail= new List<FlightsDemand>();
          DateTime today = DateTime.Today;
            try
            {
               
               
                //  SELECT *FROM flights_demands WHERE notify = 'Y' AND CONVERT(datetime, departureDate, 20) > GETDATE();

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string query = " SELECT * FROM flights_demands WHERE notify = 'Y' AND CONVERT(datetime, departureDate, 20) > GETDATE()";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        //cmd.Parameters.AddWithValue("@Today", today);

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                FlightsDemand flight = new FlightsDemand();
                                flight.DemandId = reader.GetInt32(reader.GetOrdinal("demandId"));
                                flight.Email = reader.GetString(reader.GetOrdinal("email"));
                                flight.Origin = reader.GetString(reader.GetOrdinal("origin"));
                                flight.Destination = reader.GetString(reader.GetOrdinal("destination"));
                                flight.DepartureDate = reader.GetString(reader.GetOrdinal("departureDate"));

                                if (!reader.IsDBNull(reader.GetOrdinal("returnDate")))
                                {
                                    flight.ReturnDate = reader.GetString(reader.GetOrdinal("returnDate"));
                                }
                                   
                                flight.CurrentPrice = reader.GetDouble(reader.GetOrdinal("currentPrice"));
                                flight.NewPrice = reader.GetDouble(reader.GetOrdinal("newPrice"));
                                flight.Passengers = reader.GetInt32(reader.GetOrdinal("passengers"));
                                flight.Notify = reader.GetString(reader.GetOrdinal("notify"));

                                flightsEmail.Add(flight);
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.StackTrace);
                return null;
            }

            return flightsEmail;
        }

       

    }

}
