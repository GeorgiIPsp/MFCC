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
using NAudio.Wave.SampleProviders;
using System.Reflection.PortableExecutable;
internal class Program
{
    const int SampleRate = 16000;
    private static string audioPathEdit = "C:\\Users\\Повелитель\\source\\repos\\Проба_Фрингерпринтер\\Проба_Фрингерпринтер\\FilesAudio\\TestAudioEdit.wav";

    private static void Main(string[] args)
    {
        Console.WriteLine("Добро пожаловать в Фригерпринтер основанный  на методе MFCC");
        string audiPath = "C:\\Users\\Повелитель\\source\\repos\\Проба_Фрингерпринтер\\Проба_Фрингерпринтер\\FilesAudio\\TestAudio.wav";

        Console.WriteLine("Новое");
        Console.WriteLine("fafasf");
        LoadAudio(audiPath);
        
    }
    private static float[] LoadAudio(string audioPath)
    {
       

        var audioData = new System.Collections.Generic.List<float>();

        using (var audioFile = new AudioFileReader(audioPath))
        {

            if (audioFile.WaveFormat.SampleRate != SampleRate)
            {
                Console.WriteLine($"Конвертация: {audioFile.WaveFormat.SampleRate} Гц -> {SampleRate} Гц");
                ResampleAudio(audioPath, audioPathEdit, SampleRate);

                // Загружаем конвертированный файл
                using (var convertedFile = new AudioFileReader(audioPathEdit))
                {
                    return ReadAllSamples(convertedFile);
                }

            }
            else
            {
                Console.WriteLine("OK");
            }
            return ReadAllSamples(audioFile);
        }
        

    }
    private static float[] ReadAllSamples(AudioFileReader reader)
    {
        var buffer = new float[reader.WaveFormat.SampleRate * reader.WaveFormat.Channels];
        var samples = new System.Collections.Generic.List<float>();
        int samplesRead;

        while ((samplesRead = reader.Read(buffer, 0, buffer.Length)) > 0)
        {
            for (int i = 0; i < samplesRead; i++)
            {
                samples.Add(buffer[i]);
            }
        }

        return samples.ToArray();
    }
    public static void ResampleAudio(string inputPath, string outputPath, int newSampleRate)
    {
        bool MonoBool = false;
        using (var reader = new AudioFileReader(inputPath))
        {
            ISampleProvider audioPipeline = reader;

            if (audioPipeline.WaveFormat.Channels > 1)
            {
                audioPipeline = new StereoToMonoSampleProvider(audioPipeline);
                Console.WriteLine("Успешно преобразован в моно");
            }
            if (audioPipeline.WaveFormat.Channels > 1)
            {
                MonoBool = false;
            }
            else
            {
                MonoBool = true;
            }
            audioPipeline = new WdlResamplingSampleProvider(audioPipeline, newSampleRate);

            WaveFileWriter.CreateWaveFile16(outputPath, audioPipeline);
            Console.WriteLine($"Файл успешно конвертирован в {newSampleRate} Гц");
            Console.WriteLine($"Частота файла: {audioPipeline.WaveFormat.SampleRate}, Преобразован в моно? : {(MonoBool ? "Да": "Нет" )}");
        }
    
    }
}