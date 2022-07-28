using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace TwitchBotV2.Model.WinApi
{
    [MessagePack.MessagePackObject(keyAsPropertyName: true)]
    public class WinHotkeyData
    {
        public Key Key { get; set; }
        public KeyModifier Mod { get; set; }
        public WinHotkeyData()
        {

        }
        public override string ToString()
        {
            return $"{(Mod != KeyModifier.None ? $"{Mod}+" : "")}{Key}";
        }
    }
}
