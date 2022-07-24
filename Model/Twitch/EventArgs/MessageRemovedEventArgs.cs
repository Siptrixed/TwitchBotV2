using TwitchBotV2.Model.Twitch.Enums;
using TwitchBotV2.Model.Utils;

namespace TwitchBotV2.Model.Twitch.EventArgs
{
    public class MessageRemovedEventArgs
    {
        public string Channel, NickName, MessageID;
        public MessageRemoveTypeEnum Type;
        public DynamicObject Details;
        public MessageRemovedEventArgs(string channel, string nickName, DynamicObject details, MessageRemoveTypeEnum type = MessageRemoveTypeEnum.RemoveAll)
        {
            Channel = channel;
            NickName = nickName;
            Type = type;
            MessageID = "";
            Details = details;
        }
        public MessageRemovedEventArgs(string channel, string nickName, string MsgID, DynamicObject details, MessageRemoveTypeEnum type = MessageRemoveTypeEnum.RemoveOne)
        {
            NickName = nickName;
            Channel = channel;
            Type = type;
            MessageID = MsgID;
            Details = details;
        }
    }
}
