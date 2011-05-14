﻿using System;
using System.Collections.Generic;
using System.Text;

namespace NAudio.Wave
{
    /// <summary>
    /// Utility class that takes an IWaveProvider input at any bit depth
    /// and exposes it as an IWaveProviderFloat. Turns mono inputs into stereo,
    /// and allows adjusting of volume
    /// (The eventual successor to WaveChannel32)
    /// </summary>
    public class WaveChannelFloat : WaveProvider32
    {
        private IWaveProviderFloat sourceProviderFloat;
        private float volume;

        /// <summary>
        /// Initialises a new instance of WaveChannelFloat
        /// </summary>
        /// <param name="sourceProvider">Source provider, must be PCM or IEEE</param>
        public WaveChannelFloat(IWaveProvider sourceProvider)
        {
            this.volume = 1.0f;
            this.SetWaveFormat(sourceProvider.WaveFormat.SampleRate, 2);

            if (sourceProvider.WaveFormat.Encoding == WaveFormatEncoding.Pcm)
            {
                // go to float
                if (sourceProvider.WaveFormat.BitsPerSample == 8)
                {
                    sourceProviderFloat = new Pcm8BitToWaveProviderFloat(sourceProvider);
                }
                else if (sourceProvider.WaveFormat.BitsPerSample == 16)
                {
                    sourceProviderFloat = new Pcm16BitToWaveProviderFloat(sourceProvider);
                }
                else if (sourceProvider.WaveFormat.BitsPerSample == 24)
                {
                    sourceProviderFloat = new Pcm24BitToWaveProviderFloat(sourceProvider);
                }
            }
            else if (sourceProvider.WaveFormat.Encoding == WaveFormatEncoding.IeeeFloat)
            {
                sourceProviderFloat = new WaveProviderToWaveProviderFloat(sourceProvider);
            }
            else
            {
                throw new ArgumentException("Unsupported source encoding");
            }
            if (sourceProviderFloat.WaveFormat.Channels == 1)
            {
                sourceProviderFloat = new MonoToStereoProviderFloat(sourceProviderFloat);
            }
        }

        /// <summary>
        /// Reads samples from this wave provider
        /// </summary>
        /// <param name="buffer">Sample buffer</param>
        /// <param name="offset">Offset into sample buffer</param>
        /// <param name="sampleCount">Number of samples desired</param>
        /// <returns>Number of samples read</returns>
        public override int Read(float[] buffer, int offset, int sampleCount)
        {
            int samplesRead = sourceProviderFloat.Read(buffer, offset, sampleCount);
            if (volume != 1f)
            {
                for (int n = 0; n < sampleCount; n++)
                {
                    buffer[offset + n] *= volume;
                }
            }
            return samplesRead;
        }

        /// <summary>
        /// Allows adjusting the volume, 1.0f = full volume
        /// </summary>
        public float Volume
        {
            get
            {
                return volume;
            }
            set
            {
                volume = value;
            }
        }
    }
}
