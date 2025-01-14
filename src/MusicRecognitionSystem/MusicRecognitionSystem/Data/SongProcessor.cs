using MathNet.Numerics.IntegralTransforms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace MusicRecognitionSystem.Data
{
    internal class SongProcessor
    {
        public static int CHUNK_SIZE = 4096;
        public static int OVERLAPING_SIZE = CHUNK_SIZE / 2; //50% overlapping

        public AudioFile audioFile;
        public Complex[][] songFrequencies;
        public int chunkCount;
        public int chunkSize;
        public int overlapSize;
        public int windowsCount;

        public SongProcessor(AudioFile audiofile)  //sound from file processing
        {
            this.audioFile = audiofile;
            this.chunkCount = audiofile.data.Length / CHUNK_SIZE;
            this.chunkSize = CHUNK_SIZE;
            this.overlapSize = OVERLAPING_SIZE;
            this.windowsCount = (audioFile.data.Length - chunkSize) / overlapSize + 1;
            this.songFrequencies = new Complex[windowsCount][];


            //adds song if not exists to database (dupplicates not allowed)
            CRUDManager.addSong(audiofile.name);
        }

        public SongProcessor(byte[] audioChunk) //sound from microphone processing
        {
            this.audioFile = new AudioFile("RecordingChunk", audioChunk);
            this.chunkCount = 1;
            this.chunkSize = audioChunk.Length;
            this.overlapSize = this.chunkSize / 2;
            this.windowsCount = 1;
            this.songFrequencies = new Complex[chunkCount][];
        }

        public void getFrequencies()
        {
            //iterating through overlaping windows
            for (int i = 0; i < windowsCount; i++)
            {
                Complex[] complex = new Complex[chunkSize];

                //iterating through chunk
                for (int j = 0; j < chunkSize; j++)
                {
                    complex[j] = new Complex(audioFile.data[j + i * overlapSize], 0); //for each chunk, create complex array based on each bit value
                }

                Fourier.Forward(complex, FourierOptions.Matlab);
                songFrequencies[i] = complex;
            }
        }
    }
}
