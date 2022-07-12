using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace Wpf
{
   public class Converters
    {
        public class PointToPositionConverter : IValueConverter
        {
            public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            {
                Point point = new Point();
                if (value is Position pos)
                {
                    point = new Point(pos.X, pos.Y);
                }
                return point;
            }

            public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            {
                Position pos = new Position();
                if (value is Point p)
                {
                    pos = new Position((int)p.Y, (int)p.X);
                }
                return pos;
            }
        }
    }
}
