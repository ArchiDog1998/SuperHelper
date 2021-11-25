using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using System;
using System.Collections;
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
    /// Interaction logic for GHStructureInfoControl.xaml
    /// </summary>
    public partial class GHStructureInfoControl : UserControl
    {
        public GHStructureInfoControl()
        {
            InitializeComponent();
        }

        private void TreeViewItem_Selected(object sender, RoutedEventArgs e)
        {
            TreeViewItem tvi = e.OriginalSource as TreeViewItem;

            if (tvi == null || e.Handled) return;

            tvi.IsExpanded = !tvi.IsExpanded;
            tvi.IsSelected = false;

            e.Handled = true;
        }
    }

    public struct StructureList
    {
        public GH_Path Path { get; private set; }
        public int Count { get; private set; }
        public ObservableCollection<IndexGoo> ListItems { get; private set; }
        public StructureList(GH_Path path, IList list)
        {
            this.Path = path;
            this.Count = list.Count;

            List<IndexGoo> list2 = new List<IndexGoo>();
            for (int i = 0; i < list.Count; i++)
            {
                list2.Add(new IndexGoo(i, (IGH_Goo)list[i]));
            }
            this.ListItems = new ObservableCollection<IndexGoo> (list2);
        }
    }

    public struct IndexGoo
    {
        public int Index { get; private set; }
        public IGH_Goo Data { get; private set; }
        public IndexGoo(int index, IGH_Goo goo)
        {
            this.Data = goo;
            this.Index = index;
        }
    }

    [ValueConversion(typeof(IndexGoo), typeof(string))]
    public class IndexGooDescConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return "";

            IGH_Goo goo = ((IndexGoo)value).Data;
            if(goo == null) return null;

            string result = $"Type Name : {goo.TypeName}\nType Description : {goo.TypeDescription}\nIs Valid : {goo.IsValid}";
            if (!goo.IsValid) result += $"Why not Valid : {goo.IsValidWhyNot}";
            return result;

        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }


    [ValueConversion(typeof(StructureList), typeof(string))]
    public class PathCountConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return "";

            StructureList structureList = (StructureList)value;

            return $"N = {structureList.Count}";

        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }



    [ValueConversion(typeof(IGH_Structure), typeof(bool))]
    public class ItemExpandedThanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return null;

            IGH_Structure structure = (IGH_Structure)value;

            return structure.PathCount < 15 && structure.DataCount < 100;

        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }


    [ValueConversion(typeof(IGH_Structure), typeof(string))]
    public class StructureBriefConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return null;

             IGH_Structure structure = (IGH_Structure)value;


            if (structure.DataCount == 0)
            {
                return "Empty parameter";
            }
            else if (structure.PathCount == 1)
            {
                int count = structure.get_Branch(0).Count;
                if (count == 1)
                {
                    return "Branch(List) with 1 item";
                }
                else
                {
                    return $"Branch(List) with {count} items";
                }
            }
            else
            {
                return $"Tree with {structure.PathCount} branches and {structure.DataCount} items" ;
            }

        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    [ValueConversion(typeof(IGH_Structure), typeof(ObservableCollection<StructureList>))]
    public class StructureDataConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return null;

            IGH_Structure structure =(IGH_Structure)value;

            ObservableCollection<StructureList> structureLists = new ObservableCollection<StructureList>();
            foreach (GH_Path path in structure.Paths)
            {
                structureLists.Add(new StructureList(path, structure.get_Branch(path)));
            }
            return structureLists;

        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    [ValueConversion(typeof(double), typeof(double))]
    public class MinusFortyFiveConverter : IValueConverter
    {
        /// <inheritdoc/>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (double)value - 45;
        }

        /// <inheritdoc/>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
