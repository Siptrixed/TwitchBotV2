using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TwitchBotV2.Model.Twitch;
using TwitchBotV2.Model.Twitch.EventArgs;
using TwitchBotV2.Model.Utils;

namespace TwitchBotV2.Model.UserScript.Actions
{
    [MessagePack.MessagePackObject(true)]
    public class DelayAction : MyScriptActionBase
    {
        public int Delay { get; set; }
        public DelayAction(int delay, MyScriptActionType type = MyScriptActionType.Timer)
        {
            Delay = delay;
            Type = type;
        }
        public override void Invoke(TwitchClient client, MyScriptExecuteContext context)
        {
            switch (Type)
            {
                case MyScriptActionType.Timer:
                    Thread.Sleep(Delay);
                    break;
            }
        }

        public override string ToString()
        {
            switch (Type)
            {
                case MyScriptActionType.Timer: return $"Ждать {MyAppExt.GetTimeFromMilliseconds(Delay)}";
            }
            return base.ToString();
        }
    }
}
