using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using System;
using System.Collections.Generic;
using System.Globalization;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SuperHelper
{
    /// <summary>
    /// Interaction logic for ParamInfoControl.xaml
    /// </summary>
    public partial class ParamInfoControl : UserControl
    {
        public ParamInfoControl()
        {
            InitializeComponent();
        }

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            if (e.Property == DataContextProperty)
            {
                ShowButton.Tag = false;
                ShowButton.Visibility = Visibility.Visible;
            }
            base.OnPropertyChanged(e);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            ShowButton.Tag = true;
            ShowButton.Visibility = Visibility.Collapsed;
        }
    }

    [ValueConversion(typeof(IGH_Param), typeof(IGH_Structure))]
    public class ParamDatasConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return null;
            if (value is IGH_Param)
                return ((IGH_Param)value).VolatileData;
            else return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    [ValueConversion(typeof(IGH_Param), typeof(string))]
    public class ParamSourcesConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return "";
            if (value is IGH_Param)
                return "Sources Count : " + ((IGH_Param)value).SourceCount.ToString();
            else return "";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
