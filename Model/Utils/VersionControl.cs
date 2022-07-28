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

namespace TwitchBotV2.Model.Utils
{
    public static class VersionControl
    {
        public const string AppRepository = "Siptrixed/TwitchBotV2";
        public const bool UpdateAfterClosing = false;


        public static string? Version => System.Reflection.Assembly.GetExecutingAssembly().GetName()?.Version?.ToString();
#pragma warning disable CS0162 // Обнаружен недостижимый код
        static VersionControl()
        {
            if (UpdateAfterClosing)
            {

                MyAppExt.InvokeUI(() => {
                    Application.Current.Exit += (x, y) => UpdateNow(true);
                });
            }
            if (File.Exists("Update.vbs")) File.Delete("Update.vbs");
            if (File.Exists("Update.zip")) File.Delete("Update.zip");
        }
#pragma warning restore CS0162 // Обнаружен недостижимый код

        public static async Task<VersionInfo?> CheckVersion(bool silentUpdate = false)
        {
#if !DEBUG
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("User-Agent", $"{AppRepository.Replace("/",".")} VersionControl");
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
                                    return NewVer;
                                }
                            }
                        }
                    }
                }
            }
#endif
            return null;
        }

        public static async Task<bool> DownloadUpdateAsync(VersionInfo version)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    var data = await client.GetStreamAsync(version.DownloadURL);
                    using (var stream = File.Create("Update.zip"))
                    {
                        data.CopyTo(stream);
                    }
                }
                return true;
            }
            catch { return false; }
        }

        public static bool UpdateNow(bool isClosing = false)
        {
            if (File.Exists("Update.zip"))
            {
                FileStream Batch = File.Create("Update.vbs");
                var appfile = ObjectFileSystem.AppFile;
                if (Path.GetExtension(appfile) == ".dll")
                {
                    appfile = appfile.Replace(".dll", ".exe");
                }
                string FirstPart = $"Dim path,zip,runfilename\r\nzip  = \"{ObjectFileSystem.AppDirectory}\\Update.zip\"\r\npath = \"{ObjectFileSystem.AppDirectory}\\\"\r\nrunfilename = \"{appfile}\"\r\n";
                string OpenAppPart = "\r\nSet WshShell = WScript.CreateObject(\"WScript.Shell\")\r\nWshShell.Run runfilename";
                string SecondPart = $"Set fso = CreateObject(\"Scripting.FileSystemObject\")\r\nOn Error Resume Next\r\nDo\r\nErr.Clear\r\nSet oFile = fso.OpenTextFile(runfilename, 8, False)\r\nif err.number = 0 then Exit Do\r\nWScript.Sleep 100\r\nLoop\r\nOn Error Goto 0\r\noFile.Close\r\n\r\n\r\nSet file = fso.GetFile(zip)\r\nSet dest = fso.GetFolder(path)\r\n\r\nset objShell = CreateObject(\"Shell.Application\")\r\nset zip_content = objShell.NameSpace(file.path).Items   \r\nset destnsmps = objShell.NameSpace(dest.path)\r\n\r\nfor i = 0 to zip_content.count-1\r\n    if (fso.FileExists(fso.Buildpath(path,zip_content.item(i).name))) then\r\n        fso.DeleteFile(fso.Buildpath(path,zip_content.item(i).name))\r\n    end if\r\n    destnsmps.copyHere(zip_content.item(i))\r\nnext\r\n{(isClosing?"": OpenAppPart)}\r\nOn Error GoTo 0";
                byte[] Data = Encoding.Default.GetBytes($"{FirstPart}\r\n{SecondPart}");
                Batch.Write(Data, 0, Data.Length);
                Batch.Close();
                try
                {
                    Process scriptProc = new Process();
                    scriptProc.StartInfo.FileName = @"cscript";
                    scriptProc.StartInfo.WorkingDirectory = $"{ObjectFileSystem.AppDirectory}\\"; //<---very important 
                    scriptProc.StartInfo.Arguments = "//B //Nologo Update.vbs";
                    scriptProc.StartInfo.CreateNoWindow = true;
                    scriptProc.Start();
                    if(!isClosing)MyAppExt.InvokeUI(() => Application.Current.Shutdown());
                    return true;
                }
                catch (Exception ex)
                {
                    return false;
                }
            }
            return false;
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
        private static bool CheckVersion(string newest, string? current)
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
