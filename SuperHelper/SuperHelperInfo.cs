using Grasshopper;
using Grasshopper.GUI;
using Grasshopper.GUI.Canvas;
using Grasshopper.Kernel;
using Rhino.Display;
using System;
using System.Linq;
using System.Reflection;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Windows.Forms.Integration;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.IO;
using System.Web.Script.Serialization;

namespace SuperHelper
{
    public class SuperHelperInfo : GH_AssemblyInfo
    {
        public override string Name => "SuperHelper";

        //Return a 24x24 pixel bitmap to represent this GHA library.
        public override Bitmap Icon => Properties.Resources.SuperHelperIcon_24;

        //Return a short string describing the purpose of this GHA library.
        public override string Description => "Much better helper control for grasshopper!";

        public override Guid Id => new Guid("A71D5B0A-9D5B-4E27-8933-BB83CB68066D");

        //Return a string identifying you or your company.
        public override string AuthorName => "秋水";

        //Return a string representing your preferred contact details.
        public override string AuthorContact => "1123993881@qq.com";

        public override string Version => "1.1.7";
    }

    public class SuperHelperAssemblyPriority : GH_AssemblyPriority
    {
        public static bool UseSuperHelperPanel
        {
            get => Instances.Settings.GetValue(nameof(UseSuperHelperPanel), true);
            set => Instances.Settings.SetValue(nameof(UseSuperHelperPanel), value);
        }

        public static int SuperHelperPanelWidth
        {
            get 
            { 
                var width = Instances.Settings.GetValue(nameof(SuperHelperPanelWidth), 400);
                if(width < 0)
                {
                    width = 400;
                    SuperHelperPanelWidth = width;
                }
                return width;
            }
            set => Instances.Settings.SetValue(nameof(SuperHelperPanelWidth), value);
        }

        public static bool IsSuperHelperOnRight
        {
            get => Instances.Settings.GetValue(nameof(SuperHelperPanelWidth), true);
            set
            {
                Instances.Settings.SetValue(nameof(SuperHelperPanelWidth), value);
                if (value)
                {
                    _ctrlHost.Dock = DockStyle.Right;
                    _splitter.Dock = DockStyle.Right;
                }
                else
                {
                    _ctrlHost.Dock = DockStyle.Left;
                    _splitter.Dock = DockStyle.Left;
                }

            }
        }

        private static ElementHost _ctrlHost = new ElementHost()
        {
            Dock = IsSuperHelperOnRight ? DockStyle.Right : DockStyle.Left,
            Child = MenuReplacer._control,
            Width = SuperHelperPanelWidth,
        };
        private static GH_Splitter _splitter = new GH_Splitter()
        {
            Cursor = Cursors.VSplit,
            Dock = IsSuperHelperOnRight ? DockStyle.Right : DockStyle.Left,
            Location = new Point(0, 439),
            Margin = new Padding(24),
            MaxSize = 8000,
            MinSize = 50,
            Name = "Helper Splitter",
            Size = new Size(10, 2744),
        };
        public static void SwitchSide()
        {
            if (_ctrlHost == null || _splitter == null) return;

            IsSuperHelperOnRight = !IsSuperHelperOnRight;
        }

        public static void Hide()
        {
            _ctrlHost.Hide();
            _splitter.Hide();
            UseSuperHelperPanel = false;
        }

        public static void Show()
        {
            _ctrlHost.Show();
            _splitter.Show();
            UseSuperHelperPanel = true;
        }
        public override GH_LoadingInstruction PriorityLoad()
        {
            Instances.CanvasCreated += Instances_CanvasCreated;
            return GH_LoadingInstruction.Proceed;
        }

        private void Instances_CanvasCreated(GH_Canvas canvas)
        {
            Instances.CanvasCreated -= Instances_CanvasCreated;

            GH_DocumentEditor editor = Instances.DocumentEditor;
            if (editor == null)
            {
                Instances.ActiveCanvas.DocumentChanged += ActiveCanvas_DocumentChanged;
                return;
            }
            DoingSomethingFirst(editor);
        }

        private void ActiveCanvas_DocumentChanged(GH_Canvas sender, GH_CanvasDocumentChangedEventArgs e)
        {
            Instances.ActiveCanvas.DocumentChanged -= ActiveCanvas_DocumentChanged;

            GH_DocumentEditor editor = Instances.DocumentEditor;
            if (editor == null)
            {
                MessageBox.Show("SuperHelper can't find the menu!");
                return;
            }
            DoingSomethingFirst(editor);
        }

        private void DoingSomethingFirst(GH_DocumentEditor editor)
        {
            MenuReplacer.Init();

            editor.Controls[0].Controls.Add(_splitter);
            editor.Controls[0].Controls.Add(_ctrlHost);

            Instances.DocumentEditor.FormClosing += (sender, e) =>
            {
                SuperHelperPanelWidth = _ctrlHost.Width;
            };

            Rhino.RhinoDoc.EndOpenDocument += RhinoDoc_EndOpenDocument;
            Rhino.RhinoDoc.BeginSaveDocument += RhinoDoc_BeginSaveDocument;
            Rhino.RhinoDoc.EndSaveDocument += RhinoDoc_EndSaveDocument;

            _splitter.MouseDown += (sender, e) =>
            {
                _splitter.MaxSize = editor.Controls[0].Width - 80;
            };

#if DEBUG
            SaveExamplesToJson();
#endif
        }

        private void RhinoDoc_EndSaveDocument(object sender, Rhino.DocumentSaveEventArgs e)
        {
            MenuReplacer._control.UpdateViewPortHost();
        }

        private void RhinoDoc_BeginSaveDocument(object sender, Rhino.DocumentSaveEventArgs e)
        {
            RhinoViewportHost.RemoveNameView();
        }

        private void RhinoDoc_EndOpenDocument(object sender, Rhino.DocumentOpenEventArgs e)
        {
            Task.Run(() =>
            {
                Task.Delay(500);

                MenuReplacer._control.Dispatcher.Invoke(() =>
                {
                    MenuReplacer._control.UpdateViewPortHost();
                });

            });
        }

#if DEBUG
        const string EXAMPLE_FILE = @"D:\OneDrive - stu.zafu.edu.cn\Rhino Share Files\07 Grasshopper Developments 蚱蜢开发\项目案例\SuperHelper\Examples";
        const string URL_EXAMPLE = @"https://github.com/ArchiDog1998/SuperHelper/raw/master/Examples";
        const string LocationEX = @"D:\OneDrive - stu.zafu.edu.cn\Rhino Share Files\07 Grasshopper Developments 蚱蜢开发\项目案例\SuperHelper\urlex.json";
        private void SaveExamplesToJson()
        {

            #region Get Objects' Dictionary
            Dictionary<string, Dictionary<string, Dictionary<string, Guid>>> allObjects =
                new Dictionary<string, Dictionary<string, Dictionary<string, Guid>>>();

            foreach (var item in Grasshopper.Instances.ComponentServer.ObjectProxies)
            {
                if(item.Obsolete) continue;
                if (item.Kind != GH_ObjectType.CompiledObject) continue;
                //if ((item.Exposure & GH_Exposure.hidden) == GH_Exposure.hidden) continue;

                if (!item.Desc.HasCategory) continue;

                if(!allObjects.TryGetValue(item.Desc.Category, out var subCateDict))
                    subCateDict = new Dictionary<string, Dictionary<string, Guid>>();

                if (!item.Desc.HasSubCategory) continue;

                if (!subCateDict.TryGetValue(item.Desc.SubCategory, out var nameDict))
                    nameDict = new Dictionary<string, Guid>();

                nameDict[item.Desc.Name] = item.Guid;

                subCateDict[item.Desc.SubCategory] = nameDict;
                allObjects[item.Desc.Category] = subCateDict;
            }
            #endregion

            if (!Directory.Exists(EXAMPLE_FILE)) return;

            #region Create Folder
            //foreach (var category in allObjects.Keys)
            //{
            //    string categoryPath = string.Join("\\", EXAMPLE_FILE, category);
            //    if (!Directory.Exists(categoryPath))
            //    {
            //        Directory.CreateDirectory(categoryPath);
            //    }

            //    foreach (var subCategory in allObjects[category].Keys)
            //    {
            //        string subCategoryPath = string.Join("\\", categoryPath, ObjectNameToDirectory(subCategory));
            //        if (!Directory.Exists(subCategoryPath))
            //        {
            //            Directory.CreateDirectory(subCategoryPath);
            //        }

            //        foreach (var objectName in allObjects[category][subCategory].Keys)
            //        {
            //            string objectPath = string.Join("\\", subCategoryPath, ObjectNameToDirectory(objectName));
            //            if (!Directory.Exists(objectPath))
            //            {
            //                try
            //                {
            //                    Directory.CreateDirectory(objectPath);
            //                }
            //                catch
            //                {

            //                }
            //            }
            //        }
            //    }
            //}
            #endregion


            string logs = string.Empty;
            //Set directory's files to json.
            foreach (var categroy in Directory.GetDirectories(EXAMPLE_FILE))
            {
                var Cate = categroy.Split('\\').Last();

                if (!allObjects.TryGetValue(Cate, out var subCateDict))
                {
                    logs += $"Category {Cate} not found!\n";
                    continue;
                }

                foreach (var subCategory in Directory.GetDirectories(categroy))
                {
                    var Sub = DirectoryToObjectName(subCategory.Split('\\').Last());

                    if (!subCateDict.TryGetValue(Sub, out var nameDict))
                    {
                        logs += $"Subcategory {Sub} in {Cate} not found!\n";
                        continue;
                    }
                    foreach (var name in Directory.GetDirectories(subCategory))
                    {
                        var Name = DirectoryToObjectName(name.Split('\\').Last());
                        if (!nameDict.TryGetValue(Name, out var guid))
                        {
                            logs += $"Name {Name} in {Cate} | {Sub} not found!\n";
                            continue;
                        }

                        if (!MenuReplacer.UrlExDict.TryGetValue(guid.ToString(), out var urls))
                            urls = new string[0];

                        HashSet<string> urlsList = new HashSet<string>(urls);
                        foreach (var file in Directory.GetFiles(name, "*.gh"))
                        {
                            var fileName = file.Split('\\').Last();

                            urlsList.Add(string.Join("/", URL_EXAMPLE, Cate, Sub, Name, fileName));
                        }

                        if (urlsList.Count > 0)
                            MenuReplacer.UrlExDict[guid.ToString()] = urlsList.ToArray();  
                    }
                }
            }

            if (!string.IsNullOrEmpty(logs))
            {
                MessageBox.Show(logs);
            }

            MenuReplacer.SaveUrlExToJson();
            JavaScriptSerializer ser = new JavaScriptSerializer();
            File.WriteAllText(LocationEX, ser.Serialize(MenuReplacer.UrlExDict));
        }

        private string ObjectNameToDirectory(string str)
        {
            str = str.Replace("|", " _X_ ");
            return str.Replace("/", " _S_ ");
        }

        private string DirectoryToObjectName(string str)
        {
            str = str.Replace(" _X_ ", "|");
            return str.Replace(" _S_ ", "/");
        }
#endif
    }
}