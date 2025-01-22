using NAudio.Wave;
using MusicRecognitionSystem.Model;
using Microsoft.Identity.Client;
using NAudio.Midi;
using System.Diagnostics.Contracts;

namespace MusicRecognitionSystem.Data
{
    internal class MatchData
    {
        public Guid songID { get; set; }
        public int timestamp { get; set; }

        public MatchData(Guid songID, int timestamp)
        {
            this.songID = songID;
            this.timestamp = timestamp;
        }
    }

    internal class RecordingProcessor
    {
        public WaveInEvent recorder;
        public List<MatchData> matches; //lista pasujących matchy
        public List<String> allHashes;
        public List<Byte> recorderBytes;
        public List<Guid> chunksBestMatches;

        public List<List<Double[]>> tempSpectrograms = new List<List<double[]>>();

        public RecordingProcessor()
        {
            recorder = new WaveInEvent()
            {
                WaveFormat = new WaveFormat(AudioFileManager.SAMPLING_RATE, AudioFileManager.BITS_PER_SAMPLE, AudioFileManager.CHANNELS)
            };
            recorder.DataAvailable += OnDataAvailable;

            this.matches = new List<MatchData>();
            this.allHashes = new List<String>();
            this.recorderBytes = new List<Byte>();
            this.chunksBestMatches = new List<Guid>();
        }

        public void StartRecording()
        {
            recorder.StartRecording();
        }

        public void StopRecording()
        {
            recorder.StopRecording();
            recorder.Dispose();
        }

        private void OnDataAvailable(object sender, WaveInEventArgs e)
        {
            //for audio playing
            recorderBytes.AddRange(e.Buffer[..e.BytesRecorded]);

            int bytesProcessed = 0;

            while(e.BytesRecorded - bytesProcessed >= SongProcessor.CHUNK_SIZE * 2) //int bytesToProcess = Math.Min(SongProcessor.CHUNK_SIZE, e.BytesRecorded - bytesProcessed);
            {
                byte[] audioChunk = new byte[SongProcessor.CHUNK_SIZE * 2];

                Array.Copy(e.Buffer, bytesProcessed, audioChunk, 0, SongProcessor.CHUNK_SIZE * 2);
                bytesProcessed += SongProcessor.CHUNK_SIZE * 2;

                ProcessChunk(audioChunk);
            }
        }

        private void ProcessChunk(byte[] audioChunk)
        {
            //obtain chunk hash
            SongProcessor chunkProcessor = new SongProcessor(audioChunk);
            //chunkProcessor.getFrequencies();
            chunkProcessor.computeSpectrogram();
            tempSpectrograms.Add(chunkProcessor.spectrogram);

            HashManager chunkHashManager = new HashManager(chunkProcessor);
            List<int> chunkHashList = chunkHashManager.GenerateHashesFromPeaks(HashManager.HashSave.TO_LIST);

            //string chunkHash = chunkHashManager.generateHash(0); //only one chunk so index should be 0
            //allHashes.Add(chunkHash);

            //compare hash with database

            //TUTAJ, PONIEWAŻ MAMY DO CZYNIENIA Z WIELOMA HASZAMI, NALEŻY UWZGLĘDNIĆ TEN PRZYPADEK I ROZBUDOWAĆ FUNKCJĘ

            using (MusicRecognitionContext context = new MusicRecognitionContext())
            {
                List<MatchData> chunkMatches = new List<MatchData>();
                
                foreach (int hash in chunkHashList)//for each hash, we find matches in database, convert to MatchData and add to general list
                {
                    List<SongHash> matchingHashes = context.SongHashes.Where(sh => sh.hash.hashValue == hash).ToList();
                    List<MatchData> matchs = matchingHashes.Select(mh => new MatchData (mh.songID, mh.timestamp)).ToList();
                    chunkMatches.AddRange(matchs);
                }

                IEnumerable<IGrouping<Guid, int>> groupedMatches = chunkMatches.GroupBy(x => x.songID, x=>x.timestamp);

                List<SongRecognition> matchesProbability = new List<SongRecognition>();
                foreach (var songGroup in groupedMatches)
                {
                    //listing all timestamps for song id
                    List<int> songTimestamps = songGroup.Select(x=>x).ToList();

                    SongRecognition songStats = new SongRecognition(songGroup.Key, songTimestamps);
                    
                    //IQR caused that no matches left
                    if(songStats.stdDev != -1 && songStats.mad != -1)
                        matchesProbability.Add(songStats);
                }

                chunksBestMatches.Add(SongRecognition.TypeBestLocalGuess(matchesProbability));
            }
            
        }

        public void PlayRecordedAudio()
        {
            byte[] audioBytes = recorderBytes.ToArray();

            var bufferedWaveProvider = new BufferedWaveProvider(new WaveFormat(AudioFileManager.SAMPLING_RATE, AudioFileManager.BITS_PER_SAMPLE, AudioFileManager.CHANNELS))
            {
                BufferLength = audioBytes.Length,
                DiscardOnBufferOverflow = true
            };

            bufferedWaveProvider.AddSamples(audioBytes, 0, audioBytes.Length);

            using (var waveOut = new WaveOutEvent())
            {
                waveOut.Init(bufferedWaveProvider);
                waveOut.Play();
                while (waveOut.PlaybackState == PlaybackState.Playing)
                {
                    Thread.Sleep(100);
                }
            }
        }

    }
}
