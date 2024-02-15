using System;
using System.Collections.Generic;
using System.Text;
using TravelBot.Models;
using System.Linq;

namespace TravelBot.Repository
{


    public class AirportRepository
    {


        TravelbotDbContext context;

        public TravelbotDbContext Context { get { return context; } }

        public AirportRepository()
        {
            context = new TravelbotDbContext();
        }


        public List<string> GetIataCodeByCity(string city)
        {
            List<string> iataCities;
            try
            {
                iataCities = (from a in Context.Airports
                              where a.CityIt.ToLower() == city && (a.IataCode != null)
                              select a.IataCode
                                ).ToList();

            }
            catch (Exception)
            {
                throw;
            }
            return iataCities;
        }

        public string ExtractCity(string city)
        {
            string cityExtracted;
            city = city.ToLower();
            try
            {
                cityExtracted = (from a in Context.Airports
                              where city.Contains(a.CityIt.ToLower())
                                 select a.CityIt
                                ).FirstOrDefault();

            }
            catch (Exception)
            {
                throw;
            }
            return cityExtracted;
        }

        public string GetIataContainedInValue(string value)
        {
            var iata="";
            value = value.ToLower();
            try
            {
                iata = (from a in Context.Airports
                              where value.Contains(a.CityIt.ToLower()) || value.Contains(a.IataCode.ToLower())
                                select a.IataCode
                                ).FirstOrDefault();

            }
            catch (Exception)
            {
                throw;
            }
            return iata;
        }

        public bool CityExists(string city)
        {
            city = city.ToLower();
            string result;
            try
            {
                result = (from a in Context.Airports
                          where a.CityIt.ToLower() == city || city.Contains(a.IataCode.ToLower())
                          select a.CityIt
                                ).FirstOrDefault();

            }
            catch (Exception)
            {
                throw;
            }

            if (result == null)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

    }

}