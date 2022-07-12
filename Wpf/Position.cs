using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Wpf
{
    struct Position
    {
        public static Position ConvertPointToPosition(Point point)
        {
            return new Position((int)point.Y, (int)point.X);
        }
        public static Point ConvertPositionToPoint(Position pos)
        {
            return new Point(pos.X, pos.Y);
        }

        public int Y { get; set; }
        public int X { get; set; }
        public Position(int y, int x)
        {
            Y = y;
            X = x;
        }
  
        public Position ShiftUp(int offset)
        {
            return new Position(Y - offset, X);
        }
        public Position ShiftUp()
        {
            return ShiftUp(1);
        }
        public Position ShiftDown(int offset)
        {
            return new Position(Y + offset, X);
        }
        public Position ShiftDown()
        {
            return ShiftDown(1);
        }
        public Position ShiftLeft(int offset)
        {
            return new Position(Y, X - offset);
        }
        public Position ShiftLeft()
        {
            return ShiftLeft(1);
        }
        public Position ShiftRight(int offset)
        {
            return new Position(Y, X + offset);
        }
        public Position ShiftRight()
        {
            return ShiftRight(1);
        }
       

        public static bool operator == (Position a, Position b)
        {
            return a.X == b.X && a.Y == b.Y;
        }
        public static bool operator != (Position a, Position b)
        {
            return !(a.X == b.X && a.Y == b.Y);
        }

        public override bool Equals(object other)
        {
            //Position pos = (Position)other;
            //return pos.Y == this.Y && pos.X == this.X;

            return other is Position p
                && p.Y == Y
                && p.X == X;
        }
        public override int GetHashCode() => (Y, X).GetHashCode();
    }
}
