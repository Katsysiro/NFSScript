using System;
using System.Diagnostics;
using System.IO;
using NFSScript.Core;

namespace NFSScript
{
    /// <summary>
    /// A class for NFSScript related funcions and variables.
    /// </summary>
    public static class NFSScript
    {
        /// <summary>
        /// Does debug-y things.
        /// </summary>
        // TODO: this bool should probably be a const. Opinions?
        // TODO: ReSharper advises against ANSI style constant names, and recommends regular PascalCase. Opinions?
        public static bool DEBUG = false;

        /// <summary>
        /// 
        /// </summary>
        public static NFSGame CurrentLoadedNFSGame;

        /// <summary>
        /// Returns the directory of the script loader (NFSScriptLoader.exe).
        /// </summary>
        public static string Directory => AppDomain.CurrentDomain.BaseDirectory;

        /// <summary>
        /// Returns the game's directory.
        /// </summary>
        public static string GameDirectory => Path.GetDirectoryName(GameMemory.Memory.GetMainProcess().MainModule.FileName);
    }

    /// <summary>
    /// The class that represents the current game.
    /// </summary>
    public static class CurrentGame
    {
        /// <summary>
        /// Returns a value that indicates whether the game is minimized or not.
        /// </summary>
        public static bool IsMinimized => NativeMethods.IsIconic(GameMemory.Memory.GetMainProcess().MainWindowHandle);

        /// <summary>
        /// Returns a value that indicates whether the game window is focused or not.
        /// </summary>
        public static bool IsGameInFocus
        {
            get
            {
                int processId;
                NativeMethods.GetWindowThreadProcessId(NativeMethods.GetForegroundWindow(), out processId);
                var processToCheck = Process.GetProcessById(processId);
                return GameMemory.Memory.GetMainProcess().Id == processToCheck.Id;
            }
        }
    }

    /// <summary>
    /// Enum of the different supported NFS games.
    /// </summary>
    public enum NFSGame : byte
    {
        /// <summary>
        /// None.
        /// </summary>
        Undetermined = 0,
        /// <summary>
        /// The game Need for Speed: Underground.
        /// </summary>
        Underground = 1,
        /// <summary>
        /// The game Need for Speed: Underground 2.
        /// </summary>
        Underground2 = 2,
        /// <summary>
        /// The game Need for Speed: Most Wanted.
        /// </summary>
        MW = 3,
        /// <summary>
        /// The game Need for Speed: Carbon.
        /// </summary>
        Carbon = 4,
        /// <summary>
        /// The game Need for Speed: ProStreet.
        /// </summary>
        ProStreet = 5,
        /// <summary>
        /// The game Need for Speed: Undercover.
        /// </summary>
        Undercover = 6,
        /// <summary>
        /// The game Need for Speed: World.
        /// </summary>
        World = 7,
        /// <summary>
        /// Undetermined game.
        /// </summary>
        [Obsolete("No point in using 255 instead of 0, since both serve the same purpose.")]
        LegacyUndetermined = 255
    }
}
