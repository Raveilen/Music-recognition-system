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
        public static int CHUNK_SIZE = 2048;
        public static int OVERLAPING_SIZE = CHUNK_SIZE / 2; //50% overlapping

        public AudioFile audioFile;
        public int overlapSize;

        public List<Double[]> spectrogram;
        public byte[] audioBytes;

        public SongProcessor(AudioFile audiofile)  //sound from file processing
        {
            this.audioFile = audiofile;
            this.overlapSize = OVERLAPING_SIZE;
            audioBytes = audiofile.data;


            //adds song if not exists to database (dupplicates not allowed)
            CRUDManager.addSong(audiofile.name);
        }

        public SongProcessor(byte[] audioChunk) //sound from microphone processing
        {
            this.audioFile = new AudioFile("RecordingChunk", audioChunk);
            this.overlapSize = audioChunk.Length / 2;
            this.audioBytes = audioChunk;
        }

        private float[] ConvertBytesToSamples(byte[] audio)
        {
            //we consider 16 bit audio
            float[] samples = new float[audio.Length / 2];

            for (int i = 0; i < samples.Length; i++)
            {
                short sample = BitConverter.ToInt16(audio, i * 2); //convert to 16bit
                samples[i] = sample / 32768f; //normalize to [-1, 1] float value
            }

            return samples;
        }

        public void computeSpectrogram()
        {
            //SPLIT SAMPLES INTO WINDOWS WITH OVERLAP

            float[] samples = ConvertBytesToSamples(audioBytes); //extract samples from bytes

            List<float[]> windows = new List<float[]>(); //windows (chunks) of audio samples with overlap
            int stepSize = CHUNK_SIZE - OVERLAPING_SIZE; //how much bytes to move window each time

            for(int start = 0; start + CHUNK_SIZE <= samples.Length; start += stepSize)
            {
                float[] window = new float[CHUNK_SIZE]; //create window
                Array.Copy(samples, start, window, 0, CHUNK_SIZE); //copy correct samples range to window
                
                //Apply Hamming window function
                var hamming = MathNet.Numerics.Window.Hamming(window.Length);
                for(int i = 0; i < CHUNK_SIZE; i++)
                    window[i] = window[i] * (float)hamming[i];

                windows.Add(window); //add window to list
            }

            //Callculate Magnitude
            List<double[]> linearScaleSpectrogram = new List<double[]>(); //song spectrogram

            foreach (var window in windows)
            {
                //for each window separately we present it as list of Complex values, calculates and ads calculated from fft magnitude values as our spectrogram part
                //spectrogram consist of list of magnitude arrays for each window
                
                //generate complex
                Complex[] complexSpectrum = window.Select(x => new Complex(x, 0)).ToArray();

                //apply fft
                Fourier.Forward(complexSpectrum, FourierOptions.Matlab);

                double[] magnitudeSpectrum = complexSpectrum.Select(c => c.Magnitude).ToArray();

                linearScaleSpectrogram.Add(magnitudeSpectrum);
            }

            //convert spectrogram to logaritmic scale
            //logaritmic scale is used to better represent human hearing

            const double epsilon = 1e-6; //epsilon is needed to avoid log(0) which is undefined

            //initialize final sectrogram variable
            spectrogram = new List<Double[]>();

            foreach (double[] magnitudeSpectrum in linearScaleSpectrogram)
            {
                double[] logSpectrum = magnitudeSpectrum.Select(m => 20 * Math.Log10(m + epsilon)).ToArray();
                spectrogram.Add(logSpectrum);
            }

        }
    }
}
