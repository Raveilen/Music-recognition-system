using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
using MusicRecognitionSystem.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace MusicRecognitionSystem.Data
{
    public class Peak
    {
        public int time { get; set; }
        public int frequency { get; set; }
        public double magnitude { get; set; }

        public Peak(int time, int frequency)
        {
            this.time = time;
            this.frequency = frequency;
        }

        public Peak(int time, int frequency, double magnitude) : this(time, frequency)
        {
            this.magnitude = magnitude;
        }
    }

    internal class HashManager
    {
        public static int LOWER_LIMIT = 41; //nie może być 40 bo według takiego algorytmu 40 paasowałoby jeszcze do poprzedniego przedziału.
        public static int UPPER_LIMIT = 300;
        
        //static thresholding
        public double THRESHOLD = 20.0; //defines border which tells if magnitude is high enough to be considered as a peak
        public int FANOUT = 5;

        //adaptive thresholding
        public double THRESHOLD_OFFSET = 10.0;

        //top N peaks
        public int N = 4;

        // Przedziały: 41-80, 81-120, 121-180, 181-300
        public static int[] RANGE = new int[] { 80, 120, 180, UPPER_LIMIT + 1 };

        public SongProcessor songProcessor;
        public List<Peak> peaks;

        public HashManager(SongProcessor songProcessor)
        {
            this.songProcessor = songProcessor;
            peaks = new List<Peak>();
        }

        public string generateHash(int chunkID)
        {
            double[] maxMagnitudes = { -1, -1, -1, -1, -1 };
            int[] maxFrequencies = new int[4];
            for (int i = LOWER_LIMIT; i < UPPER_LIMIT - 1; i++)
            {
                Complex frequency = songProcessor.songFrequencies[chunkID][i];
                double magnitude = Math.Log(Complex.Abs(frequency) + 1);

                int index = rangeSelect(i);
                if (index != -1) //checks if value in range
                {
                    if (magnitude > maxMagnitudes[index])
                    {
                        maxMagnitudes[index] = magnitude;
                        maxFrequencies[index] = i;
                    }
                }
                else
                {
                    throw new Exception("Attempt to process data from invalid range!");
                }
            }
            string songHash = computeHash(maxFrequencies);

            return songHash;
        }

        public void generateHashes()
        {
            //For each song chunk
            for (int i = 0; i < songProcessor.songFrequencies.Length; i++)
            {
                string songHash = generateHash(i);
                saveHashToDatabase(songHash, i);
            }
        }

        private void saveHashToDatabase(string songHash, int chunkNumber)
        {
            //If hash does not exist, adds it, else adds only SongHash record (dupplicate hashes not allowed)
            CRUDManager.addSongTimestamp(songProcessor.audioFile.name, songHash, chunkNumber);
        }

        private string computeHash(int[] maxfrequencies)
        {
            if(maxfrequencies.Length != 4)
            {
                throw new Exception("Invalid array length!");
            }

            //algorytm jest prosty w implementacji, ułatwia debugowanie i jest uniwersalny, ponieważ zwraca uwagę na kolejność przedziałów
            /* Hash do testów */
            /*return $"{maxFrequencies[0]} {maxFrequencies[1]} {maxFrequencies[2]} {maxFrequencies[3]}"; */
            return $"{maxfrequencies[0]}{maxfrequencies[1]}{maxfrequencies[2]}{maxfrequencies[3]}";
        }

        private static int rangeSelect(int frequencyPosition)
        {
            if (frequencyPosition < LOWER_LIMIT || frequencyPosition > UPPER_LIMIT)
            {
                return -1;
            }
            int index = 0;
            for (; frequencyPosition > RANGE[index]; index++);
            return index;
        }

        private static double ComputeMedian(double[] values) //mediana na potrzeby sąsiedztwa
        {
            Array.Sort(values);

            int midIndex = values.Length / 2;

            return values.Length % 2 != 0 ? values[midIndex] : (values[midIndex] + values[midIndex - 1]) / 2;
        }

        public void ApplyAdaptiveThresholding() //selects peaks based on adaptive thresholding
        {
            for(int t = 0; t < songProcessor.spectrogram.Count; t++) //time iterating
            {
                // Compute the median magnitude for the current time step
                double[] magnitudes = new double[songProcessor.spectrogram.Count];

                for (int f = 0; f < songProcessor.spectrogram[t].Length; f++) //frequency iterating
                    magnitudes[f] = songProcessor.spectrogram[t][f]; //upewnić się co do indeksów, czat mówi na odwrót, ale intuicja i copilot mówią inaczej (sprawdzić bo wygenerowaniu haszy czy efektywne, ewentualnie zamienić ze sobą)

                double median = ComputeMedian(magnitudes);
                double adaptiveThreshold = median + THRESHOLD_OFFSET;

                // Select peaks above the adaptive threshold
                for (int f = 0; f < songProcessor.spectrogram[t].Length; f++) //podobnie tutaj
                {
                    if (songProcessor.spectrogram[t][f] > adaptiveThreshold)
                    {
                        peaks.Add(new Peak(t, f, songProcessor.spectrogram[t][f]));
                    }
                }
            }
        }

        public void TopNPeaksOnly() //from thresholded peaks, selects only N peaks with highest magnitude
        {
            List<Peak> topNPeaks = new List<Peak>();

            //group by time step
            var groupedPeaks = peaks.GroupBy(p => p.time);

            //extract only top N peas from each time group
            foreach(var group in groupedPeaks)
            {
                var topNInGroup = group.OrderByDescending(p=>p.magnitude).Take(N);
                topNPeaks.AddRange(topNInGroup);
            }

            //update peaks list
            peaks = topNPeaks;
        }

        public void ExtractPeaksWithStaticThreshold() //selects peaks based on static thresholding (applied before
        {
            for (int time = 0; time < songProcessor.spectrogram.Count; time++)
            {
                for (int frequency = 0; frequency < songProcessor.spectrogram[time].Length; frequency++)
                {
                    if (songProcessor.spectrogram[time][frequency] > THRESHOLD)
                    {
                        Console.WriteLine(songProcessor.spectrogram[time][frequency]);
                        peaks.Add(new Peak(time, frequency));
                    }
                }
            }
        }

        public void ExtractPeaks()
        {
            ApplyAdaptiveThresholding();
            TopNPeaksOnly();
        }

        public void GenerateHashesFromPeaks()
        {
            ExtractPeaks();
            //combine peaks into pairs based on time difference and frequencies
            //peak time refers to peak which is considered in external loop
            for (int i = 0; i < peaks.Count; i++)
            {
                for (int j = 1; j < FANOUT && (i + j) < peaks.Count; j++)
                {
                    int t1 = peaks[i].time;
                    int f1 = peaks[i].frequency;
                    int t2 = peaks[i + j].time;
                    int f2 = peaks[i + j].frequency;

                    int timeOffset = t2 - t1;
                    string hash = ((f1 << 16) | (f2 << 8) | timeOffset).ToString(); //it's worth to consider int comparing insted of string for efficiency
                    saveHashToDatabase(hash, t1);
                }
            }
        }

        public List<string> HashesToList()
        {
            List<string> hashes = new List<string>();

            ExtractPeaks();
            for (int i = 0; i < peaks.Count; i++)
            {
                for (int j = 1; j < FANOUT && (i + j) < peaks.Count; j++)
                {
                    int t1 = peaks[i].time;
                    int f1 = peaks[i].frequency;
                    int t2 = peaks[i + j].time;
                    int f2 = peaks[i + j].frequency;

                    int timeOffset = t2 - t1;
                    string hash = ((f1 << 16) | (f2 << 8) | timeOffset).ToString(); //it's worth to consider int comparing insted of string for efficiency
                    hashes.Add(hash);
                }
            }

            return hashes;
        }
    }
}
