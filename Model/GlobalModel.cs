using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using TwitchBotV2.Model.Utils;
using TwitchBotV2.Model.WinApi;

namespace TwitchBotV2.Model
{
    public static class GlobalModel
    {
        public static EventHandler<string>? TwithcAccountAuthorized;
        public static EventHandler<bool>? TwithcClientInitialized;
        public static Settings Settings = new Settings();
        public static DateTime StartTime = DateTime.Now;
        public const string SettingsFile = "Settings.bin";
        private static WinHotkey? ImmediatelyStopSound;
        static GlobalModel()
        {
            Settings = ObjectFileSystem.TryReadObjFromFile<Settings>(SettingsFile, out bool success);
            if (!success) Settings = new Settings();
            Application.Current.Exit += Current_Exit;
            if (Settings.ISSHotkey != null)
            {
                ImmediatelyStopSound = new WinHotkey(Settings.ISSHotkey.Key, Settings.ISSHotkey.Mod, StopSoundPlays);
            }
        }

        public static void ClearISSHotKey()
        {
            if (ImmediatelyStopSound != null)
            {
                ImmediatelyStopSound.Unregister();
                ImmediatelyStopSound.Dispose();
            }
            if (Settings.ISSHotkey != null)
            {
                Settings.ISSHotkey = null;
            }
        }
        public static void SetISSHotkey(Key key, KeyModifier mod)
        {
            if (ImmediatelyStopSound != null)
            {
                ImmediatelyStopSound.Unregister();
                ImmediatelyStopSound.Dispose();
            }
            if (Settings.ISSHotkey == null)
            {
                Settings.ISSHotkey = new WinHotkeyData();
            }
            Settings.ISSHotkey.Key = key;
            Settings.ISSHotkey.Mod = mod;
            ImmediatelyStopSound = new WinHotkey(key, mod, StopSoundPlays);
        }

        private static void StopSoundPlays(WinHotkey sender)
        {
            MyAppExt.InvokeUI(() =>
            {
                AudioHelper.StopPlayer();
                if (AudioHelper.SpeechSynth.State == System.Speech.Synthesis.SynthesizerState.Speaking)
                {
                    AudioHelper.SpeechSynth.SpeakAsyncCancelAll();
                }
                AudioHelper.ClearTrueTTSFiles();
            });
        }

        private static void Current_Exit(object sender, ExitEventArgs e)
        {
            if (e.ApplicationExitCode == 0) ObjectFileSystem.TrySaveObjToFile(SettingsFile, Settings);
        }
    }
}
