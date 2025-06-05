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

        //    private static void ProcessAudio(string inputPath, string outputPath)
        //    {
        //        using (var audioFile = new AudioFileReader(inputPath))
        //        {
        //            bool needProcessing = false;

        //            if (audioFile.WaveFormat.SampleRate != TargetSampleRate)
        //            {
        //                Console.WriteLine($"Частота дискретизации отличается ({audioFile.WaveFormat.SampleRate} Гц)");
        //                needProcessing = true;
        //            }

        //            if (audioFile.WaveFormat.Channels > 1)
        //            {
        //                Console.WriteLine($"Количество каналов: {audioFile.WaveFormat.Channels}, преобразуется в моно.");
        //                needProcessing = true;
        //            }

        //            if (audioFile.WaveFormat.BitsPerSample != 16)
        //            {
        //                Console.WriteLine($"Битовая глубина: {audioFile.WaveFormat.BitsPerSample}-bit, ожидается 16 bit.");
        //                needProcessing = true;
        //            }

        //            if (!needProcessing)
        //            {
        //                Console.WriteLine("Файл соответствует требованиям, никаких изменений не требуется.");
        //                File.Copy(inputPath, outputPath, true);
        //                return;
        //            }

        //            ResampleAndExtractMFCC(inputPath, outputPath, TargetSampleRate);
        //        }
        //    }

        //    public static void ResampleAndExtractMFCC(string inputPath, string outputPath, int targetSampleRate)
        //    {
        //        using (var reader = new AudioFileReader(inputPath))
        //        {
        //            ISampleProvider pipeline = reader;
        //            bool wasStereo = pipeline.WaveFormat.Channels > 1;

        //            if (wasStereo)
        //                pipeline = new StereoToMonoSampleProvider(pipeline);

        //            pipeline = new WdlResamplingSampleProvider(pipeline, targetSampleRate);

        //            float[] samples = ReadAllSamples(pipeline);

        //            float maxBeforeNormalization = GetMaxAmplitude(samples);
        //            NormalizeSamples(samples);
        //            float maxAfterNormalization = GetMaxAmplitude(samples);

        //            samples = TrimSilence(samples);

        //            List<float[]> frames = GetFrames(samples, FrameSize, FrameStep);

        //            //var mfcc = new MelFrequencyCepstrumCoefficient(
        //            //    FrameSize: FrameSize,
        //            //    sampleRate: targetSampleRate,
        //            //    numberOfCoefficients: 13,
        //            //    melFilters: 26,
        //            //    lowerFrequency: 0,
        //            //    upperFrequency: targetSampleRate / 2,
        //            //    alpha: 0.97);

        //            //Signal[] signals = frames.Select(frame => 
        //            //    Signal.FromArray(frame, 1, targetSampleRate)).ToArray();

        //            //double[][] mfccFeatures = new double[signals.Length][];
        //            //for (int i = 0; i < signals.Length; i++)
        //            //{
        //            //    mfccFeatures[i] = mfcc.Transform(signals[i]);
        //            //}

        //            //NormalizeFeatures(mfccFeatures);

        //            //double[][][] mfccWithDeltas = AddDeltasAndDoubleDeltas(mfccFeatures);

        //            //SaveMFCCResults(mfccWithDeltas, Path.ChangeExtension(outputPath, ".mfcc"));

        //            float[] processedAudio = MergeFrames(frames, FrameStep);
        //            SaveNormalizedAudio(processedAudio, targetSampleRate, outputPath);

        //            Console.WriteLine("\nХарактеристика выходного файла:");
        //            Console.WriteLine($"Частота дискретизации: {targetSampleRate} Гц");
        //            Console.WriteLine($"Тип: {(wasStereo ? "Преобразовано в моно" : "Оригинальное моно")}");
        //            Console.WriteLine($"Максимальная амплитуда до нормализации: {maxBeforeNormalization}");
        //            Console.WriteLine($"Максимальная амплитуда после нормализации: {maxAfterNormalization}");
        //            Console.WriteLine($"Файл сохранён: {outputPath}");
        //            Console.WriteLine($"MFCC признаки сохранены в: {Path.ChangeExtension(outputPath, ".mfcc")}");
        //        }
        //    }

        //    private static float[] ReadAllSamples(ISampleProvider provider)
        //    {
        //        var buffer = new float[1024];
        //        var samples = new List<float>();
        //        int samplesRead;

        //        while ((samplesRead = provider.Read(buffer, 0, buffer.Length)) > 0)
        //        {
        //            samples.AddRange(buffer.Take(samplesRead));
        //        }

        //        return samples.ToArray();
        //    }

        //    private static float GetMaxAmplitude(float[] samples)
        //    {
        //        float max = 0;
        //        foreach (float sample in samples)
        //        {
        //            float abs = Math.Abs(sample);
        //            if (abs > max) max = abs;
        //        }
        //        return max;
        //    }

        //    private static void NormalizeSamples(float[] samples)
        //    {
        //        float max = GetMaxAmplitude(samples);
        //        if (max > 0)
        //        {
        //            for (int i = 0; i < samples.Length; i++)
        //            {
        //                samples[i] /= max;
        //            }
        //        }
        //    }

        //    private static float[] TrimSilence(float[] audioData, float silenceThreshold = 0.01f)
        //    {
        //        int start = 0;
        //        int end = audioData.Length - 1;

        //        for (int i = 0; i < audioData.Length; i++)
        //        {
        //            if (Math.Abs(audioData[i]) > silenceThreshold)
        //            {
        //                start = i;
        //                break;
        //            }
        //        }

        //        for (int i = audioData.Length - 1; i >= 0; i--)
        //        {
        //            if (Math.Abs(audioData[i]) > silenceThreshold)
        //            {
        //                end = i;
        //                break;
        //            }
        //        }

        //        int length = end - start + 1;
        //        float[] trimmedAudio = new float[length];
        //        Array.Copy(audioData, start, trimmedAudio, 0, length);

        //        return trimmedAudio;
        //    }

        //    private static List<float[]> GetFrames(float[] audioData, int frameSize, int frameStep)
        //    {
        //        List<float[]> frames = new List<float[]>();
        //        int totalFrames = (audioData.Length - frameSize) / frameStep + 1;

        //        for (int i = 0; i < totalFrames; i++)
        //        {
        //            int offset = i * frameStep;
        //            float[] frame = new float[frameSize];
        //            Array.Copy(audioData, offset, frame, 0, frameSize);
        //            frames.Add(frame);
        //        }

        //        return frames;
        //    }

        //    private static float[] MergeFrames(List<float[]> frames, int frameStep)
        //    {
        //        int totalLength = (frames.Count - 1) * frameStep + frames[0].Length;
        //        float[] merged = new float[totalLength];

        //        for (int i = 0; i < frames.Count; i++)
        //        {
        //            int offset = i * frameStep;
        //            Array.Copy(frames[i], 0, merged, offset, frames[i].Length);
        //        }
        //        return merged;
        //    }

        //    private static void NormalizeFeatures(double[][] features)
        //    {
        //        int numCoeffs = features[0].Length;
        //        for (int coeff = 0; coeff < numCoeffs; coeff++)
        //        {
        //            double mean = features.Select(f => f[coeff]).Average();
        //            double stdDev = Math.Sqrt(features.Select(f => Math.Pow(f[coeff] - mean, 2)).Average());

        //            for (int t = 0; t < features.Length; t++)
        //            {
        //                features[t][coeff] = (features[t][coeff] - mean) / (stdDev + 1e-10);
        //            }
        //        }
        //    }

        //    private static double[][] CalculateDeltas(double[][] features, int windowSize = 2)
        //    {
        //        int rows = features.Length;
        //        int cols = features[0].Length;
        //        double[][] deltas = new double[rows][];

        //        for (int t = 0; t < rows; t++)
        //        {
        //            deltas[t] = new double[cols];
        //            for (int c = 0; c < cols; c++)
        //            {
        //                double numerator = 0;
        //                double denominator = 0;
        //                for (int d = -windowSize; d <= windowSize; d++)
        //                {
        //                    if (t + d >= 0 && t + d < rows)
        //                    {
        //                        numerator += d * features[t + d][c];
        //                        denominator += d * d;
        //                    }
        //                }
        //                deltas[t][c] = numerator / (denominator + 1e-10);
        //            }
        //        }
        //        return deltas;
        //    }

        //    private static double[][][] AddDeltasAndDoubleDeltas(double[][] mfccFeatures)
        //    {
        //        double[][] firstDeltas = CalculateDeltas(mfccFeatures);
        //        double[][] secondDeltas = CalculateDeltas(firstDeltas);

        //        int timeSteps = mfccFeatures.Length;
        //        int featureCount = mfccFeatures[0].Length;

        //        double[][][] result = new double[timeSteps][][];

        //        for (int t = 0; t < timeSteps; t++)
        //        {
        //            result[t] = new double[3][];
        //            result[t][0] = mfccFeatures[t];
        //            result[t][1] = firstDeltas[t];
        //            result[t][2] = secondDeltas[t];
        //        }
        //        return result;
        //    }

        //    private static void SaveMFCCResults(double[][][] results, string fileName)
        //    {
        //        using (StreamWriter sw = new StreamWriter(fileName))
        //        {
        //            foreach (var frame in results)
        //            {
        //                sw.Write(string.Join(" ", frame[0]) + " ");
        //                sw.Write(string.Join(" ", frame[1]) + " ");
        //                sw.WriteLine(string.Join(" ", frame[2]));
        //            }
        //        }
        //    }

        //    private static void SaveNormalizedAudio(float[] samples, int sampleRate, string outputPath)
        //    {
        //        var outputFormat = WaveFormat.CreateIeeeFloatWaveFormat(sampleRate, 1);
        //        using (var writer = new WaveFileWriter(outputPath, outputFormat))
        //        {
        //            writer.WriteSamples(samples, 0, samples.Length);
        //        }
        //    }
        //}
    }
}