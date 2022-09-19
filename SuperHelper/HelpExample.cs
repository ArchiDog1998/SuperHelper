using GH_IO.Serialization;
using Grasshopper.Kernel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.IO;
using Eto.Forms;
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

namespace SuperHelper
{
    public class HelpExample : IDisposable, INotifyPropertyChanged
    {
        public class GH_CopyInteraction : GH_DragInteraction
        {
            private static readonly FieldInfo _modeInfo = typeof(GH_DragInteraction).GetRuntimeFields().First(f => f.Name.Contains("m_mode"));
            private static readonly FieldInfo _attInfo = typeof(GH_DragInteraction).GetRuntimeFields().First(f => f.Name.Contains("m_att"));
            private static readonly FieldInfo _anchorsInfo = typeof(GH_DragInteraction).GetRuntimeFields().First(f => f.Name.Contains("m_anchors"));
            private GH_Document _doc;
            public GH_CopyInteraction(GH_Canvas canvas, GH_Document doc, GH_CanvasMouseEvent mouseEvent)
                : base(canvas, mouseEvent)
            {
                _doc = doc;
                _modeInfo.SetValue(this, 1);

                _attInfo.SetValue(this, new List<IGH_Attributes>());
                _anchorsInfo.SetValue(this, new List<PointF>());

                foreach (IGH_DocumentObject @object in doc.Objects)
                {
                    //if (!m_att.Contains(@object.Attributes))
                    {
                        AddAttribute(@object.Attributes);
                    }
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

            public override GH_ObjectResponse RespondToMouseUp(GH_Canvas sender, GH_CanvasMouseEvent e)
            {
                if (e.Button == System.Windows.Forms.MouseButtons.Left)
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
                    IEnumerable<IGH_DocumentObject> objects = gH_DocumentIO.Document.Objects;
                    sender.Document.DeselectAll();
                    sender.Document.MergeDocument(gH_DocumentIO.Document);
                    sender.Document.UndoUtil.RecordAddObjectEvent("Copy", objects);
                }

                sender.Document.NewSolution(expireAllObjects: false);

                return GH_ObjectResponse.Release;
            }
        }

        private bool _isValid = true;
        public bool IsValid
        {
            get => _isValid;
            private set
            {
                if (_isValid == value) return;
                MessageBox.Show("Failed to find document!");
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

        private Bitmap _bitmap;
        public Bitmap Thumbnail
        {
            get => _bitmap;
            set
            {
                if (_bitmap == value) return;
                _bitmap = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Thumbnail)));
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

        private string _fileName;

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
        public async Task PasteFromArchive()
        {
            var result = await CheckForDocument();
            if (!result) return;

            float x = 0;
            float y = 0;
            var count = _doc.ObjectCount;
            foreach (var obj in _doc.Objects)
            {
                x += obj.Attributes.Pivot.X / count;
                y += obj.Attributes.Pivot.Y / count;
            }


            if (!Grasshopper.Instances.ActiveCanvas.IsDocument)
            {
                Grasshopper.Instances.ActiveCanvas.Document = Instances.DocumentServer.AddNewDocument();
            }

            new GH_CopyInteraction(Grasshopper.Instances.ActiveCanvas, _doc,
                new GH_CanvasMouseEvent(new Point(), new PointF(x, y), System.Windows.Forms.MouseButtons.Left));
        }

        public async Task CopyFromArchive()
        {
            var result = await CheckForDocument();
            if (!result) return;

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
            var doc = GetDocumentFormArchive(archive);
            UpdateTheProperyDoc(doc, archive);
            return doc;
        }

        private void UpdateTheProperyDoc(GH_Document doc, GH_Archive archive)
        {
            Thumbnail = GetBitmapFormArchive(archive);
            var prop = doc.Properties;
            Description = prop.Description;
            FileName = prop.ProjectFileName.Split('.')[0];

            var author = doc.Author.Name;
            if (!string.IsNullOrEmpty(author))
                Author = " - " + author;
        }

        private static Bitmap GetBitmapFormArchive(GH_Archive archive)
        {
            if (archive == null)
            {
                throw new ArgumentNullException(nameof(archive));
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
            throw new ArgumentException(nameof(archive));
        }

        private static GH_Document GetDocumentFormArchive(GH_Archive archive)
        {
            if (archive == null)
            {
                throw new ArgumentNullException(nameof(archive));
            }
            var document = new GH_Document();
            if (archive.ExtractObject(document, "Definition"))
            {
                return document;
            }
            throw new ArgumentException(nameof(archive));
        }

        private GH_Archive GetArchiveFromBytes(byte[] bytes)
        {
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
                bytes = new System.Net.WebClient().DownloadData(path);
            }
            else if (File.Exists(path))
            {
                bytes = File.ReadAllBytes(path);
            }
            return bytes;
        }

        public void Dispose()
        {
            _documentTask?.Dispose();    
            _bitmap?.Dispose();
        }
        #endregion
    }
}
