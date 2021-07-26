using System;
using System.Linq;

namespace Sitegen.Extensions
{
    public static class StringExtensions
    {
        public static string TrimLines(this string str)
        {
            return String.Join(
                "\n",
                str.Split('\n').Select(s => s.Trim()));
        }
    }
}
