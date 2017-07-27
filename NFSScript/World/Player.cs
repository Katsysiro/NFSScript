using NFSScript.Math;
using static NFSScript.Core.GameMemory;
using static NFSScript.Core.WorldAddresses;
using static NFSScript.World.EASharpBindings;

namespace NFSScript.World
{
    /// <summary>
    /// A class that represents the game's <see cref="Player"/>.
    /// </summary>
    public static class Player
    {
        /// <summary>
        /// Returns status of the player.
        /// </summary>
        public static PlayerStatus Status => (PlayerStatus)GenericMemory.Read<int>(GameAddrs.NON_STATIC_PLAYER_STATUS);

        /// <summary>
        /// Returns true if the <see cref="Player"/> is in free roam.
        /// </summary>
        public static bool IsFreeRoam => GenericMemory.Read<byte>(GameAddrs.NON_STATIC_IS_FREE_ROAM) == 1;

        /// <summary>
        /// <see cref="Player"/>'s cash (Inaccurate read only value).
        /// </summary>
        public static int Cash => GenericMemory.Read<int>(PlayerAddrs.NON_STATIC_PLAYER_CASH);

        /// <summary>
        /// <see cref="Player"/>'s boost (Inaccurate read only value).
        /// </summary>
        public static int Boost => GenericMemory.Read<int>(PlayerAddrs.NON_STATIC_PLAYER_BOOST);

        /// <summary>
        /// Returns the currnet amount of gems that the player has.
        /// </summary>
        public static int CurrentAmountOfGems
        {
            get
            {
                var address = GenericMemory.Read<int>(PlayerAddrs.NON_STATIC_GEMS_COLLECTED);
                address = GenericMemory.Read<int>(address + PlayerAddrs.PSTATIC_GEMS_COLLECTED_1, false);
                address = GenericMemory.Read<int>(address + PlayerAddrs.PSTATIC_GEMS_COLLECTED_2, false);
                address = GenericMemory.Read<int>(address + PlayerAddrs.PSTATIC_GEMS_COLLECTED_3, false);
                address = GenericMemory.Read<int>(address + PlayerAddrs.PSTATIC_GEMS_COLLECTED_4, false);
                address = GenericMemory.Read<int>(address + PlayerAddrs.PSTATIC_GEMS_COLLECTED_5, false);

                return GenericMemory.Read<int>(address, false);
            }
        }

        /// <summary>
        /// Changes the in-game auto-drive function state.
        /// </summary>
        /// <remarks>
        /// Requires any kind of directional input in-game in order to apply the change.
        /// </remarks>
        public static void ChangeAutoDrive(bool enableAutoDrive)
        {
            if (enableAutoDrive)
                GenericMemory.Write<byte>(PlayerAddrs.NON_STATIC_AUTODRIVE, 1);
            else
                GenericMemory.Write<byte>(PlayerAddrs.NON_STATIC_AUTODRIVE, 0);
        }

        /// <summary>
        /// A class that represents the <see cref="Player"/>'s car.
        /// </summary>
        public static class Car
        {
            /// <summary>
            /// Gets the local player's memory offset relative to <see cref="Game.PWorld_Objects"/>.
            /// </summary>
            public static int CarOffset
            {
                get
                {
                    for (int i = 0; i < 0x78; i++)
                    {
                        int offset = 0xB0 * i;
                        int addr = Game.PWorld_Objects + offset;

                        // This check can be improved with bytes from offset +0x5C to +0x60.
                        // 0x5D is non-player-checkish, 0x5C is initialized-checkish, check game code for more info.
                        float localX = GenericMemory.Read<float>(PlayerAddrs.NON_STATIC_PLAYER_X_POS);
                        float localY = GenericMemory.Read<float>(PlayerAddrs.NON_STATIC_PLAYER_Y_POS);
                        float localZ = GenericMemory.Read<float>(PlayerAddrs.NON_STATIC_PLAYER_Z_POS);
                        float opponentX = GenericMemory.Read<float>(addr + GameAddrs.PSTATIC_CAR_X_POS, false);
                        float opponentY = -GenericMemory.Read<float>(addr + GameAddrs.PSTATIC_CAR_Y_POS, false);
                        float opponentZ = GenericMemory.Read<float>(addr + GameAddrs.PSTATIC_CAR_Z_POS, false);

                        if (Mathf.Abs(localX - opponentX) < 1f &&
                            Mathf.Abs(localY - opponentY) < 1f &&
                            Mathf.Abs(localZ - opponentZ) < 1f)
                        {
                            return offset;
                        }
                    }
                    return 0xFF;
                }
            }

            /// <summary>
            /// The <see cref="Player"/>'s car position.
            /// </summary>
            public static Vector3 Position
            {
                get
                {
                    int addr = Game.PWorld_Objects + CarOffset;
                    float x = GenericMemory.Read<float>(addr + GameAddrs.PSTATIC_CAR_X_POS, false);
                    float y = GenericMemory.Read<float>(addr + GameAddrs.PSTATIC_CAR_Y_POS, false);
                    float z = GenericMemory.Read<float>(addr + GameAddrs.PSTATIC_CAR_Z_POS, false);

                    return new Vector3(x, y, z);
                }
                set
                {
                    int addr = Game.PWorld_Objects + CarOffset;
                    GenericMemory.Write<float>(addr + GameAddrs.PSTATIC_CAR_X_POS, value.X, false);
                    GenericMemory.Write<float>(addr + GameAddrs.PSTATIC_CAR_Y_POS, value.Y, false);
                    GenericMemory.Write<float>(addr + GameAddrs.PSTATIC_CAR_Z_POS, value.Z, false);
                }
            }

            /// <summary>
            /// The <see cref="Player"/>'s car rotation.
            /// </summary>
            public static Quaternion Rotation
            {
                get
                {
                    int addr = Game.PWorld_Objects + CarOffset;
                    float q1 = GenericMemory.Read<float>(addr + GameAddrs.PSTATIC_CAR_ANGULAR_VELOCITY + 0x5C, false);
                    float q2 = GenericMemory.Read<float>(addr + GameAddrs.PSTATIC_CAR_FACING_SOUTH, false);
                    float q3 = GenericMemory.Read<float>(addr + GameAddrs.PSTATIC_CAR_FACING_UP, false);
                    float q4 = GenericMemory.Read<float>(addr + GameAddrs.PSTATIC_CAR_FACING_EAST, false);

                    return new Quaternion(q1, q2, q3, q4);
                }
            }

            /// <summary>
            /// The <see cref="Player"/>'s car speed in MPS, you need to set it in MPS as well.
            /// </summary>
            /// <remarks>
            /// See <see cref="Math.Mathf.ConvertSpeed(float, SpeedMeasurementConversionTypes)"/>.
            /// </remarks>
            public static float Speed
            {
                get
                {
                    // TODO: Don't add to PlayerAddrs. There's a player base, it might include speed as well.
                    int address = GenericMemory.Read<int>(0x91F9D0) + 0x68;

                    return GenericMemory.Read<float>(address, false);
                }
                set
                {
                    int addr = Game.PWorld_Objects + CarOffset;
                    float southMult = GenericMemory.Read<float>(addr + GameAddrs.PSTATIC_CAR_FACING_SOUTH, false);
                    float vertMult = GenericMemory.Read<float>(addr + GameAddrs.PSTATIC_CAR_FACING_UP, false);
                    float eastMult = GenericMemory.Read<float>(addr + GameAddrs.PSTATIC_CAR_FACING_EAST, false);

                    GenericMemory.Write(addr + GameAddrs.PSTATIC_CAR_VELOCITY_TOWARDS_SOUTH, southMult * value, false);
                    GenericMemory.Write(addr + GameAddrs.PSTATIC_CAR_VERTICAL_VELOCITY, vertMult * value, false);
                    GenericMemory.Write(addr + GameAddrs.PSTATIC_CAR_VELOCITY_TOWARDS_EAST, eastMult * value, false);
                }
            }

            /// <summary>
            /// The <see cref="Player"/>'s car's gravity.
            /// </summary>
            /// <remarks>
            /// Default value is 1000.
            /// </remarks>
            public static float Gravity
            {
                get
                {
                    int addr = Game.PWorld_Objects + CarOffset;
                    return GenericMemory.Read<float>(addr + GameAddrs.PSTATIC_CAR_GRAVITY, false);
                }
                set
                {
                    int addr = Game.PWorld_Objects + CarOffset;
                    GenericMemory.Write<float>(addr + GameAddrs.PSTATIC_CAR_GRAVITY, value, false);
                }
            }

            /// <summary>
            /// The <see cref="Player"/>'s car weight.
            /// </summary>
            /// <remarks>
            /// Each car has its own weight.
            /// </remarks>
            public static float Weight
            {
                get
                {
                    int addr = Game.PWorld_Objects + CarOffset;
                    return GenericMemory.Read<float>(addr + GameAddrs.PSTATIC_CAR_WEIGHT, false);
                }
                set
                {
                    int addr = Game.PWorld_Objects + CarOffset;
                    GenericMemory.Write<float>(addr + GameAddrs.PSTATIC_CAR_WEIGHT, value, false);
                }
            }

            /// <summary>
            /// The <see cref="Player"/>'s car current velocity towards east.
            /// </summary>
            public static float VelocityTowardsEast
            {
                get
                {
                    int addr = Game.PWorld_Objects + CarOffset;
                    return GenericMemory.Read<float>(addr + GameAddrs.PSTATIC_CAR_VELOCITY_TOWARDS_EAST, false);
                }
                set
                {
                    int addr = Game.PWorld_Objects + CarOffset;
                    GenericMemory.Write<float>(addr + GameAddrs.PSTATIC_CAR_VELOCITY_TOWARDS_EAST, value, false);
                }
            }

            /// <summary>
            /// The <see cref="Player"/>'s car current velocity towards south.
            /// </summary>xw
            public static float VelocityTowardsSouth
            {
                get
                {
                    int addr = Game.PWorld_Objects + CarOffset;
                    return GenericMemory.Read<float>(addr + GameAddrs.PSTATIC_CAR_VELOCITY_TOWARDS_SOUTH, false);
                }
                set
                {
                    int addr = Game.PWorld_Objects + CarOffset;
                    GenericMemory.Write<float>(addr + GameAddrs.PSTATIC_CAR_VELOCITY_TOWARDS_SOUTH, value, false);
                }
            }

            /// <summary>
            /// The <see cref="Player"/>'s car vertical velocity towards sky.
            /// </summary>
            public static float VerticalVelocity
            {
                get
                {
                    int addr = Game.PWorld_Objects + CarOffset;
                    return GenericMemory.Read<float>(addr + GameAddrs.PSTATIC_CAR_VERTICAL_VELOCITY, false);
                }
                set
                {
                    int addr = Game.PWorld_Objects + CarOffset;
                    GenericMemory.Write<float>(addr + GameAddrs.PSTATIC_CAR_VERTICAL_VELOCITY, value, false);
                }
            }

            /// <summary>
            /// The <see cref="Player"/>'s car angular velocity towards right.
            /// </summary>
            public static float AngularVelocity
            {
                get
                {
                    int addr = Game.PWorld_Objects + CarOffset;
                    return GenericMemory.Read<float>(addr + GameAddrs.PSTATIC_CAR_ANGULAR_VELOCITY, false);
                }
                set
                {
                    int addr = Game.PWorld_Objects + CarOffset;
                    GenericMemory.Write<float>(addr + GameAddrs.PSTATIC_CAR_ANGULAR_VELOCITY, value, false);
                }
            }

            /// <summary>
            /// Instantly stops the <see cref="Player"/>'s car.
            /// </summary>
            public static void ForceStop()
            {
                VelocityTowardsSouth = 0;
                VelocityTowardsEast = 0;
                VerticalVelocity = 0;
            }

            /// <summary>
            /// Pushes the <see cref="Player"/>'s car to <seealso cref="Directions"/>.
            /// </summary>
            /// <param name="toDirection">Direction to which the car will be pushed.</param>
            /// <param name="amountOfForce">Amount of force applied to the car in meters per second.</param>
            /// <param name="resetWhenSetting">Whether to reset the current value before applying the force.</param>
            public static void Push(Directions toDirection, float amountOfForce, bool resetWhenSetting = false)
            {
                switch (toDirection)
                {
                    case Directions.North:
                        if (resetWhenSetting)
                            VelocityTowardsSouth = 0f;
                        VelocityTowardsSouth -= amountOfForce;
                        break;
                    case Directions.West:
                        if (resetWhenSetting)
                            VelocityTowardsEast = 0f;
                        VelocityTowardsEast -= amountOfForce;
                        break;
                    case Directions.East:
                        if (resetWhenSetting)
                            VelocityTowardsEast = 0f;
                        VelocityTowardsEast += amountOfForce;
                        break;
                    case Directions.South:
                        if (resetWhenSetting)
                            VelocityTowardsSouth = 0f;
                        VelocityTowardsSouth += amountOfForce;
                        break;
                    case Directions.Forwards:
                        if (resetWhenSetting)
                            Speed = 0;
                        Speed += amountOfForce;
                        break;
                    case Directions.Backwards:
                        if (resetWhenSetting)
                            Speed = 0;
                        Speed -= amountOfForce;
                        break;
                }
            }

            /// <summary>
            /// 'Member Mario? I 'member!
            /// </summary>
            public static void Hop()
            {
                VerticalVelocity = 10f;
            }

            /// <summary>
            /// Pushes the <see cref="Player"/>'s car above.
            /// </summary>
            /// <param name="amountOfForce">Amount of force applied to the car in meters per second.</param>
            /// <param name="resetWhenSetting">Whether to reset the current value before applying the force.</param>
            public static void ForceJump(float amountOfForce, bool resetWhenSetting = false)
            {
                if (resetWhenSetting)
                    VerticalVelocity = 0f;
                VerticalVelocity += amountOfForce;
            }

            /// <summary>
            /// Applies a force that turns the <see cref="Player"/>'s car clockwise.
            /// </summary>
            /// <param name="amountOfForce">Amount of force applied to the car in meters per second.</param>
            /// <param name="resetWhenSetting">Whether to reset the current value before applying the force.</param>
            public static void TurnClockwise(float amountOfForce, bool resetWhenSetting = false)
            {
                if (resetWhenSetting)
                    AngularVelocity = 0f;
                AngularVelocity += amountOfForce;
            }

            /// <summary>
            /// Applies a force that turns the <see cref="Player"/>'s car counter-clockwise.
            /// </summary>
            /// <param name="amountOfForce">Amount of force applied to the car in meters per second.</param>
            /// <param name="resetWhenSetting">Whether to reset the current value before applying the force.</param>
            public static void TurnCounterClockwise(float amountOfForce, bool resetWhenSetting = false)
            {
                if (resetWhenSetting)
                    AngularVelocity = 0f;
                AngularVelocity -= amountOfForce;
            }

            /// <summary>
            /// Changes wall collision state.
            /// </summary>
            public static void ChangeWallCollision(bool enableWallCollision)
            {
                if (enableWallCollision)
                    GenericMemory.Write<byte>(PlayerAddrs.NON_STATIC_WALL_COLLISION, 0x84);
                else
                    GenericMemory.Write<byte>(PlayerAddrs.NON_STATIC_WALL_COLLISION, 0x38);
            }

            /// <summary>
            /// Changes car collision state.
            /// </summary>
            public static void ChangeCarCollision(bool enableCarCollision)
            {
                if (enableCarCollision)
                    GenericMemory.Write<byte>(PlayerAddrs.NON_STATIC_CAR_COLLISION, 0x74);
                else
                    GenericMemory.Write<byte>(PlayerAddrs.NON_STATIC_CAR_COLLISION, 0xEB);
            }
        }
    }

    /// <summary>
    /// A class for powerups.
    /// </summary>
    public static class Powerups
    {
        /// <summary>
        /// Changes the current powerup configuration/deck.
        /// </summary>
        /// <param name="config"></param>
        public static void AssignPowerupConfiguration(PowerupConfiguration config)
        {
            CallBinding(_EASharpBinding_637, (int)config);
        }

        /// <summary>
        /// Recharges all the powerups.
        /// </summary>
        public static void RechargeAllPowerups()
        {
            CallBinding(_EASharpBinding_638);
        }

        /// <summary>
        /// Changes powerup cooldown init-state.
        /// </summary>
        public static void ChangePowerupCooldown(bool enablePowerupCooldown, bool refreshCurrentCooldowns = false)
        {
            if (refreshCurrentCooldowns)
                RechargeAllPowerups();

            if (enablePowerupCooldown)
                GenericMemory.WriteByteArray(PlayerAddrs.NON_STATIC_POWERUP_COOLDOWN, new byte[] { 0x80, 0x7D, 0xFB, 0x0 }, true);
            else
                GenericMemory.WriteByteArray(PlayerAddrs.NON_STATIC_POWERUP_COOLDOWN, new byte[] { 0x3A, 0xC0, 0x90, 0x90 }, true);
        }
    }
}
