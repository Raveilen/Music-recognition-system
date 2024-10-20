﻿using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using NAudio.Wave;
using System.Runtime.ConstrainedExecution;
using System.Numerics;
using NAudio.Dsp;
using MathNet.Numerics.IntegralTransforms;
using Complex = System.Numerics.Complex;
using MusicRecognitionSystem.Data;


AudioFileManager.LoadSongsMetadata();

for (int i = 0; i < AudioFileManager.audioFiles.Length; i++)
{
    AudioFile audioFile = AudioFileManager.LoadAudioFile(i);
    SongProcessor songProcessor = new SongProcessor(audioFile);
    songProcessor.getSongFrequencies();
    HashManager hashManager = new HashManager(songProcessor, i.ToString());
    hashManager.generateHashes();
}
