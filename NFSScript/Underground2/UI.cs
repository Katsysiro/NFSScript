using System;
using NFSScript.Math;
using static NFSScript.Core.GameMemory;
using Addrs = NFSScript.Core.UG2Addresses;

namespace NFSScript.Underground2
{

    /// <summary>
    /// A struct that represents the game's UI.
    /// </summary>
    public struct UI
    {
        /// <summary>
        /// Returns the point of where the game's cursor is located on screen.
        /// </summary>
        public static Point CursorPosition
        {
            get
            {
                var x = Memory.ReadUShort((IntPtr)Addrs.UIAddrs.STATIC_CURSOR_POS_X);
                var y = Memory.ReadUShort((IntPtr)Addrs.UIAddrs.STATIC_CURSOR_POS_Y);

                return new Point(x, y);
            }
        }
    }
}
