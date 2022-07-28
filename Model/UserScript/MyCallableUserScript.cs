using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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
        public void Invoke(TwitchClient client, RewardEventArgs Redeem)
        {
            Task.Run(() =>
            {
                Queue.Add(Redeem);
                if (IsAsync)
                {
                    Invoker(client, Redeem);
                }
                else lock (this)
                    {
                        Invoker(client, Redeem);
                    }
                Queue.Remove(Redeem);
            });
        }
        [MessagePack.IgnoreMember]
        public static List<RewardEventArgs> Queue = new List<RewardEventArgs>();
        private void Invoker(TwitchClient client, RewardEventArgs Redeem)
        {
            if (Redeem.IsRemoved) return;
            foreach (var act in Actions)
            {
                act.Invoke(this, client, Redeem);
            }
        }
    }
}
