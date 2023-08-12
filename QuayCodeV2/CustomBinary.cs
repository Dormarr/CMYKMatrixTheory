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
            Dictionary<char, string> BitKey = new Dictionary<char, string> {
                { '0', "0110" },
                { '1', "0101" },
                { '2', "1010" },
                { '3', "1011" },
                { '4', "1000" },
                { '5', "1001" },
                { '6', "0111" },
                { '7', "0100" },
                { '8', "1100" },
                { '9', "1101" }
            };

            StringBuilder stringBuilder = new StringBuilder();

            foreach (char c in input)
            {
                if (BitKey.ContainsKey(c))
                {
                    stringBuilder.Append(BitKey[c]);
                }
            }
            return stringBuilder.ToString();
        }


    }
}
