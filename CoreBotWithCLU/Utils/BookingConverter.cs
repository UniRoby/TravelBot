using System;
using Microsoft.BotBuilderSamples;

namespace CoreBotCLU.Utils
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
            string[] values = allValues.Split(' ');

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

            return bookingDetails;
        }

        private string GetValueFromBraces(string value)
        {
            // Rimuove le parentesi graffe e restituisce il valore interno o null
            string trimmedValue = value.Trim('{', '}');
            return string.IsNullOrEmpty(trimmedValue) ? null : trimmedValue;
        }
    }
}


