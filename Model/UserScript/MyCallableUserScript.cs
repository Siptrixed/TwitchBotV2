using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TwitchBotV2.Model.Twitch;
using TwitchBotV2.Model.Twitch.EventArgs;
using TwitchBotV2.Model.UserScript.Actions;

namespace TwitchBotV2.Model.UserScript
{
    [MessagePack.MessagePackObject(true)]
    public class MyCallableUserScript
    {
        public bool IsEmpty => Actions.Count == 0;
        public List<MyScriptActionBase> Actions { get; set; } = new List<MyScriptActionBase>();
        public bool IsAsync { get; set; } = false;
        public void Invoke(TwitchClient client)
        {
            Task.Run(() =>
            {
                if (IsAsync)
                {
                    Invoker(client);
                }
                else lock (this)
                    {
                        Invoker(client);
                    }
            });
        }
        [MessagePack.IgnoreMember]
        public RewardEventArgs Redeem;
        private void Invoker(TwitchClient client)
        {
            foreach (var act in Actions)
            {
                act.Invoke(this,client);
            }
        }
    }
}
