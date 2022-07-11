using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

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

    }
}
