using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Challenge
{
    class Position
    {
        public int Y { get; set; }
        public int X { get; set; }
        public Position(int y, int x)
        {
            Y = y;
            X = x;
        }

        public Position Up()
        {
            return new Position(Y - 1, X);
        }
        public Position Down()
        {
            return new Position(Y + 1, X);
        }
        public Position Left()
        {
            return new Position(Y, X - 1);
        }
        public Position Right()
        {
            return new Position(Y, X + 1);
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
