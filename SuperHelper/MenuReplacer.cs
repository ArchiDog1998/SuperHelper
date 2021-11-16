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
        private static readonly string _name = "urls.json";

        internal static Dictionary<string, string> UrlDict = new Dictionary<string, string>();


        internal static SuperHelperWindow _window = new SuperHelperWindow();

        internal static bool _windowShown = false;

        protected MenuReplacer(IGH_InstanceDescription tag) : base(tag)
        {
        }

        internal static void SaveToJson()
        {
            JavaScriptSerializer ser = new JavaScriptSerializer();
            File.WriteAllText(Path.Combine(Path.GetDirectoryName(typeof(MenuReplacer).Assembly.Location), _name),
                ser.Serialize(UrlDict));
        }

        public static bool Init()
        {

            //Read from json.
            try
            {
                string fullName = Path.Combine(Path.GetDirectoryName(typeof(MenuReplacer).Assembly.Location), _name);
                if (File.Exists(fullName))
                {
                    string jsonStr = File.ReadAllText(fullName);
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
            _window.DataContext = null;
            _window.DataContext = obj;

            //Set Urls
            _window.oldUrl.Navigate("about:blank");
            _window.oldUrl.Document.OpenNew(false);

            string html = (string)typeof(GH_DocumentObject).GetRuntimeMethods().Where(m => m.Name.Contains("HtmlHelp_Source")).First().Invoke(obj, new object[0]);

            _window.oldUrl.Document.Write(html);
            _window.oldUrl.Refresh();

            if (obj != null && UrlDict.ContainsKey(obj.ComponentGuid.ToString()))
            {
                string url = UrlDict[obj.ComponentGuid.ToString()];
                _window.UrlTextBox.Text = url;
                _window.myWeb.Source = new Uri(url);

            }
            else
            {
                _window.UrlTextBox.Text = "";
                _window.myWeb.Source = null;
            } 
        }

        internal static void ActiveCanvas_DocumentObjectMouseDown(object sender, Grasshopper.GUI.GH_CanvasObjectMouseDownEventArgs e)
        {
            if (_windowShown)
                SetOneObject((GH_DocumentObject)e.Object.Object.Attributes.GetTopLevel.DocObject);
        }
    }
}