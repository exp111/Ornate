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
        public Network.Request Request;
        public Network.Response Response;

        public Message(Network.Request request, Network.Response response) { Request = request; Response = response; }
    }

    public class Sniffer
    {
        private DevToolsProtocolHelper DevTools;
        public Dictionary<string, Message> Messages = new();
        public Sniffer(DevToolsProtocolHelper devTools)
        {
            DevTools = devTools;
        }

        public void Start()
        {
            DevTools.Network.EnableAsync();
            DevTools.Network.RequestWillBeSent += Network_RequestWillBeSent;
            DevTools.Network.ResponseReceived += Network_ResponseReceived;
        }

        private void Network_RequestWillBeSent(object sender, Network.RequestWillBeSentEventArgs e)
        {
            if (Messages.TryGetValue(e.RequestId, out _))
                return; // request already exists

            Messages[e.RequestId] = new Message(e.Request, null);
        }

        private void Network_ResponseReceived(object sender, Network.ResponseReceivedEventArgs e)
        {
            if (!Messages.TryGetValue(e.RequestId, out var msg))
                return; // no response for the msg

            msg.Response = e.Response;
        }
    }
}
