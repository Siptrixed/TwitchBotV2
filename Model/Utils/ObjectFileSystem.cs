using MessagePack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace TwitchBotV2.Model.Utils
{
    public static class ObjectFileSystem
    {
        public static string AppFile => $"{AppContext.BaseDirectory}TwitchBotV2.exe";
        public static string? AppDirectory => Path.GetDirectoryName(AppContext.BaseDirectory);
        public static bool TrySaveObjToFile<T>(string filename, T obj)
        {
            try
            {
                if (File.Exists(filename)) File.Delete(filename);
                using (Stream stream = File.Create(filename))
                    MessagePackSerializer.Serialize(stream, obj);
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }
        public static T TryReadObjFromFile<T>(string filename, out bool success)
        {
            try
            {
                if (File.Exists(filename))
                {
                    T Obj;
                    using (Stream stream = File.OpenRead(filename))
                        Obj = MessagePackSerializer.Deserialize<T>(stream);
                    success = true;
                    return Obj;
                }
                else
                {
                    success = false;
                    return default;
                }
            }
            catch
            {
                success = false;
                return default;
            }
        }
        public static bool SaveObject<T>(string name, T obj, bool CreateBackup = false)
        {
            var saveFile = $"{name}.bin";
            var backUp = $"{name}.bak";
            if(CreateBackup && File.Exists(saveFile)) File.Move(saveFile, backUp);
            return TrySaveObjToFile(saveFile, obj);
        }
        public static T LoadObject<T>(string name)
        {
            var saveFile = $"{name}.bin";
            var backUp = $"{name}.bak";
            T Obj = TryReadObjFromFile<T>(saveFile, out bool success);
            if (!success) Obj = TryReadObjFromFile<T>(backUp, out success);
            return Obj;
        }
    }
}
