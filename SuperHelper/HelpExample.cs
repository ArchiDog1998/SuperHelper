using GH_IO.Serialization;
using Grasshopper.Kernel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.IO;
using System.Windows.Controls;
using Grasshopper.GUI.Canvas.Interaction;
using Grasshopper.GUI.Canvas;
using Grasshopper.GUI;
using Grasshopper.Kernel.Special;
using Rhino.Runtime;
using System.Reflection;
using Grasshopper;
using System.Windows.Shapes;
using System.ComponentModel;
using Grasshopper.Kernel.Data;
using System.Windows.Forms;
using System.Runtime;
using System.Drawing.Imaging;

namespace SuperHelper
{
    public class HelpExample : IDisposable, INotifyPropertyChanged
    {
        private static readonly Guid HopsGuid = new Guid("C69BB52C-88BA-4640-B69F-188D111029E8");
        public class GH_CopyInteraction : GH_DragInteraction
        {
            private static readonly FieldInfo _modeInfo = typeof(GH_DragInteraction).GetRuntimeFields().First(f => f.Name.Contains("m_mode"));
            private static readonly FieldInfo _attInfo = typeof(GH_DragInteraction).GetRuntimeFields().First(f => f.Name.Contains("m_att"));
            private static readonly FieldInfo _anchorsInfo = typeof(GH_DragInteraction).GetRuntimeFields().First(f => f.Name.Contains("m_anchors"));
            private GH_Document _doc;
            private static PropertyInfo _remoteDefinitionLocationInfo;
            private string _path;

            private GH_PanInteraction _panInteraction;
            private Point _panControlLocation;

            internal GH_CopyInteraction(GH_Canvas canvas, GH_Document doc, GH_CanvasMouseEvent mouseEvent, string path)
                : base(canvas, mouseEvent)
            {
                _path = path;
                _doc = doc;
                _modeInfo.SetValue(this, 1);

                _attInfo.SetValue(this, new List<IGH_Attributes>());
                _anchorsInfo.SetValue(this, new List<PointF>());

                foreach (IGH_DocumentObject @object in doc.Objects)
                {
                    AddAttribute(@object.Attributes);
                }
                var atts = (List<IGH_Attributes>)_attInfo.GetValue(this);

                List<IGH_DocumentObject> list2 = new List<IGH_DocumentObject>();
                foreach (IGH_Attributes item2 in atts)
                {
                    list2.Add(item2.DocObject);
                }
                if (atts.Count == 1)
                {
                    AddSnapAttributes(atts[0]);
                }

                Instances.CursorServer.AttachCursor(m_canvas, "GH_DragCopy");
                Grasshopper.Instances.ActiveCanvas.ActiveInteraction = this;
            }

            public override GH_ObjectResponse RespondToMouseDown(GH_Canvas sender, GH_CanvasMouseEvent e)
            {
                if (e.Button == MouseButtons.Right)
                {
                    _panInteraction = new GH_PanInteraction(sender, e);
                    _panControlLocation = e.ControlLocation;

                }
                return base.RespondToMouseDown(sender, e);
            }

            public override GH_ObjectResponse RespondToMouseMove(GH_Canvas sender, GH_CanvasMouseEvent e)
            {
                _panInteraction?.RespondToMouseMove(sender, e);
                return base.RespondToMouseMove(sender, e);
            }

            public override GH_ObjectResponse RespondToMouseUp(GH_Canvas sender, GH_CanvasMouseEvent e)
            {

                if (e.Button == MouseButtons.Left)
                {
                    float width = e.CanvasX - m_canvas_mousedown.X;
                    float height = e.CanvasY - m_canvas_mousedown.Y;

                    var list = ((List<IGH_Attributes>)_attInfo.GetValue(this)).Select(a => a.InstanceGuid);

                    GH_DocumentIO gH_DocumentIO = new GH_DocumentIO(_doc);
                    gH_DocumentIO.Copy(GH_ClipboardType.Local, list);
                    if (!gH_DocumentIO.Paste(GH_ClipboardType.Local))
                    {
                        return GH_ObjectResponse.Release;
                    }
                    int num2 = gH_DocumentIO.Document.ObjectCount - 1;
                    for (int j = 0; j <= num2; j++)
                    {
                        gH_DocumentIO.Document.Objects[j].Attributes.Pivot += new SizeF(width, height);
                        gH_DocumentIO.Document.Objects[j].Attributes.ExpireLayout();
                        gH_DocumentIO.Document.Objects[j].Attributes.PerformLayout();
                    }

                    gH_DocumentIO.Document.SelectAll();
                    gH_DocumentIO.Document.ExpireSolution();
                    gH_DocumentIO.Document.MutateAllIds();
                    
                    if (string.IsNullOrEmpty(_path))
                    {
                        IEnumerable<IGH_DocumentObject> objects = gH_DocumentIO.Document.Objects;
                        sender.Document.DeselectAll();
                        sender.Document.MergeDocument(gH_DocumentIO.Document);
                        sender.Document.UndoUtil.RecordAddObjectEvent("Copy", objects);
                    }
                    else
                    {
                        var pivot = gH_DocumentIO.Document.Objects.First(o => o.ComponentGuid == HopsGuid).Attributes.Pivot;

                        Instances.ActiveCanvas.InstantiateNewObject(HopsGuid, pivot, false);

                        var hop = Instances.ActiveCanvas.Document.Objects.Last();
                        if (_remoteDefinitionLocationInfo == null)
                        {
                            _remoteDefinitionLocationInfo = hop.GetType().GetRuntimeProperties().First(p => p.Name.Contains("RemoteDefinitionLocation"));
                        }

                        _remoteDefinitionLocationInfo.SetValue(hop, _path);
                    }
                }
                else
                {
                    if (_panInteraction != null && DistanceTo(e.ControlLocation, _panControlLocation) < 10)
                    {
                        Instances.ActiveCanvas.ActiveInteraction = null;
                    }

                    _panInteraction = null;
                    return GH_ObjectResponse.Ignore;
                }

                sender.Document.NewSolution(expireAllObjects: false);

                return GH_ObjectResponse.Release;
            }

            public override GH_ObjectResponse RespondToKeyDown(GH_Canvas sender, KeyEventArgs e)
            {
                Keys keyCode = e.KeyCode;
                if (keyCode == Keys.Menu || keyCode == Keys.Alt)
                {
                    return GH_ObjectResponse.Ignore;
                }
                return base.RespondToKeyDown(sender, e);
            }

            internal static float DistanceTo(PointF a, PointF b)
            {
                return (float)Math.Sqrt(Math.Pow(a.X - b.X, 2) + Math.Pow(a.Y - b.Y, 2));
            }
        }

        private bool _isValid = true;
        public bool IsValid
        {
            get => _isValid;
            private set
            {
                if (_isValid == value) return;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsValid)));
                FileName = "Invalid";
                _isValid = value;
            }
        }

        private Task<GH_Document> _documentTask;
        private GH_Document _doc;

        private string _path;
        public string Path
        {
            get => _path;
            set
            {
                if (_path == value) return;
                _path = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Path)));
                _documentTask = GetDocumentAsync();
            }
        }
        public HelpExample(string path)
        {
            Path = path;
        }

        public HelpExample()
        {

        }


        public event PropertyChangedEventHandler PropertyChanged;

        private Bitmap _thumbnail;
        public Bitmap Thumbnail
        {
            get => _thumbnail;
            set
            {
                if (_thumbnail == value) return;
                _thumbnail = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Thumbnail)));
            }
        }

        private static Bitmap _ghIcon = InitIcon();
        private Bitmap _icon = _ghIcon;
        public Bitmap Icon
        {
            get => _icon;
            set
            {
                if (_icon == value) return;
                if (value == null) return;
                _icon = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Icon)));
            }
        }

        private static Bitmap InitIcon()
        {
            var path = System.IO.Path.GetDirectoryName(Instances.DocumentEditor.GetType().Assembly.Location) + "\\Tutorials";
            foreach (var p in Directory.GetFiles(path, "*.gh"))
            {
                var result = IconImageConverter.ExtractFromPath(p)?.ToBitmap();
                if (result != null)
                {
                    return result;
                }
            }
            return null;
        }

        private Bitmap _picture;
        public Bitmap Picture
        {
            get => _picture;
            set
            {
                if (_picture == value) return;
                if (value == null) return;
                _picture = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Picture)));
            }
        }

        private string _description;

        public string Description
        {
            get { return _description; }
            set
            {
                if (_description == value) return;
                _description = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Description)));
            }
        }

        private string _fileName = "Loading";

        public string FileName
        {
            get { return _fileName; }
            set
            {
                if (_fileName == value) return;
                _fileName = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(FileName)));
            }
        }

        private string _author;

        public string Author
        {
            get { return _author; }
            set
            {
                if (_author == value) return;
                _author = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Author)));
            }
        }

        #region Paste
        public async Task PasteFromArchive(bool isHops)
        {
            var result = await CheckForDocument();
            if (!result)
            {
                Instances.ActiveCanvas.Focus();
                return;
            }

            GH_Document document = _doc;
            string path = string.Empty;
            if (isHops)
            {
                document = new GH_Document();
                var hop = Instances.ComponentServer.EmitObject(HopsGuid);

                if (hop == null)
                {
                    MessageBox.Show("Please install Hops!");
                    return;
                }

                hop.CreateAttributes();
                hop.Attributes.Pivot = default(PointF);
                hop.Attributes.ExpireLayout();
                hop.Attributes.PerformLayout();

                document.AddObject(hop, true);

                path = Path;
            }

            var rect = document.BoundingBox();
            var pivot = new PointF(rect.X + rect.Width/2, rect.Y + rect.Height/2);


            if (!Instances.ActiveCanvas.IsDocument)
            {
                Instances.ActiveCanvas.Document = Instances.DocumentServer.AddNewDocument();
            }

            new GH_CopyInteraction(Instances.ActiveCanvas, document,
                new GH_CanvasMouseEvent(new Point(), pivot, System.Windows.Forms.MouseButtons.Left), path);
        }

        public async Task CopyFromArchive()
        {
            var result = await CheckForDocument();
            if (!result)
            {
                Instances.ActiveCanvas.Focus();
                return;
            }

            new GH_DocumentIO(_doc).Copy(GH_ClipboardType.System, false);
        }

        private async Task<bool> CheckForDocument()
        {
            if (!IsValid) return false;

            await LoadDocumentFromTask();

            if (_doc == null)
            {
                IsValid = false;
                return false;
            }

            return true;
        }

        private async Task LoadDocumentFromTask()
        {
            if (_documentTask == null) return;

            await _documentTask;
            _doc = _documentTask.Result;
            _documentTask.Dispose();
            _documentTask = null;
        }



        #endregion
        #region GetArchive From Url or Path
        private async Task<GH_Document> GetDocumentAsync()
        {
            var bytes = await Task.Run(() => ReadFromUrlOrPath(Path));

            var archive = GetArchiveFromBytes(bytes);
            if (archive == null) return null;
            var doc = GetDocumentFormArchive(archive);
            if(doc == null) return null;
            UpdateTheProperyDoc(doc, archive);
            return doc;
        }

        static MethodInfo[] _iconBitmap = typeof(GH_DocumentProperties).GetRuntimeMethods().Where(f => f.Name.Contains("IconBitmap")).ToArray();
        private void UpdateTheProperyDoc(GH_Document doc, GH_Archive archive)
        {
            Thumbnail = GetBitmapFormArchive(archive);
            var prop = doc.Properties;
            Description = prop.Description;
            FileName = prop.ProjectFileName.Split('.')[0];
            if(_iconBitmap != null && _iconBitmap.Length > 0)
            {
                Icon = (Bitmap) _iconBitmap[0]?.Invoke(prop, new object[] { new Size(24, 24) });
            }
            //Picture = CreatePicture(doc);

            var author = doc.Author.Name;
            if (!string.IsNullOrEmpty(author))
                Author = " - " + author;
        }

        private static Bitmap CreatePicture(GH_Document doc)
        {
            const float zoom = 1;

            var canvas = new GH_Canvas();
            canvas.Document = doc;

            var rec = canvas.Document.BoundingBox();
            rec.X *= zoom;
            rec.Y *= zoom;
            rec.Width *= zoom;
            rec.Height *= zoom;
            var rectangle = GH_Convert.ToRectangle(rec);
            rectangle.Inflate(100, 100);

            GH_Viewport gH_Viewport = new GH_Viewport(canvas.Viewport);
            gH_Viewport.Width = rectangle.Width;
            gH_Viewport.Height = rectangle.Height;
            gH_Viewport.Zoom = zoom;
            gH_Viewport.Tx = -rectangle.X;
            gH_Viewport.Ty = -rectangle.Y;
            gH_Viewport.ComputeProjection();
           
            var bitmap = canvas.GenerateHiResImageTile(gH_Viewport, Color.Transparent);

            canvas.Dispose();

            return bitmap;
        }

        private static Bitmap GetBitmapFormArchive(GH_Archive archive)
        {
            if (archive == null)
            {
                return null;
            }
            GH_IReader gH_IReader = archive.GetRootNode.FindChunk("Thumbnail");
            if (gH_IReader == null)
            {
                return null;
            }
            Bitmap bitmap = gH_IReader.GetDrawingBitmap("Thumbnail");
            if (bitmap != null)
            {
                bitmap = (Bitmap)bitmap.Clone();
            }
            return bitmap;
        }

        private static GH_Document GetDocumentFormArchive(GH_Archive archive)
        {
            if (archive == null)
            {
                return null;
            }
            var document = new GH_Document();
            if (archive.ExtractObject(document, "Definition"))
            {
                return document;
            }
            return null;
        }

        private GH_Archive GetArchiveFromBytes(byte[] bytes)
        {
            if(bytes == null || bytes.Length == 0)
            {
                IsValid = false;
                return null;
            }

            GH_Archive archive = new GH_Archive();
                if (archive.Deserialize_Binary(bytes))
                {
                    return archive;
                }

            IsValid = false;
            return null;
        }

        private static byte[] ReadFromUrlOrPath(string path)
        {
            byte[] bytes = new byte[0];
            if (path.StartsWith("http", StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    bytes = new System.Net.WebClient().DownloadData(path);
                }
                catch
                {

                }
            }
            else if (File.Exists(path))
            {
                bytes = File.ReadAllBytes(path);
            }
            return bytes;
        }
        #endregion

        ~HelpExample()
        {
            Dispose();
        }
        public void Dispose()
        {
            _documentTask?.Dispose();    
            _thumbnail?.Dispose();
        }
    }
}
