using System;
using System.IO;
using System.Text;

namespace NFSScript
{
    /// <summary>
    /// Class for sending log output
    /// </summary>
    public static class Log
    {
        private const string FileName = "NFSScriptLog";
        private const string FileExtension = "log";
        private const string DateFormat = "MM-dd-yyyy";

        /// <summary>
        /// Gets the log file name
        /// </summary>
        /// <returns></returns>
        public static string GetFileName()
        {
            var sb = new StringBuilder();
            sb.Append(FileName);
            sb.Append(" ");
            sb.Append(DateTime.Now.ToString(DateFormat));
            sb.Append(".");
            sb.Append(FileExtension);

            return sb.ToString();
        }

        /// <summary>
        /// Print a log message.
        /// </summary>
        /// <param name="tag"></param>
        /// <param name="message"></param>
        public static void Print(string tag, string message)
        {

            var output = $"({DateTime.Now.ToString("u").Replace("Z", string.Empty)}) {tag}: {message}";
            Console.WriteLine(output);

            using (var file = new StreamWriter(GetFileName(), true))
            {
                file.WriteLine(output);
            }
        }

        /// <summary>
        /// Prints a debug message if <seealso cref="NFSScript.DEBUG"/> is set to true.
        /// </summary>
        /// <param name="message"></param>
        public static void Debug(string message)
        {
            if (NFSScript.DEBUG)
            {
                var output = $"({DateTime.Now.ToString("u").Replace("Z", string.Empty)}) DEBUG: {message}";
                Console.WriteLine(output);

                using (var file = new StreamWriter(GetFileName(), true))
                {
                    file.WriteLine(output);
                }
            }
        }
    }
}
