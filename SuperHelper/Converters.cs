using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace SuperHelper
{

    [ValueConversion(typeof(GH_DocumentObject), typeof(string))]
    public class TypeInfoConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return null;
            GH_DocumentObject gH_DocumentObject = (GH_DocumentObject)value;
            if (gH_DocumentObject == null) return null;

            return value.GetType().FullName + "\n \n" +
                   "Guid: " + gH_DocumentObject.ComponentGuid + "\n \n" +
                   string.Join(",\n", value.GetType().GetInterfaces().Select((t) => t.Name)) + "\n \n" +
                   FindFathers(value.GetType());
        }

        private string FindFathers(Type type)
        {
            List<string> typeFull = new List<string>();
            Type rightType = type;
            while (rightType != typeof(object))
            {
                typeFull.Add(GetTypeName(rightType));
                rightType = rightType.BaseType;
            }

            typeFull.Reverse();
            string full = typeFull[0];
            for (int i = 1; i < typeFull.Count; i++)
            {
                string space = "";
                for (int j = 0; j < i; j++)
                {
                    space += "--";
                }
                full += "\n" + space + typeFull[i];
            }
            return full;
        }

        private string GetTypeName(Type type)
        {
            string res = type.Name;
            if (!type.IsGenericType) return res;

            res = res.Split('`')[0];
            res += "<" + string.Join(",", type.GetGenericArguments().Select((t) => GetTypeName(t)).ToArray()) + ">";

            return res;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    [ValueConversion(typeof(GH_DocumentObject), typeof(string))]
    public class TypeLoactionConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return "";

            GH_AssemblyInfo info = null;
            Assembly typeAssembly = value.GetType().Assembly;
            foreach (GH_AssemblyInfo lib in Grasshopper.Instances.ComponentServer.Libraries)
            {
                if (lib.Assembly == typeAssembly)
                {
                    info = lib;
                    break;
                }
            }
            if (info == null) return "";
            return info.Location;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
    [ValueConversion(typeof(GH_DocumentObject), typeof(String))]
    public class WindowNameConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return "Super Helper";
            return $"Super Helper [{((GH_DocumentObject)value).Name}]";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    [ValueConversion(typeof(GH_DocumentObject), typeof(String))]
    public class ObjectNameConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return "";
            return $"{((GH_DocumentObject)value).Name} ({((GH_DocumentObject)value).NickName})" ;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    [ValueConversion(typeof(GH_DocumentObject), typeof(String))]
    public class ObjectDescriptionConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return "";
            return ((GH_DocumentObject)value).Description;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    [ValueConversion(typeof(GH_DocumentObject), typeof(ImageSource))]
    public class WindowIconConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return null;

            MemoryStream ms = new MemoryStream();
            ((GH_DocumentObject)value).Icon_24x24.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
            BitmapImage image = new BitmapImage();
            image.BeginInit();
            ms.Seek(0, SeekOrigin.Begin);
            image.StreamSource = ms;
            image.EndInit();
            return image;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }


}
