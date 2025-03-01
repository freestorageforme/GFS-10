using System.Text;

namespace HuffmanCodingExample
{
    // Node class for Huffman Tree
    public class HuffmanNode
    {
        public char? Symbol { get; set; } // Nullable char for non-leaf nodes
        public int Frequency { get; set; }
        public HuffmanNode Left { get; set; }
        public HuffmanNode Right { get; set; }

        // A node is a leaf if it has no children.
        public bool IsLeaf => Left == null && Right == null;

        public HuffmanNode(char? symbol, int frequency)
        {
            Symbol = symbol;
            Frequency = frequency;
        }
    }

    public class HuffmanCoding
    {
        private HuffmanNode _root;
        private Dictionary<char, string> _codes = new Dictionary<char, string>();

        // Build the Huffman Tree for the input text.
        public void Build(string text)
        {
            // Build a frequency table for each character.
            var frequency = new Dictionary<char, int>();
            foreach (char c in text)
            {
                if (!frequency.ContainsKey(c))
                    frequency[c] = 0;
                frequency[c]++;
            }

            PrintFrequencyTable(frequency);

            // Create a list of Huffman nodes.
            var nodes = new List<HuffmanNode>();
            foreach (var kvp in frequency)
            {
                nodes.Add(new HuffmanNode(kvp.Key, kvp.Value));
            }

            // Combine nodes until only one tree remains.
            while (nodes.Count > 1)
            {
                // Sort the nodes by frequency.
                nodes = nodes.OrderBy(n => n.Frequency).ToList();
                HuffmanNode left = nodes[0];
                HuffmanNode right = nodes[1];

                // Create a new parent node with the two smallest frequencies.
                HuffmanNode parent = new HuffmanNode(null, left.Frequency + right.Frequency)
                {
                    Left = left,
                    Right = right
                };

                // Remove the two nodes and add their parent.
                nodes.Remove(left);
                nodes.Remove(right);
                nodes.Add(parent);
            }

            _root = nodes[0];

            // Generate the codes by traversing the Huffman tree.
            GenerateCodes(_root, "");
        }

        // Recursively build the code table.
        private void GenerateCodes(HuffmanNode node, string code)
        {
            if (node == null)
                return;

            // When we reach a leaf node, store the code for that symbol.
            if (node.IsLeaf)
            {
                _codes[node.Symbol.Value] = code;
            }
            else
            {
                GenerateCodes(node.Left, code + "0");
                GenerateCodes(node.Right, code + "1");
            }
        }

        // Encode the input text into a Huffman encoded string.
        public string Encode(string text)
        {
            string encoded = "";
            foreach (char c in text)
            {
                encoded += _codes[c];
            }
            return encoded;
        }

        // Decode the Huffman encoded string back to the original text.
        public string Decode(string encodedText)
        {
            string decoded = "";
            HuffmanNode current = _root;
            foreach (char bit in encodedText)
            {
                current = (bit == '0') ? current.Left : current.Right;

                // When we hit a leaf node, append the symbol and reset for next code.
                if (current.IsLeaf)
                {
                    decoded += current.Symbol.Value;
                    current = _root;
                }
            }
            return decoded;
        }

        // Helper method to print the codes for each symbol.
        public void PrintCodes()
        {
            Console.WriteLine("\nHuffman Codes:");
            foreach (var kvp in _codes)
            {
                Console.WriteLine($"{kvp.Key}: {kvp.Value}");
            }
        }

        public void PrintFrequencyTable(Dictionary<char, int> freq)
        {
            Console.WriteLine("\nFrequenzen:");
            foreach (var f in freq)
            {
                Console.WriteLine($"{f.Key}: {f.Value}");
            }
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;

            Console.WriteLine("Eingabe:");
            string text = Console.ReadLine() ?? throw new Exception("Empty input");

            if (File.Exists(text))
                text = File.ReadAllText(text);

            Console.WriteLine("\nKlartext: " + text);

            // Create HuffmanCoding instance and build tree.
            HuffmanCoding huffman = new HuffmanCoding();
            huffman.Build(text);

            // Display the codes for each character.
            huffman.PrintCodes();

            // Encode the text.
            string encoded = huffman.Encode(text);

            
            Console.WriteLine("\nKodierter Text: " + encoded);

            int encoedLength = encoded.Length;
            int clearTextLength = Encoding.UTF8.GetBytes(text.ToCharArray()).Length * 8;
            int dif = clearTextLength - encoedLength;

            float percentageSaved = (float)dif / clearTextLength;

            // Convert to percent
            percentageSaved *= 100f;

            Console.WriteLine($"\nAnzahl der Bits(0,1) im codiertem Text: {encoedLength}, Anzahl Bits im Klartext: {clearTextLength}, Differenz: ≈ {Math.Round(percentageSaved)}%");

            Console.ReadKey();
        }
    }
}
