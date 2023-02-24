using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Media;
using Windows.Media.Effects;
using Windows.Media.MediaProperties;

namespace AudioEffectComponent
{
    public sealed class AudioFadeEffect : IBasicAudioEffect
    {
        private static IReadOnlyList<AudioEncodingProperties> supportedEncodingProperties;

        private AudioEncodingProperties encodingProperties;
        private IPropertySet configuration;

        public void SetEncodingProperties(AudioEncodingProperties encodingProperties)
        {
            this.encodingProperties = encodingProperties;
        }

        public unsafe void ProcessFrame(ProcessAudioFrameContext context)
        {
            if (configuration.TryGetValue("AudioFade_Disabled", out _)) return;

            if (context.InputFrame.IsReadOnly) return;

            var _time = context.InputFrame.RelativeTime;
            if (!_time.HasValue) return;
            var time = _time.Value;

            var interval = 3f;
            configuration.TryGetValue("AudioFade_FadeDuration", out object intervalProperty);
            if (intervalProperty != null)
            {
                interval = (float)intervalProperty;
            }

            var amount = 1f;

            if (time.TotalSeconds < interval)
            {
                amount = (float)(time.TotalSeconds / interval);
            }
            else if (configuration.TryGetValue("AudioFade_TrackDuration", out var _duration)
                && _duration is TimeSpan duration)
            {
                amount = Math.Min(1f, (float)((duration.TotalSeconds - time.TotalSeconds) / interval));
            }

            using (AudioBuffer inputBuffer = context.InputFrame.LockBuffer(AudioBufferAccessMode.Read))
            using (IMemoryBufferReference inputReference = inputBuffer.CreateReference())
            {
                ((IMemoryBufferByteAccess)inputReference).GetBuffer(out byte* inputDataInBytes, out uint inputCapacity);

                float* inputDataInFloat = (float*)inputDataInBytes;

                int dataInFloatLength = (int)inputBuffer.Length / sizeof(float);

                for (int i = 0; i < dataInFloatLength; i++)
                {
                    inputDataInFloat[i] *= amount;
                }
            }
        }

        public void Close(MediaEffectClosedReason reason)
        {

        }

        public void DiscardQueuedFrames()
        {

        }

        public IReadOnlyList<AudioEncodingProperties> SupportedEncodingProperties
        {
            get
            {
                if (supportedEncodingProperties == null)
                {
                    AudioEncodingProperties encodingProps1 = AudioEncodingProperties.CreatePcm(44100, 1, 32);
                    encodingProps1.Subtype = MediaEncodingSubtypes.Float;
                    AudioEncodingProperties encodingProps2 = AudioEncodingProperties.CreatePcm(48000, 1, 32);
                    encodingProps2.Subtype = MediaEncodingSubtypes.Float;

                    AudioEncodingProperties encodingProps3 = AudioEncodingProperties.CreatePcm(44100, 2, 32);
                    encodingProps3.Subtype = MediaEncodingSubtypes.Float;
                    AudioEncodingProperties encodingProps4 = AudioEncodingProperties.CreatePcm(48000, 2, 32);
                    encodingProps4.Subtype = MediaEncodingSubtypes.Float;

                    AudioEncodingProperties encodingProps5 = AudioEncodingProperties.CreatePcm(96000, 2, 32);
                    encodingProps5.Subtype = MediaEncodingSubtypes.Float;
                    AudioEncodingProperties encodingProps6 = AudioEncodingProperties.CreatePcm(192000, 2, 32);
                    encodingProps6.Subtype = MediaEncodingSubtypes.Float;

                    supportedEncodingProperties = new List<AudioEncodingProperties>()
                    {
                        encodingProps1,
                        encodingProps2,
                        encodingProps3,
                        encodingProps4,
                        encodingProps5,
                        encodingProps6,
                    };
                }
                return supportedEncodingProperties;
            }
        }

        public bool UseInputFrameForOutput => true;

        public void SetProperties(IPropertySet configuration)
        {
            this.configuration = configuration;
        }
    }

    public sealed class AudioGainEffect : IBasicAudioEffect
    {
        private static IReadOnlyList<AudioEncodingProperties> supportedEncodingProperties;

        private AudioEncodingProperties encodingProperties;
        private IPropertySet configuration;
        private float LastAudioGainValue = 1.0f;
        private float AudioGainMultiplier = 1.0f;
        public void SetEncodingProperties(AudioEncodingProperties encodingProperties)
        {
            this.encodingProperties = encodingProperties;
        }

        public unsafe void ProcessFrame(ProcessAudioFrameContext context)
        {
            if (configuration.TryGetValue("AudioGain_Disabled", out _)) return;

            if (context.InputFrame.IsReadOnly) return;

            configuration.TryGetValue("AudioGain_GainValue", out object audioGainProperty);
            if (audioGainProperty != null)
            {
                float audioGainValue = (float)audioGainProperty;
                if (LastAudioGainValue != audioGainValue)
                {
                    LastAudioGainValue = audioGainValue;
                    AudioGainMultiplier = GetAudioGainMultiplier(audioGainValue);
                }
            }
            else
            {
                if (LastAudioGainValue != 1.0f)
                {
                    LastAudioGainValue = 1.0f;
                    AudioGainMultiplier = GetAudioGainMultiplier(1.0f);
                }
            }
            using (AudioBuffer inputBuffer = context.InputFrame.LockBuffer(AudioBufferAccessMode.Read))
            using (IMemoryBufferReference inputReference = inputBuffer.CreateReference())
            {
                ((IMemoryBufferByteAccess)inputReference).GetBuffer(out byte* inputDataInBytes, out uint inputCapacity);

                float* inputDataInFloat = (float*)inputDataInBytes;

                int dataInFloatLength = (int)inputBuffer.Length / sizeof(float);

                for (int i = 0; i < dataInFloatLength; i++)
                {
                    inputDataInFloat[i] *= AudioGainMultiplier;
                }
            }
        }

        public void Close(MediaEffectClosedReason reason)
        {

        }

        public void DiscardQueuedFrames()
        {
        }

        public IReadOnlyList<AudioEncodingProperties> SupportedEncodingProperties
        {
            get
            {
                if (supportedEncodingProperties == null)
                {
                    AudioEncodingProperties encodingProps1 = AudioEncodingProperties.CreatePcm(44100, 1, 32);
                    encodingProps1.Subtype = MediaEncodingSubtypes.Float;
                    AudioEncodingProperties encodingProps2 = AudioEncodingProperties.CreatePcm(48000, 1, 32);
                    encodingProps2.Subtype = MediaEncodingSubtypes.Float;

                    AudioEncodingProperties encodingProps3 = AudioEncodingProperties.CreatePcm(44100, 2, 32);
                    encodingProps3.Subtype = MediaEncodingSubtypes.Float;
                    AudioEncodingProperties encodingProps4 = AudioEncodingProperties.CreatePcm(48000, 2, 32);
                    encodingProps4.Subtype = MediaEncodingSubtypes.Float;

                    AudioEncodingProperties encodingProps5 = AudioEncodingProperties.CreatePcm(96000, 2, 32);
                    encodingProps5.Subtype = MediaEncodingSubtypes.Float;
                    AudioEncodingProperties encodingProps6 = AudioEncodingProperties.CreatePcm(192000, 2, 32);
                    encodingProps6.Subtype = MediaEncodingSubtypes.Float;

                    supportedEncodingProperties = new List<AudioEncodingProperties>()
                    {
                        encodingProps1,
                        encodingProps2,
                        encodingProps3,
                        encodingProps4,
                        encodingProps5,
                        encodingProps6,
                    };
                }
                return supportedEncodingProperties;
            }
        }

        public bool UseInputFrameForOutput => true;

        public void SetProperties(IPropertySet configuration)
        {
            this.configuration = configuration;
        }
        public static float GetAudioGainMultiplier(float audioGainValue)
        {
            var gainValue = (float)Math.Pow(10, audioGainValue / 20);
            return gainValue;
        }
    }

    // Using the COM interface IMemoryBufferByteAccess allows us to access the underlying byte array in an AudioFrame
    [ComImport]
    [Guid("5B0D3235-4DBA-4D44-865E-8F1D0E4FD04D")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal unsafe interface IMemoryBufferByteAccess
    {
        void GetBuffer(out byte* buffer, out uint capacity);
    }
}
