﻿using System;
using NFSScript.Core;
using NFSScript.Math;
using static NFSScript.Core.GameMemory;
using static NFSScript.Core.MWAddresses;
using static NFSScript.MWFunctions;

namespace NFSScript.MW
{
    /// <summary>
    /// A class that represents the game's UI.
    /// </summary>
    public static class UI
    {
        /// <summary>
        /// Dialog info int.
        /// </summary>
        public const int DIALOG_INFO = 0;

        /// <summary>
        /// Dialog warning int
        /// </summary>
        public const int DIALOG_WARNING = 1;

        /// <summary>
        /// Returns the point of where the game's cursor is located on screen. (Might be inaccurate)
        /// </summary>
        public static Point CursorPosition
        {
            get
            {
                var x = Memory.ReadUShort((IntPtr)UIAddrs.STATIC_CURSOR_POS_X);
                var y = Memory.ReadUShort((IntPtr)UIAddrs.STATIC_CURSOR_POS_Y);

                return new Point(x, y);
            }
        }

        /// <summary>
        /// Shows the EA Trax notification 6 seconds after calling this function.
        /// </summary>
        public static void CallEATraxNotification()
        {
            Memory.WriteByte((IntPtr)UIAddrs.STATIC_TRIGGER_EA_TRAX_NOTIFICATION, 1);
        }

        /// <summary>
        /// Shows the GPS lost message.
        /// </summary>
        public static void CallGPSLost()
        {
            Function.Call(GPS_LOST);
        }

        /// <summary>
        /// Shows the race countdown.
        /// </summary>
        public static void ShowRaceCountdown()
        {
            //ASM.CallAssembly(Funcs.E_SHOW_RACE_COUNTDOWN);
            Function.Call(UI_SHOW_RACE_COUTNDOWN);
        }

        /// <summary>
        /// Pops a dialog box open with the defined message string.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="id"></param>
        public static void ShowDialogBox(int id, string message)
        {
            _setHelloWorldString(message);
            byte[] ar;
            switch (id)
            {
                case DIALOG_INFO:
                    ar = DIALOG_BOX_SHOW_INFO;
                    break;

                case DIALOG_WARNING:
                    ar = DIALOG_BOX_SHOW_WARNING;
                    break;

                default:
                    ar = DIALOG_BOX_SHOW_INFO;
                    break;
            }

            ASM.CallAssembly(ar);
        }

        /// <summary>
        /// Shows a message on screen.
        /// </summary>
        /// <param name="message"></param>
        public static void ShowTextScreenMessage(string message)
        {
            _setDebugErrorString(message);
            Function.Call(SHOW_SCREEN_MESSAGE);
        }

        private const int STRING_MAX_LENGTH = 128;
        internal static void _setHelloWorldString(string s)
        {
            var address = (IntPtr)UIAddrs.STATIC_STRING_HELLO_WORLD_ADDRESS;
            ASM.Abolish(address, STRING_MAX_LENGTH);
            var newString = s;
            if (newString.Length > STRING_MAX_LENGTH)
                newString = newString.Substring(0, STRING_MAX_LENGTH);
            Memory.WriteStringASCII(address, newString);
        }

        private const int DEBUG_ERROR_STRING_MAX_LENGTH = 64;
        internal static void _setDebugErrorString(string s)
        {
            var address = (IntPtr)UIAddrs.STATIC_DEBUG_ERROR_MSG_ADDRESS;
            ASM.Abolish(address, DEBUG_ERROR_STRING_MAX_LENGTH);
            var newString = s;
            if (newString.Length > DEBUG_ERROR_STRING_MAX_LENGTH)
                newString = newString.Substring(0, DEBUG_ERROR_STRING_MAX_LENGTH);
            Memory.WriteStringASCII(address, newString);
        }
    }
}
