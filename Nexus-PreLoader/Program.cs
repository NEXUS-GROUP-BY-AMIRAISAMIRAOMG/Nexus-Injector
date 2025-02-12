using System;
using System.IO;
using System.Diagnostics;
using System.Security.Principal;
using System.Threading.Tasks;

namespace Nexus_PreLoader
{
    internal class Program
    {
        static void Main(string[] args)
        {
            // Starting the program
            LogGreen("Program started.");

            // Check for administrator privileges
            if (!IsAdministrator())
            {
                LogRed("No admin rights. Restarting as admin...");
                RestartAsAdmin();
                return;
            }

            Console.SetWindowSize(150, 40);
            Console.Beep();
            Console.ForegroundColor = ConsoleColor.DarkMagenta;

            // Displaying ASCII Art header
            DisplayHeader();

            LogGreen("Running with administrator privileges.");

            string processName = "FortniteClient-Win64-Shipping";
            LogYellow($"Checking if process '{processName}' is running...");

            var processes = TryFindProcess(processName);

            if (processes.Length == 0)
            {
                LogRed($"Process '{processName}' not found, Exiting...");
                WaitForUserToExit();
                return;
            }

            LogGreen($"Found {processes.Length} process(es) for '{processName}'. Proceeding.");

            string dllPathsInput = string.Join(",", args);

            // If no arguments were passed, prompt the user for input
            if (string.IsNullOrEmpty(dllPathsInput))
            {
                LogYellow("Enter comma-separated DLL paths: ");
                dllPathsInput = Console.ReadLine();
            }

            // Log the user input or passed arguments
            LogGreen($"User entered DLL paths: {dllPathsInput}");

            // Split the DLL paths input by commas
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
                    using (var injector = new Reloaded.Injector.Injector(processes[0]))
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

            // Wait for a moment before closing
            Task.Delay(1000).Wait();
            Environment.Exit(0);
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

        // Retry the process search up to 10 times with a 1-second delay between attempts
        static Process[] TryFindProcess(string processName)
        {
            Process[] processes = null;
            int attempts = 0;

            while (attempts < 100)
            {
                processes = Process.GetProcessesByName(processName);

                if (processes.Length > 0)
                    break;

                attempts++;
                LogYellow($"Fortnite Was Not Found, Retrying");
                Task.Delay(1).Wait();
            }
            return processes;
        }

        // Log messages with specific color based on the status
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
    }
}