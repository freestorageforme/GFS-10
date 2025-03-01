using System.Text;

namespace HuffmanCodingExample
{
    // Node class for n-ary Huffman Tree
    public class HuffmanNode
    {
        public char? Symbol { get; set; } // Actual symbol for leaves; null for internal/dummy nodes.
        public int Frequency { get; set; }
        public List<HuffmanNode> Children { get; set; }

        // A node is a leaf if it has no children.
        public bool IsLeaf => Children == null || Children.Count == 0;

        public HuffmanNode(char? symbol, int frequency)
        {
            Symbol = symbol;
            Frequency = frequency;
            Children = new List<HuffmanNode>();
        }
    }

    public class HuffmanCoding
    {
        private HuffmanNode _root;
        private Dictionary<char, string> _codes = new Dictionary<char, string>();
        private int _n; // n-ary branching factor

        // Constructor to specify the value of n.
        public HuffmanCoding(int n)
        {
            if (n < 2)
                throw new ArgumentException("n must be at least 2.");
            _n = n;
        }

        // Build the n-ary Huffman tree for the given text.
        public void Build(string text)
        {
            // Build frequency table.
            var frequency = new Dictionary<char, int>();
            foreach (char c in text)
            {
                if (!frequency.ContainsKey(c))
                    frequency[c] = 0;
                frequency[c]++;
            }

            PrintFrequencyTable(frequency);

            // Create initial nodes for each character.
            var nodes = new List<HuffmanNode>();
            foreach (var kvp in frequency)
            {
                nodes.Add(new HuffmanNode(kvp.Key, kvp.Value));
            }

            // Special case: if only one unique symbol, assign code "0" directly.
            if (nodes.Count == 1)
            {
                _root = nodes[0];
                _codes[_root.Symbol.Value] = "0";
                return;
            }

            // Padding: add dummy nodes (with frequency 0) so that 
            // (number_of_leaves - 1) mod (n - 1) == 0.
            int k = nodes.Count;
            int remainder = (k - 1) % (_n - 1);
            if (remainder != 0)
            {
                int dummyCount = (_n - 1) - remainder;
                for (int i = 0; i < dummyCount; i++)
                {
                    // Dummy nodes have null symbol and frequency 0.
                    nodes.Add(new HuffmanNode(null, 0));
                }
            }

            // Combine nodes until one tree remains.
            while (nodes.Count > 1)
            {
                // Sort nodes by frequency.
                nodes = nodes.OrderBy(node => node.Frequency).ToList();
                // Take the n nodes with the smallest frequencies.
                List<HuffmanNode> children = nodes.Take(_n).ToList();
                nodes.RemoveRange(0, _n);

                // Create a new parent node with frequency equal to the sum.
                int sumFrequency = children.Sum(child => child.Frequency);
                HuffmanNode parent = new HuffmanNode(null, sumFrequency);
                parent.Children.AddRange(children);

                // Add the new parent node back to the list.
                nodes.Add(parent);
            }

            // The remaining node is the root of the Huffman tree.
            _root = nodes[0];

            // Generate codes by traversing the tree.
            GenerateCodes(_root, "");
        }

        // Recursively traverse the n-ary tree to generate codes.
        private void GenerateCodes(HuffmanNode node, string code)
        {
            if (node == null)
                return;

            // If we reach a leaf node and it represents a valid symbol, assign its code.
            if (node.IsLeaf)
            {
                if (node.Symbol.HasValue)
                    _codes[node.Symbol.Value] = string.IsNullOrEmpty(code) ? "0" : code;
                return;
            }

            // For each child, append the index (as a digit) to the current code.
            for (int i = 0; i < node.Children.Count; i++)
            {
                GenerateCodes(node.Children[i], code + i.ToString());
            }
        }

        // Encode the input text using the generated n-ary Huffman codes.
        public string Encode(string text)
        {
            string encoded = "";
            foreach (char c in text)
            {
                encoded += _codes[c];
            }
            return encoded;
        }

        // Decode the n-ary Huffman encoded string back to the original text.
        public string Decode(string encodedText)
        {
            string decoded = "";
            HuffmanNode current = _root;
            foreach (char digit in encodedText)
            {
                // Convert the digit character into an index.
                int index = digit - '0';  // Assumes _n is small enough that single digits suffice.
                if (current.Children != null && index < current.Children.Count)
                {
                    current = current.Children[index];
                }
                else
                {
                    throw new Exception("Invalid encoded digit or tree structure.");
                }

                // When a leaf is reached, append its symbol (if valid) and reset to the root.
                if (current.IsLeaf)
                {
                    if (current.Symbol.HasValue)
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

            Console.WriteLine("\nn:");
            int n = Convert.ToInt32(Console.ReadLine() ?? throw new Exception("Empty input"));

            if (File.Exists(text))
                text = File.ReadAllText(text);

            Console.WriteLine("\nKlartext: " + text);

            // Create HuffmanCoding instance and build tree.
            HuffmanCoding huffman = new HuffmanCoding(n);
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
