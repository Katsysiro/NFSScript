using System;
using System.Diagnostics;
using NFSScript.Core;

namespace NFSScript
{
    /// <summary>
    /// 
    /// </summary>
    public static class Time
    {
        /// <summary>
        /// Returns NFSScript's run time in milliseconds.
        /// </summary>
        public static float Runtime => (float)(DateTime.UtcNow - Process.GetCurrentProcess().StartTime.ToUniversalTime()).TotalMilliseconds;

        /// <summary>
        /// Returns the game's run time in milliseconds.
        /// </summary>
        public static float GameRuntime => (float)(DateTime.UtcNow - GameMemory.Memory.GetMainProcess().StartTime.ToUniversalTime()).TotalMilliseconds;

        /// <summary>
        /// Returns a float that moves back and foruth.
        /// </summary>
        /// <param name="speed">How fast the ping pong occurs</param>
        /// <param name="t">Factor</param>
        /// <returns></returns>
        public static float PingPong(float speed, float t)
        {            
            return (float)(System.Math.Sin(t * speed) + 1) / 2.0f;
        }
    }
}
