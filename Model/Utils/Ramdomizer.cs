using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwitchBotV2.Model.Utils
{
    public static class Ramdomizer
    {
        private static readonly Random random = new Random();
        private const string ASCII = " !\"#$%&'()*+,-./0123456789:;<=>?@ABCDEFGHIJKLMNOPQRSTUVWXYZ[\\]^_`abcdefghijklmnopqrstuvwxyz{|}~";
        private const string Base64 = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/";
        public static Random Rand => random;
        public static string RandomASCIIString(int length) => RandomStringGenerator(ASCII, length);
        public static string RandomBase64String(int length) => RandomStringGenerator(Base64, length);
        private static string RandomStringGenerator(string chars, int length) => new string(Enumerable.Repeat(chars, length).Select(s => s[random.Next(s.Length)]).ToArray());
    }
}
