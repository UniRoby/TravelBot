using System;
using System.Collections.Generic;
using System.Text;
using CoreBotCLU.Models;
using System.Linq;

namespace CoreBotCLU.Repository
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

        public bool CityExists(string city)
        {
            city = city.ToLower();
            string result;
            try
            {
                result = (from a in Context.Airports
                          where a.CityIt.ToLower() == city
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