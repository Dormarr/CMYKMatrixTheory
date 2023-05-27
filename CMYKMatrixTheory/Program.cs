using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZXing;
using ZXing.Common.ReedSolomon;
using ZXing.QrCode.Internal;

namespace CMYKMatrixTheory
{
    class Program
    {



        static void Main(string[] args)
        {
            //NativeWindow window = new NativeWindow();

            string str = "Ryan Appleyard, this is a test of Reed Solomon, an attempt to restore corrupted data for using in the QUAY.";
            /*
            string asciiString = PlaintoASCII(str);
            Console.WriteLine();
            PlaintoBinary(str);
            Console.WriteLine();
            ASCIItoBinary(asciiString);
            

            string inputString = str;
            int errorCorrectionBytes = 1;

            byte[] encodedBytes = ReedSolomonEncoder.Encode(inputString, errorCorrectionBytes);
            string encodedString = Convert.ToBase64String(encodedBytes);

            Console.WriteLine("Encoded string: " + encodedString);
            */
            ReedSolomonEncoder rsEncoder = new ReedSolomonEncoder(GenericGF.QR_CODE_FIELD_256);
            byte[] dataBytes = Encoding.ASCII.GetBytes(str);
            int[] dataInts = Array.ConvertAll(dataBytes, b => (int)b);
            int errorCorrectionBytes = 7;

            rsEncoder.encode(dataInts, errorCorrectionBytes);

            byte[] encodedBytes = Array.ConvertAll(dataInts, i => (byte)i);
            string encodedData = Encoding.ASCII.GetString(encodedBytes);

            Console.WriteLine(encodedData);

            ReedSolomonDecoder rsDecoder = new ReedSolomonDecoder(GenericGF.QR_CODE_FIELD_256);
            int[] recievedData = Array.ConvertAll(encodedBytes, i => (int)i);



            bool success = rsDecoder.decode(recievedData, errorCorrectionBytes);

            byte[] decodedData = Array.ConvertAll(recievedData, b => (byte)b);
            string decodedText = Encoding.ASCII.GetString(decodedData);

            Console.WriteLine();
            

            Console.ReadKey();
        }

        static string PlaintoASCII(string str)
        {
            List<int> combo = new List<int>();
            foreach(char c in str)
            {
                int uni = c;
                //Console.Write("{0} ", uni);
                combo.Add(uni);
            }
            Console.WriteLine(string.Join(" ", combo));
            return combo.ToString();
        }

        static void PlaintoBinary(string str)
        {
            StringBuilder binaryBuilder = new StringBuilder();

            foreach(char c in str)
            {
                string binary = Convert.ToString(c, 2).PadLeft(8, '0');
                binaryBuilder.Append(binary);
            }
            Console.WriteLine(binaryBuilder.ToString());
            
        }

        static void ASCIItoBinary(string str)
        {
            StringBuilder binaryBuilder = new StringBuilder();

            foreach (char c in str)
            {
                string binary = Convert.ToString(c, 2).PadLeft(8, '0');
                binaryBuilder.Append(binary);
            }
            Console.WriteLine(binaryBuilder.ToString());

            //ConvertBinaryToMatrix(binaryBuilder.ToString(), 3, 3);
        }


        static int[,] ConvertBinaryToMatrix(string binaryInput, int rows, int columns)
        {
            int[,] matrix = new int[rows, columns];

            int index = 0;
            for(int i = 0; i < rows; i++)
            {
                for (int j = 0; j < columns; j++)
                {
                    if (index >= binaryInput.Length)
                    {
                        matrix[i, j] = 0;
                    }
                    else
                    {
                        matrix[i, j] = binaryInput[index] == '0' ? 0 : 1;
                    }

                    index++;
                }
            }
            return matrix;
        }
    }
    /*
    public class ReedSolomonEncoder
    {
        private byte[] GaloisLogTable;
        private int GaloisPrimitiveRoot = FindPrimitiveRoot(256);
        private const int GaloisFieldSize = 256;
        private const int GeneratorPolynomial = 0x1D; //not anymore //x^8 + x^4 + x^3 + x^2 + 1 VOYAGER CODE

        public static byte[] Encode(string inputString, int errorCorrectionBytes)
        {
            byte[] inputBytes = Encoding.ASCII.GetBytes(inputString);
            //Do I want ASCII though? I'll reassess once it's working.

            //Calc no. data bytes
            int dataBytes = inputBytes.Length;

            //Calculate no. total bytes (data + ec)
            int totalBytes = dataBytes + errorCorrectionBytes;

            byte[] message = new byte[totalBytes];

            //Copy bytes to message
            Array.Copy(inputBytes, message, dataBytes);

            //Perform Reed-Solomon
            for(int i = 0; i < dataBytes; i++)
            {
                byte factor = message[i];
                if(factor != 0)
                {
                    for( int j = 1; j < errorCorrectionBytes; j++)
                    {
                        message[i + dataBytes + j] ^= GaloisMultiply(factor, GetGeneratorPolynomial(j, errorCorrectionBytes), GaloisFieldSize);
                    }
                }
            }

            return message;

        }

        private byte GaloisMultiply(byte a, byte b, int fieldSize)
        {
            byte p = 0;
            for (int i = 0; i < 8; i++)
            {
                if ((b & 1) != 0)
                {
                    p ^= a;
                }
                bool carry = (a & 0x80) != 0;
                a <<= 1;
                if (carry)
                {
                    a ^= GeneratorPolynomial;
                }
                b >>= 1;

                if (a == 0 || b == 0)
                    return 0;
                GaloisLogTable = GenerateGaloisLogTable();
                int logA = GaloisLogTable[a];
                int logB = GaloisLogTable[b];
                int logResult = (logA + logB) % (fieldSize - 1);
                return GenerateGaloisExponentialTable[logResult];
            }
            return p;
        }

        private static byte GetGeneratorPolynomial(int index, int errorCorrectionBytes)
        {
            int generatorPolynomial = GeneratorPolynomial;
            for(int i=0; i < errorCorrectionBytes; i++)
            {
                generatorPolynomial = (generatorPolynomial << 1) ^ (GeneratorPolynomial >> 7);
            }
            return (byte)generatorPolynomial;
        }
    /*
        //DECODE ---------------------------------------------------------------

        private static byte GaloisPower(byte value, int power, int fieldSize)
        {
            if (power == 0)
                return 1;

            byte result = value;
            for (int i = 1; i < power; i++)
            {
                result = GaloisMultiply(result, value, fieldSize);
            }

            return result;
        }

        

        public static int FindPrimitiveRoot(int fieldSize)
        {
            for (int root = 2; root < fieldSize; root++)
            {
                bool isPrimitiveRoot = true;
                HashSet<int> generatedValues = new HashSet<int>();

                // Test all possible powers of the root
                for (int exponent = 1; exponent < fieldSize - 1; exponent++)
                {
                    int value = GaloisPower((byte)root, exponent, fieldSize);

                    // Check if the value is already generated
                    if (generatedValues.Contains(value))
                    {
                        isPrimitiveRoot = false;
                        break;
                    }

                    generatedValues.Add(value);
                }

                if (isPrimitiveRoot)
                {
                    return root;
                }
            }

            throw new Exception("Primitive root not found for the specified field size.");
        }

        private byte[] GenerateGaloisLogTable()
        {
            int fieldSize = GaloisFieldSize;
            byte[] logTable = new byte[fieldSize];

            byte value = 1;
            for (int power = 0; power < fieldSize - 1; power++)
            {
                logTable[value] = (byte)power;
                value = GaloisMultiply(value, 2, fieldSize);
            }

            return logTable;
        }

        private byte[] GenerateGaloisExponentialTable()
        {
            int fieldSize = GaloisFieldSize;
            int primitiveRoot = GaloisPrimitiveRoot;
            byte[] exponentialTable = new byte[fieldSize];

            byte value = 1;
            for (int i = 0; i < fieldSize - 1; i++)
            {
                exponentialTable[i] = value;
                value = GaloisMultiply(value, (byte)primitiveRoot, fieldSize);
            }

            return exponentialTable;
        }

        private byte GaloisDivide(byte dividend, byte divisor)
        {
            if (divisor == 0)
            {
                throw new DivideByZeroException("Divisor cannot be zero.");
            }

            if (dividend == 0)
            {
                return 0;
            }

            int dividendLog = GaloisLogTable[dividend];
            int divisorLog = GaloisLogTable[divisor];

            int resultLog = (dividendLog - divisorLog + GaloisFieldSize) % GaloisFieldSize;

            return GaloisExponentialTable[resultLog];
        }


        private static List<int> FindErrorLocations(List<byte> receivedCodeword, int errorCorrectionBytes)
        {
            int codewordLength = receivedCodeword.Count;

            // Initialize the syndrome polynomial with the last `errorCorrectionBytes` coefficients of the received codeword
            List<byte> syndromePolynomial = new List<byte>(receivedCodeword.GetRange(codewordLength - errorCorrectionBytes, errorCorrectionBytes));

            // Initialize the error locations list
            List<int> errorLocations = new List<int>();

            // Apply the Berlekamp-Massey algorithm
            List<byte> discrepancy = new List<byte>(syndromePolynomial);
            List<byte> oldDiscrepancy = new List<byte>(syndromePolynomial);

            List<byte> lambda = new List<byte>(new byte[] { 1 });
            List<byte> oldLambda = new List<byte>(new byte[] { 1 });

            for (int i = 1; i <= errorCorrectionBytes; i++)
            {
                byte error = 0;

                // Calculate the discrepancy
                for (int j = 0; j < lambda.Count; j++)
                {
                    error ^= GaloisMultiply(lambda[j], receivedCodeword[codewordLength - errorCorrectionBytes - 1 - j]);
                }

                discrepancy.Insert(0, error);

                if (error != 0)
                {
                    if (oldDiscrepancy.Count > discrepancy.Count)
                    {
                        List<byte> temp = new List<byte>(oldDiscrepancy);
                        oldDiscrepancy = new List<byte>(discrepancy);
                        discrepancy = new List<byte>(temp);
                    }

                    byte factor = GaloisDivide(discrepancy[0], oldDiscrepancy[0]);
                    List<byte> tempLambda = new List<byte>(lambda);

                    // Update lambda
                    for (int j = 0; j < discrepancy.Count; j++)
                    {
                        int lambdaIndex = j - i;
                        if (lambdaIndex >= 0 && lambdaIndex < oldLambda.Count)
                        {
                            lambda[j] ^= GaloisMultiply(factor, oldLambda[lambdaIndex]);
                        }
                    }

                    oldDiscrepancy = new List<byte>(discrepancy);
                    oldLambda = new List<byte>(tempLambda);
                }

                lambda.Add(0);

                if (i < errorCorrectionBytes)
                {
                    syndromePolynomial.Add(0);
                    lambda.Add(0);
                }
            }

            // Find the roots of the error locator polynomial
            for (int i = 0; i < codewordLength; i++)
            {
                if (EvaluatePolynomial(lambda, GaloisPower(2, i)) == 0)
                {
                    errorLocations.Add(codewordLength - 1 - i);
                }
            }

            return errorLocations;
        }

        private static byte[] ReedSolomonDecode(byte[] receivedMessage, int errorCorrectionBytes)
        {
            int dataBytes = receivedMessage.Length - errorCorrectionBytes;
            int[] syndromes = new int[errorCorrectionBytes];

            // Calculate syndromes
            for (int i = 0; i < errorCorrectionBytes; i++)
            {
                int syndrome = 0;
                for (int j = 0; j < receivedMessage.Length; j++)
                {
                    int power = GaloisPower((byte)j, i, GaloisFieldSize);
                    syndrome ^= GaloisMultiply(receivedMessage[j], (byte)power);
                }
                syndromes[i] = syndrome;
            }

            // Find error locations
            int[] errorLocations = FindErrorLocations(syndromes, errorCorrectionBytes);

            // Calculate error locator polynomial
            int[] errorLocatorPolynomial = CalculateErrorLocatorPolynomial(errorLocations);

            // Calculate error evaluator polynomial
            int[] errorEvaluatorPolynomial = CalculateErrorEvaluatorPolynomial(syndromes, errorLocatorPolynomial, errorCorrectionBytes);

            // Correct errors
            for (int i = 0; i < errorLocations.Length; i++)
            {
                int errorLocation = errorLocations[i];
                int errorValue = CalculateErrorValue(errorLocation, errorLocatorPolynomial, errorEvaluatorPolynomial);
                receivedMessage[errorLocation] ^= (byte)errorValue;
            }

            // Extract the original message (without error correction bytes)
            byte[] originalMessage = new byte[dataBytes];
            Array.Copy(receivedMessage, 0, originalMessage, 0, dataBytes);

            return originalMessage;
        }

    }
    */
}
