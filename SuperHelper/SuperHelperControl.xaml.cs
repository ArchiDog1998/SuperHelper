using Grasshopper;
using Grasshopper.GUI;
using Grasshopper.Kernel;
using Rhino.Display;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SuperHelper
{
    /// <summary>
    /// Interaction logic for SuperHelperControl.xaml
    /// </summary>
    public partial class SuperHelperControl : UserControl
    {
        private System.Drawing.Color _wireColorDefault = System.Drawing.Color.DarkBlue;
        public System.Drawing.Color WireColor
        {
            get => Instances.Settings.GetValue(nameof(WireColor), _wireColorDefault);
            set
            {
                Instances.Settings.SetValue(nameof(WireColor), value);
                foreach (var view in Rhino.RhinoDoc.ActiveDoc.Views)
                {
                    view.Redraw();
                }
            }
        }

        private System.Drawing.Color _materialColorDefault = System.Drawing.Color.DarkBlue;
        public System.Drawing.Color MaterialColor
        {
            get => Instances.Settings.GetValue(nameof(MaterialColor), _materialColorDefault);
            set
            {
                Instances.Settings.SetValue(nameof(MaterialColor), value);
                foreach (var view in Rhino.RhinoDoc.ActiveDoc.Views)
                {
                    view.Redraw();
                }
            }
        }

        private bool _autoTargetDefault = true;

        public bool AutoTarget
        {
            get => Instances.Settings.GetValue(nameof(AutoTarget), _autoTargetDefault);
            set => Instances.Settings.SetValue(nameof(AutoTarget), value);
        }

        private int _wireWidthDefault = 1;

        public int DisplayWireWidth
        {
            get => Instances.Settings.GetValue(nameof(DisplayWireWidth), _wireWidthDefault);
            set
            {
                Instances.Settings.SetValue(nameof(DisplayWireWidth), value);
                foreach (var view in Rhino.RhinoDoc.ActiveDoc.Views)
                {
                    view.Redraw();
                }
            }
        }


        public SuperHelperControl()
        {
            InitializeComponent();

            GH_ColourPicker wirePicker = new GH_ColourPicker();
            wirePicker.Colour = WireColor;
            wirePicker.ColourChanged += (sender, e) =>
            {
                WireColor = e.Colour;
            };
            LeftColor.Child = wirePicker;

            GH_ColourPicker materialPicker = new GH_ColourPicker();
            materialPicker.Colour = MaterialColor;
            materialPicker.ColourChanged += (sender, e) =>
            {
                MaterialColor = e.Colour;
            };
            RightColor.Child = materialPicker;
        }

        public void OnClosed()
        {
            HighLightConduit.HighLightObject = null;

            if (Rhino.RhinoDoc.ActiveDoc != null)
            {
                foreach (var view in Rhino.RhinoDoc.ActiveDoc.Views)
                {
                    view.Redraw();
                }
            }
        }

        private void SaveClick(object sender, RoutedEventArgs e)
        {
            MenuReplacer.UrlDict[((GH_DocumentObject)DataContext).ComponentGuid.ToString()] = UrlTextBox.Text;
            MenuReplacer.SaveToJson();
        }

        private void GoClick(object sender, RoutedEventArgs e)
        {
            myWeb.Source = new Uri(UrlTextBox.Text);
        }

        private void OpenFileClick(object sender, RoutedEventArgs e)
        {
            Process p = new Process();
            p.StartInfo.FileName = "explorer.exe";
            p.StartInfo.Arguments = $" /select, {((Button)sender).Content}";
            p.Start();
        }

        private void LeftResetClick(object sender, RoutedEventArgs e)
        {
            ((GH_ColourPicker)LeftColor.Child).Colour = _wireColorDefault;
            WireColor = _wireColorDefault;
        }

        private void RightResetClick(object sender, RoutedEventArgs e)
        {
            ((GH_ColourPicker)RightColor.Child).Colour = _materialColorDefault;
            MaterialColor = _materialColorDefault;
        }

        private void myWeb_Navigated(object sender, NavigationEventArgs e)
        {
            SuppressScriptErrors((WebBrowser)sender, true);
        }

        private void SuppressScriptErrors(WebBrowser wb, bool Hide)
        {
            FieldInfo fiComWebBrowser = typeof(WebBrowser).GetField("_axIWebBrowser2", BindingFlags.Instance | BindingFlags.NonPublic);
            if (fiComWebBrowser == null) return;

            object objComWebBrowser = fiComWebBrowser.GetValue(wb);
            if (objComWebBrowser == null) return;

            objComWebBrowser.GetType().InvokeMember("Silent", BindingFlags.SetProperty, null, objComWebBrowser, new object[] { Hide });
        }

        private void SwitchSide_Click(object sender, RoutedEventArgs e)
        {
            SuperHelperAssemblyPriority.SwitchSide();
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            SuperHelperAssemblyPriority.Hide();
        }
    }
}
