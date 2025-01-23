using System.Numerics;


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

        public enum HashSave
        {
            TO_DATABASE,
            TO_LIST
        }
        
        //static thresholding
        public double THRESHOLD = 20.0; //defines border which tells if magnitude is high enough to be considered as a peak
        public int FANOUT = 5;

        //adaptive thresholding
        public double THRESHOLD_OFFSET = 10.0;

        //top N peaks
        public int N = 4;

        public SongProcessor songProcessor;
        public List<Peak> peaks;

        public HashManager(SongProcessor songProcessor)
        {
            this.songProcessor = songProcessor;
            peaks = new List<Peak>();
        }

        private void saveHashToDatabase(int songHash, int chunkNumber)
        {
            //If hash does not exist, adds it, else adds only SongHash record (dupplicate hashes not allowed)
            CRUDManager.addSongTimestamp(songProcessor.audioFile.name, songHash, chunkNumber);
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
                double[] magnitudes = new double[songProcessor.spectrogram[t].Length];

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

            //extract only top N peaks from each time group
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

        public List<int> GenerateHashesFromPeaks(HashSave hs)
        {
            //initialize list if saving to list
            List<int>? hashes = hs == HashSave.TO_LIST ? new List<int>() : null;

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
                    int hash = ((f1 << 16) | (f2 << 8) | timeOffset);
                    
                    //save hash with selected option
                    if(hs == HashSave.TO_DATABASE)
                        saveHashToDatabase(hash, t1);
                    else if(hs == HashSave.TO_LIST)
                        hashes.Add(hash);
                }
            }

            return hashes;
        }
    }
}
