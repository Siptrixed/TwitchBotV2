using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace TwitchBotV2.Model
{
    public static class MyAppExt
    {
        public static void InvokeUI(Action act) => _ = Application.Current?.Dispatcher?.BeginInvoke(DispatcherPriority.Background, act);
        public static void RunCMD(string scripd)
        {
            Process cmd = new Process();
            cmd.StartInfo.FileName = "cmd.exe";
            cmd.StartInfo.RedirectStandardInput = true;
            cmd.StartInfo.RedirectStandardOutput = true;
            cmd.StartInfo.CreateNoWindow = true;
            cmd.StartInfo.UseShellExecute = false;
            cmd.Start();

            cmd.StandardInput.WriteLine(scripd);
            cmd.StandardInput.Flush();
            cmd.StandardInput.Close();
            cmd.WaitForExit();
        }
        public static int TrueRandom(int min, int max) => TrueRandom(min, max, 1)[0];
        public static int[] TrueRandom(int min, int max, int count = 1)
        {
            List<int> vs = new List<int>();
            if (min > max)
            {
                int p = max;
                max = min;
                min = p;
            }
            if (max > min)
                using (var client = new System.Net.WebClient())
                {
                    client.Encoding = Encoding.UTF8;
                    string lol = client.DownloadString($"https://www.random.org/integers/?num={count}&min={min}&max={max}&col=1&base=10&format=plain&rnd=new");
                    foreach (string num in lol.Split('\n'))
                    {
                        if (string.IsNullOrEmpty(num)) continue;
                        int.TryParse(num.Trim(), out int ret);
                        while (vs.Contains(ret))
                        {
                            ret++;
                            if (ret > max && max - min >= count)
                                ret = min;
                        }
                        vs.Add(ret);
                    }
                }
            else
            {
                for (int s = 0; s < count; s++)
                {
                    while (vs.Contains(min))
                    {
                        min++;
                        //if (min > max && max - min >= count)
                        //   min = min;
                    }
                    vs.Add(min);
                }
            }

            return vs.ToArray();
        }

        public static string GetTimeFromMilliseconds(int milliseconds, bool allowEmpty = false)
        {
            if (milliseconds == 0 && allowEmpty) return "";
            if (milliseconds < 1000)
            {
                return $"{milliseconds}ms";
            }
            if (milliseconds < 60000)
            {
                return $"{milliseconds / 1000}s {GetTimeFromMilliseconds(milliseconds % 1000,true)}";
            }
            if (milliseconds < 3600000)
            {
                return $"{milliseconds / 60000:N0}min {GetTimeFromMilliseconds(milliseconds % 60000,true)}";
            }
            return $"{milliseconds / 3600000:N0}h {GetTimeFromMilliseconds(milliseconds % 3600000,true)}";
        }
    }
}
