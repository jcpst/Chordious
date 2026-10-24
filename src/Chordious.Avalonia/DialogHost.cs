// Copyright (c) Jon Thysell <http://jonthysell.com>
// Licensed under the MIT License.

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Threading;

namespace Chordious.Avalonia
{
    /// <summary>
    /// The shared view models expect WPF-style blocking dialogs (Send a message, the dialog shows,
    /// and Process() runs after it closes). Avalonia's ShowDialog is async, so we pump a nested
    /// dispatcher loop until the dialog closes to keep the same semantics on desktop.
    /// </summary>
    public static class DialogHost
    {
        public static Window ActiveWindow
        {
            get
            {
                if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
                {
                    return desktop.Windows.LastOrDefault(w => w.IsVisible) ?? desktop.MainWindow;
                }
                return null;
            }
        }

        public static void ShowDialog(Window window)
        {
            if (window is null)
            {
                throw new ArgumentNullException(nameof(window));
            }

            Window owner = ActiveWindow;

            using CancellationTokenSource cts = new CancellationTokenSource();
            window.Closed += (sender, e) => cts.Cancel();

            if (owner is not null && owner != window && owner.IsVisible)
            {
                _ = window.ShowDialog(owner);
            }
            else
            {
                window.Show();
            }

            Dispatcher.UIThread.MainLoop(cts.Token);
        }

        /// <summary>
        /// Blocks (while still pumping the UI) until the given task completes.
        /// Used for the async-only storage pickers, so callers can stay synchronous.
        /// </summary>
        public static T WaitFor<T>(Task<T> task)
        {
            if (!task.IsCompleted)
            {
                using CancellationTokenSource cts = new CancellationTokenSource();
                task.ContinueWith(t => cts.Cancel(), TaskScheduler.Default);
                Dispatcher.UIThread.MainLoop(cts.Token);
            }
            return task.GetAwaiter().GetResult();
        }

        public static void WaitFor(Task task)
        {
            WaitFor(task.ContinueWith(t => { t.GetAwaiter().GetResult(); return true; }, TaskScheduler.Default));
        }
    }
}
