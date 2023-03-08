using Avalonia.Controls;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.Core.DevToolsProtocolExtension;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ornate.Lite
{
    public class Message
    {
        public CoreWebView2WebResourceRequest Request;
        public CoreWebView2WebResourceResponseView Response;

        public Message(CoreWebView2WebResourceRequest request, CoreWebView2WebResourceResponseView response) { Request = request; Response = response; }
    }

    public class Sniffer
    {
        private WebView2 WebView;
        public Dictionary<int, Message> Messages = new();
        public Sniffer(WebView2 webview)
        {
            WebView = webview;
        }

        public void Start()
        {
            //TODO: only checks responses, so messages with no response are ignored // WebResourceRequested isn't called
            WebView.CoreWebView2.WebResourceResponseReceived += ResponseReceived;
        }

        private void ResponseReceived(object sender, CoreWebView2WebResourceResponseReceivedEventArgs e)
        {
            var id = e.Request.GetHashCode();
            Messages[id] = new Message(e.Request, e.Response);
        }
    }
}
