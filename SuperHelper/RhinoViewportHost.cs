using Rhino.Display;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Interop;

namespace SuperHelper
{
    public class RhinoViewportHost : HwndHost
    {
        //private RhinoView _view = null;
        private IntPtr _windowHandle = IntPtr.Zero;
        protected override System.Windows.Size ArrangeOverride(System.Windows.Size finalSize)
        {
            if(_windowHandle != IntPtr.Zero)
            {

                //_view.Size = new Size((int)finalSize.Width, (int)finalSize.Height);
                SetWindowPos(_windowHandle, 0, 0, 0, (int)finalSize.Width, (int)finalSize.Height, 0);
            }
            return base.ArrangeOverride(finalSize);
        }
        protected override HandleRef BuildWindowCore(HandleRef hwndParent)
        {
            var view = Rhino.RhinoDoc.ActiveDoc.Views.Find("Helper", true) ??
                Rhino.RhinoDoc.ActiveDoc.Views.Add("Helper", DefinedViewportProjection.Perspective, new Rectangle(0, 0, 300, 200), true);

            _windowHandle = GetParent(view.Handle);
            //SetParent(parent, Grasshopper.Instances.DocumentEditor.Handle);

            //Remove Resize & Caption  
            SetWindowLong(_windowHandle, -16, GetWindowLong(_windowHandle, -16) & ~0x00040000L & ~0x00C00000L);

            return new HandleRef(this, _windowHandle);
        }

        protected override void DestroyWindowCore(HandleRef hwnd)
        {
            DestroyWindow(hwnd.Handle);
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        internal static extern IntPtr GetParent(IntPtr hWnd);


        //[DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        //internal static extern IntPtr SetParent(IntPtr child, IntPtr parent);

        [DllImport("user32.dll", EntryPoint = "DestroyWindow", CharSet = CharSet.Unicode)]
        internal static extern bool DestroyWindow(IntPtr hwnd);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        internal static extern long SetWindowLong(IntPtr hWnd, int nIndex, long deNewLong);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        internal static extern long GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        internal static extern int SetWindowPos(IntPtr hWnd, int hWndInsertAfter, int x, int y, int Width, int Height, int flags);
    }
}
