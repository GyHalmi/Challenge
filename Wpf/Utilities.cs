using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Wpf
{
    static class Utilities
    {
        //extension methods
        public static Position ToPosition(this Point point)
        {
            return new Position((int)point.Y, (int)point.X);
        }
        public static Point ToPoint(this Position position)
        {
            return new Point(position.X, position.Y);
        }

        //converters
        public class ConverterReverseBool : IValueConverter
        {
            public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            {
                if(targetType != typeof(bool)) throw new InvalidOperationException("The target must be a bool");
                return !(bool)value;
            }

            public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            {
                throw new NotSupportedException();
            }
        }

        public class ConverterObjectIsNullToBool : IValueConverter
        {
            public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            {
                return value != null;
            }

            public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            {
                throw new NotSupportedException();
            }
        }

        public class ConverterSelectedItemToItsContent : IValueConverter
        {
            public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            {
                object content = null;
                if(value != null)
                {
                    content = ((ListBoxItem)value).Content;
                }
                return content;
            }

            public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            {
                return value;
            }
        }

    }
}
