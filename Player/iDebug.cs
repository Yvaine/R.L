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
        static bool AlreadyCreated = false;
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
        }
        /// <summary>
        /// CLear button event handler
        /// </summary>
        private void btn_clear_Click(object sender, EventArgs e) { this.richTextBox1.Clear(); this.updateUI(); }
        protected void updateUI() { Application.DoEvents(); if (!this.Visible) this.Show(); }
        /// <summary>
        /// Add a line in format
        /// </summary>
        public void AddLine(string format, params object[] arg) { this.richTextBox1.AppendText(String.Format(format, arg) + "\r\n"); this.updateUI(); }
        /// <summary>
        /// Add a text in format
        /// </summary>
        public void AddText(string format, params object[] arg) { this.richTextBox1.AppendText(String.Format(format, arg)); this.updateUI(); }
        /// <summary>
        /// Clears the text
        /// </summary>
        public void ClearText() { this.btn_clear_Click(new object(), new EventArgs()); }
    }
}