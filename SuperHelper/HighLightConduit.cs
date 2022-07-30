using Grasshopper.Kernel;
using Rhino.Display;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SuperHelper
{
    public class HighLightConduit : DisplayConduit
    {
        public static IGH_PreviewData HighLightObject { get; set; }

        protected override void DrawOverlay(DrawEventArgs e)
        {
            if(HighLightObject != null)
            {
                int thickness = MenuReplacer._control.DisplayWireWidth;
                HighLightObject.DrawViewportWires(new GH_PreviewWireArgs(e.Viewport, e.Display, MenuReplacer._control.WireColor, thickness));
                HighLightObject.DrawViewportMeshes(new GH_PreviewMeshArgs(e.Viewport, e.Display, 
                    new DisplayMaterial(MenuReplacer._control.MaterialColor), MeshingParameters.Default));
            }
            base.DrawOverlay(e);
        }
    }
}
