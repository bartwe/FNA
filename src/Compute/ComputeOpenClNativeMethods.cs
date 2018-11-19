using System;
using System.Runtime.InteropServices;
using OpenCL.Net;

namespace FNAExt.Compute {
    internal static class ComputeOpenClNativeMethods {
        [DllImport("opencl.dll")]
        internal static extern ErrorCode clEnqueueWriteImage(IntPtr commandQueue, IntPtr image, Bool blockingWrite, [MarshalAs(UnmanagedType.LPArray, SizeConst = 3), In] IntPtr[] origin, [MarshalAs(UnmanagedType.LPArray, SizeConst = 3), In] IntPtr[] region, IntPtr rowPitch, IntPtr slicePitch, IntPtr ptr, uint numEventsIntWaitList, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 8, ArraySubType = UnmanagedType.SysUInt), In] Event[] eventWaitList, [MarshalAs(UnmanagedType.Struct)] out Event e);

        [DllImport("opencl.dll")]
        internal static extern ErrorCode clEnqueueReadImage(IntPtr commandQueue, IntPtr image, Bool blockingRead, [MarshalAs(UnmanagedType.LPArray, SizeConst = 3), In] IntPtr[] origin, [MarshalAs(UnmanagedType.LPArray, SizeConst = 3), In] IntPtr[] region, IntPtr rowPitch, IntPtr slicePitch, IntPtr ptr, uint numEventsIntWaitList, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 8, ArraySubType = UnmanagedType.SysUInt), In] Event[] eventWaitList, [MarshalAs(UnmanagedType.Struct)] out Event e);

        [DllImport("opencl.dll")]
        internal static extern ErrorCode clSetKernelArg(IntPtr kernel, uint argIndex, IntPtr argSize, IntPtr argValue);

        [DllImport("opencl.dll")]
        internal static extern IntPtr clCreateFromGLBuffer(IntPtr contextHandle, MemFlags flags, IntPtr gluintbufobj, out ErrorCode errorCode);

        [DllImport("opencl.dll")]
        internal static extern IntPtr clCreateFromGLTexture2D(IntPtr contextHandle, MemFlags flags, IntPtr texture_target, IntPtr miplevel, IntPtr texture, out ErrorCode errorCode);

        [DllImport("opencl.dll")]
        internal static extern ErrorCode clEnqueueAcquireGLObjects(IntPtr commandQueue, IntPtr numMemObjects, IntPtr memObjects, uint numEventsIntWaitList, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 8, ArraySubType = UnmanagedType.SysUInt), In] Event[] eventWaitList, [MarshalAs(UnmanagedType.Struct)] out Event e);

        [DllImport("opencl.dll")]
        internal static extern ErrorCode clEnqueueReleaseGLObjects(IntPtr commandQueue, IntPtr numMemObjects, IntPtr memObjects, uint numEventsIntWaitList, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 8, ArraySubType = UnmanagedType.SysUInt), In] Event[] eventWaitList, [MarshalAs(UnmanagedType.Struct)] out Event e);

        [DllImport("opencl.dll")]
        internal static extern IntPtr clCreateBuffer(IntPtr context, MemFlags flags, IntPtr size, IntPtr hostPtr, [MarshalAs(UnmanagedType.I4)] out ErrorCode errcodeRet);

        [DllImport("opencl.dll")]
        internal static extern ErrorCode clEnqueueWriteBuffer(IntPtr commandQueue, IntPtr buffer, Bool blockingWrite, int offset, int count, IntPtr ptr, uint numEventsIntWaitList, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 8, ArraySubType = UnmanagedType.SysUInt), In] Event[] eventWaitList, [MarshalAs(UnmanagedType.Struct)] out Event e);

        [DllImport("opencl.dll")]
        internal static extern ErrorCode clEnqueueReadBuffer(IntPtr commandQueue, IntPtr buffer, Bool blockingRead, int offset, int count, IntPtr ptr, uint numEventsIntWaitList, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 8, ArraySubType = UnmanagedType.SysUInt), In] Event[] eventWaitList, [MarshalAs(UnmanagedType.Struct)] out Event e);

    }
}