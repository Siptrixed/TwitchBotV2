using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TwitchBotV2.Model.DataModel;
using TwitchBotV2.Model.WinApi;

namespace TwitchBotV2.Model
{
    [MessagePack.MessagePackObject(keyAsPropertyName: true)]
    public class Settings
    {
        public string? TwitchToken { get; set; }
        public Dictionary<string, MyRewardInfo> CustomRewards { get; set; } = new Dictionary<string, MyRewardInfo>();
        public bool MinimizeToTray { get; set; }
        public string YandexToken { get; set; }
        public WinHotkeyData? ISSHotkey { get; set; }
    }
}
