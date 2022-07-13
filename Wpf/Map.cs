﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Wpf
{
    enum Figure { UncleanedArea = 0, Wall = 1, CleanedArea = 8 };


    class Map
    {
        public static sbyte[][] initCoordinates(int y, int x)
        {
            sbyte[][] mapCoordinates;
            mapCoordinates = new sbyte[y][];
            for (int i = 0; i < mapCoordinates.Length; i++)
            {
                mapCoordinates[i] = new sbyte[x];
            }
            return mapCoordinates;
        }

        /// <summary>
        /// offset = 1
        /// </summary>
        /// <param name="headingRC"></param>
        /// <param name="basePosition"></param>
        /// <returns></returns>
        public static Position AreaOnTheLeft(Heading headingRC, Position basePosition)
        {
            return AreaOnTheLeft(headingRC, basePosition, 1);
        }
        public static Position AreaOnTheLeft(Heading headingRC, Position basePosition, int offset)
        {
            Position pos= new Position();
            switch (headingRC)
            {
                case Heading.Up:
                    pos = basePosition.ShiftLeft(offset);
                    break;
                case Heading.Down:
                    pos = basePosition.ShiftRight(offset);
                    break;
                case Heading.Left:
                    pos = basePosition.ShiftDown(offset);
                    break;
                case Heading.Right:
                    pos = basePosition.ShiftUp(offset);
                    break;
            }
            return pos;
        }

        /// <summary>
        /// offset = 1
        /// </summary>
        /// <param name="headingRC"></param>
        /// <param name="basePosition"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        public static Position AreaOnTheRight(Heading headingRC, Position basePosition)
        {
            return AreaOnTheRight(headingRC, basePosition, 1);
        }
        public static Position AreaOnTheRight(Heading headingRC, Position basePosition, int offset)
        {
            Position pos = new Position();
            switch (headingRC)
            {
                case Heading.Up:
                    pos = basePosition.ShiftRight(offset);
                    break;
                case Heading.Down:
                    pos = basePosition.ShiftLeft(offset);
                    break;
                case Heading.Left:
                    pos = basePosition.ShiftUp(offset);
                    break;
                case Heading.Right:
                    pos = basePosition.ShiftDown(offset);
                    break;
            }
            return pos;
        }
        
        /// <summary>
        /// offset = 1
        /// </summary>
        /// <param name="headingRC"></param>
        /// <param name="basePosition"></param>
        /// <returns></returns>
        public static Position AreaOnTheFront(Heading headingRC, Position basePosition)
        {
            return AreaOnTheFront(headingRC, basePosition, 1);
        }
        public static Position AreaOnTheFront(Heading headingRC, Position basePosition, int offset)
        {
            Position pos = new Position();
            switch (headingRC)
            {
                case Heading.Up:
                    pos = basePosition.ShiftUp(offset);
                    break;
                case Heading.Down:
                    pos = basePosition.ShiftDown(offset);
                    break;
                case Heading.Left:
                    pos = basePosition.ShiftLeft(offset);
                    break;
                case Heading.Right:
                    pos = basePosition.ShiftRight(offset);
                    break;
            }

            return pos;
        }


        private Random rnd;
        public sbyte[][] Coordinates { get; set; }
        public Position PositionRC { get; set; }
        public Map(sbyte[][] coordinates, Position posRC)
        {
            rnd = new Random();
            Coordinates = coordinates;
            PositionRC = posRC;
        }

        public void PutRoboCleanerOnMap(Position pos, Heading headingRC)
        {
            PositionRC = pos;
            RefreshCoordinate(pos, (sbyte)headingRC);
        }
      
        public void RefreshCoordinate(Position coordinate, sbyte figure)
        {
            Coordinates[coordinate.Y][coordinate.X] = figure;
        }

        public void MoveOnMapAndUpdatePositionRC(Heading headingRC, Position newPosition)
        {
            RefreshCoordinate(PositionRC, (sbyte)Figure.CleanedArea);
            RefreshCoordinate(newPosition, (sbyte)headingRC);
            PositionRC = newPosition;
        }

        /// <summary>
        /// returns -1 if position is out of range
        /// </summary>
        /// <param name="pos"></param>
        /// <returns></returns>
        public sbyte CoordinateFigureByPosition(Position pos)
        {
            sbyte figure = -1;
            try
            {
                figure = Coordinates[(int)pos.Y][(int)pos.X];
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

        public Position AreaOnTheLeft(Heading headingRC)
        {
            return AreaOnTheLeft(headingRC, PositionRC);
        }
        public Position AreaOnTheRight(Heading headingRC)
        {
            return AreaOnTheRight(headingRC, PositionRC);
        }
        public Position AreaOnTheFront(Heading headingRC)
        {
            return AreaOnTheFront(headingRC, PositionRC);
        }

        
      public void PutRoboCleanerOnMapRandom()
      {
        /*
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

          PutRoboCleanerOnMap(new Position(y, x), headUp);

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
        */
      }
      
    }
}
