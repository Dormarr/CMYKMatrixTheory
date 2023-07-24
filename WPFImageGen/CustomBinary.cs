using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WPFImageGen
{
    class CustomBinary
    {
        public static string ConvertToFour(char[] input)
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

            StringBuilder str = new StringBuilder();

            foreach(char c in input)
            {
                if (BitKey.ContainsKey(c))
                {
                    str.Append(BitKey[c]);
                }
            }

            string output = str.ToString();

            return output;
        }
    }
}
