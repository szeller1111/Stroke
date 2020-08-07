using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace Stroke.Configure
{
    public partial class Configure : Form
    {
        Action CurrentAction;
        ActionPackage CurrentActionPackage;
        bool spy = false;

        public Configure()
        {
            try
            {
                Settings.ReadSettings();
            }
            catch
            {
                LoadDefaultSetting();
            }

            InitializeComponent();

            PenConfigure = new PenConfigure();
            GestureConfigure = new GestureConfigure();
            ContextMenuStripActionPackage = new ContextMenuStrip();
            ContextMenuStripAction = new ContextMenuStrip();
            ToolStripMenuItem ToolStripMenuItemAddActionPackage = new ToolStripMenuItem();
            ToolStripMenuItem ToolStripMenuItemRemoveActionPackage = new ToolStripMenuItem();
            ToolStripMenuItemAddActionPackage.Text = "添加 [动作包]";
            ToolStripMenuItemRemoveActionPackage.Text = "删除 [动作包]";
            ContextMenuStripActionPackage.Items.Add(ToolStripMenuItemAddActionPackage);
            ContextMenuStripActionPackage.Items.Add(ToolStripMenuItemRemoveActionPackage);
            ToolStripMenuItemAddActionPackage.Click += ToolStripMenuItemAddActionPackage_Click;
            ToolStripMenuItemRemoveActionPackage.Click += ToolStripMenuItemRemoveActionPackage_Click;
            ToolStripMenuItem ToolStripMenuItemAddAction = new ToolStripMenuItem();
            ToolStripMenuItem ToolStripMenuItemRemoveAction = new ToolStripMenuItem();
            ToolStripMenuItemAddAction.Text = "添加 [动作]";
            ToolStripMenuItemRemoveAction.Text = "删除 [动作]";
            ContextMenuStripAction.Items.Add(ToolStripMenuItemAddAction);
            ContextMenuStripAction.Items.Add(ToolStripMenuItemRemoveAction);
            ToolStripMenuItemAddAction.Click += ToolStripMenuItemAddAction_Click;
            ToolStripMenuItemRemoveAction.Click += ToolStripMenuItemRemoveAction_Click;
            comboBoxGesture.DisplayMember = "Value";
            comboBoxGesture.ValueMember = "Key";
            MouseHook.MouseAction += MouseHook_MouseAction;
        }

        private bool MouseHook_MouseAction(object sender, MouseHook.MouseActionArgs e)
        {
            if (spy)
            {
                try
                {
                    if (e.MouseButtonState == MouseHook.MouseButtonStates.Up)
                    {
                        IntPtr hwnd = API.WindowFromPoint(new API.POINT(e.Location.X, e.Location.Y));
                        API.GetWindowThreadProcessId(hwnd, out uint pid);
                        IntPtr hProcess = API.OpenProcess(API.AccessRights.PROCESS_QUERY_INFORMATION | API.AccessRights.PROCESS_VM_READ, false, (int)pid);
                        StringBuilder path = new StringBuilder(256);
                        API.GetModuleFileNameEx(hProcess, IntPtr.Zero, path, (uint)path.Capacity);
                        richTextBoxCode.Text = richTextBoxCode.Text.TrimEnd('\n') + '\n' + Regex.Replace(path.ToString(), @"([\\\.\{\}\[\]\(\)\^\$\|\*\+\?])", @"\$1");
                        spy = false;
                        Cursor.Current = Cursors.Default;
                    }
                }
                catch (Exception exception)
                {

                    MessageBox.Show(exception.Message);
                }
            }

            return false;
        }

        private void LoadDefaultSetting()
        {
            Settings.StrokeButton = MouseButtons.Right;

            Settings.Pen = new Pen();
            Settings.Pen.Color = Color.FromArgb(31, 127, 255);
            Settings.Pen.Opacity = 0.8;
            Settings.Pen.Thickness = 4;

            Settings.Gestures = new List<Gesture>();
            Settings.Gestures.Add(new Gesture("↑", new List<Point> { new Point(0, 0), new Point(0, -1) }));
            Settings.Gestures.Add(new Gesture("↓", new List<Point> { new Point(0, 0), new Point(0, 1) }));
            Settings.Gestures.Add(new Gesture("←", new List<Point> { new Point(0, 0), new Point(-1, 0) }));
            Settings.Gestures.Add(new Gesture("→", new List<Point> { new Point(0, 0), new Point(1, 0) }));
            Settings.Gestures.Add(new Gesture("↖", new List<Point> { new Point(0, 0), new Point(-1, -1) }));
            Settings.Gestures.Add(new Gesture("↗", new List<Point> { new Point(0, 0), new Point(1, -1) }));
            Settings.Gestures.Add(new Gesture("↙", new List<Point> { new Point(0, 0), new Point(-1, 1) }));
            Settings.Gestures.Add(new Gesture("↘", new List<Point> { new Point(0, 0), new Point(1, 1) }));
            Settings.Gestures.Add(new Gesture("↑↓", new List<Point> { new Point(0, 0), new Point(0, -1), new Point(0, 0) }));
            Settings.Gestures.Add(new Gesture("↑←", new List<Point> { new Point(0, 0), new Point(0, -1), new Point(-1, -1) }));
            Settings.Gestures.Add(new Gesture("↑→", new List<Point> { new Point(0, 0), new Point(0, -1), new Point(1, -1) }));
            Settings.Gestures.Add(new Gesture("↓↑", new List<Point> { new Point(0, 0), new Point(0, 1), new Point(0, 0) }));
            Settings.Gestures.Add(new Gesture("↓←", new List<Point> { new Point(0, 0), new Point(0, 1), new Point(-1, 1) }));
            Settings.Gestures.Add(new Gesture("↓→", new List<Point> { new Point(0, 0), new Point(0, 1), new Point(1, 1) }));
            Settings.Gestures.Add(new Gesture("→←", new List<Point> { new Point(0, 0), new Point(1, 0), new Point(0, 0) }));
            Settings.Gestures.Add(new Gesture("→↑", new List<Point> { new Point(0, 0), new Point(1, 0), new Point(1, -1) }));
            Settings.Gestures.Add(new Gesture("→↓", new List<Point> { new Point(0, 0), new Point(1, 0), new Point(1, 1) }));
            Settings.Gestures.Add(new Gesture("←→", new List<Point> { new Point(0, 0), new Point(-1, 0), new Point(0, 0) }));
            Settings.Gestures.Add(new Gesture("←↑", new List<Point> { new Point(0, 0), new Point(-1, 0), new Point(-1, -1) }));
            Settings.Gestures.Add(new Gesture("←↓", new List<Point> { new Point(0, 0), new Point(-1, 0), new Point(-1, 1) }));

            Settings.ActionPackages = new List<ActionPackage>();
            Settings.ActionPackages.Add(new ActionPackage("Global", ".*"));
        }

        private void Configure_Load(object sender, EventArgs e)
        {
            foreach (System.Diagnostics.Process process in System.Diagnostics.Process.GetProcessesByName("Stroke"))
            {
                process.Kill();
            }

            switch (Settings.StrokeButton)
            {
                case MouseButtons.Middle:
                    comboBoxMouse.SelectedIndex = 0;
                    break;
                case MouseButtons.Left:
                    comboBoxMouse.SelectedIndex = 1;
                    break;
                case MouseButtons.Right:
                    comboBoxMouse.SelectedIndex = 2;
                    break;
                case MouseButtons.XButton1:
                    comboBoxMouse.SelectedIndex = 3;
                    break;
                case MouseButtons.XButton2:
                    comboBoxMouse.SelectedIndex = 4;
                    break;
            }

            ArrayList gestures = new ArrayList();
            gestures.Add(new DictionaryEntry("", ""));
            gestures.Add(new DictionaryEntry("#0", "中键点击"));
            gestures.Add(new DictionaryEntry("#1", "左键点击"));
            gestures.Add(new DictionaryEntry("#2", "右键点击"));
            gestures.Add(new DictionaryEntry("#3", "X1键点击"));
            gestures.Add(new DictionaryEntry("#4", "X2键点击"));
            gestures.Add(new DictionaryEntry("#5", "滚轮向上"));
            gestures.Add(new DictionaryEntry("#6", "滚轮向下"));
            gestures.AddRange(Settings.Gestures.Select(g => new DictionaryEntry(g.Name, g.Name)).ToArray());
            comboBoxGesture.DataSource = gestures;

            for (int i = 0; i < Settings.ActionPackages.Count; i++)
            {
                treeViewAction.Nodes.Add(Settings.ActionPackages[i].Name);
                for (int j = 0; j < Settings.ActionPackages[i].Actions.Count; j++)
                {
                    treeViewAction.Nodes[i].Nodes.Add(Settings.ActionPackages[i].Actions[j].Name);
                }
            }

            treeViewAction.ExpandAll();
        }

        private void comboBoxMouse_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (comboBoxMouse.SelectedIndex)
            {
                case 0:
                    Settings.StrokeButton = MouseButtons.Middle;
                    break;
                case 1:
                    Settings.StrokeButton = MouseButtons.Left;
                    break;
                case 2:
                    Settings.StrokeButton = MouseButtons.Right;
                    break;
                case 3:
                    Settings.StrokeButton = MouseButtons.XButton1;
                    break;
                case 4:
                    Settings.StrokeButton = MouseButtons.XButton2;
                    break;
            }
        }

        private void buttonPen_Click(object sender, EventArgs e)
        {
            PenConfigure.ShowDialog();
        }

        private void buttonGesture_Click(object sender, EventArgs e)
        {
            if (treeViewAction.SelectedNode != null)
            {
                treeViewAction.SelectedNode = treeViewAction.SelectedNode.Parent;
            }

            GestureConfigure.ShowDialog();

            ArrayList gestures = new ArrayList();
            gestures.Add(new DictionaryEntry("", ""));
            gestures.Add(new DictionaryEntry("#0", "中键点击"));
            gestures.Add(new DictionaryEntry("#1", "左键点击"));
            gestures.Add(new DictionaryEntry("#2", "右键点击"));
            gestures.Add(new DictionaryEntry("#3", "X1键点击"));
            gestures.Add(new DictionaryEntry("#4", "X2键点击"));
            gestures.Add(new DictionaryEntry("#5", "滚轮向上"));
            gestures.Add(new DictionaryEntry("#6", "滚轮向下"));
            gestures.AddRange(Settings.Gestures.Select(g => new DictionaryEntry(g.Name, g.Name)).ToArray());
            comboBoxGesture.DataSource = gestures;
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            Settings.SaveSettings();
            Close();
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void textBoxName_TextChanged(object sender, EventArgs e)
        {
            treeViewAction.SelectedNode.Text = textBoxName.Text;
            if (CurrentAction != null)
            {
                CurrentAction.Name = textBoxName.Text;
            }
            else
            {
                CurrentActionPackage.Name = textBoxName.Text;
            }
        }

        private void comboBoxGesture_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (CurrentAction != null)
            {
                CurrentAction.Gesture = (string)comboBoxGesture.SelectedValue;
            }
        }

        private void richTextBoxCode_TextChanged(object sender, EventArgs e)
        {
            if (CurrentAction != null)
            {
                CurrentAction.Code = richTextBoxCode.Text;
            }
            else
            {
                CurrentActionPackage.Code = richTextBoxCode.Text;
            }
        }

        private void treeViewAction_MouseClick(object sender, MouseEventArgs e)
        {
            treeViewAction.SelectedNode = treeViewAction.HitTest(e.Location).Node;

            if (e.Button == MouseButtons.Right)
            {
                if (treeViewAction.SelectedNode.Level == 0)
                {
                    ContextMenuStripActionPackage.Show(Control.MousePosition);
                }
                else
                {
                    ContextMenuStripAction.Show(Control.MousePosition);
                }
            }
        }

        private void treeViewAction_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (e.Node.Level == 0)
            {
                comboBoxGesture.Visible = false;
                buttonSpy.Visible = true;
                CurrentActionPackage = Settings.ActionPackages[treeViewAction.SelectedNode.Index];
                CurrentAction = null;
                textBoxName.Text = CurrentActionPackage.Name;
                richTextBoxCode.Text = CurrentActionPackage.Code;
                if (e.Node.Nodes.Count == 0)
                {
                    CurrentActionPackage.Actions.Add(new Action("", "", ""));
                    e.Node.Nodes.Add("");
                    e.Node.Expand();
                }
            }
            else
            {
                comboBoxGesture.Visible = true;
                buttonSpy.Visible = false;
                CurrentActionPackage = Settings.ActionPackages[treeViewAction.SelectedNode.Parent.Index];
                CurrentAction = CurrentActionPackage.Actions[treeViewAction.SelectedNode.Index];
                textBoxName.Text = CurrentAction.Name;
                comboBoxGesture.SelectedValue = CurrentAction.Gesture;
                richTextBoxCode.Text = CurrentAction.Code;
            }
        }

        private void ToolStripMenuItemAddActionPackage_Click(object sender, EventArgs e)
        {
            Settings.ActionPackages.Insert(treeViewAction.SelectedNode.Index + 1, new ActionPackage("", ".*"));
            treeViewAction.SelectedNode = treeViewAction.Nodes.Insert(treeViewAction.SelectedNode.Index + 1, "");
        }

        private void ToolStripMenuItemRemoveActionPackage_Click(object sender, EventArgs e)
        {
            Settings.ActionPackages.Remove(CurrentActionPackage);
            treeViewAction.Nodes.Remove(treeViewAction.SelectedNode);

            if (treeViewAction.Nodes.Count == 0)
            {
                Settings.ActionPackages.Add(new ActionPackage("", ".*"));
                treeViewAction.SelectedNode = treeViewAction.Nodes.Add("");
            }
        }

        private void ToolStripMenuItemAddAction_Click(object sender, EventArgs e)
        {
            CurrentActionPackage.Actions.Insert(treeViewAction.SelectedNode.Index + 1, new Action("", "", ""));
            treeViewAction.SelectedNode = (treeViewAction.SelectedNode.Parent.Nodes.Insert(treeViewAction.SelectedNode.Index + 1, ""));
        }

        private void ToolStripMenuItemRemoveAction_Click(object sender, EventArgs e)
        {
            CurrentActionPackage.Actions.Remove(CurrentAction);
            treeViewAction.Nodes.Remove(treeViewAction.SelectedNode);
        }

        private void buttonSpy_MouseDown(object sender, MouseEventArgs e)
        {
            if (CurrentActionPackage != null)
            {
                spy = true;
                Cursor.Current = Cursors.Cross;
            }
        }

    }
}
