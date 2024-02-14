using System.Collections.Generic;
using System;
using System.Text.RegularExpressions;

namespace TravelBot.Utils
{
    public class WordNumberConverter
    {
        
            private  Dictionary<string, int> numberTable = new Dictionary<string,int>{
        {"zero",0},{"uno",1},{"una",1},{"solo",1},{"due",2},{"coppia",2},{"tre",3},{"quattro",4},{"cinque",5},{"sei",6},
        {"sette",7},{"otto",8},{"nove",9},{"dieci",10}
        };

        public WordNumberConverter() { }

            public  string ConvertToNumbers(string numberString)
            {
                // Cerca la chiave nel dizionario
                foreach (var coppia in numberTable)
                {
                    if (numberString.Contains(coppia.Key))
                    {
                        return coppia.Value.ToString();
                    }
                }
            return numberString;
            }

        public string NumberExtractor(string input)
        {
            
            string pattern = @"\d+";

        
            Match match = Regex.Match(input, pattern);

           
            if (match.Success)
            {
                return match.Value;
            }
            else
            {
             
                return "0";
                
            }
        }

    }

}
