using MathNet.Numerics.IntegralTransforms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace MusicRecognitionSystem.Data
{
    internal class SongProcessor
    {
        public static int CHUNK_SIZE = 4096;

        public AudioFile audioFile;
        public Complex[][] songFrequencies;
        public int chunkCount;
        
        public SongProcessor(AudioFile audiofile)
        {
            this.audioFile = audiofile;
            this.chunkCount = audiofile.data.Length / CHUNK_SIZE;
            this.songFrequencies = new Complex[chunkCount][];
        }

        public void getSongFrequencies()
        {
            for (int i = 0; i < chunkCount; i++)
            {
                Complex[] complex = new Complex[CHUNK_SIZE];

                for (int j = 0; j < CHUNK_SIZE; j++)
                {
                    complex[j] = new Complex(audioFile.data[j + i * CHUNK_SIZE], 0);
                }

                Fourier.Forward(complex, FourierOptions.Matlab);
                songFrequencies[i] = complex;
            }
        }
    }
}
