// Copyright (c) Jon Thysell <http://jonthysell.com>
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;

using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.LogicalTree;
using Avalonia.Platform;

namespace Chordious.Avalonia
{
    /// <summary>
    /// Base class for all Chordious windows.
    /// </summary>
    public class ChordiousWindow : Window
    {
        protected override Type StyleKeyOverride => typeof(Window);

        /// <summary>
        /// Name of the control to focus when the window opens (replaces WPF's FocusManager.FocusedElement).
        /// </summary>
        public string InitialFocus { get; set; }

        public ChordiousWindow()
        {
            Icon = AppIcon;
        }

        protected override void OnOpened(EventArgs e)
        {
            base.OnOpened(e);

            if (OperatingSystem.IsMacOS())
            {
                AddMacKeyBindings();
            }

            if (!string.IsNullOrWhiteSpace(InitialFocus) && this.FindControl<Control>(InitialFocus) is Control control)
            {
                control.Focus();
            }
        }

        /// <summary>
        /// The shortcuts are authored with Ctrl, so mirror them with Cmd on macOS.
        /// </summary>
        private void AddMacKeyBindings()
        {
            IEnumerable<InputElement> elements = new[] { this }.Concat(this.GetLogicalDescendants().OfType<InputElement>());
            foreach (InputElement element in elements)
            {
                List<KeyBinding> macBindings = new List<KeyBinding>();
                foreach (KeyBinding binding in element.KeyBindings)
                {
                    if (binding.Gesture is KeyGesture gesture && gesture.KeyModifiers.HasFlag(KeyModifiers.Control))
                    {
                        KeyModifiers modifiers = (gesture.KeyModifiers & ~KeyModifiers.Control) | KeyModifiers.Meta;
                        macBindings.Add(new KeyBinding()
                        {
                            Gesture = new KeyGesture(gesture.Key, modifiers),
                            Command = new ForwardingCommand(binding),
                        });
                    }
                }
                element.KeyBindings.AddRange(macBindings);
            }
        }

        /// <summary>
        /// Executes whatever command the original key binding is currently bound to.
        /// </summary>
        private class ForwardingCommand : ICommand
        {
            private readonly KeyBinding _source;

            public ForwardingCommand(KeyBinding source)
            {
                _source = source;
            }

            public event EventHandler CanExecuteChanged
            {
                add { }
                remove { }
            }

            public bool CanExecute(object parameter) => _source.Command?.CanExecute(_source.CommandParameter) ?? false;

            public void Execute(object parameter) => _source.Command?.Execute(_source.CommandParameter);
        }

        private static WindowIcon AppIcon => _appIcon ??= new WindowIcon(AssetLoader.Open(new Uri("avares://Chordious/Resources/chordious.ico")));
        private static WindowIcon _appIcon;
    }
}
