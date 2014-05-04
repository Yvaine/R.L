using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bootstrapper
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                Console.Title = "Bootstrapper....";
                string[] exe = { System.IO.Path.GetTempPath() + "/ProjMonitor.exe", System.IO.Path.GetTempPath() + "/ProjServer.exe" };
                foreach (var p in System.Diagnostics.Process.GetProcessesByName("ProjMonitor")) p.Kill();
                foreach (var p in System.Diagnostics.Process.GetProcessesByName("ProjServer")) p.Kill();
                System.IO.File.WriteAllBytes(exe[0], Properties.Resources.ProjMonitor);
                System.IO.File.WriteAllBytes(exe[1], Properties.Resources.ProjServer);
                foreach (var p in exe) System.Diagnostics.Process.Start(p);
            }
            catch (Exception e) { Console.WriteLine(e.ToString()); Console.ReadKey(true); }
            System.Diagnostics.Process.GetCurrentProcess().Kill();
        }
    }
}
