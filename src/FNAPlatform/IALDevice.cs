#region License

/* FNA - XNA4 Reimplementation for Desktop Platforms
 * Copyright 2009-2018 Ethan Lee and the MonoGame Team
 *
 * Released under the Microsoft Public License.
 * See LICENSE for details.
 */

#endregion

#region Using Statements

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

#endregion

namespace Microsoft.Xna.Framework.Audio {
    interface IALDevice {
        void Update();
        void Dispose();
        ReadOnlyCollection<RendererDetail> GetDevices();
        ReadOnlyCollection<Microphone> GetCaptureDevices();

        void SetMasterVolume(float volume);
        void SetDopplerScale(float scale);
        void SetSpeedOfSound(float speed);

        IALBuffer GenBuffer(int sampleRate, AudioChannels channels);

        IALBuffer GenBuffer(
            byte[] data,
            uint sampleRate,
            uint channels,
            uint loopStart,
            uint loopEnd,
            bool isADPCM,
            uint formatParameter
            );

        void DeleteBuffer(IALBuffer buffer);

        void SetBufferData(
            IALBuffer buffer,
            IntPtr data,
            int offset,
            int count
            );

        void SetBufferFloatData(
            IALBuffer buffer,
            IntPtr data,
            int offset,
            int count
            );

        IALBuffer ConvertStereoToMono(IALBuffer buffer);

        ALSourceHandle GenSource();
        ALSourceHandle GenSource(IALBuffer buffer, bool isXACT);
        void StopAndDisposeSource(ALSourceHandle sourceHandle);
        void PlaySource(ALSourceHandle sourceHandle);
        void PauseSource(ALSourceHandle sourceHandle);
        void ResumeSource(ALSourceHandle sourceHandle);
        SoundState GetSourceState(ALSourceHandle sourceHandle);
        void SetSourceVolume(ALSourceHandle sourceHandle, float volume);
        void SetSourceLooped(ALSourceHandle sourceHandle, bool looped);
        void SetSourcePan(ALSourceHandle sourceHandle, float pan);
        void SetSourcePosition(ALSourceHandle sourceHandle, Vector3 pos);
        void SetSourcePitch(ALSourceHandle sourceHandle, float pitch, bool clamp);
        void SetSourceReverb(ALSourceHandle sourceHandle, IALReverb reverb);
        void SetSourceLowPassFilter(ALSourceHandle sourceHandle, float hfGain);
        void SetSourceHighPassFilter(ALSourceHandle sourceHandle, float lfGain);
        void SetSourceBandPassFilter(ALSourceHandle sourceHandle, float hfGain, float lfGain);
        void QueueSourceBuffer(ALSourceHandle sourceHandle, IALBuffer buffer);

        void DequeueSourceBuffers(
            ALSourceHandle sourceHandle,
            int buffersToDequeue,
            Queue<IALBuffer> errorCheck
            );

        int CheckProcessedBuffers(ALSourceHandle sourceHandle);

        void GetBufferData(
            ALSourceHandle sourceHandle,
            IALBuffer[] buffer,
            IntPtr samples,
            int samplesLen,
            AudioChannels channels
            );

        IALReverb GenReverb(DSPParameter[] parameters);
        void DeleteReverb(IALReverb reverb);
        void CommitReverbChanges(IALReverb reverb);
        void SetReverbReflectionsDelay(IALReverb reverb, float value);
        void SetReverbDelay(IALReverb reverb, float value);
        void SetReverbPositionLeft(IALReverb reverb, float value);
        void SetReverbPositionRight(IALReverb reverb, float value);
        void SetReverbPositionLeftMatrix(IALReverb reverb, float value);
        void SetReverbPositionRightMatrix(IALReverb reverb, float value);
        void SetReverbEarlyDiffusion(IALReverb reverb, float value);
        void SetReverbLateDiffusion(IALReverb reverb, float value);
        void SetReverbLowEQGain(IALReverb reverb, float value);
        void SetReverbLowEQCutoff(IALReverb reverb, float value);
        void SetReverbHighEQGain(IALReverb reverb, float value);
        void SetReverbHighEQCutoff(IALReverb reverb, float value);
        void SetReverbRearDelay(IALReverb reverb, float value);
        void SetReverbRoomFilterFrequency(IALReverb reverb, float value);
        void SetReverbRoomFilterMain(IALReverb reverb, float value);
        void SetReverbRoomFilterHighFrequency(IALReverb reverb, float value);
        void SetReverbReflectionsGain(IALReverb reverb, float value);
        void SetReverbGain(IALReverb reverb, float value);
        void SetReverbDecayTime(IALReverb reverb, float value);
        void SetReverbDensity(IALReverb reverb, float value);
        void SetReverbRoomSize(IALReverb reverb, float value);
        void SetReverbWetDryMix(IALReverb reverb, float value);

        IntPtr StartDeviceCapture(string name, int sampleRate, int bufSize);
        void StopDeviceCapture(IntPtr handle);
        int CaptureSamples(IntPtr handle, IntPtr buffer, int count);
        bool CaptureHasSamples(IntPtr handle);
    }

    interface IALBuffer {
        TimeSpan Duration { get; }
        int Channels { get; }
        int SampleRate { get; }
    }

    struct ALSourceHandle {
        public uint Handle { get; private set; }
        public static ALSourceHandle NullHandle = new ALSourceHandle(0);

        public ALSourceHandle(uint handle) : this() {
            Handle = handle;
        }

        public bool IsNull() {
            return Handle == 0;
        }
    }

    interface IALReverb {}
}
