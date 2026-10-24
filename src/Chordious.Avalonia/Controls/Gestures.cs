// Copyright (c) Jon Thysell <http://jonthysell.com>
// Licensed under the MIT License.

using System.Windows.Input;

using Avalonia;
using Avalonia.Input;
using Avalonia.Interactivity;

namespace Chordious.Avalonia
{
    /// <summary>
    /// Replaces WPF's MouseBinding Gesture="LeftDoubleClick".
    /// </summary>
    public static class Gestures
    {
        public static readonly AttachedProperty<ICommand> DoubleTapCommandProperty =
            AvaloniaProperty.RegisterAttached<InputElement, ICommand>("DoubleTapCommand", typeof(Gestures));

        public static readonly AttachedProperty<object> DoubleTapCommandParameterProperty =
            AvaloniaProperty.RegisterAttached<InputElement, object>("DoubleTapCommandParameter", typeof(Gestures));

        static Gestures()
        {
            DoubleTapCommandProperty.Changed.AddClassHandler<InputElement>((element, e) =>
            {
                element.DoubleTapped -= Element_DoubleTapped;
                if (e.NewValue is not null)
                {
                    element.DoubleTapped += Element_DoubleTapped;
                }
            });
        }

        public static ICommand GetDoubleTapCommand(InputElement element) => element.GetValue(DoubleTapCommandProperty);

        public static void SetDoubleTapCommand(InputElement element, ICommand value) => element.SetValue(DoubleTapCommandProperty, value);

        public static object GetDoubleTapCommandParameter(InputElement element) => element.GetValue(DoubleTapCommandParameterProperty);

        public static void SetDoubleTapCommandParameter(InputElement element, object value) => element.SetValue(DoubleTapCommandParameterProperty, value);

        private static void Element_DoubleTapped(object sender, TappedEventArgs e)
        {
            if (sender is InputElement element)
            {
                ICommand command = GetDoubleTapCommand(element);
                object parameter = GetDoubleTapCommandParameter(element);
                if (command is not null && command.CanExecute(parameter))
                {
                    command.Execute(parameter);
                    e.Handled = true;
                }
            }
        }
    }
}
