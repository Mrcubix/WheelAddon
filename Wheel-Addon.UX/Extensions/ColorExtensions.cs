using System;
using System.Drawing;

namespace WheelAddon.UX.Extensions
{
    public static class ColorExtensions
    {
        public static string ToHex(this Color color)
        {
            return $"#{color.R:X2}{color.G:X2}{color.B:X2}";
        }

        /// <summary>
        ///     Generates a random color
        /// </summary>
        /// <returns></returns>
        public static Color RandomColor()
        {
            var random = new Random();
            return Color.FromArgb(random.Next(0, 255), random.Next(0, 255), random.Next(0, 255));
        }
    }
}