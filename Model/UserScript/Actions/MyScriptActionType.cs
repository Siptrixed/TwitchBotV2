using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwitchBotV2.Model.UserScript.Actions
{
    public enum MyScriptActionType
    {
        Timer, 
        SendMessage, 
        PlayAudio, 
        Speech,
        ShellComand,
        SpeechTrueTTS
    }
}
