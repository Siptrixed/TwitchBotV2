using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TwitchBotV2.Model.Utils;

namespace TwitchBotV2.Model.Twitch.Utils
{
    public static class TwitchProprietaryData
    {
        public static List<DynamicObject> ParseIRCSocketMessages(string raw)
        {
            List<DynamicObject> parsed = new List<DynamicObject>();
            var messages = raw.Split("\r\n");
            foreach(var message in messages)
            {
                if (string.IsNullOrWhiteSpace(message)) continue;
                parsed.Add(ParseSingleSocketMessage(message));
            }
            return parsed;
        }
        private static DynamicObject ParseSingleSocketMessage(string raw)
        {
            DynamicObject parsed = DynamicObject.CreateObject();
            int index = 0;
            if(raw[index] == '@')
            {
                int endIdx = raw.IndexOf(' ');
                string RawTagsComponent = raw.Substring(1, endIdx - 1);
                parsed["tags"] = ParseTagsComponent(RawTagsComponent);
                index = endIdx + 1;
            }
            if (raw[index] == ':')
            {
                index += 1;
                int endIdx = raw.IndexOf(' ', index);
                string RawSourceComponent = raw.Substring(index, endIdx - index);
                parsed["source"] = ParseSourceComponent(RawSourceComponent);
                index = endIdx + 1;
            }

            int endIndex = raw.IndexOf(':', index); 
            if (-1 == endIndex) endIndex = raw.Length;
            string RawComandComponent = raw.Substring(index, endIndex - index).Trim();
            parsed["command"] = ParseComandComponent(RawComandComponent);

            if (endIndex != raw.Length)
            {
                int idx = endIndex + 1;
                string RawParametersComponent = raw.Substring(idx);
                parsed["parameters"] = RawParametersComponent;
            }

            return parsed;
        }
        private static DynamicObject ParseTagsComponent(string raw)
        {
            DynamicObject parsed = DynamicObject.CreateObject();
            foreach (var tag in raw.Split(';'))
            { 
                var splitedTag = tag.Split('=');
                string tagKey = splitedTag.Length > 0 ? splitedTag[0] : "";
                string tagValue = splitedTag.Length > 1 ? splitedTag[1] : "";
                if (tagKey == "") continue;

                switch (tagKey)
                {
                    case "badges":
                    case "badge-info":
                        if (tagValue != "")
                        {
                            DynamicObject parsedTagbadges = DynamicObject.CreateObject();
                            foreach (var pair in tagValue.Split(','))
                            {
                                var badgeParts = pair.Split('/');
                                if (badgeParts.Length > 1)
                                {
                                    parsedTagbadges[badgeParts[0]] = badgeParts[1];
                                }
                            }
                            parsed[tagKey] = parsedTagbadges;
                        }
                        break;
                    case "emote-sets":
                        var emoteSetIds = tagValue.Split(',');
                        DynamicObject parsedTagEmotes = DynamicObject.CreateArray();
                        foreach(var ems in emoteSetIds) parsedTagEmotes[-1] = ems;
                        parsed[tagKey] = parsedTagEmotes;
                        break;
                    default:
                        parsed[tagKey] = tagValue;
                        break;
                }
            }
            return parsed;
        }
        private static DynamicObject ParseSourceComponent(string raw)
        {
            DynamicObject parsed = DynamicObject.CreateObject();
            var sourceParts = raw.Split('!');
            if (sourceParts.Length > 1)
            {
                parsed["nick"] = sourceParts[0];
                parsed["host"] = sourceParts[1];
            }
            else if (sourceParts.Length > 0)
            {
                parsed["host"] = sourceParts[0];
            }
            return parsed;
        }
        private static DynamicObject ParseComandComponent(string raw)
        {
            DynamicObject parsed = DynamicObject.CreateObject();
            try
            {
                var commandParts = raw.Split(" ");
                if (commandParts.Length > 0)
                    switch (commandParts[0])
                    {
                        case "JOIN":
                        case "PART":
                        case "NOTICE":
                        case "CLEARCHAT":
                        case "CLEARMSG":
                        case "HOSTTARGET":
                        case "PRIVMSG":
                        case "USERSTATE":
                        case "ROOMSTATE":
                        case "001":  // Logged in (successfully authenticated). 
                            parsed["command"] = commandParts[0];
                            parsed["channel"] = commandParts[1];
                            break;
                        case "GLOBALUSERSTATE":
                        case "PING":
                        case "RECONNECT":
                            parsed["command"] = commandParts[0];
                            break;
                        case "CAP":
                            parsed["command"] = commandParts[0];
                            parsed["isCapRequestEnabled"] = (commandParts[2] == "ACK") ? true : false;
                            break;
                        case "421":
                            parsed["command"] = commandParts[0];
                            parsed["unsupported"] = commandParts[2];
                            break;
                        default:
                            parsed["command"] = commandParts[0];
                            DynamicObject comandParts = DynamicObject.CreateArray();
                            foreach (var cp in commandParts) comandParts[-1] = cp;
                            parsed["parts"] = comandParts;
                            break;
                    }
            }
            catch (Exception e)
            {
                parsed["command"] = "EXCEPTION";
                parsed["message"] = e.Message;
            }
            return parsed;
        }


    }
}
