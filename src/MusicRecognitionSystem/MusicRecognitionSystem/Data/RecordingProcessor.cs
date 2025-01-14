using NAudio.Wave;
using MusicRecognitionSystem.Model;
using Microsoft.Identity.Client;

namespace MusicRecognitionSystem.Data
{
    internal class MatchData
    {
        public Guid songID { get; set; }
        public int timestamp { get; set; }
    }

    internal class RecordingProcessor
    {
        public static int SAMPLING_RATE = 44100;  //44.1kHz
        public static int BITS_PER_SAMPLE = 16; //16-bit PCM (depth)
        public static int CHANNELS = 1; //Mono

        public WaveInEvent recorder;
        public List<MatchData> matches; //lista pasujących matchy
        public List<String> allHashes;
        public List<Byte> recorderBytes;

        public RecordingProcessor()
        {
            recorder = new WaveInEvent()
            {
                WaveFormat = new WaveFormat(SAMPLING_RATE, BITS_PER_SAMPLE, CHANNELS)
            };
            recorder.DataAvailable += OnDataAvailable;

            matches = new List<MatchData>();
            allHashes = new List<String>();
            recorderBytes = new List<Byte>();
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

            while(bytesProcessed < e.BytesRecorded)
            {
                int bytesToProcess = Math.Min(SongProcessor.CHUNK_SIZE, e.BytesRecorded - bytesProcessed);
                byte[] audioChunk = new byte[bytesToProcess];

                Array.Copy(e.Buffer, bytesProcessed, audioChunk, 0, bytesToProcess);
                bytesProcessed += bytesToProcess;

                ProcessChunk(audioChunk);
            }
        }

        private void ProcessChunk(byte[] audioChunk)
        {
            //obtain chunk hash
            SongProcessor chunkProcessor = new SongProcessor(audioChunk);
            chunkProcessor.getFrequencies();

            HashManager chunkHashManager = new HashManager(chunkProcessor);
            string chunkHash = chunkHashManager.generateHash(0); //only one chunk so index should be 0
            allHashes.Add(chunkHash);

            //compare hash with database
            using (MusicRecognitionContext context = new MusicRecognitionContext())
            {
                List<SongHash> matchingHashes = context.SongHashes.Where(sh => sh.hash.hashValue == chunkHash).ToList();

                foreach (SongHash sh in matchingHashes)
                {
                    MatchData match = new MatchData()
                    {
                        songID = sh.songID,
                        timestamp = sh.timestamp
                    };
                    matches.Add(match);
                }
            }
        }

        public void PlayRecordedAudio()
        {
            byte[] audioBytes = recorderBytes.ToArray();

            var bufferedWaveProvider = new BufferedWaveProvider(new WaveFormat(SAMPLING_RATE, BITS_PER_SAMPLE, CHANNELS))
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
