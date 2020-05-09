using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;

namespace MBO
{
    internal delegate void SignalHandler(ConsoleSignal consoleSignal);

    internal enum ConsoleSignal
    {
        CtrlC = 0,
        CtrlBreak = 1,
        Close = 2,
        LogOff = 5,
        Shutdown = 6
    }

    internal class Program
    {
        [DllImport("Kernel32", EntryPoint = "SetConsoleCtrlHandler")]
        private static extern bool SetSignalHandler(SignalHandler handler, bool add);

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        private static extern IntPtr GetWindowThreadProcessId(IntPtr hWnd, out uint ProcessId);

        private static SignalHandler signalHandler;
        private static string path;

        private static void Secure()
        {
            FileInfo fileInfo = new FileInfo(path);

            while (true)
            {
                IntPtr hwnd = GetForegroundWindow();
                uint pid;
                GetWindowThreadProcessId(hwnd, out pid);
                Process p = Process.GetProcessById((int)pid);

                if (p.ProcessName.Equals("Far")) fileInfo.IsReadOnly = false;
                else fileInfo.IsReadOnly = true;

                Thread.Sleep(300);
            }
        }

        static void Main(string[] args)
        {
            path = CheckArgunment(args);
            if (path == null) return;

            signalHandler += HandleConsoleSignal;
            SetSignalHandler(signalHandler, true);

            Console.WriteLine("Welcome to secure :)");
            Thread thread = new Thread(Secure) { IsBackground = true };
            thread.Start();
            Console.ReadLine();

            thread.Abort();
            IsClosing();
        }

        private static void HandleConsoleSignal(ConsoleSignal consoleSignal)
        {
            IsClosing();
        }

        private static void IsClosing()
        {
            FileInfo fileInfo = new FileInfo(path) { IsReadOnly = false };
        }

        private static string CheckArgunment(string[] args)
        {
            if (args.Length == 0 || args.Length > 1)
            {
                Console.WriteLine("Argument isn't correct.");
                Console.ReadLine();
                return null;
            }
            return args[0];
        }
    }
}