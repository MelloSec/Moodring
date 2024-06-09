using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cryptorchid
{
    public static class Encrypt
    {
        public const int HASH_SIZE = 4; // Size of the hash in bytes (32-bit integer)
        public const int kL = 4; // Key length
        public static void RC4EncryptDecrypt(byte[] key, byte[] input, byte[] output)
        {
            int len = key.Length;
            byte[] S = new byte[256];
            byte tmp;
            int j = 0;

            for (int i = 0; i < 256; i++)
                S[i] = (byte)i;

            for (int i = 0; i < 256; i++)
            {
                j = (j + S[i] + key[i % len]) % 256;
                tmp = S[i];
                S[i] = S[j];
                S[j] = tmp;
            }

            int iIndex = 0;
            int jIndex = 0;

            for (int n = 0; n < input.Length; n++)
            {
                iIndex = (iIndex + 1) % 256;
                jIndex = (jIndex + S[iIndex]) % 256;
                tmp = S[iIndex];
                S[iIndex] = S[jIndex];
                S[jIndex] = tmp;
                int rnd = S[(S[iIndex] + S[jIndex]) % 256];
                output[n] = (byte)(rnd ^ input[n]);
            }
        }

        public static uint DJB2Hash(byte[] data)
        {
            uint hash = 9876;

            foreach (byte b in data)
            {
                hash = ((hash << 5) + hash) + b;
            }

            return hash;
        }

        public static string GenerateRandomKey(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, length).Select(s => s[random.Next(s.Length)]).ToArray());
        }

        public static string ByteArrayToHexString(byte[] byteArray)
        {
            return BitConverter.ToString(byteArray).Replace("-", "");
        }
        public static void EncryptFile(string inputFilePath, string outputFilePath)
        {
            /*        string keyString = GenerateRandomKey(kL); // Generate a random key of length kL*/

            string keyString = "T";
            byte[] key = Encoding.ASCII.GetBytes(keyString);

            /*byte[] key = GenerateRandomKey(kL); // Generate a random key of length kL*/
            byte[] inputBytes = File.ReadAllBytes(inputFilePath);
            uint plaintextHash = Encrypt.DJB2Hash(inputBytes); // Calculate the hash of the plaintext

            // Embed the hash at the beginning of the data
            byte[] hashBytes = BitConverter.GetBytes(plaintextHash);
            byte[] combinedInput = new byte[HASH_SIZE + inputBytes.Length];
            Array.Copy(hashBytes, 0, combinedInput, 0, HASH_SIZE);
            Array.Copy(inputBytes, 0, combinedInput, HASH_SIZE, inputBytes.Length);

            // Encrypt the combined data (hash + plaintext)
            byte[] outputBytes = new byte[combinedInput.Length];
            Encrypt.RC4EncryptDecrypt(key, combinedInput, outputBytes);

            File.WriteAllBytes(outputFilePath, outputBytes);
            Console.WriteLine("Encryption complete. Key: " + keyString);
            Console.WriteLine("Embedded hash: " + plaintextHash);


            /*            Console.WriteLine("Encryption complete. Key: " + BitConverter.ToString(key).Replace("-", ""));
                        Console.WriteLine("Embedded hash: " + plaintextHash);*/
        }


        public static void DecryptFile(string inputFilePath, string outputFilePath)
        {
            byte[] encryptedData = File.ReadAllBytes(inputFilePath);

            // Extract the embedded hash (first HASH_SIZE bytes of the encrypted data)
            uint hardcodedHash = BitConverter.ToUInt32(encryptedData, 0);
            Console.WriteLine("Extracted hash: " + hardcodedHash);

            // Attempt to crack the encryption key
            string crackedKeyString = CrackKey(encryptedData, hardcodedHash);
            if (string.IsNullOrEmpty(crackedKeyString))
            {
                Console.WriteLine("Failed to crack the encryption key.");
                return;
            }

            byte[] crackedKey = Encoding.ASCII.GetBytes(crackedKeyString);
            byte[] outputBytes = new byte[encryptedData.Length];
            Encrypt.RC4EncryptDecrypt(crackedKey, encryptedData, outputBytes);

            // Copy the decrypted data, excluding the first HASH_SIZE bytes (embedded hash)
            byte[] decryptedData = new byte[encryptedData.Length - HASH_SIZE];
            Array.Copy(outputBytes, HASH_SIZE, decryptedData, 0, decryptedData.Length);

            File.WriteAllBytes(outputFilePath, decryptedData);

            Console.WriteLine("Decryption complete.");
        }

        public static string RecursiveCrack(byte[] encryptedData, byte[] key, int level, uint hardcodedHash)
        {
            string keySpace = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            byte[] decryptedData = new byte[encryptedData.Length];

            for (int i = 0; i < keySpace.Length; i++)
            {
                key[kL - level] = (byte)keySpace[i];

                /* Console.WriteLine($"Level {level}, Trying key: {Encoding.ASCII.GetString(key)}");*/

                if (level == kL)
                {
                    Encrypt.RC4EncryptDecrypt(key, encryptedData, decryptedData);
                    uint currentHash = Encrypt.DJB2Hash(decryptedData);

                    if (currentHash == hardcodedHash)
                    {
                        Console.WriteLine("Key found!");
                        return Encoding.ASCII.GetString(key);
                    }
                }
                else
                {
                    string result = RecursiveCrack(encryptedData, key, level + 1, hardcodedHash);
                    if (result != null)
                        return result;
                }
            }

            return null;
        }

        public static string CrackKey(byte[] encryptedData, uint hardcodedHash)
        {
            byte[] key = new byte[kL];
            return RecursiveCrack(encryptedData, key, 1, hardcodedHash);
        }
        public static void ExEncrypt(string inputFilePath, string key)
        {
            // Read the plaintext from the input file
            byte[] plaintextBytes = File.ReadAllBytes(inputFilePath);

            // Convert the key to byte arrays
            byte[] keyBytes = Encoding.ASCII.GetBytes(key);
            byte[] encryptedBytes = new byte[plaintextBytes.Length];

            // Perform RC4 encryption
            RC4EncryptDecrypt(keyBytes, plaintextBytes, encryptedBytes);

            // Base64 encode the encrypted data
            string base64Encrypted = Convert.ToBase64String(encryptedBytes);

            // Write the base64-encoded encrypted data to a text file
            File.WriteAllText("mod.txt", base64Encrypted);

            // Compute the hash
            uint hardcodedHash = DJB2Hash(plaintextBytes);

            // Write the hash to a text file
            File.WriteAllText("hash.txt", hardcodedHash.ToString());
        }

        public static void ExDecrypt(string outputPath = null)
        {
            // Uses the hash spit out during encyrption.  In future, hash can just be taken from a .png's filename or metadata or something
            byte[] encryptedBytes = File.ReadAllBytes("encrypted.bin");
            uint hardcodedHash = uint.Parse(File.ReadAllText("hash.txt"));

            string crackedKey = CrackKey(encryptedBytes, hardcodedHash);
            if (crackedKey == null)
            {
                Console.WriteLine("Failed to crack the encryption key.");
                return;
            }

            byte[] keyBytes = Encoding.ASCII.GetBytes(crackedKey);
            byte[] decryptedBytes = new byte[encryptedBytes.Length];
            RC4EncryptDecrypt(keyBytes, encryptedBytes, decryptedBytes);

            if (outputPath != null)
            {
                File.WriteAllBytes(outputPath, decryptedBytes);
                Console.WriteLine("Decryption complete with key: " + crackedKey);
                Console.WriteLine("Decrypted data written to: " + outputPath);
            }
            else
            {
                Console.WriteLine("Decryption complete with key: " + crackedKey);
                Console.WriteLine("Decrypted: " + Encoding.ASCII.GetString(decryptedBytes));
            }
        }
    }
}
