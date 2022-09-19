using GH_IO.Serialization;
using Grasshopper;
using Grasshopper.GUI;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Parameters.Hints;
using Rhino.Display;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml.Linq;

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

            oldUrl.ScriptErrorsSuppressed = true;
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
            MenuReplacer.SaveUrlDictToJson();
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

        public void UpdateViewPortHost()
        {
            foreach (var child in MotherGrid.Children)
            {
                if(child is RhinoViewportHost host)
                {
                    MotherGrid.Children.Remove(host);
                    break;
                }
            }
            MotherGrid.Children.Add(new RhinoViewportHost());
        }

        private void SwitchButton_Click(object sender, RoutedEventArgs e)
        {
            if (!(sender is Button b)) return;
            if (!(this.DataContext is GH_DocumentObject obj)) return;

            //var url = @"https://github.com/ArchiDog1998/Grasshopper-Player-Hops-External-GH-Documents/raw/master/Circles%20On%20Sphere.gh";

            if (ExampleList.Visibility == Visibility.Visible)
            {
                ExampleList.Visibility = Visibility.Collapsed;
                ExampleData.Visibility = Visibility.Visible;
                b.Content = "Save and Use";
                ExampleData.ItemsSource = ExampleList.ItemsSource;
            }
            else
            {
                ExampleList.Visibility = Visibility.Visible;
                ExampleData.Visibility = Visibility.Collapsed;
                b.Content = "Edit";
                
                if(ExampleData.ItemsSource is ObservableCollection<HelpExample> set)
                {
                    var newSet = new ObservableCollection<HelpExample>(set.Where(s => s.IsValid));
                    ExampleList.ItemsSource = newSet;
                    MenuReplacer.UrlExDict[obj.ComponentGuid.ToString()] = newSet.Select(s => s.Path).ToArray();
                    MenuReplacer.SaveUrlExToJson();
                }
            }
        }

        //private async void ListBoxItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        //{
        //    if (!(sender is HelpExample ex)) return;
        //    await ex.PasteFromArchive();
        //}

        private async void ListBoxItem_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (!(sender is ListViewItem it)) return;
            if (!(it.DataContext is HelpExample ex)) return;

            switch (e.ChangedButton)
            {
                case MouseButton.Left:
                    await ex.PasteFromArchive();
                    break;
                case MouseButton.Right:
                    await ex.CopyFromArchive();
                    break;
            }
        }

        private void RemoveButton_Click(object sender, RoutedEventArgs e)
        {
            if (!(sender is Button it)) return;
            if (!(it.DataContext is HelpExample ex)) return;
            if(!(ExampleData.ItemsSource is ObservableCollection<HelpExample> sets)) return;

            sets.Remove(ex);
        }
    }
}
