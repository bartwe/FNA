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

namespace Microsoft.Xna.Framework.Audio
{
	/* This is a device that deliberately does as little as possible, allowing
	 * for no sound without throwing NoAudioHardwareExceptions. This is not a
	 * part of the XNA4 spec, however, so behavior here is entirely undefined!
	 * -flibit
	 */
	internal class NullALDevice : IALDevice
	{
		static ALBuffer NullBuffer = new ALBuffer(uint.MaxValue, TimeSpan.Zero, 1, 1);

		private class NullReverb : IALReverb
		{
		}

		public void Update()
		{
			// No-op, duh.
		}

		public void Dispose()
		{
			// No-op, duh.
		}

		public ReadOnlyCollection<RendererDetail> GetDevices()
		{
			return new ReadOnlyCollection<RendererDetail>(
				new List<RendererDetail>()
			);
		}

		public ReadOnlyCollection<Microphone> GetCaptureDevices()
		{
			return new ReadOnlyCollection<Microphone>(
				new List<Microphone>()
			);
		}

		public void SetMasterVolume(float volume)
		{
			// No-op, duh.
		}

		public void SetDopplerScale(float volume)
		{
			// No-op, duh.
		}

		public void SetSpeedOfSound(float volume)
		{
			// No-op, duh.
		}

		public ALBuffer GenBuffer(int sampleRate, AudioChannels channels)
		{
			return  NullBuffer;
		}

		public unsafe ALBuffer GenBuffer(
			void* data,
            int dataLength,
			uint sampleRate,
			uint channels,
			uint loopStart,
			uint loopEnd,
			bool isADPCM,
			uint formatParameter
		) {
            return NullBuffer;
		}

		public void DeleteBuffer(ALBuffer buffer)
		{
			// No-op, duh.
		}

		public void SetBufferData(
			ALBuffer buffer,
			IntPtr data,
			int offset,
			int count
		) {
			// No-op, duh.
		}

		public void SetBufferFloatData(
			ALBuffer buffer,
			IntPtr data,
			int offset,
			int count
		) {
			// No-op, duh.
		}

		public ALBuffer ConvertStereoToMono(ALBuffer buffer)
		{
			// No-op, we should never get here!
            return NullBuffer;
		}

		public ALSourceHandle GenSource()
		{
			return new ALSourceHandle(uint.MaxValue);
		}

		public ALSourceHandle GenSource(ALBuffer buffer, bool isXACT)
		{
            return new ALSourceHandle(uint.MaxValue);
		}

		public void StopAndDisposeSource(ALSourceHandle sourceHandle)
		{
			// No-op, duh.
		}

		public void PlaySource(ALSourceHandle sourceHandle)
		{
			// No-op, duh.
		}

		public void PauseSource(ALSourceHandle sourceHandle)
		{
			// No-op, duh.
		}

		public void ResumeSource(ALSourceHandle sourceHandle)
		{
			// No-op, duh.
		}

		public SoundState GetSourceState(ALSourceHandle sourceHandle)
		{
			/* FIXME: This return value is highly volatile!
			 * You can't necessarily do Stopped, because then stuff like Song
			 * explodes, but SoundState.Playing doesn't make a whole lot of
			 * sense either. This at least prevents annoyances like Song errors
			 * from happening and, for the most part, claims to be "playing"
			 * depending on how you ask for a source's state.
			 * -flibit
			 */
			return SoundState.Paused;
		}

		public void SetSourceVolume(ALSourceHandle sourceHandle, float volume)
		{
			// No-op, duh.
		}

		public void SetSourceLooped(ALSourceHandle sourceHandle, bool looped)
		{
			// No-op, duh.
		}

		public void SetSourcePan(ALSourceHandle sourceHandle, float pan)
		{
			// No-op, duh.
		}

		public void SetSourcePosition(ALSourceHandle sourceHandle, Vector3 pos)
		{
			// No-op, duh.
		}

		public void SetSourcePitch(ALSourceHandle sourceHandle, float pitch, bool clamp)
		{
			// No-op, duh.
		}

		public void SetSourceReverb(ALSourceHandle sourceHandle, IALReverb reverb)
		{
			// No-op, duh.
		}

		public void SetSourceLowPassFilter(ALSourceHandle sourceHandle, float hfGain)
		{
			// No-op, duh.
		}

		public void SetSourceHighPassFilter(ALSourceHandle sourceHandle, float lfGain)
		{
			// No-op, duh.
		}

		public void SetSourceBandPassFilter(ALSourceHandle sourceHandle, float hfGain, float lfGain)
		{
			// No-op, duh.
		}

		public void QueueSourceBuffer(ALSourceHandle sourceHandle, ALBuffer buffer)
		{
			// No-op, duh.
		}

		public void DequeueSourceBuffers(
			ALSourceHandle sourceHandle,
			int buffersToDequeue,
			Queue<ALBuffer> errorCheck
		) {
			// No-op, duh.
		}

		public int CheckProcessedBuffers(ALSourceHandle sourceHandle)
		{
			return 0;
		}

		public void GetBufferData(
			ALSourceHandle sourceHandle,
			ALBuffer[] buffer,
			IntPtr samples,
			int samplesLen,
			AudioChannels channels
		) {
			// No-op, duh.
		}

		public IALReverb GenReverb(DSPParameter[] parameters)
		{
			return new NullReverb();
		}

		public void DeleteReverb(IALReverb reverb)
		{
			// No-op, duh.
		}

		public void CommitReverbChanges(IALReverb reverb)
		{
			// No-op, duh.
		}

		public void SetReverbReflectionsDelay(IALReverb reverb, float value)
		{
			// No-op, duh.
		}

		public void SetReverbDelay(IALReverb reverb, float value)
		{
			// No-op, duh.
		}

		public void SetReverbPositionLeft(IALReverb reverb, float value)
		{
			// No-op, duh.
		}

		public void SetReverbPositionRight(IALReverb reverb, float value)
		{
			// No-op, duh.
		}

		public void SetReverbPositionLeftMatrix(IALReverb reverb, float value)
		{
			// No-op, duh.
		}

		public void SetReverbPositionRightMatrix(IALReverb reverb, float value)
		{
			// No-op, duh.
		}

		public void SetReverbEarlyDiffusion(IALReverb reverb, float value)
		{
			// No-op, duh.
		}

		public void SetReverbLateDiffusion(IALReverb reverb, float value)
		{
			// No-op, duh.
		}

		public void SetReverbLowEQGain(IALReverb reverb, float value)
		{
			// No-op, duh.
		}

		public void SetReverbLowEQCutoff(IALReverb reverb, float value)
		{
			// No-op, duh.
		}

		public void SetReverbHighEQGain(IALReverb reverb, float value)
		{
			// No-op, duh.
		}

		public void SetReverbHighEQCutoff(IALReverb reverb, float value)
		{
			// No-op, duh.
		}

		public void SetReverbRearDelay(IALReverb reverb, float value)
		{
			// No-op, duh.
		}

		public void SetReverbRoomFilterFrequency(IALReverb reverb, float value)
		{
			// No-op, duh.
		}

		public void SetReverbRoomFilterMain(IALReverb reverb, float value)
		{
			// No-op, duh.
		}

		public void SetReverbRoomFilterHighFrequency(IALReverb reverb, float value)
		{
			// No-op, duh.
		}

		public void SetReverbReflectionsGain(IALReverb reverb, float value)
		{
			// No-op, duh.
		}

		public void SetReverbGain(IALReverb reverb, float value)
		{
			// No-op, duh.
		}

		public void SetReverbDecayTime(IALReverb reverb, float value)
		{
			// No-op, duh.
		}

		public void SetReverbDensity(IALReverb reverb, float value)
		{
			// No-op, duh.
		}

		public void SetReverbRoomSize(IALReverb reverb, float value)
		{
			// No-op, duh.
		}

		public void SetReverbWetDryMix(IALReverb reverb, float value)
		{
			// No-op, duh.
		}

		public IntPtr StartDeviceCapture(string name, int sampleRate, int bufSize)
		{
			return IntPtr.Zero;
		}

		public void StopDeviceCapture(IntPtr handle)
		{
			// No-op, duh.
		}

		public int CaptureSamples(IntPtr handle, IntPtr buffer, int count)
		{
			return 0;
		}

		public bool CaptureHasSamples(IntPtr handle)
		{
			return false;
		}
	}
}
