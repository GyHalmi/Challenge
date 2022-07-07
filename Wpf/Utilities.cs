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
        public static void SetUID(UIElement uiElement, int y, int x)
        {
            uiElement.Uid = $"{y},{x}";
        }

        public static Position ReadUID(UIElement uiElement)
        {
            int[] yx = uiElement.Uid.Split(',').Select(n => int.Parse(n)).ToArray();
            return new Position(yx[0], yx[1]);
        }

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
