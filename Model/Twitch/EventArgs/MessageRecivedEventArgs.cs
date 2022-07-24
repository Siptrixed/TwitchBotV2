using TwitchBotV2.Model.Utils;

namespace TwitchBotV2.Model.Twitch.EventArgs
{
    public class MessageRecivedEventArgs
    {
        public string NickName, Message, ID, Chanel, UserID, CustomRewardID;
        public DynamicObject Details;
        public MessageRecivedEventArgs(string nickName, string message, string userid, string id, string chanel, string customRewardId, DynamicObject details)
        {
            NickName = nickName;
            Message = message;
            ID = id;
            Chanel = chanel;
            UserID = userid;
            Details = details;
            CustomRewardID = customRewardId;
        }
    }
}
