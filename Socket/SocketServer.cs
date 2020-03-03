using log4net;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace SocketServer
{
    public static class SocketServer
    {
        [DllImport("user32.dll", EntryPoint = "keybd_event", SetLastError = true)]
        public static extern void keybd_event(Keys bVk, byte bScan, uint dwFlags, uint dwExtraInfo);

        public const int KEYEVENTF_KEYDOWN = 0x0000; // New definition
        public const int KEYEVENTF_EXTENDEDKEY = 0x0001; //Key down flag
        public const int KEYEVENTF_KEYUP = 0x0002; //Key up flag

        private static ILog logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private static byte[] result = new byte[1024];
        
        private static Socket serverSocket;
        private static List<Socket> clientSockets = new List<Socket>();

        public static void Start(string serverIP, int serverPort)
        {
            string log = string.Format("Service is now starting...");
            Console.Clear();
            Console.WriteLine(log);
            Console.WriteLine();
            logger.Warn(log);

            IPAddress ip = IPAddress.Parse(serverIP);

            try
            {
                serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                serverSocket.Bind(new IPEndPoint(ip, serverPort)); 
                serverSocket.Listen(10);
                log = string.Format("Listening at [{0}] successfully.", serverSocket.LocalEndPoint.ToString());

                logger.Warn(log);
                Console.WriteLine(log);

                Thread myThread = new Thread(ListenClientConnect);
                myThread.Start();
            }
            catch (Exception e)
            {
                log = string.Format("Exception when starting service：{0}", e.Message);

                Console.WriteLine(log);
                logger.Error(log);
            }
        }

        private static void ListenClientConnect()
        {
            while (true)
            {
                Socket clientSocket = serverSocket.Accept();
                clientSockets.Add(clientSocket);
                
                string log = string.Format("Client [{0}] Connected.", clientSocket.RemoteEndPoint.ToString());
                logger.Info(log);
                if (logger.IsInfoEnabled || logger.IsDebugEnabled)
                {
                    Console.WriteLine(log);
                }

                Thread receiveThread = new Thread(ReceiveMessage);
                receiveThread.Start(clientSocket);
            }
        }

        private static void ReceiveMessage(object clientSocket)
        {
            Socket myClientSocket = (Socket)clientSocket;
            while (true)
            {
                try
                {
                    int receiveNumber = myClientSocket.Receive(result);
                    if(receiveNumber > 0)
                    {
                        string strContent = Encoding.ASCII.GetString(result, 0, receiveNumber);
                        if (strContent.Length > 1)
                        {
                            string log = string.Format("Message [{1}] received from client [{0}]", myClientSocket.RemoteEndPoint.ToString(), Encoding.ASCII.GetString(result, 0, receiveNumber));

                            logger.Debug(log);
                            if (logger.IsDebugEnabled)
                            {
                                Console.WriteLine(log);
                            }

                            if (strContent.Contains("VolUp"))
                            {
                                keybd_event(Keys.Up, 0, 0, 0);
                            }
                            else if (strContent.Contains("VolDown"))
                            {
                                keybd_event(Keys.Down, 0, 0, 0);
                            }
                            else if (strContent.Contains("Next"))
                            {
                                keybd_event(Keys.PageDown, 0, 0, 0);
                            }
                            else if (strContent.Contains("Prev"))
                            {
                                keybd_event(Keys.PageUp, 0, 0, 0);
                            }
                            else if (strContent.Contains("PlayPause"))
                            {
                                keybd_event(Keys.Space, 0, 0, 0);
                            }
                            else if (strContent.Contains("Mute"))
                            {
                                keybd_event(Keys.M, 0, 0, 0);
                            }
                            else if (strContent.Contains("F5S"))
                            {
                                keybd_event(Keys.Right, 0, KEYEVENTF_KEYDOWN, 0);
                                keybd_event(Keys.Right, 0, KEYEVENTF_KEYUP, 0);
                            }
                            else if (strContent.Contains("F30S"))
                            {
                                keybd_event(Keys.Control, 0, KEYEVENTF_KEYDOWN, 0);
                                keybd_event(Keys.Right, 0, KEYEVENTF_KEYDOWN, 0);
                                keybd_event(Keys.Right, 0, KEYEVENTF_KEYUP, 0);
                                keybd_event(Keys.Control, 0, KEYEVENTF_KEYUP, 0);
                            }
                            else if (strContent.Contains("B5S"))
                            {
                                keybd_event(Keys.Left, 0, KEYEVENTF_KEYDOWN, 0);
                                keybd_event(Keys.Left, 0, KEYEVENTF_KEYUP, 0);
                            }
                            else if (strContent.Contains("B30S"))
                            {
                                keybd_event(Keys.Control, 0, KEYEVENTF_KEYDOWN, 0);
                                keybd_event(Keys.Left, 0, KEYEVENTF_KEYDOWN, 0);
                                keybd_event(Keys.Left, 0, KEYEVENTF_KEYUP, 0);
                                keybd_event(Keys.Control, 0, KEYEVENTF_KEYUP, 0);
                            }
                            else if (strContent.Contains("Shutdown"))
                            {
                                ShutDownPC();
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    logger.Error(ex.Message);

                    if (myClientSocket != null && myClientSocket.Connected)
                    {
                        myClientSocket.Shutdown(SocketShutdown.Both);
                    }                    
                    myClientSocket.Close();
                    break;
                }
                Thread.Sleep(20);
            }
        }

        private static void ShutDownPC()
        {
            var psi = new ProcessStartInfo("shutdown", "/s /t 0");
            psi.CreateNoWindow = true;
            psi.UseShellExecute = false;
            Process.Start(psi);
        }

        private static void SendMessage(string message)
        {
            foreach (Socket clientSocket in clientSockets)
            {
                if (clientSocket!=null&& clientSocket.Connected)
                {
                    clientSocket.Send(Encoding.ASCII.GetBytes(message));

                    string log = string.Format("Send message [{1}] to client [{0}].", clientSocket.RemoteEndPoint.ToString(), message);

                    logger.Debug(log);
                    if (logger.IsDebugEnabled)
                    {
                        Console.WriteLine(log);
                    }
                }
            }
        }
    }
}
