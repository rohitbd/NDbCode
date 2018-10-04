using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Text;

namespace NoDatabase
{
    [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
    internal struct StreamInfo
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 2048)]
        public string StreamName;
    }

    internal static class NativeFunctions
    {
        [DllImport("StorageCore.dll", CallingConvention = CallingConvention.Cdecl)]
        internal static extern int OpenOrCreateStorage([MarshalAs(UnmanagedType.LPWStr)] string storageName, out IntPtr storage);

        [DllImport("StorageCore.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern uint GetNumStreams(IntPtr storage, out IntPtr streamNames);

        internal static void GetStreams(IntPtr storage, out StreamInfo[] streamNames)
        {
            IntPtr streamNamesPtr = IntPtr.Zero;

            int sz = Marshal.SizeOf(typeof(StreamInfo));

            uint numStreams = GetNumStreams(storage, out streamNamesPtr);

            streamNames = new StreamInfo[numStreams];

            for(int i = 0; i < numStreams; i++)
            {
                IntPtr tmp = new IntPtr(streamNamesPtr.ToInt64() + (i * sz));

                streamNames[i] = (StreamInfo)Marshal.PtrToStructure(tmp, typeof(StreamInfo));
            }
        }

        [DllImport("StorageCore.dll", CallingConvention = CallingConvention.Cdecl)]
        internal static extern int OpenOrCreateStream(IntPtr storage, [MarshalAs(UnmanagedType.LPWStr)] string streamName, out IntPtr stream);

        [DllImport("StorageCore.dll", CallingConvention = CallingConvention.Cdecl)]
        internal static extern uint UpdateStream(IntPtr storage, IntPtr stream, byte[] bytes, uint numBytes);

        [DllImport("StorageCore.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern void ReadStream(IntPtr stream, out IntPtr bytes, out ulong numRead);

        internal static void ReadStream(IntPtr stream, out byte[] bytes, out ulong numRead)
        {
            IntPtr bytesPtr;

            ReadStream(stream, out bytesPtr, out numRead);

            bytes = new byte[numRead];

            Marshal.Copy(bytesPtr, bytes, 0, (int)numRead);
        }

        [DllImport("StorageCore.dll", CallingConvention = CallingConvention.Cdecl)]
        internal static extern void CloseStream(IntPtr stream);

        [DllImport("StorageCore.dll", CallingConvention = CallingConvention.Cdecl)]
        internal static extern void DeleteStream(IntPtr storage, [MarshalAs(UnmanagedType.LPWStr)] string streamName);

        [DllImport("StorageCore.dll", CallingConvention = CallingConvention.Cdecl)]
        internal static extern void CloseStorage(IntPtr storage);
    }
}
