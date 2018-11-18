using System;
using System.Reflection;
using Microsoft.Xna.Framework.Graphics;
using OpenCL.Net;

namespace FNAExt.Compute {
    public class ComputeBuffer : IDisposable {
        internal IMem _image2D;
        internal IntPtr _image2DHandle;
        readonly InputOutput _flags;
        bool _disposed;
        byte[] _data;
        internal PinnedObject _pinnedData;
        internal int _width;
        internal int _height;
        bool _active;
        readonly int _elementSize;

        internal ComputeBuffer(IMem image2D, InputOutput flags, int width, int height, int elementSize, byte[] data, PinnedObject pinnedData) {
            _image2D = image2D;
            _image2DHandle = (IntPtr)image2D.GetType().GetField("_handle", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(_image2D);
            _flags = flags;
            _width = width;
            _height = height;
            _elementSize = elementSize;
            _data = data;
            _pinnedData = pinnedData;
        }

        ~ComputeBuffer() {
            Dispose(false);
        }

        public void Dispose() {
            if (_active)
                throw new Exception();
            Dispose(true);
        }

        void Dispose(bool disposing) {
            if (_disposed)
                return;
            _disposed = true;
            Cl.ReleaseMemObject(_image2D);
            _image2D = null;
            _data = null;
            if (_flags != InputOutput.Texture) {
                _pinnedData.Dispose();
            }
        }

        internal void EnsureCanSend() {
            if (_flags != InputOutput.SendToCompute)
                throw new Exception();
            if (_active)
                throw new Exception();
        }

        internal void EnsureCanReceive() {
            if (_flags != InputOutput.ReceiveFromCompute)
                throw new Exception();
            if (_active)
                throw new Exception();
        }

        public void EnsureGLResource() {
            if (_flags != InputOutput.Texture)
                throw new Exception();
            if (_active)
                throw new Exception();
        }

        internal void MarkActive() {
            if (_active)
                throw new Exception();
            _active = true;
        }

        internal void MarkInactive() {
            if (!_active)
                throw new Exception();
            _active = false;
        }

        public void Read(byte[] buffer, int bufferOffset, int offset, int length) {
            if (_active)
                throw new Exception();
            if (_flags != InputOutput.ReceiveFromCompute)
                throw new Exception();
            Array.Copy(_data, offset, buffer, bufferOffset, length);
        }

        public void Write(byte[] buffer, int bufferOffset, int offset, int length) {
            if (_active)
                throw new Exception();
            if (_flags != InputOutput.SendToCompute)
                throw new Exception();
            Array.Copy(buffer, bufferOffset, _data, offset, length);
        }

        public unsafe void CopyToTexture(Texture2D texture) {
            if (_active)
                throw new Exception();
            if (_flags != InputOutput.ReceiveFromCompute)
                throw new Exception();
            if ((texture.Width != _width) || (texture.Height != _height))
                throw new Exception();
            fixed (byte* p = _data)
                texture.SetDataPointerEXT(0, null, (IntPtr)p, _width * _height * _elementSize);
        }
    }
}
