// [----------------------------------------------------------------------------------------------------------------------------------------------]
// Thank you Vivid Abstractions for this wonderful class!
// I'll probably replace this class with my own memory editor class when I have enough time.
// [----------------------------------------------------------------------------------------------------------------------------------------------]

using System;
using System.Diagnostics;
using System.Text;
using static NFSScript.Core.NativeMethods;
// ReSharper disable All
// Not dealing with this class. Please don't Dennis me.

namespace NFSScript.Core
{
    /// <summary>
    /// Where the magic happens.
    /// </summary>
    public class VAMemory
    {
        /// <summary>
        /// 
        /// </summary>
        public static bool debugMode;
        private IntPtr baseAddress;
        private ProcessModule processModule;
        private Process[] mainProcess;
        private IntPtr processHandle;

        /// <summary>
        /// 
        /// </summary>
        public string processName { get; set; }

        /// <summary>
        /// Get the process handle.
        /// </summary>
        public IntPtr ProcessHandle => processHandle;

        /// <summary>
        /// 
        /// </summary>
        public long getBaseAddress
        {
            get
            {
                baseAddress = (IntPtr)0;
                processModule = mainProcess[0].MainModule;
                baseAddress = processModule.BaseAddress;
                return (long)baseAddress;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public VAMemory()
        {
        }

        /// <summary>
        /// 
        /// </summary>
        public VAMemory(string pProcessName)
        {
            processName = pProcessName;
        }

        /// <summary>
        /// 
        /// </summary>
        public bool CheckProcess()
        {
            if (processName != null)
            {
                mainProcess = Process.GetProcessesByName(processName);
                if (mainProcess.Length == 0)
                {
                    ErrorProcessNotFound(processName);
                    return false;
                }
                processHandle = OpenProcess(2035711U, false, mainProcess[0].Id);
                if (!(processHandle == IntPtr.Zero))
                    return true;
                ErrorProcessNotFound(processName);
                return false;
            }
            //int num = (int)MessageBox.Show("Programmer, define process name first!");
            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        public byte[] ReadByteArray(IntPtr pOffset, uint pSize)
        {
            if (processHandle == IntPtr.Zero)
                CheckProcess();
            try
            {
                uint lpflOldProtect;
                VirtualProtectEx(processHandle, pOffset, (UIntPtr)pSize, 4U, out lpflOldProtect);
                var lpBuffer = new byte[(int)pSize];
                ReadProcessMemory(processHandle, pOffset, lpBuffer, pSize, 0U);
                VirtualProtectEx(processHandle, pOffset, (UIntPtr)pSize, lpflOldProtect, out lpflOldProtect);
                return lpBuffer;
            }
            catch (Exception ex)
            {
                if (debugMode)
                    Console.WriteLine("Error: ReadByteArray" + ex);
                return new byte[1];
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public string ReadStringUnicode(IntPtr pOffset, uint pSize)
        {
            if (processHandle == IntPtr.Zero)
                CheckProcess();
            try
            {
                return Encoding.Unicode.GetString(ReadByteArray(pOffset, pSize), 0, (int)pSize);
            }
            catch (Exception ex)
            {
                if (debugMode)
                    Console.WriteLine("Error: ReadStringUnicode" + ex);
                return "";
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public string ReadStringASCII(IntPtr pOffset, uint pSize)
        {
            if (processHandle == IntPtr.Zero)
                CheckProcess();
            try
            {
                return Encoding.ASCII.GetString(ReadByteArray(pOffset, pSize), 0, (int)pSize);
            }
            catch (Exception ex)
            {
                if (debugMode)
                    Console.WriteLine("Error: ReadStringASCII" + ex);
                return "";
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public char ReadChar(IntPtr pOffset)
        {
            if (processHandle == IntPtr.Zero)
                CheckProcess();
            try
            {
                return BitConverter.ToChar(ReadByteArray(pOffset, 1U), 0);
            }
            catch (Exception ex)
            {
                if (debugMode)
                    Console.WriteLine("Error: ReadChar" + ex);
                return ' ';
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public bool ReadBoolean(IntPtr pOffset)
        {
            if (processHandle == IntPtr.Zero)
                CheckProcess();
            try
            {
                return BitConverter.ToBoolean(ReadByteArray(pOffset, 1U), 0);
            }
            catch (Exception ex)
            {
                if (debugMode)
                    Console.WriteLine("Error: ReadByte" + ex);
                return false;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public byte ReadByte(IntPtr pOffset)
        {
            if (processHandle == IntPtr.Zero)
                CheckProcess();
            try
            {
                return ReadByteArray(pOffset, 1U)[0];
            }
            catch (Exception ex)
            {
                if (debugMode)
                    Console.WriteLine("Error: ReadByte" + ex);
                return 0;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public short ReadInt16(IntPtr pOffset)
        {
            if (processHandle == IntPtr.Zero)
                CheckProcess();
            try
            {
                return BitConverter.ToInt16(ReadByteArray(pOffset, 2U), 0);
            }
            catch (Exception ex)
            {
                if (debugMode)
                    Console.WriteLine("Error: ReadInt16" + ex);
                return 0;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public short ReadShort(IntPtr pOffset)
        {
            if (processHandle == IntPtr.Zero)
                CheckProcess();
            try
            {
                return BitConverter.ToInt16(ReadByteArray(pOffset, 2U), 0);
            }
            catch (Exception ex)
            {
                if (debugMode)
                    Console.WriteLine("Error: ReadInt16" + ex);
                return 0;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public int ReadInt32(IntPtr pOffset)
        {
            if (processHandle == IntPtr.Zero)
                CheckProcess();
            try
            {
                return BitConverter.ToInt32(ReadByteArray(pOffset, 4U), 0);
            }
            catch (Exception ex)
            {
                if (debugMode)
                    Console.WriteLine("Error: ReadInt32" + ex);
                return 0;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public int ReadInteger(IntPtr pOffset)
        {
            if (processHandle == IntPtr.Zero)
                CheckProcess();
            try
            {
                return BitConverter.ToInt32(ReadByteArray(pOffset, 4U), 0);
            }
            catch (Exception ex)
            {
                if (debugMode)
                    Console.WriteLine("Error: ReadInteger" + ex);
                return 0;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public long ReadInt64(IntPtr pOffset)
        {
            if (processHandle == IntPtr.Zero)
                CheckProcess();
            try
            {
                return BitConverter.ToInt64(ReadByteArray(pOffset, 8U), 0);
            }
            catch (Exception ex)
            {
                if (debugMode)
                    Console.WriteLine("Error: ReadInt64" + ex);
                return 0;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public long ReadLong(IntPtr pOffset)
        {
            if (processHandle == IntPtr.Zero)
                CheckProcess();
            try
            {
                return BitConverter.ToInt64(ReadByteArray(pOffset, 8U), 0);
            }
            catch (Exception ex)
            {
                if (debugMode)
                    Console.WriteLine("Error: ReadLong" + ex);
                return 0;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public ushort ReadUInt16(IntPtr pOffset)
        {
            if (processHandle == IntPtr.Zero)
                CheckProcess();
            try
            {
                return BitConverter.ToUInt16(ReadByteArray(pOffset, 2U), 0);
            }
            catch (Exception ex)
            {
                if (debugMode)
                    Console.WriteLine("Error: ReadUInt16" + ex);
                return 0;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public ushort ReadUShort(IntPtr pOffset)
        {
            if (processHandle == IntPtr.Zero)
                CheckProcess();
            try
            {
                return BitConverter.ToUInt16(ReadByteArray(pOffset, 2U), 0);
            }
            catch (Exception ex)
            {
                if (debugMode)
                    Console.WriteLine("Error: ReadUShort" + ex);
                return 0;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public uint ReadUInt32(IntPtr pOffset)
        {
            if (processHandle == IntPtr.Zero)
                CheckProcess();
            try
            {
                return BitConverter.ToUInt32(ReadByteArray(pOffset, 4U), 0);
            }
            catch (Exception ex)
            {
                if (debugMode)
                    Console.WriteLine("Error: ReadUInt32" + ex);
                return 0;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public uint ReadUInteger(IntPtr pOffset)
        {
            if (processHandle == IntPtr.Zero)
                CheckProcess();
            try
            {
                return BitConverter.ToUInt32(ReadByteArray(pOffset, 4U), 0);
            }
            catch (Exception ex)
            {
                if (debugMode)
                    Console.WriteLine("Error: ReadUInteger" + ex);
                return 0;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public ulong ReadUInt64(IntPtr pOffset)
        {
            if (processHandle == IntPtr.Zero)
                CheckProcess();
            try
            {
                return BitConverter.ToUInt64(ReadByteArray(pOffset, 8U), 0);
            }
            catch (Exception ex)
            {
                if (debugMode)
                    Console.WriteLine("Error: ReadUInt64" + ex);
                return 0;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public long ReadULong(IntPtr pOffset)
        {
            if (processHandle == IntPtr.Zero)
                CheckProcess();
            try
            {
                return (long)BitConverter.ToUInt64(ReadByteArray(pOffset, 8U), 0);
            }
            catch (Exception ex)
            {
                if (debugMode)
                    Console.WriteLine("Error: ReadULong" + ex);
                return 0;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public float ReadFloat(IntPtr pOffset)
        {
            if (processHandle == IntPtr.Zero)
                CheckProcess();
            try
            {
                return BitConverter.ToSingle(ReadByteArray(pOffset, 4U), 0);
            }
            catch (Exception ex)
            {
                if (debugMode)
                    Console.WriteLine("Error: ReadFloat" + ex);
                return 0.0f;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public double ReadDouble(IntPtr pOffset)
        {
            if (processHandle == IntPtr.Zero)
                CheckProcess();
            try
            {
                return BitConverter.ToDouble(ReadByteArray(pOffset, 8U), 0);
            }
            catch (Exception ex)
            {
                if (debugMode)
                    Console.WriteLine("Error: ReadDouble" + ex);
                return 0.0;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public bool WriteByteArray(IntPtr pOffset, byte[] pBytes)
        {
            if (processHandle == IntPtr.Zero)
                CheckProcess();
            try
            {
                uint lpflOldProtect;
                VirtualProtectEx(processHandle, pOffset, (UIntPtr)((ulong)pBytes.Length), 4U, out lpflOldProtect);
                var flag = WriteProcessMemory(processHandle, pOffset, pBytes, (uint)pBytes.Length, 0U);
                VirtualProtectEx(processHandle, pOffset, (UIntPtr)((ulong)pBytes.Length), lpflOldProtect, out lpflOldProtect);
                return flag;
            }
            catch (Exception ex)
            {
                if (debugMode)
                    Console.WriteLine("Error: WriteByteArray" + ex);
                return false;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public bool WriteStringUnicode(IntPtr pOffset, string pData)
        {
            if (processHandle == IntPtr.Zero)
                CheckProcess();
            try
            {
                return WriteByteArray(pOffset, Encoding.Unicode.GetBytes(pData));
            }
            catch (Exception ex)
            {
                if (debugMode)
                    Console.WriteLine("Error: WriteStringUnicode" + ex);
                return false;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public bool WriteStringASCII(IntPtr pOffset, string pData)
        {
            if (processHandle == IntPtr.Zero)
                CheckProcess();
            try
            {
                return WriteByteArray(pOffset, Encoding.ASCII.GetBytes(pData));
            }
            catch (Exception ex)
            {
                if (debugMode)
                    Console.WriteLine("Error: WriteStringASCII" + ex);
                return false;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public bool WriteBoolean(IntPtr pOffset, bool pData)
        {
            if (processHandle == IntPtr.Zero)
                CheckProcess();
            try
            {
                return WriteByteArray(pOffset, BitConverter.GetBytes(pData));
            }
            catch (Exception ex)
            {
                if (debugMode)
                    Console.WriteLine("Error: WriteBoolean" + ex);
                return false;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public bool WriteChar(IntPtr pOffset, char pData)
        {
            if (processHandle == IntPtr.Zero)
                CheckProcess();
            try
            {
                return WriteByteArray(pOffset, BitConverter.GetBytes(pData));
            }
            catch (Exception ex)
            {
                if (debugMode)
                    Console.WriteLine("Error: WriteChar" + ex);
                return false;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public bool WriteByte(IntPtr pOffset, byte pData)
        {
            if (processHandle == IntPtr.Zero)
                CheckProcess();
            try
            {
                return WriteByteArray(pOffset, BitConverter.GetBytes(pData));
            }
            catch (Exception ex)
            {
                if (debugMode)
                    Console.WriteLine("Error: WriteByte" + ex);
                return false;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public bool WriteInt16(IntPtr pOffset, short pData)
        {
            if (processHandle == IntPtr.Zero)
                CheckProcess();
            try
            {
                return WriteByteArray(pOffset, BitConverter.GetBytes(pData));
            }
            catch (Exception ex)
            {
                if (debugMode)
                    Console.WriteLine("Error: WriteInt16" + ex);
                return false;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public bool WriteShort(IntPtr pOffset, short pData)
        {
            if (processHandle == IntPtr.Zero)
                CheckProcess();
            try
            {
                return WriteByteArray(pOffset, BitConverter.GetBytes(pData));
            }
            catch (Exception ex)
            {
                if (debugMode)
                    Console.WriteLine("Error: WriteShort" + ex);
                return false;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public bool WriteInt32(IntPtr pOffset, int pData)
        {
            if (processHandle == IntPtr.Zero)
                CheckProcess();
            try
            {
                return WriteByteArray(pOffset, BitConverter.GetBytes(pData));
            }
            catch (Exception ex)
            {
                if (debugMode)
                    Console.WriteLine("Error: WriteInt32" + ex);
                return false;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public bool WriteInteger(IntPtr pOffset, int pData)
        {
            if (processHandle == IntPtr.Zero)
                CheckProcess();
            try
            {
                return WriteByteArray(pOffset, BitConverter.GetBytes(pData));
            }
            catch (Exception ex)
            {
                if (debugMode)
                    Console.WriteLine("Error: WriteInt" + ex);
                return false;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public bool WriteInt64(IntPtr pOffset, long pData)
        {
            if (processHandle == IntPtr.Zero)
                CheckProcess();
            try
            {
                return WriteByteArray(pOffset, BitConverter.GetBytes(pData));
            }
            catch (Exception ex)
            {
                if (debugMode)
                    Console.WriteLine("Error: WriteInt64" + ex);
                return false;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public bool WriteLong(IntPtr pOffset, long pData)
        {
            if (processHandle == IntPtr.Zero)
                CheckProcess();
            try
            {
                return WriteByteArray(pOffset, BitConverter.GetBytes(pData));
            }
            catch (Exception ex)
            {
                if (debugMode)
                    Console.WriteLine("Error: WriteLong" + ex);
                return false;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public bool WriteUInt16(IntPtr pOffset, ushort pData)
        {
            if (processHandle == IntPtr.Zero)
                CheckProcess();
            try
            {
                return WriteByteArray(pOffset, BitConverter.GetBytes(pData));
            }
            catch (Exception ex)
            {
                if (debugMode)
                    Console.WriteLine("Error: WriteUInt16" + ex);
                return false;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public bool WriteUShort(IntPtr pOffset, ushort pData)
        {
            if (processHandle == IntPtr.Zero)
                CheckProcess();
            try
            {
                return WriteByteArray(pOffset, BitConverter.GetBytes(pData));
            }
            catch (Exception ex)
            {
                if (debugMode)
                    Console.WriteLine("Error: WriteShort" + ex);
                return false;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public bool WriteUInt32(IntPtr pOffset, uint pData)
        {
            if (processHandle == IntPtr.Zero)
                CheckProcess();
            try
            {
                return WriteByteArray(pOffset, BitConverter.GetBytes(pData));
            }
            catch (Exception ex)
            {
                if (debugMode)
                    Console.WriteLine("Error: WriteUInt32" + ex);
                return false;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public bool WriteUInteger(IntPtr pOffset, uint pData)
        {
            if (processHandle == IntPtr.Zero)
                CheckProcess();
            try
            {
                return WriteByteArray(pOffset, BitConverter.GetBytes(pData));
            }
            catch (Exception ex)
            {
                if (debugMode)
                    Console.WriteLine("Error: WriteUInt" + ex);
                return false;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public bool WriteUInt64(IntPtr pOffset, ulong pData)
        {
            if (processHandle == IntPtr.Zero)
                CheckProcess();
            try
            {
                return WriteByteArray(pOffset, BitConverter.GetBytes(pData));
            }
            catch (Exception ex)
            {
                if (debugMode)
                    Console.WriteLine("Error: WriteUInt64" + ex);
                return false;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public bool WriteULong(IntPtr pOffset, ulong pData)
        {
            if (processHandle == IntPtr.Zero)
                CheckProcess();
            try
            {
                return WriteByteArray(pOffset, BitConverter.GetBytes(pData));
            }
            catch (Exception ex)
            {
                if (debugMode)
                    Console.WriteLine("Error: WriteULong" + ex);
                return false;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public bool WriteFloat(IntPtr pOffset, float pData)
        {
            if (processHandle == IntPtr.Zero)
                CheckProcess();
            try
            {
                return WriteByteArray(pOffset, BitConverter.GetBytes(pData));
            }
            catch (Exception ex)
            {
                if (debugMode)
                    Console.WriteLine("Error: WriteFloat" + ex);
                return false;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public bool WriteDouble(IntPtr pOffset, double pData)
        {
            if (processHandle == IntPtr.Zero)
                CheckProcess();
            try
            {
                return WriteByteArray(pOffset, BitConverter.GetBytes(pData));
            }
            catch (Exception ex)
            {
                if (debugMode)
                    Console.WriteLine("Error: WriteDouble" + ex);
                return false;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public Process GetMainProcess()
        {
            return mainProcess[0];
        }

        private void ErrorProcessNotFound(string pProcessName)
        {
            Log.Print("ERROR",
                $"{processName} {"is not running or has not been found. Try to open the loader as an administrator."}");
            Environment.Exit(0);
        }

        [Flags]
        private enum ProcessAccessFlags : uint
        {
            All = 2035711,
            Terminate = 1,
            CreateThread = 2,
            VMOperation = 8,
            VMRead = 16,
            VMWrite = 32,
            DupHandle = 64,
            SetInformation = 512,
            QueryInformation = 1024,
            Synchronize = 1048576
        }

        private enum VirtualMemoryProtection : uint
        {
            PAGE_NOACCESS = 1,
            PAGE_READONLY = 2,
            PAGE_READWRITE = 4,
            PAGE_WRITECOPY = 8,
            PAGE_EXECUTE = 16,
            PAGE_EXECUTE_READ = 32,
            PAGE_EXECUTE_READWRITE = 64,
            PAGE_EXECUTE_WRITECOPY = 128,
            PAGE_GUARD = 256,
            PAGE_NOCACHE = 512,
            PROCESS_ALL_ACCESS = 2035711
        }
    }
}
