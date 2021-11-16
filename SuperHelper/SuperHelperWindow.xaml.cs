using Grasshopper.Kernel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
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

namespace SuperHelper
{


    /// <summary>
    /// Interaction logic for SuperHelperWindow.xaml
    /// </summary>
    public partial class SuperHelperWindow : Window
    {
        public SuperHelperWindow()
        {
            InitializeComponent();
        }

        protected override void OnClosed(EventArgs e)
        {
            MenuReplacer._window = new SuperHelperWindow();
            MenuReplacer._windowShown = false;
            base.OnClosed(e);
        }

        private void SaveClick(object sender, RoutedEventArgs e)
        {
            MenuReplacer.UrlDict[((GH_DocumentObject)DataContext).ComponentGuid.ToString()] = UrlTextBox.Text;
            myWeb.Source = new Uri(UrlTextBox.Text);
            MenuReplacer.SaveToJson();
        }

        private void OpenFileClick(object sender, RoutedEventArgs e)
        {
            Process p = new Process();
            p.StartInfo.FileName = "explorer.exe";
            p.StartInfo.Arguments = $" /select, {((Button)sender).Content}";
            p.Start();
        }
    }


}
