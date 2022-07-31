using Grasshopper;
using Grasshopper.GUI;
using Grasshopper.GUI.Canvas;
using Grasshopper.Kernel;
using Rhino.Display;
using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Windows.Forms.Integration;

namespace SuperHelper
{
    public class SuperHelperInfo : GH_AssemblyInfo
    {
        public override string Name => "SuperHelper";

        //Return a 24x24 pixel bitmap to represent this GHA library.
        public override Bitmap Icon => Properties.Resources.SuperHelperIcon_24;

        //Return a short string describing the purpose of this GHA library.
        public override string Description => "Much better helper window for grasshopper";

        public override Guid Id => new Guid("A71D5B0A-9D5B-4E27-8933-BB83CB68066D");

        //Return a string identifying you or your company.
        public override string AuthorName => "秋水";

        //Return a string representing your preferred contact details.
        public override string AuthorContact => "1123993881@qq.com";

        public override string Version => "1.1.0";
    }

    public class SuperHelperAssemblyPriority : GH_AssemblyPriority
    {
        public static bool UseSuperHelperPanel
        {
            get => Instances.Settings.GetValue(nameof(UseSuperHelperPanel), true);
            set => Instances.Settings.SetValue(nameof(UseSuperHelperPanel), value);
        }

        public static int SuperHelperPanelWidth
        {
            get => Instances.Settings.GetValue(nameof(SuperHelperPanelWidth), 400);
            set => Instances.Settings.SetValue(nameof(SuperHelperPanelWidth), value);
        }

        public static bool IsSuperHelperOnRight
        {
            get => Instances.Settings.GetValue(nameof(SuperHelperPanelWidth), true);
            set
            {
                Instances.Settings.SetValue(nameof(SuperHelperPanelWidth), value);
                if (value)
                {
                    _ctrlHost.Dock = DockStyle.Right;
                    _splitter.Dock = DockStyle.Right;
                }
                else
                {
                    _ctrlHost.Dock = DockStyle.Left;
                    _splitter.Dock = DockStyle.Left;
                }

            }
        }

        private static ElementHost _ctrlHost = new ElementHost()
        {
            Dock = IsSuperHelperOnRight ? DockStyle.Right : DockStyle.Left,
            Child = MenuReplacer._control,
            Width = SuperHelperPanelWidth,
        };
        private static GH_Splitter _splitter = new GH_Splitter()
        {
            Cursor = Cursors.VSplit,
            Dock = IsSuperHelperOnRight ? DockStyle.Right : DockStyle.Left,
            Location = new Point(0, 439),
            Margin = new Padding(24),
            MaxSize = 800,
            MinSize = 50,
            Name = "Helper Splitter",
            Size = new Size(10, 2744),
        };

        public static void SwitchSide()
        {
            if (_ctrlHost == null || _splitter == null) return;

            IsSuperHelperOnRight = !IsSuperHelperOnRight;
        }

        public static void Hide()
        {
            _ctrlHost.Hide();
            _splitter.Hide();
            UseSuperHelperPanel = false;
        }

        public static void Show()
        {
            _ctrlHost.Show();
            _splitter.Show();
            UseSuperHelperPanel = true;
        }
        public override GH_LoadingInstruction PriorityLoad()
        {
            Instances.CanvasCreated += Instances_CanvasCreated;
            return GH_LoadingInstruction.Proceed;
        }

        private void Instances_CanvasCreated(GH_Canvas canvas)
        {
            Instances.CanvasCreated -= Instances_CanvasCreated;

            GH_DocumentEditor editor = Instances.DocumentEditor;
            if (editor == null)
            {
                Instances.ActiveCanvas.DocumentChanged += ActiveCanvas_DocumentChanged;
                return;
            }
            DoingSomethingFirst(editor);
        }

        private void ActiveCanvas_DocumentChanged(GH_Canvas sender, GH_CanvasDocumentChangedEventArgs e)
        {
            Instances.ActiveCanvas.DocumentChanged -= ActiveCanvas_DocumentChanged;

            GH_DocumentEditor editor = Instances.DocumentEditor;
            if (editor == null)
            {
                MessageBox.Show("SuperHelper can't find the menu!");
                return;
            }
            DoingSomethingFirst(editor);
        }

        private void DoingSomethingFirst(GH_DocumentEditor editor)
        {
            MenuReplacer.Init();

            editor.Controls[0].Controls.Add(_splitter);
            editor.Controls[0].Controls.Add(_ctrlHost);

            Instances.DocumentEditor.FormClosing += (sender, e) =>
            {
                SuperHelperPanelWidth = _ctrlHost.Width;
            };
        }
    }
}