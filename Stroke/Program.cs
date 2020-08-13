using System;
using System.Windows.Forms;

namespace Stroke
{
    static class Programl
    {
        [STAThread]
        static void Main()
        {
            try
            {
                if (System.Diagnostics.Process.GetProcessesByName(System.Diagnostics.Process.GetCurrentProcess().ProcessName).Length > 1)
                {
                    Application.Exit();
                    return;
                }
                Settings.ReadSettings();
            }
            catch
            {
                Application.Exit();
                return;
            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Stroke stroke = new Stroke();
            KeyboardHook.StartHook();
            MouseHook.StartHook();
            Script.CompileScript();
            Application.Run(stroke);
            MouseHook.StopHook();
            KeyboardHook.StopHook();
        }
    }
}
