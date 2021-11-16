using Grasshopper.Kernel;
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
    /// Interaction logic for InstanceDescControl.xaml
    /// </summary>
    public partial class InstanceDescControl : UserControl
    {
        public InstanceDescControl()
        {
            InitializeComponent();
        }

    }

    [ValueConversion(typeof(IGH_DocumentObject), typeof(Visibility))]
    public class IsNotParamConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return Visibility.Collapsed;
            if (value is IGH_Param)
            {
                return Visibility.Collapsed;
            }
            return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    [ValueConversion(typeof(IGH_DocumentObject), typeof(string))]
    public class GetRuntimeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return null;
            if (value is GH_ActiveObject)
            {
                GH_ActiveObject gH_ActiveObject = (GH_ActiveObject)value;
                List<string> list = new List<string>();
                if (GetRuntime(gH_ActiveObject, GH_RuntimeMessageLevel.Error, out string error)) list.Add(error);
                if (GetRuntime(gH_ActiveObject, GH_RuntimeMessageLevel.Warning, out string warning)) list.Add(warning);
                if (GetRuntime(gH_ActiveObject, GH_RuntimeMessageLevel.Remark, out string remark)) list.Add(remark);
                if (GetRuntime(gH_ActiveObject, GH_RuntimeMessageLevel.Blank, out string blank)) list.Add(blank);

                if(list.Count == 0) return null;
                return string.Join("\n \n", list);
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }

        private bool GetRuntime(GH_ActiveObject activeObject, GH_RuntimeMessageLevel level, out string result)
        {
            result = level.ToString() + ": \n";

            var lt = activeObject.RuntimeMessages(level);
            if (lt != null && lt.Count > 0)
            {
                result += string.Join("\n", lt);
                return true;
            }
            else return false;
        }
    }

    [ValueConversion(typeof(IGH_DocumentObject), typeof(string))]
    public class GetRunTimeSpanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return null;
            if (value is GH_ActiveObject)
            {
                GH_ActiveObject gH_ActiveObject = (GH_ActiveObject)value;
                return "Time : " + gH_ActiveObject.ProcessorTime.ToString();
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
