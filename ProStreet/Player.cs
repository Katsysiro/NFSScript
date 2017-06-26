using System;
using NFSScript.Math;
using static NFSScript.Core.GameMemory;
using Addrs = NFSScript.Core.ProStreetAddresses;
using Funcs = NFSScript.ProStreetFunctions;

namespace NFSScript.ProStreet
{
    /// <summary>
    /// A class that represents the game's <see cref="Player"/>.
    /// </summary>
    public static class Player
    {
        /// <summary>
        /// <see cref="Player"/>'s cash (Read only).
        /// </summary>
        public static int Cash
        {
            get
            {
                var addr = Memory.ReadInt32((IntPtr)Memory.getBaseAddress + Addrs.PlayerAddrs.NON_STATIC_PLAYER_CASH);
                return Memory.ReadInt32((IntPtr)addr + Addrs.PlayerAddrs.POINTER_NON_STATIC_PLAYER_CASH);
            }
        }

        /// <summary>
        /// Award the <see cref="Player"/> with cash.
        /// </summary>
        /// <param name="value">The amount of cash to award.</param>
        public static void AwardCash(int value)
        {
            var addr = Memory.ReadInt32((IntPtr)Memory.getBaseAddress + Addrs.PlayerAddrs.NON_STATIC_PLAYER_CASH);
            Memory.WriteInt32((IntPtr)addr + Addrs.PlayerAddrs.POINTER_NON_STATIC_PLAYER_CASH, Cash + value);
        }

        /// <summary>
        /// Force the AI to control the <see cref="Player"/>'s car.
        /// </summary>
        public static void ForceAIControl()
        {
            Function.Call(Funcs.FORCE_AI_CONTROL);
        }

        /// <summary>
        /// Clear the AI control for the <see cref="Player"/>'s car.
        /// </summary>
        public static void ClearAIControl()
        {
            Function.Call(Funcs.CLEAR_AI_CONTROL);
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
            /// Returns the <see cref="Player"/>'s car current speed in MPH.
            /// </summary>
            /// <returns></returns>
            public static float Speed => Memory.ReadFloat((IntPtr)Addrs.PlayerAddrs.STATIC_PLAYER_SPEED);
        }
    }
}
