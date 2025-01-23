using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Markup;

namespace MusicRecognitionSystem.Data
{
    /*PIPELINE
     * 1) IQR
     * 2) stdDev
     * 3) mad
     * 4) rankingVal
     * After callculated for all songs in chunk...
     * Choose best matching song in chunk
     * After recording is stopped, choose best matching song from gathered candidates
     */

    internal class SongRecognition
    {
        //input
        public Guid songID { get; set; }

        //output
        public double rankingVal;

        public double stdDev { get; set; }
        public double mad { get; set; }

        public SongRecognition(Guid songID, List<int> timestamps)
        {
            this.songID = songID;

            // stdDev & mad callculations
            //List<int> filteredTimestamps = FilterWithIQR(timestamps); //IQR seem sto be too aggresive atm, because only few hashes left and almost always with the same value

            if(timestamps.Count == 0)
            {
                //markers means that there are no timestamps (check if not to frequent occurence)
                stdDev = -1;
                mad = -1;

                return;
            }

            //now we have both deciding values callculated`
            this.stdDev = CalculateStandardDeviation(timestamps);
            this.mad = CalculateMedianAbsoluteDeviation(timestamps);

            //we can now calculate ranking value (allows to compare songs)
            this.rankingVal = CalculateRankingValue(this.stdDev, this.mad);
        }

        private static double CalculateStandardDeviation(List<int> timestamps)
        {
            double mean = timestamps.Average();
            double sumOfSquaresOfDifferences = timestamps.Select(val => (val - mean) * (val - mean)).Sum();
            double sd = Math.Sqrt(sumOfSquaresOfDifferences / timestamps.Count);

            return sd;
        }

        private static double CalculateMedianAbsoluteDeviation(List<int> timestamps)
        {
            double median = timestamps.OrderBy(x=>x).ElementAt(timestamps.Count / 2);
            double mad = timestamps.Average(x => Math.Abs(x - median));

            return mad;
        }

        private static double CalculateRankingValue(double stdDev, double mad)
        {
            return Math.Sqrt(stdDev * mad);
        }

        private static List<int> FilterWithIQR(List<int> timestamps) //calculates IQR and removes outliers
        {
            List<int> sortedTimestamps = timestamps.OrderBy(x => x).ToList();
            int count = sortedTimestamps.Count;

            double q1 = sortedTimestamps[(int)(0.25 * count)];
            double q3 = sortedTimestamps[(int)(0.25 * count)];
            
            double iqr = q3 - q1;

            double lowerBound = q1 - 1.5 * iqr;
            double upperBound = q3 + 1.5 * iqr;

            List<int> timestampWithoutOutliers =  timestamps.Where(x => x >= lowerBound && x <= upperBound).ToList();

            return timestampWithoutOutliers;
        }

        //ewenatulanie jeszcze wartości do DBSCAN

        public static Guid TypeBestLocalGuess(List<SongRecognition> songsStats)
        {
            List<SongRecognition> sorted = songsStats.OrderBy(x => x.rankingVal).ToList();
            return sorted[0].songID;
        }

        public static Guid ChooseBestMatchingSongID(List<Guid> localBestMatches) //after recording is stopped, we choose best matching song from gathered candidates
        {
            if(localBestMatches.Count == 0)
            {
                Console.WriteLine("No Matches Found");
                return Guid.Empty;
            }
            var groupedMatches = localBestMatches.GroupBy(x => x);
            var sortedMatches = groupedMatches.OrderByDescending(x => x.Count());

            return sortedMatches.First().Key;
        }
    }
}
