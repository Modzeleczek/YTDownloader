using System;
using System.Collections.Generic;

namespace YTDownloader
{
    /// <summary>
    /// Contains extension methods of String class.
    /// </summary>
    public static class StringExtensions
    {
        /// <summary>
        /// Returns a list containing indexes of all beginnings of the specified string.
        /// </summary>
        /// <param name="str">String, where to search.</param>
        /// <param name="value">String, whose occurrences' positions should be returned.</param>
        /// <returns>List containing indexes of all beginnings of the specified string.</returns>
        public static List<int> AllIndexesOf(this string str, string value)
        {
            if (String.IsNullOrEmpty(value))
                throw new ArgumentException("The string to find may not be empty.", "value");
            List<int> indexes = new List<int>();
            for (int index = 0; ; index += value.Length)
            {
                index = str.IndexOf(value, index);
                if (index == -1)
                    return indexes;
                indexes.Add(index);
            }
        }
        /// <summary>
        /// Returns a list of substrings, which begin with specified string and end with specified character.
        /// </summary>
        /// <param name="str">String, where to search substrings.</param>
        /// <param name="beginning">String, which has to be at the beginning of a returned substring.</param>
        /// <param name="end">Character, which has to be the first character after a returned substring.</param>
        /// <returns>List of substrings, which begin with specified string and end with specified character.</returns>
        public static List<string> SubstringsBeginningAndEnding(this string str, string beginning, char end)
        {
            if (String.IsNullOrEmpty(beginning))
                throw new ArgumentException("The string at the beginning may not be empty.", "beginning");
            List<string> substrings = new List<string>();
            for (int index = 0; ;)
            {
                index = str.IndexOf(beginning, index);
                if (index == -1)
                    return substrings;
                int endIndex = str.IndexOf(end, index + beginning.Length);
                if (endIndex == -1)
                    return substrings;
                substrings.Add(str.Substring(index, endIndex - index));
                index = endIndex;
            }
        }
    }
}
