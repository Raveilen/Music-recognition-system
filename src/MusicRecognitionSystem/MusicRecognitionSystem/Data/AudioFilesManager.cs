using Azure.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicRecognitionSystem.Data
{
    internal class AudioFile
    {
        public string name;
        public byte[] data;

        public AudioFile(string name, byte[] data)
        {
            this.name = name;
            this.data = data;
        }
    }

    internal class AudioFileManager
    {   
        public static string pathToMainDirectory = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.FullName;
        public static string pathToAudioFiles = Path.Combine(pathToMainDirectory, "AudioFiles");

        public static string[] audioFiles; //full paths for files
        public static string[] audioNames; //names of audio files

        public static AudioFile LoadAudioFile(int index)
        {
            return new AudioFile(audioNames[index], File.ReadAllBytes(audioFiles[index]));
        }

        public static void LoadSongsMetadata()
        {
            audioFiles = Directory.GetFiles(pathToAudioFiles, "*.mp3", SearchOption.AllDirectories);
            audioNames = new string[audioFiles.Length];

            for (int i=0; i < audioFiles.Length; i++)
            {
                audioNames[i] = Path.GetFileName(audioFiles[i]);
            }
        }
    }
}
