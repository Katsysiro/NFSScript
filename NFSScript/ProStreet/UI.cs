using System;
using NFSScript.Core;
using NFSScript.Math;
using static NFSScript.Core.GameMemory;
using Addrs = NFSScript.Core.ProStreetAddresses;

namespace NFSScript.ProStreet
{
    /// <summary>
    /// A struct that represents the game's UI.
    /// </summary>
    public static class UI
    {
        private const int INVALID_CODE_ENTRY_TEXT_MAX_LENGTH = 108;

        /// <summary>
        /// Set the code entry message when a player enters a cheat code that is not valid.
        /// </summary>
        /// <param name="msg"></param>
        public static void SetCodeEntryMessage(string msg)
        {
            var addr = Memory.ReadInt32((IntPtr)Memory.getBaseAddress + Addrs.UIAddrs.PNON_STATIC_UI_INVALID_CODE_ENTRY_TEXT);
            addr = Memory.ReadInt32((IntPtr)addr + Addrs.UIAddrs.POINTER_UI_INVALID_CODE_ENTRY_TEXT_1);
            addr = Memory.ReadInt32((IntPtr)addr + Addrs.UIAddrs.POINTER_UI_INVALID_CODE_ENTRY_TEXT_2);
            addr = Memory.ReadInt32((IntPtr)addr + Addrs.UIAddrs.POINTER_UI_INVALID_CODE_ENTRY_TEXT_3);
            addr = Memory.ReadInt32((IntPtr)addr + Addrs.UIAddrs.POINTER_UI_INVALID_CODE_ENTRY_TEXT_4);
            var final = (IntPtr)addr + Addrs.UIAddrs.POINTER_UI_INVALID_CODE_ENTRY_TEXT_5;
            ASM.Abolish(final, INVALID_CODE_ENTRY_TEXT_MAX_LENGTH);
            Memory.WriteStringASCII(final, msg.Substring(0, INVALID_CODE_ENTRY_TEXT_MAX_LENGTH));
        }

        /// <summary>
        /// Returns the point of where the game's cursor is located on screen. (Fairly accurate)
        /// </summary>
        public static Point CursorPosition
        {
            get
            {
                var x = Memory.ReadUShort((IntPtr)ProStreetAddresses.UIAddrs.STATIC_CURSOR_POS_X);
                var y = Memory.ReadUShort((IntPtr)ProStreetAddresses.UIAddrs.STATIC_CURSOR_POS_Y);

                return new Point(x, y);
            }
        }
    }
}
