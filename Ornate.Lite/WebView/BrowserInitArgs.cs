using Avalonia.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ornate.Lite.WebView
{
    public class BrowserInitArgs : EventArgs
    {
        public WebView2 WebView;
        public BrowserInitArgs(WebView2 webview)
        {
            WebView = webview;
        }
    }
}
