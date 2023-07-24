using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Emgu.CV.CvEnum;
using MathNet.Numerics;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Complex;
using MathNet.Numerics.LinearAlgebra.Double;
using STH1123.ReedSolomon;
//using ZXing.Common.ReedSolomon;

namespace WPFImageGen
{
    public class Node
    {
        //send in raw data, unpaired ints.
        //Huffman or Reed Solomon first?
        public char Symbol{get; set;}
        public int Freq {get; set;}
        public Node Right {get; set;}
        public Node Left {get; set;}

        public List<bool> Traverse(char Symbol, List<bool> data)
        {
            //Leaf
            if(Right == null && Left == null)
            {
                if (Symbol.Equals(this.Symbol))
                {
                    return data;
                }
                else
                {
                    return null;
                }
            }
            else
            {
                List<bool> left = null;
                List<bool> right = null;

                 if(Left != null)
                {
                    List<bool> leftPath = new List<bool>();
                    leftPath.AddRange(data);
                    leftPath.Add(false);

                    left = Left.Traverse(Symbol, leftPath);
                }
                 if(Right != null)
                {
                    List<bool> rightPath = new List<bool>();
                    rightPath.AddRange(data);
                    rightPath.Add(true);
                    right = Right.Traverse(Symbol, rightPath);
                }
                 if(left != null)
                {
                    return left;
                }
                else
                {
                    return right;
                }
            }
        }

    }

    public class DoReedSolomon
    {

        public int[] Encode(string input)
        {
            int fieldSize = input.Length;// + (input.Length / 2);

            GenericGF field = new GenericGF(285, 256, 0); //primitive, size, genBase
            ReedSolomonEncoder rsE = new ReedSolomonEncoder(field);

            byte[] bytes = Encoding.UTF8.GetBytes(input);

            int[] bytesAsInts = bytes.Select(x => (int)x).ToArray();
            for(int i = 0; i < bytesAsInts.Length / 2; i++)
            {
                bytesAsInts.Append(0);
            }

            rsE.Encode(bytesAsInts, 8);

            return bytesAsInts.ToArray();

        }

        public int[] Decode(byte[] input)
        {
            int[] erasures = new int[] { };
            GenericGF field = new GenericGF(285, 256, 0);

            ReedSolomonDecoder rsD = new ReedSolomonDecoder(field);

            /*
            char[] chars = input.ToCharArray();
            int[] output = new int[chars.Length];// Array.ConvertAll(chars, c => (int)Char.GetNumericValue(c));

            for(int i = 0; i < output.Length; i++)
            {
                //output[i] = (int)chars[i];

                if (chars[i] == '1')
                {
                    output[i] = 1;
                }
                else
                {
                    output[i] = 0;
                }
            }
            */

            int[] output = new int[input.Length];

            for(int i = 0; i < input.Length; i++)
            {
                output[i] = input[i];
            }

            //int[] output = Array.ConvertAll(input.ToArray(), x => (int)x);

            if(rsD.Decode(output, 7, erasures))
            {
                //data corrected.
                return output;
            }
            else
            {
                return null;
            }

        }
    }

    public class HuffmanTree
    {
        private List<Node> nodes = new List<Node>();
        private Node Root {get; set;}
        public Dictionary<char, int> Freq = new Dictionary<char, int>();

        public void Build(string input)
        {
            for (int i = 0; i < input.Length; i++)
            {
                if (!Freq.ContainsKey(input[i]))
                {
                    Freq.Add(input[i], 0);
                }
                Freq[input[i]]++;
            }

            foreach(KeyValuePair<char, int> symbol in Freq)
            {
                nodes.Add(new Node() { Symbol = symbol.Key, Freq = symbol.Value });
            }

            while (nodes.Count > 1)
            {
                List<Node> orderedNodes = nodes.OrderBy(node => node.Freq).ToList<Node>();

                if(orderedNodes.Count >= 2)
                {
                    List<Node> taken = orderedNodes.Take(2).ToList<Node>();
                    Node parent = new Node() {
                        Symbol = '*',
                        Freq = taken[0].Freq + taken[1].Freq,
                        Left = taken[0],
                        Right = taken[1]
                    };

                    nodes.Remove(taken[0]);
                    nodes.Remove(taken[1]);
                    nodes.Add(parent);
                }

                this.Root = nodes.FirstOrDefault();
            }
        }

        public BitArray Encode(string input)
        {
            List<bool> encodedInput = new List<bool>();

            for(int i = 0; i < input.Length; i++)
            {
                List<bool> encodedSymbol = this.Root.Traverse(input[i], new List<bool>()); //issue when only one.
                encodedInput.AddRange(encodedSymbol);
            }

            BitArray bits = new BitArray(encodedInput.ToArray());

            return bits;
        }

        public BitArray BinaryToBits(string input)
        {
            char[] binaryChars = input.ToCharArray();

            BitArray bitArray = new BitArray(binaryChars.Length);

            for (int i = 0; i < binaryChars.Length; i++)
            {
                if (binaryChars[i] == '1')
                {
                    bitArray[i] = true;
                }
                else if (binaryChars[i] == '0')
                {
                    bitArray[i] = false;
                }
                else
                {
                    //Handle Error
                }
            }

            return bitArray;
        }

        public string Decode(string toDecode)
        {
            Node current = this.Root;
            string decoded = "";

            BitArray bits = BinaryToBits(toDecode);

            foreach (bool bit in bits)
            {
                if (bit)
                {
                    if (current.Right != null)
                    {
                        current = current.Right;
                    }
                }
                else
                {
                    if (current.Left != null)
                    {
                        current = current.Left;
                    }
                }

                if (IsLeaf(current))
                {
                    decoded += current.Symbol;
                    current = this.Root;
                }
            }

            return decoded;
        }

        public bool IsLeaf(Node node)
        {
            return (node.Left == null && node.Right == null);
        }
    }
}
