using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wpf
{
    enum Figure { UncleanedArea = 0, Wall = 1, CleanedArea = 8 };

    class Map
    {
        public static int[][] initCoordinates(int y, int x)
        {
            int[][] mapCoordinates;
            mapCoordinates = new int[y][];
            for (int i = 0; i < mapCoordinates.Length; i++)
            {
                mapCoordinates[i] = new int[x];
            }
            return mapCoordinates;
        }

        private Random rnd;
        public int[][] Coordinates { get; set; }
        public Position PositionRC { get; set; }
        public Map(int[][] coordinates, Position posRC)
        {
            rnd = new Random();
            Coordinates = coordinates;
            PositionRC = posRC;
        }

        public void PutRoboCleanerOnMap(Position pos, Heading headingRC)
        {
            PositionRC = pos;
            RefreshCoordinate(pos, (int)headingRC);
        }
        public void PutRoboCleanerOnMapRandom()
        {
            Heading headUp = Heading.Up;
            int xMax = 0;
            
            for (int i = 0; i < Coordinates.Length; i++)
            {
                if (Coordinates[i].Length > xMax) xMax = Coordinates[i].Length;
            }

            int y, x;
            do
            {
                y = rnd.Next(1, Coordinates.Length - 1);
                x = rnd.Next(1, xMax - 1);
            }
            while (search());

            PutRoboCleanerOnMap(new Position(y, x),headUp);

            bool search()
            {
                bool s = true;
                Position p = new Position(y, x);
                if (CoordinateFigureByPosition(p) == (int)Figure.UncleanedArea)
                {
                    int wall = (int)Figure.Wall;
                    if (CoordinateFigureByPosition(AreaOnTheFront(headUp, p)) == wall ||
                        CoordinateFigureByPosition(AreaOnTheLeft(headUp, p)) == wall ||
                        CoordinateFigureByPosition(AreaOnTheRight(headUp, p)) == wall ||
                        CoordinateFigureByPosition(AreaOnTheFront(Heading.Down, p)) == wall)
                    {
                        s = false;
                    }
                }
                return s;
            }
        }
        public void RefreshCoordinate(Position coordinate, int figure)
        {
            Coordinates[coordinate.Y][coordinate.X] = figure;
        }
        /// <summary>
        /// returns -1 if position is out of range
        /// </summary>
        /// <param name="pos"></param>
        /// <returns></returns>
        public void MoveOnMapAndUpdatePositionRC(Heading headingRC, Position newPosition)
        {
            RefreshCoordinate(PositionRC, (int)Figure.CleanedArea);
            RefreshCoordinate(newPosition, (int)headingRC);
            PositionRC = newPosition;
        }
        
        public int CoordinateFigureByPosition(Position pos)
        {
            int figure = -1;
            try
            {
                figure = Coordinates[pos.Y][pos.X];
            }
            catch (IndexOutOfRangeException)
            {
            }
            return figure;
        }
        public void Display()
        {
            for (int i = 0; i < Coordinates.Length; i++)
            {
                for (int j = 0; j < Coordinates[i].Length; j++)
                {
                    switch (Coordinates[i][j])
                    {
                        case (int)Heading.Up:
                            Console.ForegroundColor = ConsoleColor.Red;
                            break;
                        case (int)Heading.Down:
                            Console.ForegroundColor = ConsoleColor.Red;
                            break;
                        case (int)Heading.Left:
                            Console.ForegroundColor = ConsoleColor.Red;
                            break;
                        case (int)Heading.Right:
                            Console.ForegroundColor = ConsoleColor.Red;
                            break;
                        case (int)Figure.Wall:
                            Console.ForegroundColor = ConsoleColor.DarkGray;
                            break;
                        case (int)Figure.CleanedArea:
                            Console.ForegroundColor = ConsoleColor.Green;
                            break;
                        case (int)Figure.UncleanedArea:
                            Console.ForegroundColor = ConsoleColor.White;
                            break;
                        default:
                            Console.ForegroundColor = ConsoleColor.White;
                            break;
                    }
                    Console.Write(Coordinates[i][j]);
                }
                Console.WriteLine();
            }

        }

        public Position AreaOnTheLeft(Heading headingRC, Position basePosition)
        {
            Position pos = null;
            switch (headingRC)
            {
                case Heading.Up:
                    pos = new Position(basePosition.Y, basePosition.X - 1);
                    break;
                case Heading.Down:
                    pos = new Position(basePosition.Y, basePosition.X + 1);
                    break;
                case Heading.Left:
                    pos = new Position(basePosition.Y + 1, basePosition.X);
                    break;
                case Heading.Right:
                    pos = new Position(basePosition.Y - 1, basePosition.X);
                    break;
                    //default:
                    //    break;
            }
            return pos;
        }
        public Position AreaOnTheLeft(Heading headingRC)
        {
            return AreaOnTheLeft(headingRC, PositionRC);
        }
        public Position AreaOnTheRight(Heading headingRC, Position basePosition)
        {
            Position pos = null;
            switch (headingRC)
            {
                case Heading.Up:
                    pos = new Position(basePosition.Y, basePosition.X + 1);
                    break;
                case Heading.Down:
                    pos = new Position(basePosition.Y, basePosition.X - 1);
                    break;
                case Heading.Left:
                    pos = new Position(basePosition.Y - 1, basePosition.X);
                    break;
                case Heading.Right:
                    pos = new Position(basePosition.Y + 1, basePosition.X);
                    break;
                    //default:
                    //    break;
            }
            return pos;
        }
        public Position AreaOnTheRight(Heading headingRC)
        {
            return AreaOnTheRight(headingRC, PositionRC);
        }
        public Position AreaOnTheFront(Heading headingRC, Position basePosition)
        {
            Position pos = null;
            switch (headingRC)
            {
                case Heading.Up:
                    pos = new Position(basePosition.Y - 1, basePosition.X);
                    break;
                case Heading.Down:
                    pos = new Position(basePosition.Y + 1, basePosition.X);
                    break;
                case Heading.Left:
                    pos = new Position(basePosition.Y, basePosition.X - 1);
                    break;
                case Heading.Right:
                    pos = new Position(basePosition.Y, basePosition.X + 1);
                    break;
                    //default:
                    //    break;
            }

            return pos;
        }
        public Position AreaOnTheFront(Heading headingRC)
        {
            return AreaOnTheFront(headingRC, PositionRC);
        }
    }
}
