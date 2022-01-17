using Grasshopper;
using Grasshopper.GUI.HTML;
using Grasshopper.Kernel;
using Rhino.Geometry;
using System;
using System.Linq;
using System.IO;
using System.Web.Script.Serialization;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Windows.Forms;

namespace SuperHelper
{
    public abstract class MenuReplacer : GH_DocumentObject
    {
        private static readonly string _location = Path.Combine(Folders.SettingsFolder, "urls.json");

        internal static Dictionary<string, string> UrlDict = new Dictionary<string, string>();


        internal static SuperHelperWindow _window = new SuperHelperWindow();

        internal static bool _windowShown = false;

        protected MenuReplacer(IGH_InstanceDescription tag) : base(tag)
        {
        }

        internal static void SaveToJson()
        {
            JavaScriptSerializer ser = new JavaScriptSerializer();
            File.WriteAllText(_location, ser.Serialize(UrlDict));
        }

        public static bool Init()
        {
            new HighLightConduit().Enabled = true;

            //Read from json.
            try
            {
                if (File.Exists(_location))
                {
                    string jsonStr = File.ReadAllText(_location);
                    JavaScriptSerializer ser = new JavaScriptSerializer();
                    UrlDict = ser.Deserialize<Dictionary<string, string>>(jsonStr);
                }
                else
                {
                    UrlDict = new Dictionary<string, string>();
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            Instances.ActiveCanvas.DocumentObjectMouseDown += ActiveCanvas_DocumentObjectMouseDown;

            return ExchangeMethod(
                typeof(GH_DocumentObject).GetRuntimeMethods().Where(m => m.Name.Contains("Menu_ObjectHelpClick")).First(),
                typeof(MenuReplacer).GetRuntimeMethods().Where(m => m.Name.Contains("Menu_ObjectHelpClickNew")).First()
                );
        }


        private static bool ExchangeMethod(MethodInfo targetMethod, MethodInfo injectMethod)
        {
            if (targetMethod == null || injectMethod == null)
            {
                return false;
            }
            RuntimeHelpers.PrepareMethod(targetMethod.MethodHandle);
            RuntimeHelpers.PrepareMethod(injectMethod.MethodHandle);
            unsafe
            {
                if (IntPtr.Size == 4)
                {
                    int* tar = (int*)targetMethod.MethodHandle.Value.ToPointer() + 2;
                    int* inj = (int*)injectMethod.MethodHandle.Value.ToPointer() + 2;
                    var relay = *tar;
                    *tar = *inj;
                    *inj = relay;
                }
                else
                {
                    long* tar = (long*)targetMethod.MethodHandle.Value.ToPointer() + 1;
                    long* inj = (long*)injectMethod.MethodHandle.Value.ToPointer() + 1;
                    var relay = *tar;
                    *tar = *inj;
                    *inj = relay;
                }
            }
            return true;
        }

        private void Menu_ObjectHelpClickNew(object sender, EventArgs e)
        {
            SetOneObject(this);
            _windowShown = true;
            _window.Show();
        }

        private static void SetOneObject(GH_DocumentObject obj)
        {
            HighLightConduit.HighLightObject = null;
            foreach (var view in Rhino.RhinoDoc.ActiveDoc.Views)
            {
                view.Redraw();
            }

            _window.DataContext = null;
            _window.DataContext = obj;

            string html = (string)typeof(GH_DocumentObject).GetRuntimeMethods().Where(m => m.Name.Contains("HtmlHelp_Source")).First().Invoke(obj, new object[0]);

            if(html == null || html.Length == 0)
            {
                html = "We're sorry. Help is not yet available for this object";
            }


            if (html.ToUpperInvariant().StartsWith("GOTO:"))
            {
                _window.oldUrl.AllowNavigation = true;
                _window.oldUrl.Navigate(html.Substring(5));
            }
            else
            {
                _window.oldUrl.Navigate("about:blank");
                _window.oldUrl.Document.OpenNew(false);

                _window.oldUrl.Document.Write(html);
                _window.oldUrl.Refresh();
            }


            if (obj != null && UrlDict.ContainsKey(obj.ComponentGuid.ToString()))
            {
                string url = UrlDict[obj.ComponentGuid.ToString()];
                _window.UrlTextBox.Text = url;

                if(_window.myWeb.Source == null)
                {
                    _window.myWeb.Source = new Uri(url);
                }
            }
            else
            {
                _window.UrlTextBox.Text = "";
            } 
        }

        internal static void ActiveCanvas_DocumentObjectMouseDown(object sender, Grasshopper.GUI.GH_CanvasObjectMouseDownEventArgs e)
        {
            if (_windowShown && _window.AutoTarget)
            {
                Instances.ActiveCanvas.Document.ScheduleSolution(10, (doc) =>
                {
                    IGH_DocumentObject obj = e.Object.Object.Attributes.GetTopLevel.DocObject;
                    if (obj.Attributes.Selected)
                        SetOneObject((GH_DocumentObject)obj);
                });
            }
        }
    }
}