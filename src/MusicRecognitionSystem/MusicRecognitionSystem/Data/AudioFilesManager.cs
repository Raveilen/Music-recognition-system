using Azure.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MusicRecognitionSystem.Data;
using NAudio.Wave;

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
        public static int SAMPLING_RATE = 44100;  //44.1kHz
        public static int BITS_PER_SAMPLE = 16; //16-bit PCM (depth)
        public static int CHANNELS = 1; //Mono

        public static string pathToMainDirectory = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.FullName;
        public static string pathToAudioFiles = Path.Combine(pathToMainDirectory, "AudioFiles");

        public static string[] audioFiles; //full paths for files
        public static string[] audioNames; //names of audio files

        public static AudioFile LoadAudioFile(int index)
        {
            //extract song name
            string extension = Path.GetExtension(audioNames[index]);
            string songName = audioNames[index].Substring(0, audioNames[index].Length - extension.Length);

            //load mp3 file with specified parameters
            using (var mp3Reader = new Mp3FileReader(audioFiles[index]))
            {
                var waveFormat = new WaveFormat(SAMPLING_RATE, BITS_PER_SAMPLE, CHANNELS);

                using (var conversionStream = new WaveFormatConversionStream(waveFormat, mp3Reader))
                {
                    using (var memoryStream = new MemoryStream())
                    {
                        conversionStream.CopyTo(memoryStream);
                        byte[] buffer = memoryStream.ToArray();
                        return new AudioFile(songName, buffer);
                    }
                }
            }
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
