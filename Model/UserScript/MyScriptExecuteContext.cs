using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TwitchBotV2.Model.Twitch;
using TwitchBotV2.Model.Twitch.EventArgs;

namespace TwitchBotV2.Model.UserScript
{
    public class MyScriptExecuteContext
    {
        public Dictionary<string, string> StringVars = new Dictionary<string, string>();
        public RewardEventArgs? Redeemption;
        public MessageRecivedEventArgs? Message;
        public bool IsCanceled = false;
        public string? MsgId => Message?.ID;

        public MyScriptExecuteContext(RewardEventArgs redeem)
        {
            Redeemption = redeem;
            StringVars["text"] = redeem.Text;
            StringVars["title"] = redeem.Title;
            StringVars["nick"] = redeem.NickName;
            StringVars["channel"] = redeem.Chanel;
        }
        public MyScriptExecuteContext(MessageRecivedEventArgs message)
        {
            Message = message;
            StringVars["text"] = message.Message;
            StringVars["nick"] = message.NickName;
            StringVars["channel"] = message.Chanel;
        }
    }
}
