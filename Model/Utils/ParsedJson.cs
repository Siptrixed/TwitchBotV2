using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace TwitchBotV2.Model.Utils
{
    public class ParsedJson
    {
        public JsonElement? RootElement = null;
        public ParsedJson this[string key]
        {
            get
            {
                if (RootElement.HasValue && RootElement.Value.TryGetProperty(key, out JsonElement value))
                {
                    return new ParsedJson(value);
                }
                return new ParsedJson();
            }
        }

        public ParsedJson() { }
        public ParsedJson(JsonElement? json) => RootElement = json;
        public ParsedJson(string json) => RootElement = JsonDocument.Parse(json).RootElement;

        public static implicit operator bool(ParsedJson json) => json.RootElement.HasValue && json.RootElement.Value.ValueKind == JsonValueKind.True;
        public static implicit operator byte(ParsedJson json)
        {
            if (json.RootElement.HasValue && json.RootElement.Value.ValueKind == JsonValueKind.Number && json.RootElement.Value.TryGetByte(out byte value)) return value;
            else return 0;
        }
        public static implicit operator sbyte(ParsedJson json)
        {
            if (json.RootElement.HasValue && json.RootElement.Value.ValueKind == JsonValueKind.Number && json.RootElement.Value.TryGetSByte(out sbyte value)) return value;
            else return 0;
        }
        public static implicit operator short(ParsedJson json)
        {
            if (json.RootElement.HasValue && json.RootElement.Value.ValueKind == JsonValueKind.Number && json.RootElement.Value.TryGetInt16(out short value)) return value;
            else return 0;
        }
        public static implicit operator ushort(ParsedJson json)
        {
            if (json.RootElement.HasValue && json.RootElement.Value.ValueKind == JsonValueKind.Number && json.RootElement.Value.TryGetUInt16(out ushort value)) return value;
            else return 0;
        }
        public static implicit operator int(ParsedJson json)
        {
            if (json.RootElement.HasValue && json.RootElement.Value.ValueKind == JsonValueKind.Number && json.RootElement.Value.TryGetInt32(out int value)) return value;
            else return 0;
        }
        public static implicit operator uint(ParsedJson json)
        {
            if (json.RootElement.HasValue && json.RootElement.Value.ValueKind == JsonValueKind.Number && json.RootElement.Value.TryGetUInt32(out uint value)) return value;
            else return 0;
        }
        public static implicit operator long(ParsedJson json)
        {
            if (json.RootElement.HasValue && json.RootElement.Value.ValueKind == JsonValueKind.Number && json.RootElement.Value.TryGetInt64(out long value)) return value;
            else return 0;
        }
        public static implicit operator ulong(ParsedJson json)
        {
            if (json.RootElement.HasValue && json.RootElement.Value.ValueKind == JsonValueKind.Number && json.RootElement.Value.TryGetUInt64(out ulong value)) return value;
            else return 0;
        }

        public static implicit operator float(ParsedJson json)
        {
            if (json.RootElement.HasValue && json.RootElement.Value.ValueKind == JsonValueKind.Number && json.RootElement.Value.TryGetSingle(out float value)) return value;
            else return 0;
        }
        public static implicit operator double(ParsedJson json)
        {
            if (json.RootElement.HasValue && json.RootElement.Value.ValueKind == JsonValueKind.Number && json.RootElement.Value.TryGetDouble(out double value)) return value;
            else return 0;
        }
        public static implicit operator decimal(ParsedJson json)
        {
            if (json.RootElement.HasValue && json.RootElement.Value.ValueKind == JsonValueKind.Number && json.RootElement.Value.TryGetDecimal(out decimal value)) return value;
            else return 0;
        }

        public static implicit operator string(ParsedJson json) => json.ToString()??"";
        public static implicit operator byte[](ParsedJson json)
        {
            if (json.RootElement.HasValue && json.RootElement.Value.ValueKind == JsonValueKind.String && json.RootElement.Value.TryGetBytesFromBase64(out byte[]? value)) return value;
            return new byte[0];
        }

        public static implicit operator DateTime(ParsedJson json)
        {
            if (json.RootElement.HasValue && json.RootElement.Value.ValueKind == JsonValueKind.String && json.RootElement.Value.TryGetDateTime(out DateTime value)) return value;
            return DateTime.MinValue;
        }
        public static implicit operator DateTimeOffset(ParsedJson json)
        {
            if (json.RootElement.HasValue && json.RootElement.Value.ValueKind == JsonValueKind.String && json.RootElement.Value.TryGetDateTimeOffset(out DateTimeOffset value)) return value;
            return DateTimeOffset.MinValue;
        }
        public static implicit operator Guid(ParsedJson json)
        {
            if (json.RootElement.HasValue && json.RootElement.Value.ValueKind == JsonValueKind.String && json.RootElement.Value.TryGetGuid(out Guid value)) return value;
            return Guid.Empty;
        }

        public static implicit operator List<ParsedJson>(ParsedJson json) => json.List();
        public List<ParsedJson> List()
        {
            List<ParsedJson> Parsed = new List<ParsedJson>();
            if (RootElement.HasValue && RootElement.Value.ValueKind == JsonValueKind.Array)
            {
                foreach (var el in RootElement.Value.EnumerateArray())
                {
                    Parsed.Add(new ParsedJson(el));
                }
            }
            return Parsed;
        }
        public Dictionary<string, ParsedJson> Dictionary()
        {
            Dictionary<string, ParsedJson> Parsed = new Dictionary<string, ParsedJson>();
            if (RootElement.HasValue && RootElement.Value.ValueKind == JsonValueKind.Object)
            {
                foreach (var el in RootElement.Value.EnumerateObject())
                {
                    Parsed.Add(el.Name, new ParsedJson(el.Value));
                }
            }
            return Parsed;
        }
        public override string? ToString()
        {
            if (RootElement.HasValue && RootElement.Value.ValueKind == JsonValueKind.String)
            {
                return RootElement.Value.GetString();
            }
            return RootElement?.GetRawText();
        }
    }
}
