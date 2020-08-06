using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace Stroke.Configure
{
    public partial class GestureCanvas : Form
    {
        private Gesture Gesture;

        public GestureCanvas(Gesture gesture)
        {
            InitializeComponent();
            this.Bounds = SystemInformation.VirtualScreen;
            Gesture = gesture;
            draw = new Draw(this.Handle, API.CreatePen(API.PenStyle.PS_SOLID, Settings.Pen.Thickness, new API.COLORREF(Settings.Pen.Color.R, Settings.Pen.Color.G, Settings.Pen.Color.B)));
            this.Load += GestureCanvas_Load;
            this.FormClosing += GestureCanvas_FormClosing;
        }

        private void GestureCanvas_Load(object sender, EventArgs e)
        {
            MouseHook.MouseAction += MouseHook_MouseAction;
        }
        private void GestureCanvas_FormClosing(object sender, FormClosingEventArgs e)
        {
            MouseHook.MouseAction -= MouseHook_MouseAction;
        }

        private readonly Draw draw;
        private bool drawing = false;
        private bool stroked = false;
        private Point lastPoint = new Point(0, 0);
        private List<Point> drwaingPoints = new List<Point>();

        private bool MouseHook_MouseAction(object sender, MouseHook.MouseActionArgs e)
        {
            if (e.MouseButton == Settings.StrokeButton)
            {
                if (e.MouseButtonState == MouseHook.MouseButtonStates.Down)
                {
                    drawing = true;
                    Cursor.Hide();
                    lastPoint = e.Location;
                    drwaingPoints.Add(e.Location);
                    return true;
                }
                else if (e.MouseButtonState == MouseHook.MouseButtonStates.Up)
                {
                    drawing = false;
                    Cursor.Show();

                    if (stroked)
                    {
                        if (Gesture.Vectors == null)
                        {
                            Gesture.GenerateVectors(drwaingPoints);
                        }
                        else
                        {
                            Gesture gesture = new Gesture("", drwaingPoints);
                            for (int i = 0; i < 128; i++)
                            {
                                double x = (Gesture.Vectors[i].X * 0.9 + gesture.Vectors[i].X * 0.1);
                                double y = (Gesture.Vectors[i].Y * 0.9 + gesture.Vectors[i].Y * 0.1);

                                double distance = Math.Sqrt(Math.Pow(x, 2) + Math.Pow(y, 2));
                                Gesture.Vectors[i].X = (sbyte)(x * 127 / distance);
                                Gesture.Vectors[i].Y = (sbyte)(y * 127 / distance);
                            }
                        }
                        stroked = false;
                    }

                    drwaingPoints.Clear();
                    this.Close();
                    return true;
                }
            }

            if (e.MouseButtonState == MouseHook.MouseButtonStates.Move && drawing)
            {
                stroked = true;
                if (Settings.Pen.Opacity != 0 && Settings.Pen.Thickness != 0)
                {
                    draw.DrawPath(lastPoint, e.Location);
                }
                lastPoint = e.Location;
                drwaingPoints.Add(e.Location);
            }

            return false;
        }
    }
}
