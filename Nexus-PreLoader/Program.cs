using System;
using System.IO;
using System.Diagnostics;
using System.Security.Principal;
using System.Threading.Tasks;
using System.Linq;

namespace Nexus_PreLoader
{
    internal class Program
    {
        static void Main(string[] args)
        {
            LogGreen("Program started.");

            if (!IsAdministrator())
            {
                LogRed("No admin rights. Restarting as admin...");
                RestartAsAdmin();
                return;
            }

            Console.SetWindowSize(150, 40);
            Console.Beep();
            Console.ForegroundColor = ConsoleColor.DarkMagenta;

            DisplayHeader();

            LogGreen("Running with administrator privileges.");

            string processName = "FortniteClient-Win64-Shipping";
            LogYellow($"Searching for running instances of '{processName}'...");

            var processes = TryFindProcess(processName);

            if (processes.Length == 0)
            {
                LogRed($"No instances of '{processName}' found. Exiting...");
                WaitForUserToExit();
                return;
            }

            // Ask user to select a process instance if multiple are found
            int selectedIndex = SelectProcessInstance(processes);
            if (selectedIndex == -1)
            {
                LogRed("No valid process selected. Exiting...");
                WaitForUserToExit();
                return;
            }

            Process selectedProcess = processes[selectedIndex];

            LogGreen($"Selected Process: {selectedProcess.ProcessName} (PID: {selectedProcess.Id})");

            string dllPathsInput = string.Join(",", args);

            if (string.IsNullOrEmpty(dllPathsInput))
            {
                LogYellow("Enter comma-separated DLL paths: ");
                dllPathsInput = Console.ReadLine();
            }

            LogGreen($"User entered DLL paths: {dllPathsInput}");

            var dllPaths = dllPathsInput.Split(',');
            foreach (var dllPath in dllPaths)
            {
                string trimmedDllPath = dllPath.Trim();

                if (!File.Exists(trimmedDllPath))
                {
                    LogRed($"DLL not found at {trimmedDllPath}. Skipping...");
                    continue;
                }

                LogGreen($"Found DLL: {trimmedDllPath}. Proceeding with injection.");

                try
                {
                    using (var injector = new Reloaded.Injector.Injector(selectedProcess))
                    {
                        LogYellow("Injecting DLL...");

                        long dllBaseAddress = injector.Inject(trimmedDllPath);

                        if (dllBaseAddress != 0)
                        {
                            LogGreen($"Injection successful! Base address: 0x{dllBaseAddress:X}");
                            Console.Beep();
                        }
                        else
                        {
                            LogRed("Injection failed.");
                            Console.Beep();
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogRed($"Error during injection: {ex.Message}");
                }
            }

            LogGreen("Execution finished.");
            Task.Delay(1000).Wait();
            Environment.Exit(0);
        }

        static int SelectProcessInstance(Process[] processes)
        {
            LogYellow("Multiple instances found. Please select one:");

            for (int i = 0; i < processes.Length; i++)
            {
                Console.WriteLine($"[{i}] Process ID: {processes[i].Id} | Name: {processes[i].ProcessName}");
            }

            Console.Write("Enter the number of the process to inject into: ");
            string input = Console.ReadLine();

            if (int.TryParse(input, out int selectedIndex) && selectedIndex >= 0 && selectedIndex < processes.Length)
            {
                return selectedIndex;
            }

            LogRed("Invalid selection.");
            return -1;
        }

        static Process[] TryFindProcess(string processName)
        {
            Process[] processes = null;
            int attempts = 0;

            while (attempts < 10)
            {
                processes = Process.GetProcessesByName(processName);

                if (processes.Length > 0)
                    break;

                attempts++;
                LogYellow($"Fortnite not found, retrying... ({attempts}/10)");
                Task.Delay(1000).Wait();
            }
            return processes;
        }

        static bool IsAdministrator()
        {
            using (WindowsIdentity identity = WindowsIdentity.GetCurrent())
            {
                WindowsPrincipal principal = new WindowsPrincipal(identity);
                bool isAdmin = principal.IsInRole(WindowsBuiltInRole.Administrator);
                LogGreen(isAdmin ? "Administrator rights confirmed." : "No admin rights.");
                return isAdmin;
            }
        }

        static void RestartAsAdmin()
        {
            LogYellow("Restarting as administrator...");

            ProcessStartInfo startInfo = new ProcessStartInfo()
            {
                FileName = Process.GetCurrentProcess().MainModule.FileName,
                UseShellExecute = true,
                Verb = "runas"
            };

            try
            {
                Process.Start(startInfo);
                LogGreen("Requested admin privileges.");
            }
            catch (Exception ex)
            {
                LogRed($"Failed to restart: {ex.Message}");
            }

            Environment.Exit(0);
        }

        static void WaitForUserToExit()
        {
            LogGreen("Press any key to exit...");
            Console.ReadKey();
        }

        static void LogGreen(string message)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(message);
        }

        static void LogRed(string message)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(message);
        }

        static void LogYellow(string message)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(message);
        }

        static void DisplayHeader()
        {
            Console.WriteLine("");
            Console.WriteLine("");
            Console.WriteLine("███╗   ██╗███████╗██╗  ██╗██╗   ██╗███████╗    ██████╗ ██╗     ██╗         ██╗███╗   ██╗     ██╗███████╗ ██████╗████████╗ ██████╗ ██████╗ ");
            Console.WriteLine("████╗  ██║██╔════╝╚██╗██╔╝██║   ██║██╔════╝    ██╔══██╗██║     ██║         ██║████╗  ██║     ██║██╔════╝██╔════╝╚══██╔══╝██╔═══██╗██╔══██╗");
            Console.WriteLine("██╔██╗ ██║█████╗   ╚███╔╝ ██║   ██║███████╗    ██║  ██║██║     ██║         ██║██╔██╗ ██║     ██║█████╗  ██║        ██║   ██║   ██║██████╔╝");
            Console.WriteLine("██║╚██╗██║██╔══╝   ██╔██╗ ██║   ██║╚════██║    ██║  ██║██║     ██║         ██║██║╚██╗██║██   ██║██╔══╝  ██║        ██║   ██║   ██║██╔══██╗");
            Console.WriteLine("██║ ╚████║███████╗██╔╝ ██╗╚██████╔╝███████║    ██████╔╝███████╗███████╗    ██║██║ ╚████║╚█████╔╝███████╗╚██████╗   ██║   ╚██████╔╝██║  ██║");
            Console.WriteLine("╚═╝  ╚═══╝╚══════╝╚═╝  ╚═╝ ╚═════╝ ╚══════╝    ╚═════╝ ╚══════╝╚══════╝    ╚═╝╚═╝  ╚═══╝ ╚════╝ ╚══════╝ ╚═════╝   ╚═╝    ╚═════╝ ╚═╝  ╚═╝");
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.Title = "Nexus PreLoader By AmiraIsAmiraOMG";
            Console.WriteLine(" ____  _  _     __   _  _  __  ____   __   __  ____   __   _  _  __  ____   __    __   _  _   ___ ");
            Console.WriteLine("(  _ \\( \\/ )   / _\\ ( \\/ )(  )(  _ \\ / _\\ (  )/ ___) / _\\ ( \\/ )(  )(  _ \\ / _\\  /  \\ ( \\/ ) / __)");
            Console.WriteLine(" ) _ ( )  /   /    \\/ \\/ \\ )(  )   //    \\ )( \\___ \\/    \\/ \\/ \\ )(  )   //    \\(  O )/ \\/ \\( (_ \\");
            Console.WriteLine("(____/(__/    \\_/\\_/\\_)(_/(__)(__\\_)\\_/\\_/(__)(____/\\_/\\_/\\_)(_/(__)(__\\_)\\_/\\_/ \\__/ \\_)(_/ \\___/");
            Console.WriteLine("");
            Console.WriteLine("");
            Console.ResetColor();
        }
    }
}
