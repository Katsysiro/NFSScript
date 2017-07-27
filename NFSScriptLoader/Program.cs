using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Timers;
using System.Windows.Forms;
using Microsoft.CSharp;
using Microsoft.VisualBasic;
using NFSScript;
using NFSScript.Core;
using Tick = System.Timers.Timer;
using RunningMemory = NFSScript.Core.VAMemory;
using NFSSKeys = NFSScript.Keys;

namespace NFSScriptLoader
{
    public class Program
    {
        // ReSharper disable InconsistentNaming
        private static class InternalPtrs
        {
            public static IntPtr GAMEPLAY_ACTIVE;
            public static IntPtr IS_ACTIVITY_MODE;
        }
        // ReSharper restore InconsistentNaming
        
        private static class Settings
        {
            internal static bool ShowConsole;
            internal static bool Debug;
        }
        private static Tick _timer;

        private static bool _inGameplay;
        private static bool _inRace;

        private const int UpdateTick = 1;
        private const int WaitBeforeLoad = 5000;

        private static List<ModScript> _scripts;

//        private const int WH_KEYBOARD_LL = 13;
//        private const int WM_KEYUP = 256;
//        private const int WM_KEYDOWN = 257;
//        private const int SW_HIDE = 0;
//        private const int SW_SHOW = 5;

        private const string IniFileName = "NFSScriptSettings.ini";

        private const int ResetKey = (int) NFSSKeys.Insert;

        public delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);
        
        private static readonly LowLevelKeyboardProc Proc = HookCallback;
        private static IntPtr _hookId = IntPtr.Zero;

        private static NFSGame _currentNFSGame;

        /// <summary>
        /// For internal use only!
        /// </summary>
        public static RunningMemory GameMemory;

        private static string _gameProcessName = string.Empty;
        private static string _processNameTitle = string.Empty;

        public static Process GameProcess { get; private set; }

        private static void EntryPoint(string pwzArgument)
        {
            Log.Print(NFSScriptLoader.INFO_TAG, $"{pwzArgument} by Dennis Stanistan");

            InitIni();
            GetNFSGame();
            Start();
        }

        private static void Main()
        {
            EntryPoint("NFSScript");
        }

        /// <summary>
        /// Call the OnExit method for any loaded scripts and shuts down the program.
        /// </summary>
        private static void Terminate()
        {
            CallScriptsOnExit();
            _timer.Dispose();
            _scripts = null;
            _inGameplay = false;
            _inRace = false;
            GameMemory = null;
            _hookId = IntPtr.Zero;
            Environment.Exit(0);
        }

        /// <summary>
        /// Loads data from the .ini file.
        /// </summary>
        private static void InitIni()
        {
            var ini = new IniFile(IniFileName);
            if (File.Exists(ini.Path))
            {
                if (ini.KeyExists("ShowConsole", "NFSScript"))
                {
                    Settings.ShowConsole = FlexiableParse(ini.Read("ShowConsole", "NFSScript")) == 1;                    
                }
                if (ini.KeyExists("Debug", "NFSScript"))
                {
                    Settings.Debug = FlexiableParse(ini.Read("Debug", "NFSScript")) == 1;
                }
            }

            if (!Settings.ShowConsole)
                NativeMethods.ShowWindow(NativeMethods.GetConsoleWindow(), 0);
        }

        /// <summary>
        /// Detects and applies process variables for the supported NFS games.
        /// </summary>
        private static void GetNFSGame()
        {
            Log.Print(NFSScriptLoader.INFO_TAG, "Getting executeable in directory.");
            _currentNFSGame = NFS.DetectGameExecutableInDirectory;
            var wrldTick = 0;
            // ReSharper disable once SwitchStatementMissingSomeCases
            switch (_currentNFSGame)
            {
                case NFSGame.Undetermined:
                    Log.Print(NFSScriptLoader.ERROR_TAG, "A valid NFS executable was not found.");
                    Environment.Exit(1);
                    return;

                case NFSGame.Carbon:
                    Log.Print(NFSScriptLoader.INFO_TAG, "Need for Speed: Carbon detected.");
                    SetProcessVariables(Process.Start(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, NFS.ImageNames.Carbon)));
                    break;

                case NFSGame.MW:
                    Log.Print(NFSScriptLoader.INFO_TAG, "Need for Speed: Most Wanted detected.");
                    SetProcessVariables(Process.Start(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, NFS.ImageNames.MWUG)));
                    break;

                case NFSGame.Underground:
                    Log.Print(NFSScriptLoader.INFO_TAG, "Need for Speed: Underground detected.");
                    SetProcessVariables(Process.Start(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, NFS.ImageNames.MWUG)));
                    break;

                case NFSGame.Underground2:
                    Log.Print(NFSScriptLoader.INFO_TAG, "Need for Speed: Underground 2 detected.");
                    SetProcessVariables(Process.Start(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, NFS.ImageNames.UG2)));
                    break;

                case NFSGame.ProStreet:
                    Log.Print(NFSScriptLoader.INFO_TAG, "Need for Speed: ProStreet detected.");
                    SetProcessVariables(Process.Start(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, NFS.ImageNames.UndercoverProStreet)));
                    break;

                case NFSGame.Undercover:
                    Log.Print(NFSScriptLoader.INFO_TAG, "Need for Speed: Undercover detected.");
                    SetProcessVariables(Process.Start(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, NFS.ImageNames.UndercoverProStreet)));
                    break;

                case NFSGame.World:
                    Log.Print(NFSScriptLoader.INFO_TAG, "Waiting for game's launch.");
                    while (Process.GetProcessesByName("nfsw").Length == 0 && (wrldTick < 120))
                    {
                        Thread.Sleep(1000);
                        wrldTick++;
                    }
                    if (Process.GetProcessesByName("nfsw").Length == 0)
                        Environment.Exit(0);
                    else
                    {
                        Log.Print(NFSScriptLoader.INFO_TAG, "Need for Speed: World detected.");
                        SetProcessVariables(Process.GetProcessesByName("nfsw")[0]);
                    }
                    break;

                default:
                    Log.Print(NFSScriptLoader.ERROR_TAG, "A valid NFS executeable was not found.");
                    Environment.Exit(0);
                    return;
            }
            NFSScript.NFSScript.CurrentLoadedNFSGame = _currentNFSGame;
        }

        /// <summary>
        /// Get the game's memory by it's process name.
        /// </summary>
        /// <param name="processName"></param>
        private static void GetGameMemory(string processName)
        {
            GameMemory = new RunningMemory(_gameProcessName);
            GameMemory.ReadInt32((IntPtr)0);

            NFSScript.Core.GameMemory.Memory = GameMemory;
            NFSScript.Core.GameMemory.GenericMemory = new GMemory(GameMemory.processName);
            if (Settings.Debug)
                RunningMemory.debugMode = true;
        }

        /// <summary>
        /// Start loading scripts and applying keys & scripts events.
        /// </summary>
        private static void Start()
        {
            LoadScripts();

            GetGameMemory(_gameProcessName);
            CallPreScriptMethod();
            Log.Print(NFSScriptLoader.INFO_TAG,
                $"Delaying the loader's thread for {(WaitBeforeLoad / 1000)} seconds before initializing.");
            // TODO FIXME: this is a very bad way to do this...
            Thread.Sleep(WaitBeforeLoad);
            Log.Print(NFSScriptLoader.INFO_TAG, "Delay is over, initializing.");
            // Step 1
            if (NFS.IsGameRunning())
            {
                CallInitScriptMethod();
                WaitForGameLoad();

                ApplyAndLoadIntPtrs();
                Log.Print(NFSScriptLoader.INFO_TAG, "Game is fully loaded.");

                CallMainScriptMethod();

                _timer = new Tick {Interval = UpdateTick};
                _timer.Elapsed += Update_Elapsed;
                _timer.Start();

                SetKeyboardHook();
            }
            else Environment.Exit(0);
        }

        /// <summary>
        /// Wait till the game loads.
        /// </summary>
        private static void WaitForGameLoad()
        {
            Log.Print(NFSScriptLoader.INFO_TAG, "Waiting for the game to fully load. (Disabled in Most Wanted)");
            switch (_currentNFSGame)
            {
                case NFSGame.Undetermined:
                    break;

                case NFSGame.Underground:
                    while (GameMemory.ReadByte((IntPtr)UGAddresses.GenericAddrs.STATIC_IS_GAME_LOADED) != 0x01 && NFS.IsGameRunning())
                    {
                        Thread.Sleep(100);
                    }
                    break;

                case NFSGame.Underground2:
                    while (GameMemory.ReadByte((IntPtr)UG2Addresses.GenericAddrs.STATIC_IS_GAME_LOADED) != 0x01 && NFS.IsGameRunning())
                    {
                        Thread.Sleep(100);
                    }
                    break;

                case NFSGame.MW:
                    break;

                case NFSGame.Carbon:
                    while (GameMemory.ReadByte((IntPtr)CarbonAddresses.GenericAddrs.STATIC_IS_GAME_LOADED) != 0x01 && NFS.IsGameRunning())
                    {
                        Thread.Sleep(100);
                    }
                    break;

                case NFSGame.ProStreet:
                    while(GameMemory.ReadByte((IntPtr)ProStreetAddresses.GenericAddrs.STATIC_IS_GAME_LOADED) != 0x01 && NFS.IsGameRunning())
                    {
                        Thread.Sleep(100);
                    }
                    break;

                case NFSGame.Undercover:
                    while (GameMemory.ReadByte((IntPtr)UndercoverAddresses.GenericAddrs.STATIC_IS_GAME_LOADED) != 0x01 && NFS.IsGameRunning())
                    {
                        Thread.Sleep(100);
                    }
                    break;

                case NFSGame.World:
                    while (GameMemory.ReadByte((IntPtr)GameMemory.getBaseAddress + WorldAddresses.GenericAddrs.NON_STATIC_IS_GAME_LOADED) != 0x01 && NFS.IsGameRunning())
                    {
                        Thread.Sleep(100);
                    }
                    break;
            }
        }

        /// <summary>
        /// Applies and loads necessary memory pointers.
        /// </summary>
        private static void ApplyAndLoadIntPtrs()
        {
            switch (_currentNFSGame)
            {
                case NFSGame.Undetermined:
                    break;

                case NFSGame.Underground:
                    InternalPtrs.GAMEPLAY_ACTIVE = (IntPtr)UGAddresses.GenericAddrs.STATIC_IS_GAMEPLAY_ACTIVE;
                    InternalPtrs.IS_ACTIVITY_MODE = (IntPtr)UGAddresses.GenericAddrs.STATIC_IS_GAMEPLAY_ACTIVE;
                    break;

                case NFSGame.Underground2:
                    InternalPtrs.GAMEPLAY_ACTIVE = (IntPtr)UG2Addresses.GenericAddrs.STATIC_IS_GAMEPLAY_ACTIVE;
                    InternalPtrs.IS_ACTIVITY_MODE = IntPtr.Zero;
                    break;

                case NFSGame.MW:
                    InternalPtrs.GAMEPLAY_ACTIVE = (IntPtr)MWAddresses.GenericAddrs.STATIC_IS_GAMEPLAY_ACTIVE;
                    InternalPtrs.IS_ACTIVITY_MODE = (IntPtr)MWAddresses.GameAddrs.STATIC_IS_ACTIVITY_MODE;
                    break;

                case NFSGame.Carbon:
                    InternalPtrs.GAMEPLAY_ACTIVE = (IntPtr)CarbonAddresses.GenericAddrs.STATIC_IS_GAMEPLAY_ACTIVE;
                    InternalPtrs.IS_ACTIVITY_MODE = (IntPtr)CarbonAddresses.GameAddrs.STATIC_IS_ACTIVITY_MODE;
                    break;

                case NFSGame.ProStreet:
                    InternalPtrs.GAMEPLAY_ACTIVE = (IntPtr)ProStreetAddresses.GenericAddrs.STATIC_IS_GAMEPLAY_ACTIVE;
                    InternalPtrs.IS_ACTIVITY_MODE = (IntPtr)ProStreetAddresses.GameAddrs.STATIC_IS_ACTIVITY_MODE;
                    break;

                case NFSGame.Undercover:
                    InternalPtrs.GAMEPLAY_ACTIVE = (IntPtr)UndercoverAddresses.GenericAddrs.STATIC_IS_GAMEPLAY_ACTIVE;
                    InternalPtrs.IS_ACTIVITY_MODE = IntPtr.Zero;
                    break;

                case NFSGame.World:
                    InternalPtrs.GAMEPLAY_ACTIVE = (IntPtr)GameMemory.getBaseAddress + WorldAddresses.GenericAddrs.NON_STATIC_IS_GAMEPLAY_ACTIVE;
                    InternalPtrs.IS_ACTIVITY_MODE = (IntPtr)GameMemory.getBaseAddress + WorldAddresses.GameAddrs.NON_STATIC_IS_ACTIVITY_MODE;
                    break;
            }
        }

        /// <summary>
        /// Load supported .NET scripts from the /scripts directory.
        /// </summary>
        private static void LoadScripts()
        {
            _scripts = new List<ModScript>();
            var dllFiles = Directory.GetFiles(NFSScriptLoader.SCRIPTS_FOLDER, "*.dll");
            var sourceFiles = Directory.GetFiles(NFSScriptLoader.SCRIPTS_FOLDER, "*.cs")
                .Concat(Directory.GetFiles(NFSScriptLoader.SCRIPTS_FOLDER, "*.vb"));

            var compiledScripts = 0;

            foreach (var file in dllFiles)
            {
                if (!IsValidAssembly(file)) continue;
                var script = new ModScript(file);
                _scripts.Add(script);
                compiledScripts++;
                Log.Print(NFSScriptLoader.INFO_TAG, $"Loaded {script.File}");
            }
            foreach (var file in sourceFiles)
            {
                if (!CompileScript(file))
                {
                    Log.Print(NFSScriptLoader.ERROR_TAG, $"Error compiling {file}!! Will abort.");
                    NFSScriptLoader.CriticalError($"Could not compile script at {file}");
                }
                
                compiledScripts++;
            }

            Log.Print(NFSScriptLoader.INFO_TAG, $"{compiledScripts} scripts are loaded.");
        }

        private static void CallPreScriptMethod()
        {
            for (var i = 0; i < _scripts.Count; i++)
            {
                if (_scripts[i].HasPre)
                    _scripts[i].CallModFunction(ModScript.ModMethod.Pre);
            }
        }

        /// <summary>
        /// Call scripts Init() method.
        /// </summary>
        private static void CallInitScriptMethod()
        {
            for (var i = 0; i < _scripts.Count; i++)
            {
                if (_scripts[i].HasInitialize)
                    _scripts[i].CallModFunction(ModScript.ModMethod.Initialize);
            }
        }

        /// <summary>
        /// Call scripts Main() method.
        /// </summary>
        private static void CallMainScriptMethod()
        {
            for (var i = 0; i < _scripts.Count; i++)
            {
                if (_scripts[i].HasMain)
                    _scripts[i].CallModFunction(ModScript.ModMethod.Main);
            }
        }

        /// <summary>
        /// Call scripts methods such as OnKeyUp, OnKeyDown & Update.
        /// </summary>
        private static void CallScriptsEvents()
        {
            var isGameplayActive = false;
            if (_currentNFSGame != NFSGame.Undercover && _currentNFSGame != NFSGame.World)
                isGameplayActive = GameMemory.ReadByte(InternalPtrs.GAMEPLAY_ACTIVE) == 1;
            else if(_currentNFSGame == NFSGame.Undercover)
                isGameplayActive = GameMemory.ReadInt32((IntPtr)UndercoverAddresses.GenericAddrs.STATIC_GAME_STATE) == 6;
            else if (_currentNFSGame == NFSGame.World)
                isGameplayActive = GameMemory.ReadInt32((IntPtr)GameMemory.getBaseAddress + WorldAddresses.GenericAddrs.NON_STATIC_GAME_STATE) == 6;

            if (isGameplayActive && !_inGameplay)
            {
                for (var i = 0; i < _scripts.Count; i++)
                {
                    if (_scripts[i].HasOnGameplayStart && !_scripts[i].IsInGameplay)
                    {
                        _scripts[i].CallModFunction(ModScript.ModMethod.OnGameplayStart);
                        _scripts[i].IsInGameplay = true;
                    }
                }
                _inGameplay = true;
            }

            if (!isGameplayActive && _inGameplay)
            {
                for (var i = 0; i < _scripts.Count; i++)
                {
                    if (_scripts[i].HasOnGameplayExit && _scripts[i].IsInGameplay)
                    {
                        _scripts[i].CallModFunction(ModScript.ModMethod.OnGameplayExit);
                        _scripts[i].IsInGameplay = false;
                    }
                }
                _inGameplay = false;
            }

            if ((GameMemory.ReadByte(InternalPtrs.IS_ACTIVITY_MODE)) == 1 && !_inRace)
            {
                for (var i = 0; i < _scripts.Count; i++)
                {
                    if (_scripts[i].HasOnActivityEnter && !_scripts[i].IsInActivity)
                    {
                        _scripts[i].CallModFunction(ModScript.ModMethod.OnActivityEnter);
                        _scripts[i].IsInActivity = true;
                    }
                }
                _inRace = true;
            }

            if ((GameMemory.ReadByte(InternalPtrs.IS_ACTIVITY_MODE)) == 0 && _inRace)
            {
                for (var i = 0; i < _scripts.Count; i++)
                {
                    if (_scripts[i].HasOnActivityExit && _scripts[i].IsInActivity)
                    {
                        _scripts[i].CallModFunction(ModScript.ModMethod.OnActivityExit);
                        _scripts[i].IsInActivity = false;
                    }
                }
                _inRace = false;
            }
        }

        /// <summary>
        /// Call scripts OnExit() method.
        /// </summary>
        private static void CallScriptsOnExit()
        {
            for (var i = 0; i < _scripts.Count; i++)
            {
                if (_scripts[i].HasOnExit)
                    _scripts[i].CallModFunction(ModScript.ModMethod.OnExit);
            }
        }

        /// <summary>
        /// Update elapsed event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void Update_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (!NFS.IsGameRunning())
            {
                Log.Print(NFSScriptLoader.INFO_TAG, "Game is not running.");
                Terminate();                
            }

            CallScriptsEvents();
            for (var i = 0; i < _scripts.Count; i++)
            {
                if (_scripts[i].HasUpdate)
                    _scripts[i].CallModFunction(ModScript.ModMethod.Update);
            }
        }

        /// <summary>
        /// Compile script by file.
        /// </summary>
        /// <param name="file"></param>
        private static bool CompileScript(string file)
        {
            var sourceFile = new FileInfo(file);
            CodeDomProvider provider = null;
            var csc = new CSharpCodeProvider(new Dictionary<string, string> { { "CompilerVersion", "v4.0" } });

            // TODO: maybe allow custom references like CS-Script
            var parameters = new CompilerParameters(new[]
                {"mscorlib.dll", "System.Core.dll", "System.Windows.Forms.dll", "NFSScript.dll"})
            {
                GenerateExecutable = false,
                GenerateInMemory = true
            };

            switch (sourceFile.Extension.ToUpperInvariant())
            {
                case ".CS":
                    provider = new CSharpCodeProvider();
                    break;
                case ".VB":
                    provider = new VBCodeProvider();
                    break;
            }

            if (provider == null) return false;
            
            var results = provider.CompileAssemblyFromFile(parameters, file);
            if (results.Errors.HasErrors)
            {
                foreach (CompilerError r in results.Errors)
                {
                    Log.Print(NFSScriptLoader.ERROR_TAG, r.ToString());
                }
                return false;
            }
            var ass = results.CompiledAssembly;
            var script = new ModScript(ass, Path.GetFileName(file));
            _scripts.Add(script);
            Log.Print(NFSScriptLoader.INFO_TAG, $"Loaded {script.File}");

            return true;
        }

        /// <summary>
        /// Restart the program.
        /// </summary>
        private static void Restart()
        {
            Log.Print(NFSScriptLoader.INFO_TAG, "Restarting...");
            CallScriptsOnExit();
            _timer.Stop();
            _scripts = null;
            _inGameplay = false;
            _inRace = false;
            LoadScripts();
            CallPreScriptMethod();
            CallInitScriptMethod();
            CallMainScriptMethod();            
            _timer.Start();
        }

        /// <summary>
        /// A flexiable parse method for the NFSScriptLoader.ini file.
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        private static int FlexiableParse(string s)
        {
            try {
                return int.Parse(s);
            }
            catch { return 0; }
        }

        /// <summary>
        /// Returns a value that indicates whether the specified .dll is a valid assembly file or not.
        /// </summary>
        /// <param name="dll"></param>
        /// <returns></returns>
        private static bool IsValidAssembly(string dll)
        {
            try
            {
                Assembly.LoadFrom(dll);
                return true;
            }
            catch { return false; }
        }

        public static bool IsRunning(Process process)
        {
            if (process == null)
                throw new ArgumentNullException(nameof(process));

            try
            {
                Process.GetProcessById(process.Id);
            }
            catch (ArgumentException)
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Set keyboard's hook to get any pressed key when the game is maximized.
        /// </summary>
        private static void SetKeyboardHook()
        {
            _hookId = SetHook(Proc);
            Application.Run();
            NativeMethods.UnhookWindowsHookEx(_hookId);
        }

        /// <summary>
        /// Print a debug message to the log if the debug value is set to 1 in the .ini file.
        /// </summary>
        /// <param name="msg"></param>
        private static void PrintDebugMsg(string msg)
        {
            if (Settings.Debug)
                Log.Print(NFSScriptLoader.DEBUG_TAG, msg);
        }

        /// <summary>
        /// Get the hook callback.
        /// </summary>
        /// <param name="nCode">classic EA param that does nothing</param>
        /// <param name="wParam">event param</param>
        /// <param name="lParam">key num param</param>
        /// <returns></returns>
        private static IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            var mainWindowHandle = GameMemory.GetMainProcess().MainWindowHandle;

            if ((_currentNFSGame == NFSGame.World || NFS.IsGameMinimized()) &&
                (_currentNFSGame != NFSGame.World || !NFS.IsGameFocused()))
                return NativeMethods.CallNextHookEx(_hookId, nCode, wParam, lParam);
            if (nCode >= 0 && wParam == (IntPtr)257) // OnKeyUp
            {                    
                var num = Marshal.ReadInt32(lParam);

                if (num == ResetKey)
                    Restart();

                foreach (var script in _scripts)
                {
                    if (script.HasOnKeyUp)
                    {
                        script.CallModFunction(ModScript.ModMethod.OnKeyUp, (NFSSKeys)num);
                    }
                }
            }

            if (nCode < 0 || wParam != (IntPtr) 256) // OnKeyDown
                return NativeMethods.CallNextHookEx(_hookId, nCode, wParam, lParam);
            {
                var num = Marshal.ReadInt32(lParam);
                foreach (var script in _scripts)
                {
                    if (script.HasOnKeyDown)
                    {
                        script.CallModFunction(ModScript.ModMethod.OnKeyDown, (NFSSKeys)num);
                    }
                }
            }

            return NativeMethods.CallNextHookEx(_hookId, nCode, wParam, lParam);
        }

        /// <summary>
        /// Set process variables.
        /// </summary>
        /// <param name="p"></param>
        private static void SetProcessVariables(Process p)
        {
            GameProcess = p;
            _gameProcessName = p.ProcessName;
            _processNameTitle = p.MainWindowTitle;

            Log.Print(NFSScriptLoader.INFO_TAG, "Game memory is loaded.");
        }

        /// <summary>
        /// Set hook.
        /// </summary>
        /// <param name="proc"></param>
        /// <returns></returns>
        private static IntPtr SetHook(LowLevelKeyboardProc proc)
        {
            using (var currentProcess = Process.GetCurrentProcess())
            {
                using (var mainModule = currentProcess.MainModule)
                    return NativeMethods.SetWindowsHookEx(13, proc, NativeMethods.GetModuleHandle(mainModule.ModuleName), 0U);
            }
        }

    }

    internal struct NFS
    {
        // A list of constant strings that contain the executable file name.
        internal static class ImageNames
        {
            public const string MWUG = "speed.exe";
            public const string UG2 = "speed2.exe";
            public const string Carbon = "NFSC.exe";
            public const string UndercoverProStreet = "nfs.exe";
            public const string World = "nfsw.exe";
        }

        private static class ContainedStrings
        {
            public static readonly byte[] MostWanted = Encoding.ASCII.GetBytes("Most Wanted");
            public static readonly byte[] Underground = Encoding.ASCII.GetBytes("underground");
            public static readonly byte[] Undercover = Encoding.ASCII.GetBytes("Undercover");
        }

        /// <summary>
        /// Detects the supported game executable in directory.
        /// </summary>
        public static NFSGame DetectGameExecutableInDirectory
        {
            // This is a really bad way of detecting executables
            // TODO: Get game versions by their entry points
            // TODO: (fallk) maybe just use window names? i don't have prostreet/undercover so i can't confirm if theyre all unique
            get
            {
                if (File.Exists(ImageNames.MWUG)) // speed.exe exists
                {
                    var which = File.OpenRead(ImageNames.MWUG)
                        .ContainsWhich(ContainedStrings.MostWanted, ContainedStrings.Underground);

                    if (which == ContainedStrings.MostWanted) // "Most Wanted" in .exe
                    {
                        return NFSGame.MW;
                    }
                    if (which == ContainedStrings.Underground) // "underground" in .exe
                    {
                        return NFSGame.Underground;
                    }
                    if (which == null) // none of the above
                    {
                        return NFSGame.Undetermined;
                    }
                }
                if (File.Exists(ImageNames.UG2)) // speed2.exe exists
                {
                    return NFSGame.Underground2;
                }
                if (File.Exists(ImageNames.Carbon)) // NFSC.exe exists
                {
                    return NFSGame.Carbon;
                }
                if (File.Exists(ImageNames.UndercoverProStreet)) // nfs.exe exists
                {
                    return File.OpenRead(ImageNames.MWUG).Contains(ContainedStrings.Undercover) 
                        ? NFSGame.Undercover 
                        : NFSGame.ProStreet;
                }
                if (File.Exists(ImageNames.World)) // nfsw.exe exists
                {
                    return NFSGame.World;
                }
                return NFSGame.Undetermined;
            }
        }

        public static bool IsGameMinimized()
        {
            return CurrentGame.IsMinimized;
        }

        public static bool IsGameFocused()
        {
            return CurrentGame.IsGameInFocus;
        }

        public static bool IsGameRunning()
        {
            return (uint)Process.GetProcessesByName(Program.GameMemory.GetMainProcess().ProcessName).Length > 0U;
        }
    }

    public static class NFSScriptLoader
    {
        public const string LOADER_TAG = "NFSScriptLoader";
        public const string ERROR_TAG = "NFSScriptLoader ERROR";
        public const string INFO_TAG = "NFSScriptLoader INFO";
        public const string DEBUG_TAG = "NFSScriptLoader DEBUG";
        public const string SCRIPTS_FOLDER = "scripts\\";

        public static RunningMemory GetRunningGameMemory()
        {
            return Program.GameMemory;
        }

        // TODO in the future we can add something more useful like error dialog.
        public static void CriticalError(string s)
        {
            throw new NotImplementedException();
        }
    }

    public static class NativeMethods
    {
        [DllImport("user32.dll", SetLastError = true)]
        public static extern uint GetWindowThreadProcessId(IntPtr hWnd, out int lpdwProcessId);

        [DllImport("user32.dll")]
        public static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr SetWindowsHookEx(int idHook, Program.LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr GetModuleHandle(string lpModuleName);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool IsIconic(IntPtr hWnd);

        [DllImport("user32.dll")]
        public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("Kernel32")]
        public static extern IntPtr GetConsoleWindow();
    }
}
