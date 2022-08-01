using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TwitchBotV2.Model.Twitch;
using TwitchBotV2.Model.Twitch.EventArgs;
using TwitchBotV2.Model.Utils;

namespace TwitchBotV2.Model.UserScript.Actions
{
    [MessagePack.MessagePackObject(true)]
    public class TextActions : MyScriptActionBase
    {
        public TextActions(string text, MyScriptActionType type = MyScriptActionType.SendMessage)
        {
            Text = text;
            Type = type;
        }
        public string Text { get; set; }
        public override void Invoke(TwitchClient client, MyScriptExecuteContext context)
        {
            switch (Type)
            {
                case MyScriptActionType.SendMessage:
                    client.SendMessage(ComposeText(context, Text));
                    break;
                case MyScriptActionType.ShellComand:
                    MyAppExt.RunCMD(ComposeText(context, Text));
                    break;
            }
        }

        public override string ToString()
        {
            switch (Type)
            {
                case MyScriptActionType.SendMessage: return $"Написать '{Text}'";
                case MyScriptActionType.ShellComand: return $"Выполнить '{Text}'";
            }
            return base.ToString();
        }
    }
}
