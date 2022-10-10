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
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.Runtime.InteropServices;
using System.Text;
using Rhino.Commands;

namespace SuperHelper
{
    public abstract class MenuReplacer : GH_DocumentObject
    {
        private static readonly MethodInfo _htmlHelpInfo = typeof(GH_DocumentObject).GetRuntimeMethods().First(m => m.Name.Contains("HtmlHelp_Source"));

        private static readonly string _location = Path.Combine(Folders.SettingsFolder, "urls.json");
        private static readonly string _locationEx = Path.Combine(Folders.SettingsFolder, "urlex.json");

        internal static Dictionary<string, string> UrlDict = new Dictionary<string, string>();
        internal static Dictionary<string, string[]> UrlExDict = new Dictionary<string, string[]>();

        internal static SuperHelperControl _control = new SuperHelperControl();

        public static DateTime LastDownloadURLTime
        {
            get => Instances.Settings.GetValue(nameof(LastDownloadURLTime), DateTime.MinValue);
            set => Instances.Settings.SetValue(nameof(LastDownloadURLTime), value);
        }

        //[DllImport("kernel32.dll", ExactSpelling = true, CallingConvention = CallingConvention.StdCall, SetLastError = true)]
        //private static extern int GetUserGeoID(int geoClass);
        //[DllImport("kernel32.dll")]
        //private static extern int GetUserDefaultLCID();

        //[DllImport("kernel32.dll")]
        //private static extern int GetGeoInfo(int geoid, int geoType, StringBuilder lpGeoData, int cchData, int langid);

        //public static string GetMachineCurrentLocation()
        //{
        //    int geoId = GetUserGeoID(16);
        //    int lcid = GetUserDefaultLCID();
        //    StringBuilder locationBuffer = new StringBuilder(100);
        //    GetGeoInfo(geoId, 5, locationBuffer, locationBuffer.Capacity, lcid);

        //    return locationBuffer.ToString().Trim();
        //}

        protected MenuReplacer(IGH_InstanceDescription tag) : base(tag)
        {
        }

        internal static void SaveUrlDictToJson()
        {
            JavaScriptSerializer ser = new JavaScriptSerializer();
            File.WriteAllText(_location, ser.Serialize(UrlDict));
        }

        internal static void SaveUrlExToJson()
        {
            JavaScriptSerializer ser = new JavaScriptSerializer();
            File.WriteAllText(_locationEx, ser.Serialize(UrlExDict));
        }

        public static bool Init()
        {
            new HighLightConduit().Enabled = true;
            JavaScriptSerializer ser = new JavaScriptSerializer();
            var client =  new System.Net.WebClient();

            //Read from json.
            try
            {
#if DEBUG
#else
                var now = DateTime.Now;
                //Download for first.
                if (now - LastDownloadURLTime > new TimeSpan(1, 0, 0, 0))
                {
                    LastDownloadURLTime = now;
                    try
                    {
                        var bytes = client.DownloadData(@"https://raw.githubusercontent.com/ArchiDog1998/SuperHelper/master/urls.json");
                        UrlDict = ser.Deserialize<Dictionary<string, string>>(Encoding.UTF8.GetString(bytes));
                    }
                    catch
                    {
                        try
                        {
                            var bytes = client.DownloadData(@"https://gitee.com/ArchiTed1998/SuperHelper/raw/master/urls.json");
                            UrlDict = ser.Deserialize<Dictionary<string, string>>(Encoding.UTF8.GetString(bytes));
                        }
                        catch
                        {

                        }
                    }

                    try
                    {
                        var bytes = client.DownloadData(@"https://raw.githubusercontent.com/ArchiDog1998/SuperHelper/master/urlex.json");
                        UrlExDict = ser.Deserialize<Dictionary<string, string[]>>(Encoding.UTF8.GetString(bytes));
                    }
                    catch
                    {
                        try
                        {
                            var bytes = client.DownloadData(@"https://gitee.com/ArchiTed1998/SuperHelper/raw/master/urlex.json");
                            UrlExDict = ser.Deserialize<Dictionary<string, string[]>>(Encoding.UTF8.GetString(bytes));
                        }
                        catch
                        {

                        }
                    }
                }
#endif

                if (File.Exists(_location))
                {
                    string jsonStr = File.ReadAllText(_location);
                    foreach (var pair in ser.Deserialize<Dictionary<string, string>>(jsonStr))
                    {
                        UrlDict[pair.Key] = pair.Value;
                    }
                }


                if (File.Exists(_locationEx))
                {
                    string jsonStr = File.ReadAllText(_locationEx);
                    foreach (var pair in ser.Deserialize<Dictionary<string, string[]>>(jsonStr))
                    {
                        if(UrlExDict.TryGetValue(pair.Key, out var values))
                        {
                            var strs = new HashSet<string>(values);
                            foreach (var s in pair.Value)
                            {
                                strs.Add(s);
                            }
                            UrlExDict[pair.Key] = strs.ToArray();
                        }
                        else
                        {
                            UrlExDict[pair.Key] = pair.Value;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            Instances.ActiveCanvas.DocumentObjectMouseDown += ActiveCanvas_DocumentObjectMouseDown;
            Instances.ActiveCanvas.DocumentChanged += ActiveCanvas_DocumentChanged;

            return ExchangeMethod(
                typeof(GH_DocumentObject).GetRuntimeMethods().Where(m => m.Name.Contains("Menu_ObjectHelpClick")).First(),
                typeof(MenuReplacer).GetRuntimeMethods().Where(m => m.Name.Contains(nameof(Menu_ObjectHelpClickNew))).First()
                );
        }

        private static void ActiveCanvas_DocumentChanged(Grasshopper.GUI.Canvas.GH_Canvas sender, Grasshopper.GUI.Canvas.GH_CanvasDocumentChangedEventArgs e)
        {
            if(e.OldDocument != null)
            {
                e.OldDocument.SolutionEnd -= DocumentSolutionEnd;
            }
            if (e.NewDocument != null)
            {
                e.NewDocument.SolutionEnd -= DocumentSolutionEnd;
                e.NewDocument.SolutionEnd += DocumentSolutionEnd;
            }
        }

        private static void DocumentSolutionEnd(object sender, GH_SolutionEventArgs e)
        {
            if (_control.MajorControl.SelectedIndex != 3) return;
            var data = _control.DataContext;
            if (data == null) return;

            _control.DataContext = null;
            _control.DataContext = data;
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
            SuperHelperAssemblyPriority.Show();
            SetOneObject(this);
        }

        private static void SetOneObject(GH_DocumentObject obj)
        {
            Task.Run(() =>
            {
                HighLightConduit.HighLightObject = null;
                foreach (var view in Rhino.RhinoDoc.ActiveDoc.Views)
                {
                    view.Redraw();
                }

                _control.Dispatcher.Invoke(() =>
                {
                    bool shouldSwitchURL = true;
                    if(_control.DataContext is GH_DocumentObject docObj)
                    {
                        if (docObj.ComponentGuid == obj.ComponentGuid) shouldSwitchURL = false;
                    }

                    //Change selected tabItem.
                    if((_control.MajorControl.SelectedIndex == 5 && _control.DataContext == null) || _control.MajorControl.SelectedIndex == 6)
                    {
                        _control.MajorControl.SelectedIndex = 3;
                    }

                    try
                    {
                        _control.DataContext = null;
                        _control.DataContext = obj;
                    }
                    catch
                    {

                    }

                    if (!shouldSwitchURL) return;

                    string html = (string)_htmlHelpInfo.Invoke(obj, new object[0]);

                    //OldUrl
                    if (html == null || html.Length == 0)
                    {
                        html = "We're sorry. Help is not yet available for this object";
                    }

                    if (html.StartsWith("GOTO:", StringComparison.OrdinalIgnoreCase))
                    {
                        _control.oldUrl.AllowNavigation = true;
                        _control.oldUrl.Navigate(html.Substring(5));
                    }
                    else
                    {
                        _control.oldUrl.Navigate("about:blank");
                        _control.oldUrl.Document.OpenNew(false);

                        _control.oldUrl.Document.Write(html);
                        _control.oldUrl.Refresh();
                    }

                    if (obj != null)
                    {
                        //meWeb
                        if (UrlDict.TryGetValue(obj.ComponentGuid.ToString(), out string url))
                        {
                            _control.UrlTextBox.Text = url;

                            if (_control.myWeb.Source == null)
                            {
                                _control.myWeb.Source = new Uri(url);
                            }
                        }
                        else if (html.StartsWith("GOTO:", StringComparison.OrdinalIgnoreCase))
                        {
                            url = html.Substring(5);
                            _control.UrlTextBox.Text = url;

                            if (_control.myWeb.Source == null)
                            {
                                _control.myWeb.Source = new Uri(url);
                            }
                        }
                        else
                        {
                            _control.UrlTextBox.Text = "";
                        }

                        //Example
                        _control.SaveExampleValue(false);
                        if (UrlExDict.TryGetValue(obj.ComponentGuid.ToString(), out string[] urls))
                        {
                            _control.ExampleList.ItemsSource = new ObservableCollection<HelpExample>(urls.Select(u => new HelpExample(u)));
                        }
                        else
                        {
                            _control.ExampleList.ItemsSource = new ObservableCollection<HelpExample>();
                        }
                    }
                });
            });
        }

        internal static void ActiveCanvas_DocumentObjectMouseDown(object sender, Grasshopper.GUI.GH_CanvasObjectMouseDownEventArgs e)
        {
            if (_control.AutoTarget)
            {
                var obj = (GH_DocumentObject)e.Object.Object.Attributes.GetTopLevel?.DocObject;
                SetOneObject(obj);
            }
        }
    }
}