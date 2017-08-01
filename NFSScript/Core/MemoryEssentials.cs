using System;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

namespace NFSScript.Core
{
    /// <summary>
    /// 
    /// </summary>
    public class MemoryAllocMap : IDisposable
    {
        /// <summary>
        /// 
        /// </summary>
        public IntPtr StoredAddress { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public uint CalledAddress { get; private set; }

        /// <summary>
        /// Returns the amount of usages.
        /// </summary>
        public int Usages { get; private set; }

        /// <summary>
        /// Whether this map has been disposed or not.
        /// </summary>
        public bool Disposed;

        private readonly SafeHandle _handle = new SafeFileHandle(IntPtr.Zero, true);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="storedAddress"></param>
        /// <param name="calledAddress"></param>
        public MemoryAllocMap(IntPtr storedAddress, uint calledAddress)
        {
            StoredAddress = storedAddress;
            CalledAddress = calledAddress;
            Usages = 0;
        }

        // TODO: This method is never used.
        private void Use()
        {
            Usages++;
        }
       
        /// <summary>
        /// 
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            if (Disposed)
                return;

            if (disposing)
            {
                _handle.Dispose();
            }

            Disposed = true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="calledAddressValue"></param>
        /// <returns></returns>
        public static bool ExistsInMemoryReturnAlloc(uint calledAddressValue)
        {
            return ASM.memoryReturnAllocation.Find(x => x.CalledAddress == calledAddressValue) != null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="calledAddressValue"></param>
        /// <returns></returns>
        public static MemoryAllocMap GetMemoryAllocMapByCalledAddress(uint calledAddressValue)
        {
            return ASM.memoryReturnAllocation.Find(x => x.CalledAddress == calledAddressValue);
        }
    }

}
