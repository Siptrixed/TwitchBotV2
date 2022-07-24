using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using TwitchBotV2.Model.Twitch.EventArgs;
using TwitchBotV2.Model.Twitch.Utils;

namespace TwitchBotV2.Model.Twitch
{
    public class TwitchClient
    {
        public TwitchAccount Account;
        public TwitchPubSub PubSub;
        public TwitchIRC TwitchIRC;
        public static async Task<TwitchClient> Construct(string token)
        {
            TwitchClient Client = new TwitchClient();
            Client.Account = await TwitchAccount.Construct(token);
            Client.TwitchIRC = new TwitchIRC(Client.Account, Client.Account.Login);
            Client.PubSub = new TwitchPubSub(Client.Account.UserID);//await Client.Account.GetStreamerIDAsync(Client.Account.Login)
            Client.Init();
            return Client;
        }
        private void Init()
        {
            PubSub.RewardRedeemed += (x, y) => RewardRedeemed?.Invoke(x, y);
            TwitchIRC.MessageRecived += (x, y) => MessageRecived?.Invoke(x, y);
            TwitchIRC.MessageRemoved += (x, y) => MessageRemoved?.Invoke(x, y);
            PubSub.Connect();
            TwitchIRC.Connect();
            GlobalModel.Settings.TwitchToken = Account.Token;
            GlobalModel.TwithcClientInitialized?.Invoke(this, Account.UserID != "");
            
        }
        public void SendMessage(string text) => TwitchIRC.SendMessage(text);
        public EventHandler<RewardEventArgs>? RewardRedeemed;
        public EventHandler<MessageRecivedEventArgs>? MessageRecived;
        public EventHandler<MessageRemovedEventArgs>? MessageRemoved;
    }
}
