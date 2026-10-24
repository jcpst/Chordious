// Copyright (c) Jon Thysell <http://jonthysell.com>
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Threading;

using CommunityToolkit.Mvvm.Messaging;

using Chordious.Core.ViewModel;
using Chordious.Avalonia.Resources;

namespace Chordious.Avalonia
{
    public partial class App : Application, IAppView
    {
        public static AppViewModel AppVM => AppViewModel.Instance;

        public static string UserConfigPathOverride { get; set; }

        public string UserConfigPath { get; private set; }

        private bool _saveConfigOnExit = true;

        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            MessageHandlers.RegisterMessageHandlers(this);

            UserConfigPath = GetUserConfigPath(UserConfigPathOverride, true);

            AppViewModel.Init(Assembly.GetEntryAssembly(), this, UserConfigPath);

            AppVM.LoadAppConfig();

            if (!File.Exists(UserConfigPath))
            {
                // Makes sure that LoadUserConfig will be successful
                AppVM.SaveUserConfig();
            }

            AppVM.LoadUserConfig(HandleUserConfigLoadException);

            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.ShutdownMode = ShutdownMode.OnMainWindowClose;
                desktop.MainWindow = new MainWindow();
                desktop.Exit += Desktop_Exit;
            }

            base.OnFrameworkInitializationCompleted();
        }

        public static string GetUserConfigPath(string userConfigPath)
        {
            return GetUserConfigPath(userConfigPath, false);
        }

        private static string GetUserConfigPath(string userConfigPath, bool setup)
        {
            if (!string.IsNullOrWhiteSpace(userConfigPath))
            {
                return userConfigPath.Trim();
            }

            // Roaming AppData on Windows (matches the WPF build), ~/Library/Application Support on macOS, ~/.local/share on Linux
            string userFolder = Environment.GetFolderPath(OperatingSystem.IsWindows() ? Environment.SpecialFolder.ApplicationData : Environment.SpecialFolder.LocalApplicationData);
            userFolder = Path.Combine(userFolder, "Chordious");

            if (setup && !Directory.Exists(userFolder))
            {
                Directory.CreateDirectory(userFolder);
            }

            return Path.Combine(userFolder, DefaultUserConfigFileName);
        }

        private const string DefaultUserConfigFileName = "Chordious.User.xml";

        public void HandleUserConfigLoadException(Exception ex)
        {
            string userFile = UserConfigPath;
            StrongReferenceMessenger.Default.Send(new ConfirmationMessage(Strings.ResetAndBackupUserConfigConfirmationMessage, (confirmed) =>
            {
                if (!confirmed) // No, exit app
                {
                    _saveConfigOnExit = false; // So we don't overwrite the corrupt config with whatever we may have loaded
                    (ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)?.Shutdown(1);
                }
                else // Yes, backup and continue
                {
                    string backupFile = ResetAndBackupUserConfig(userFile);
                    string message = string.Format(Strings.ResetAndBackupUserConfigBackupFileMessageFormat, backupFile);
                    ExceptionUtils.HandleException(new Exception(message, ex));
                }
            }));
        }

        private static string ResetAndBackupUserConfig(string userFile)
        {
            if (string.IsNullOrWhiteSpace(userFile))
            {
                throw new ArgumentNullException(nameof(userFile));
            }

            string userFolder = Path.GetDirectoryName(userFile);
            string userFileName = Path.GetFileNameWithoutExtension(userFile);
            string userFileExt = Path.GetExtension(userFile);

            string backupFile = Path.Combine(userFolder, string.Format("{0}.{1:yyyy.MM.dd.HH.mm.ss}{2}", userFileName, DateTime.UtcNow, userFileExt));

            File.Move(userFile, backupFile);

            AppVM.ResetUserConfig();
            AppVM.SaveUserConfig();

            return backupFile;
        }

        private void Desktop_Exit(object sender, ControlledApplicationLifetimeExitEventArgs e)
        {
            try
            {
                if (_saveConfigOnExit)
                {
                    AppVM.Close();
                }
                MessageHandlers.UnregisterMessageHandlers(this);
            }
            catch (Exception ex)
            {
                ExceptionUtils.HandleException(ex);
            }
        }

        #region IAppView

        public void DoOnUIThread(Action action)
        {
            Dispatcher.UIThread.Invoke(action);
        }

        public object DoOnUIThread(Func<object> func)
        {
            return Dispatcher.UIThread.Invoke(func);
        }

        public Stream GetAppConfigStream()
        {
            return typeof(App).GetTypeInfo().Assembly.GetManifestResourceStream("Chordious.Avalonia.Chordious.Avalonia.xml");
        }

        public Stream GetUserConfigStreamToRead()
        {
            return new FileStream(UserConfigPath, FileMode.Open);
        }

        public Stream GetUserConfigStreamToWrite()
        {
            return new FileStream(UserConfigPath, FileMode.Create);
        }

        public object SvgTextToImage(string svgText, int width, int height, bool editMode)
        {
            return ImageUtils.SvgTextToDisplayImage(svgText, width, height, editMode);
        }

        public void TextToClipboard(string text)
        {
            IntegrationUtils.TextToClipboard(text);
        }

        public void DiagramToClipboard(ObservableDiagram diagram, float scaleFactor)
        {
            IntegrationUtils.DiagramToClipboard(diagram, scaleFactor);
        }

        public IEnumerable<string> GetSystemFonts()
        {
            return FontManager.Current.SystemFonts.Select(f => f.Name).OrderBy(n => n, StringComparer.CurrentCultureIgnoreCase);
        }

        #endregion
    }
}
