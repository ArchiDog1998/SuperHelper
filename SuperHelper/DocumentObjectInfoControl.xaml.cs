﻿using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
    /// Interaction logic for DocumentObjectInfoControl.xaml
    /// </summary>
    public partial class DocumentObjectInfoControl : UserControl
    {
        public DocumentObjectInfoControl()
        {
            InitializeComponent();
        }
    }

    [ValueConversion(typeof(IGH_DocumentObject), typeof(bool))]
    public class IsParamConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return false;
            return value is IGH_Param;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    [ValueConversion(typeof(IGH_DocumentObject), typeof(bool))]
    public class IsComponentConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return false;
            return value is IGH_Component;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
