using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Player
{
    public partial class iDebug : Form
    {
        private static bool AlreadyCreated = false;
        private Queue<String> q = new Queue<String>();
        public iDebug(string title = "Debug")
        {
            if (AlreadyCreated) { this.Show(); return; }
            InitializeComponent();
            this.FormClosing += new FormClosingEventHandler((object sender, FormClosingEventArgs e) =>
            {
                e.Cancel = true;
                this.Hide();
            });
            AlreadyCreated = true;
            this.Text = title;
            uint Count = 0;
            if (System.Diagnostics.Process.GetCurrentProcess().ProcessName.Contains(".vshost"))
                Count = 0;
            else Count = (uint)System.Diagnostics.Process.GetProcessesByName(System.Diagnostics.Process.GetCurrentProcess().ProcessName.Replace(".vshost", "")).Length;
            this.Location = new Point(Screen.PrimaryScreen.Bounds.X, Screen.PrimaryScreen.Bounds.Y + (int)Count * this.Height);
            new System.Threading.Thread(new System.Threading.ThreadStart(() =>
            {
                while (true)
                {
                    try
                    {
                        System.Threading.Thread.Sleep(100);
                        if (q.Count == 0) { continue; }
                        string t = "";
                        lock (this) t = q.Dequeue();
                        if (this.richTextBox1.InvokeRequired)
                            this.richTextBox1.Invoke(new Action(() =>
                            {
                                this.richTextBox1.AppendText(t);
                            }));
                        else
                            this.richTextBox1.AppendText(t);
                    }
                    catch { }
                }
            })).Start();
        }
        /// <summary>
        /// CLear button event handler
        /// </summary>
        private void btn_clear_Click(object sender, EventArgs e) { this.richTextBox1.Clear(); this.updateUI(); }
        protected void updateUI() { Application.DoEvents(); if (!this.Visible) this.Show(); }
        /// <summary>
        /// Add a line in format
        /// </summary>
        public void AddLine(string format, params object[] arg) { lock (this) q.Enqueue(String.Format(format, arg) + "\r\n"); this.updateUI(); }
        /// <summary>
        /// Add a text in format
        /// </summary>
        public void AddText(string format, params object[] arg) { lock (this) q.Enqueue(String.Format(format, arg)); this.updateUI(); }
        /// <summary>
        /// Add a line
        /// </summary>
        public void AddLine(string text) { this.AddLine("{0}", text); }
        /// <summary>
        /// Add a text
        /// </summary>
        public void AddText(string text) { this.AddText("{0}", text); }
        /// <summary>
        /// Clears the text
        /// </summary>
        public void ClearText() { this.btn_clear_Click(new object(), new EventArgs()); }
    }
}