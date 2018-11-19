using System;
using System.Reflection;
using OpenCL.Net;

namespace FNAExt.Compute {
    public class ComputeKernel : IDisposable {
        internal Kernel Kernel;
        bool _disposed;
        readonly IntPtr _kernelHandle;
        readonly IntPtr[] _kernalArgValue = new IntPtr[1];
        PinnedObject _pinnedKernalArgValue;


        internal ComputeKernel(Kernel kernel) {
            Kernel = kernel;
            _kernelHandle = (IntPtr)typeof(Kernel).GetField("_handle", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(kernel);
            _pinnedKernalArgValue = _kernalArgValue.Pin();
        }

        ~ComputeKernel() {
            if (!_disposed)
                Dispose(false);
        }

        public void Dispose() {
            Dispose(true);
        }

        void Dispose(bool disposing) {
            if (_disposed)
                return;
            _disposed = true;
            Cl.ReleaseKernel(Kernel);
            _pinnedKernalArgValue.Dispose();
        }

        public void SetArgument<T>(uint index, ComputeBuffer<T> buffer) where T : struct {
            _kernalArgValue[0] = buffer._image2DHandle;
            var errorCode = ComputeOpenClNativeMethods.clSetKernelArg(_kernelHandle, index, (IntPtr)IntPtr.Size, (IntPtr)_pinnedKernalArgValue);
            if (errorCode != ErrorCode.Success)
                throw new Cl.Exception(errorCode, "SetKernelArg");
        }
    }
}
