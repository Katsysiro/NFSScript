using System;
using NFSScript.Math;
using static NFSScript.Core.GameMemory;
using Addrs = NFSScript.Core.MWAddresses;

namespace NFSScript.MW
{
    /// <summary>
    /// A class that represents a dynamic (physics) game object in the game world.
    /// </summary>
    public class DynamicGameObject : EAGLPhysicsObject
    {
        /// <summary>
        /// Returns the player's car dynamic game object.
        /// </summary>
        public static DynamicGameObject Player => new DynamicGameObject(0);

        private int offset;

        /// <summary>
        /// Returns whether the dynamic game object was applied to an opponent or not.
        /// </summary>
        public bool IsOpponent { get; private set; }

        /// <summary>
        /// Dynamic game object gravity values.
        /// </summary>
        public override Vector3 GravityValues
        {
            get
            {
                var x = Memory.ReadFloat((IntPtr)Addrs.PlayerAddrs.STATIC_PLAYER_POS_X + offset + 30);
                var y = Memory.ReadFloat((IntPtr)Addrs.PlayerAddrs.STATIC_PLAYER_POS_Y + offset + 30);
                var z = Memory.ReadFloat((IntPtr)Addrs.PlayerAddrs.STATIC_PLAYER_POS_Z + offset + 30);

                return new Vector3(x, y, z);
            }
            set
            {
                Memory.WriteFloat((IntPtr)Addrs.PlayerAddrs.STATIC_PLAYER_POS_X + offset + 30, value.X);
                Memory.WriteFloat((IntPtr)Addrs.PlayerAddrs.STATIC_PLAYER_POS_Y + offset + 30, value.Y);
                Memory.WriteFloat((IntPtr)Addrs.PlayerAddrs.STATIC_PLAYER_POS_Z + offset + 30, value.Z);
            }
        }

        /// <summary>
        /// The position of the defined dynamic object ID.
        /// </summary>
        public override Vector3 Position
        {
            get
            {
                var addr = (int)Memory.getBaseAddress;
                var x = Memory.ReadFloat((IntPtr)addr + Addrs.PlayerAddrs.STATIC_PLAYER_POS_X + offset);
                var y = Memory.ReadFloat((IntPtr)addr + Addrs.PlayerAddrs.STATIC_PLAYER_POS_Y + offset);
                var z = Memory.ReadFloat((IntPtr)addr + Addrs.PlayerAddrs.STATIC_PLAYER_POS_Z + offset);

                return new Vector3(x, y, z);
            }
            set
            {
                var addr = (int)Memory.getBaseAddress;
                Memory.WriteFloat((IntPtr)addr + Addrs.PlayerAddrs.STATIC_PLAYER_POS_X + offset, value.X);
                Memory.WriteFloat((IntPtr)addr + Addrs.PlayerAddrs.STATIC_PLAYER_POS_Y + offset, value.Y);
                Memory.WriteFloat((IntPtr)addr + Addrs.PlayerAddrs.STATIC_PLAYER_POS_Z + offset, value.Z);
            }
        }

        /// <summary>
        /// The rotation of the defined dynamic object ID.
        /// </summary>
        public override Quaternion Rotation
        {
            get
            {
                var x = Memory.ReadFloat((IntPtr)Addrs.PlayerAddrs.STATIC_PLAYER_X_ROT + offset);
                var y = Memory.ReadFloat((IntPtr)Addrs.PlayerAddrs.STATIC_PLAYER_Y_ROT + offset);
                var z = Memory.ReadFloat((IntPtr)Addrs.PlayerAddrs.STATIC_PLAYER_Z_ROT + offset);
                var w = Memory.ReadFloat((IntPtr)Addrs.PlayerAddrs.STATIC_PLAYER_W_ROT + offset);

                return new Quaternion(x, y, z, w);
            }
            set
            {
                Memory.WriteFloat((IntPtr)Addrs.PlayerAddrs.STATIC_PLAYER_X_ROT + offset, value.X);
                Memory.WriteFloat((IntPtr)Addrs.PlayerAddrs.STATIC_PLAYER_Y_ROT + offset, value.Y);
                Memory.WriteFloat((IntPtr)Addrs.PlayerAddrs.STATIC_PLAYER_Z_ROT + offset, value.Z);
                Memory.WriteFloat((IntPtr)Addrs.PlayerAddrs.STATIC_PLAYER_W_ROT + offset, value.W);
            }
        }

        /// <summary>
        /// The rotation axis of the dynamic object.
        /// </summary>
        public override Vector3 RotationAxis
        {
            get
            {
                var x = Memory.ReadFloat((IntPtr)Addrs.PlayerAddrs.STATIC_PLAYER_POS_X + offset + 0x20);
                var y = Memory.ReadFloat((IntPtr)Addrs.PlayerAddrs.STATIC_PLAYER_POS_X + offset + 0x24);
                var z = Memory.ReadFloat((IntPtr)Addrs.PlayerAddrs.STATIC_PLAYER_POS_X + offset + 0x28);

                return new Vector3(x, y, z);
            }
            set
            {
                Memory.WriteFloat((IntPtr)Addrs.PlayerAddrs.STATIC_PLAYER_POS_X + offset + 0x20, value.X);
                Memory.WriteFloat((IntPtr)Addrs.PlayerAddrs.STATIC_PLAYER_POS_X + offset + 0x24, value.Y);
                Memory.WriteFloat((IntPtr)Addrs.PlayerAddrs.STATIC_PLAYER_POS_X + offset + 0x28, value.Z);
            }
        }

        /// <summary>
        /// The ID of the dynamic object that the class will effect, an ID bigger than 32 will probably crash the game.
        /// </summary>
        public override byte Id { get; set; }

        /// <summary>
        /// Instantiate a dynamic game object class by ID.
        /// </summary>
        /// <param name="ID">The ID of the car in the world, an ID bigger than 32 will probably crash the game.</param>
        public DynamicGameObject(byte ID)
        {
            Id = ID;
            IsOpponent = false;
            offset = GetOffset(ID);
        }

        /// <summary>
        /// Instantiate a dynamic game object class by ID.
        /// </summary>
        /// <param name="ID">The ID of the car in the world, an ID bigger than 32 will probably crash the game.</param>
        /// <param name="isOpponent">Get the dynamic game object of an opponent.</param>
        public DynamicGameObject(byte ID, bool isOpponent)
        {
            Id = ID;
            IsOpponent = isOpponent;
            offset = GetOffset(ID);
        }

        private int GetOffset(byte ID)
        {
            var offset = 0;
            for (var i = 0; i <= ID; i++)
            {
                if (!IsOpponent)
                    offset = offset + Addrs.GenericAddrs.POINTER_CAR_OFFSET;
                else offset = offset + 0xB0;
            }

            return offset;
        }
    }
}
