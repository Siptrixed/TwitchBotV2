using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TwitchBotV2.Model.UserScript;

namespace TwitchBotV2.Model.DataModel
{
    [MessagePack.MessagePackObject(true)]
    public class MyRewardInfo
    {
        public string ID { get; set; }
        public string Title { get; set; }
        public string Prompt { get; set; }
        public int Cost { get; set; }
        public bool RequaredApply { get; set; }
        public MyCallableUserScript Script { get; set; } = new MyCallableUserScript();
        
    }
}
