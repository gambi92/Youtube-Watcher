using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using CefSharp;
using System.Windows.Forms;

namespace Youtube_Watcher_with_Chrome
{
    public class DragHandler : IDragHandler
    {
        public DragHandler()
        {

        }

        public bool OnDragEnter(IWebBrowser browserControl, IBrowser browser, IDragData dragData, DragOperationsMask mask)
        {
            return true;
        }

        public void OnDraggableRegionsChanged(IWebBrowser browserControl, IBrowser browser, IList<DraggableRegion> regions)
        {

        }
    }
}
