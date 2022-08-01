using TwitchBotV2.Model.Utils;

namespace TwitchBotV2.Model.Twitch.EventArgs
{
    public class RewardEventArgs
    {
        public string NickName, ID, Chanel, UserID, CustomRewardID, Title, Text;
        public ParsedJson Details;
        public RewardEventArgs(string nickName, string crid, string userid, string id, string chanel, string title, string text, ParsedJson details)
        {
            NickName = nickName;
            ID = id;
            Chanel = chanel;
            UserID = userid;
            CustomRewardID = crid;
            Title = title;
            Text = text;
            Details = details;
        }
    }
}
