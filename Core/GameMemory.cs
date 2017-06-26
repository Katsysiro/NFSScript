using System;

namespace NFSScript.Core
{
    /// <summary>
    /// Where the magic happens.
    /// </summary>
    public static class GameMemory
    {        
        /// <summary>
        /// Where the magic happens.
        /// </summary>
        public static VAMemory Memory;

        /// <summary>
        /// A generic memory.
        /// </summary>
        public static GMemory GenericMemory;

        /// <summary>
        /// Alias for <see cref="GenericMemory"/>
        /// </summary>
        public static GMemory Generic => GenericMemory;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static IntPtr WriteData(byte[] data)
        {
            var size = (uint)data.Length;

            var hAlloc = NativeMethods.VirtualAllocEx(Memory.ProcessHandle, IntPtr.Zero, size, AllocationType.Commit, MemoryProtection.ExecuteReadWrite);

            Memory.WriteByteArray(hAlloc, data);

            return hAlloc;
        }
    }
}
