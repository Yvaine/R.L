using System;
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
        /// <summary>
        /// The socket channel for communication with server
        /// </summary>
        private Socket Channel { get; set; }
        /// <summary>
        /// Shortcut for `Console.WriteLine()`
        /// </summary>
        private void o(string format, params object[] arg) { Console.WriteLine(format, arg); }
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
        public static void Main()
        {
            // init title
            Console.Title = "Player";
            // bootstrap and initiate the game-play ops
            new Player()
                .Bootstrap()
                .Init()
                .Gameplay();
            // wait for any key
            Console.ReadKey(true);
        }
        /// <summary>
        /// Bootstrap the app.
        /// </summary>
        private Player Bootstrap()
        {
            var pwd = System.IO.Path.GetDirectoryName(System.Windows.Forms.Application.ExecutablePath);
            foreach (var p in new string[] { "ps", "pm" })
            {
                foreach (var k in System.Diagnostics.Process.GetProcessesByName(p)) k.Kill();
                Process.Start(String.Format("{0}/{1}.exe", pwd, p));
            }
            // update the console's title
            Console.Title += (Process.GetProcessesByName(Process.GetCurrentProcess().ProcessName.Replace(".vshost", "")).Length).ToString();
            // if in debug mode??
            if (Process.GetCurrentProcess().ProcessName.Contains(".vshost"))
            {
                Process.Start(System.Windows.Forms.Application.ExecutablePath);
            }
            return this;
        }
        /// <summary>
        /// Initializes the player
        /// </summary>
        private Player Init()
        {
            // open-up a socket
            this.Channel = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            o("Waiting for server ....");
            while (true)
            {
                try
                {
                    this.Channel.Connect(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 55000));
                    break;
                }
                catch { Thread.Sleep(100); }
            }
            // send player's name
            this.Channel.Send(this.s2b("I:{0}", Console.Title));
            o("Connected to server ....");
            // init the learner
            Learner.Init();
            return this;
        }
        /// <summary>
        /// Initiates the game-play ops
        /// </summary>
        private void Gameplay()
        {
            GameState gs = null;
            do
            {
                try
                {
                    byte[] status = new byte[26];
                    int rcv = this.Channel.Receive(status);
                    if (rcv == 0) { o("!!Connection Dropped!!"); return; }
                    gs = new GameState(status);
                    Direction direction = Learner.ChooseAction(gs);
                    this.Channel.Send(this.s2b(((int)direction).ToString()));
                    o("Moved: {0}", direction.ToString());
                }
                catch (Exception e) { System.IO.File.AppendAllText("error.log", e.ToString()); o("Exception\n[ ABORT ]"); return; }
            } while (gs != null && gs.Game_State != GameState.State.GAME_FINISHED);
        }
    }
}