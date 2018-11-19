using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework.Graphics;
using OpenCL.Net;

namespace FNAExt.Compute {
    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    delegate void GlFinish();

    public class ComputeDevice {
        Context _context;
        readonly Device _device;
        bool _disposed;

        static ComputeDevice _instance;
        internal readonly IntPtr _contextHandle;
        internal GlFinish _glFinish;

        public static void Cleanup() {
            if (_instance != null) {
                _instance.Dispose();
                _instance = null;
            }
        }

        public ComputeDevice(GraphicsDevice graphicsDevice) {
            ErrorCode errorCode;
            var platforms = Cl.GetPlatformIDs(out errorCode);
            var devicesList = new List<KeyValuePair<Platform, Device>>();

            if (errorCode != ErrorCode.Success)
                throw new Cl.Exception(errorCode, "GetPlatformIDs");

            foreach (var platform in platforms) {
                var platformName = Cl.GetPlatformInfo(platform, PlatformInfo.Name, out errorCode).ToString();
                if (errorCode != ErrorCode.Success)
                    throw new Cl.Exception(errorCode, "GetPlatformInfo");
                Console.WriteLine("Platform: " + platformName);
                var idx = 1;
                var devices = Cl.GetDeviceIDs(platform, DeviceType.Gpu, out errorCode);
                if (errorCode != ErrorCode.Success)
                    throw new Cl.Exception(errorCode, "GetDeviceIDs");
                foreach (var device in devices) {
                    var deviceInfo = Cl.GetDeviceInfo(device, DeviceInfo.Extensions, out errorCode);
                    if (errorCode != ErrorCode.Success)
                        throw new Cl.Exception(errorCode, "GetDeviceIDs");
                    var supportsSharing = deviceInfo.ToString().Contains("gl_sharing");
                    Console.WriteLine(" - Device: #" + idx + " " + device + " " + supportsSharing);
                    if (supportsSharing) {
                        idx++;
                        devicesList.Add(new KeyValuePair<Platform, Device>(platform, device));
                    }
                }
            }

            if (devicesList.Count <= 0)
                throw new Exception("No suitable opencl compute devices found.");
            var devicePlatform = devicesList[0].Key;
            var devicePlatformHandle = (IntPtr)typeof(Platform).GetField("_handle", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(devicePlatform);
            _device = devicesList[0].Value;

            var imageSupport = Cl.GetDeviceInfo(_device, DeviceInfo.ImageSupport, out errorCode);
            if (errorCode != ErrorCode.Success)
                throw new Cl.Exception(errorCode, "GetDeviceInfo");
            if (imageSupport.CastTo<Bool>() == Bool.False) {
                Console.WriteLine("No image support.");
                return;
            }
            const int CL_GL_CONTEXT_KHR = 0x2008;
            const int CL_WGL_HDC_KHR = 0x200B;
            var openGLDevice = graphicsDevice.GLDevice as OpenGLDevice;
            var properties = new ContextProperty[4];
            properties[0] = new ContextProperty(ContextProperties.Platform, devicePlatformHandle);
            properties[1] = new ContextProperty((ContextProperties)CL_GL_CONTEXT_KHR, openGLDevice.glContext);
            properties[2] = new ContextProperty((ContextProperties)CL_WGL_HDC_KHR, openGLDevice.glContextHDC);
            properties[3] = new ContextProperty();

            _context = Cl.CreateContext(properties, 1, new[] { _device }, ContextNotify, IntPtr.Zero, out errorCode);
            if (errorCode != ErrorCode.Success)
                throw new Cl.Exception(errorCode, "CreateContext");
            _contextHandle = (IntPtr)typeof(Context).GetField("_handle", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(_context);

            _glFinish = (GlFinish)openGLDevice.GetProcAddressEXT(
                "glFinish",
                typeof(GlFinish)
                );
        }

        ~ComputeDevice() {
            Dispose(false);
        }

        public void Dispose() {
            Dispose(true);
        }

        void Dispose(bool disposing) {
            if (_disposed)
                return;
            _disposed = true;
            Cl.ReleaseContext(_context);
            _context = Context.Zero;
        }

        void ContextNotify(string errInfo, byte[] data, IntPtr cb, IntPtr userData) {
            Console.WriteLine("OpenCL Notification: " + errInfo);
        }

        public ComputeKernel CreateKernel(string programSource, string kernelName) {
            if (string.IsNullOrEmpty(programSource))
                throw new ArgumentOutOfRangeException("programSource");
            if (string.IsNullOrEmpty(kernelName) || (kernelName.Length > 64))
                throw new ArgumentOutOfRangeException("kernelName");
            ErrorCode errorCode;
            var program = Cl.CreateProgramWithSource(_context, 1, new[] { programSource }, null, out errorCode);
            if (errorCode != ErrorCode.Success)
                throw new Cl.Exception(errorCode, "CreateProgramWithSource");
            errorCode = Cl.BuildProgram(program, 1, new[] { _device }, string.Empty, null, IntPtr.Zero);
            var errorText = "";
            if (errorCode != ErrorCode.Success) {
                ErrorCode infoErrorCode;
                var info = Cl.GetProgramBuildInfo(program, _device, ProgramBuildInfo.Log, out infoErrorCode);
                if (infoErrorCode == ErrorCode.Success)
                    errorText = info.ToString();
                info.Dispose();
            }
            if (errorCode != ErrorCode.Success)
                throw new Cl.Exception(errorCode, "BuildProgram: " + errorText);

            var buildStatus = Cl.GetProgramBuildInfo(program, _device, ProgramBuildInfo.Status, out errorCode);
            if (errorCode != ErrorCode.Success)
                throw new Cl.Exception(errorCode, "GetProgramBuildInfo");
            var buildStatusStatus = buildStatus.CastTo<BuildStatus>();
            buildStatus.Dispose();
            if (buildStatusStatus != BuildStatus.Success) {
                var info = Cl.GetProgramBuildInfo(program, _device, ProgramBuildInfo.Log, out errorCode);
                if (errorCode != ErrorCode.Success)
                    throw new Cl.Exception(errorCode, "GetProgramBuildInfo");
                var infoText = buildStatusStatus.ToString();
                info.Dispose();
                Console.WriteLine(infoText);
                info.Dispose();
                throw new Exception("Failed to build kernel. " + infoText);
            }
            var kernel = Cl.CreateKernel(program, kernelName, out errorCode);
            if (errorCode != ErrorCode.Success)
                throw new Cl.Exception(errorCode, "CreateKernel");

            return new ComputeKernel(kernel);
        }

        public ComputeBuffer<T> CreateBuffer<T>(int length, InputOutput inputOutput, T[] data = null) where T : struct {
            if (data != null) {
                if (data.Length < length)
                    throw new ArgumentOutOfRangeException("data");
            }
            else
                data = new T[length];
            var pinnedData = data.Pin();
            ErrorCode errorCode;
            var flags = MemFlags.CopyHostPtr;
            switch (inputOutput) {
                case InputOutput.ReceiveFromCompute:
                    flags |= MemFlags.WriteOnly;
                    break;
                case InputOutput.SendToCompute:
                    flags |= MemFlags.ReadOnly;
                    break;
                default:
                    throw new ArgumentOutOfRangeException("inputOutput");
            }
            var memHandle = ComputeOpenClNativeMethods.clCreateBuffer(_contextHandle, flags, (IntPtr)(length * TypeSize<T>.SizeInt), pinnedData, out errorCode);
            if (errorCode != ErrorCode.Success)
                throw new Cl.Exception(errorCode, "CreateBuffer");
            var mem = new Mem();
            var boxedMem = (object)mem;
            typeof(Mem).GetField("_handle", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(boxedMem, memHandle);
            mem = (Mem)boxedMem;

            return new ComputeBuffer<T>(mem, 1, inputOutput, length, 1, TypeSize<T>.SizeInt, data, pinnedData);
        }

        public ComputeBuffer<T> CreateBuffer2D<T>(int width, int height, InputOutput inputOutput, T[] data = null) where T : struct {
            var clImageFormat = new ImageFormat(ChannelOrder.RGBA, ChannelType.Unsigned_Int8);
            var elementSize = SizeOfElement(clImageFormat);
            var length = width * height * elementSize;
            if (data != null) {
                if (data.Length < length)
                    throw new ArgumentOutOfRangeException("data");
            }
            else
                data = new T[length];
            var pinnedData = data.Pin();
            ErrorCode errorCode;
            var flags = MemFlags.CopyHostPtr;
            switch (inputOutput) {
                case InputOutput.ReceiveFromCompute:
                    flags |= MemFlags.WriteOnly;
                    break;
                case InputOutput.SendToCompute:
                    flags |= MemFlags.ReadOnly;
                    break;
                default:
                    throw new ArgumentOutOfRangeException("inputOutput");
            }
            var image2D = Cl.CreateImage2D(_context, flags, clImageFormat, (IntPtr)width, (IntPtr)height, (IntPtr)0, pinnedData, out errorCode);
            if (errorCode != ErrorCode.Success)
                throw new Cl.Exception(errorCode, "CreateImage2D");
            return new ComputeBuffer<T>(image2D, 2, inputOutput, width, height, elementSize, data, pinnedData);
        }

        public ComputeBuffer<T> CreateBuffer2D<T>(GraphicsDevice graphicsDevice, Texture2D texture) where T : struct {
            ErrorCode errorCode;
            var flags = MemFlags.WriteOnly;

            var innerTexture = (texture.texture as OpenGLDevice.OpenGLTexture);
            var textureHandle = innerTexture.Handle;
            innerTexture.Filter = TextureFilter.Linear;
            var openGLDevice = graphicsDevice.GLDevice as OpenGLDevice;
            openGLDevice.glTexParameteri(innerTexture.Target, OpenGLDevice.GLenum.GL_TEXTURE_MAG_FILTER, (int)OpenGLDevice.GLenum.GL_LINEAR);
            openGLDevice.glTexParameteri(innerTexture.Target, OpenGLDevice.GLenum.GL_TEXTURE_MIN_FILTER, (int)OpenGLDevice.GLenum.GL_LINEAR);

            var memHandle = ComputeOpenClNativeMethods.clCreateFromGLTexture2D(_contextHandle, flags, (IntPtr)OpenGLDevice.GLenum.GL_TEXTURE_2D, (IntPtr)0, (IntPtr)textureHandle, out errorCode);
            if (errorCode != ErrorCode.Success)
                throw new Cl.Exception(errorCode, "clCreateFromGLTexture");
            var mem = new Mem();
            var boxedMem = (object)mem;
            typeof(Mem).GetField("_handle", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(boxedMem, memHandle);
            mem = (Mem)boxedMem;
            return new ComputeBuffer<T>(mem, 2, InputOutput.Texture, texture.Width, texture.Height, TypeSize<T>.SizeInt, null, new PinnedObject());
        }

        int SizeOfElement(ImageFormat clImageFormat) {
            var size = 1;
            switch (clImageFormat.ChannelOrder) {
                case ChannelOrder.R:
                case ChannelOrder.A:
                    break;
                case ChannelOrder.RG:
                case ChannelOrder.RA:
                    size *= 2;
                    break;
                case ChannelOrder.RGB:
                    size *= 3;
                    break;
                case ChannelOrder.RGBA:
                case ChannelOrder.BGRA:
                case ChannelOrder.ARGB:
                    size *= 4;
                    break;
//                case ChannelOrder.Intensity:
//                case ChannelOrder.Luminance:
                default:
                    throw new ArgumentOutOfRangeException();
            }
            switch (clImageFormat.ChannelType) {
                case ChannelType.Snorm_Int8:
                case ChannelType.Unorm_Int8:
                case ChannelType.Signed_Int8:
                case ChannelType.Unsigned_Int8:
                    break;
                case ChannelType.Snorm_Int16:
                case ChannelType.Unorm_Int16:
                case ChannelType.Unorm_Short565:
                case ChannelType.Unorm_Short555:
                case ChannelType.Signed_Int16:
                case ChannelType.Unsigned_Int16:
                case ChannelType.HalfFloat:
                    size *= 2;
                    break;
                case ChannelType.Unorm_Int101010:
                case ChannelType.Signed_Int32:
                case ChannelType.Unsigned_Int32:
                case ChannelType.Float:
                    size *= 4;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            return size;
        }

        public CommandQueue CreateCommandQueue() {
            ErrorCode errorCode;
            var queue = Cl.CreateCommandQueue(_context, _device, 0, out errorCode);
            if (errorCode != ErrorCode.Success)
                throw new Cl.Exception(errorCode, "CreateCommandQueue");
            return new CommandQueue(queue, this);
        }
    }
}
