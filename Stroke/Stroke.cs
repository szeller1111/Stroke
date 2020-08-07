using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Stroke
{
    public partial class Stroke : Form
    {
        private readonly Draw draw;
        private bool stroking = false;
        private bool stroked = false;
        private bool abolish = false;
        private Point lastPoint = new Point(0, 0);
        private List<Point> drwaingPoints = new List<Point>();
        private readonly int threshold = 80;
        public static IntPtr CurrentWindow;

        private void InitializeComponent()
        {
            this.SuspendLayout();
            this.AutoScaleDimensions = new SizeF(96F, 96F);
            this.AutoScaleMode = AutoScaleMode.Dpi;
            this.BackColor = Color.Black;
            this.Bounds = SystemInformation.VirtualScreen;
            this.ControlBox = false;
            this.FormBorderStyle = FormBorderStyle.None;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "Stroke";
            this.Opacity = Settings.Pen.Opacity;
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = FormStartPosition.Manual;
            this.TransparencyKey = Color.Black;
            this.ResumeLayout(false);
        }

        public Stroke()
        {
            InitializeComponent();

            draw = new Draw(this.Handle, API.CreatePen(API.PenStyle.PS_SOLID, Settings.Pen.Thickness, new API.COLORREF(Settings.Pen.Color.R, Settings.Pen.Color.G, Settings.Pen.Color.B)));
            this.Shown += Stroke_Shown;
            this.FormClosing += Stroke_FormClosing;
            MouseHook.MouseAction += MouseHook_MouseAction;
            API.AllowSetForegroundWindow(Process.GetCurrentProcess().Id);
        }

        private void Stroke_Shown(object sender, EventArgs e)
        {
            API.SetWindowPos(this.Handle, (IntPtr)(1), 0, 0, 0, 0, (API.SWP.NOSIZE | API.SWP.NOMOVE | API.SWP.NOACTIVATE | API.SWP.HIDEWINDOW));
        }

        private void Stroke_FormClosing(object sender, FormClosingEventArgs e)
        {
            draw.Dispose();
        }

        private bool MouseHook_MouseAction(object sender, MouseHook.MouseActionArgs e)
        {
            if (e.MouseButton == Settings.StrokeButton)
            {
                if (e.MouseButtonState == MouseHook.MouseButtonStates.Down)
                {
                    CurrentWindow = API.GetAncestor(API.WindowFromPoint(new API.POINT(e.Location.X, e.Location.Y)), API.GetAncestorFlags.GA_ROOT);
                    stroking = true;
                    Cursor.Hide();
                    lastPoint = e.Location;
                    drwaingPoints.Add(e.Location);
                    return true;
                }
                else if (e.MouseButtonState == MouseHook.MouseButtonStates.Up)
                {
                    if (abolish)
                    {
                        stroking = false;
                        abolish = false;
                        return true;
                    }

                    stroking = false;
                    Cursor.Show();
                    this.Refresh();
                    API.SetWindowPos(this.Handle, (IntPtr)(1), 0, 0, 0, 0, (API.SWP.NOSIZE | API.SWP.NOMOVE | API.SWP.NOACTIVATE | API.SWP.HIDEWINDOW));
                    if (stroked)
                    {
                        Gesture gesture = new Gesture("", drwaingPoints);
                        int similarity = 0, index = 0;
                        for (int i = 0; i < Settings.Gestures.Count; i++)
                        {
                            if (Settings.Gestures[i].Vectors == null)
                            {
                                continue;
                            }

                            int temp = gesture.Similarity(Settings.Gestures[i]);
                            if (temp > similarity)
                            {
                                similarity = temp;
                                index = i;
                            }
                        }

                        if (similarity > threshold)
                        {
                            API.GetWindowThreadProcessId(CurrentWindow, out uint pid);
                            IntPtr hProcess = API.OpenProcess(API.AccessRights.PROCESS_QUERY_INFORMATION | API.AccessRights.PROCESS_VM_READ, false, (int)pid);
                            StringBuilder path = new StringBuilder(256);
                            API.GetModuleFileNameEx(hProcess, IntPtr.Zero, path, (uint)path.Capacity);

                            for (int i = Settings.ActionPackages.Count - 1; i > -1; i--)
                            {
                                bool match = false;
                                foreach (string item in Settings.ActionPackages[i].Code.Split('\n'))
                                {
                                    if (item != "" && Regex.IsMatch(path.ToString(), item))
                                    {
                                        match = true;
                                        break;
                                    }
                                }

                                if (match)
                                {
                                    foreach (Action action in Settings.ActionPackages[i].Actions)
                                    {
                                        if (action.Gesture == Settings.Gestures[index].Name)
                                        {
                                            var task = Task.Run(() =>
                                            {
                                                Script.RunScript($"{Settings.ActionPackages[i].Name}.{action.Name}");
                                            });
                                            stroked = false;
                                            drwaingPoints.Clear();
                                            return true;
                                        }
                                    }
                                }
                            }
                        }

                        stroked = false;
                    }
                    else
                    {
                        ClickStrokeButton();
                    }

                    drwaingPoints.Clear();
                    return true;
                }
            }
            else if (stroking)
            {
                string gesture = "#";

                if (e.MouseButtonState == MouseHook.MouseButtonStates.Down)
                {
                    return true;
                }
                else if (e.MouseButtonState == MouseHook.MouseButtonStates.Up)
                {
                    switch (e.MouseButton)
                    {
                        case MouseButtons.Middle:
                            gesture = gesture + (int)(SpecialGesture.MiddleClick);
                            break;
                        case MouseButtons.Left:
                            gesture = gesture + (int)(SpecialGesture.LeftClick);
                            break;
                        case MouseButtons.Right:
                            gesture = gesture + (int)(SpecialGesture.RightClick);
                            break;
                        case MouseButtons.XButton1:
                            gesture = gesture + (int)(SpecialGesture.X1Click);
                            break;
                        case MouseButtons.XButton2:
                            gesture = gesture + (int)(SpecialGesture.X2Click);
                            break;
                    }
                }
                else if (e.MouseButtonState == MouseHook.MouseButtonStates.Wheel)
                {
                    if (e.WheelDelta > 0)
                    {
                        gesture = gesture + (int)(SpecialGesture.WheelUp);
                    }
                    else
                    {
                        gesture = gesture + (int)(SpecialGesture.WheelDown);
                    }
                }

                if (gesture != "#")
                {
                    API.GetWindowThreadProcessId(CurrentWindow, out uint pid);
                    IntPtr hProcess = API.OpenProcess(API.AccessRights.PROCESS_QUERY_INFORMATION | API.AccessRights.PROCESS_VM_READ, false, (int)pid);
                    StringBuilder path = new StringBuilder(256);
                    API.GetModuleFileNameEx(hProcess, IntPtr.Zero, path, (uint)path.Capacity);

                    for (int i = Settings.ActionPackages.Count - 1; i > -1; i--)
                    {
                        bool match = false;
                        foreach (string item in Settings.ActionPackages[i].Code.Split('\n'))
                        {
                            if (item != "" && Regex.IsMatch(path.ToString(), item))
                            {
                                match = true;
                                break;
                            }
                        }

                        if (match)
                        {
                            foreach (Action action in Settings.ActionPackages[i].Actions)
                            {
                                if (action.Gesture == gesture)
                                {
                                    stroked = false;
                                    drwaingPoints.Clear();
                                    Cursor.Show();
                                    this.Refresh();
                                    API.SetWindowPos(this.Handle, (IntPtr)(1), 0, 0, 0, 0, (API.SWP.NOSIZE | API.SWP.NOMOVE | API.SWP.NOACTIVATE | API.SWP.HIDEWINDOW));
                                    var task = Task.Run(() =>
                                    {
                                        Script.RunScript($"{Settings.ActionPackages[i].Name}.{action.Name}");
                                    });
                                    abolish = true;
                                    return true;
                                }
                            }
                        }
                    }

                }
            }

            if (e.MouseButtonState == MouseHook.MouseButtonStates.Move && stroking && !abolish)
            {
                stroked = true;
                if (Settings.Pen.Opacity != 0 && Settings.Pen.Thickness != 0)
                {
                    API.SetWindowPos(this.Handle, (IntPtr)(-1), 0, 0, 0, 0, (API.SWP.NOSIZE | API.SWP.NOMOVE | API.SWP.NOACTIVATE | API.SWP.SHOWWINDOW));
                    draw.DrawPath(lastPoint, e.Location);
                }
                lastPoint = e.Location;
                drwaingPoints.Add(e.Location);
            }

            return false;
        }

        private static void ClickStrokeButton()
        {
            var task = Task.Run(() =>
            {
                MouseHook.StopHook();
                API.INPUT imput = new API.INPUT();
                imput.type = API.INPUTTYPE.MOUSE;
                imput.mi.dx = 0;
                imput.mi.dy = 0;
                imput.mi.mouseData = 0;
                imput.mi.time = 0u;
                imput.mi.dwExtraInfo = (UIntPtr)0uL;

                switch (Settings.StrokeButton)
                {
                    case MouseButtons.Left:
                        if (API.GetSystemMetrics(API.SystemMetrics.SM_SWAPBUTTON) == 0)
                        {
                            imput.mi.dwFlags = (API.MOUSEEVENTF.LEFTDOWN | API.MOUSEEVENTF.LEFTUP);
                        }
                        else
                        {
                            imput.mi.dwFlags = (API.MOUSEEVENTF.RIGHTDOWN | API.MOUSEEVENTF.RIGHTUP);
                        }
                        break;
                    case MouseButtons.Right:
                        if (API.GetSystemMetrics(API.SystemMetrics.SM_SWAPBUTTON) == 0)
                        {
                            imput.mi.dwFlags = (API.MOUSEEVENTF.RIGHTDOWN | API.MOUSEEVENTF.RIGHTUP);
                        }
                        else
                        {
                            imput.mi.dwFlags = (API.MOUSEEVENTF.LEFTDOWN | API.MOUSEEVENTF.LEFTUP);
                        }
                        break;
                    case MouseButtons.Middle:
                        imput.mi.dwFlags = (API.MOUSEEVENTF.MIDDLEDOWN | API.MOUSEEVENTF.MIDDLEUP);
                        break;
                    case MouseButtons.XButton1:
                        imput.mi.dwFlags = (API.MOUSEEVENTF.XDOWN | API.MOUSEEVENTF.XUP);
                        imput.mi.mouseData = 0x0001;
                        break;
                    case MouseButtons.XButton2:
                        imput.mi.dwFlags = (API.MOUSEEVENTF.XDOWN | API.MOUSEEVENTF.XUP);
                        imput.mi.mouseData = 0x0002;
                        break;
                }

                API.SendInput(1u, ref imput, Marshal.SizeOf(typeof(API.INPUT)));
            });
            task.GetAwaiter().OnCompleted(() =>
            {
                MouseHook.StartHook();
            });
        }

    }
}
