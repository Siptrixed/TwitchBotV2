using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwitchBotV2.Model.Twitch.Utils
{
    public static class TwitchConsts
    {
        public const string ClientID = "eehovpakxnf9dib7eyyaevrd8a0jv0";
        public static readonly List<string> RequaredScopes = new List<string>() { "channel:moderate", "chat:edit", "chat:read", "channel:edit:commercial", "channel:read:redemptions", "channel:manage:redemptions", "channel:read:polls", "channel:manage:polls", "channel:manage:raids", "channel:manage:broadcast", "channel:read:subscriptions", "moderation:read", "moderator:read:chat_settings", "moderator:manage:chat_settings" };
        public static string GetAuthURL() => $"https://id.twitch.tv/oauth2/authorize?response_type=token&client_id={ClientID}&redirect_uri=http://localhost:8181/Auth/Token&scope={String.Join("+", RequaredScopes)}&state={Guid.NewGuid()}";
    }
}
