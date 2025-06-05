using Accord.Audio;
using Accord.Audio.Filters;
using Accord.Math;
using AudioFingerprinting;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Mfccextractor
{
    internal class Program
    {
        const int TargetSampleRate = 16000;
        public const int FrameSize = 1024;
        public const int FrameStep = 512;

        static void Main(string[] args)
        {
            var fingerprinter = new MfccFingerprinter();

            Console.WriteLine("Генерация фингерпринтов...");
            byte[] fp1 = fingerprinter.GenerateFingerprint("C:\\Users\\Повелитель\\source\\repos\\Проба_Фрингерпринтер\\Проба_Фрингерпринтер\\FilesAudio\\TestAudio.wav");
            byte[] fp2 = fingerprinter.GenerateFingerprint("C:\\Users\\Повелитель\\source\\repos\\Проба_Фрингерпринтер\\Проба_Фрингерпринтер\\FilesAudio\\TestAudioEdit.wav");
            byte[] fp3 = fingerprinter.GenerateFingerprint("C:\\Users\\Повелитель\\source\\repos\\Проба_Фрингерпринтер\\Проба_Фрингерпринтер\\FilesAudio\\Nor.wav");
            byte[] fp4 = fingerprinter.GenerateFingerprint("C:\\Users\\Повелитель\\source\\repos\\Проба_Фрингерпринтер\\Проба_Фрингерпринтер\\FilesAudio\\2.wav");


            Console.WriteLine("Сравнение файлов...");
            double sim1 = fingerprinter.Compare(fp1, fp2);
            double sim2 = fingerprinter.Compare(fp1, fp3);
            double sim3 = fingerprinter.Compare(fp1, fp4);

            Console.WriteLine("\nРезультаты сравнения:");
            Console.WriteLine($"Схожесть оригинал и редактированный: {sim1:F2}%");
            Console.WriteLine($"Схожесть оригинал и в два раза длинее: {sim2:F2}%");
            Console.WriteLine($"Схожесть оригинал и другой: {sim3:F2}%");

            Console.WriteLine("\nНажмите любую клавишу для выхода...");
            Console.ReadKey();
        }

    }
}