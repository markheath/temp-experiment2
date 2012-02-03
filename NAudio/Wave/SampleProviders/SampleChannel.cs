﻿using System;
using System.Collections.Generic;
using System.Text;

namespace NAudio.Wave.SampleProviders
{
    /// <summary>
    /// Utility class that takes an IWaveProvider input at any bit depth
    /// and exposes it as an ISampleProvider. Turns mono inputs into stereo,
    /// and allows adjusting of volume
    /// (The eventual successor to WaveChannel32)
    /// This class also serves as an example of how you can link together several simple 
    /// Sample Providers to form a more useful class.
    /// </summary>
    public class SampleChannel : ISampleProvider
    {
        private VolumeSampleProvider volumeProvider;
        private MeteringSampleProvider preVolumeMeter;
        private WaveFormat waveFormat;

        /// <summary>
        /// Initialises a new instance of SampleChannel
        /// </summary>
        /// <param name="waveProvider">Source wave provider, must be PCM or IEEE</param>
        public SampleChannel(IWaveProvider waveProvider)
        {
            ISampleProvider sampleProvider = SampleProviderConverters.ConvertWaveProviderIntoSampleProvider(waveProvider);            
            if (sampleProvider.WaveFormat.Channels == 1)
            {
                sampleProvider = new MonoToStereoSampleProvider(sampleProvider);
            }
            this.waveFormat = sampleProvider.WaveFormat;
            // let's put the meter before the volume (useful for drawing waveforms)
            this.preVolumeMeter = new MeteringSampleProvider(sampleProvider);
            this.volumeProvider = new VolumeSampleProvider(preVolumeMeter);
        }

        /// <summary>
        /// Reads samples from this sample provider
        /// </summary>
        /// <param name="buffer">Sample buffer</param>
        /// <param name="offset">Offset into sample buffer</param>
        /// <param name="sampleCount">Number of samples desired</param>
        /// <returns>Number of samples read</returns>
        public int Read(float[] buffer, int offset, int sampleCount)
        {
            return volumeProvider.Read(buffer, offset, sampleCount);
        }

        /// <summary>
        /// The WaveFormat of this Sample Provider
        /// </summary>
        public WaveFormat WaveFormat
        {
            get { return this.waveFormat; }
        }

        /// <summary>
        /// Allows adjusting the volume, 1.0f = full volume
        /// </summary>
        public float Volume
        {
            get { return volumeProvider.Volume; }
            set { volumeProvider.Volume = value; }
        }

        /// <summary>
        /// Raised periodically to inform the user of the max volume
        /// (before the volume meter)
        /// </summary>
        public event EventHandler<StreamVolumeEventArgs> PreVolumeMeter
        {
            add { this.preVolumeMeter.StreamVolume += value; }
            remove { this.preVolumeMeter.StreamVolume -= value; }
        }
    }
}
