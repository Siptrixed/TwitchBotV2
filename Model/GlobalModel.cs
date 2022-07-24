using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using TwitchBotV2.Model.Utils;

namespace TwitchBotV2.Model
{
    public static class GlobalModel
    {
        public static EventHandler<string>? TwithcAccountAuthorized;
        public static EventHandler<bool>? TwithcClientInitialized;
        public static Settings Settings = new Settings();
        public static DateTime StartTime = DateTime.Now;
        public const string SettingsFile = "Settings.bin";
        static GlobalModel()
        {
            Settings = ObjectFileSystem.TryReadObjFromFile<Settings>(SettingsFile, out bool success);
            if (!success) Settings = new Settings();
            Application.Current.Exit += Current_Exit;
        }

        private static void Current_Exit(object sender, ExitEventArgs e)
        {
            if (e.ApplicationExitCode == 0) ObjectFileSystem.TrySaveObjToFile(SettingsFile, Settings);
        }
    }
}
