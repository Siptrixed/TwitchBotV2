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

        public abstract void Invoke(TwitchClient client, MyScriptExecuteContext context);

        public override string ToString()
        {
            return $"Action ({Type})";
        }
        
        internal string ComposeText(MyScriptExecuteContext context, string text)
        {
            foreach(Match match in Regex.Matches(text, @"\{([\.\w]*)\}"))
            {
                if (match.Success)
                {
                    var key = match.Groups[1].Value;
                    if(RedeemAvailableVariables.ContainsKey(key))
                        text = text.Replace($"{{{key}}}", RedeemAvailableVariables[key].Invoke(context));
                    else if(context.StringVars.ContainsKey(key))
                        text = text.Replace($"{{{key}}}", context.StringVars[key]);
                }
            }
            return text;
        }
        internal static readonly Dictionary<string, Func<MyScriptExecuteContext, string>> RedeemAvailableVariables = new Dictionary<string, Func<MyScriptExecuteContext, string>>() {
            { "date", (x)=> DateTime.Now.ToShortDateString() }
        };
    }
}
