using System;
using System.Collections.Generic;
using System.IO;
using MathNet.Numerics.IntegralTransforms;
using NAudio.Wave;
using System.Numerics;


namespace Soundlyzer
{
    public static class AudioProcessor
    {
        public static float[] ReadSamplesMono(string path, out int sampleRate)
        {
            using var reader = new AudioFileReader(path);
            
            sampleRate = reader.WaveFormat.SampleRate;

            var samples = new List<float>();

            float[] buffer = new float[reader.WaveFormat.SampleRate * reader.WaveFormat.Channels];
            int read;

            // czytanie sampli z pliku
            while ((read = reader.Read(buffer, 0, buffer.Length)) > 0)
            {
                for (int i = 0; i < read; i += reader.WaveFormat.Channels)
                {
                    float mono = 0;
                    for (int ch = 0; ch < reader.WaveFormat.Channels; ch++)
                        //sumowanie kanalow do mono
                        mono += buffer[i + ch];
                    samples.Add(mono / reader.WaveFormat.Channels);
                }
            }

            return samples.ToArray();
        }

        public static Complex[][] CalculateSpectrogram(float[] samples, int sampleRate, int windowSize = 1024, int overlap = 512)
        {
            int stride = windowSize - overlap;

            //podzielenie na okienka
            int segments = (samples.Length - windowSize) / stride;
            //jesli nie ma wystarczajaco duzo sampli
            if (segments < 1) throw new ArgumentException("Sample length is too short for the given window size and overlap.");
            //tablica na wyniki
            var result = new Complex[segments][];

            for (int i = 0; i < segments; i++)
            {
                Complex[] buffer = new Complex[windowSize];
                for (int j = 0; j < windowSize; j++)
                {
                    buffer[j] = samples[i * stride + j];//zapisanie sampli do bufora
                }
                //fft
                Fourier.Forward(buffer, FourierOptions.Matlab);
                //dla danego segmentu
                result[i] = buffer;

            }
            return result;
        }
    }
}
