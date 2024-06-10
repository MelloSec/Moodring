using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

class DNSCheck
{
    public static void CheckDNSAndContinue(string ipAddress)
    {
        // Convert the input string to an IPAddress object
        if (!IPAddress.TryParse(ipAddress, out IPAddress ipToCheck))
        {
            Console.WriteLine("Invalid IP address format.");
            return;
        }

        // Get all network interfaces on the system
        NetworkInterface[] networkInterfaces = NetworkInterface.GetAllNetworkInterfaces();

        // Check each network interface for DNS servers
        foreach (NetworkInterface ni in networkInterfaces)
        {
            IPInterfaceProperties properties = ni.GetIPProperties();
            IPAddressCollection dnsAddresses = properties.DnsAddresses;

            // Check if any of the DNS servers match the provided IP address
            if (dnsAddresses.Any(dns => dns.Equals(ipToCheck)))
            {
                Console.WriteLine("DNS server matches. Continuing...");
                return;
            }
        }

        // If no match was found, exit the program
        Console.WriteLine("No matching DNS server found. Exiting...");
        Environment.Exit(0);
    }
}
