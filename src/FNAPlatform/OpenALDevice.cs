#region License
/* FNA - XNA4 Reimplementation for Desktop Platforms
 * Copyright 2009-2018 Ethan Lee and the MonoGame Team
 *
 * Released under the Microsoft Public License.
 * See LICENSE for details.
 */
#endregion

#region VERBOSE_AL_DEBUGGING Option
// #define VERBOSE_AL_DEBUGGING
/* OpenAL does not have a function similar to ARB_debug_output. Because of this,
 * we only have alGetError to debug. In DEBUG, we call this once per frame.
 *
 * If you enable this define, we call this after every single AL operation, and
 * throw an Exception when any errors show up. This makes finding problems a lot
 * easier, but calling alGetError so often can slow things down.
 * -flibit
 */
#endregion

#region Using Statements
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.InteropServices;

using OpenAL;
#endregion

namespace Microsoft.Xna.Framework.Audio
{
	internal class OpenALDevice : IALDevice
	{
		#region OpenAL Reverb Effect Container Class

		private class OpenALReverb : IALReverb
		{
			public uint SlotHandle
			{
				get;
				private set;
			}

			public uint EffectHandle
			{
				get;
				private set;
			}

			public OpenALReverb(uint slot, uint effect)
			{
				SlotHandle = slot;
				EffectHandle = effect;
			}
		}

		#endregion

		#region Private ALC Variables

		// OpenAL Device/Context Handles
		private IntPtr alDevice;
		private IntPtr alContext;

		#endregion

		#region Private EFX Variables

		// OpenAL Filter Handle
		private uint INTERNAL_alFilter;

		#endregion

		#region Public Constructor

		public OpenALDevice()
		{
			string envDevice = Environment.GetEnvironmentVariable("FNA_AUDIO_DEVICE_NAME");
			if (String.IsNullOrEmpty(envDevice))
			{
				/* Be sure ALC won't explode if the variable doesn't exist.
				 * But, fail if the device name is wrong. The user needs to know
				 * if their environment variable was incorrect.
				 * -flibit
				 */
				envDevice = String.Empty;
			}
			alDevice = ALC10.alcOpenDevice(envDevice);
			if (CheckALCError() || alDevice == IntPtr.Zero)
			{
				throw new InvalidOperationException("Could not open audio device!");
			}

			int[] attribute = new int[0];
			alContext = ALC10.alcCreateContext(alDevice, attribute);
			if (CheckALCError() || alContext == IntPtr.Zero)
			{
				Dispose();
				throw new InvalidOperationException("Could not create OpenAL context");
			}

			ALC10.alcMakeContextCurrent(alContext);
			if (CheckALCError())
			{
				Dispose();
				throw new InvalidOperationException("Could not make OpenAL context current");
			}

			float[] ori = new float[]
			{
				0.0f, 0.0f, -1.0f, 0.0f, 1.0f, 0.0f
			};
			AL10.alListenerfv(AL10.AL_ORIENTATION, ori);
			AL10.alListener3f(AL10.AL_POSITION, 0.0f, 0.0f, 0.0f);
			AL10.alListener3f(AL10.AL_VELOCITY, 0.0f, 0.0f, 0.0f);
			AL10.alListenerf(AL10.AL_GAIN, 1.0f);

			EFX.alGenFilters(1, out INTERNAL_alFilter);
		}

		#endregion

		#region Public Dispose Method

		public void Dispose()
		{
			EFX.alDeleteFilters(1, ref INTERNAL_alFilter);

			ALC10.alcMakeContextCurrent(IntPtr.Zero);
			if (alContext != IntPtr.Zero)
			{
				ALC10.alcDestroyContext(alContext);
				alContext = IntPtr.Zero;
			}
			if (alDevice != IntPtr.Zero)
			{
				ALC10.alcCloseDevice(alDevice);
				alDevice = IntPtr.Zero;
			}
		}

		#endregion

		#region Public Update Method

		public void Update()
		{
#if DEBUG
			CheckALError();
#endif
		}

		#endregion

		#region Public Listener Methods

		public void SetMasterVolume(float volume)
		{
			/* FIXME: How to ignore listener for individual sources? -flibit
			 * AL10.alListenerf(AL10.AL_GAIN, volume);
			 * Media.MediaPlayer.Queue.ActiveSong.Volume = Media.MediaPlayer.Volume;
			 */
		}

		public void SetDopplerScale(float scale)
		{
			AL10.alDopplerFactor(scale);
		}

		public void SetSpeedOfSound(float speed)
		{
			AL11.alSpeedOfSound(speed);
		}

		#endregion

		#region OpenAL Buffer Methods

		public ALBuffer GenBuffer(int sampleRate, AudioChannels channels)
		{
			uint result;
			AL10.alGenBuffers(1, out result);
#if VERBOSE_AL_DEBUGGING
			CheckALError();
#endif
			return new ALBuffer(result, TimeSpan.Zero, (int) channels, sampleRate);
		}

		int[] _loopArgumentsSpare = new int[2];

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
			uint result;

			// Generate the buffer now, in case we need to perform alBuffer ops.
			AL10.alGenBuffers(1, out result);
#if VERBOSE_AL_DEBUGGING
			CheckALError();
#endif

			int format;
			int length = dataLength;
			if (isADPCM)
			{
				format = (channels == 2) ?
					ALEXT.AL_FORMAT_STEREO_MSADPCM_SOFT :
					ALEXT.AL_FORMAT_MONO_MSADPCM_SOFT;
				AL10.alBufferi(
					result,
					ALEXT.AL_UNPACK_BLOCK_ALIGNMENT_SOFT,
					(int) formatParameter
				);
			}
			else
			{
				if (formatParameter == 1)
				{
					format = (channels == 2) ?
						AL10.AL_FORMAT_STEREO16:
						AL10.AL_FORMAT_MONO16;

					/* We have to perform extra data validation on
					 * PCM16 data, as the MS SoundEffect builder will
					 * leave extra bytes at the end which will confuse
					 * alBufferData and throw an AL_INVALID_VALUE.
					 * -flibit
					 */
					length &= 0x7FFFFFFE;
				}
				else
				{
					format = (channels == 2) ?
						AL10.AL_FORMAT_STEREO8:
						AL10.AL_FORMAT_MONO8;
				}
			}

			// Load it!
			AL10.alBufferData(
				result,
				format,
				(IntPtr)data,
				length,
				(int) sampleRate
			);
#if VERBOSE_AL_DEBUGGING
			CheckALError();
#endif

			// Calculate the duration now, after we've unpacked the buffer
			int bufLen, bits;
			AL10.alGetBufferi(
				result,
				AL10.AL_SIZE,
				out bufLen
			);
			AL10.alGetBufferi(
				result,
				AL10.AL_BITS,
				out bits
			);
			if (bufLen == 0 || bits == 0)
			{
				throw new InvalidOperationException(
					"OpenAL buffer allocation failed! ErrorText: " + CheckALErrorString()
				);
			}
			TimeSpan resultDur = TimeSpan.FromSeconds(
				bufLen /
				(bits / 8) /
				channels /
				((double) sampleRate)
			);

			// Set the loop points, if applicable
			if (loopStart > 0 || loopEnd > 0)
				unsafe {
					_loopArgumentsSpare[0] = (int)loopStart;
					_loopArgumentsSpare[1] = (int)loopEnd;
					fixed (int* loopArgumentsp = _loopArgumentsSpare) {
						AL10.alBufferiv(
							result,
							ALEXT.AL_LOOP_POINTS_SOFT,
							loopArgumentsp
							);
					}
				}
#if VERBOSE_AL_DEBUGGING
			CheckALError();
#endif

			// Finally.
			return new ALBuffer(result, resultDur, (int) channels, (int) sampleRate);
		}

		public void DeleteBuffer(ALBuffer buffer)
		{
			uint handle = buffer.Handle;
			AL10.alDeleteBuffers(1, ref handle);
#if VERBOSE_AL_DEBUGGING
			CheckALError();
#endif
		}

		public void SetBufferData(
			ALBuffer buffer,
			IntPtr data,
			int offset,
			int count
		) {
			AL10.alBufferData(
				buffer.Handle,
				XNAToShort[buffer.Channels],
				data + offset,
				count,
				buffer.SampleRate
			);
#if VERBOSE_AL_DEBUGGING
			CheckALError();
#endif
		}

		public void SetBufferFloatData(
			ALBuffer buffer,
			IntPtr data,
			int offset,
			int count
		) {
			AL10.alBufferData(
				buffer.Handle,
				XNAToFloat[buffer.Channels],
				data + (offset * 4),
				count * 4,
				buffer.SampleRate
			);
#if VERBOSE_AL_DEBUGGING
			CheckALError();
#endif
		}

		public unsafe ALBuffer ConvertStereoToMono(ALBuffer buffer)
		{
			int bufLen, bits;
			AL10.alGetBufferi(
				buffer.Handle,
				AL10.AL_SIZE,
				out bufLen
			);
			AL10.alGetBufferi(
				buffer.Handle,
				AL10.AL_BITS,
				out bits
			);
			bits /= 8;
#if VERBOSE_AL_DEBUGGING
			CheckALError();
#endif

			byte[] data = new byte[bufLen];
			byte[] monoData = new byte[bufLen / 2];
			fixed (void* datap = data) {
				var dataPtr = (IntPtr)datap;
				ALEXT.alGetBufferSamplesSOFT(
					buffer.Handle,
					0,
					bufLen / bits / 2,
					ALEXT.AL_STEREO_SOFT,
					bits == 2 ? ALEXT.AL_SHORT_SOFT : ALEXT.AL_BYTE_SOFT,
					dataPtr
				);
	#if VERBOSE_AL_DEBUGGING
				CheckALError();
	#endif

				fixed (void* monoPtr = monoData) {
					if (bits == 2)
					{
						short* src = (short*) dataPtr;
						short* dst = (short*) monoPtr;
						for (int i = 0; i < monoData.Length / 2; i += 1)
						{
							dst[i] = (short) (((int) src[0] + (int) src[1]) / 2);
							src += 2;
						}
					}
					else
					{
						sbyte* src = (sbyte*) dataPtr;
						sbyte* dst = (sbyte*) monoPtr;
						for (int i = 0; i < monoData.Length; i += 1)
						{
							dst[i] = (sbyte) (((short) src[0] + (short) src[1]) / 2);
							src += 2;
						}
					}
				}
			}
			data = null;

			fixed(void* monoDatap = monoData)
			return GenBuffer(
				monoDatap,
				monoData.Length,
				(uint)buffer.SampleRate,
				1,
				0,
				0,
				false,
				(uint) bits - 1
			);
		}

		#endregion

		#region OpenAL Source Methods

		public ALSourceHandle GenSource()
		{
			uint result;
			AL10.alGenSources(1, out result);
#if VERBOSE_AL_DEBUGGING
			CheckALError();
#endif
			if (result == 0)
			{
				return ALSourceHandle.NullHandle;
			}
			AL10.alSourcef(
				result,
				AL10.AL_REFERENCE_DISTANCE,
				AudioDevice.DistanceScale
			);
			return new ALSourceHandle(result);
		}

		public ALSourceHandle GenSource(ALBuffer buffer, bool isXACT)
		{
			uint result;
			AL10.alGenSources(1, out result);
#if VERBOSE_AL_DEBUGGING
			CheckALError();
#endif
			if (result == 0)
			{
				return ALSourceHandle.NullHandle;
			}
			AL10.alSourcei(
				result,
				AL10.AL_BUFFER,
				(int) buffer.Handle
			);
			AL10.alSourcef(
				result,
				AL10.AL_REFERENCE_DISTANCE,
				AudioDevice.DistanceScale
			);
			if (isXACT)
			{
				AL10.alSourcef(
					result,
					AL10.AL_MAX_GAIN,
					AudioDevice.MAX_GAIN_VALUE
				);
			}
#if VERBOSE_AL_DEBUGGING
			CheckALError();
#endif
			return new ALSourceHandle(result);
		}

		public void StopAndDisposeSource(ALSourceHandle sourceHandle)
		{
			uint handle = sourceHandle.Handle;
			AL10.alSourceStop(handle);
#if VERBOSE_AL_DEBUGGING
			CheckALError();
#endif
			AL10.alDeleteSources(1, ref handle);
#if VERBOSE_AL_DEBUGGING
			CheckALError();
#endif
		}

		public void PlaySource(ALSourceHandle sourceHandle)
		{
			AL10.alSourcePlay(sourceHandle.Handle);
#if VERBOSE_AL_DEBUGGING
			CheckALError();
#endif
		}

		public void PauseSource(ALSourceHandle sourceHandle)
		{
			AL10.alSourcePause(sourceHandle.Handle);
#if VERBOSE_AL_DEBUGGING
			CheckALError();
#endif
		}

		public void ResumeSource(ALSourceHandle sourceHandle)
		{
			AL10.alSourcePlay(sourceHandle.Handle);
#if VERBOSE_AL_DEBUGGING
			CheckALError();
#endif
		}

		public SoundState GetSourceState(ALSourceHandle sourceHandle)
		{
			int state;
			AL10.alGetSourcei(
				sourceHandle.Handle,
				AL10.AL_SOURCE_STATE,
				out state
			);
#if VERBOSE_AL_DEBUGGING
			CheckALError();
#endif
			if (state == AL10.AL_PLAYING)
			{
				return SoundState.Playing;
			}
			else if (state == AL10.AL_PAUSED)
			{
				return SoundState.Paused;
			}
			return SoundState.Stopped;
		}

		public void SetSourceVolume(ALSourceHandle sourceHandle, float volume)
		{
			AL10.alSourcef(
				sourceHandle.Handle,
				AL10.AL_GAIN,
				volume * SoundEffect.MasterVolume // FIXME: alListener(AL_GAIN) -flibit
			);
#if VERBOSE_AL_DEBUGGING
			CheckALError();
#endif
		}

		public void SetSourceLooped(ALSourceHandle sourceHandle, bool looped)
		{
			AL10.alSourcei(
				sourceHandle.Handle,
				AL10.AL_LOOPING,
				looped ? 1 : 0
			);
#if VERBOSE_AL_DEBUGGING
			CheckALError();
#endif
		}

		public void SetSourcePan(ALSourceHandle sourceHandle, float pan)
		{
			AL10.alSource3f(
				sourceHandle.Handle,
				AL10.AL_POSITION,
				pan,
				0.0f,
				(float) -Math.Sqrt(1 - Math.Pow(pan, 2))
			);
#if VERBOSE_AL_DEBUGGING
			CheckALError();
#endif
		}

		public void SetSourcePosition(ALSourceHandle sourceHandle, Vector3 pos)
		{
			AL10.alSource3f(
				sourceHandle.Handle,
				AL10.AL_POSITION,
				pos.X,
				pos.Y,
				pos.Z
			);
#if VERBOSE_AL_DEBUGGING
			CheckALError();
#endif
		}

		public void SetSourcePitch(ALSourceHandle sourceHandle, float pitch, bool clamp)
		{
			/* XNA sets pitch bounds to [-1.0f, 1.0f], each end being one octave.
			 * OpenAL's AL_PITCH boundaries are (0.0f, INF).
			 * Consider the function f(x) = 2 ^ x
			 * The domain is (-INF, INF) and the range is (0, INF).
			 * 0.0f is the original pitch for XNA, 1.0f is the original pitch for OpenAL.
			 * Note that f(0) = 1, f(1) = 2, f(-1) = 0.5, and so on.
			 * XNA's pitch values are on the domain, OpenAL's are on the range.
			 * Remember: the XNA limit is arbitrarily between two octaves on the domain.
			 * To convert, we just plug XNA pitch into f(x).
			 * -flibit
			 */
			if (clamp && (pitch < -1.0f || pitch > 1.0f))
			{
				throw new IndexOutOfRangeException("XNA PITCH MUST BE WITHIN [-1.0f, 1.0f]!");
			}
			AL10.alSourcef(
				sourceHandle.Handle,
				AL10.AL_PITCH,
				(float) Math.Pow(2, pitch)
			);
#if VERBOSE_AL_DEBUGGING
			CheckALError();
#endif
		}

		public void SetSourceReverb(ALSourceHandle sourceHandle, IALReverb reverb)
		{
			AL10.alSource3i(
				sourceHandle.Handle,
				EFX.AL_AUXILIARY_SEND_FILTER,
				(int) (reverb as OpenALReverb).SlotHandle,
				0,
				0
			);
#if VERBOSE_AL_DEBUGGING
			CheckALError();
#endif
		}

		public void SetSourceLowPassFilter(ALSourceHandle sourceHandle, float hfGain)
		{
			EFX.alFilteri(INTERNAL_alFilter, EFX.AL_FILTER_TYPE, EFX.AL_FILTER_LOWPASS);
			EFX.alFilterf(INTERNAL_alFilter, EFX.AL_LOWPASS_GAINHF, hfGain);
			AL10.alSourcei(
				sourceHandle.Handle,
				EFX.AL_DIRECT_FILTER,
				(int) INTERNAL_alFilter
			);
#if VERBOSE_AL_DEBUGGING
			CheckALError();
#endif
		}

		public void SetSourceHighPassFilter(ALSourceHandle sourceHandle, float lfGain)
		{
			EFX.alFilteri(INTERNAL_alFilter, EFX.AL_FILTER_TYPE, EFX.AL_FILTER_HIGHPASS);
			EFX.alFilterf(INTERNAL_alFilter, EFX.AL_HIGHPASS_GAINLF, lfGain);
			AL10.alSourcei(
				sourceHandle.Handle,
				EFX.AL_DIRECT_FILTER,
				(int) INTERNAL_alFilter
			);
#if VERBOSE_AL_DEBUGGING
			CheckALError();
#endif
		}

		public void SetSourceBandPassFilter(ALSourceHandle sourceHandle, float hfGain, float lfGain)
		{
			EFX.alFilteri(INTERNAL_alFilter, EFX.AL_FILTER_TYPE, EFX.AL_FILTER_BANDPASS);
			EFX.alFilterf(INTERNAL_alFilter, EFX.AL_BANDPASS_GAINHF, hfGain);
			EFX.alFilterf(INTERNAL_alFilter, EFX.AL_BANDPASS_GAINLF, lfGain);
			AL10.alSourcei(
				sourceHandle.Handle,
				EFX.AL_DIRECT_FILTER,
				(int) INTERNAL_alFilter
			);
#if VERBOSE_AL_DEBUGGING
			CheckALError();
#endif
		}

		public void QueueSourceBuffer(ALSourceHandle sourceHandle, ALBuffer buffer)
		{
			uint buf = buffer.Handle;
			AL10.alSourceQueueBuffers(
				sourceHandle.Handle,
				1,
				ref buf
			);
#if VERBOSE_AL_DEBUGGING
			CheckALError();
#endif
		}

		public void DequeueSourceBuffers(
			ALSourceHandle sourceHandle,
			int buffersToDequeue,
			Queue<ALBuffer> errorCheck
		) {
			uint[] bufs = new uint[buffersToDequeue];
			AL10.alSourceUnqueueBuffers(
				sourceHandle.Handle,
				buffersToDequeue,
				bufs
			);
#if VERBOSE_AL_DEBUGGING
			CheckALError();
#endif
#if DEBUG
			// Error check our queuedBuffers list.
			ALBuffer[] sync = errorCheck.ToArray();
			for (int i = 0; i < buffersToDequeue; i += 1)
			{
				if (bufs[i] != sync[i].Handle)
				{
					throw new InvalidOperationException("Buffer desync!");
				}
			}
#endif
		}

		public int CheckProcessedBuffers(ALSourceHandle sourceHandle)
		{
			int result;
			AL10.alGetSourcei(
				sourceHandle.Handle,
				AL10.AL_BUFFERS_PROCESSED,
				out result
			);
#if VERBOSE_AL_DEBUGGING
			CheckALError();
#endif
			return result;
		}

		public void GetBufferData(
			ALSourceHandle sourceHandle,
			ALBuffer[] buffer,
			IntPtr samples,
			int samplesLen,
			AudioChannels channels
		) {
			int copySize1 = samplesLen / (int) channels;
			int copySize2 = 0;

			// Where are we now?
			int offset;
			AL10.alGetSourcei(
				sourceHandle.Handle,
				AL11.AL_SAMPLE_OFFSET,
				out offset
			);

			// Is that longer than what the active buffer has left...?
			uint buf = buffer[0].Handle;
			int len;
			AL10.alGetBufferi(
				buf,
				AL10.AL_SIZE,
				out len
			);
			len /= 2; // FIXME: Assuming 16-bit!
			len /= (int) channels;
			if (offset > len)
			{
				copySize2 = copySize1;
				copySize1 = 0;
				offset -= len;
			}
			else if (offset + copySize1 > len)
			{
				copySize2 = copySize1 - (len - offset);
				copySize1 = (len - offset);
			}

			// Copy!
			if (copySize1 > 0)
			{
				ALEXT.alGetBufferSamplesSOFT(
					buf,
					offset,
					copySize1,
					channels == AudioChannels.Stereo ?
						ALEXT.AL_STEREO_SOFT :
						ALEXT.AL_MONO_SOFT,
					ALEXT.AL_FLOAT_SOFT,
					samples
				);
				offset = 0;
			}
			if (buffer.Length > 1 && copySize2 > 0)
			{
				ALEXT.alGetBufferSamplesSOFT(
					buffer[1].Handle,
					0,
					copySize2,
					channels == AudioChannels.Stereo ?
						ALEXT.AL_STEREO_SOFT :
						ALEXT.AL_MONO_SOFT,
					ALEXT.AL_FLOAT_SOFT,
					samples + (copySize1 * (int) channels)
				);
			}
		}

		#endregion

		#region OpenAL Reverb Effect Methods

		public IALReverb GenReverb(DSPParameter[] parameters)
		{
			uint slot, effect;
			EFX.alGenAuxiliaryEffectSlots(1, out slot);
			EFX.alGenEffects(1, out effect);
			// Set up the Reverb Effect
			EFX.alEffecti(
				effect,
				EFX.AL_EFFECT_TYPE,
				EFX.AL_EFFECT_EAXREVERB
			);

			IALReverb result = new OpenALReverb(slot, effect);

			// Apply initial values
			SetReverbReflectionsDelay(result, parameters[0].Value);
			SetReverbDelay(result, parameters[1].Value);
			SetReverbPositionLeft(result, parameters[2].Value);
			SetReverbPositionRight(result, parameters[3].Value);
			SetReverbPositionLeftMatrix(result, parameters[4].Value);
			SetReverbPositionRightMatrix(result, parameters[5].Value);
			SetReverbEarlyDiffusion(result, parameters[6].Value);
			SetReverbLateDiffusion(result, parameters[7].Value);
			SetReverbLowEQGain(result, parameters[8].Value);
			SetReverbLowEQCutoff(result, parameters[9].Value);
			SetReverbHighEQGain(result, parameters[10].Value);
			SetReverbHighEQCutoff(result, parameters[11].Value);
			SetReverbRearDelay(result, parameters[12].Value);
			SetReverbRoomFilterFrequency(result, parameters[13].Value);
			SetReverbRoomFilterMain(result, parameters[14].Value);
			SetReverbRoomFilterHighFrequency(result, parameters[15].Value);
			SetReverbReflectionsGain(result, parameters[16].Value);
			SetReverbGain(result, parameters[17].Value);
			SetReverbDecayTime(result, parameters[18].Value);
			SetReverbDensity(result, parameters[19].Value);
			SetReverbRoomSize(result, parameters[20].Value);
			SetReverbWetDryMix(result, parameters[21].Value);

			// Bind the Effect to the EffectSlot. XACT will use the EffectSlot.
			EFX.alAuxiliaryEffectSloti(
				slot,
				EFX.AL_EFFECTSLOT_EFFECT,
				(int) effect
			);

#if VERBOSE_AL_DEBUGGING
			CheckALError();
#endif
			return result;
		}

		public void DeleteReverb(IALReverb reverb)
		{
			OpenALReverb rv = (reverb as OpenALReverb);
			uint slot = rv.SlotHandle;
			uint effect = rv.EffectHandle;
			EFX.alDeleteAuxiliaryEffectSlots(1, ref slot);
			EFX.alDeleteEffects(1, ref effect);
#if VERBOSE_AL_DEBUGGING
			CheckALError();
#endif
		}

		public void CommitReverbChanges(IALReverb reverb)
		{
			OpenALReverb rv = (reverb as OpenALReverb);
			EFX.alAuxiliaryEffectSloti(
				rv.SlotHandle,
				EFX.AL_EFFECTSLOT_EFFECT,
				(int) rv.EffectHandle
			);
#if VERBOSE_AL_DEBUGGING
			CheckALError();
#endif
		}

		public void SetReverbReflectionsDelay(IALReverb reverb, float value)
		{
			EFX.alEffectf(
				(reverb as OpenALReverb).EffectHandle,
				EFX.AL_EAXREVERB_REFLECTIONS_DELAY,
				value / 1000.0f
			);
#if VERBOSE_AL_DEBUGGING
			CheckALError();
#endif
		}

		public void SetReverbDelay(IALReverb reverb, float value)
		{
			EFX.alEffectf(
				(reverb as OpenALReverb).EffectHandle,
				EFX.AL_EAXREVERB_LATE_REVERB_DELAY,
				value / 1000.0f
			);
#if VERBOSE_AL_DEBUGGING
			CheckALError();
#endif
		}

		public void SetReverbPositionLeft(IALReverb reverb, float value)
		{
			// No known mapping :(
		}

		public void SetReverbPositionRight(IALReverb reverb, float value)
		{
			// No known mapping :(
		}

		public void SetReverbPositionLeftMatrix(IALReverb reverb, float value)
		{
			// No known mapping :(
		}

		public void SetReverbPositionRightMatrix(IALReverb reverb, float value)
		{
			// No known mapping :(
		}

		public void SetReverbEarlyDiffusion(IALReverb reverb, float value)
		{
			// Same as late diffusion, whatever... -flibit
			EFX.alEffectf(
				(reverb as OpenALReverb).EffectHandle,
				EFX.AL_EAXREVERB_DIFFUSION,
				value / 15.0f
			);
#if VERBOSE_AL_DEBUGGING
			CheckALError();
#endif
		}

		public void SetReverbLateDiffusion(IALReverb reverb, float value)
		{
			// Same as early diffusion, whatever... -flibit
			EFX.alEffectf(
				(reverb as OpenALReverb).EffectHandle,
				EFX.AL_EAXREVERB_DIFFUSION,
				value / 15.0f
			);
#if VERBOSE_AL_DEBUGGING
			CheckALError();
#endif
		}

		public void SetReverbLowEQGain(IALReverb reverb, float value)
		{
			// Cutting off volumes from 0db to 4db! -flibit
			EFX.alEffectf(
				(reverb as OpenALReverb).EffectHandle,
				EFX.AL_EAXREVERB_GAINLF,
				Math.Min(
					XACTCalculator.CalculateAmplitudeRatio(
						value - 8.0f
					),
					1.0f
				)
			);
#if VERBOSE_AL_DEBUGGING
			CheckALError();
#endif
		}

		public void SetReverbLowEQCutoff(IALReverb reverb, float value)
		{
			EFX.alEffectf(
				(reverb as OpenALReverb).EffectHandle,
				EFX.AL_EAXREVERB_LFREFERENCE,
				(value * 50.0f) + 50.0f
			);
#if VERBOSE_AL_DEBUGGING
			CheckALError();
#endif
		}

		public void SetReverbHighEQGain(IALReverb reverb, float value)
		{
			EFX.alEffectf(
				(reverb as OpenALReverb).EffectHandle,
				EFX.AL_EAXREVERB_GAINHF,
				XACTCalculator.CalculateReverbAmplitudeRatio(
					value - 8.0f
				)
			);
#if VERBOSE_AL_DEBUGGING
			CheckALError();
#endif
		}

		public void SetReverbHighEQCutoff(IALReverb reverb, float value)
		{
			EFX.alEffectf(
				(reverb as OpenALReverb).EffectHandle,
				EFX.AL_EAXREVERB_HFREFERENCE,
				(value * 500.0f) + 1000.0f
			);
#if VERBOSE_AL_DEBUGGING
			CheckALError();
#endif
		}

		public void SetReverbRearDelay(IALReverb reverb, float value)
		{
			// No known mapping :(
		}

		public void SetReverbRoomFilterFrequency(IALReverb reverb, float value)
		{
			// No known mapping :(
		}

		public void SetReverbRoomFilterMain(IALReverb reverb, float value)
		{
			// No known mapping :(
		}

		public void SetReverbRoomFilterHighFrequency(IALReverb reverb, float value)
		{
			// No known mapping :(
		}

		public void SetReverbReflectionsGain(IALReverb reverb, float value)
		{
			// Cutting off possible float values above 3.16, for EFX -flibit
			EFX.alEffectf(
				(reverb as OpenALReverb).EffectHandle,
				EFX.AL_EAXREVERB_REFLECTIONS_GAIN,
				Math.Min(
					XACTCalculator.CalculateReverbAmplitudeRatio(value),
					3.16f
				)
			);
#if VERBOSE_AL_DEBUGGING
			CheckALError();
#endif
		}

		public void SetReverbGain(IALReverb reverb, float value)
		{
			// Cutting off volumes from 0db to 20db! -flibit
			EFX.alEffectf(
				(reverb as OpenALReverb).EffectHandle,
				EFX.AL_EAXREVERB_GAIN,
				Math.Min(
					XACTCalculator.CalculateReverbAmplitudeRatio(value),
					1.0f
				)
			);
#if VERBOSE_AL_DEBUGGING
			CheckALError();
#endif
		}

		public void SetReverbDecayTime(IALReverb reverb, float value)
		{
			/* FIXME: WTF is with this XACT value?
			 * XACT: 0-30 equal to 0.1-inf seconds?!
			 * EFX: 0.1-20 seconds
			 * -flibit
			EFX.alEffectf(
				(reverb as OpenALReverb).EffectHandle,
				EFX.AL_EAXREVERB_GAIN,
				value
			);
			*/
		}

		public void SetReverbDensity(IALReverb reverb, float value)
		{
			EFX.alEffectf(
				(reverb as OpenALReverb).EffectHandle,
				EFX.AL_EAXREVERB_DENSITY,
				value / 100.0f
			);
#if VERBOSE_AL_DEBUGGING
			CheckALError();
#endif
		}

		public void SetReverbRoomSize(IALReverb reverb, float value)
		{
			// No known mapping :(
		}

		public void SetReverbWetDryMix(IALReverb reverb, float value)
		{
			/* FIXME: Note that were dividing by 200, not 100.
			 * For some ridiculous reason the mix is WAY too wet
			 * when we actually do the correct math, but cutting
			 * the ratio in half mysteriously makes it sound right.
			 *
			 * Or, well, "more" right. I'm sure we're still off.
			 * -flibit
			 */
			EFX.alAuxiliaryEffectSlotf(
				(reverb as OpenALReverb).SlotHandle,
				EFX.AL_EFFECTSLOT_GAIN,
				value / 200.0f
			);
		}

		#endregion

		#region OpenAL Capture Methods

		public IntPtr StartDeviceCapture(string name, int sampleRate, int bufSize)
		{
			IntPtr result = ALC11.alcCaptureOpenDevice(
				name,
				(uint) sampleRate,
				AL10.AL_FORMAT_MONO16,
				bufSize
			);
			ALC11.alcCaptureStart(result);
#if VERBOSE_AL_DEBUGGING
			if (CheckALCError())
			{
				throw new InvalidOperationException("AL device error!");
			}
#endif
			return result;
		}

		public void StopDeviceCapture(IntPtr handle)
		{
			ALC11.alcCaptureStop(handle);
			ALC11.alcCaptureCloseDevice(handle);
#if VERBOSE_AL_DEBUGGING
			if (CheckALCError())
			{
				throw new InvalidOperationException("AL device error!");
			}
#endif
		}

		public int CaptureSamples(IntPtr handle, IntPtr buffer, int count)
		{
			int[] samples = new int[1] { 0 };
			ALC10.alcGetIntegerv(
				handle,
				ALC11.ALC_CAPTURE_SAMPLES,
				1,
				samples
			);
			samples[0] = Math.Min(samples[0], count / 2);
			if (samples[0] > 0)
			{
				ALC11.alcCaptureSamples(handle, buffer, samples[0]);
			}
#if VERBOSE_AL_DEBUGGING
			if (CheckALCError())
			{
				throw new InvalidOperationException("AL device error!");
			}
#endif
			return samples[0] * 2;
		}

		public bool CaptureHasSamples(IntPtr handle)
		{
			int[] samples = new int[1] { 0 };
			ALC10.alcGetIntegerv(
				handle,
				ALC11.ALC_CAPTURE_SAMPLES,
				1,
				samples
			);
			return samples[0] > 0;
		}

		#endregion

		#region Private OpenAL Error Check Methods

		
		public static string ALEnumToString(int alEnum) {
			switch (alEnum) {
					//case AL_NONE: return "AL_NONE"; 
					//case AL_FALSE : return ""; 
				case AL10.AL_TRUE:
					return "AL_TRUE";
				case AL10.AL_SOURCE_RELATIVE:
					return "AL_SOURCE_RELATIVE";
				case AL10.AL_CONE_INNER_ANGLE:
					return "AL_CONE_INNER_ANGLE";
				case AL10.AL_CONE_OUTER_ANGLE:
					return "AL_CONE_OUTER_ANGLE";
				case AL10.AL_PITCH:
					return "AL_PITCH";
				case AL10.AL_POSITION:
					return "AL_POSITION";
				case AL10.AL_DIRECTION:
					return "AL_DIRECTION";
				case AL10.AL_VELOCITY:
					return "AL_VELOCITY";
				case AL10.AL_LOOPING:
					return "AL_LOOPING";
				case AL10.AL_BUFFER:
					return "AL_BUFFER";
				case AL10.AL_GAIN:
					return "AL_GAIN";
				case AL10.AL_MIN_GAIN:
					return "AL_MIN_GAIN";
				case AL10.AL_MAX_GAIN:
					return "AL_MAX_GAIN";
				case AL10.AL_ORIENTATION:
					return "AL_ORIENTATION";
				case AL10.AL_SOURCE_STATE:
					return "AL_SOURCE_STATE";
				case AL10.AL_INITIAL:
					return "AL_INITIAL";
				case AL10.AL_PLAYING:
					return "AL_PLAYING";
				case AL10.AL_PAUSED:
					return "AL_PAUSED";
				case AL10.AL_STOPPED:
					return "AL_STOPPED";
				case AL10.AL_BUFFERS_QUEUED:
					return "AL_BUFFERS_QUEUED";
				case AL10.AL_BUFFERS_PROCESSED:
					return "AL_BUFFERS_PROCESSED";
				case AL10.AL_REFERENCE_DISTANCE:
					return "AL_REFERENCE_DISTANCE";
				case AL10.AL_ROLLOFF_FACTOR:
					return "AL_ROLLOFF_FACTOR";
				case AL10.AL_CONE_OUTER_GAIN:
					return "AL_CONE_OUTER_GAIN";
				case AL10.AL_MAX_DISTANCE:
					return "AL_MAX_DISTANCE";
				case AL10.AL_SOURCE_TYPE:
					return "AL_SOURCE_TYPE";
				case AL10.AL_STATIC:
					return "AL_STATIC";
				case AL10.AL_STREAMING:
					return "AL_STREAMING";
				case AL10.AL_UNDETERMINED:
					return "AL_UNDETERMINED";
				case AL10.AL_FORMAT_MONO8:
					return "AL_FORMAT_MONO8";
				case AL10.AL_FORMAT_MONO16:
					return "AL_FORMAT_MONO16";
				case AL10.AL_FORMAT_STEREO8:
					return "AL_FORMAT_STEREO8";
				case AL10.AL_FORMAT_STEREO16:
					return "AL_FORMAT_STEREO16";
				case AL10.AL_FREQUENCY:
					return "AL_FREQUENCY";
				case AL10.AL_BITS:
					return "AL_BITS";
				case AL10.AL_CHANNELS:
					return "AL_CHANNELS";
				case AL10.AL_SIZE:
					return "AL_SIZE";
				case AL10.AL_NO_ERROR:
					return "AL_NO_ERROR";
				case AL10.AL_INVALID_NAME:
					return "AL_INVALID_NAME";
				case AL10.AL_INVALID_ENUM:
					return "AL_INVALID_ENUM";
				case AL10.AL_INVALID_VALUE:
					return "AL_INVALID_VALUE";
				case AL10.AL_INVALID_OPERATION:
					return "AL_INVALID_OPERATION";
				case AL10.AL_OUT_OF_MEMORY:
					return "AL_OUT_OF_MEMORY";
				case AL10.AL_VENDOR:
					return "AL_VENDOR";
				case AL10.AL_VERSION:
					return "AL_VERSION";
				case AL10.AL_RENDERER:
					return "AL_RENDERER";
				case AL10.AL_EXTENSIONS:
					return "AL_EXTENSIONS";
				case AL10.AL_DOPPLER_FACTOR:
					return "AL_DOPPLER_FACTOR";
				case AL10.AL_DOPPLER_VELOCITY:
					return "AL_DOPPLER_VELOCITY";
				case AL10.AL_DISTANCE_MODEL:
					return "AL_DISTANCE_MODEL";
				case AL10.AL_INVERSE_DISTANCE:
					return "AL_INVERSE_DISTANCE";
				case AL10.AL_INVERSE_DISTANCE_CLAMPED:
					return "AL_INVERSE_DISTANCE_CLAMPED";
				default:
					return "ALEnum: "+ alEnum.ToString("X4");
			}
		}

		private void CheckALError()
		{
			var errorText = CheckALErrorString();

			if (errorText == null)
			{
				return;
			}

			FNALoggerEXT.LogError("OpenAL Error: " + errorText);
#if VERBOSE_AL_DEBUGGING
			throw new InvalidOperationException("OpenAL Error! " + errorText);
#endif
		}


		private string CheckALErrorString() {
			int err = AL10.alGetError();

			if (err == AL10.AL_NO_ERROR) {
				return null;
			}

			return ALEnumToString(err);
		}

		public static string ALCEnumToString(int alcEnum) {
			switch (alcEnum) {
				//case ALC_FALSE:
				//    return "ALC_FALSE";
				case ALC10.ALC_TRUE:
					return "ALC_TRUE";
				case ALC10.ALC_FREQUENCY:
					return "ALC_FREQUENCY";
				case ALC10.ALC_REFRESH:
					return "ALC_REFRESH";
				case ALC10.ALC_SYNC:
					return "ALC_SYNC";
				case ALC10.ALC_NO_ERROR:
					return "ALC_NO_ERROR";
				case ALC10.ALC_INVALID_DEVICE:
					return "ALC_INVALID_DEVICE";
				case ALC10.ALC_INVALID_CONTEXT:
					return "ALC_INVALID_CONTEXT";
				case ALC10.ALC_INVALID_ENUM:
					return "ALC_INVALID_ENUM";
				case ALC10.ALC_INVALID_VALUE:
					return "ALC_INVALID_VALUE";
				case ALC10.ALC_OUT_OF_MEMORY:
					return "ALC_OUT_OF_MEMORY";
				case ALC10.ALC_ATTRIBUTES_SIZE:
					return "ALC_ATTRIBUTES_SIZE";
				case ALC10.ALC_ALL_ATTRIBUTES:
					return "ALC_ALL_ATTRIBUTES";
				case ALC10.ALC_DEFAULT_DEVICE_SPECIFIER:
					return "ALC_DEFAULT_DEVICE_SPECIFIER";
				case ALC10.ALC_DEVICE_SPECIFIER:
					return "ALC_DEVICE_SPECIFIER";
				case ALC10.ALC_EXTENSIONS:
					return "ALC_EXTENSIONS";
				default:
					return "ALCEnum: " + alcEnum.ToString("X4");
			}
		}
		private bool CheckALCError()
		{
			int err = ALC10.alcGetError(alDevice);

			if (err == ALC10.ALC_NO_ERROR)
			{
				return false;
			}

			FNALoggerEXT.LogError("OpenAL Device Error: " + ALCEnumToString(err));
			return true;
		}

		#endregion

		#region Private Static XNA->AL Dictionaries

		private static readonly int[] XNAToShort = new int[]
		{
			AL10.AL_NONE,			// NOPE
			AL10.AL_FORMAT_MONO16,		// AudioChannels.Mono
			AL10.AL_FORMAT_STEREO16,	// AudioChannels.Stereo
		};

		private static readonly int[] XNAToFloat = new int[]
		{
			AL10.AL_NONE,			// NOPE
			ALEXT.AL_FORMAT_MONO_FLOAT32,	// AudioChannels.Mono
			ALEXT.AL_FORMAT_STEREO_FLOAT32	// AudioChannels.Stereo
		};

		#endregion

		#region OpenAL Device Enumerators

		public ReadOnlyCollection<RendererDetail> GetDevices()
		{
			IntPtr deviceList = ALC10.alcGetString(IntPtr.Zero, ALC11.ALC_ALL_DEVICES_SPECIFIER);
			List<RendererDetail> renderers = new List<RendererDetail>();

			int i = 0;
			string curString = Marshal.PtrToStringAnsi(deviceList);
			while (!String.IsNullOrEmpty(curString))
			{
				renderers.Add(new RendererDetail(
					curString,
					i.ToString()
				));
				i += 1;
				deviceList += curString.Length + 1;
				curString = Marshal.PtrToStringAnsi(deviceList);
			}

			return new ReadOnlyCollection<RendererDetail>(renderers);
		}

		public ReadOnlyCollection<Microphone> GetCaptureDevices()
		{
			IntPtr deviceList = ALC10.alcGetString(IntPtr.Zero, ALC11.ALC_CAPTURE_DEVICE_SPECIFIER);
			List<Microphone> microphones = new List<Microphone>();

			string curString = Marshal.PtrToStringAnsi(deviceList);
			while (!String.IsNullOrEmpty(curString))
			{
				microphones.Add(new Microphone(curString));
				deviceList += curString.Length + 1;
				curString = Marshal.PtrToStringAnsi(deviceList);
			}

			return new ReadOnlyCollection<Microphone>(microphones);
		}

		#endregion
	}
}
