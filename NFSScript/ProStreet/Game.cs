﻿using System;
using System.IO;
using static NFSScript.Core.GameMemory;
using Addrs = NFSScript.Core.ProStreetAddresses;
using Funcs = NFSScript.ProStreetFunctions;

namespace NFSScript.ProStreet
{
    /// <summary>
    /// A struct that represents the main game manager.
    /// </summary>
    public static class Game
    {
        #region Constant Variables
        private const int SOUND_ID = 0x1;
        private const int AUDIO_STREAMING_ID = 0x2;
        private const int SPEECH_ID = 0x3;
        private const int NIS_AUDIO_ID = 0x4;
        #endregion

        /// <summary>
        /// Returns the currently playing song ID.
        /// </summary>
        public static int CurrentlyPlayingSongID => Memory.ReadInt32((IntPtr)Addrs.GameAddrs.STATIC_SONG_ID);

        /// <summary>
        /// Returns the save game directory path.
        /// </summary>
        public static string SaveDirectory => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "NFS ProStreet");

        /// <summary>
        /// Returns true is the gameplay is active.
        /// </summary>
        public static bool IsGameplayActive
        {
            get
            {
                if (Memory.ReadByte((IntPtr)Addrs.GenericAddrs.STATIC_IS_GAMEPLAY_ACTIVE) == 1)
                    return true;
                return false;
            }
        }

        /// <summary>
        /// Returns true if the career is loaded.
        /// </summary>
        public static bool IsCareerLoaded
        {
            get
            {
                if (Memory.ReadByte((IntPtr)Addrs.GenericAddrs.STATIC_IS_CAREER_LOADED) == 1)
                    return true;
                return false;
            }
        }

        /// <summary>
        /// Is sound enabled?
        /// </summary>
        public static bool IsSoundEnabled
        {
            get => _readAudioIDValue(SOUND_ID);
            set => _setAudioIDValue(SOUND_ID, value);
        }

        /// <summary>
        /// Is audio stream enabled?
        /// </summary>
        public static bool IsAudioStreamingEnabled
        {
            get => _readAudioIDValue(AUDIO_STREAMING_ID);
            set => _setAudioIDValue(AUDIO_STREAMING_ID, value);
        }

        /// <summary>
        /// Is speech enabled?
        /// </summary>
        public static bool IsSpeechEnabled
        {
            get => _readAudioIDValue(SPEECH_ID);
            set => _setAudioIDValue(SPEECH_ID, value);
        }

        /// <summary>
        /// Is NIS audio enabled?
        /// </summary>
        public static bool IsNISAudioEnabled
        {
            get => _readAudioIDValue(NIS_AUDIO_ID);
            set => _setAudioIDValue(NIS_AUDIO_ID, value);
        }

        /// <summary>
        /// Sets or gets a value that indicates whether the movies should be skipped or not.
        /// </summary>
        public static bool SkipMovies
        {
            get => (Memory.ReadByte((IntPtr)Addrs.GameAddrs.STATIC_SKIP_MOVIES) == 1);
            set
            {
                byte b = 0;
                if (value)
                    b = 1;

                Memory.WriteByte((IntPtr)Addrs.GameAddrs.STATIC_SKIP_MOVIES, b);
            }
        }

        /// <summary>
        /// Shows a loading screen.
        /// </summary>
        public static void ShowLoadingScreen()
        {
            Function.Call(Funcs.SHOW_LOADING_SCREEN);
        }

        /// <summary>
        /// Hides a loading screen.
        /// </summary>
        public static void HideLoadingScreen()
        {
            Function.Call(Funcs.HIDE_LOADING_SCREEN);
        }

        /// <summary>
        /// Unlock everything.
        /// </summary>
        public static void UnlockAllThings()
        {
            Memory.WriteByte((IntPtr)Addrs.GameAddrs.STATIC_UNLOCK_ALL_THINGS, 1);
        }

        /// <summary>
        /// Skip the career intro.
        /// </summary>
        public static void SkipCareerIntro()
        {
            Memory.WriteByte((IntPtr)Addrs.GameAddrs.STATIC_SKIP_CAREER_INTRO, 1);
        }

        /// <summary>
        /// Skips the frontend values with a custom set of values. Keep in mind that on every gameplay change (Going to the safehouse) the SkipFE gets disabled and must be recalled again.
        /// </summary>
        /// <param name="enabled"></param>
        public static void SkipFE(bool enabled)
        {
            byte value = 0;
            if (enabled)
                value = 1;

            Memory.WriteByte((IntPtr)Addrs.GameAddrs.STATIC_SKIP_FE, value);
        }

        /// <summary>
        /// Returns a string with a length of 26 which contains the latest entered code (If the player has entered any).
        /// </summary>
        /// <returns></returns>
        public static string GetEnteredCode()
        {
            return Memory.ReadStringASCII((IntPtr)Addrs.GameAddrs.STATIC_ENTERED_CODE, 26);
        }

        internal static void _setAudioIDValue(int id, bool value)
        {
            byte b = 0;
            if (value) b = 1;
            var address = IntPtr.Zero;
            switch (id)
            {
                case SOUND_ID:
                    address = (IntPtr)Addrs.GenericAddrs.STATIC_IS_SOUND_ENABLED;
                    break;

                case AUDIO_STREAMING_ID:
                    address = (IntPtr)Addrs.GenericAddrs.STATIC_IS_AUDIO_STREAMING_ENABLED;
                    break;

                case SPEECH_ID:
                    address = (IntPtr)Addrs.GenericAddrs.STATIC_IS_SPEECH_ENABLED;
                    break;

                case NIS_AUDIO_ID:
                    address = (IntPtr)Addrs.GenericAddrs.STATIC_IS_NISAUDIO_ENABLED;
                    break;

                default:
                    return;
            }

            Memory.WriteByte(address, b);
        }

        internal static bool _readAudioIDValue(int id)
        {
            byte b = 0;
            switch (id)
            {
                case SOUND_ID:
                    b = Memory.ReadByte((IntPtr)Addrs.GenericAddrs.STATIC_IS_SOUND_ENABLED);
                    break;

                case AUDIO_STREAMING_ID:
                    b = Memory.ReadByte((IntPtr)Addrs.GenericAddrs.STATIC_IS_AUDIO_STREAMING_ENABLED);
                    break;

                case SPEECH_ID:
                    b = Memory.ReadByte((IntPtr)Addrs.GenericAddrs.STATIC_IS_SPEECH_ENABLED);
                    break;

                case NIS_AUDIO_ID:
                    b = Memory.ReadByte((IntPtr)Addrs.GenericAddrs.STATIC_IS_NISAUDIO_ENABLED);
                    break;

                default:
                    b = 0;
                    break;
            }

            return (b == 1);
        }
        
    }

    /// <summary>
    /// The game's flow manager class.
    /// </summary>
    public class GameFlowManager
    {
        /// <summary>
        /// The address where the main GameFlowManager is located at.
        /// </summary>
        public static IntPtr Address => (IntPtr)Addrs.GenericAddrs.STATIC_GAME_STATE;

        private int gameStateValue;

        /// <summary>
        /// The main GameFlowManager.
        /// </summary>
        public static GameFlowManager TheGameFlowManager => new GameFlowManager(Memory.ReadInt32(Address));

        /// <summary>
        /// Instantiate a GameFlowManager class.
        /// </summary>
        /// <param name="gameStateValue"></param>
        private GameFlowManager(int gameStateValue)
        {
            this.gameStateValue = gameStateValue;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="instance"></param>
        public static implicit operator int(GameFlowManager instance)
        {
            if (instance == null)
            {
                return -1;
            }
            return instance.gameStateValue;
        }

        /// <summary>
        /// Returns the game state value string.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return gameStateValue.ToString();
        }
    }
}
