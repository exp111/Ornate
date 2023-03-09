using Avalonia.Controls;
using Microsoft.Web.WebView2.Core;
using System;
using System.Collections.Generic;
using System.IO;

namespace Ornate.Lite
{
    public class Message
    {
        private static readonly object CacheLock = new();
        public CoreWebView2WebResourceRequest Request;
        public string PostData;
        public CoreWebView2WebResourceResponseView Response;
        public string ResponseData;

        public Message(CoreWebView2WebResourceRequest request, CoreWebView2WebResourceResponseView response) 
        { 
            Request = request;
            Response = response;
            lock (CacheLock)
            {
                CacheData();
            }
        }

        public async void CacheData()
        {
            if (Request != null && Request.Content != null) 
            {
                using (var sr = new StreamReader(Request.Content))
                {
                    var post = sr.ReadToEnd();
                    PostData = post;
                }
            }

            if (Response != null) 
            {
                try
                {
                    var content = await Response.GetContentAsync(); //FIXME: doesnt get content at some parts. idk why
                    using (var sr = new StreamReader(content))
                    {
                        var data = sr.ReadToEnd();
                        ResponseData = data;
                    }
                }
                catch (Exception ex) 
                {
                    ResponseData = $"Exception : {ex}";
                }
            }
        }
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
