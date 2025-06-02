using System;
using System.IO;
using Accord.Audio;
using Accord.Audio.Filters;
using Accord.Audio.Filters;
using Accord.Audio.Generators;
using Accord.Audio.Windows;
using Accord.Math;
using Accord.Statistics.Analysis;
using Accord.Math.Transforms;
using NAudio.Wave;
internal class Program
{
    const int SampleRate = 16;
    private static void Main(string[] args)
    {
        Console.WriteLine("Добро пожаловать в Фригерпринтер основанный  на методе MFCC");
        string audiPath = "\\FilesAudio\\TestAudio.wav";
        Console.WriteLine("Новое");
        Console.WriteLine("fafasf");
        
    }
    private float[] LoadAudio(string audioPath)
    {
        using var reader = new AudioFileReader(audioPath);
        // Ресемплинг до нужной частоты, если необходимо
        var resampler = new MediaFoundationResampler(reader, SampleRate);
        var buffer = new float[reader.Length];
        int samplesRead = resampler.Read(buffer, 0, buffer.Length);
        return buffer.Take(samplesRead).ToArray();
    }
}