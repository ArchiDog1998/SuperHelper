using Grasshopper.Kernel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using Grasshopper.GUI;
using System.IO;
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
using System.Windows.Shapes;
using Grasshopper;
using System.Windows.Interop;

namespace SuperHelper
{


    /// <summary>
    /// Interaction logic for SuperHelperWindow.xaml
    /// </summary>
    public partial class SuperHelperWindow : Window
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



        public SuperHelperWindow()
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

            new WindowInteropHelper(this).Owner = Instances.DocumentEditor.Handle;
        }

        protected override void OnClosed(EventArgs e)
        {
            HighLightConduit.HighLightObject = null;

            if(Rhino.RhinoDoc.ActiveDoc != null)
            {
                foreach (var view in Rhino.RhinoDoc.ActiveDoc.Views)
                {
                    view.Redraw();
                }
            }

            MenuReplacer._window = new SuperHelperWindow();
            MenuReplacer._windowShown = false;
            base.OnClosed(e);
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

        private void myWeb_Navigated(object sender, System.Windows.Navigation.NavigationEventArgs e)
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
    }

    [ValueConversion(typeof(GH_DocumentObject), typeof(string))]
    public class TypeInfoConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return null;
            GH_DocumentObject gH_DocumentObject = (GH_DocumentObject)value;
            if (gH_DocumentObject == null) return null;

            return value.GetType().FullName + "\n \n" +
                   "Guid: " + gH_DocumentObject.ComponentGuid + "\n \n" +
                   string.Join(",\n", value.GetType().GetInterfaces().Select((t) => t.Name)) + "\n \n" +
                   FindFathers(value.GetType());
        }

        private string FindFathers(Type type)
        {
            List<string> typeFull = new List<string>();
            Type rightType = type;
            while (rightType != typeof(object))
            {
                typeFull.Add(GetTypeName(rightType));
                rightType = rightType.BaseType;
            }

            typeFull.Reverse();
            string full = typeFull[0];
            for (int i = 1; i < typeFull.Count; i++)
            {
                string space = "";
                for (int j = 0; j < i; j++)
                {
                    space += "--";
                }
                full += "\n" + space + typeFull[i];
            }
            return full;
        }

        private string GetTypeName(Type type)
        {
            string res = type.Name;
            if (!type.IsGenericType) return res;

            res = res.Split('`')[0];
            res += "<" + string.Join(",", type.GetGenericArguments().Select((t) => GetTypeName(t)).ToArray()) + ">";

            return res;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    [ValueConversion(typeof(GH_DocumentObject), typeof(string))]
    public class TypeLoactionConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return "";

            GH_AssemblyInfo info = null;
            Assembly typeAssembly = value.GetType().Assembly;
            foreach (GH_AssemblyInfo lib in Grasshopper.Instances.ComponentServer.Libraries)
            {
                if (lib.Assembly == typeAssembly)
                {
                    info = lib;
                    break;
                }
            }
            if (info == null) return "";
            return info.Location;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
