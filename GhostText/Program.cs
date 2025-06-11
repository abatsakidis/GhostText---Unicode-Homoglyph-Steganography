using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;
using System.IO;

namespace GhostText
{
    class Program
    {
        static readonly Dictionary<char, (char zero, char one)> homoglyphMap = new Dictionary<char, (char zero, char one)>()
        {
            { 'a', ('a', 'а') },
            { 'e', ('e', 'е') },
            { 'o', ('o', 'о') },
            { 'i', ('i', 'і') },
            { 'c', ('c', 'с') },
            { 'd', ('d', 'ԁ') },
            { 'm', ('m', 'м') },
            { 'n', ('n', 'п') },
            { 'r', ('r', 'г') },
            { 's', ('s', 'ѕ') },
            { 't', ('t', 'т') }
        };

        static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;
            if (args.Length == 5 && args[0].ToLower() == "encode")
            {
                EncodeFromFile(args[1], args[2], args[3], args[4]);
            }
            else if (args.Length == 3 && args[0].ToLower() == "decode")
            {
                DecodeFromFile(args[1], args[2]);
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("Usage:");
                Console.WriteLine("  GhostText.exe encode carrier.txt secret.txt output.txt hash.txt");
                Console.WriteLine("  GhostText.exe decode output.txt hash.txt");
                Console.ResetColor();
            }
        }

        static void EncodeFromFile(string carrierPath, string secretPath, string outputPath, string hashPath)
        {
            try
            {
                string carrier = File.ReadAllText(carrierPath, Encoding.UTF8);
                string secret = File.ReadAllText(secretPath, Encoding.UTF8);

                string stegano = HideMessage(carrier, secret);
                string hash = ComputeSHA256Hash(stegano);

                File.WriteAllText(outputPath, stegano, Encoding.UTF8);
                File.WriteAllText(hashPath, hash);

                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine($"Steganographic text saved to '{outputPath}'.");
                Console.WriteLine($"SHA256 hash saved to '{hashPath}'.");
                Console.ResetColor();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Error: " + ex.Message);
                Console.ResetColor();
            }
        }

        static void DecodeFromFile(string steganoPath, string hashPath)
        {
            try
            {
                if (!File.Exists(steganoPath) || !File.Exists(hashPath))
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Required files not found.");
                    Console.ResetColor();
                    return;
                }

                string stegano = File.ReadAllText(steganoPath, Encoding.UTF8);
                string expectedHash = File.ReadAllText(hashPath).Trim();
                string computedHash = ComputeSHA256Hash(stegano);

                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine($"Expected SHA256 hash: {expectedHash}");
                Console.WriteLine($"Computed SHA256 hash: {computedHash}");
                Console.ResetColor();

                if (expectedHash == computedHash)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("Integrity check passed.");
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("Warning: Integrity check failed! Text may be altered.");
                }
                Console.ResetColor();

                string message = ExtractMessage(stegano);
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("\nExtracted Message:");
                Console.WriteLine(message);
                Console.ResetColor();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Error: " + ex.Message);
                Console.ResetColor();
            }
        }

        static string HideMessage(string carrier, string secret)
        {
            byte[] secretBytes = Encoding.UTF8.GetBytes(secret);
            byte[] lengthBytes = BitConverter.GetBytes(secretBytes.Length);
            if (!BitConverter.IsLittleEndian)
                Array.Reverse(lengthBytes);

            StringBuilder bits = new StringBuilder();
            foreach (byte b in lengthBytes)
                bits.Append(Convert.ToString(b, 2).PadLeft(8, '0'));
            foreach (byte b in secretBytes)
                bits.Append(Convert.ToString(b, 2).PadLeft(8, '0'));

            int totalBits = bits.Length;
            int suitableChars = carrier.Count(c => homoglyphMap.ContainsKey(char.ToLower(c)));

            if (suitableChars < totalBits)
                throw new Exception($"Not enough suitable characters in carrier. Needed: {totalBits}, Available: {suitableChars}");

            StringBuilder output = new StringBuilder();
            int bitIndex = 0;
            foreach (char c in carrier)
            {
                char lower = char.ToLower(c);
                if (homoglyphMap.ContainsKey(lower) && bitIndex < totalBits)
                {
                    var glyphs = homoglyphMap[lower];
                    char encoded = bits[bitIndex++] == '1' ? glyphs.one : glyphs.zero;
                    output.Append(char.IsUpper(c) ? char.ToUpper(encoded) : encoded);
                }
                else
                {
                    output.Append(c);
                }
            }

            return output.ToString();
        }

        static string ExtractMessage(string stegano)
        {
            StringBuilder bits = new StringBuilder();
            foreach (char c in stegano)
            {
                char lower = char.ToLower(c);
                foreach (var kv in homoglyphMap)
                {
                    if (lower == kv.Value.zero) { bits.Append('0'); break; }
                    if (lower == kv.Value.one) { bits.Append('1'); break; }
                }
            }

            if (bits.Length < 32) return "";

            byte[] lengthBytes = new byte[4];
            for (int i = 0; i < 4; i++)
                lengthBytes[i] = Convert.ToByte(bits.ToString(i * 8, 8), 2);
            if (!BitConverter.IsLittleEndian)
                Array.Reverse(lengthBytes);

            int length = BitConverter.ToInt32(lengthBytes, 0);
            int totalBits = 32 + (length * 8);
            if (bits.Length < totalBits) return "";

            List<byte> secretBytes = new List<byte>();
            for (int i = 32; i < totalBits; i += 8)
                secretBytes.Add(Convert.ToByte(bits.ToString(i, 8), 2));

            return Encoding.UTF8.GetString(secretBytes.ToArray());
        }

        static string ComputeSHA256Hash(string input)
        {
            using (SHA256 sha = SHA256.Create())
            {
                byte[] bytes = Encoding.UTF8.GetBytes(input);
                byte[] hash = sha.ComputeHash(bytes);
                return Convert.ToBase64String(hash);
            }
        }
    }
}
