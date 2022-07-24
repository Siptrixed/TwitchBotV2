using MessagePack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwitchBotV2.Model.Twitch
{
    public class TwitchAccount
    {
        public string UserID { get; set; } = "";
        public string Login { get; set; } = "justinfan99999";
        public string Token { get; set; } = "SCHMOOPIIE";
        public static async Task<TwitchAccount> Construct(string token)
        {
            TwitchAccount @new = new TwitchAccount();
            @new.Token = token;
            if (!await @new.ValidateAsync())
            {
                @new.Login = "justinfan99999";
                //@new.Token = "SCHMOOPIIE";
                @new.UserID = "";
            }
            return @new;
        }
    }
}
