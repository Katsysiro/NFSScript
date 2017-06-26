using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NFSScript.Math;
using static NFSScript.Core.GameMemory;
using static NFSScript.Core.CarbonAddresses;
using static NFSScript.CarbonFunctions;

namespace NFSScript.Carbon
{
    /// <summary>
    /// A class that represents the game's <see cref="Player"/>.
    /// </summary>
    public static class Player
    {
        #region Constant Variables
        private const uint POLICE_IGNORE_PLAYER_ENABLED = 3943023862U;
        private const uint POLICE_IGNORE_PLAYER_DISABLED = 2047198454U;
        #endregion

        /// <summary>
        /// Returns the <see cref="Player"/>'s last known position (Read only).
        /// </summary>
        public static Vector3 LastKnownPosition
        {
            get
            {
                var x = Memory.ReadFloat((IntPtr)PlayerAddrs.STATIC_PLAYER_LAST_KNOWN_POSITION_X);
                var y = Memory.ReadFloat((IntPtr)PlayerAddrs.STATIC_PLAYER_LAST_KNOWN_POSITION_Y);
                var z = Memory.ReadFloat((IntPtr)PlayerAddrs.STATIC_PLAYER_LAST_KNOWN_POSITION_Z);

                return new Vector3(x, y, z);
            }
        }

        /// <summary>
        /// <see cref="Player"/>'s cash (Read only).
        /// </summary>
        public static int Cash
        {
            get
            {
                var address = Memory.ReadInt32((IntPtr)Memory.getBaseAddress + PlayerAddrs.PNON_STATIC_PLAYER_CASH);
                address = Memory.ReadInt32((IntPtr)address + PlayerAddrs.POINTER_PLAYER_CASH_1);
                address = Memory.ReadInt32((IntPtr)address + PlayerAddrs.POINTER_PLAYER_CASH_2);
                address = Memory.ReadInt32((IntPtr)address + PlayerAddrs.POINTER_PLAYER_CASH_3);
                address = Memory.ReadInt32((IntPtr)address + PlayerAddrs.POINTER_PLAYER_CASH_4);
                //address = memory.ReadInt32((IntPtr)address + PlayerAddrs.POINTER_PLAYER_CASH_5);

                return Memory.ReadInt32((IntPtr)address + PlayerAddrs.POINTER_PLAYER_CASH_5);
            }
        }

        /// <summary>
        /// Sets a value indicating whether the <see cref="Player"/> is ignored by the police or not.
        /// </summary>
        public static bool IgnoredByPolice
        {
            get
            {
                var value = Memory.ReadUInteger((IntPtr)PlayerAddrs.STATIC_POLICE_IGNORE_PLAYER);
                if (value == POLICE_IGNORE_PLAYER_ENABLED)
                    return true;
                return false;
            }
            set
            {
                if (value)
                    Memory.WriteUInteger((IntPtr)PlayerAddrs.STATIC_POLICE_IGNORE_PLAYER, POLICE_IGNORE_PLAYER_ENABLED);
                else Memory.WriteUInteger((IntPtr)PlayerAddrs.STATIC_POLICE_IGNORE_PLAYER, POLICE_IGNORE_PLAYER_DISABLED);
            }
        }

        /// <summary>
        /// Sets a value indicating whether the AI controls the <see cref="Player"/>'s car.
        /// </summary>
        public static bool IsControlledByAi => _safeGetAIControlValue();

        /// <summary>
        /// Returns the name of the player.
        /// </summary>
        public static string Name
        {
            get
            {
                var address = Memory.ReadInt32((IntPtr)Memory.getBaseAddress + PlayerAddrs.NON_STATIC_PLAYER_NAME);
                address = Memory.ReadInt32((IntPtr)address + PlayerAddrs.POINTER_PLAYER_NAME_1);
                address = Memory.ReadInt32((IntPtr)address + PlayerAddrs.POINTER_PLAYER_NAME_2);
                address = Memory.ReadInt32((IntPtr)address + PlayerAddrs.POINTER_PLAYER_NAME_3);
                address = Memory.ReadInt32((IntPtr)address + PlayerAddrs.POINTER_PLAYER_NAME_4);

                return Encoding.ASCII.GetString(Memory.ReadByteArray((IntPtr)address + PlayerAddrs.POINTER_PLAYER_NAME_5, 15).Where(x => x != 0x00).ToArray());
            }
        }

        /// <summary>
        /// Award the <see cref="Player"/> with cash.
        /// </summary>
        /// <param name="value"></param>
        public static void AwardCash(int value)
        {
            var address = Memory.ReadInt32((IntPtr)Memory.getBaseAddress + PlayerAddrs.PNON_STATIC_PLAYER_CASH);
            address = Memory.ReadInt32((IntPtr)address + PlayerAddrs.POINTER_PLAYER_CASH_1);
            address = Memory.ReadInt32((IntPtr)address + PlayerAddrs.POINTER_PLAYER_CASH_2);
            address = Memory.ReadInt32((IntPtr)address + PlayerAddrs.POINTER_PLAYER_CASH_3);
            address = Memory.ReadInt32((IntPtr)address + PlayerAddrs.POINTER_PLAYER_CASH_4);

            Memory.WriteInt32((IntPtr)address + PlayerAddrs.POINTER_PLAYER_CASH_5, Cash + value);
        }

        /// <summary>
        /// Force the AI to control the <see cref="Player"/>'s car.
        /// </summary>
        public static void ForceAIControl()
        {
            var u = Memory.ReadUInteger((IntPtr)PlayerAddrs.STATIC_PLAYER_AI_CONTROL);
            Memory.WriteByte((IntPtr)u + PlayerAddrs.POINTER_PLAYER_AI_CONTROL_POINTER, 1);
        }

        /// <summary>
        /// Clear the AI control for the <see cref="Player"/>'s car.
        /// </summary>
        public static void ClearAIControl()
        {
            var u = Memory.ReadUInteger((IntPtr)PlayerAddrs.STATIC_PLAYER_AI_CONTROL);
            Memory.WriteByte((IntPtr)u + PlayerAddrs.POINTER_PLAYER_AI_CONTROL_POINTER, 0);
        }

        /// <summary>
        /// Prevent the player from being busted by the cops.
        /// </summary>
        public static void PreventPlayerBeingBusted()
        {
            Function.Call(PREVENT_PLAYER_BEING_BUSTED);
        }

        /// <summary>
        /// Saves the current <see cref="Player"/>'s current position to TRACKS/HotPositionL5RA.HOT.
        /// </summary>
        /// <param name="b">Valid range is 1-5</param>
        public static void SaveCurrentPositionToHOT(byte b)
        {
            Memory.WriteByte((IntPtr)PlayerAddrs.STATIC_PLAYER_HOT_SAVE_POSITION, b);
        }

        /// <summary>
        /// Warps the <see cref="Player"/> to a saved position from TRACKS/HotPositionL5RA.HOT.
        /// </summary>
        /// <param name="b">Valid range is 1-5</param>
        public static void WarpToSavedPositionFromHOT(byte b)
        {
            Memory.WriteByte((IntPtr)PlayerAddrs.STATIC_PLAYER_HOT_WARP_TO_SAVED_POSITION, b);
        }
        
        internal static bool _safeGetAIControlValue()
        {
            var skipFEEnabled = Memory.ReadByte((IntPtr)GameAddrs.STATIC_SKIP_FE_DAMAGE_ENABLED);
            var skipFEAI = Memory.ReadByte((IntPtr)GameAddrs.STATIC_SKIP_FE_AI_CONTROL_PLAYER);
            var u = Memory.ReadUInteger((IntPtr)PlayerAddrs.STATIC_PLAYER_AI_CONTROL);
            var aiControl = Memory.ReadByte((IntPtr)u + PlayerAddrs.POINTER_PLAYER_AI_CONTROL_POINTER);

            if (aiControl == 1)
                return true;

            if (skipFEEnabled == 1)
            {
                if (skipFEAI == 1)
                    return true;
                if (skipFEAI == 0)
                    return false;
            }

            return false;
        }

        /// <summary>
        /// A class that represents the <see cref="Player"/>'s crew.
        /// </summary>
        public static class Crew
        {
            /// <summary>
            /// Returns the player's crew name.
            /// </summary>
            public static string Name
            {
                get
                {
                    var address = Memory.ReadInt32((IntPtr)Memory.getBaseAddress + PlayerAddrs.NON_STATIC_PLAYER_CREW_NAME);
                    address = Memory.ReadInt32((IntPtr)address + PlayerAddrs.POINTER_PLAYER_CREW_NAME_1);
                    address = Memory.ReadInt32((IntPtr)address + PlayerAddrs.POINTER_PLAYER_CREW_NAME_2);
                    address = Memory.ReadInt32((IntPtr)address + PlayerAddrs.POINTER_PLAYER_CREW_NAME_3);
                    address = Memory.ReadInt32((IntPtr)address + PlayerAddrs.POINTER_PLAYER_CREW_NAME_4);

                    return Encoding.ASCII.GetString(Memory.ReadByteArray((IntPtr)address + PlayerAddrs.POINTER_PLAYER_CREW_NAME_5, 15).Where(x => x != 0x00).Where(x => x != 0x03).ToArray());
                }
                set
                {
                    var address = Memory.ReadInt32((IntPtr)Memory.getBaseAddress + PlayerAddrs.NON_STATIC_PLAYER_CREW_NAME);
                    address = Memory.ReadInt32((IntPtr)address + PlayerAddrs.POINTER_PLAYER_CREW_NAME_1);
                    address = Memory.ReadInt32((IntPtr)address + PlayerAddrs.POINTER_PLAYER_CREW_NAME_2);
                    address = Memory.ReadInt32((IntPtr)address + PlayerAddrs.POINTER_PLAYER_CREW_NAME_3);
                    address = Memory.ReadInt32((IntPtr)address + PlayerAddrs.POINTER_PLAYER_CREW_NAME_4);

                    Memory.WriteStringASCII((IntPtr)address + PlayerAddrs.POINTER_PLAYER_CREW_NAME_5, new string(value.Take(15).ToArray()));
                }
            }

            /// <summary>
            /// The <see cref="Player"/>'s <see cref="Crew"/> color as a decimal.
            /// </summary>
            public static int Color
            {
                get => Memory.ReadInt32(_getCrewColorAddress());
                set => Memory.WriteInt32(_getCrewColorAddress(), value);
            }

            internal static IntPtr _getCrewColorAddress()
            {
                var addr = Memory.ReadInt32((IntPtr)Memory.getBaseAddress + 0x0069816C);
                addr = Memory.ReadInt32((IntPtr)addr + 0x320);
                addr = Memory.ReadInt32((IntPtr)addr + 0x304);
                addr = Memory.ReadInt32((IntPtr)addr + 0x240);
                addr = Memory.ReadInt32((IntPtr)addr + 0x28C);
                addr = addr + 0x774;

                return (IntPtr)addr;
            }
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
                    var addr = (int)Memory.getBaseAddress;
                    var x = Memory.ReadFloat((IntPtr)addr + PlayerAddrs.NON_STATIC_PLAYER_X_POS);
                    var y = Memory.ReadFloat((IntPtr)addr + PlayerAddrs.NON_STATIC_PLAYER_Y_POS);
                    var z = Memory.ReadFloat((IntPtr)addr + PlayerAddrs.NON_STATIC_PLAYER_Z_POS);

                    return new Vector3(x, y, z);
                }
                set
                {
                    var addr = (int)Memory.getBaseAddress;
                    Memory.WriteFloat((IntPtr)addr + PlayerAddrs.NON_STATIC_PLAYER_X_POS, value.X);
                    Memory.WriteFloat((IntPtr)addr + PlayerAddrs.NON_STATIC_PLAYER_Y_POS, value.Y);
                    Memory.WriteFloat((IntPtr)addr + PlayerAddrs.NON_STATIC_PLAYER_Z_POS, value.Z);
                }
            }

            /// <summary>
            /// The <see cref="Player"/>'s car rotation
            /// </summary>
            public static Quaternion Rotation
            {
                get
                {
                    var x = Memory.ReadFloat((IntPtr)PlayerAddrs.STATIC_PLAYER_X_ROT);
                    var y = Memory.ReadFloat((IntPtr)PlayerAddrs.STATIC_PLAYER_Y_ROT);
                    var z = Memory.ReadFloat((IntPtr)PlayerAddrs.STATIC_PLAYER_Z_ROT);
                    var w = Memory.ReadFloat((IntPtr)PlayerAddrs.STATIC_PLAYER_W_ROT);

                    return new Quaternion(x, y, z, w);
                }
                set
                {
                    Memory.WriteFloat((IntPtr)PlayerAddrs.STATIC_PLAYER_X_ROT, value.X);
                    Memory.WriteFloat((IntPtr)PlayerAddrs.STATIC_PLAYER_Y_ROT, value.Y);
                    Memory.WriteFloat((IntPtr)PlayerAddrs.STATIC_PLAYER_Z_ROT, value.Z);
                    Memory.WriteFloat((IntPtr)PlayerAddrs.STATIC_PLAYER_W_ROT, value.W);
                }
            }

            /// <summary>
            /// Returns the <see cref="Player"/>'s car speed.
            /// </summary>
            public static float Speed => Memory.ReadFloat((IntPtr)Memory.getBaseAddress + PlayerAddrs.NON_STATIC_PLAYER_SPEED);

            /// <summary>
            /// Resets the <see cref="Player"/>'s car.
            /// </summary>
            public static void Reset()
            {
                Function.Call(RESET_PLAYER_CAR);
            }

            static int axisAddr = Memory.ReadInt32((IntPtr)Memory.getBaseAddress + PlayerAddrs.NON_STATIC_LEFT_AXIS);
            static int axisAddr2 = Memory.ReadInt32((IntPtr)axisAddr + PlayerAddrs.PSTATIC_LEFT_AXIS_OFFSET_1);
            static int finalAxisAddress = axisAddr2 + PlayerAddrs.PSTATIC_LEFT_AXIS_OFFSET_2;

            /// <summary>
            /// <see cref="Player"/>'s car wheels left axis value.
            /// </summary>
            public static float LeftAxis
            {
                get => Memory.ReadFloat((IntPtr)finalAxisAddress);
                set => Memory.WriteFloat((IntPtr)finalAxisAddress, value);
            }

            /// <summary>
            /// <see cref="Player"/>'s car wheels right axis value.
            /// </summary>
            public static float RightAxis
            {
                get => Memory.ReadFloat((IntPtr)(finalAxisAddress - 0x4));
                set => Memory.WriteFloat((IntPtr)(finalAxisAddress - 0x4), value);
            }

            /// <summary>
            /// <see cref="Player"/>'s car wheels forward axis value.
            /// </summary>
            public static float ForwardAxis
            {
                get => Memory.ReadFloat((IntPtr)(finalAxisAddress + 0x14));
                set => Memory.WriteFloat((IntPtr)(finalAxisAddress + 0x14), value);
            }

            /// <summary>
            /// <see cref="Player"/>'s car wheels backward axis value.
            /// </summary>
            public static float BackwardAxis
            {
                get => Memory.ReadFloat((IntPtr)(finalAxisAddress + 0x18));
                set => Memory.WriteFloat((IntPtr)(finalAxisAddress + 0x18), value);
            }

            /// <summary>
            /// Returns true if the speedbreaker is currently in use by the player.
            /// </summary>
            public static bool IsUsingSpeedbreaker => Memory.ReadInt32((IntPtr)PlayerAddrs.STATIC_IS_PLAYER_USING_SPEED_BREAKER) == 1;

            /// <summary>
            /// Sets a value indicating whether the car should drift like in-game cutscnes (NIS) using the handbrake. (Unsafe, may crash the game if not called on the proper game state).
            /// </summary>
            public static bool _unsafeEnableAugmentedDriftWithEBrake
            {
                get
                {
                    var b = Memory.ReadByte((IntPtr)PlayerAddrs.STATIC_AUGMENTED_DRIFT_WITH_EBRAKE);
                    if (b == 1)
                        return true;
                    return false;
                }
                set
                {
                    if (value)
                        Memory.WriteByte((IntPtr)PlayerAddrs.STATIC_AUGMENTED_DRIFT_WITH_EBRAKE, 1);
                    else Memory.WriteByte((IntPtr)PlayerAddrs.STATIC_AUGMENTED_DRIFT_WITH_EBRAKE, 0);
                }
            }

            /// <summary>
            /// Sets the car's left and right headlights intensity value.
            /// </summary>
            /// <param name="left"></param>
            /// <param name="right"></param>
            public static void SetCarHeadlightsBrightnessIntensityValue(float left, float right)
            {
                Memory.WriteFloat((IntPtr)PlayerAddrs.STATIC_PLAYER_HEADLIGHTS_LEFT, left);
                Memory.WriteFloat((IntPtr)PlayerAddrs.STATIC_PLAYER_HEADLIGHTS_RIGHT, right);
            }

            /// <summary>
            /// Returns the car's left and right headlights intensity value.
            /// </summary>
            /// <returns></returns>
            public static Dictionary<string, float> GetCarHeadlightsBrightnessIntensityValue()
            {
                var dic = new Dictionary<string, float>();


                dic.Add("Left", Memory.ReadFloat((IntPtr)PlayerAddrs.STATIC_PLAYER_HEADLIGHTS_LEFT));
                dic.Add("Right", Memory.ReadFloat((IntPtr)PlayerAddrs.STATIC_PLAYER_HEADLIGHTS_RIGHT));

                return dic;
            }
        }
    }
}
