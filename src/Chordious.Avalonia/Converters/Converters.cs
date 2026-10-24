// Copyright (c) Jon Thysell <http://jonthysell.com>
// Licensed under the MIT License.

using System;
using System.Globalization;

using Avalonia.Data.Converters;
using Avalonia.Input;
using Avalonia.Media;

namespace Chordious.Avalonia
{
    public class AltTextToTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string sValue)
            {
                return sValue.Replace("_", "");
            }

            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }

    public class IdleBoolToWaitCursorConverter : IValueConverter
    {
        private static readonly Cursor WaitCursor = new Cursor(StandardCursorType.Wait);

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isIdle && !isIdle)
            {
                return WaitCursor;
            }

            return Cursor.Default;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }

    public class InverseBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool b)
            {
                return !b;
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Convert(value, targetType, parameter, culture);
        }
    }

    public class BoolToBoldConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is bool b && b ? FontWeight.Bold : FontWeight.Normal;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
