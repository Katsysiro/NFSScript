using System;
using System.Linq;
using System.Text;
using NFSScript.Core;
using static NFSScript.Core.GameMemory;
using Addrs = NFSScript.Core.MWAddresses;

namespace NFSScript.MW
{
    /// <summary>
    /// A class representing the <see cref="Network"/> in the game.
    /// </summary>
    public static class Network
    {
        private const int NETWORK_LOBBY_IP_LENGTH = 32;

        /// <summary>
        /// The network's lobby IP (A string with a length of 32).
        /// </summary>
        public static string LobbyIP
        {
            get => _readNetworkLobbyIP();
            set => _setNetworkLobbyIP(value);
        }

        /// <summary>
        /// The network's lobby port.
        /// </summary>
        public static ushort LobbyPort
        {
            get => _readNetworkLobbyPort();
            set => _setNetworkLobbyPort(value);
        }

        /// <summary>
        /// A value that indicates whether the network debug is enabled or not.
        /// </summary>
        public static bool Debug
        {
            get => _readIsNetworkDebug();
            set => _setIsNetworkDebug(value);
        }

        internal static bool _readIsNetworkDebug()
        {
            return Memory.ReadByte((IntPtr)Addrs.NetworkAddrs.STATIC_NETWORK_DEBUG) == 1;
        }

        internal static void _setIsNetworkDebug(bool value)
        {
            byte b = 0;
            if (value)
                b = 1;

            Memory.WriteByte((IntPtr)Addrs.NetworkAddrs.STATIC_NETWORK_DEBUG, b);
        }

        internal static ushort _readNetworkLobbyPort()
        {
            return Memory.ReadUShort((IntPtr)Addrs.NetworkAddrs.STATIC_NETWORK_LOBBY_PORT);
        }

        internal static void _setNetworkLobbyPort(ushort port)
        {
            Memory.WriteUShort((IntPtr)Addrs.NetworkAddrs.STATIC_NETWORK_LOBBY_PORT, port);
        }

        internal static string _readNetworkLobbyIP()
        {
            var address = (IntPtr)Addrs.NetworkAddrs.STATIC_NETWORK_LOBBY_IP;
            return Memory.ReadStringASCII(address, NETWORK_LOBBY_IP_LENGTH);
        }

        internal static void _setNetworkLobbyIP(string ip)
        {
            var address = (IntPtr)Addrs.NetworkAddrs.STATIC_NETWORK_LOBBY_IP;
            ASM.Abolish(address, NETWORK_LOBBY_IP_LENGTH);
            Memory.WriteStringASCII(address, ip.Substring(0, NETWORK_LOBBY_IP_LENGTH));
        }

        /// <summary>
        /// A class for the <see cref="Network"/>'s <see cref="GameRoom"/>.
        /// </summary>
        public static class GameRoom
        {
            /// <summary>
            /// Returns an array of players in the current game room.
            /// </summary>
            public static Player[] Players => _readPlayers();

            internal static Player[] _readPlayers()
            {
                var players = new Player[4];
                players[0] = new Player(0);
                players[1] = new Player(1);
                players[2] = new Player(2);
                players[3] = new Player(3);

                return players;
            }

            /// <summary>
            /// A class for player's own host game.
            /// </summary>
            public static class HostGame
            {
                /// <summary>
                /// The minimum amount of laps in the player's own host game.
                /// </summary>
                public static byte MinimumLaps { get => _readMinLaps();
                    set => _setMinLaps(value);
                }

                /// <summary>
                /// The maximum amount of laps in the player's own host game.
                /// </summary>
                public static byte MaximumLaps { get => _readMaxLaps();
                    set => _setMaxLaps(value);
                }

                internal static byte _readMaxLaps()
                {
                    return Memory.ReadByte((IntPtr)MWAddresses.NetworkAddrs.STATIC_NETWORK_HOST_MAX_LAPS_1);
                }

                internal static byte _readMinLaps()
                {
                    return Memory.ReadByte((IntPtr)MWAddresses.NetworkAddrs.STATIC_NETWORK_HOST_MIN_LAPS_1);
                }

                internal static void _setMaxLaps(byte laps)
                {
                    Memory.WriteByte((IntPtr)MWAddresses.NetworkAddrs.STATIC_NETWORK_HOST_MAX_LAPS_1, laps);
                    Memory.WriteByte((IntPtr)MWAddresses.NetworkAddrs.STATIC_NETWORK_HOST_MAX_LAPS_2, laps);
                }

                internal static void _setMinLaps(byte laps)
                {
                    Memory.WriteByte((IntPtr)MWAddresses.NetworkAddrs.STATIC_NETWORK_HOST_MIN_LAPS_1, laps);
                    Memory.WriteByte((IntPtr)MWAddresses.NetworkAddrs.STATIC_NETWORK_HOST_MIN_LAPS_2, laps);
                }
            }
        }

        /// <summary>
        /// A class representing a <see cref="Player"/> in the game's <see cref="Network"/>.
        /// </summary>
        public class Player
        {
            /// <summary>
            /// The ID of the <see cref="Player"/>.
            /// </summary>
            public int ID { get; private set; }

            /// <summary>
            /// The name of the <see cref="Player"/>.
            /// </summary>
            public string Name => _readName();

            /// <summary>
            /// The unique ID of the <see cref="Player"/>.
            /// </summary>
            public string UID => _readUID();

            /// <summary>
            /// Instantiate a <see cref="Network"/> <see cref="Player"/> class by ID.
            /// </summary>
            /// <param name="ID"></param>
            public Player(int ID)
            {
                this.ID = ID;
            }

            /// <summary>
            /// Returns a nicely formmated <see cref="Player"/> string.
            /// </summary>
            /// <returns></returns>
            public override string ToString()
            {
                return $"ID: {ID} Player: {Name} UID: {UID}";
            }

            internal string _readName()
            {
                var offset = 0;
                for (var i = 0; i < ID; i++)
                {
                    offset = offset + MWAddresses.NetworkAddrs.STATIC_OFFSET_GAME_ROOM_PLAYER;
                }
                return Encoding.ASCII.GetString(Memory.ReadByteArray((IntPtr)MWAddresses.NetworkAddrs.STATIC_NETWORK_GAME_ROOM_PLAYER + offset, 12).Where(x => x != 0x00).ToArray());
            }

            internal string _readUID()
            {
                var offset = 0;
                for (var i = 0; i < ID; i++)
                {
                    offset = offset + MWAddresses.NetworkAddrs.STATIC_OFFSET_GAME_ROOM_PLAYER;
                }
                return Memory.ReadStringASCII((IntPtr)MWAddresses.NetworkAddrs.STATIC_NETWORK_GAME_ROOM_PLAYER + MWAddresses.NetworkAddrs.STATIC_OFFSET_GAME_ROOM_PLAYER_UID
                    + offset, 9).Replace(" ", string.Empty);
            }
        }
    }
}
