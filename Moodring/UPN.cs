using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;


    class UPN
    {
        public static string GetSha256Hash(string rawData)
        {
            // Create a SHA256
            using (SHA256 sha256Hash = SHA256.Create())
            {
                // ComputeHash - returns byte array
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(rawData));

                // Convert byte array to a string
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }

/*        public static void DisplayUPNAndHash()
        {
            string regValue = (string)Registry.GetValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Office\Outlook\Settings\Accounts", "upn", null);
            if (regValue == null)
            {
                Console.WriteLine("UPN value not found in the registry.");
                return;
            }

            string upnName = (string)regValue;
            string hashedUpnValue = GetSha256Hash(upnName);

            Console.WriteLine("UPN: " + upnName);
            Console.WriteLine("Hashed UPN: " + hashedUpnValue);
        }*/

        public static void ListOutlookAccounts()
        {
            string outlookFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Microsoft", "Outlook");
            if (!Directory.Exists(outlookFolder))
            {
                Console.WriteLine("Outlook directory not found.");
                return;
            }

            string[] ostFiles = Directory.GetFiles(outlookFolder, "*.ost");
            if (ostFiles.Length == 0)
            {
                Console.WriteLine("No .ost files found in the Outlook directory.");
                return;
            }

            foreach (string ostFile in ostFiles)
            {
                string upn = Path.GetFileNameWithoutExtension(ostFile);
                string hashedUpn = GetSha256Hash(upn);
                Console.WriteLine($"OST File: {ostFile}");
                Console.WriteLine($"UPN: {upn}");
                Console.WriteLine($"Hashed UPN: {hashedUpn}");
            }
        }

        /*        public static void ListOutlookAccounts()
                {
                    using (RegistryKey accountsKey = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Office\Outlook\Settings\Accounts"))
                    {
                        if (accountsKey == null)
                        {
                            Console.WriteLine("Accounts key not found in the registry.");
                            return;
                        }

                        foreach (string valueName in accountsKey.GetValueNames())
                        {
                            string accountJson = accountsKey.GetValue(valueName) as string;
                            if (!string.IsNullOrEmpty(accountJson))
                            {
                                try
                                {
                                    // Parse the JSON data
                                    using (JsonDocument doc = JsonDocument.Parse(accountJson))
                                    {
                                        foreach (JsonElement account in doc.RootElement.EnumerateArray())
                                        {
                                            string email = account.GetProperty("userUpn").GetString();
                                            string hashedEmail = GetSha256Hash(email);
                                            Console.WriteLine($"Account: {email}");
                                            Console.WriteLine($"Hashed Email: {hashedEmail}");
                                        }
                                    }
                                }
                                catch (System.Text.Json.JsonException ex)
                                {
                                    Console.WriteLine($"Error parsing account information for value {valueName}: {ex.Message}");
                                }
                            }
                            else
                            {
                                Console.WriteLine($"No account information found for value {valueName}");
                            }
                        }
                    }
                }*/
        public static void HashUPN(string input)
        {
            string hashedValue = GetSha256Hash(input);
            Console.WriteLine($"Input: {input}");
            Console.WriteLine($"Hashed Value: {hashedValue}");
        }
        public static void ExUpnDecrypt(string outputPath = null)
        {
            // Read the payload from mod.txt
            string base64Payload = File.ReadAllText("mod.txt");
            byte[] encryptedBytes = Convert.FromBase64String(base64Payload);

            string regValue = (string)Microsoft.Win32.Registry.GetValue(@"HKEY_CURRENT_USER\", "upn", null);
            if (regValue == null)
            {
                Console.WriteLine("UPN value not found in the registry.");
                return;
            }

            string upnName = (string)regValue;
            string hashedUpnValue = GetSha256Hash(upnName);

            // Compare the hashed UPN with stored hash (assuming storedHashValue is provided/defined somewhere)
            string storedHashValue = ""; // You should define or retrieve this value from where it is stored.
            bool correctKey = hashedUpnValue.Equals(storedHashValue);

            if (!correctKey)
            {
                Console.WriteLine("UPN hash does not match the stored hash value.");
                return;
            }

            byte[] keyBytes = Encoding.ASCII.GetBytes(upnName);
            byte[] decryptedBytes = new byte[encryptedBytes.Length];
            Encrypt.RC4EncryptDecrypt(keyBytes, encryptedBytes, decryptedBytes);

            if (outputPath != null)
            {
                File.WriteAllBytes(outputPath, decryptedBytes);
                Console.WriteLine("Decryption complete with UPN: " + upnName);
                Console.WriteLine("Decrypted data written to: " + outputPath);
            }
            else
            {
                Console.WriteLine("Decryption complete with UPN: " + upnName);
                Console.WriteLine("Decrypted: " + Encoding.ASCII.GetString(decryptedBytes));
            }
        }
    }
