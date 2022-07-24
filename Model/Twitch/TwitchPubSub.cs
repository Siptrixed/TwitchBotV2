using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TwitchBotV2.Model.Twitch.EventArgs;
using TwitchBotV2.Model.Utils;
using WebSocketSharp;

namespace TwitchBotV2.Model.Twitch
{
    public class TwitchPubSub
    {
        #region Initialization
        private WebSocket WSock = new WebSocket("wss://pubsub-edge.twitch.tv/v1");
        public TwitchPubSub(string channelID)
        {
            ChannelID = channelID;
            BindEventsToWSock();
        }
        private void BindEventsToWSock()
        {
            WSock.OnClose += WSock_Closed;
            WSock.OnMessage += WSock_MessageReceived;
            WSock.OnOpen += WSock_Opened;
        }
        #endregion

        #region PublicLogic
        public event EventHandler<RewardEventArgs>? RewardRedeemed;
        public void Connect()
        {
            HoldСonnection = true;
            WSock.Connect();
        }
        public void Reconnect()
        {
            HoldСonnection = true;
            if (!WSock.IsAlive)
            {
                WSock = new WebSocket("wss://pubsub-edge.twitch.tv/v1");
                BindEventsToWSock();
                WSock.Connect();
            }
        }
        public void Disconnect()
        {
            HoldСonnection = false;
            if (WSock.IsAlive) WSock.Close();
        }
        #endregion

        #region SocketLogic
        public bool IsAlive => WSock.IsAlive;
        public string ChannelID;

        private bool HoldСonnection = true;
        private void WSock_Opened(object? sender, System.EventArgs e)
        {
            WSock.Send(GeneratePING());
            WSock.Send(GenerateSendJSON(new List<string>() { $"community-points-channel-v1.{ChannelID}" }));
        }

        private void WSock_MessageReceived(object? sender, MessageEventArgs e)
        {
            var data = new ParsedJson(e.Data);
            switch (data["type"].ToString())
            {
                case "PING":
                    WSock.Send(GeneratePING(true));
                    break;
                case "RECONNECT":
                    WSock.Close();
                    break;
                case "MESSAGE":
                    var ddat = data["data"];
                    if(ddat["topic"] == $"community-points-channel-v1.{ChannelID}")
                    {
                        var reward = new ParsedJson(ddat["message"]);
                        if(reward["type"] == "reward-redeemed")
                        {
                            var rewdata = reward["data"]["redemption"];
                            var rewarg = new RewardEventArgs(rewdata["user"]["display_name"], 
                                    rewdata["reward"]["id"], rewdata["user"]["id"], 
                                    rewdata["id"], rewdata["channel_id"], rewdata["reward"]["title"], rewdata["user_input"], rewdata);
                            RewardRedeemed?.Invoke(this, rewarg);
                        }
                    }
                    break;
            }
        }

        private void WSock_Closed(object? sender, CloseEventArgs e)
        {
            if (!HoldСonnection) return;
            WSock = new WebSocket("wss://pubsub-edge.twitch.tv/v1");
            BindEventsToWSock();
            WSock.Connect();
        }
        #endregion

        private string GeneratePING(bool pong = false)
        {
            DynamicObject obj = DynamicObject.CreateObject();
            obj["type"] = pong?"PONG":"PING";
            return obj.ToJSON();
        }
        private string GenerateSendJSON(List<string> topics, string type = "LISTEN")
        {
            DynamicObject obj = DynamicObject.CreateObject();
            obj["type"] = type;
            obj["nonce"] = Ramdomizer.RandomBase64String(30);
            var data = DynamicObject.CreateObject();
            var twtopics = DynamicObject.CreateArray();
            foreach (string topic in topics)
            {
                twtopics[-1] = topic;
            }
            data["topics"] = twtopics;
            obj["data"] = data;
            return obj.ToJSON();
        }

    }
}
