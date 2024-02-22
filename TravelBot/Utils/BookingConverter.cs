using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;
using TravelBot;

namespace TravelBot.Utils
{
    public class BookingConverter
    {
        public string Date { get; set; }
        public string PromptType { get; set; }

        
        public BookingConverter()
        {

        }

        public BookingDetails ParseFromString(string allValues)
        {
            // Divide la stringa in base agli spazi
            string[] values = allValues.Split(',');

            // Estrai i valori tra parentesi e inseriscili in una lista
           
            foreach (var value in values)
            {
                Console.WriteLine(value.ToString());
            }

            // Crea un nuovo oggetto BookingDetails e assegna i valori appropriati
            BookingDetails bookingDetails = new BookingDetails
            {
                Origin = GetValueFromBraces(values[0]),
                Destination = GetValueFromBraces(values[1]),
                TravelDate = GetValueFromBraces(values[2]),
                ReturnDate = GetValueFromBraces(values[3]),
                PassengersNumber = GetValueFromBraces(values[4]),
                Budget = GetValueFromBraces(values[5])
            };

            Console.WriteLine("\n------------------------------------------BOOKING CONVERTER");
            Console.WriteLine(bookingDetails.ReturnDate == null);
            return bookingDetails;
        }

  
        private string GetValueFromBraces(string value)
        {
            // Rimuove le parentesi graffe e restituisce il valore interno o null
            string trimmedValue = value.Trim('{', '}');
            return string.IsNullOrEmpty(trimmedValue) ? null : trimmedValue;
        }


        public bool ContainsIATA(string city, List<string> listIATA)
        {
            if (listIATA.Count >= 1)
            {
                Console.WriteLine("IF CONTAINS METHOD");
                foreach (string iata in listIATA)
                {
                    if (city.Contains(iata))
                    {
                        return true;
                    }
                }
            }
             return false;
        }

        public int GetContainedIATA(string city, List<string> listIATA)
        {
            var index = 10000;
               for(int i = 0; i < listIATA.Count; i++)
                {
                    if (city.Contains(listIATA[i]))
                    {
                        index= i;
                        return index;
                    }
                }
            return index;
        }

        public string RimuoviPreposizioni(string input)
        {
           if(input == null)
                return null;
            // Lista delle preposizioni da rimuovere
            string[] preposizioni = { "per", "da", "a" };

            Console.WriteLine(input); 
            // Dividi la stringa in parole
            string[] parole = input.Split(' ');

            Console.WriteLine(parole);

            // Rimuovi le preposizioni e riunisci le parole
            string risultato = string.Join(" ", parole.Where(parola => !preposizioni.Contains(parola)));

            Console.WriteLine(risultato);
            return risultato;
        }

    }
}


