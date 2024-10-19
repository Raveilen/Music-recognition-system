using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace MusicRecognitionSystem.Data
{
    internal class HashManager
    {
        public static int LOWER_LIMIT = 41; //nie może być 40 bo według takiego algorytmu 40 paasowałoby jeszcze do poprzedniego przedziału.
        public static int UPPER_LIMIT = 300;

        // Przedziały: 41-80, 81-120, 121-180, 181-300
        public static int[] RANGE = new int[] { 80, 120, 180, UPPER_LIMIT + 1 };

        public SongProcessor songProcessor;

        //temporary solution
        public string filename;

        public HashManager(SongProcessor songProcessor, string filename)
        {
            this.songProcessor = songProcessor;
            this.filename = filename;
        }

        public void generateHashes()
        {
            using (StreamWriter hashFile = new StreamWriter($"Hashes/{filename}.txt"))
            {
                for (int i = 0; i < songProcessor.songFrequencies.Length; i++)
                {
                    double[] maxMagnitudes = { -1, -1, -1, -1, -1 };
                    int[] maxFrequencies = new int[4];
                    for (int j = LOWER_LIMIT; j < UPPER_LIMIT - 1; j++)
                    {
                        Complex frequency = songProcessor.songFrequencies[i][j];
                        double magnitude = Math.Log(Complex.Abs(frequency) + 1);

                        int index = rangeSelect(j);
                        if (index != -1) //checks if value in range
                        {
                            if (magnitude > maxMagnitudes[index])
                            {
                                maxMagnitudes[index] = magnitude;
                                maxFrequencies[index] = j;
                            }
                        }
                        else
                        {
                            throw new Exception("Attempt to process data from invalid range!");
                        }
                    }
                    string songHash = computeHash(maxFrequencies);
                    saveHashToDatabase(songHash);
                    hashFile.WriteLine(songHash);
                }
            }
        }

        private void saveHashToDatabase(string hash)
        {
            //To extend after creating database structre
            return;
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
    }
}
