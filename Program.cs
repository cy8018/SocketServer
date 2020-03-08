using System;
using System.Configuration;
using System.Diagnostics;

namespace SocketServer
{
    class Program
    {
        private static int serverPort = 20000;
        private static string serverIP = "0.0.0.0";

        private static void LoadConfig()
        {
            Configuration config = System.Configuration.ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

            if (config.AppSettings.Settings["ServerListenIP"].Value.Length > 0)
            {
                serverIP = config.AppSettings.Settings["ServerListenIP"].Value;
            }

            string strServerPort = config.AppSettings.Settings["ServerListenPort"].Value;
            if (strServerPort.Length > 0)
            {
                serverPort = int.Parse(strServerPort);
            }
        }


        static void Main(string[] args)
        {

            Process current = Process.GetCurrentProcess();
            Process[] processes = Process.GetProcessesByName(current.ProcessName);
            foreach (Process process in processes)
            {
                if (process.Id != current.Id)
                {
                    if (process.MainModule.FileName == current.MainModule.FileName)
                    {
                        Console.Clear();
                        Console.WriteLine("Application is already running, press any key to exit.");
                        Console.ReadKey();
                        return;
                    }
                }
            }

            LoadConfig();
            SocketServer.Start(serverIP, serverPort);
        }
    }
}
