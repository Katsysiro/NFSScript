using System;
using System.Collections.Generic;
using NFSScript.Math;
using static NFSScript.Core.GameMemory;
using Addrs = NFSScript.Core.MWAddresses;

namespace NFSScript.MW
{
    /// <summary>
    /// A struct that represents the game's <see cref="Player"/>.
    /// </summary>
    public static class Player
    {
        /// <summary>
        /// Sets a value indicating whether the AI controls the <see cref="Player"/>'s car.
        /// </summary>
        public static bool IsControlledByAi => Memory.ReadByte((IntPtr)Addrs.PlayerAddrs.STATIC_PLAYER_AI_CONTROL) == 1;

        /// <summary>
        /// Force the AI to control the <see cref="Player"/>'s car.
        /// </summary>
        public static void ForceAIControl()
        {
            Memory.WriteByte((IntPtr)Addrs.PlayerAddrs.STATIC_PLAYER_AI_CONTROL, 1);
        }

        /// <summary>
        /// Clear the AI control for the <see cref="Player"/>'s car.
        /// </summary>
        public static void ClearAIControl()
        {
            Memory.WriteByte((IntPtr)Addrs.PlayerAddrs.STATIC_PLAYER_AI_CONTROL, 0);
        }

        /// <summary>
        /// Saves the current <see cref="Player"/>'s current position to TRACKS/HotPositionL2RA.HOT.
        /// </summary>
        /// <param name="b">Valid range is 1-5</param>
        public static void SaveCurrentPositionToHOT(byte b)
        {
            Memory.WriteByte((IntPtr)Addrs.PlayerAddrs.STATIC_PLAYER_HOT_SAVE_POSITION, b);
        }

        /// <summary>
        /// Warps the <see cref="Player"/> to a saved position from TRACKS/HotPositionL2RA.HOT.
        /// </summary>
        /// <param name="b">Valid range is 1-5</param>
        public static void WarpToSavedPositionFromHOT(byte b)
        {
            Memory.WriteByte((IntPtr)Addrs.PlayerAddrs.STATIC_PLAYER_HOT_WARP_TO_SAVED_POSITION, b);
        }

        /// <summary>
        /// A class that represents the <see cref="Player"/>'s car.
        /// </summary>
        public static class Car
        {
            /// <summary>
            /// The <see cref="Player"/>'s car position
            /// </summary>
            public static Vector3 Position
            {
                get
                {
                    var x = Memory.ReadFloat((IntPtr)Addrs.PlayerAddrs.STATIC_PLAYER_POS_X);
                    var y = Memory.ReadFloat((IntPtr)Addrs.PlayerAddrs.STATIC_PLAYER_POS_Y);
                    var z = Memory.ReadFloat((IntPtr)Addrs.PlayerAddrs.STATIC_PLAYER_POS_Z);

                    return new Vector3(x, y, z);
                }
                set
                {
                    Memory.WriteFloat((IntPtr)Addrs.PlayerAddrs.STATIC_PLAYER_POS_X, value.X);
                    Memory.WriteFloat((IntPtr)Addrs.PlayerAddrs.STATIC_PLAYER_POS_Y, value.Y);
                    Memory.WriteFloat((IntPtr)Addrs.PlayerAddrs.STATIC_PLAYER_POS_Z, value.Z);
                }
            }

            /// <summary>
            /// The <see cref="Player"/>'s car rotation
            /// </summary>
            public static Quaternion Rotation
            {
                get
                {
                    var x = Memory.ReadFloat((IntPtr)Addrs.PlayerAddrs.STATIC_PLAYER_X_ROT);
                    var y = Memory.ReadFloat((IntPtr)Addrs.PlayerAddrs.STATIC_PLAYER_Y_ROT);
                    var z = Memory.ReadFloat((IntPtr)Addrs.PlayerAddrs.STATIC_PLAYER_Z_ROT);
                    var w = Memory.ReadFloat((IntPtr)Addrs.PlayerAddrs.STATIC_PLAYER_W_ROT);

                    return new Quaternion(x, y, z, w);
                }
                set
                {
                    Memory.WriteFloat((IntPtr)Addrs.PlayerAddrs.STATIC_PLAYER_X_ROT, value.X);
                    Memory.WriteFloat((IntPtr)Addrs.PlayerAddrs.STATIC_PLAYER_Y_ROT, value.Y);
                    Memory.WriteFloat((IntPtr)Addrs.PlayerAddrs.STATIC_PLAYER_Z_ROT, value.Z);
                    Memory.WriteFloat((IntPtr)Addrs.PlayerAddrs.STATIC_PLAYER_W_ROT, value.W);
                }
            }

            /// <summary>
            /// Returns the <see cref="Player"/>'s car speed.
            /// </summary>
            public static float Speed => Memory.ReadFloat((IntPtr)Addrs.PlayerAddrs.STATIC_PLAYER_SPEED);

            /// <summary>
            /// Returns the car's left and right headlights intensity value.
            /// </summary>
            /// <returns></returns>
            public static Dictionary<string, float> GetCarHeadlightsBrightnessIntensityValue()
            {
                var dic = new Dictionary<string, float>();


                dic.Add("Left", Memory.ReadFloat((IntPtr)Addrs.PlayerAddrs.STATIC_PLAYER_HEADLIGHTS_LEFT));
                dic.Add("Right", Memory.ReadFloat((IntPtr)Addrs.PlayerAddrs.STATIC_PLAYER_HEADLIGHTS_RIGHT));

                return dic;
            }
        }
    }
}
