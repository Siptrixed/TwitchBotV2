using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Speech.Synthesis;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Windows.Media;

namespace TwitchBotV2.Model.Utils
{
    public static class AudioHelper
    {
        public static MediaPlayer Player;
        public static int MediaDurationMs;
        public static SpeechSynthesizer SpeechSynth = new SpeechSynthesizer();

        static AudioHelper()
        {
            MyAppExt.InvokeUI(() =>
            {
                Player = new MediaPlayer();
                Player.MediaOpened += Player_MediaOpened;
            });
        }

        private static void Player_MediaOpened(object? sender, EventArgs e)
        {
            MediaDurationMs = (int)(Player.NaturalDuration.HasTimeSpan ? Player.NaturalDuration.TimeSpan.TotalMilliseconds : 0);
        }

        public static void TextToSpeech(string Text)
        {
            new Task(() =>
            {
                if (SpeechSynth == null)
                {
                    SpeechSynth = new SpeechSynthesizer();
                }
                SpeechSynth.SpeakAsync(Text);
            }).Start();
        }

        public static float RateToSpeed()
        {
            if (SpeechSynth.Rate == 0)
                return 1;
            else if (SpeechSynth.Rate > 0)
            {
                return (float)(1 + (SpeechSynth.Rate * 0.1));
            }
            else
            {
                return (float)(1 - ((SpeechSynth.Rate * -1d) / 20d));
            }
        }

        public static void GetTrueTTSReady(string text, string voice = "alena", string emotion = "good", bool lastInvoke = false)
        {
            DynamicObject setts = DynamicObject.CreateObject();
            setts["message"] = text;
            setts["language"] = "ru-RU";
            setts["speed"] = RateToSpeed().ToString().Replace(",", ".");
            setts["voice"] = voice;
            setts["emotion"] = emotion;
            setts["format"] = "lpcm";
            string Setts = setts.ToJSON();

            byte[] byteArray = Encoding.UTF8.GetBytes(Setts);
            HttpWebRequest reqGetUser = (HttpWebRequest)WebRequest.Create("https://cloud.yandex.ru/api/speechkit/tts");
            reqGetUser.Accept = "application/json, text/plain, */*";
            reqGetUser.ContentType = "application/json";
            reqGetUser.Method = "POST";
            reqGetUser.Headers["x-csrf-token"] = GlobalModel.Settings.YandexToken;
            reqGetUser.Headers["Accept-Encoding"] = "gzip, deflate, br";
            reqGetUser.Headers["Accept-Language"] = "ru-RU,ru;q=0.8,en-US;q=0.5,en;q=0.3";
            reqGetUser.Headers["Cookie"] = @"XSRF-TOKEN=" + HttpUtility.UrlEncode(GlobalModel.Settings.YandexToken);
            reqGetUser.ContentLength = byteArray.Length;
            Stream dataStream = reqGetUser.GetRequestStream();
            dataStream.Write(byteArray, 0, byteArray.Length);
            dataStream.Close();
            WebResponse response;
            try
            {
                response = reqGetUser.GetResponse();
                Stream receiveStream = response.GetResponseStream();
                string FileX = Path.GetTempPath() + "/TwitchBot-TrueTTS.wav";
                if (File.Exists(FileX)) File.Delete(FileX);
                using (FileStream tempfile = new FileStream(FileX, FileMode.OpenOrCreate))
                {
                    WriteWavHeader(tempfile, false, 1, 16, 48000, -1);
                    CopyStream(receiveStream, tempfile);
                }
                TrueTTSReady = true;
            }
            catch (WebException ex)
            {
                //Console.WriteLine(Web.GetResponse(ex.Response));
                try
                {
                    var cookie = ex.Response.Headers["Set-Cookie"];
                    Match MTH = Regex.Match(cookie, @"XSRF-TOKEN=([^;]*);");
                    if (MTH.Success)
                    {
                        GlobalModel.Settings.YandexToken = HttpUtility.UrlDecode(MTH.Groups[1].Value);
                        if (lastInvoke) TrueTTSReady = false;
                        else
                        {
                            Thread.Sleep(100);
                            GetTrueTTSReady(text, voice, emotion, true);
                        }
                    }
                    else
                        TrueTTSReady = false;
                }
                catch (Exception e)
                {
                    TrueTTSReady = false;
                }
            }
        }
        static bool TrueTTSReady = false;
        public static void TrueTTS(string Text)
        {
            string path = Path.GetTempPath() + "/TwitchBot-TrueTTS.wav";
            if (!TrueTTSReady || !File.Exists(path))
            {
                TextToSpeech($"{Text}");
                do
                {
                    Thread.Sleep(100);
                } 
                while (SpeechSynth.State == SynthesizerState.Speaking);
                return;
            }
            MyAppExt.InvokeUI(() =>
            {
                //Uri File = new Uri(path, UriKind.Absolute);
                /*if (!MySave.Current.Bools[0])
                {
                    return;
                }*/
                Player.Open(new Uri("./TwitchBot-TrueTTS.wav", UriKind.Relative));
                Player.Open(new Uri(path, UriKind.Absolute));
                //Player.Volume = MySave.Current.Nums[4] / 100d;
                Player.Play();
            });
            Thread.Sleep(1000);
            Thread.Sleep(MediaDurationMs);
            MyAppExt.InvokeUI(() =>
            {
                Player.Close();
            });
            if (File.Exists(path)) File.Delete(path);
            TrueTTSReady = false;
        }
        public static void ClearTrueTTSFiles()
        {
            string path = Path.GetTempPath() + "/TwitchBot-TrueTTS.wav";
            if (File.Exists(path)) File.Delete(path);
            TrueTTSReady = false;
        }

        public static void CopyStream(Stream input, Stream output)
        {
            byte[] buffer = new byte[8 * 1024];
            int len;
            while ((len = input.Read(buffer, 0, buffer.Length)) > 0)
            {
                output.Write(buffer, 0, len);
            }
        }
        public static void WriteWavHeader(Stream stream, bool isFloatingPoint, ushort channelCount, ushort bitDepth, int sampleRate, int totalSampleCount)
        {
            stream.Position = 0;

            // RIFF header.
            // Chunk ID.
            stream.Write(Encoding.ASCII.GetBytes("RIFF"), 0, 4);

            // Chunk size.
            stream.Write(BitConverter.GetBytes(((bitDepth / 8) * totalSampleCount) + 36), 0, 4);

            // Format.
            stream.Write(Encoding.ASCII.GetBytes("WAVE"), 0, 4);



            // Sub-chunk 1.
            // Sub-chunk 1 ID.
            stream.Write(Encoding.ASCII.GetBytes("fmt "), 0, 4);

            // Sub-chunk 1 size.
            stream.Write(BitConverter.GetBytes(16), 0, 4);

            // Audio format (floating point (3) or PCM (1)). Any other format indicates compression.
            stream.Write(BitConverter.GetBytes((ushort)(isFloatingPoint ? 3 : 1)), 0, 2);

            // Channels.
            stream.Write(BitConverter.GetBytes(channelCount), 0, 2);

            // Sample rate.
            stream.Write(BitConverter.GetBytes(sampleRate), 0, 4);

            // Bytes rate.
            stream.Write(BitConverter.GetBytes(sampleRate * channelCount * (bitDepth / 8)), 0, 4);

            // Block align.
            stream.Write(BitConverter.GetBytes((ushort)channelCount * (bitDepth / 8)), 0, 2);

            // Bits per sample.
            stream.Write(BitConverter.GetBytes(bitDepth), 0, 2);



            // Sub-chunk 2.
            // Sub-chunk 2 ID.
            stream.Write(Encoding.ASCII.GetBytes("data"), 0, 4);

            // Sub-chunk 2 size.
            stream.Write(BitConverter.GetBytes((bitDepth / 8) * totalSampleCount), 0, 4);
        }
    }
}
