using Grasshopper.Kernel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    /// Interaction logic for ComponentInfoControl.xaml
    /// </summary>
    public partial class ComponentInfoControl : UserControl
    {
        public ComponentInfoControl()
        {
            InitializeComponent();
        }
    }

    [ValueConversion(typeof(IGH_Component), typeof(ObservableCollection<IGH_Param>))]
    public class ComponentInputConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return null;

            if (value is IGH_Component)
            {
                IGH_Component component = value as IGH_Component;
                return new ObservableCollection<IGH_Param>(component.Params.Input);
            }
            return null;

        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    [ValueConversion(typeof(IGH_Component), typeof(ObservableCollection<IGH_Param>))]
    public class ComponentOutputConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return null;

            if (value is IGH_Component)
            {
                IGH_Component component = value as IGH_Component;
                return new ObservableCollection<IGH_Param>(component.Params.Output);
            }
            return null;

        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
