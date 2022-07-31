using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;


namespace SuperHelper
{
    /// <summary>
    /// Interaction logic for GHGooControl.xaml
    /// </summary>
    public partial class GHGooControl : UserControl
    {

        public GHGooControl()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            //Close base.
            Button button = (Button)sender;
            //button.IsEnabled = false;

            if (DataContext is GH_Colour)
            {
                button.Tag = nameof(GH_Colour);
            }
            else if (DataContext is GH_Material)
            {
                button.Tag = nameof(GH_Material);
            }
            else if (DataContext is GH_Transform)
            {
                button.Tag = nameof(GH_Transform);
            }
            else if (DataContext is GH_Matrix)
            {
                button.Tag = nameof(GH_Matrix);
            }
            else if (DataContext is GH_Plane)
            {
                button.Tag = nameof(GH_Plane);
            }
            else if (DataContext is GH_Circle)
            {
                button.Tag = nameof(GH_Circle);
            }
            else if (DataContext is GH_Arc)
            {
                button.Tag = nameof(GH_Arc);
            }
            else if (DataContext is GH_Box)
            {
                button.Tag = nameof(GH_Box);
            }
            else if (DataContext is GH_Curve)
            {
                button.Tag = nameof(GH_Curve);
            }
            else if (DataContext is GH_Line)
            {
                button.Tag = nameof(GH_Line);
            }

            else if (DataContext is GH_Surface)
            {
                button.Tag = nameof(GH_Surface);
            }
        }

        private void SwitchButton_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            Button button = sender as Button;
            if (button == null) return;

            IGH_Goo goo = (IGH_Goo)button.DataContext;

            HighLightConduit.HighLightObject = null;
            foreach (var view in Rhino.RhinoDoc.ActiveDoc.Views)
            {
                view.Redraw();
            }
        }

        private void SwitchButton_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            Button button = sender as Button;
            if (button == null) return;

            IGH_Goo goo = (IGH_Goo)button.DataContext;
            if (goo is IGH_PreviewData)
            {
                HighLightConduit.HighLightObject = (IGH_PreviewData)goo;
                foreach (var view in Rhino.RhinoDoc.ActiveDoc.Views)
                {
                    view.Redraw();
                }
            }
        }
    }
    [ValueConversion(typeof(Surface), typeof(int))]
    public class SurfaceDegreeVConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return null;
            Surface srf = (Surface)value;
            if (srf == null) return null;
            return srf.Degree(1);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    [ValueConversion(typeof(Surface), typeof(int))]
    public class SurfaceDegreeUConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return null;
            Surface srf = (Surface)value;
            if (srf == null) return null;
            return srf.Degree(0);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    [ValueConversion(typeof(Surface), typeof(int))]
    public class SurfaceDomainVConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return null;
            Surface srf = (Surface)value;
            if (srf == null) return null;
            return srf.Domain(1);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    [ValueConversion(typeof(Surface), typeof(int))]
    public class SurfaceDomainUConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return null;
            Surface srf = (Surface)value;
            if (srf == null) return null;
            return srf.Domain(0);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    [ValueConversion(typeof(IGH_Goo), typeof(string))]
    public class NameConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return "null";
            IGH_Goo goo = (IGH_Goo)value;
            if(goo == null) return "null";
            return goo.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    [ValueConversion(typeof(Transform), typeof(double[][]))]
    public class TransformGridConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return null;
            Transform transform = (Transform)value;

            return new double[][] {
                   new double[] { transform.M00, transform.M01, transform.M02, transform.M03 },
                   new double[] {transform.M10, transform.M11, transform.M12, transform.M13 },
                   new double[] {transform.M20, transform.M21, transform.M22, transform.M23 },
                   new double[] {transform.M30, transform.M31, transform.M32, transform.M33 } };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    [ValueConversion(typeof(Matrix), typeof(List<List<double>>))]
    public class MatrixGridConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return null;
            Matrix matrix = (Matrix)value;
            
            List<List<double>> list = new List<List<double>>();

            for (int i = 0; i < matrix.RowCount; i++)
            {
                List<double> list2 = new List<double>();
                for (int j = 0; j < matrix.ColumnCount; j++)
                {
                    list2.Add(matrix[i, j]);
                }
                list.Add(list2);
            }
            return list;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
