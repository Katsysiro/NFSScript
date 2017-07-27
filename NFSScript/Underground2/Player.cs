﻿using System;
using NFSScript.Math;
using static NFSScript.Core.GameMemory;
using Addrs = NFSScript.Core.UG2Addresses;


namespace NFSScript.Underground2
{
    /// <summary>
    /// A class that represents the game's <see cref="Player"/>.
    /// </summary>
    public static class Player
    {
        /// <summary>
        /// <see cref="Player"/>'s cash (Read only).
        /// </summary>
        public static int Cash => Memory.ReadInt32((IntPtr)Addrs.PlayerAddrs.STATIC_PLAYER_CASH);

        /// <summary>
        /// Award the <see cref="Player"/> with cash.
        /// </summary>
        /// <param name="value"></param>
        public static void AwardCash(int value)
        {
            Memory.WriteInt32((IntPtr)Addrs.PlayerAddrs.STATIC_PLAYER_CASH, Cash + value);
        }

        /// <summary>
        /// A class that represents the <see cref="Player"/>'s current car
        /// </summary>
        public static class Car
        {
            /// <summary>
            /// Returns the <see cref="Player"/>'s car current position (Read only).
            /// </summary>
            public static Vector3 Position
            {
                get
                {
                    var x = Memory.ReadFloat((IntPtr)Addrs.PlayerAddrs.STATIC_PLAYER_POSITION_READONLY_X);
                    var y = Memory.ReadFloat((IntPtr)Addrs.PlayerAddrs.STATIC_PLAYER_POSITION_READONLY_Y);
                    var z = Memory.ReadFloat((IntPtr)Addrs.PlayerAddrs.STATIC_PLAYER_POSITION_READONLY_Z);

                    return new Vector3(x, y, z);
                }
            }

            /// <summary>
            /// Returns the <see cref="Player"/>'s current car star rating
            /// </summary>
            public static float StarRating => Memory.ReadFloat((IntPtr)Addrs.PlayerAddrs.STATIC_PLAYER_CURRENT_CAR_STAR_RATING);

            /// <summary>
            /// Returns the <see cref="Player"/>'s car current speed in MPH
            /// </summary>
            /// <returns></returns>
            public static float Speed => Memory.ReadFloat((IntPtr)Addrs.PlayerAddrs.STATIC_PLAYER_CAR_SPEED_MPH);
        }
    }
}
