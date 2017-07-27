using System;
using NFSScript.Core;
using NFSScript.Math;
using static NFSScript.Core.GameMemory;
using Addrs = NFSScript.Core.UndercoverAddresses;

namespace NFSScript.Undercover
{
    /// <summary>
    /// A struct that represents the game's UI.
    /// </summary>
    public static class UI
    {
        private const int INVALID_SECRET_CODE_ENTRY_TEXT_MAX_LENGTH = 87;

        /// <summary>
        /// Set the secret code entry message when a player enters a cheat code that is not valid.
        /// </summary>
        /// <param name="msg"></param>
        public static void SetSecretCodeEntryMessage(string msg)
        {
            var address = (IntPtr)Addrs.UIAddrs.STATIC_UI_INVALID_SECRET_CODE_ENTRY_TEXT;
            ASM.Abolish(address, INVALID_SECRET_CODE_ENTRY_TEXT_MAX_LENGTH);
            Memory.WriteStringASCII(address, msg.Substring(0, INVALID_SECRET_CODE_ENTRY_TEXT_MAX_LENGTH));
        }

        /// <summary>
        /// Returns the point of where the game's cursor is located on screen. (Inaccurate)
        /// </summary>
        public static Point CursorPosition
        {
            get
            {
                var x = Memory.ReadUShort((IntPtr)UndercoverAddresses.UIAddrs.STATIC_CURSOR_POS_X);
                var y = Memory.ReadUShort((IntPtr)UndercoverAddresses.UIAddrs.STATIC_CURSOR_POS_Y);

                return new Point(x, y);
            }
        }

        /// <summary>
        /// Returns the point of where the world map's cursor is located on screen. (Inaccurate)
        /// </summary>
        public static Point WorldMapCursorPosition
        {
            get
            {
                var x = Memory.ReadUShort((IntPtr)UndercoverAddresses.UIAddrs.STATIC_UI_WORLD_MAP_CUSROR_POS_X);
                var y = Memory.ReadUShort((IntPtr)UndercoverAddresses.UIAddrs.STATIC_UI_WORLD_MAP_CUSROR_POS_Y);

                return new Point(x, y);
            }
        }

        /// <summary>
        /// A class that represents the UI's minimap.
        /// </summary>
        public static class Minimap
        {
            /// <summary>
            /// 
            /// </summary>
            public static bool ShowNonPursuitCops {
                get => Memory.ReadByte((IntPtr)Addrs.UIAddrs.STATIC_MINIMAP_SHOW_NON_PURSUIT_COPS) == 1;
                set => Memory.WriteBoolean((IntPtr)Addrs.UIAddrs.STATIC_MINIMAP_SHOW_NON_PURSUIT_COPS, value);
            }
        }
    }
}
