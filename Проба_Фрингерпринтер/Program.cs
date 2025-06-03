using Accord.Audio;
using NAudio.Midi;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;

internal class Program
{
    const int SampleRate = 16000;
    private static string audioPathEdit = "C:\\Users\\Повелитель\\source\\repos\\Проба_Фрингерпринтер\\Проба_Фрингерпринтер\\FilesAudio\\TestAudioEdit.wav";

    private static void Main(string[] args)
    {
        Console.WriteLine("Добро пожаловать в Фригерпринтер основанный на методе MFCC");
        string audiPath = "C:\\Users\\Повелитель\\source\\repos\\Проба_Фрингерпринтер\\Проба_Фрингерпринтер\\FilesAudio\\TestAudio.wav";
        LoadAudio(audiPath);
    }

    private static float[] LoadAudio(string audioPath)
    {
        var audioData = new System.Collections.Generic.List<float>();

        using (var audioFile = new AudioFileReader(audioPath))
        {
            if (audioFile.WaveFormat.SampleRate != SampleRate)
            {
                ResampleAudio(audioPath, audioPathEdit, SampleRate);
            }
            else
            {
                Console.WriteLine("Частота уже соответствует требуемой");
            }
            return ReadAllSamples(audioFile);
        }
    }

    public static void ResampleAudio(string inputPath, string outputPath, int newSampleRate)
    {
        using (var reader = new AudioFileReader(inputPath))
        {
            ISampleProvider audioPipeline = reader;
            bool wasStereo = audioPipeline.WaveFormat.Channels > 1;

            if (wasStereo)
            {
                audioPipeline = new StereoToMonoSampleProvider(audioPipeline);
            }

            audioPipeline = new WdlResamplingSampleProvider(audioPipeline, newSampleRate);
            var samples = ReadAllSamples(audioPipeline);

            float maxBefore = GetMaxAmplitude(samples);
            NormalizeSamples(samples);
            float maxAfter = GetMaxAmplitude(samples);

            SaveNormalizedAudio(samples, newSampleRate, outputPath);

            Console.WriteLine("Характеристики нового аудиофайла:");
            Console.WriteLine($"Частота дискретизации: {newSampleRate} Гц");
            Console.WriteLine($"Тип: {(wasStereo ? "Преобразовано в моно" : "Исходное моно")}");
            Console.WriteLine($"Макс. амплитуда до нормализации: {maxBefore}");
            Console.WriteLine($"Макс. амплитуда после нормализации: {maxAfter}");
            Console.WriteLine($"Файл сохранен: {outputPath}");
        }
    }

    private static void NormalizeSamples(float[] samples)
    {
        float max = GetMaxAmplitude(samples);
        if (max > 0 && max < 1.0f)
        {
            for (int i = 0; i < samples.Length; i++)
            {
                samples[i] /= max;
            }
        }
    }

    private static float GetMaxAmplitude(float[] samples)
    {
        float max = 0;
        foreach (float sample in samples)
        {
            float abs = Math.Abs(sample);
            if (abs > max) max = abs;
        }
        return max;
    }

    private static float[] ReadAllSamples(ISampleProvider provider)
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

    private static void SaveNormalizedAudio(float[] samples, int sampleRate, string outputPath)
    {
        var outputFormat = WaveFormat.CreateCustomFormat(
            WaveFormatEncoding.IeeeFloat,
            sampleRate,
            1,
            sampleRate * 4,
            4,
            32);

        using (var writer = new WaveFileWriter(outputPath, outputFormat))
        {
            writer.WriteSamples(samples, 0, samples.Length);
        }
    }
}