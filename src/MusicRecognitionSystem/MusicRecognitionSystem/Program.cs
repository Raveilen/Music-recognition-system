using MusicRecognitionSystem.Data;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Xml.Linq;


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
        hashManager.GenerateHashesFromPeaks(HashManager.HashSave.TO_DATABASE);

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

    Console.WriteLine("Recording stopped");

    //real-time approach with statistics at the end
    /* var matchList = recordingProcessor.matches;

     List<SongRecognition> stats = new List<SongRecognition>();
     var groupedMatches = matchList.GroupBy(x => x.songID, x => x.timestamp);
     foreach (var group in groupedMatches)
     {
         List<int> songTimestamps = group.Select(x => x).ToList();

         SongRecognition songStats = new SongRecognition(group.Key, songTimestamps);

         Console.WriteLine($"song: {CRUDManager.GetSongName(group.Key)}, stdDev: {songStats.stdDev} mad: {songStats.mad} ranking: {songStats.rankingVal}");
         stats.Add(songStats);
     }

     Guid bestMatchID = SongRecognition.TypeBestLocalGuess(stats);

     Console.WriteLine($"Best matching song: {CRUDManager.GetSongName(bestMatchID)}");*/


    //real-time approoach with each batch ranking
    /*Guid bestMatchID = SongRecognition.ChooseBestMatchingSongID(recordingProcessor.chunksBestMatches);
    string bestMatchTitle = CRUDManager.GetSongName(bestMatchID);

    Console.WriteLine($"Best matching song is: {bestMatchTitle}");

    Console.WriteLine("All candidates list");
    var groupedCadidates = recordingProcessor.chunksBestMatches.GroupBy(x => x);

    foreach (var item in groupedCadidates)
    {
        Console.WriteLine($"song: {CRUDManager.GetSongName(item.Key)} count: {item.Count()}");
    }*/

    //simple quantity check at the end of computing
  /*  var list = recordingProcessor.matches;

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

    list.GroupBy(x => x.songID).Select(x => new { songID = x.Key, count = x.Count() }).ToList().ForEach(x => Console.WriteLine($"Song: {CRUDManager.GetSongName(x.songID)}, Count: {x.count}"));
*/

    //for each song count the most frequent offset
    var list = recordingProcessor.matches;
    var groupedMatches = list.GroupBy(x => x.songID, x => x.timestamp);

    List<MatchData> mostFreqentOffsets = new List<MatchData>();
    foreach (var song in groupedMatches) 
    {
        var groupedOffsets = song.GroupBy(x => x);
        var offsetsCount = groupedOffsets.Select(x => x.Count());
        var test = offsetsCount.OrderByDescending(x=>x);
        var test2 = groupedOffsets.OrderByDescending(x => x.Count());
        var mostFrequentOffset = offsetsCount.OrderByDescending(x => x).FirstOrDefault();

        Console.WriteLine($"Song: {CRUDManager.GetSongName(song.Key)} MostFrequentOffset: {mostFrequentOffset}");
        mostFreqentOffsets.Add(new MatchData(song.Key, mostFrequentOffset));
    }

    //choose best matching song
   string mostFrequentSong = CRUDManager.GetSongName(mostFreqentOffsets.OrderByDescending(x => x.timestamp).FirstOrDefault().songID);
    Console.WriteLine($"Best matching song: {mostFrequentSong}");

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

//TestRecording();
/*SpectrogramBasedHashGeneration();*/
TestRecording();