﻿using Accord.Audio;
using Accord.Audio.Filters;
using Accord.Audio.Windows;
using Accord.Math;
using Accord.Math.Decompositions;
using Accord.Statistics;
using Accord.Statistics.Analysis;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using NWaves.Filters;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AudioFingerprinting
{
    public class MfccFingerprinter
    {
        private const int TargetSampleRate = 16000;
        private const int FrameSize = 1024;
        private const int FrameStep = 512;
        private const int MelFilterCount = 40;  
        private const int MfccCoefficients = 20; 
        private const int DeltaWindow = 2;
        private const int HashTimeWindow = 3;
        private const float SilenceThreshold = 0.025f; 

        public byte[] GenerateFingerprint(string audioPath)
        {
            try
            {
                float[] samples = LoadAndPreprocessAudio(audioPath);
                if (samples.Length == 0) return Array.Empty<byte>();

                List<float[]> frames = GetFrames(samples, FrameSize, FrameStep);
                if (frames.Count == 0) return Array.Empty<byte>();

                double[][] mfccFeatures = ExtractMfccWithEnergy(frames);
                if (mfccFeatures.Length == 0) return Array.Empty<byte>();

                NormalizeFeatures(mfccFeatures);
                double[][][] mfccWithDeltas = AddDeltasAndDoubleDeltas(mfccFeatures);

                double[][] reducedFeatures = ApplyPca(mfccWithDeltas);

                return QuantizeAndHash(reducedFeatures);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при генерации фингерпринта: {ex.Message}");
                return Array.Empty<byte>();
            }
        }

        public double Compare(byte[] fp1, byte[] fp2)
        {
            if (fp1 == null || fp2 == null || fp1.Length == 0 || fp2.Length == 0)
                return 0.0;

            try
            {
                double[][] features1 = DecodeFingerprint(fp1);
                double[][] features2 = DecodeFingerprint(fp2);

                // Используем комбинацию DTW и косинусного сходства
                double dtwDistance = DynamicTimeWarping(features1, features2);
                double cosineSim = CosineSimilarity(
                    FlattenFeatures(features1),
                    FlattenFeatures(features2)
                );

                // Комбинированная метрика (70% DTW, 30% косинусное)
                double combinedScore = 0.7 * (1.0 / (1.0 + dtwDistance)) + 0.3 * cosineSim;

                // Нормализация и пороговая обработка
                return Math.Max(0, Math.Min(100, combinedScore * 100));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при сравнении: {ex.Message}");
                return 0.0;
            }
        }

        private float[] LoadAndPreprocessAudio(string audioPath)
        {
            if (!File.Exists(audioPath))
                throw new FileNotFoundException("Audio file not found", audioPath);

            using (var reader = new AudioFileReader(audioPath))
            {
                ISampleProvider pipeline = reader;

                // Конвертация в моно
                if (pipeline.WaveFormat.Channels > 1)
                    pipeline = new StereoToMonoSampleProvider(pipeline);

                // Ресемплинг
                if (pipeline.WaveFormat.SampleRate != TargetSampleRate)
                    pipeline = new WdlResamplingSampleProvider(pipeline, TargetSampleRate);

                // Чтение сэмплов
                float[] samples = ReadAllSamples(pipeline);
                if (samples.Length == 0) return Array.Empty<float>();

                // Нормализация
                NormalizeSamples(samples);

                // Обрезка тишины
                return TrimSilence(samples, SilenceThreshold);
            }
        }

        private float[] ReadAllSamples(ISampleProvider provider)
        {
            var buffer = new float[1024];
            var samples = new List<float>();
            int samplesRead;

            while ((samplesRead = provider.Read(buffer, 0, buffer.Length)) > 0)
            {
                samples.AddRange(buffer.Take(samplesRead));
            }

            return samples.ToArray();
        }

        private void NormalizeSamples(float[] samples)
        {
            float max = samples.Max(s => Math.Abs(s));
            if (max > 0)
            {
                for (int i = 0; i < samples.Length; i++)
                {
                    samples[i] /= max;
                }
            }
        }

        private float[] TrimSilence(float[] audioData, float threshold)
        {
            int start = 0;
            int end = audioData.Length - 1;

            // Начало аудио
            for (int i = 0; i < audioData.Length; i++)
            {
                if (Math.Abs(audioData[i]) > threshold)
                {
                    start = Math.Max(0, i - (TargetSampleRate / 10)); // Оставляем небольшой запас
                    break;
                }
            }

            // Конец аудио
            for (int i = audioData.Length - 1; i >= 0; i--)
            {
                if (Math.Abs(audioData[i]) > threshold)
                {
                    end = Math.Min(audioData.Length - 1, i + (TargetSampleRate / 10));
                    break;
                }
            }

            if (start >= end) return Array.Empty<float>();

            float[] trimmed = new float[end - start + 1];
            Array.Copy(audioData, start, trimmed, 0, trimmed.Length);
            return trimmed;
        }

        private List<float[]> GetFrames(float[] audioData, int frameSize, int frameStep)
        {
            var frames = new List<float[]>();
            if (audioData == null || audioData.Length < frameSize)
                return frames;

            int totalFrames = (audioData.Length - frameSize) / frameStep + 1;

            for (int i = 0; i < totalFrames; i++)
            {
                int offset = i * frameStep;
                float[] frame = new float[frameSize];
                Array.Copy(audioData, offset, frame, 0, frameSize);
                frames.Add(frame);
            }

            return frames;
        }

        private void ApplyHannWindow(float[] data)
        {
            int n = data.Length;
            for (int i = 0; i < n; i++)
            {
                double w = 0.5 * (1 - Math.Cos(2 * Math.PI * i / (n - 1)));
                data[i] *= (float)w;
            }
        }

        private double[][] ExtractMfccWithEnergy(List<float[]> frames)
        {
            if (frames == null || frames.Count == 0)
                return Array.Empty<double[]>();

            var mfccExtractor = new MelFrequencyCepstrumCoefficient(
                filterCount: MelFilterCount,
                cepstrumCount: MfccCoefficients - 1, // -1 потому что первый коэффициент заменим на энергию
                lowerFrequency: 300,
                upperFrequency: TargetSampleRate / 2,
                alpha: 0.97,
                samplingRate: TargetSampleRate,
                frameRate: 100,
                windowLength: (double)FrameSize / TargetSampleRate,
                numberOfBins: 512);

            var features = new double[frames.Count][];

            for (int i = 0; i < frames.Count; i++)
            {
                try
                {
                    float[] frame = frames[i];
                    if (frame.Length != FrameSize)
                        Array.Resize(ref frame, FrameSize);

                    ApplyHannWindow(frame);

                    // Вычисление энергии
                    double energy = Math.Log(frame.Sum(x => x * x) + 1e-10);

                    // Извлечение MFCC
                    var signal = Signal.FromArray(frame, TargetSampleRate);
                    var descriptors = mfccExtractor.Transform(signal);
                    var mfcc = ((dynamic)descriptors.First()).Descriptor as double[];

                    // Комбинируем энергию с MFCC
                    features[i] = new double[MfccCoefficients];
                    features[i][0] = energy;
                    Array.Copy(mfcc, 0, features[i], 1, MfccCoefficients - 1);
                }
                catch
                {
                    features[i] = new double[MfccCoefficients];
                }
            }

            return features;
        }

        private void NormalizeFeatures(double[][] features)
        {
            if (features == null || features.Length == 0)
                return;

            int numCoeffs = features[0].Length;
            for (int coeff = 0; coeff < numCoeffs; coeff++)
            {
                double mean = features.Select(f => f[coeff]).Average();
                double stdDev = Math.Sqrt(features.Select(f => Math.Pow(f[coeff] - mean, 2)).Average());

                for (int t = 0; t < features.Length; t++)
                {
                    features[t][coeff] = (features[t][coeff] - mean) / (stdDev + 1e-10);
                }
            }
        }

        private double[][] CalculateDeltas(double[][] features, int windowSize = DeltaWindow)
        {
            if (features == null || features.Length == 0)
                return Array.Empty<double[]>();

            int rows = features.Length;
            int cols = features[0].Length;
            double[][] deltas = new double[rows][];

            for (int t = 0; t < rows; t++)
            {
                deltas[t] = new double[cols];
                for (int c = 0; c < cols; c++)
                {
                    double numerator = 0;
                    double denominator = 0;
                    for (int d = 1; d <= windowSize; d++)
                    {
                        if (t + d < rows)
                        {
                            numerator += d * features[t + d][c];
                            denominator += d * d;
                        }
                        if (t - d >= 0)
                        {
                            numerator -= d * features[t - d][c];
                            denominator += d * d;
                        }
                    }
                    deltas[t][c] = numerator / (denominator + 1e-10);
                }
            }
            return deltas;
        }

        private double[][][] AddDeltasAndDoubleDeltas(double[][] mfccFeatures)
        {
            if (mfccFeatures == null || mfccFeatures.Length == 0)
                return Array.Empty<double[][]>();

            double[][] firstDeltas = CalculateDeltas(mfccFeatures);
            double[][] secondDeltas = CalculateDeltas(firstDeltas);

            int timeSteps = mfccFeatures.Length;
            var result = new double[timeSteps][][];

            for (int t = 0; t < timeSteps; t++)
            {
                result[t] = new double[3][];
                result[t][0] = mfccFeatures[t];
                result[t][1] = firstDeltas[t];
                result[t][2] = secondDeltas[t];
            }
            return result;
        }

        private double[][] ApplyPca(double[][][] features)
        {
            if (features == null || features.Length == 0)
                return Array.Empty<double[]>();

            // Преобразуем 3D массив в 2D
            int timeSteps = features.Length;
            int originalDim = features[0][0].Length * 3;

            double[][] data = new double[timeSteps][];
            for (int t = 0; t < timeSteps; t++)
            {
                data[t] = new double[originalDim];
                int idx = 0;
                for (int i = 0; i < 3; i++)
                {
                    Array.Copy(features[t][i], 0, data[t], idx, features[t][i].Length);
                    idx += features[t][i].Length;
                }
            }

            // PCA
            var pca = new PrincipalComponentAnalysis()
            {
                Method = PrincipalComponentMethod.Center,
                Whiten = true,
                NumberOfOutputs = MfccCoefficients // Сохраняем размерность
            };

            pca.Learn(data);
            return pca.Transform(data);
        }

        private byte[] QuantizeAndHash(double[][] features)
        {
            if (features == null || features.Length == 0)
                return Array.Empty<byte>();

            using (var ms = new MemoryStream())
            using (var writer = new BinaryWriter(ms))
            {
                writer.Write(features.Length);
                foreach (var feature in features)
                {
                    foreach (var val in feature)
                    {
                        // Квантование до 1 байта на значение
                        byte quantized = (byte)Math.Max(0, Math.Min(255, (val + 4) * 32));
                        writer.Write(quantized);
                    }
                }
                return ms.ToArray();
            }
        }

        private double[][] DecodeFingerprint(byte[] fingerprint)
        {
            if (fingerprint == null || fingerprint.Length == 0)
                return Array.Empty<double[]>();

            try
            {
                using (var ms = new MemoryStream(fingerprint))
                using (var reader = new BinaryReader(ms))
                {
                    int count = reader.ReadInt32();
                    var features = new double[count][];

                    for (int i = 0; i < count; i++)
                    {
                        features[i] = new double[MfccCoefficients];
                        for (int j = 0; j < MfccCoefficients; j++)
                        {
                            // Обратное квантование
                            features[i][j] = reader.ReadByte() / 32.0 - 4;
                        }
                    }

                    return features;
                }
            }
            catch
            {
                return Array.Empty<double[]>();
            }
        }

        private double DynamicTimeWarping(double[][] seq1, double[][] seq2)
        {
            if (seq1 == null || seq2 == null || seq1.Length == 0 || seq2.Length == 0)
                return double.MaxValue;

            int n = seq1.Length;
            int m = seq2.Length;

            double[,] dtw = new double[n + 1, m + 1];

            for (int i = 1; i <= n; i++)
                dtw[i, 0] = double.PositiveInfinity;
            for (int j = 1; j <= m; j++)
                dtw[0, j] = double.PositiveInfinity;
            dtw[0, 0] = 0;

            for (int i = 1; i <= n; i++)
            {
                for (int j = 1; j <= m; j++)
                {
                    double cost = EuclideanDistance(seq1[i - 1], seq2[j - 1]);
                    dtw[i, j] = cost + Math.Min(dtw[i - 1, j], Math.Min(dtw[i, j - 1], dtw[i - 1, j - 1]));
                }
            }

            return dtw[n, m] / (n + m);
        }

        private double EuclideanDistance(double[] a, double[] b)
        {
            if (a == null || b == null || a.Length != b.Length)
                return double.MaxValue;

            double sum = 0;
            for (int i = 0; i < a.Length; i++)
                sum += (a[i] - b[i]) * (a[i] - b[i]);

            return Math.Sqrt(sum);
        }

        private double CosineSimilarity(double[] a, double[] b)
        {
            if (a == null || b == null || a.Length != b.Length)
                return 0;

            double dot = 0, mag1 = 0, mag2 = 0;
            for (int i = 0; i < a.Length; i++)
            {
                dot += a[i] * b[i];
                mag1 += a[i] * a[i];
                mag2 += b[i] * b[i];
            }

            return dot / (Math.Sqrt(mag1) * Math.Sqrt(mag2) + 1e-10);
        }

        private double[] FlattenFeatures(double[][] features)
        {
            if (features == null || features.Length == 0)
                return Array.Empty<double>();

            var result = new double[features.Length * features[0].Length];
            int idx = 0;
            foreach (var feature in features)
                foreach (var val in feature)
                    result[idx++] = val;

            return result;
        }
    }
}