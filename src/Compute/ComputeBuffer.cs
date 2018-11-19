using System;
using System.Reflection;
using Microsoft.Xna.Framework.Graphics;
using OpenCL.Net;

namespace FNAExt.Compute {
    internal interface IComputeBuffer {
        void MarkInactive();
    }
    public class ComputeBuffer<T> : IDisposable, IComputeBuffer where T : struct {
        internal IMem _mem;
        internal IntPtr _image2DHandle;
        readonly InputOutput _flags;
        bool _disposed;
        T[] _data;
        internal PinnedObject _pinnedData;
        internal int _width;
        internal int _height;
        bool _active;
        readonly int _elementSize;
        public int _dimensions;

        internal ComputeBuffer(IMem mem, int dimensions, InputOutput flags, int width, int height, int elementSize, T[] data, PinnedObject pinnedData) {
            _mem = mem;
            _dimensions = dimensions;
            _image2DHandle = (IntPtr)mem.GetType().GetField("_handle", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(_mem);
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
            Cl.ReleaseMemObject(_mem);
            _mem = null;
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

        void IComputeBuffer.MarkInactive() {
            if (!_active)
                throw new Exception();
            _active = false;
        }

        public void Read(T[] buffer, int bufferOffset, int offset, int length) {
            if (_active)
                throw new Exception();
            if (_flags != InputOutput.ReceiveFromCompute)
                throw new Exception();
            Array.Copy(_data, offset, buffer, bufferOffset, length);
        }

        public void Write(T[] buffer, int bufferOffset, int offset, int length) {
            if (_active)
                throw new Exception();
            if (_flags != InputOutput.SendToCompute)
                throw new Exception();
            Array.Copy(buffer, bufferOffset, _data, offset, length);
        }

        public void CopyToTexture(Texture2D texture) {
            if (_active)
                throw new Exception();
            if (_flags != InputOutput.ReceiveFromCompute)
                throw new Exception();
            if ((texture.Width != _width) || (texture.Height != _height))
                throw new Exception();
            texture.SetDataPointerEXT(0, null, _pinnedData, _width * _height * _elementSize);
        }
    }
}
