using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;

namespace TwitchBotV2.Model.Utils
{
    public class DynamicObject
    {
        public object Value;
        public DynamicObject this[string key]
        {
            get
            {
                if (Value is Dictionary<string, DynamicObject> fields && fields.ContainsKey(key))
                {
                    return fields[key];
                }
                return null;
            }
            set
            {
                if (Value is Dictionary<string, DynamicObject> fields)
                {
                    if (fields.ContainsKey(key))
                    {
                        fields[key] = value;
                    }
                    else
                    {
                        fields.Add(key, value);
                    }
                }
            }
        }
        public DynamicObject this[int key]
        {
            get
            {
                if (Value is List<DynamicObject> array && key < array.Count && key >= 0)
                {
                    return array[key];
                }
                return null;
            }
            set
            {
                if (Value is List<DynamicObject> array)
                {
                    if (key >= 0)
                    {
                        if (key < array.Count)
                        {
                            array[key] = value;
                        }
                        else if (key == array.Count)
                        {
                            array.Add(value);
                        }
                        else
                        {
                            while (key > array.Count)
                            {
                                array.Add(null);
                            }
                            array.Add(value);
                        }
                    }
                    else if (key == -1)
                    {
                        ArrayAdd(value);
                    }
                }
            }
        }

        public DynamicObject(object value)
        {
            Value = value;
        }
        public static DynamicObject CreateArray() => new DynamicObject(new List<DynamicObject>());
        public void ArrayAdd(DynamicObject value)
        {
            if (Value is List<DynamicObject> array)
            {
                array.Add(value);
            }
        }
        public List<DynamicObject> List()
        {
            if (Value is List<DynamicObject> array) return array;
            return new List<DynamicObject>();
        }
        public Dictionary<string, DynamicObject> Dictionary()
        {
            if (Value is Dictionary<string, DynamicObject> array) return array;
            return new Dictionary<string, DynamicObject>();
        }
        public static DynamicObject CreateObject() => new DynamicObject(new Dictionary<string, DynamicObject>());

        public static implicit operator bool(DynamicObject obj) => obj.Value is bool blo && blo;
        public static implicit operator DynamicObject(bool obj) => new DynamicObject(obj);
        public static implicit operator byte(DynamicObject obj)
        {
            if (obj.Value is byte val) return val;
            else return 0;
        }
        public static implicit operator DynamicObject(byte obj) => new DynamicObject(obj);
        public static implicit operator sbyte(DynamicObject obj)
        {
            if (obj.Value is sbyte val) return val;
            else return 0;
        }
        public static implicit operator DynamicObject(sbyte obj) => new DynamicObject(obj);
        public static implicit operator short(DynamicObject obj)
        {
            if (obj.Value is short val) return val;
            else return 0;
        }
        public static implicit operator DynamicObject(short obj) => new DynamicObject(obj);
        public static implicit operator ushort(DynamicObject obj)
        {
            if (obj.Value is ushort val) return val;
            else return 0;
        }
        public static implicit operator DynamicObject(ushort obj) => new DynamicObject(obj);
        public static implicit operator int(DynamicObject obj)
        {
            if (obj.Value is int val) return val;
            else return 0;
        }
        public static implicit operator DynamicObject(int obj) => new DynamicObject(obj);
        public static implicit operator uint(DynamicObject obj)
        {
            if (obj.Value is uint val) return val;
            else return 0;
        }
        public static implicit operator DynamicObject(uint obj) => new DynamicObject(obj);
        public static implicit operator long(DynamicObject obj)
        {
            if (obj.Value is long val) return val;
            else return 0;
        }
        public static implicit operator DynamicObject(long obj) => new DynamicObject(obj);
        public static implicit operator ulong(DynamicObject obj)
        {
            if (obj.Value is ulong val) return val;
            else return 0;
        }
        public static implicit operator DynamicObject(ulong obj) => new DynamicObject(obj);

        public static implicit operator float(DynamicObject obj)
        {
            if (obj.Value is float val) return val;
            else return 0;
        }
        public static implicit operator DynamicObject(float obj) => new DynamicObject(obj);
        public static implicit operator double(DynamicObject obj)
        {
            if (obj.Value is double val) return val;
            else return 0;
        }
        public static implicit operator DynamicObject(double obj) => new DynamicObject(obj);
        public static implicit operator decimal(DynamicObject obj)
        {
            if (obj.Value is decimal val) return val;
            else return 0;
        }
        public static implicit operator DynamicObject(decimal obj) => new DynamicObject(obj);

        public static implicit operator string(DynamicObject obj) => obj.ToString();
        public static implicit operator DynamicObject(string obj) => new DynamicObject(obj);
        public static implicit operator DateTime(DynamicObject obj)
        {
            if (obj.Value is DateTime val) return val; 
            return DateTime.MinValue;
        }
        public static implicit operator DynamicObject(DateTime obj) => new DynamicObject(obj);
        public override string ToString() => Value?.ToString();
        public string ToJSON()
        {
            if (Value is Dictionary<string, DynamicObject> fields)
            {
                List<string> serialized = new List<string>();
                foreach (var keyval in fields)
                {
                    serialized.Add($"\"{keyval.Key}\":{keyval.Value.ToJSON()}");
                }
                if(serialized.Count > 0) return $"{{{string.Join(",", serialized)}}}";
                return "{}";
            }
            else if (Value is List<DynamicObject> array)
            {
                List<string> serialized = new List<string>();
                foreach (var val in array)
                {
                    serialized.Add(val.ToJSON());
                }
                if (serialized.Count > 0) return $"[{string.Join(",", serialized)}]";
                return "[]";
            }
            else
            {
                if (IsNumberOrBool(this)) return Value.ToString().ToLower().Replace(",",".");
                else return $"\"{Value}\"";
            }
        }
        public static DynamicObject FromJSON(string json)
        {
            var root = JsonDocument.Parse(json).RootElement;
            return FromJSON(root);
        }
        private static DynamicObject FromJSON(JsonElement elem)
        {
            switch (elem.ValueKind)
            {
                case JsonValueKind.Object:
                    var newobj = CreateObject();
                    foreach (var lis in elem.EnumerateObject())
                    {
                        newobj[lis.Name] = FromJSON(lis.Value);
                    }
                    return newobj;
                case JsonValueKind.Array:
                    var newarr = CreateArray();
                    foreach (var lis in elem.EnumerateArray())
                    {
                        newarr[-1] = FromJSON(lis);
                    }
                    return newarr;
                case JsonValueKind.Number: return new DynamicObject(elem.GetDouble());
                case JsonValueKind.True: return true;
                case JsonValueKind.False: return false;
                default: return elem.ToString();
            }
        }
        public bool ValueIsNumericOrBool() => IsNumberOrBool(this);
        public static bool IsNumberOrBool(DynamicObject obj)
        {
            if (obj.Value != null)
            {
                return obj.Value is bool || obj.Value is sbyte || obj.Value is byte || obj.Value is short || obj.Value is ushort
                    || obj.Value is int || obj.Value is uint || obj.Value is long || obj.Value is ulong
                    || obj.Value is float || obj.Value is double || obj.Value is decimal;
            }
            else return false;
        }
    }
}