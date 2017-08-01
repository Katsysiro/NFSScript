using static NFSScript.Core.GameMemory;

namespace NFSScript.World
{
    /// <summary>
    /// A class that makes life easy.
    /// </summary>
    internal static class MemoryBase
    {
        internal static uint FunctionBase => ((uint)Memory.getBaseAddress - 0x400000);
    }
}
