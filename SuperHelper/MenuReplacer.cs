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

        private static Dictionary<string, string> _urlDict = new Dictionary<string, string>();

        protected MenuReplacer(IGH_InstanceDescription tag) : base(tag)
        {
        }

        public static bool Init()
        {
            JavaScriptSerializer ser = new JavaScriptSerializer();

            //var save = new Dictionary<string, string>()
            //{
            //    {"807b86e3-be8d-4970-92b5-f8cdcb45b06b", "http://100-gh.com/1665762" },
            //    {"47886835-e3ff-4516-a3ed-1b419f055464", "http://100-gh.com/1665763" }
            //};

            //File.WriteAllText(Path.Combine(Path.GetDirectoryName(typeof(MenuReplacer).Assembly.Location), _name),
            //    ser.Serialize(save));

            //Read from json.
            try
            {
                string jsonStr = File.ReadAllText(Path.Combine(Path.GetDirectoryName(typeof(MenuReplacer).Assembly.Location), _name));
                _urlDict = ser.Deserialize<Dictionary<string, string>>(jsonStr);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }



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

            if (_urlDict.ContainsKey(this.ComponentGuid.ToString()))
            {
                new SuperHelperWindow(this, _urlDict[this.ComponentGuid.ToString()]).Show();
            }
            else
            {
                GH_HtmlHelpPopup gH_HtmlHelpPopup = new GH_HtmlHelpPopup();
                if (gH_HtmlHelpPopup.LoadObject(this))
                {
                    gH_HtmlHelpPopup.SetLocation(Cursor.Position);
                    gH_HtmlHelpPopup.Show(Instances.DocumentEditor);
                }
            }

        }
    }
}