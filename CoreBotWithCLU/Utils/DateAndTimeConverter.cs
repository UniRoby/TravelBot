using System;
using System.Globalization;

namespace CoreBotCLU.Utils
{


    public class DateAndTimeConverter
    {
        public DateAndTimeConverter()
        {
        }

        public string WordToDate(string date)
        {
            DateTime newDate;
            string risultato = null;
            try
            {
                if (DateTime.TryParseExact(date, "d MMMM yyyy", CultureInfo.GetCultureInfo("it-IT"), DateTimeStyles.None, out newDate))
                {
                    risultato = newDate.ToString("dd/MM/yyyy");
                    return risultato;
                }
                else if (DateTime.TryParseExact(date, "d MMMM", CultureInfo.GetCultureInfo("it-IT"), DateTimeStyles.None, out newDate))
                {
                    // Caso 1: Aggiunta dell'anno corrente
                    risultato = newDate.ToString("dd/MM/yyyy");
                    return risultato;
                }
                else if (DateTime.TryParseExact(date, "d/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out newDate))
                {
                    risultato = newDate.ToString("dd/MM/yyyy");
                    return risultato;
                }
                else if (DateTime.TryParseExact(date, "d-MM-yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out newDate))
                {
                    risultato = newDate.ToString("dd/MM/yyyy");
                    return risultato;
                }
                else if (DateTime.TryParseExact(date, "dd-MM-yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out newDate))
                {
                    risultato = newDate.ToString("dd/MM/yyyy");
                    return risultato;
                }
                else if (DateTime.TryParseExact(date, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out newDate))
                {
                    risultato = newDate.ToString("dd/MM/yyyy");
                    return risultato;
                }
                else
                {
                    Console.WriteLine("Formato data non valido.");
                    return risultato;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                return risultato;
            }
           
           

        }

        public string ItalianToEnUs(string dataItaliana)
        {
            // Convertire la data italiana in DateTime
            DateTime data = DateTime.ParseExact(dataItaliana, "dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture);

            // Convertire la data in formato en-US (yyyy-mm-dd)
            string dataEnUs = data.ToString("yyyy-MM-dd");
            //DateTime data = DateTime.ParseExact(dataEnUs, "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture);

            return dataEnUs;
        }

        public string EnUsWithTimeToItalian(string dataEnUs)
        {
            DateTime data = DateTime.ParseExact(dataEnUs, "yyyy-MM-dd HH:mm", System.Globalization.CultureInfo.InvariantCulture);

            // Formatta la data nel formato desiderato "dd-MM-yyyy hh:mm"
            string dataIta = data.ToString("dd-MM-yyyy HH:mm");

            return dataIta;
        }


        public string ConvertiMinutiInOre(int minuti)
        {
            // Calcola le ore dividendo i minuti per 60
            int ore = minuti / 60;

            // Calcola i minuti rimanenti dopo aver convertito in ore
            int minutiRimanenti = minuti % 60;


            // Costruisci la stringa risultato nel formato "ore:minuti"
            string oreStringa = ore != 0 ? (ore + "h ") : "";
            string minutiStringa = minutiRimanenti != 0 ? (minutiRimanenti.ToString("D2") + "m") : "";
            string risultato = oreStringa + minutiStringa;

            return risultato;
        }
    }
    
}
