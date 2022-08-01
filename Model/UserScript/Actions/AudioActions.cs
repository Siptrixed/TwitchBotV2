using System;
using System.Collections.Generic;
using System.Linq;
using System.Speech.Synthesis;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TwitchBotV2.Model.Twitch;
using TwitchBotV2.Model.Twitch.EventArgs;
using TwitchBotV2.Model.Utils;

namespace TwitchBotV2.Model.UserScript.Actions
{
    [MessagePack.MessagePackObject(true)]
    public class AudioActions : MyScriptActionBase
    {
        public string Text { get; set; }
        public byte Volume { get; set; } = 50;
        public int Rate { get; set; } = 0;
        public TrueTTSVoices Voice { get; set; } = TrueTTSVoices.@default;
        public AudioActions(string text, MyScriptActionType type = MyScriptActionType.PlayAudio)
        {
            Text = text;
            Type = type;
        }
        public override void Invoke(TwitchClient client, MyScriptExecuteContext context)
        {
            switch (Type)
            {
                case MyScriptActionType.Speech:
                    lock (AudioHelper.SpeechSynth)
                    {
                        AudioHelper.CurrentPlayingMessage = context.Message?.ID??"";
                        MyAppExt.InvokeUI(() =>
                        {
                            AudioHelper.SpeechSynth.Rate = (int)((Rate * 0.7 + GlobalModel.Settings.DefaultRate * 1.3) / 2);
                            AudioHelper.SpeechSynth.Volume = (int)((Volume * 0.7 + GlobalModel.Settings.DefaultVolume * 1.3) / 2);
                        })?.Wait();
                        AudioHelper.TextToSpeech(ComposeText(context, Text));
                        do
                        {
                            Thread.Sleep(100);
                        }
                        while (AudioHelper.SpeechSynth.State == SynthesizerState.Speaking);
                        AudioHelper.CurrentPlayingMessage = "";
                    }
                    break;
                case MyScriptActionType.SpeechTrueTTS:
                    lock (AudioHelper.SpeechSynth)
                    {
                        AudioHelper.CurrentPlayingMessage = context.Message?.ID ?? "";
                        MyAppExt.InvokeUI(() => {
                            AudioHelper.SpeechSynth.Rate = (int)((Rate * 0.7 + GlobalModel.Settings.DefaultRate * 1.3) / 2);// : GlobalModel.Settings.DefaultRate;
                            AudioHelper.Player.Volume = (Volume * 0.7 + GlobalModel.Settings.DefaultVolume * 1.3) / 200d;
                        })?.Wait();
                        var text = ComposeText(context, Text);
                        AudioHelper.GetTrueTTSReady(text, Voice == TrueTTSVoices.@default? GlobalModel.Settings.DefaultVoice.ToString() : Voice.ToString());
                        AudioHelper.TrueTTS(text);
                        AudioHelper.CurrentPlayingMessage = "";
                    }
                    break;
                case MyScriptActionType.PlayAudio:
                    lock (AudioHelper.SpeechSynth)
                    {
                        MyAppExt.InvokeUI(() =>
                        {
                            AudioHelper.Player.Volume = (Volume  * 0.7 + GlobalModel.Settings.DefaultVolume * 1.3) / 200d;
                            AudioHelper.Player.Open(new Uri(Text, UriKind.Absolute));
                            AudioHelper.Player.Play();
                        })?.Wait();
                        Thread.Sleep(1000);
                        do
                        {
                            Thread.Sleep(100);
                        }
                        while (AudioHelper.PlayerIsPlaying);
                        MyAppExt.InvokeUI(()=> AudioHelper.Player.Close());
                    }
                    break;
            }
        }

        public override string ToString()
        {
            switch (Type)
            {
                case MyScriptActionType.Speech: return $"Синтез речи '{Text}'";
                case MyScriptActionType.SpeechTrueTTS: return $"Озвучить '{Text}'";
                case MyScriptActionType.PlayAudio: return $"Воспроизвести '{Text}'";
            }
            return base.ToString();
        }
    }
}
