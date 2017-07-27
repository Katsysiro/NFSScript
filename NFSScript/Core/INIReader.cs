using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

// Took it from StackOverflow because I couldn't be bothered writing my own .INI file.
namespace NFSScript
{
    /// <summary>
    /// A class for reading and writing INI files
    /// </summary>
    public class IniFile
    {
        /// <summary>
        /// Returns the path of the INIFile
        /// </summary>
        public string Path { get; private set; }

        private readonly string _exe = Assembly.GetExecutingAssembly().GetName().Name;

        [DllImport("kernel32", CharSet = CharSet.Unicode)]
        private static extern long WritePrivateProfileString(string section, string key, string value, string filePath);

        [DllImport("kernel32", CharSet = CharSet.Unicode)]
#pragma warning disable 1591
        public static extern int GetPrivateProfileString(string section, string key, string Default, StringBuilder retVal, int size, string filePath);
#pragma warning restore 1591

        /// <summary>
        /// Instantiate an INIFile class
        /// </summary>
        /// <param name="iniPath"></param>
        public IniFile(string iniPath = null)
        {
            Path = new FileInfo(iniPath ?? _exe + ".ini").FullName;
        }

        /// <summary>
        /// Read a key from the INI file
        /// </summary>
        /// <param name="key"></param>
        /// <param name="section"></param>
        /// <returns></returns>
        public string Read(string key, string section = null)
        {
            var retVal = new StringBuilder(255);
            GetPrivateProfileString(section ?? _exe, key, "", retVal, 255, Path);
            return retVal.ToString();
        }

        /// <summary>
        /// Write a key to the ini file
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="section"></param>
        public void Write(string key, string value, string section = null)
        {
            WritePrivateProfileString(section ?? _exe, key, value, Path);
        }

        /// <summary>
        /// Delete a key from the ini file
        /// </summary>
        /// <param name="key"></param>
        /// <param name="section"></param>
        public void DeleteKey(string key, string section = null)
        {
            Write(key, null, section ?? _exe);
        }

        /// <summary>
        /// Delete a section
        /// </summary>
        /// <param name="section"></param>
        public void DeleteSection(string section = null)
        {
            Write(null, null, section ?? _exe);
        }

        /// <summary>
        /// Returns true if the key exists
        /// </summary>
        /// <param name="key"></param>
        /// <param name="section"></param>
        /// <returns></returns>
        public bool KeyExists(string key, string section = null)
        {
            return Read(key, section).Length > 0;
        }
    }
}
