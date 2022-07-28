using MessagePack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using TwitchBotV2.Model.Twitch;
using TwitchBotV2.Model.Twitch.EventArgs;

namespace TwitchBotV2.Model.UserScript.Actions
{
    [Union(0, typeof(TextActions))]
    [Union(1, typeof(DelayAction))]
    [Union(2, typeof(AudioActions))]
    [MessagePackObject(true)]
    public abstract class MyScriptActionBase
    {
        public MyScriptActionType Type { get; set; }

        public abstract void Invoke(MyCallableUserScript context, TwitchClient client, RewardEventArgs Redeem);

        public override string ToString()
        {
            return $"Action ({Type})";
        }
        
        internal string ComposeText(MyCallableUserScript context, string text, RewardEventArgs Redeem)
        {
            foreach(Match match in Regex.Matches(text, @"\{([\.\w]*)\}"))
            {
                if (match.Success)
                {
                    var key = match.Groups[1].Value;
                    if(RedeemAvailableVariables.ContainsKey(key))
                        text = text.Replace($"{{{key}}}", RedeemAvailableVariables[key].Invoke(Redeem));
                }
            }
            return text;
        }
        internal static readonly Dictionary<string, Func<RewardEventArgs,string>> RedeemAvailableVariables = new Dictionary<string, Func<RewardEventArgs, string>>() {
            { "date", (x)=> DateTime.Now.ToShortDateString() },
            { "redeem.text", (x)=> x.Text },
            { "redeem.user", (x)=> x.NickName },
            { "reward.title", (x)=> x.Title },
            { "reward.channel", (x)=> x.Chanel },
        };
    }
}
