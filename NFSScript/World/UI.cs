using System;
using NFSScript.Math;
using static NFSScript.Core.GameMemory;
using static NFSScript.Core.WorldAddresses;

namespace NFSScript.World
{
    /// <summary>
    /// A class that represents the game's UI.
    /// </summary>
    public static class UI
    {
        /// <summary>
        /// Returns the point of where the game's cursor is located on screen. (Might be inaccurate)
        /// </summary>
        public static Point CursorPosition
        {
            get
            {
                var x = Memory.ReadUShort((IntPtr)Memory.getBaseAddress + UIAddrs.NON_STATIC_CURSOR_POS_X);
                var y = Memory.ReadUShort((IntPtr)Memory.getBaseAddress + UIAddrs.NON_STATIC_CURSOR_POS_Y);

                return new Point(x, y);
            }
        }
    }
}
