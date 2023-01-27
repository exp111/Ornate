using Avalonia.Controls;
using Microsoft.Web.WebView2.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ornate.Lite.WebView
{
    public class CustomWebView : WebView2
    {
        public CustomWebView() : base()
        {
            this.CoreWebView2InitializationCompleted += Init;
        }

        private void Init(object sender, CoreWebView2InitializationCompletedEventArgs e)
        {
            if (CoreWebView2 == null)
                return;

            CoreWebView2.PermissionRequested += CoreWebView2_PermissionRequested;
        }

        private void CoreWebView2_PermissionRequested(object sender, Microsoft.Web.WebView2.Core.CoreWebView2PermissionRequestedEventArgs e)
        {
            if (e.PermissionKind == CoreWebView2PermissionKind.Geolocation)
            {
                e.State = CoreWebView2PermissionState.Allow;
                e.Handled = true;
                return;
            }
        }
    }
}
