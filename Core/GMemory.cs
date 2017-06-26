using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using static NFSScript.Core.NativeMethods;

namespace NFSScript.Core
{
    /// <summary>
    /// A memory class with generic functions.
    /// </summary>
    public class GMemory
    {
        /// <summary>
        /// 
        /// </summary>
        public static bool DebugMode;
        private IntPtr _baseAddress;
        private ProcessModule _processModule;
        private Process[] _mainProcess;

        /// <summary>
        /// 
        /// </summary>
        public string ProcessName { get; set; }

        /// <summary>
        /// Get the process handle.
        /// </summary>
        public IntPtr ProcessHandle { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public long BaseAddress
        {
            get
            {
                // Before you complain, this will be inlined by the JIT.
                // The method call should make it clearer that this isn't a transparent operation.
                SetVariables();
                return (long)_baseAddress;
            }
        }

        private void SetVariables()
        {
            _baseAddress = IntPtr.Zero;
            _processModule = _mainProcess[0].MainModule;
            _baseAddress = _processModule.BaseAddress;
        }

        /// <summary>
        /// 
        /// </summary>
        public GMemory()
        {
        }

        /// <summary>
        /// 
        /// </summary>
        public GMemory(string pProcessName)
        {
            ProcessName = pProcessName;
            CheckProcess();
            SetVariables();
        }

        /// <summary>
        /// 
        /// </summary>
        public bool CheckProcess()
        {
            if (ProcessName != null)
            {
                _mainProcess = Process.GetProcessesByName(ProcessName);
                if (_mainProcess.Length == 0)
                {
                    ErrorProcessNotFound(ProcessName);
                    return false;
                }
                ProcessHandle = OpenProcess(2035711U, false, _mainProcess[0].Id);
                if (!(ProcessHandle == IntPtr.Zero))
                    return true;
                ErrorProcessNotFound(ProcessName);
                return false;
            }
            //int num = (int)MessageBox.Show("Programmer, define process name first!");
            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="address"></param>
        /// <param name="size"></param>
        /// <param name="isRelativeToMemoryBase"></param>
        public byte[] ReadByteArray(int address, uint size, bool isRelativeToMemoryBase)
        {
            if (ProcessHandle == IntPtr.Zero)
                CheckProcess();
            try
            {
                if (isRelativeToMemoryBase)
                    address += (int)_baseAddress;

                var addr = (IntPtr)address;
                uint lpflOldProtect;
                var lpBuffer = new byte[size];

                VirtualProtectEx(ProcessHandle, addr, (UIntPtr)size, 0x4, out lpflOldProtect);
                var read = ReadProcessMemory(ProcessHandle, addr, lpBuffer, size, 0x0);
                VirtualProtectEx(ProcessHandle, addr, (UIntPtr)size, lpflOldProtect, out lpflOldProtect);

                if (read)
                    return lpBuffer;
                throw new Exception("ReadProcessMemory returned false.");
            }
            catch (Exception ex)
            {
                if (DebugMode)
                    Log.Print("ERROR",
                        $"Error during ReadByteArray:\r\nAddress: {address:X}, Size: {size}, isRelativeToMemoryBase: {isRelativeToMemoryBase}\r\n{ex}");
                return new byte[1];
            }
        }

        /// <summary>
        /// Reads the value from the memory and returns the <typeparamref name="T"/> instance.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="address"></param>
        /// <param name="isRelativeToMemoryBase">Whether the address is relevant to the module's base address.</param>
        public T Read<T>(int address, bool isRelativeToMemoryBase = false)
        {
            return ReadByteArray(address, typeof(T) == typeof(bool) ? 1 : (uint)Marshal.SizeOf(typeof(T)), isRelativeToMemoryBase)
                .GetObject<T>();
        }

        /// <summary>
        /// Reads a string from the memory with the specified <paramref name="encoding"/>.
        /// </summary>
        /// <param name="encoding">The encoding to use.</param>
        /// <param name="address"></param>
        /// <param name="length">Length of the string.</param>
        /// <param name="isRelativeToMemoryBase">Whether the address is relevant to the module's base address.</param>
        public string ReadString(Encoding encoding, int address, uint length, bool isRelativeToMemoryBase = false)
        {
            try
            {
                return encoding.GetString(ReadByteArray(address, length, isRelativeToMemoryBase));
            }
            catch (Exception ex)
            {
                if (DebugMode)
                    Log.Print("ERROR",
                        $"Error during ReadString:\r\nEncoding: {encoding.BodyName}, Address: {address:X}, Length: {length}, isRelativeToMemoryBase: {isRelativeToMemoryBase}\r\n{ex}");
                return string.Empty;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="address"></param>
        /// <param name="bytes"></param>
        /// <param name="isRelativeToMemoryBase"></param>
        public bool WriteByteArray(int address, byte[] bytes, bool isRelativeToMemoryBase)
        {
            if (ProcessHandle == IntPtr.Zero)
                CheckProcess();
            try
            {
                if (isRelativeToMemoryBase)
                    address += (int)_baseAddress;

                var addr = (IntPtr)address;
                uint lpflOldProtect;

                VirtualProtectEx(ProcessHandle, addr, (UIntPtr)(bytes.Length), 0x4, out lpflOldProtect);
                var flag = WriteProcessMemory(ProcessHandle, addr, bytes, (uint)bytes.Length, 0x0);
                VirtualProtectEx(ProcessHandle, addr, (UIntPtr)(bytes.Length), lpflOldProtect, out lpflOldProtect);

                return flag;
            }
            catch (Exception ex)
            {
                if (DebugMode)
                    Log.Print("ERROR",
                        $"Error during WriteByteArray:\r\nAddress: {address:X}, bytes.Length: {bytes.Length}, isRelativeToMemoryBase: {isRelativeToMemoryBase}\r\n{ex}");
                return false;
            }
        }

        /// <summary>
        /// Writes the <paramref name="value"/> to the memory.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="address"></param>
        /// <param name="value">The value to write.</param>
        /// <param name="isRelativeToMemoryBase">Whether the address is relevant to the module's base address.</param>
        public bool Write<T>(int address, T value, bool isRelativeToMemoryBase = false)
        {
            return WriteByteArray(address, value.GetBytes(), isRelativeToMemoryBase);
        }

        /// <summary>
        /// Writes '<paramref name="str"/>' to the memory using specified <paramref name="encoding"/> to get bytes.
        /// </summary>
        /// <remarks>
        /// Automatically converts str into a null-terminated string.
        /// </remarks>
        /// <param name="encoding">The encoding to use.</param>
        /// <param name="address"></param>
        /// <param name="str">The string to write.</param>
        /// <param name="isRelativeToMemoryBase">Whether the address is relevant to the module's base address.</param>
        public bool WriteString(Encoding encoding, int address, string str, bool isRelativeToMemoryBase = false)
        {
            try
            {
                return WriteByteArray(address, encoding.GetBytes(str + '\0'), isRelativeToMemoryBase);
            }
            catch (Exception ex)
            {
                if (DebugMode)
                    Log.Print("ERROR",
                        $"Error during WriteString:\r\nEncoding: {encoding.BodyName}, Address: {address:X}, String: {str}, isRelativeToMemoryBase: {isRelativeToMemoryBase}\r\n{ex}");
                return false;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public Process GetMainProcess()
        {
            return _mainProcess[0];
        }

        private void ErrorProcessNotFound(string pProcessName)
        {
            // FIXME: Shouldn't this be pProcessName?
            Log.Print("ERROR", $"{ProcessName} is not running or has not been found. Try to open the loader as an administrator.");
            Environment.Exit(0);
        }
    }
}