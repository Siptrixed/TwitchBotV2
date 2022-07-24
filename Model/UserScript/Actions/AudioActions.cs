using System;
using System.Collections.Generic;
using System.Linq;
using System.Speech.Synthesis;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TwitchBotV2.Model.Twitch;
using TwitchBotV2.Model.Utils;

namespace TwitchBotV2.Model.UserScript.Actions
{
    [MessagePack.MessagePackObject(true)]
    public class AudioActions : MyScriptActionBase
    {
        public string Text { get; set; }
        public byte Volume { get; set; } = 50;
        public int Rate { get; set; } = 0;
        public TrueTTSVoices Voice { get; set; } = TrueTTSVoices.alena;
        public string Emotion { get; set; }
        public AudioActions(string text, MyScriptActionType type = MyScriptActionType.PlayAudio)
        {
            Text = text;
            Type = type;
        }
        public override void Invoke(MyCallableUserScript context, TwitchClient client)
        {
            switch (Type)
            {
                case MyScriptActionType.Speech:
                    lock (AudioHelper.SpeechSynth)
                    {
                        MyAppExt.InvokeUI(() =>
                        {
                            AudioHelper.SpeechSynth.Rate = Rate;
                            AudioHelper.SpeechSynth.Volume = Volume;
                        });
                        AudioHelper.TextToSpeech(ComposeText(context, Text));
                        do
                        {
                            Thread.Sleep(100);
                        }
                        while (AudioHelper.SpeechSynth.State == SynthesizerState.Speaking);
                    }
                    break;
                case MyScriptActionType.SpeechTrueTTS:
                    lock (AudioHelper.SpeechSynth)
                    {
                        MyAppExt.InvokeUI(() => {
                            AudioHelper.SpeechSynth.Rate = Rate;
                            AudioHelper.Player.Volume = Volume / 100d;
                        });
                        var text = ComposeText(context, Text);
                        AudioHelper.GetTrueTTSReady(text, Voice.ToString());
                        AudioHelper.TrueTTS(text);
                    }
                    break;
                case MyScriptActionType.PlayAudio:
                    lock (AudioHelper.SpeechSynth)
                    {
                        MyAppExt.InvokeUI(() =>
                        {
                            AudioHelper.Player.Volume = Volume / 100d;
                            AudioHelper.Player.Open(new Uri(Text, UriKind.Absolute));
                            AudioHelper.Player.Play();
                        });
                        Thread.Sleep(1200);
                        Thread.Sleep(AudioHelper.MediaDurationMs);
                        MyAppExt.InvokeUI(()=> AudioHelper.Player.Close());
                    }
                    break;
            }
        }

        public override string? ToString()
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
