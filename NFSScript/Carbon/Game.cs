﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NFSScript.Math;
using static NFSScript.Core.GameMemory;
using static NFSScript.Core.CarbonAddresses;
using static NFSScript.CarbonFunctions;

namespace NFSScript.Carbon
{
    /// <summary>
    /// A class that represents the main game manager.
    /// </summary>
    public static class Game
    {
        #region Constant Variables
        private const int SOUND_ID = 0x1;
        private const int AUDIO_STREAMING_ID = 0x2;
        private const int SPEECH_ID = 0x3;
        private const int NIS_AUDIO_ID = 0x4;

        private const int DRAW_RACE_CAR_ADDR = 0x1;
        private const int DRAW_HELI_ADDR = 0x2;
        private const int DRAW_COP_CAR_ADDR = 0x3;
        private const int DRAW_TRAFFIC_ADDR = 0x4;
        private const int DRAW_NIS_CAR_ADDR = 0x5;

        /// <summary>
        /// The ID for the DrawRaceCar constructor pointer.
        /// </summary>
        public const int DRAW_RACE_CAR_POINTER_ID = 0x1;

        /// <summary>
        /// The ID for DrawHeli constructor pointer.
        /// </summary>
        public const int DRAW_HELI_POINTER_ID = 0x2;

        /// <summary>
        /// The ID for DrawCopCar constructor pointer.
        /// </summary>
        public const int DRAW_COP_CAR_POINTER_ID = 0x3;

        /// <summary>
        /// The ID for DrawTraffic constructor pointer.
        /// </summary>
        public const int DRAW_TRAFFIC_POINTER_ID = 0x4;

        /// <summary>
        /// The ID for DrawNISCar constructor pointer.
        /// </summary>
        public const int DRAW_NIS_CAR_POINTER_ID = 0x5;
        #endregion

        private static float sirensIntensityR = 1, sirensIntensityB = 1, sirensIntensityW = 1;

        /// <summary>
        /// Returns the save game directory path.
        /// </summary>
        public static string SaveDirectory => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "NFS Carbon");

        /// <summary>
        /// Returns a value that indicates whether the sirens are enabled in or not.
        /// </summary>
        public static bool SirensEnabled { get; private set; }

        /// <summary>
        /// Returns the game's run time in seconds.
        /// </summary>
        public static float RunTime => Memory.ReadFloat((IntPtr)GenericAddrs.STATIC_RUN_TIME);

        /// <summary>
        /// The scale at which the gameplay's time is passing. (Not the global time scale)
        /// </summary>
        public static float GameSpeed
        {
            get => Memory.ReadFloat((IntPtr)GameAddrs.STATIC_GAME_SPEED);
            set => Memory.WriteFloat((IntPtr)GameAddrs.STATIC_GAME_SPEED, value);
        }

        /// <summary>
        /// Returns the amount of seconds it takes to render a frame. (Might be inaccurate)
        /// </summary>
        public static float LastFrameTime => Memory.ReadFloat((IntPtr)GenericAddrs.STATIC_LAST_FRAME_TIME);

        /// <summary>
        /// Returns true if the gameplay is currently active.
        /// </summary>
        public static bool IsGameplayActive => Memory.ReadByte((IntPtr)GenericAddrs.STATIC_IS_GAMEPLAY_ACTIVE) == 1;

        /// <summary>
        /// Returns true if there is an ongoing activity/race.
        /// </summary>
        public static bool IsActivityActive => Memory.ReadByte((IntPtr)GameAddrs.STATIC_IS_ACTIVITY_MODE) == 1;

        /// <summary>
        /// Returns the current game resolution.
        /// </summary>
        public static Point VideoResolution
        {
            get
            {
                var x = Memory.ReadInt32((IntPtr)GenericAddrs.STATIC_CURRENT_RES_X);
                var y = Memory.ReadInt32((IntPtr)GenericAddrs.STATIC_CURRENT_RES_Y);

                return new Point(x, y);
            }
        }

        /// <summary>
        /// Returns true if the mouse button is currently being held.
        /// </summary>
        public static bool IsMouseButtonHeld => Memory.ReadInt32((IntPtr)GenericAddrs.STATIC_IS_MOUSE_HELD) == 1;

        /// <summary>
        /// Returns true if the game camera moment is shown.
        /// </summary>
        public static bool IsGameMoment => (Memory.ReadInt32((IntPtr)GameAddrs.STATIC_IS_GAME_MOMENT)) == 1;

        /// <summary>
        /// Game paused value.
        /// </summary>
        public static bool IsPaused
        {
            get => (Memory.ReadInt32((IntPtr)GameAddrs.STATIC_IS_PAUSED)) == 1;
            set => Memory.WriteInt32((IntPtr)GameAddrs.STATIC_IS_PAUSED, value.ToByte());
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
        /// Is memcard versioning enabled?
        /// </summary>
        public static bool IsMemcardVersioningEnabled => Memory.ReadBoolean((IntPtr)GenericAddrs.STATIC_IS_MEMCARD_VERSIONING_ENABLED);

        /// <summary>
        /// Returns a boolean that indicates whether it's the collectors edition of the game or not.
        /// </summary>
        public static bool IsCollectorsEdition => Memory.ReadBoolean((IntPtr)GenericAddrs.STATIC_IS_COLLECTORS_EDITION);

        /// <summary>
        /// Returns the current active crew member ID.
        /// </summary>
        public static int CurrentActiveCrewMemberID => Memory.ReadInt32((IntPtr)GameAddrs.STATIC_CURRENT_ACTIVE_CREW_MEMBER_ID);

        /// <summary>
        /// Returns the active crew member ID from the change current active crew member screen.
        /// </summary>
        public static int CurrentChangeActiveCrewMemberCrewMemberID => Memory.ReadInt32((IntPtr)GameAddrs.STATIC_CURRENT_CHANGE_ACTIVE_CREW_MEMBER_CURRENT_ID);

        /// <summary>
        /// Returns the total amount of cops in pursuit.
        /// </summary>
        public static int TotalCopsInPursuit
        {
            get
            {
                var addr = Memory.ReadInt32((IntPtr)Memory.getBaseAddress + 0x00683EA0);
                return Memory.ReadInt32((IntPtr)addr + 0x194);
            }
        }

        /// <summary>
        /// Returns the storyline progression.
        /// </summary>
        /// <returns></returns>
        public static int GetStorylineProgression()
        {
            return (int)Function.Call<int>(GET_STORY_LINE_PROGRESSION);
        }

        /// <summary>
        /// Unlock everything.
        /// </summary>
        public static void UnlockAllThings()
        {
            Memory.WriteByte((IntPtr)GameAddrs.STATIC_UNLOCK_ALL_THINGS, 1);
        }

        /// <summary>
        /// Skip the career intro.
        /// </summary>
        public static void SkipCareerIntro()
        {
            Memory.WriteByte((IntPtr)GameAddrs.STATIC_SKIP_CAREER_INTRO, 1);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        public static void EnableRightStickInFrontend(bool value)
        {
            Memory.WriteBoolean((IntPtr)GenericAddrs.STATIC_ENABLE_RIGHT_STICK_IN_FRONTEND, value);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        public static void GDebugEventStrings(bool value)
        {
            Memory.WriteBoolean((IntPtr)GenericAddrs.STATIC_GDEBUG_EVENT_STRINGS, value);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        public static void BRumbleEnabled(bool value)
        {
            Memory.WriteBoolean((IntPtr)GenericAddrs.STATIC_B_RUMBLE_ENABLED, value);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        public static void PrecipitationEnable(bool value)
        {
            Memory.WriteBoolean((IntPtr)GenericAddrs.STATIC_PRECIPITATION_ENABLE, value);
        }

        /// <summary>
        /// Enable debug car customize.
        /// </summary>
        /// <param name="value"></param>
        public static void EnableDebugCarCustomize(bool value)
        {
            Memory.WriteBoolean((IntPtr)GameAddrs.STATIC_ENABLE_DEBUG_CAR_CUSTOMIZE, value);
        }

        /// <summary>
        /// Changes to the next untuned preset in a specific order on debug car customize.
        /// </summary>
        public static void DebugVehicleSelection()
        {
            Memory.WriteByte((IntPtr)GameAddrs.STATIC_DEBUG_VEHICLE_SELECTION, 1);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        public static void DoScreenPrintf(bool value)
        {
            Memory.WriteBoolean((IntPtr)GenericAddrs.STATIC_DO_SCREEN_PRINTF, value);
        }

        /// <summary>
        /// Calls the DrawHeli constructor for every set ID present in the game's world.
        /// </summary>
        /// <param name="id">The constructor ID.</param>
        public static void DrawHeli(int id)
        {
            _setConstructorRaceCarPointerValue(id, DRAW_HELI_POINTER_ID);
        }

        /// <summary>
        /// Calls the DrawRaceCar constructor for every set ID present in the world.
        /// </summary>
        /// <param name="id">The constructor ID.</param>
        public static void DrawRaceCar(int id)
        {
            _setConstructorRaceCarPointerValue(id, DRAW_RACE_CAR_POINTER_ID);
        }

        /// <summary>
        /// Calls the DrawCopCar constructor for every set ID present in the world.
        /// </summary>
        /// <param name="id">The constructor ID.</param>
        public static void DrawCopCar(int id)
        {
            _setConstructorRaceCarPointerValue(id, DRAW_COP_CAR_POINTER_ID);
        }

        /// <summary>
        /// Calls the DrawNISCar constructor for every set ID present in the world.
        /// </summary>
        /// <param name="id">The constructor ID.</param>
        public static void DrawNISCar(int id)
        {
            _setConstructorRaceCarPointerValue(id, DRAW_NIS_CAR_POINTER_ID);
        }

        /// <summary>
        /// Calls the DrawTraffic constructor for every set ID present in the world.
        /// </summary>
        /// <param name="id">The constructor ID.</param>
        public static void DrawTraffic(int id)
        {
            _setConstructorRaceCarPointerValue(id, DRAW_TRAFFIC_POINTER_ID);

        }

        /// <summary>
        /// Should the game skip movies?
        /// </summary>
        /// <param name="value"></param>
        public static void SkipMovies(bool value)
        {
            Memory.WriteBoolean((IntPtr)GenericAddrs.STATIC_SKIP_MOVIES, value);
        }

        /// <summary>
        /// Enable the police sirens for the cars present in the world.
        /// </summary>
        public static void EnablePoliceSirens()
        {
            _setPoliceSirensIntensity(sirensIntensityR, sirensIntensityB, sirensIntensityW);
            SirensEnabled = true;
        }

        /// <summary>
        /// Enable the police sirens for the cars present in the world.
        /// </summary>
        public static void DisablePoliceSirens()
        {
            _setPoliceSirensIntensity(0, 0, 0);
            SirensEnabled = false;
        }
        
        /// <summary>
        /// Sets a value for the game world's heat
        /// </summary>
        /// <param name="heat"></param>
        public static void SetWorldHeat(float heat)
        {
            var addr = Memory.ReadUInteger((IntPtr)PlayerAddrs.PSTATIC_PLAYER_HEAT_LEVEL);
            Memory.WriteFloat((IntPtr)addr + PlayerAddrs.POINTER_PLAYER_HEAT_LEVEL, heat - 0.001f);
        }

        /// <summary>
        /// Returns the game world's heat
        /// </summary>
        /// <returns></returns>
        public static float GetWorldHeat()
        {
            var addr = Memory.ReadUInteger((IntPtr)PlayerAddrs.PSTATIC_PLAYER_HEAT_LEVEL);
            return Memory.ReadFloat((IntPtr)addr + PlayerAddrs.POINTER_PLAYER_HEAT_LEVEL);
        }

        /// <summary>
        /// Set a value that decides whether cops are enabled in the game or not.
        /// </summary>
        /// <param name="isEnabled"></param>
        public static void SetCopsEnabled(bool isEnabled)
        {
            Function.Call(SET_COPS_ENABLED, isEnabled);
        }

        /// <summary>
        /// Bails the current pursuit.
        /// </summary>
        public static void BailPursuit()
        {
            Function.Call(BAIL_PURSUIT);
        }

        /// <summary>
        /// Forces a pursuit to start.
        /// </summary>
        public static void ForcePursuitStart()
        {
            Function.Call(FORCE_PURSUIT_START);
        }

        /// <summary>
        /// No more new pursuits or cops.
        /// </summary>
        public static void NoNewPursuitsOrCops()
        {
            Function.Call(NO_NEW_PURSUITS_OR_COPS);
        }

        /// <summary>
        /// Abandons the race.
        /// </summary>
        public static void AbandonRace()
        {
            Function.Call(ABANDON_RACE);
        }

        /// <summary>
        /// Shakes the camera.
        /// </summary>
        public static void CameraShake()
        {
            Function.Call(CAMERA_SHAKE);
        }

        /// <summary>
        /// Starts a race from in-game.
        /// </summary>
        /// <param name="raceID">The race ID to start.</param>
        public static void StartRaceFromInGame(string raceID)
        {
            Function.Call(START_RACE_FROM_IN_GAME, raceID);
        }

        /// <summary>
        /// Jumps to a safehouse.
        /// </summary>
        public static void JumpToSafeHouse()
        {
            Function.Call(JUMP_TO_SAFE_HOUSE);
        }

        /// <summary>
        /// Jumps to a car lot.
        /// </summary>
        public static void JumpToCarLot()
        {
            Function.Call(JUMP_TO_CAR_LOT);
        }

        /// <summary>
        /// Jumps to a new wingman.
        /// </summary>
        public static void JumpToNewWingman()
        {
            Function.Call(JUMP_TO_NEW_WING_MAN);
        }

        /// <summary>
        /// Activates drift mode.
        /// </summary>
        public static void ActivateDriftMode()
        {
            Function.Call(ACTIVATE_DRIFT_MODE);
        }

        /// <summary>
        /// Deactivates drift mode.
        /// </summary>
        public static void DeactivateDriftMode()
        {
            Function.Call(DEACTIVATE_DRIFT_MODE);
        }

        /// <summary>
        /// Set the police lights intensity for the cars present in the world.
        /// </summary>
        /// <param name="red"></param>
        /// <param name="blue"></param>
        /// <param name="white"></param>
        public static void SetPoliceLightsIntensity(float red, float blue, float white)
        {
            sirensIntensityR = red;
            sirensIntensityB = blue;
            sirensIntensityW = white;

            if (SirensEnabled)
            {
                _setPoliceSirensIntensity(red, blue, white);
            }
        }

        /// <summary>
        /// Skips the frontend values with a custom set of values. Keep in mind that on every gameplay change (Going to the safehouse) the SkipFE gets disabled and must be recalled again.
        /// </summary>
        /// <param name="enabled"></param>
        public static void SkipFE(bool enabled)
        {
            Memory.WriteBoolean((IntPtr)GameAddrs.STATIC_SKIP_FE_ENABLED, enabled);
        }

        /// <summary>
        /// Returns the police lights intensity value.
        /// </summary>
        /// <returns></returns>
        public static Dictionary<string, float> GetCurrentPoliceLightsIntensity()
        {
            var dic = new Dictionary<string, float>();

            dic.Add("Redlights", Memory.ReadFloat((IntPtr)GameAddrs.STATIC_GLOBAL_COP_LIGHTS_RED));
            dic.Add("Bluelights", Memory.ReadFloat((IntPtr)GameAddrs.STATIC_GLOBAL_COP_LIGHTS_BLUE));
            dic.Add("Whitelights", Memory.ReadFloat((IntPtr)GameAddrs.STATIC_GLOBAL_COP_LIGHTS_WHITE));

            return dic;
        }

        internal static void _setPoliceSirensIntensity(float r, float b, float w)
        {
            Memory.WriteFloat((IntPtr)GameAddrs.STATIC_GLOBAL_COP_LIGHTS_RED, r);
            Memory.WriteFloat((IntPtr)GameAddrs.STATIC_GLOBAL_COP_LIGHTS_BLUE, b);
            Memory.WriteFloat((IntPtr)GameAddrs.STATIC_GLOBAL_COP_LIGHTS_WHITE, w);
        }

        internal static void _setAudioIDValue(int id, bool value)
        {
            byte b = 0;
            if(value) b = 1;
            var address = IntPtr.Zero;
            switch (id)
            {
                case SOUND_ID:
                    address = (IntPtr)GenericAddrs.STATIC_IS_SOUND_ENABLED;
                    break;

                case AUDIO_STREAMING_ID:
                    address = (IntPtr)GenericAddrs.STATIC_IS_AUDIO_STREAMING_ENABLED;
                    break;

                case SPEECH_ID:
                    address = (IntPtr)GenericAddrs.STATIC_IS_SPEECH_ENABLED;
                    break;

                case NIS_AUDIO_ID:
                    address = (IntPtr)GenericAddrs.STATIC_IS_NIS_AUDIO_ENABLED;
                    break;

                default:
                    return;
            }

            Memory.WriteByte(address, b);
        }

        internal static void _setConstructorRaceCarPointerValue(int id, int pointer)
        {
            var p = 0;
            switch (pointer)
            {
                case DRAW_RACE_CAR_POINTER_ID:
                    p = GameAddrs.STATIC_CONSTRUCTOR_DRAW_RACE_CAR;
                    break;

                case DRAW_HELI_POINTER_ID:
                    p = GameAddrs.STATIC_CONSTRUCTOR_DRAW_HELI;
                    break;

                case DRAW_COP_CAR_POINTER_ID:
                    p = GameAddrs.STATIC_CONSTRUCTOR_DRAW_COP_CAR;
                    break;

                case DRAW_TRAFFIC_POINTER_ID:
                    p = GameAddrs.STATIC_CONSTRUCTOR_DRAW_TRAFFIC;
                    break;

                case DRAW_NIS_CAR_POINTER_ID:
                    p = GameAddrs.STATIC_CONSTRUCTOR_DRAW_NIS_CAR;
                    break;
            }

            var addr = IntPtr.Zero;
            switch (id)
            {
                case DRAW_RACE_CAR_ADDR:
                    addr = (IntPtr)GameAddrs.STATIC_CONSTRUCTOR_POINTER_CALL_DRAW_RACER_CAR;
                    break;

                case DRAW_HELI_ADDR:
                    addr = (IntPtr)GameAddrs.STATIC_CONSTRUCTOR_POINTER_CALL_DRAW_HELI;
                    break;

                case DRAW_COP_CAR_ADDR:
                    addr = (IntPtr)GameAddrs.STATIC_CONSTRUCTOR_POINTER_CALL_DRAW_COP_CAR;
                    break;

                case DRAW_TRAFFIC_ADDR:
                    addr = (IntPtr)GameAddrs.STATIC_CONSTRUCTOR_POINTER_CALL_DRAW_TRAFFIC;
                    break;

                case DRAW_NIS_CAR_ADDR:
                    addr = (IntPtr)GameAddrs.STATIC_CONSTRUCTOR_DRAW_NIS_CAR;
                    break;
            }

            Memory.WriteInt32(addr, p);
        }

        internal static bool _readAudioIDValue(int id)
        {
            byte b = 0;
            switch (id)
            {
                case SOUND_ID:
                    b = Memory.ReadByte((IntPtr)GenericAddrs.STATIC_IS_SOUND_ENABLED);
                    break;

                case AUDIO_STREAMING_ID:
                    b = Memory.ReadByte((IntPtr)GenericAddrs.STATIC_IS_AUDIO_STREAMING_ENABLED);
                    break;

                case SPEECH_ID:
                    b = Memory.ReadByte((IntPtr)GenericAddrs.STATIC_IS_SPEECH_ENABLED);
                    break;

                case NIS_AUDIO_ID:
                    b = Memory.ReadByte((IntPtr)GenericAddrs.STATIC_IS_NIS_AUDIO_ENABLED);
                    break;

                default:
                    b = 0;
                    break;
            }

            return (b == 1);
        }

        /// <summary>
        /// A class that represents the game's world
        /// </summary>
        public static class World
        {
            /// <summary>
            /// World's speed modifier for animations (like waving trees) on the game's world. (Default value is 45.0)
            /// </summary>
            public static float AnimationSpeed
            {
                get => Memory.ReadFloat((IntPtr)WorldAddrs.STATIC_ANIMATION_SPEED);
                set => Memory.WriteFloat((IntPtr)WorldAddrs.STATIC_ANIMATION_SPEED, value);
            }

            /// <summary>
            /// Sets a value that indicates whether it should always rain.
            /// </summary>
            public static bool AlwaysRain
            {
                get
                {
                    var b = Memory.ReadByte((IntPtr)WorldAddrs.STATIC_ALWAYS_RAIN);
                    if (b == 1)
                        return true;
                    return false;
                }
                set
                {
                    if (value)
                    {
                        Memory.WriteByte((IntPtr)WorldAddrs.STATIC_ALWAYS_RAIN, 1);
                    }
                    else
                    {
                        Memory.WriteByte((IntPtr)WorldAddrs.STATIC_ALWAYS_RAIN, 0);
                    }
                }
            }

            /// <summary>
            /// Game world cars current scale.
            /// </summary>
            public static Vector3 CarsScale
            {
                get
                {
                    var x = Memory.ReadFloat((IntPtr)WorldAddrs.STATIC_GLOBAL_CAR_SCALE_X);
                    var y = Memory.ReadFloat((IntPtr)WorldAddrs.STATIC_GLOBAL_CAR_SCALE_Y);
                    var z = Memory.ReadFloat((IntPtr)WorldAddrs.STATIC_GLOBAL_CAR_SCALE_Z);

                    return new Vector3(x, y, z);
                }
                set
                {
                    Memory.WriteFloat((IntPtr)WorldAddrs.STATIC_GLOBAL_CAR_SCALE_X, value.X);
                    Memory.WriteFloat((IntPtr)WorldAddrs.STATIC_GLOBAL_CAR_SCALE_Y, value.Y);
                    Memory.WriteFloat((IntPtr)WorldAddrs.STATIC_GLOBAL_CAR_SCALE_Z, value.Z);
                }
            }

            /// <summary>
            /// Enable fog.
            /// </summary>
            public static void EnableFog()
            {
                Memory.WriteByte((IntPtr)WorldAddrs.STATIC_FOG, 1);
            }

            /// <summary>
            /// Disable fog.
            /// </summary>
            public static void DisableFog()
            {
                Memory.WriteByte((IntPtr)WorldAddrs.STATIC_FOG, 0);
            }

            /// <summary>
            /// Resets all the props in the <see cref="World"/>.
            /// </summary>
            public static void ResetProps()
            {
                Function.Call(RESET_PROPS);
            }

            /// <summary>
            /// Sets a value that indicates whether rainy road reflections are enabled.
            /// </summary>
            public static bool RainyRoadReflections
            {
                get
                {
                    var b = Memory.ReadByte((IntPtr)WorldAddrs.STATIC_RAIN_ROAD_REFLECTION);
                    if (b == 1)
                        return true;
                    return false;
                }
                set
                {
                    if (value)
                        Memory.WriteByte((IntPtr)WorldAddrs.STATIC_RAIN_ROAD_REFLECTION, 1);
                    else Memory.WriteByte((IntPtr)WorldAddrs.STATIC_RAIN_ROAD_REFLECTION, 0);
                }
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="value"></param>
            public static void TimeOfDaySwapEnable(bool value)
            {
                byte b = 0;
                if(value) b = 1;

                Memory.WriteByte((IntPtr)WorldAddrs.STATIC_TIME_OF_DAY_SWAP_ENABLE, b);
            }

            /// <summary>
            /// Sets the world's current rain properties
            /// </summary>
            /// <param name="rainSize"></param>
            /// <param name="intensity"></param>
            /// <param name="crossing"></param>
            /// <param name="fallSpeed"></param>
            /// <param name="gravity"></param>
            public static void SetRainProperties(float rainSize, float intensity, float crossing, float fallSpeed, float gravity)
            {
                Memory.WriteFloat((IntPtr)WorldAddrs.STATIC_RAIN_SIZE, rainSize);
                Memory.WriteFloat((IntPtr)WorldAddrs.STATIC_RAIN_INTENSITY, intensity);
                Memory.WriteFloat((IntPtr)WorldAddrs.STATIC_RAIN_XING, crossing);
                Memory.WriteFloat((IntPtr)WorldAddrs.STATIC_RAIN_FALL_SPEED, fallSpeed);
                Memory.WriteFloat((IntPtr)WorldAddrs.STATIC_RAIN_GRAVITY, gravity);
            }

            /// <summary>
            /// Returns the world's current rain properties.
            /// </summary>
            /// <returns></returns>
            public static Dictionary<string, float> GetRainProperties()
            {
                var dic = new Dictionary<string, float>();

                dic.Add("RainSize", Memory.ReadFloat((IntPtr)WorldAddrs.STATIC_RAIN_SIZE));
                dic.Add("Intensity", Memory.ReadFloat((IntPtr)WorldAddrs.STATIC_RAIN_INTENSITY));
                dic.Add("Crossing", Memory.ReadFloat((IntPtr)WorldAddrs.STATIC_RAIN_XING));
                dic.Add("FallSpeed", Memory.ReadFloat((IntPtr)WorldAddrs.STATIC_RAIN_FALL_SPEED));
                dic.Add("Gravity", Memory.ReadFloat((IntPtr)WorldAddrs.STATIC_RAIN_GRAVITY));

                return dic;
            }
        }

        /// <summary>
        /// A class that represents the game's race manager
        /// </summary>
        public static class RacesManager
        {
            /// <summary>
            /// Returns the current activity ID.
            /// </summary>
            /// <returns></returns>
            public static string CurrentActivityID
            {
                get
                {
                    return Encoding.Default.GetString(Memory.ReadByteArray((IntPtr)GameAddrs.STATIC_ACTIVITY_ID, 106).Where(b => b != 0x00).ToArray());
                }
            }

            /// <summary>
            /// Maximum Drift Multiplier for Track (Circuit) Drifts. (Default value is 10)
            /// </summary>
            public static byte MaximumTrackDriftMultiplier
            {
                get => Memory.ReadByte((IntPtr)RaceAddrs.STATIC_MAX_DRIFT_MULTIPLIER_TRACK);
                set => Memory.WriteByte((IntPtr)RaceAddrs.STATIC_MAX_DRIFT_MULTIPLIER_TRACK, value);
            }

            /// <summary>
            /// Maximum Drift Multiplier for Canyon (Sprint) Drifts. (Default value is 20)
            /// </summary>
            public static byte MaximumCanyonDriftMultiplier
            {
                get => Memory.ReadByte((IntPtr)RaceAddrs.STATIC_MAX_DRIFT_MULTIPLIER_CANYON);
                set
                {
                    Memory.WriteByte((IntPtr)RaceAddrs.STATIC_MAX_DRIFT_MULTIPLIER_CANYON, value);
                    Memory.WriteByte((IntPtr)RaceAddrs.STATIC_MAX_DRIFT_MULTIPLIER_CANYON2, value);
                }
            }

            /// <summary>
            /// Returns an array of string which contains the current opponent names.
            /// </summary>
            /// <param name="length">The amount of the opponents name that the array will return.</param>
            /// <returns></returns>
            public static string[] GetOpponentNames(byte length)
            {
                var opponents = new List<string>();
                for(byte i = 0; i < length; i++)
                {
                    opponents.Add(_getOpponentNameByID(i));
                }

                return opponents.ToArray();
            }

            /// <summary>
            /// Sets the name of the defined opponent ID.
            /// </summary>
            /// <param name="ID">The ID of the opponent.</param>
            /// <param name="name">The name.</param>
            public static void SetOpponentNameByID(byte ID, string name)
            {
                _setOpponentNameByID(ID, name);
            }

            /// <summary>
            /// Expires the race.
            /// </summary>
            public static void SetRaceExpired()
            {
                Function.Call(SET_RACE_EXPIRED);
            }

            /// <summary>
            /// Set if the player should win the race regardless of the player's or his wingman finishing position.
            /// </summary>
            /// <param name="enabled"></param>
            public static void SetPlayerAlwaysWin(bool enabled)
            {
                uint x = 0, x2 = 0, x3 = 0, x4 = 0, x5 = 0;

                if (enabled)
                {
                    x = 0x90900134;
                    x2 = 0x13EBC084;
                    x3 = 0x9001347F;
                    x4 = 0x90909090;
                    x5 = 0xC8878A90;
                }
                else if (!enabled)
                {
                    x = 0x40750134;
                    x2 = 0x1374C084;
                    x3 = 0xF01347F;
                    x4 = 0x24BB5;
                    x5 = 0xC8878A00;
                }

                Memory.WriteUInteger((IntPtr)RaceAddrs.STATIC_PLAYER_ALWAYS_WIN_ADDR_1, x);
                Memory.WriteUInteger((IntPtr)RaceAddrs.STATIC_PLAYER_ALWAYS_WIN_ADDR_2, x2);
                Memory.WriteUInteger((IntPtr)RaceAddrs.STATIC_PLAYER_ALWAYS_WIN_ADDR_3, x3);
                Memory.WriteUInteger((IntPtr)RaceAddrs.STATIC_PLAYER_ALWAYS_WIN_ADDR_4, x4);
                Memory.WriteUInteger((IntPtr)RaceAddrs.STATIC_PLAYER_ALWAYS_WIN_ADDR_5, x5);
            }

            internal static string _getOpponentNameByID(byte ID)
            {
                var pointer = Memory.ReadInt32((IntPtr)RaceAddrs.STATIC_RACE_LEADERBOARD_POINTER);
                var firstIDAddr = pointer + RaceAddrs.STATIC_RACE_LEADERBOARD_FIRST_PLACE_OFFSET;

                var x = 0;
                for(var i = 0; i < ID; i++)
                {
                    x = x + RaceAddrs.STATIC_RACE_LEADERBOARD_OFFSET;
                }

                //return memory.ReadStringASCII((IntPtr)firstIDAddr + x, 15);
                return Encoding.ASCII.GetString(Memory.ReadByteArray((IntPtr)firstIDAddr + x, 15).Where(c => c != 0xEE).Where(b => b != 0x00).Where(b => b != 0xFF)
                    .Where(b => b != 0x20).Where(b => b != 0x74).Where(b => b != 0x9D).Where(b => b != 0xFE).ToArray());
            }

            internal static void _setOpponentNameByID(byte ID, string name)
            {
                var pointer = Memory.ReadInt32((IntPtr)RaceAddrs.STATIC_RACE_LEADERBOARD_POINTER);
                var firstIDAddr = pointer + RaceAddrs.STATIC_RACE_LEADERBOARD_FIRST_PLACE_OFFSET;

                var x = 0;
                for (var i = 0; i < ID; i++)
                {
                    x = x + RaceAddrs.STATIC_RACE_LEADERBOARD_OFFSET;
                }

                var nameArray = new List<byte>();
                nameArray.AddRange(Encoding.ASCII.GetBytes(new string(name.Take(15).ToArray())));
                nameArray.Add(0x00);

                Memory.WriteByteArray((IntPtr)firstIDAddr + x, nameArray.ToArray());
            }
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
        public static IntPtr Address => (IntPtr)GenericAddrs.STATIC_GAME_STATE;

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
