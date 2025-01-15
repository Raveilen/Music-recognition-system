using MusicRecognitionSystem.Data;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;

void DatabaseInit()
{
    Logger.OpenLog();

    AudioFileManager.LoadSongsMetadata();

    //for each song
    for (int i = 0; i < AudioFileManager.audioFiles.Length; i++)
    {
        AudioFile audioFile = AudioFileManager.LoadAudioFile(i);
        SongProcessor songProcessor = new SongProcessor(audioFile);
        songProcessor.getFrequencies();
        HashManager hashManager = new HashManager(songProcessor);
        hashManager.generateHashes();

        Logger.Log("\n");
        Console.WriteLine($"Song {audioFile.name} has been added to database");
    }

    Logger.CloseLog();
}

void SpectrogramBasedHashGeneration()
{
    Logger.OpenLog();

    AudioFileManager.LoadSongsMetadata();

    for(int i=0; i< AudioFileManager.audioFiles.Length; i++)
    {
        AudioFile audioFile = AudioFileManager.LoadAudioFile(i);
        SongProcessor songProcessor = new SongProcessor(audioFile);
        songProcessor.computeSpectrogram();
        HashManager hashManager = new HashManager(songProcessor);
        hashManager.GenerateHashesFromPeaks();

        Logger.Log("\n");
        Console.WriteLine($"Song {audioFile.name} has been added to database");
    }

    Logger.CloseLog();
}

void TestRecording()
{
    RecordingProcessor recordingProcessor = new RecordingProcessor();
    recordingProcessor.StartRecording();
    Console.WriteLine("Recording started. Press any key to stop recording.");
    Console.ReadKey();

    recordingProcessor.StopRecording();

    var list = recordingProcessor.matches;

    var tempSpectrograms = recordingProcessor.tempSpectrograms;
    var averages = tempSpectrograms.Select(x => x[0].Average());

    Console.WriteLine("Hashes generated:");
    foreach (var hash in recordingProcessor.allHashes)
    {
        Console.WriteLine(hash);
    }

    if (list.Count == 0)
    {
        Console.WriteLine("No matches found.");
    }

    foreach (var match in list)
    {
        Console.WriteLine($"Match for songID {match.songID}");
    }

    list.GroupBy(x => x.songID).Select(x => new { songID = x.Key, count = x.Count() }).ToList().ForEach(x => Console.WriteLine($"SongID: {x.songID}, Count: {x.count}"));
}

void RecordedAudioPlayTest()
{
    RecordingProcessor recordingProcessor = new RecordingProcessor();
    recordingProcessor.StartRecording();
    Console.WriteLine("Recording started. Press any key to stop recording.");
    Console.ReadKey();
    recordingProcessor.recorder.StopRecording();
    Console.WriteLine("Recording stopped");
    recordingProcessor.PlayRecordedAudio();
    recordingProcessor.recorder.Dispose();
}

TestRecording();