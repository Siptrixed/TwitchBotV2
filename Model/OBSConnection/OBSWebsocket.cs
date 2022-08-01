using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using TwitchBotV2.Model.Utils;
using WebSocketSharp;

namespace TwitchBotV2.Model.OBSConnection
{
    public static class OBSWebSocket
    {
        static WebSocket WSock = null;
        static bool HoldConnection;
        public static EventHandler<bool>? OBSConnected;
        public static void Connect()
        {
            Task.Run(() => { 
                if (GlobalModel.Settings.OBSWebSockCI != null)
                {
                    WSock = new WebSocket($"ws://localhost:{GlobalModel.Settings.OBSWebSockCI.Port}/");
                    WSock.OnMessage += WSock_OnMessage;
                    WSock.OnClose += WSock_OnClose;
                    HoldConnection = true;
                    WSock.Connect();
                }
            });
        }
        public static void Disconnect()
        {
            HoldConnection = false;
            if (WSock.IsAlive) WSock.Close();
        }

        private static void WSock_OnClose(object? sender, CloseEventArgs e)
        {
            if (e.Code == 1006 || e.Code == 4009) OBSConnected?.Invoke(sender, false);
            else if (HoldConnection) Connect();
        }

        private static void WSock_OnMessage(object? sender, MessageEventArgs e)
        {
            var jp = new ParsedJson(e.Data);
            var data = jp["d"];
            switch ((int)jp["op"])
            {
                case 0:
                    var auth = data["authentication"];
                    using (SHA256 mySHA256 = SHA256.Create())
                    {
                        var saltedPass = $"{GlobalModel.Settings.OBSWebSockCI?.Password}{auth["salt"]}";
                        var saltedPassHash = Convert.ToBase64String(mySHA256.ComputeHash(Encoding.ASCII.GetBytes(saltedPass)));
                        var challengedSaltedPassHashHash = $"{saltedPassHash}{auth["challenge"]}";
                        var authString = Convert.ToBase64String(mySHA256.ComputeHash(Encoding.ASCII.GetBytes(challengedSaltedPassHashHash)));
                        var obj = DynamicObject.CreateObject();
                        obj["op"] = 1;
                        obj["d"] = DynamicObject.CreateObject();
                        obj["d"]["rpcVersion"] = 1;
                        obj["d"]["authentication"] = authString;
                        obj["d"]["eventSubscriptions"] = 0;
                        WSock.Send(obj.ToJSON());
                    }
                    break;
                case 2:
                    OBSConnected?.Invoke(sender, true);
                    break;
            }
            //throw new NotImplementedException();
        }
    }
}
