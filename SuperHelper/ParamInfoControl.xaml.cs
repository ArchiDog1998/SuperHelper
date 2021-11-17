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

        private void ShowMoreClick(object sender, RoutedEventArgs e)
        {
            ShowBorder.Visibility = Visibility.Collapsed;

            SourceTexts.Visibility = Visibility.Visible;
            StrucureInfo.Visibility = Visibility.Visible;
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
