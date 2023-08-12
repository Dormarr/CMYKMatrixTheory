using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuayCodeV2
{
    class CustomBinary
    {
        public static string Convert8Bit(char[] input)
        {
            //Correlate with ASCII.

            Dictionary<char, string> BitKey = new Dictionary<char, string> {
                { ' ', "00100000" },
                { '!', "00100001" },
                { '"', "00100010" },
                { '#', "00100011" },
                { '$', "00100100" },
                { '%', "00100101" },
                { '&', "00100110" },
                { '`', "00100111" },
            };

            StringBuilder stringBuilder = new StringBuilder();

            foreach(char c in input)
            {
                if(BitKey.ContainsKey(c))
                {
                    stringBuilder.Append(c);
                }   
            }
            return stringBuilder.ToString();
        }

        public static string Convert4Bit(char[] input)
        {
            Dictionary<int, string> BitKey = new Dictionary<int, string> {
                { '0', "0100" },
                { '1', "0101" },
                { '2', "0110" },
                { '3', "0111" },
                { '4', "1000" },
                { '5', "1001" },
                { '6', "1010" },
                { '7', "1011" },
                { '8', "1100" },
                { '9', "1101" }
            };

            StringBuilder stringBuilder = new StringBuilder();

            foreach (char c in input)
            {
                if (BitKey.ContainsKey(c))
                {
                    stringBuilder.Append(c);
                }
            }
            return stringBuilder.ToString();
        }


    }
}
