// Copyright (c) Jon Thysell <http://jonthysell.com>
// Licensed under the MIT License.

using System;
using System.IO;

namespace Chordious.Avalonia
{
    public class FolderUtils
    {
        public static bool PromptForFolder(string defaultPath, out string value)
        {
            value = FileDialogs.PromptForFolder(defaultPath) ?? "";
            return !string.IsNullOrWhiteSpace(value);
        }

        public static string CleanTitle(string title)
        {
            if (string.IsNullOrWhiteSpace(title))
            {
                return "";
            }

            return ReplaceChars(title.Trim(), Path.GetInvalidFileNameChars());
        }

        public static string CleanPath(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentNullException(nameof(path));
            }

            path = path.Trim();

            bool isUnc = path.StartsWith(@"\\");
            bool containsFile = !string.IsNullOrEmpty(Path.GetFileName(path));

            string[] split = path.Trim().Split(Path.DirectorySeparatorChar);

            // Unix absolute paths start with the separator, so the first split part is empty
            bool isUnixRooted = !OperatingSystem.IsWindows() && path.StartsWith(Path.DirectorySeparatorChar);

            string cleanPath = isUnc ? @"\\" : isUnixRooted ? Path.DirectorySeparatorChar.ToString() : "";

            for (int i = 0; i < split.Length; i++)
            {
                string part = split[i].Trim();

                if (!string.IsNullOrWhiteSpace(part))
                {
                    if (i == 0 && !isUnc && !isUnixRooted) // First item, aka root with drive letter
                    {
                        cleanPath += ReplaceChars(split[i], Path.GetInvalidPathChars());
                    }
                    else
                    {
                        cleanPath += ReplaceChars(split[i], Path.GetInvalidFileNameChars());
                    }

                    cleanPath += Path.DirectorySeparatorChar;
                }
            }

            if (containsFile)
            {
                cleanPath = cleanPath.TrimEnd(Path.DirectorySeparatorChar);
            }

            return cleanPath;
        }

        public static string ReplaceChars(string source, char[] oldValues, char? newValue = null)
        {
            if (string.IsNullOrWhiteSpace(source))
            {
                throw new ArgumentNullException(nameof(source));
            }

            foreach (char oldValue in oldValues)
            {
                source = source.Replace(oldValue.ToString(), (newValue is null) ? "" : newValue.ToString());
            }

            return source;
        }
    }
}
