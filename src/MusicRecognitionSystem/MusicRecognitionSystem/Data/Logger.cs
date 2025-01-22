using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.Marshalling;
using System.Text;
using System.Threading.Tasks;

namespace MusicRecognitionSystem.Data
{
    class Logger
    {
        public static string HashlogPath = "HashLog.txt";
        private static StreamWriter Hashlog;

        public static void OpenLog()
        {
            Hashlog = new StreamWriter(HashlogPath);
        }

        public static void LogSong(string songName)
        {
            Hashlog.WriteLine($"Song added: {songName}");
            Hashlog.Flush();
        }

        public static void LogHash(int hashValue)
        {
            Hashlog.WriteLine($"Hash added: {hashValue}");
            Hashlog.Flush();
        }

        public static void LogTimestamp(int hashValue, string songName, int chunkNumber)
        {
            Hashlog.WriteLine($"New Timestamp:  Chunk no. {chunkNumber} Song {songName} Hash {hashValue}");
            Hashlog.Flush();
        }

        public static void Log(string message)
        {
            Hashlog.WriteLine(message);
            Hashlog.Flush();
        }

        public static void CloseLog()
        {
            Hashlog.Close();
        }

    }
}
