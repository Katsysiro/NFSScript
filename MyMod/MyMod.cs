using System;
using NFSScript;
using NFSScript.Math;
using NFSScript.Core;
using NFSScript.Carbon;

using static NFSScript.Core.GameMemory;
using Addrs = NFSScript.Core.UG2Addresses;


//using Newtonsoft.Json;
//using SocketIOClient.WebSocketClient;
using System.Collections.Generic;
//using NativeWebSocket;
using System.Threading.Tasks;

namespace MyMod
{
    public class AMod : Mod
    {
        //public WebSocket socket = new WebSocket("ws://localhost:52302");

        public AMod()
        {
            //socket = new WebSocket("ws://localhost:52301");

            //setupEvent();

            /*socket.Connect().Wait();

            Log.Print("socket", "Connect");

            if (socket != null)
            {
                if (socket.State == WebSocketState.Open)
                {
                    socket.SendText(@"{""e"": ""update"", ""d"": { x: 0, y:1, z:2, s:3 } }").Wait();
                }
            }*/

            //SynchronousSocketClient.StartClient();
            //AsynchronousClient.StartClient();
        }

        /// <summary>
        /// Pre is called as soon as the the game memory loads.
        /// </summary>
        public override void Pre()
        {
            Log.Print("MyMod", "Pre");
        }

        /// <summary>
        /// Initialize is called when the script is ready and enabled just before any of the Main and Update methods are called.
        /// </summary>
        public override void Initialize()
        {
            Log.Print("MyMod", "Initialize");

            // прочитаем WindowedMode
            var WindowedModeGet = Memory.ReadInt32((IntPtr)0x87098C);
            Log.Print("MyMod", $"Initialize WindowedModeGet: {WindowedModeGet}");

            // попробуем добавить window mode
            // WindowedMode 
            //var WindowedMode = Memory.WriteInt32((IntPtr)0x87098C, 2);
            //Log.Print("MyMod", $"WindowedMode: {WindowedMode}");

            // попробуем пропустить видео в начале
            // SkipMovies  
            //var SkipMovies = Memory.WriteInt32((IntPtr)0x8650A8, 1);
            //Log.Print("MyMod", $"SkipMovies: {SkipMovies}");
        }

        /// <summary>
        /// Main is called after the Initialize method is called and before the Update method is called.
        /// </summary>
        public void Main()
        {
            Log.Print("MyMod", "Main");

            var WindowedModeGet = Memory.ReadInt32((IntPtr)0x87098C);
            Log.Print("MyMod", $"Main WindowedModeGet: {WindowedModeGet}");
        }

        /// <summary>
        /// Update method is called every 1ms.
        /// </summary>
        public override void Update()
        {
            //Log.Print("MyMod", "Update");

            var Speed = NFSScript.Underground2.Player.Car.Speed;

            //Log.Print("MyMod", $"Update Speed: {Speed}");

            var x = NFSScript.Underground2.Player.Car.Position.X;

            //Log.Print("MyMod", $"Update position x: {x}");

            /*if (socket != null) {
                if (socket.State == WebSocketState.Open) {
                    socket.SendText(@"{""e"": ""update"", ""d"": { x: 0, y:1, z:2, s:3 } }").Wait();
                }
            }*/

            //SynchronousSocketClient.Send();
        }

        /// <summary>
        /// This method is called when the gameplay starts.
        /// </summary>
        public override void OnGameplayStart()
        {
            Log.Print("MyMod", "OnGameplayStart");
        }

        /// <summary>
        /// This method is called when the gameplay is stopped (When the player enters the garage/car lot/etc...).
        /// </summary>
        public override void OnGameplayExit()
        {
            Log.Print("MyMod", "OnGameplayExit");
        }

        /// <summary>
        /// This method is called when the player has entered an activity (Doesn't work in Most Wanted and Undercover, yet).
        /// </summary>
        public override void OnActivityEnter()
        {
            Log.Print("MyMod", "OnActivityEnter");
        }

        /// <summary>
        /// This method is called when the player has exited from an activity (Doesn't work in Most Wanted and Undercover, yet).
        /// </summary>
        public override void OnActivityExit()
        {
            Log.Print("MyMod", "OnActivityExit");
        }

        /// <summary>
        /// This method is called when a key is being held in-game.
        /// </summary>
        public override void OnKeyDown(Keys key)
        {
            Log.Print("MyMod", $"OnKeyDown: {key.ToString()}");
        }

        /// <summary>
        /// This method is called when a key has been released in-game.
        /// </summary>
        public override void OnKeyUp(Keys key)
        {
            Log.Print("MyMod", $"OnKeyUp: {key.ToString()}");

            if (key == Keys.D1)
            {
                var x = NFSScript.Underground2.Player.Car.Position.X;

                var teleport_result = Memory.WriteFloat((IntPtr)Addrs.PlayerAddrs.STATIC_PLAYER_POSITION_READONLY_X, x + 10);

                Log.Print("MyMod", $"teleport_result: {teleport_result}");
            }
            else if (key == Keys.D2)
            {
                var x = NFSScript.Underground2.Player.Car.Position.X;

                var teleport_result = Memory.WriteFloat((IntPtr)Addrs.PlayerAddrs.STATIC_PLAYER_POSITION_READONLY_X, x - 10);

                Log.Print("MyMod", $"teleport_result: {teleport_result}");
            }
        }

        /// <summary>
        /// This method is called when the script is being terminated.
        /// </summary>
        public override void OnExit()
        {
            Log.Print("MyMod", "OnExit");
        }
    }
}
