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

        //extension methods
        public static Point Left(this Point p)
        {
            return new Point(p.X - 1, p.Y);
        }
        public static Point Right(this Point p)
        {
            return new Point(p.X + 1, p.Y);
        }
        public static Point Up(this Point p)
        {
            return new Point(p.X, p.Y-1);
        }
        public static Point Down(this Point p)
        {
            return new Point(p.X, p.Y+1);
        }
    }
}
