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

void TestRecording()
{
    RecordingProcessor recordingProcessor = new RecordingProcessor();
    recordingProcessor.StartRecording();
    Console.WriteLine("Recording started. Press any key to stop recording.");
    Console.ReadKey();
    var list = recordingProcessor.matches;

    recordingProcessor.StopRecording();

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

