using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using OpenCL.Net;
using EventInfo = OpenCL.Net.EventInfo;

namespace FNAExt.Compute {
    public class CommandQueue : IDisposable {
        readonly OpenCL.Net.CommandQueue _queue;
        readonly IntPtr _queueHandle;
        readonly IntPtr[] _originPtr;
        readonly IntPtr[] _regionPtr;
        readonly IntPtr[] _workGroupSizePtr;
        readonly IntPtr[] _localWorkSizePtr;
        readonly IntPtr[] _pointerScratch;
        readonly Event[] _eventScratch;
        readonly Queue<Event> _events = new Queue<Event>();
        Event _mostRecentEvent;
        InfoBuffer _infoBuffer;
        bool _disposed;
        const int InfoBufferCapacity = 256;
        readonly Queue<IComputeBuffer> _acitveBuffers = new Queue<IComputeBuffer>();
        readonly ComputeDevice _device;

        internal CommandQueue(OpenCL.Net.CommandQueue queue, ComputeDevice device) {
            _device = device;
            _queue = queue;
            _queueHandle = (IntPtr)typeof(OpenCL.Net.CommandQueue).GetField("_handle", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(queue);
            _originPtr = new IntPtr[3];
            _regionPtr = new IntPtr[3];
            _workGroupSizePtr = new IntPtr[3];
            _localWorkSizePtr = new IntPtr[3];
            _pointerScratch = new IntPtr[1];
            _eventScratch = new Event[1];
            _infoBuffer = new InfoBuffer((IntPtr)InfoBufferCapacity);

            _localWorkSizePtr[0] = (IntPtr)8;
            _localWorkSizePtr[1] = (IntPtr)8;
            _localWorkSizePtr[2] = (IntPtr)8;
        }

        public void EnqueueSendToCompute<T>(ComputeBuffer<T> buffer) where T : struct {
            buffer.EnsureCanSend();
            if (buffer._dimensions == 1) {
                Event clevent;
                _eventScratch[0] = _mostRecentEvent;
                var hasPreviousEvent = _events.Count > 0;
                var errorCode = ComputeOpenClNativeMethods.clEnqueueWriteBuffer(_queueHandle, buffer._image2DHandle, Bool.False, 0, buffer._width, buffer._pinnedData, hasPreviousEvent ? 1u : 0, hasPreviousEvent ? _eventScratch : null, out clevent);
                if (errorCode != ErrorCode.Success)
                    throw new Cl.Exception(errorCode, "EnqueueWriteImage");
                _events.Enqueue(clevent);
                _mostRecentEvent = clevent;
            }
            else if (buffer._dimensions == 2) {
                _originPtr[0] = IntPtr.Zero;
                _originPtr[1] = IntPtr.Zero;
                _originPtr[2] = IntPtr.Zero;
                _regionPtr[0] = (IntPtr)buffer._width;
                _regionPtr[1] = (IntPtr)buffer._height;
                _regionPtr[2] = (IntPtr)1;
                Event clevent;
                _eventScratch[0] = _mostRecentEvent;
                var hasPreviousEvent = _events.Count > 0;
                var errorCode = ComputeOpenClNativeMethods.clEnqueueWriteImage(_queueHandle, buffer._image2DHandle, Bool.False, _originPtr, _regionPtr, (IntPtr)0, (IntPtr)0, buffer._pinnedData, hasPreviousEvent ? 1u : 0, hasPreviousEvent ? _eventScratch : null, out clevent);
                if (errorCode != ErrorCode.Success)
                    throw new Cl.Exception(errorCode, "EnqueueWriteImage");
                _events.Enqueue(clevent);
                _mostRecentEvent = clevent;
            }
            else
                throw new Exception();
            buffer.MarkActive();
            _acitveBuffers.Enqueue(buffer);
        }

        public void EnqueueReceiveFromCompute<T>(ComputeBuffer<T> buffer) where T : struct {
            buffer.EnsureCanReceive();
            _originPtr[0] = IntPtr.Zero;
            _originPtr[1] = IntPtr.Zero;
            _originPtr[2] = IntPtr.Zero;
            _regionPtr[0] = (IntPtr)buffer._width;
            _regionPtr[1] = (IntPtr)buffer._height;
            _regionPtr[2] = (IntPtr)1;
            Event clevent;
            _eventScratch[0] = _mostRecentEvent;
            var hasPreviousEvent = _events.Count > 0;
            var errorCode = ComputeOpenClNativeMethods.clEnqueueReadImage(_queueHandle, buffer._image2DHandle, Bool.False, _originPtr, _regionPtr, (IntPtr)0, (IntPtr)0, buffer._pinnedData, hasPreviousEvent ? 1u : 0, hasPreviousEvent ? _eventScratch : null, out clevent);
            if (errorCode != ErrorCode.Success)
                throw new Cl.Exception(errorCode, "EnqueueWriteImage");
            buffer.MarkActive();
            _acitveBuffers.Enqueue(buffer);
            _events.Enqueue(clevent);
            _mostRecentEvent = clevent;
        }

        public unsafe void EnqueueAcquireGlResource<T>(ComputeBuffer<T> buffer) where T : struct {
            buffer.EnsureGLResource();
            Event clevent;
            _pointerScratch[0] = buffer._image2DHandle;
            _eventScratch[0] = _mostRecentEvent;
            var hasPreviousEvent = _events.Count > 0;
            fixed (IntPtr* p = _pointerScratch) {
                var errorCode = ComputeOpenClNativeMethods.clEnqueueAcquireGLObjects(_queueHandle, (IntPtr)1, (IntPtr)p, hasPreviousEvent ? 1u : 0, hasPreviousEvent ? _eventScratch : null, out clevent);
                if (errorCode != ErrorCode.Success)
                    throw new Cl.Exception(errorCode, "clEnqueueAcquireGLObjects");
            }
            buffer.MarkActive();
            _acitveBuffers.Enqueue(buffer);
            _events.Enqueue(clevent);
            _mostRecentEvent = clevent;
        }

        public unsafe void EnqueueReleaseGlResource<T>(ComputeBuffer<T> buffer) where T : struct {
            Event clevent;
            _pointerScratch[0] = buffer._image2DHandle;
            _eventScratch[0] = _mostRecentEvent;
            var hasPreviousEvent = _events.Count > 0;
            fixed (IntPtr* p = _pointerScratch) {
                var errorCode = ComputeOpenClNativeMethods.clEnqueueReleaseGLObjects(_queueHandle, (IntPtr)1, (IntPtr)p, hasPreviousEvent ? 1u : 0, hasPreviousEvent ? _eventScratch : null, out clevent);
                if (errorCode != ErrorCode.Success)
                    throw new Cl.Exception(errorCode, "clEnqueueAcquireGLObjects");
            }
            _events.Enqueue(clevent);
            _mostRecentEvent = clevent;
        }

        public void EnqueueExecuteKernel(ComputeKernel kernel, WorkGroupSize size) {
            _workGroupSizePtr[0] = (IntPtr)size.Width;
            _workGroupSizePtr[1] = (IntPtr)size.Height;
            _workGroupSizePtr[2] = (IntPtr)size.Depth;
            Event clevent;
            _eventScratch[0] = _mostRecentEvent;
            var hasPreviousEvent = _events.Count > 0;
            //_localWorkSizePtr
            var errorCode = Cl.EnqueueNDRangeKernel(_queue, kernel.Kernel, 2, null, _workGroupSizePtr, null, hasPreviousEvent ? 1u : 0, hasPreviousEvent ? _eventScratch : null, out clevent);
            if (errorCode != ErrorCode.Success)
                throw new Cl.Exception(errorCode, "EnqueueNDRangeKernel");
            _events.Enqueue(clevent);
            _mostRecentEvent = clevent;
        }

        // call to ensure work is started
        public void Flush() {
            var errorCode = Cl.Flush(_queue);
            if (errorCode != ErrorCode.Success)
                throw new Cl.Exception(errorCode, "Flush");
        }

        public void WaitForFinish() {
            var errorCode = Cl.Finish(_queue);
            if (errorCode != ErrorCode.Success)
                throw new Cl.Exception(errorCode, "Finish");
            while (_events.Count > 0)
                _events.Dequeue().Dispose();
            _mostRecentEvent = new Event();
            while (_acitveBuffers.Count > 0)
                _acitveBuffers.Dequeue().MarkInactive();
        }

        // periodic poll option to see if calling WaitForFinish needs to happen
        public bool Ready() {
            if (_events.Count == 0)
                return true;
            _mostRecentEvent = new Event();
            while (true) {
                if (_events.Count == 0)
                    return true;
                var paramValueSize = IntPtr.Zero;
                var errorCode = Cl.GetEventInfo(_events.Peek(), EventInfo.CommandExecutionStatus, (IntPtr)InfoBufferCapacity, _infoBuffer, out paramValueSize);
                if (paramValueSize.ToInt32() > InfoBufferCapacity)
                    throw new Exception("Info buffer too small.");
                if (errorCode != ErrorCode.Success)
                    throw new Cl.Exception(errorCode, "Finish");
                switch (_infoBuffer.CastTo<ExecutionStatus>()) {
                    case ExecutionStatus.Complete:
                        _events.Dequeue();
                        break;
                    case ExecutionStatus.Error:
                        return true;
                    case ExecutionStatus.Running:
                    case ExecutionStatus.Submitted:
                    case ExecutionStatus.Queued:
                        return false;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        ~CommandQueue() {
            if (!_disposed)
                if (Debugger.IsAttached)
                    throw new Exception("CommandQueue was leaked instead of properly disposed.");
            Dispose(false);
        }

        public void Dispose() {
            Dispose(true);
        }

        void Dispose(bool disposing) {
            if (_disposed)
                return;
            _disposed = true;
            Cl.ReleaseCommandQueue(_queue);
            _infoBuffer.Dispose();
        }

        public void SwitchClToGlPhase() {
            WaitForFinish();
        }

        public void SwitchGlToClPhase() {
            _device._glFinish();
        }
    }
}
