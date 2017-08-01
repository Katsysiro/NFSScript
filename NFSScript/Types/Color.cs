using System;
using System.Globalization;
using System.Text;
using NFSScript.Math;

namespace NFSScript
{
    /// <summary>
    /// Representation of RGBA colors with floating points values with a range from 0 to 1.
    /// </summary>
    public struct Color
    {
        /// <summary>
        /// Alpha value of the color.
        /// </summary>
        public float a { get; set; }

        /// <summary>
        /// Blue value of the color.
        /// </summary>
        public float b { get; set; }

        /// <summary>
        /// Green value of the color.
        /// </summary>
        public float g { get; set; }

        /// <summary>
        /// Red value of the color.
        /// </summary>
        public float r { get; set; }

        /// <summary>
        /// The color black.
        /// </summary>
        public static Color black => new Color(0.0f, 0.0f, 0.0f, 1f);

        /// <summary>
        /// The color blue.
        /// </summary>
        public static Color blue => new Color(0.0f, 0.0f, 1f, 1f);

        /// <summary>
        /// Transparent.
        /// </summary>
        public static Color clear => new Color(0.0f, 0.0f, 0.0f, 0.0f);

        /// <summary>
        /// The color cyan.
        /// </summary>
        public static Color cyan => new Color(0.0f, 1f, 1f, 1f);

        /// <summary>
        /// The color gray for the Americans.
        /// </summary>
        public static Color gray => new Color(0.5f, 0.5f, 0.5f, 1f);

        /// <summary>
        /// The color green.
        /// </summary>
        public static Color green => new Color(0.0f, 1f, 0.0f, 1f);

        /// <summary>
        /// The color grey for the Englishmen.
        /// </summary>
        public static Color grey => new Color(0.5f, 0.5f, 0.5f, 1f);

        /// <summary>
        /// The color magenta.
        /// </summary>
        public static Color magenta => new Color(1f, 0.0f, 1f, 1f);

        /// <summary>
        /// The color red.
        /// </summary>
        public static Color red => new Color(1f, 0.0f, 0.0f, 1f);

        /// <summary>
        /// The color white.
        /// </summary>
        public static Color white => new Color(1f, 1f, 1f, 1f);

        /// <summary>
        /// The color yellow.
        /// </summary>
        public static Color yellow => new Color(1f, 0.92f, 0.016f, 1f);

        /// <summary>
        /// Constructs a new Color with given RGB values.
        /// </summary>
        public Color(float r, float g, float b)
        {
            a = 1f;

            this.b = b <= 1.0 ? (b >= 0.0 ? b : 0.0f) : 1f;
            this.g = g <= 1.0 ? (g >= 0.0 ? g : 0.0f) : 1f;
            this.r = r <= 1.0 ? (r >= 0.0 ? r : 0.0f) : 1f;
        }

        /// <summary>
        /// Constructs a new Color with given RGBA values.
        /// </summary>
        public Color(float r, float g, float b, float a)
        {
            this.a = a <= 1.0 ? (a >= 0.0 ? a : 0.0f) : 1f;
            this.b = b <= 1.0 ? (b >= 0.0 ? b : 0.0f) : 1f;
            this.g = g <= 1.0 ? (g >= 0.0 ? g : 0.0f) : 1f;
            this.r = r <= 1.0 ? (r >= 0.0 ? r : 0.0f) : 1f;
        }

        /// <summary>
        /// Linearly interpolates between two colors. 
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="factor"></param>
        /// <returns></returns>
        public static Color Lerp(Color start, Color end, float factor)
        {
            var r = start.r + ((end.r - start.r) * factor);
            var g = start.g + ((end.g - start.g) * factor);
            var b = start.b + ((end.b - start.b) * factor);

            return new Color(r, g, b);
        }

        private static float Lerp(float v0, float v1, float t)
        {
            return v0 * (1 - t) + v1 * t;
        }

        /// <summary>
        /// Converts the current color to color with 32-bit format.
        /// </summary>
        public Color32 ToColor32()
        {
            return new Color32(Mathf.FloatToByte(r), Mathf.FloatToByte(g), Mathf.FloatToByte(b), Mathf.FloatToByte(a));
        }

        /// <summary>
        /// Returns a formatted string of this color.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            var sb = new StringBuilder();

            sb.Append("R: ");
            sb.Append(r);
            sb.Append(" G: ");
            sb.Append(g);
            sb.Append(" B: ");
            sb.Append(b);

            return sb.ToString();
        }

        /// <summary>
        /// Returns a color from hex.
        /// </summary>
        /// <param name="hex"></param>
        /// <returns></returns>
        public static Color FromHex(string hex)
        {
            try
            {
                var argb = int.Parse(hex.Replace("#", "").Replace("0x", ""), NumberStyles.HexNumber);
                return new Color32((byte)((argb & -16777216) >> 0x18),
                                      (byte)((argb & 0xff0000) >> 0x10),
                                      (byte)((argb & 0xff00) >> 8),
                                      (byte)(argb & 0xff)).ToColor();
            }
            catch
            {
                throw new Exception($"{hex} is not a valid color");
            }
        }
    }
}
