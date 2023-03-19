using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ornate.Lite
{
    public static class DesignData
    {
        public static SnifferWindow SnifferWindow { get; } = new SnifferWindow
        {
            Requests = new() { new() { Text = "GET https://playorna.com/api/me", Key = 0} },
            //TODO: set request/response text
            Sockets = new() { new() { Text = "/api/socket", Key = 0} },
            Frames = new() { new() { Text = "-> {message='attack', target='slime'}"} }
        };
    }

}
