using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace NFSScript
{
    /// <summary>
    /// A class for extension methods
    /// </summary>
    public static class ExtensionMethods
    {
        /// <summary>
        /// Converts a boolean to byte.
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        public static byte ToByte(this bool x)
        {
            // ReSharper disable once ConvertIfStatementToReturnStatement
            // This must have explicit constant return or else it'll require a cast which is slower
            if (x)
                return 1;
            return 0;
        }

        /// <summary>
        /// Returns <typeparamref name="T"/> instance of the <paramref name="byteArray"/>.
        /// </summary>
        /// <param name="byteArray"></param>
        /// <returns></returns>
        public static T GetObject<T>(this byte[] byteArray)
        {
            // ReSharper disable RedundantCaseLabel
            switch (Type.GetTypeCode(typeof(T)))
            {
                case TypeCode.Boolean:
                    return (T)(object)BitConverter.ToBoolean(byteArray, 0);
                case TypeCode.Byte:
                    return (T)(object)byteArray[0];
                case TypeCode.Char:
                    return (T)(object)Encoding.UTF8.GetChars(byteArray)[0];
                case TypeCode.Double:
                    return (T)(object)BitConverter.ToDouble(byteArray, 0);
                case TypeCode.Int16:
                    return (T)(object)BitConverter.ToInt16(byteArray, 0);
                case TypeCode.Int32:
                    return (T)(object)BitConverter.ToInt32(byteArray, 0);
                case TypeCode.Int64:
                    return (T)(object)BitConverter.ToInt64(byteArray, 0);
                case TypeCode.Single:
                    return (T)(object)BitConverter.ToSingle(byteArray, 0);
                case TypeCode.UInt16:
                    return (T)(object)BitConverter.ToUInt16(byteArray, 0);
                case TypeCode.UInt32:
                    return (T)(object)BitConverter.ToUInt32(byteArray, 0);
                case TypeCode.UInt64:
                    return (T)(object)BitConverter.ToUInt64(byteArray, 0);
                case TypeCode.Empty:
                case TypeCode.Object:
                case TypeCode.DBNull:
                case TypeCode.SByte:
                case TypeCode.Decimal:
                case TypeCode.DateTime:
                case TypeCode.String:
                default:
                    return default(T);
            }
            // ReSharper restore RedundantCaseLabel
        }

        /// <summary>
        /// Returns an initialized byte[] of the given <typeparamref name="T"/> object.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static byte[] GetBytes<T>(this T obj)
        {
            switch (Type.GetTypeCode(typeof(T)))
            {
                case TypeCode.Boolean:
                    return BitConverter.GetBytes((bool)(object)obj);
                case TypeCode.Char:
                    return Encoding.UTF8.GetBytes(new[] { (char)(object)obj });
                case TypeCode.Double:
                    return BitConverter.GetBytes((double)(object)obj);
                case TypeCode.Int16:
                    return BitConverter.GetBytes((short)(object)obj);
                case TypeCode.Int32:
                    return BitConverter.GetBytes((int)(object)obj);
                case TypeCode.Int64:
                    return BitConverter.GetBytes((long)(object)obj);
                case TypeCode.Single:
                    return BitConverter.GetBytes((float)(object)obj);
                case TypeCode.UInt16:
                    return BitConverter.GetBytes((ushort)(object)obj);
                case TypeCode.UInt32:
                    return BitConverter.GetBytes((uint)(object)obj);
                case TypeCode.UInt64:
                    return BitConverter.GetBytes((ulong)(object)obj);
            }

            return new byte[1];
        }

        /*
        /// <summary>
        /// Search for a subarray inside of a larger array. For example:
        /// <c>{a, b, c}</c>
        /// will match <c>{a, b}</c> and <c>{c}</c> but not <c>{a, c}</c>
        /// </summary>
        /// <param name="arr">The initial array to search</param>
        /// <param name="subarr">The subarray to look for</param>
        /// <typeparam name="T">The array's element type</typeparam>
        /// <returns></returns>
        public static bool Contains<T>(this T[] arr, T[] subarr)
        {
            // ReSharper disable once ConvertIfStatementToSwitchStatement
            if (subarr.Length == 0) return true;
            if (subarr.Length == 1) return Array.IndexOf(arr, subarr[0]) != -1;
            for (var i = 0; i < arr.Length; i++)
            {
                // since we can't use == with generics, lets compare primitive (non generic) == with #Equals:
                //== operator                            1974380 ticks
                //Equals()                               1976358 ticks
                //== operator in another static function 1974604 ticks
                //EqualityComparer<int>.Default...      32486695 ticks
                // so itll do
                // i wonder though, is ReferenceEquals faster? does it even work with primitives/structs?
                if (arr[i].Equals(subarr[0]))
                {
                    for (var j = 1; j < subarr.Length; j++)
                    {
                        if (!Equals(arr[i + j], subarr[j])) goto contin;
                    }
                    return true;
                }
                // yeah, this is bad, but i come from a Java standpoint, if you have a better idea go ahead
                contin: {}
            }
            return false;
        }
        */
        
        /// <summary>
        /// Search for a subarray inside of a larger array. For example:
        /// <c>{a, b, c}</c>
        /// will match <c>{a, b}</c> and <c>{c}</c> but not <c>{a, c}</c>
        /// </summary>
        /// <param name="arr">The initial array to search</param>
        /// <param name="subarr">The subarray to look for</param>
        /// <typeparam name="T">The array's element type</typeparam>
        /// <returns>True if the subarray was found, false otherwise</returns>
        public static bool Contains<T>(this T[] arr, T[] subarr)
        {
            // adaptation of https://stackoverflow.com/a/283648 for single index/generic
            if (arr == null || subarr == null || subarr.Length > arr.Length)
            {
                return false;
            }
            if (arr.Length == subarr.Length)
            {
                return arr.Length == 0 || arr.Equals(subarr);
            }
            if (subarr.Length == 0)
            {
                return true;
            }
            // ReSharper disable once LoopCanBeConvertedToQuery for performance!
            for (var i = 0; i < arr.Length; i++)
            {
                if (!IsMatch (arr, i, subarr))
                    continue;

                return true;
            }

            return false;
        }
        
        /// <summary>
        /// Search for any of X subarrays inside of a larger array. For example:
        /// <c>{a, b, c}</c>
        /// will match <c>{a, b}</c> and <c>{c}</c> but not <c>{a, c}</c>
        /// </summary>
        /// <param name="arr">The initial array to search</param>
        /// <param name="subarrays">The subarrays to look for</param>
        /// <typeparam name="T">The array's element type</typeparam>
        /// <returns>The first subarray found, null if none found</returns>
        public static T[] ContainsWhich<T>(this T[] arr, params T[][] subarrays)
        {
            // adaptation of https://stackoverflow.com/a/283648 for single index/generic
            if (arr == null || subarrays == null)
            {
                return null;
            }
            // ReSharper disable once ConvertIfStatementToSwitchStatement
            if (subarrays.Length == 0)
            {
                return new T[]{};
            }
            if (subarrays.Length == 1)
            {
                return Contains(arr, subarrays[0])? subarrays[0] : null;
            }
            
            var blacklist = new bool[subarrays.Length];

            for (var i = 0; i < subarrays.Length; i++)
            {
                var subarr = subarrays[i];
                if (arr.Length == subarr.Length)
                {
                    if (arr.Length == 0 || arr.Equals(subarrays))
                    {
                        return subarr;
                    }
                }
                if (subarr.Length == 0)
                {
                    return subarr;
                }
                if (subarr.Length > arr.Length)
                {
                    blacklist[i] = true;
                }
            }

            // ReSharper disable once LoopCanBeConvertedToQuery for performance!
            for (var j = 0; j < subarrays.Length; j++)
            {
                var subarr = subarrays[j];
                if (blacklist[j])
                {
                    continue;
                }
                for (var i = 0; i < arr.Length; i++)
                {
                    if (!IsMatch(arr, i, subarr))
                        continue;

                    return subarr;
                }
            }

            return null;
        }

        /// <summary>
        /// Search for a byte array inside of a stream. For example:<br/>
        /// For a Stream containing <c>{a, b, c}</c>,
        /// will match <c>{a, b}</c> and <c>{c}</c> but not <c>{a, c}</c>
        /// </summary>
        /// <param name="stream">The stream to look in</param>
        /// <param name="subarr">The byte sequence to look for</param>
        /// <returns>The byte sequence, or null if none found</returns>
        public static bool Contains(this Stream stream, params byte[] subarr)
        {
            var buffer = new byte[2048]; // read in chunks of 2KB

            var progress = 0;
            
            int bytesRead;
            while((bytesRead = stream.Read(buffer, 0, buffer.Length)) > 0)
            {
                for (var i = 0; i < bytesRead; i++)
                {
                    if (subarr[progress] == buffer[i])
                    {
                        // chain continue
                        progress++;

                        if (progress != subarr.Length) continue;

                        // chain finish. we found it!
                        stream.Close();
                        return true;
                    }
                    
                    // chain broken
                    progress = 0;
                    // no break: keep reading buffer for maybe another chain.
                    //break;
                }
            }
            
            // nothing found, leave...
            stream.Close();
            return false;
        }
        
        /// <summary>
        /// Search for any of X byte arrays inside of a stream. For example:<br/>
        /// For a Stream containing <c>{a, b, c}</c>,
        /// will match <c>{a, b}</c> and <c>{c}</c> but not <c>{a, c}</c>
        /// </summary>
        /// <param name="stream">The stream to look in</param>
        /// <param name="subarrays">The byte sequences to look for</param>
        /// <returns>The byte sequence, or null if none found</returns>
        public static byte[] ContainsWhich(this Stream stream, params byte[][] subarrays)
        {
            var buffer = new byte[2048]; // read in chunks of 2KB

            var progress = new int[subarrays.Length];
            
            int bytesRead;
            while((bytesRead = stream.Read(buffer, 0, buffer.Length)) > 0)
            {
                for (var si = 0; si < subarrays.Length; si++)
                {
                    var subarr = subarrays[si];
                    for (var i = 0; i < bytesRead; i++)
                    {
                        if (subarr[progress[si]] == buffer[i])
                        {
                            // chain continue
                            progress[si]++;
                            
                            if (progress[si] != subarr.Length) continue;
                            
                            // chain finish. we found it!
                            stream.Close();
                            return subarr;
                        }
                        
                        // chain broken
                        progress[si] = 0;
                        // no break: keep reading buffer for maybe another chain.
                        //break;
                    }
                }
            }
            
            // nothing found, leave...
            stream.Close();
            return null;
        }
        
        private static bool IsMatch<T>(IList<T> array, int position, IList<T> candidate)
        {
            if (candidate.Count > (array.Count - position))
                return false;

            // ReSharper disable once LoopCanBeConvertedToQuery for performance!
            for (var i = 0; i < candidate.Count; i++)
                if (!Equals(array[position + i], candidate[i]))
                    return false;

            return true;
        }

        /// <summary>
        /// Turns an array to a string, joining with commas.
        /// </summary>
        /// <param name="arr">The array to convert</param>
        /// <typeparam name="T">The element type of the array</typeparam>
        /// <returns>String representation of the array</returns>
        public static string ArrToStr<T>(this T[] arr)
        {
            return string.Join(",", arr);
        }
    }
}
