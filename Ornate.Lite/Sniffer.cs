using Avalonia.Controls;
using Avalonia.Diagnostics;
using JetBrains.Annotations;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.Core.DevToolsProtocolExtension;
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

    public class WebSocketConnection
    {
        public List<Network.WebSocketFrame> Messages = new();
        public string URL;
        // Request isn't really necessary, as it only contains Headers, which aren't useful for us rn (cause they're empty) //TODO: also save those if fixed
        public Network.WebSocketResponse Response; // Response contains HeadersTest and RequestHeadersText

        public WebSocketConnection(string url)
        {
            URL = url;
        }
    }

    public class Sniffer
    {
        private WebView2 WebView;
        private DevToolsProtocolHelper DevTools;
        public Dictionary<int, Message> Messages = new();
        public Dictionary<string, WebSocketConnection> WebSockets = new();
        public Sniffer(WebView2 webview, DevToolsProtocolHelper devTools)
        {
            WebView = webview;
            DevTools = devTools;
        }

        public void Start()
        {
            //TODO: only checks responses, so messages with no response are ignored // WebResourceRequested isn't called
            WebView.CoreWebView2.WebResourceResponseReceived += ResponseReceived;
            DevTools.Network.EnableAsync();
            DevTools.Network.WebSocketCreated += Network_WebSocketCreated;
            DevTools.Network.WebSocketFrameSent += Network_WebSocketFrameSent;
            DevTools.Network.WebSocketFrameReceived += Network_WebSocketFrameReceived;
            DevTools.Network.WebSocketHandshakeResponseReceived += Network_WebSocketHandshakeResponseReceived;
        }

        private void Network_WebSocketHandshakeResponseReceived(object sender, Network.WebSocketHandshakeResponseReceivedEventArgs e)
        {
            if (!WebSockets.TryGetValue(e.RequestId, out var socket))
                return;

            socket.Response = e.Response;
        }

        private void Network_WebSocketFrameReceived(object sender, Network.WebSocketFrameReceivedEventArgs e)
        {
            if (!WebSockets.TryGetValue(e.RequestId, out var socket))
                return;

            socket.Messages.Add(e.Response);
        }

        private void Network_WebSocketFrameSent(object sender, Network.WebSocketFrameSentEventArgs e)
        {
            if (!WebSockets.TryGetValue(e.RequestId, out var socket))
                return;

            socket.Messages.Add(e.Response);
        }

        private void Network_WebSocketCreated(object sender, Network.WebSocketCreatedEventArgs e)
        {
            if (WebSockets.TryGetValue(e.RequestId, out _))
                return;

            WebSockets[e.RequestId] = new(e.Url);
        }

        private void ResponseReceived(object sender, CoreWebView2WebResourceResponseReceivedEventArgs e)
        {
            var id = e.Request.GetHashCode();
            Messages[id] = new Message(e.Request, e.Response);
        }
    }
}
