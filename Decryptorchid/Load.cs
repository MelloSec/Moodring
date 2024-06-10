using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public static class Load
{
    public static void TestLoad(string loadMethodSignature)
    {
        string precompiledDllPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "calc.dll");

        // Check if the precompiled DLL exists
        if (!File.Exists(precompiledDllPath))
        {
            Console.WriteLine($"Error: The precompiled DLL file does not exist at {precompiledDllPath}");
            return;
        }

        // Load the DLL and invoke the specified method
        try
        {
            InvokeMethodFromPrecompiledData(precompiledDllPath, loadMethodSignature);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error invoking method: {ex.Message}");
        }
    }

    private static void InvokeMethodFromPrecompiledData(string assemblyPath, string methodSignature)
    {
        // Load the assembly from the precompiled file path
        var assembly = System.Reflection.Assembly.LoadFrom(assemblyPath);

        // Parse the method signature
        var parts = methodSignature.Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length < 3)
        {
            throw new ArgumentException("Invalid method signature. Expected format: Namespace.Class.Method");
        }

        var namespaceAndClass = string.Join(".", parts.Take(parts.Length - 1));
        var methodName = parts.Last();

        // Find the type and method
        var type = assembly.GetType(namespaceAndClass);
        if (type == null)
        {
            throw new ArgumentException($"Type {namespaceAndClass} not found in assembly.");
        }

        var method = type.GetMethod(methodName);
        if (method == null)
        {
            throw new ArgumentException($"Method {methodName} not found in type {namespaceAndClass}.");
        }

        // Invoke the method
        var instance = Activator.CreateInstance(type);
        method.Invoke(instance, null);
    }

    public static void ExStegDecryptAndLoad(string imageFilePath, string loadMethodSignature)
    {
        // Extract the hash from the least significant bits of the image
        uint extractedHash = Steg.ExtractHashFromImage(imageFilePath);
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

        // Verify the decrypted DLL against the original DLL
        string originalDllPath = "Calc.dll"; // Set the path to the original DLL used for encryption
        byte[] originalDllBytes = File.ReadAllBytes(originalDllPath);

        if (originalDllBytes.SequenceEqual(decryptedBytes))
        {
            Console.WriteLine("Decrypted DLL matches the original DLL.");
        }
        else
        {
            Console.WriteLine("Decrypted DLL does not match the original DLL.");
        }

        // Load the DLL from the decrypted bytes in memory and invoke the specified method
        try
        {
            InvokeMethodFromDecryptedData(decryptedBytes, loadMethodSignature);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error invoking method: {ex.Message}");
        }
    }

    public static void InvokeMethodFromDecryptedData(byte[] decryptedBytes, string methodSignature)
    {
        // Load the assembly from the decrypted bytes
        var assembly = System.Reflection.Assembly.Load(decryptedBytes);

        // Parse the method signature
        var parts = methodSignature.Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length < 3)
        {
            throw new ArgumentException("Invalid method signature. Expected format: Namespace.Class.Method");
        }

        var namespaceAndClass = string.Join(".", parts.Take(parts.Length - 1));
        var methodName = parts.Last();

        // Find the type and method
        var type = assembly.GetType(namespaceAndClass);
        if (type == null)
        {
            throw new ArgumentException($"Type {namespaceAndClass} not found in assembly.");
        }

        var method = type.GetMethod(methodName);
        if (method == null)
        {
            throw new ArgumentException($"Method {methodName} not found in type {namespaceAndClass}.");
        }

        // Invoke the method
        var instance = Activator.CreateInstance(type);
        method.Invoke(instance, null);
    }
}