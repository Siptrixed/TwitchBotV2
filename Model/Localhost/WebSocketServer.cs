using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwitchBotV2.Model.Localhost
{
    public class WebSocketServer
    {
        private WebSocketSharp.Server.WebSocketServer WSS;

        public WebSocketServer()
        {
            WSS = new WebSocketSharp.Server.WebSocketServer("ws://localhost:8182");
            //WebSocketServer.AddWebSocketService<WebSockServ>("/widgets");
            //WebSocketServer.AddWebSocketService<WebSockServKeybd>("/keybd");
            WSS.Start();
        }
    }
}
