using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Player.RL;

namespace Player
{
    class Player
    {
        private Socket Channel { get; set; }
        /// <summary>
        /// Shortcut for `Console.WriteLine()`
        /// </summary>
        private static void o(string format, params object[] arg) { Console.WriteLine(format, arg); }
        /// <summary>
        /// Converts string to byte array
        /// </summary>
        /// <param name="__string">The formatted string</param>
        /// <param name="arg">The arguments of formatted string</param>
        /// <returns>The byte array</returns>
        private byte[] s2b(string __string, params object[] arg) { return Encoding.ASCII.GetBytes(String.Format(__string, arg)); }
        /// <summary>
        /// Converts bytes to string
        /// </summary>
        /// <param name="__bytes">The bytes</param>
        /// <returns>The string</returns>
        private string b2s(byte[] __bytes) { return Encoding.ASCII.GetString(__bytes); }
        /// <summary>
        /// Main entery point of application
        /// </summary>
        static void Main()
        {
            Console.Title = "Player";
            var player = new Player();
            Player.Bootstrap();
            player.Init();
            player.Iterate();
            Console.ReadKey(true);
        }
        /// <summary>
        /// Bootstrap the app
        /// </summary>
        private static void Bootstrap()
        {
            var pwd = System.IO.Path.GetDirectoryName(System.Windows.Forms.Application.ExecutablePath);
            foreach (var p in new string[] { "ps", "pm" })
            {
                foreach (var k in System.Diagnostics.Process.GetProcessesByName(p)) k.Kill();
                Process.Start(String.Format("{0}/{1}.exe", pwd, p));
            }
            if (Process.GetCurrentProcess().ProcessName.Contains(".vshost"))
                Process.Start(System.Windows.Forms.Application.ExecutablePath);
        }
        /// <summary>
        /// Initializes the app.
        /// </summary>
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
            Learner.Init();
        }
        /// <summary>
        /// Plays with iteration
        /// </summary>
        private void Iterate()
        {
            while (true)
            {
                byte[] status = new byte[26];
                int rcv = this.Channel.Receive(status);
                Learner.Direction direction = Learner.ChooseAction(new GameStatus(status));
                this.Channel.Send(this.s2b(((int)direction).ToString()));
                o("Moved: {0}", direction.ToString());
                o(this.b2s(status));
                Thread.Sleep(1000);
            }
        }
    }
}