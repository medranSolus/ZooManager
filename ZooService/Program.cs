using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;

namespace ZooService
{

    class Program
    {
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool SetForegroundWindow(IntPtr hWnd);

        static void Main(string[] args)
        {
            bool createdNew = true;
            using (Mutex mutex = new Mutex(true, "ZooService", out createdNew))
            {
                if (createdNew)
                {
                    const int port = 2560;
                    Console.WriteLine($"Starting Zoo Service on port {port}, please wait...");
                    ZooService service = new ZooService(port);
                    service.Start();
                    Console.WriteLine("Zoo Service is ON. Press [E] to stop.");
                    while (Console.ReadKey().Key == ConsoleKey.E) ;
                }
                else
                {
                    Process current = Process.GetCurrentProcess();
                    foreach (Process process in Process.GetProcessesByName(current.ProcessName))
                    {
                        if (process.Id != current.Id)
                        {
                            SetForegroundWindow(process.MainWindowHandle);
                            break;
                        }
                    }
                }
            }
        }
    }
}
