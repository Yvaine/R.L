using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Player
{
    class Player
    {
        private Socket Channel { get; set; }
        private static List<Process> Processes { get; set; }
        static void Main(string[] args)
        {
            Console.Title = "Player";
            var player = new Player();
            Player.Bootstrap();
            player.Init();
            player.Iterate();
            Console.ReadKey(true);
        }
        private static void Bootstrap()
        {
            if (System.Diagnostics.Process.GetProcessesByName("pm").Length == 0)
            {
                Player.Processes.Add(new Process());
            }
        }
        private void o(string format, params object[] arg) { Console.WriteLine(format, arg); }
        private byte[] s2b(string __string, params object[] arg) { return Encoding.ASCII.GetBytes(String.Format(__string, arg)); }
        private string b2s(byte[] __bytes) { return Encoding.ASCII.GetString(__bytes); }
        private void Init()
        {
            var pname = Process.GetCurrentProcess().ProcessName.Replace(".vshost", "");
            this.Channel = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            while (true)
            {
                try
                {
                    this.Channel.Connect(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 55000));
                    break;
                }
                catch { Thread.Sleep(100); }
            }
            this.Channel.Send(this.s2b("I:{0}{1}", pname, System.Diagnostics.Process.GetProcessesByName(pname).Length + 1));
            o("Connected to server");
        }
        private void Iterate()
        {
            while (true)
            {
                byte[] status = new byte[26];
                int rcv = this.Channel.Receive(status);

                this.Channel.Send(this.s2b("4"));
                Console.WriteLine("Moved to right");
                Console.WriteLine(this.b2s(status));
                Thread.Sleep(1000);
            }
        }
    }
}
