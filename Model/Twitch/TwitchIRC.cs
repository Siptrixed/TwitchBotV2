using System;
using System.Collections.Generic;
using TwitchBotV2.Model.Twitch.EventArgs;
using TwitchBotV2.Model.Twitch.Utils;
using WebSocketSharp;

namespace TwitchBotV2.Model.Twitch
{
    public class TwitchIRC
    {
        #region Initialization
        private WebSocket WSock = new WebSocket("wss://irc-ws.chat.twitch.tv/");
        public TwitchAccount Account;
        public TwitchIRC(TwitchAccount account, string channel)
        {
            Channel = channel;
            Account = account;
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
        public event EventHandler<MessageRecivedEventArgs>? MessageRecived;
        public event EventHandler<MessageRemovedEventArgs>? MessageRemoved;
        public void SendMessage(string text)
        {
            if(Account.UserID != "") WSockSend("PRIVMSG #" + Channel + " :" + text);
        }
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
                WSock = new WebSocket("wss://irc-ws.chat.twitch.tv/");
                BindEventsToWSock();
                WSock.Connect();
            }
        }
        public void Disconnect()
        {
            HoldСonnection = false;
            if(WSock.IsAlive) WSock.Close();
        }
        #endregion

        #region SocketLogic
        public bool IsAlive => WSock.IsAlive;
        public string Channel;

        private bool HoldСonnection = true;
        private List<string> Delayed = new List<string>();
        private bool Logined = false;
        
        private void WSockSend(string Data)
        {
            if (Logined)
            {
                WSock.Send(Data);
            }
            else
            {
                Delayed.Add(Data);
            }
        }
        private void WSock_Opened(object? sender, System.EventArgs e)
        {
            WSock.Send("CAP REQ :twitch.tv/tags twitch.tv/commands");
            WSock.Send("PASS oauth:" + Account.Token);
            WSock.Send("NICK " + Account.Login);
            WSock.Send("USER " + Account.Login + " 8 * :" + Account.Login);
        }

        private void WSock_MessageReceived(object? sender, MessageEventArgs e)
        {
            var parsed = TwitchProprietaryData.ParseIRCSocketMessages(e.Data);
            foreach (var comand in parsed)
            {
                switch(comand["command"]["command"].ToString())
                {
                    case "PING":
                        WSock.Send("PONG");
                        break;
                    case "CLEARCHAT":
                        MessageRemoved?.Invoke(this, 
                            new MessageRemovedEventArgs(comand["command"]["channel"], comand["parameters"], comand));
                        break;
                    case "CLEARMSG":
                        MessageRemoved?.Invoke(this, 
                            new MessageRemovedEventArgs(comand["command"]["channel"], 
                            comand["tags"]["login"], comand["tags"]["target-msg-id"], comand));
                        break;
                    case "PRIVMSG":
                        MessageRecived?.Invoke(this, 
                            new MessageRecivedEventArgs(comand["source"]["nick"], comand["parameters"], 
                                comand["tags"]["user-id"], comand["tags"]["id"], comand["command"]["channel"],comand["tags"]["custom-reward-id"], comand));
                        break;
                    case "001":
                        WSock.Send("JOIN #" + Channel);
                        Logined = true;
                        foreach (string send in Delayed)
                        {
                            WSock.Send(send);
                        }
                        Delayed.Clear();
                        break;
                    case "RECONNECT":
                        Logined = false; 
                        WSock.Close(); 
                        break;
                }
            }
        }

        private void WSock_Closed(object? sender, CloseEventArgs e)
        {
            Logined = false;
            if (!HoldСonnection) return;
            WSock = new WebSocket("wss://irc-ws.chat.twitch.tv/");
            BindEventsToWSock();
            WSock.Connect();
        }
        #endregion

        
    }
}
