using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using TwitchBotV2.Model.Utils;

namespace TwitchBotV2.Model
{
    public static class VersionControl
    {
        public const string AppRepository = "Siptrixed/TwitchBotV2";
        public static string? Version => System.Reflection.Assembly.GetExecutingAssembly().GetName()?.Version?.ToString();
        public static async Task<VersionInfo?> CheckVersion(bool silentUpdate = false)
        {
            
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("User-Agent", "SiptrixedAppVersionChecker");
                string GitHubData = await client.GetStringAsync($"https://api.github.com/repos/{AppRepository}/releases");
                var Json = new ParsedJson(GitHubData);
                var AllReleases = Json.List();
                if(AllReleases.Count > 0)
                {
                    var LastRelease = AllReleases[0];
                    string NewestVersion = LastRelease["tag_name"];
                    if (CheckVersion(NewestVersion,Version))
                    {
                        var NewVer = new VersionInfo();
                        NewVer.Version = NewestVersion;
                        foreach (var asset in LastRelease["assets"].List())
                        {
                            if (asset["content_type"].ToString() == "application/x-zip-compressed")
                            {
                                NewVer.Name = asset["name"];
                                if (CheckIsCorrectOS(NewVer.Name)) 
                                {
                                    NewVer.DownloadURL = asset["browser_download_url"];
                                    if (silentUpdate)
                                    {
                                        if (File.Exists("Updater.exe"))
                                        {
                                            RunUpdater();
                                        }
                                        else
                                        {
                                            WebClient web = new WebClient();
                                            web.DownloadFileAsync(new Uri("https://github.com/ScriptedEngineer/AutoUpdater/releases/download/1.0.0.2/Updater.exe"), "Updater.exe");
                                            web.DownloadFileCompleted += (x, e) => RunUpdater();
                                        }
                                    }
                                    return NewVer;
                                }
                            }
                        }
                    }
                }
            }
            return null;
        }

        public static void RunUpdater(string updater = "Updater.exe")
        {
            Process.Start(updater, $"/DarkTheme /GitHub \"{AppRepository}\" /RunApp \"{ObjectFileSystem.AppFile}\" /JustDownload");
            Application.Current.Shutdown();
        }
        private static bool CheckIsCorrectOS(string PackName)
        {
            PackName = PackName.ToLower();
            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                if (!PackName.Contains("win")) return false;
                if (RuntimeInformation.OSArchitecture == Architecture.X86)
                {
                    if (!PackName.Contains("x86") && !PackName.Contains("32bit")) return false;
                }
                return true;
            }
            else
            {
                return false;
            }
        }
        public static bool CheckVersion(string newest, string? current)
        {
            if (string.IsNullOrEmpty(current)) return true;
            string[] VFc = current.Split('.');
            string[] VFl = newest.Split('.');
            bool oldVer = false;
            for (int i = 0; i < VFc.Length; i++)
            {
                if (VFc[i] != VFl[i])
                {
                    int.TryParse(VFc[i], out int VFci);
                    int.TryParse(VFl[i], out int VFli);
                    if (VFli > VFci) oldVer = true;
                }
            }
            return oldVer;
        }
        public class VersionInfo
        {
            public string Version { get; set; } = VersionControl.Version??"0.0.0.0";
            public string Name { get; set; } = "Undefined";
            public string DownloadURL { get; set; } = "";

        }
    }
}
