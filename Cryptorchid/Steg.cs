using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Cryptorchid
{
    public static class SteganographyHelper
    {
        public static void HideUPNInImage(string imageFilePath, string upn)
        {
            // Hash the UPN
            string hashString = GetSha256Hash(upn);

            // Convert the hash string to bytes
            byte[] hashBytes = Encoding.UTF8.GetBytes(hashString); // Use the full hash string
            int hashByteIndex = 0;
            int hashBitIndex = 0;

            Bitmap bitmap = new Bitmap(imageFilePath);

            for (int y = 0; y < bitmap.Height; y++)
            {
                for (int x = 0; x < bitmap.Width; x++)
                {
                    if (hashByteIndex >= hashBytes.Length)
                    {
                        // All hash bytes have been hidden
                        string newImageFilePath = Path.Combine(Path.GetDirectoryName(imageFilePath), Path.GetFileNameWithoutExtension(imageFilePath) + "_copy" + Path.GetExtension(imageFilePath));
                        bitmap.Save(newImageFilePath, ImageFormat.Png);
                        return;
                    }

                    Color pixelColor = bitmap.GetPixel(x, y);

                    // Modify the least significant bit of the blue channel
                    byte blue = pixelColor.B;
                    blue = (byte)((blue & 0xFE) | ((hashBytes[hashByteIndex] >> hashBitIndex) & 1));

                    Color newPixelColor = Color.FromArgb(pixelColor.R, pixelColor.G, blue);
                    bitmap.SetPixel(x, y, newPixelColor);

                    hashBitIndex++;
                    if (hashBitIndex == 8)
                    {
                        hashBitIndex = 0;
                        hashByteIndex++;
                    }
                }
            }

            // Save the modified image
            string finalImageFilePath = Path.Combine(Path.GetDirectoryName(imageFilePath), Path.GetFileNameWithoutExtension(imageFilePath) + "_copy" + Path.GetExtension(imageFilePath));
            bitmap.Save(finalImageFilePath, ImageFormat.Png);
        }

        public static string RetrieveUPNFromImage(string imageFilePath)
        {
            Bitmap bitmap = new Bitmap(imageFilePath);
            byte[] hashBytes = new byte[64]; // Adjust the size to match the full hash length in characters
            int hashByteIndex = 0;
            int hashBitIndex = 0;

            for (int y = 0; y < bitmap.Height; y++)
            {
                for (int x = 0; x < bitmap.Width; x++)
                {
                    if (hashByteIndex >= hashBytes.Length)
                    {
                        // All hash bytes have been retrieved
                        return Encoding.UTF8.GetString(hashBytes); // Return as string
                    }

                    Color pixelColor = bitmap.GetPixel(x, y);

                    // Extract the least significant bit of the blue channel
                    byte blue = pixelColor.B;
                    hashBytes[hashByteIndex] |= (byte)((blue & 1) << hashBitIndex);

                    hashBitIndex++;
                    if (hashBitIndex == 8)
                    {
                        hashBitIndex = 0;
                        hashByteIndex++;
                    }
                }
            }

            // If the loop completes without returning, return the retrieved hash
            return Encoding.UTF8.GetString(hashBytes); // Return as string
        }

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
    }
    public static class Steg
    {
        public static void ExStegDecrypt(string imageFilePath, string loadMethodSignature = null)
        {
            // Extract the hash from the least significant bits of the image
            uint extractedHash = ExtractHashFromImage(imageFilePath);
            Console.WriteLine($"Extracted hash: {extractedHash}");

            // Read the base64-encoded encrypted data
            string base64Encrypted = File.ReadAllText("mod.txt");
            byte[] encryptedBytes = Convert.FromBase64String(base64Encrypted);

            // Attempt to crack the encryption key using the extracted hash
            string crackedKey = Encrypt.CrackKey(encryptedBytes, extractedHash);
            if (crackedKey == null)
            {
                Console.WriteLine("Failed to crack the encryption key.");
                return;
            }

            // Decrypt the data using the cracked key
            byte[] keyBytes = Encoding.ASCII.GetBytes(crackedKey);
            byte[] decryptedBytes = new byte[encryptedBytes.Length];
            Encrypt.RC4EncryptDecrypt(keyBytes, encryptedBytes, decryptedBytes);

            // Print the decrypted contents
            Console.WriteLine("Decryption complete with key: " + crackedKey);
            Console.WriteLine("Decrypted: " + Encoding.ASCII.GetString(decryptedBytes));

        }
        public static uint ExtractHashFromImage(string imageFilePath)
        {
            Bitmap bitmap = new Bitmap(imageFilePath);

            byte[] hashBytes = new byte[sizeof(uint)];
            int hashByteIndex = 0;
            int hashBitIndex = 0;

            for (int y = 0; y < bitmap.Height; y++)
            {
                for (int x = 0; x < bitmap.Width; x++)
                {
                    if (hashByteIndex >= hashBytes.Length)
                    {
                        // All hash bytes have been extracted
                        return BitConverter.ToUInt32(hashBytes, 0);
                    }

                    Color pixelColor = bitmap.GetPixel(x, y);

                    // Extract the least significant bit of the blue channel
                    byte blue = pixelColor.B;
                    hashBytes[hashByteIndex] |= (byte)((blue & 1) << hashBitIndex);

                    hashBitIndex++;
                    if (hashBitIndex == 8)
                    {
                        hashBitIndex = 0;
                        hashByteIndex++;
                    }
                }
            }

            return BitConverter.ToUInt32(hashBytes, 0);
        }
        public static void EncryptAndHideInImage(string inputFilePath, string imageFilePath, string key)
        {
            // Call the ExEncrypt method to perform encryption
            Encrypt.ExEncrypt(inputFilePath, key);

            // Read the hash from the hash.txt file
            uint hash = uint.Parse(File.ReadAllText("hash.txt"));

            // Hide the hash in the least significant bits of the image
            HideHashInImage(imageFilePath, hash);

            Console.WriteLine($"Hash {hash} hidden in image {imageFilePath}");
        }

        private static void HideHashInImage(string imageFilePath, uint hash)
        {
            Bitmap bitmap = new Bitmap(imageFilePath);

            byte[] hashBytes = BitConverter.GetBytes(hash);
            int hashByteIndex = 0;
            int hashBitIndex = 0;

            for (int y = 0; y < bitmap.Height; y++)
            {
                for (int x = 0; x < bitmap.Width; x++)
                {
                    if (hashByteIndex >= hashBytes.Length)
                    {
                        // All hash bytes have been hidden
                        string newImageFilePath = Path.Combine(Path.GetDirectoryName(imageFilePath), Path.GetFileNameWithoutExtension(imageFilePath) + "_copy" + Path.GetExtension(imageFilePath));
                        bitmap.Save(newImageFilePath, ImageFormat.Png);
                        return;
                    }

                    Color pixelColor = bitmap.GetPixel(x, y);

                    // Modify the least significant bit of the blue channel
                    byte blue = pixelColor.B;
                    blue = (byte)((blue & 0xFE) | ((hashBytes[hashByteIndex] >> hashBitIndex) & 1));

                    Color newPixelColor = Color.FromArgb(pixelColor.R, pixelColor.G, blue);
                    bitmap.SetPixel(x, y, newPixelColor);

                    hashBitIndex++;
                    if (hashBitIndex == 8)
                    {
                        hashBitIndex = 0;
                        hashByteIndex++;
                    }
                }
            }

            // Save the modified image
            string finalImageFilePath = Path.Combine(Path.GetDirectoryName(imageFilePath), Path.GetFileNameWithoutExtension(imageFilePath) + "_copy" + Path.GetExtension(imageFilePath));
            bitmap.Save(finalImageFilePath, ImageFormat.Png);
        }
        public static void ExStegDecryptAndLoad(string imageFilePath, string loadMethodSignature)
        {
            // Extract the hash from the least significant bits of the image
            uint extractedHash = ExtractHashFromImage(imageFilePath);
            Console.WriteLine($"Extracted hash: {extractedHash}");

            /*            // Read the encrypted data directly from the binary file
                        byte[] encryptedBytes = File.ReadAllBytes("encrypted.bin");
            */
            string base64Encrypted = File.ReadAllText("mod.txt");
            byte[] encryptedBytes = Convert.FromBase64String(base64Encrypted);

            // Attempt to crack the encryption key using the extracted hash
            string crackedKey = Encrypt.CrackKey(encryptedBytes, extractedHash);
            if (crackedKey == null)
            {
                Console.WriteLine("Failed to crack the encryption key.");
                return;
            }

            // Decrypt the data using the cracked key
            byte[] keyBytes = Encoding.ASCII.GetBytes(crackedKey);
            byte[] decryptedBytes = new byte[encryptedBytes.Length];
            Encrypt.RC4EncryptDecrypt(keyBytes, encryptedBytes, decryptedBytes);

            // DLL Verification - This needs to come as an optional argument, give the source DLL to perform integrity check.
            // Verify the decrypted DLL against the original DLL
/*            string originalDllPath = "Calc.dll"; // Set the path to the original DLL used for encryption
            byte[] originalDllBytes = File.ReadAllBytes(originalDllPath);

            if (originalDllBytes.SequenceEqual(decryptedBytes))
            {
                Console.WriteLine("Decrypted DLL matches the original DLL.");
            }
            else
            {
                Console.WriteLine("Decrypted DLL does not match the original DLL.");
            }*/

            // Load the DLL from the decrypted bytes in memory and invoke the specified method
            try
            {
                Load.InvokeMethodFromDecryptedData(decryptedBytes, loadMethodSignature);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error invoking method: {ex.Message}");
            }
        }
    }
}
