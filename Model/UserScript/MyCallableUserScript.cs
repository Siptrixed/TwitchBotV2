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
        public void Invoke(TwitchClient client, MyScriptExecuteContext context)
        {
            Task.Run(() =>
            {
                Queue.Add(context);
                Thread.Sleep(150);
                if (IsAsync)
                {
                    Invoker(client, context);
                }
                else lock (this)
                    {
                        Invoker(client, context);
                    }
                Queue.Remove(context);
            });
        }
        [MessagePack.IgnoreMember]
        public static List<MyScriptExecuteContext> Queue = new List<MyScriptExecuteContext>();
        private void Invoker(TwitchClient client, MyScriptExecuteContext context)
        {
            if (context.IsCanceled) return;
            foreach (var act in Actions)
            {
                if (context.IsCanceled) return;
                act.Invoke(client, context);
            }
        }
    }
}
