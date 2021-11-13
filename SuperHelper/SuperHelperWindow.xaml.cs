using Grasshopper.Kernel;
using System;
using System.Collections.Generic;
using System.Linq;
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
        public SuperHelperWindow(GH_DocumentObject owner, string url)
        {
            InitializeComponent();
            myWeb.Source = new Uri(url);
        }
    }
}
