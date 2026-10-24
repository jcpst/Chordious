// Copyright (c) Jon Thysell <http://jonthysell.com>
// Licensed under the MIT License.

using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading;

using Avalonia;

using Chordious.Avalonia.Resources;

namespace Chordious.Avalonia
{
    public class Program
    {
        private static Mutex _mutex;

        [STAThread]
        public static int Main(string[] args)
        {
            // Set user file from command-line args
            if (args is not null && args.Length > 0)
            {
                App.UserConfigPathOverride = args[0];
            }

            _mutex = new Mutex(true, GetMutexName(), out bool createdNew);
            if (!createdNew)
            {
                Console.Error.WriteLine(Strings.ChordiousAlreadyRunningErrorMessage);
                return 1;
            }

            try
            {
                return BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(string.Format(Strings.ChordiousUnhandledExceptionMessageFormat, ex.Message));
                Console.Error.WriteLine(ex.StackTrace);
                return 1;
            }
            finally
            {
                _mutex.ReleaseMutex();
            }
        }

        // Used by the Avalonia designer as well
        public static AppBuilder BuildAvaloniaApp()
        {
            return AppBuilder.Configure<App>()
                             .UsePlatformDetect()
                             .LogToTrace();
        }

        private static string GetMutexName()
        {
            // Named mutexes can't contain path separators on Unix, so use a stable hash of the path
            // (string.GetHashCode is randomized per process, so it can't be used here)
            string configPath = Path.GetFullPath(App.GetUserConfigPath(App.UserConfigPathOverride)).ToLowerInvariant();
            string hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(configPath)))[..16];
            return string.Format("{0}.{1}", MutexBase, hash);
        }

        private const string MutexBase = "Chordious.Avalonia";
    }
}
