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
using System.Drawing;

namespace SuperHelper
{
    /// <summary>
    /// Interaction logic for ColorControl.xaml
    /// </summary>
    public partial class ColorControl : UserControl
    {
        public ColorControl()
        {
            InitializeComponent();
        }
    }

    [ValueConversion(typeof(Color), typeof(double))]
    public class AWidthConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return null;

            Color colour = (Color)value;

            return (double)colour.A / 255 * 90 + 10;

        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    [ValueConversion(typeof(Color), typeof(double))]
    public class RWidthConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return null;

            Color colour = (Color)value;

            return (double)colour.R / 255 * 90 + 10;

        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    [ValueConversion(typeof(Color), typeof(double))]
    public class GWidthConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return null;

            Color colour = (Color)value;

            return (double)colour.G / 255 * 90 + 10;

        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    [ValueConversion(typeof(Color), typeof(double))]
    public class BWidthConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return null;

            Color colour = (Color)value;

            return (double)colour.B / 255 * 90 + 10;

        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    [ValueConversion(typeof(Color), typeof(Brush))]
    public class GetColourConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return null;

            Color colour = (Color)value;

            return new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromArgb(colour.A, colour.R, colour.G, colour.B));

        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
