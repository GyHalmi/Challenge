using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Shapes;

namespace Wpf
{
    enum Heading { Up = 3, Down = 4, Left = 5, Right = 6 };

    class RoboCleaner
    {
        public Rectangle RC { get; set; }
        public Heading HeadingRC { get; set; }
        private Map MapExternal { get; set; }
        public Map MapOwn { get; set; }
        private int MapRefreshTime { get; set; }

        public RoboCleaner(Map mapExternal, Rectangle roboCleaner)
        {
            RC = roboCleaner;
            MapExternal = mapExternal;
            HeadingRC = Heading.Up;
            MapOwn = new Map(Map.initCoordinates(10, 10), new Position(0, 0));
            MapRefreshTime = 600;
        }

        public void CleanTheHouse()
        {
            DisplayMap(MapExternal);
            MapOwn = CreateMap();
            DisplayTwoMaps(MapExternal, MapOwn);

            List<Position> nearestUncleanedZones = FindNearestUncleanedZones();

            while (nearestUncleanedZones.Count > 0)
            {
                List<List<Position>> possibleWays = new List<List<Position>>();
                nearestUncleanedZones.ForEach(pos => possibleWays.Add(FindShortestWayTo(pos)));

                List<Position> shortestWay = possibleWays.Aggregate((stored, next) => stored.Count > next.Count ? next : stored);

                bool barrierFound = !MoveOnWay(shortestWay);
                if (!barrierFound) barrierFound |= !CleanActualZone();
                if (barrierFound) DetectAndAddBarrierToOwnMap();

                nearestUncleanedZones = FindNearestUncleanedZones();
            }
        }

        /// <summary>
        /// if no uncleaned zones found retuns an empty List
        /// </summary>
        /// <returns></returns>
        private List<Position> FindNearestUncleanedZones()
        {
            int[][] mapCoords = MapOwn.Coordinates;
            Position posRC = MapOwn.PositionRC;

            int xMax = 0;
            foreach (int[] x in mapCoords)
            {
                if (x.Length > xMax) xMax = x.Length;
            }
            int maxLength = mapCoords.Length > xMax ? mapCoords.Length : xMax;

            HashSet<Position> closestPoints = new HashSet<Position>();
            int i = 1;

            while (closestPoints.Count == 0 && i < maxLength)
            {
                int y = MapOwn.PositionRC.Y;
                int x = MapOwn.PositionRC.X;
                int j = 0;

                //for (int j = 0; j <= i; j++)
                while (j <= i && closestPoints.Count == 0)
                {

                    /*      p
                     *     xxx
                     */
                    try
                    {
                        if (mapCoords[y + i][x + j] == 0)
                        {
                            closestPoints.Add(new Position(posRC.Y + i, posRC.X + j));
                        }
                    }
                    catch (IndexOutOfRangeException)
                    {
                        ;
                    }
                    try
                    {
                        if (mapCoords[y + i][x - j] == 0)
                        {
                            closestPoints.Add(new Position(posRC.Y + i, posRC.X - j));
                        }
                    }
                    catch (IndexOutOfRangeException)
                    {
                        ;
                    }

                    /*   
                     *   x
                     *   x p
                     *   x
                     */
                    try
                    {
                        if (mapCoords[y + j][x - i] == 0)
                        {
                            closestPoints.Add(new Position(posRC.Y + j, posRC.X - i));
                        }
                    }
                    catch (IndexOutOfRangeException)
                    {
                        ;
                    }
                    try
                    {
                        if (mapCoords[y - j][x - i] == 0)
                        {
                            closestPoints.Add(new Position(posRC.Y - j, posRC.X - i));
                        }
                    }
                    catch (IndexOutOfRangeException)
                    {
                        ;
                    }

                    /*     xxx
                     *      p
                     */
                    try
                    {
                        if (mapCoords[y - i][x + j] == 0)
                        {
                            closestPoints.Add(new Position(posRC.Y - i, posRC.X + j));
                        }
                    }
                    catch (IndexOutOfRangeException)
                    {
                        ;
                    }
                    try
                    {
                        if (mapCoords[y - i][x - j] == 0)
                        {
                            closestPoints.Add(new Position(posRC.Y - i, posRC.X - j));
                        }
                    }
                    catch (IndexOutOfRangeException)
                    {
                        ;
                    }

                    /*   
                     *      x
                     *    p x
                     *      x
                     */
                    try
                    {
                        if (mapCoords[y + j][x + i] == 0)
                        {
                            closestPoints.Add(new Position(posRC.Y + j, posRC.X + i));
                        }
                    }
                    catch (IndexOutOfRangeException)
                    {
                        ;
                    }
                    try
                    {
                        if (mapCoords[y - j][x + i] == 0)
                        {
                            closestPoints.Add(new Position(posRC.Y - j, posRC.X + i));
                        }
                    }
                    catch (IndexOutOfRangeException)
                    {
                        ;
                    }
                    j++;
                }
                i++;
            }

            return closestPoints.ToList();
        }
        /// <summary>
        /// finds the shortest way to the target or the way to the first uncleaned area towards target
        /// </summary>
        /// <param name="targetPosition"></param>
        /// <returns></returns>
        private List<Position> FindShortestWayTo(Position targetPosition)
        {
            List<List<Position>> shortestWays = new List<List<Position>>();

            Dictionary<Position, int> wayWithMethods = new Dictionary<Position, int>();
            wayWithMethods.Add(MapOwn.PositionRC, -1);

            while (wayWithMethods.Count != 0)
            {
                bool success = false;
                Position last = wayWithMethods.Last().Key;
                Position next;


                //decreaseDistanceX
                int methodNumber = 0;
                if (ongoin() && last.X != targetPosition.X)
                {
                    next = last.X > targetPosition.X ? new Position(last.Y, last.X - 1) : new Position(last.Y, last.X + 1);
                    evaluateNext();
                }

                //decreaseDistanceY
                methodNumber = 1;
                if (ongoin() && last.Y != targetPosition.Y)
                {
                    next = last.Y > targetPosition.Y ? new Position(last.Y - 1, last.X) : new Position(last.Y + 1, last.X);
                    evaluateNext();
                }

                //increaseDistanceX
                methodNumber = 2;
                if (ongoin() && last.X != targetPosition.X)
                {
                    next = last.X > targetPosition.X ? new Position(last.Y, last.X + 1) : new Position(last.Y, last.X - 1);
                    evaluateNext();
                }
                methodNumber = 3; //increase direction right
                if (ongoin() && last.X == targetPosition.X)
                {
                    next = last.Right();
                    evaluateNext();
                }
                methodNumber = 4; //increase direction left
                if (ongoin() && last.X == targetPosition.X)
                {
                    next = last.Left();
                    evaluateNext();
                }

                //increaseDistanceY
                methodNumber = 6;
                if (ongoin() && last.Y != targetPosition.Y)
                {
                    next = last.Y > targetPosition.Y ? new Position(last.Y + 1, last.X) : new Position(last.Y - 1, last.X);
                    evaluateNext();
                }
                methodNumber = 7; //increase direction up
                if (ongoin() && last.Y == targetPosition.Y)
                {
                    next = last.Up();
                    evaluateNext();
                }
                methodNumber = 8; //increase direction down
                if (ongoin() && last.Y == targetPosition.Y)
                {
                    next = last.Down();
                    evaluateNext();
                }

                last = wayWithMethods.Last().Key;

                if (success &&
                    (last.Equals(targetPosition) ||
                     MapOwn.CoordinateFigureByPosition(last).Equals((int)Figure.UncleanedArea))) //if an uncleanedArea entered before target, then do not reach target
                {
                    if (shortestWays.Count > 0 &&
                        shortestWays[0].Count > wayWithMethods.Count)
                    {
                        //store ways with minimum length
                        shortestWays.Clear();
                    }
                    //store found way
                    List<Position> newWay = new List<Position>();
                    foreach (var posAndMethod in wayWithMethods)
                    {
                        newWay.Add(posAndMethod.Key);
                    }
                    shortestWays.Add(newWay);

                    removeLast();
                }
                else if (success && shortestWays.Count > 0 &&
                    shortestWays[0].Count == wayWithMethods.Count &&
                    last != targetPosition)
                {
                    removeLast();
                }
                else if (!success)
                {
                    removeLast();
                }

                void removeLast()
                {
                    wayWithMethods.Remove(wayWithMethods.Last().Key);
                }

                void evaluateNext()
                {

                    if (MapOwn.CoordinateFigureByPosition(next) != (int)Figure.Wall && !wayWithMethods.ContainsKey(next))
                    {
                        if (shortestWays.Count <= 0 ||
                            (shortestWays.Count > 0 && wayWithMethods.Count < shortestWays[0].Count))
                        {
                            wayWithMethods[wayWithMethods.Last().Key] = methodNumber;
                            wayWithMethods.Add(next, -1);

                            success = true;
                        }
                    }
                }

                bool ongoin()
                {
                    return !success && wayWithMethods.Values.Last() < methodNumber;
                }

            }

            int shortesIndex = 0;
            int min = CountTurnsOnWay(shortestWays[shortesIndex]);

            for (int i = 1; i < shortestWays.Count; i++)
            {
                if (CountTurnsOnWay(shortestWays[i]) < min)
                {
                    shortesIndex = i;
                    min = CountTurnsOnWay(shortestWays[i]);
                }
            }
            return shortestWays[shortesIndex];

        }
        private int CountTurnsOnWay(List<Position> way)
        {
            int turns = 0;
            int heading = (int)HeadingRC;

            for (int i = 1; i < way.Count; i++)
            {
                Position robo = way[i - 1];
                Position next = way[i];

                if (Map.AreaOnTheLeft(HeadingRC, robo).Equals(next))
                {
                    TurnLeftRC();
                    turns++;
                }
                else if (Map.AreaOnTheFront(HeadingRC, robo).Equals(next))
                {
                    ;
                }
                else if (Map.AreaOnTheRight(HeadingRC, robo).Equals(next))
                {
                    TurnRightRC();
                    turns++;
                }
                else
                {
                    TurnRightRC();
                    turns++;
                    TurnRightRC();
                    turns++;
                }
            }

            HeadingRC = (Heading)heading;
            return turns;
        }
        /// <summary>
        /// returns true if the end of the way is reached, else stops before wall and returns false
        /// </summary>
        /// <param name="way"></param>
        
        
        private bool MoveOnWay(List<Position> way)
        {
            int i = 1;

            while (i < way.Count && MapExternal.CoordinateFigureByPosition(way[i]) != ((int)Figure.Wall))
            {
                Position p = way[i];
                if (MapOwn.AreaOnTheLeft(HeadingRC).Equals(p))
                {
                    TurnLeftRC();
                }
                else if (MapOwn.AreaOnTheFront(HeadingRC).Equals(p))
                {
                    ;
                }
                else if (MapOwn.AreaOnTheRight(HeadingRC).Equals(p))
                {
                    TurnRightRC();
                }
                else
                {
                    TurnRightRC();
                    TurnRightRC();
                }

                MoveAndUpdatePositonOnBothMap();
                DisplayTwoMaps(MapExternal, MapOwn);
                i++;
            }

            return way.Count >= i;

        }

        /// <summary>
        /// returns true if the zone is cleaned, false if cleaning cannot go on beacause of a wall
        /// </summary>
        /// <returns></returns>
        private bool CleanActualZone()
        {
            do
            {
                RecordBarriersAround();
                if (!WallOnTheLeft() && !CleanedOnTheLeft())
                {
                    TurnLeftRC();
                }
                else if (!WallOnTheFront() && !CleanedOnTheFront())
                {
                    ;
                }
                else if (!WallOnTheRight() && !CleanedOnTheRight())
                {
                    TurnRightRC();
                }
                MoveAndUpdatePositonOnBothMap();
                DisplayTwoMaps(MapExternal, MapOwn);

            } while (!WallOnTheFront() && !CleanedOnTheFront() ||
                    !WallOnTheLeft() && !CleanedOnTheLeft() ||
                    !WallOnTheRight() && !CleanedOnTheRight());

            return CleanedOnTheFront() && CleanedOnTheLeft() && CleanedOnTheRight();
        }
        private void RecordBarriersAround()
        {
            int figWall = (int)Figure.Wall;

            if (WallOnTheLeft())
            {
                MapOwn.RefreshCoordinate(MapOwn.AreaOnTheLeft(HeadingRC), figWall);
            }
            if (WallOnTheFront())
            {
                MapOwn.RefreshCoordinate(MapOwn.AreaOnTheFront(HeadingRC), figWall);
            }
            if (WallOnTheRight())
            {
                MapOwn.RefreshCoordinate(MapOwn.AreaOnTheRight(HeadingRC), figWall);
            }

        }
        private void DetectAndAddBarrierToOwnMap()
        {
            Map barrierMap = CreateMap();
            DisplayTwoMaps(MapExternal, barrierMap);

            int shiftY = MapOwn.PositionRC.Y - barrierMap.PositionRC.Y;
            int shiftX = MapOwn.PositionRC.X - barrierMap.PositionRC.X;

            for (int y = 0; y < barrierMap.Coordinates.Length; y++)
            {
                for (int x = 0; x < barrierMap.Coordinates[y].Length; x++)
                {
                    Position p = new Position(y, x);
                    int figure = barrierMap.CoordinateFigureByPosition(p);
                    if (figure != 0) MapOwn.RefreshCoordinate(new Position(p.Y + shiftY, p.X + shiftX), figure);
                }
            }
            DisplayTwoMaps(MapExternal, MapOwn);


        }


        private Map CreateMap()
        {
            SetHeadingToStartEdgeDetection();
            (List<Position> cleanedPath, List<Position> barriers) edgeDetection = DetectEdges();

            //measure map dimensons /array lenghts
            (int minimum, int maximum) barrierYMinMax = YMinMax(edgeDetection.barriers);
            (int minimum, int maximum) barrierXMinMax = XMinMax(edgeDetection.barriers);
            (int minimum, int maximum) pathYMinMax = YMinMax(edgeDetection.cleanedPath);
            (int minimum, int maximum) pathXMinMax = XMinMax(edgeDetection.cleanedPath);

            int yMin = barrierYMinMax.minimum < pathYMinMax.minimum ? barrierYMinMax.minimum : pathYMinMax.minimum;
            int yMax = barrierYMinMax.maximum > pathYMinMax.maximum ? barrierYMinMax.maximum : pathYMinMax.maximum;
            int xMin = barrierXMinMax.minimum < pathXMinMax.minimum ? barrierXMinMax.minimum : pathXMinMax.minimum;
            int xMax = barrierXMinMax.maximum > pathXMinMax.maximum ? barrierXMinMax.maximum : pathXMinMax.maximum;

            Map newMap = new Map(Map.initCoordinates(yMax - yMin + 1, xMax - xMin + 1), new Position(0, 0));
            DisplayTwoMaps(MapExternal, newMap);

            Position shiftCoordinate(Position p)
            {
                return new Position(p.Y - yMin, p.X - xMin);
            }

            //shift detected coordinates
            (List<Position> cleanedPath, List<Position> barriers) shiftedCoordinates;
            shiftedCoordinates.barriers = edgeDetection.barriers.Select(p => shiftCoordinate(p)).ToList();
            shiftedCoordinates.cleanedPath = edgeDetection.cleanedPath.Select(p => shiftCoordinate(p)).ToList();

            //add barriers and cleanened path to newMap
            shiftedCoordinates.barriers.ForEach(p => newMap.RefreshCoordinate(p, (int)Figure.Wall));
            shiftedCoordinates.cleanedPath.ForEach(p => newMap.RefreshCoordinate(p, (int)Figure.CleanedArea));

            //DisplayTwoMaps(MapExternal, newMap);

            FillMapWhithUnreachableWalls(newMap, shiftedCoordinates);

            //put robo on the map
            newMap.PutRoboCleanerOnMap(shiftedCoordinates.cleanedPath.Last(), HeadingRC);

            return newMap;
        }
        /// <summary>
        /// set heading before detection
        /// </summary>
        /// <returns></returns>
        private (List<Position> cleanedPath, List<Position> barriers) DetectEdges()
        {
            List<Position> cleanedPathCoordinates = new List<Position>();
            HashSet<Position> barriersCoordinates = new HashSet<Position>();
            Position PositionDetectingRC = new Position(0, 0);

            void RecordBarrier()
            {
                if (WallOnTheLeft())
                {
                    barriersCoordinates.Add(Map.AreaOnTheLeft(HeadingRC, PositionDetectingRC));
                }
            }
            void RecordPath()
            {
                cleanedPathCoordinates.Add(PositionDetectingRC);
            }

            //record edges
            RecordBarrier();
            RecordPath();

            do
            {
                if (!WallOnTheLeft())
                {
                    TurnLeftRC();
                }
                else if (!WallOnTheFront())
                {
                    ;
                }
                else if (!WallOnTheRight())
                {
                    TurnRightRC();
                }
                else
                {
                    //turn around
                    TurnRightRC();
                    RecordBarrier();
                    TurnRightRC();
                }
                RecordBarrier();

                MapExternal.MoveOnMapAndUpdatePositionRC(HeadingRC, MoveForwardRC(MapExternal.PositionRC));
                PositionDetectingRC = MoveForwardRC(PositionDetectingRC);
                RecordBarrier();
                RecordPath();
                //DisplayMap(MapExternal);
            } while (ongoingDetection());

            bool ongoingDetection()
            {
                bool od = true;

                if (cleanedPathCoordinates.Last().Equals(cleanedPathCoordinates[0]))
                {
                    //avoid cases when robot starts on a path from where it must come back
                    int i = 0;
                    int cCount = cleanedPathCoordinates.Count;
                    while (i < cCount / 2 &&
                        cleanedPathCoordinates[cCount - 1 - i].Equals(cleanedPathCoordinates[i]))
                    {
                        i++;
                    }
                    od = i >= cCount / 2;
                }

                return od;
            }

            return (cleanedPathCoordinates, barriersCoordinates.ToList());
        }
        private void FillMapWhithUnreachableWalls(Map map, (List<Position> cleandedPath, List<Position> barriers) pathAndBarriers)
        {
            Dictionary<Position, bool> checkedPositions = new Dictionary<Position, bool>();
            pathAndBarriers.barriers.ForEach(p => checkedPositions.Add(p, false));

            while (!checkedPositions.All(p => p.Value == true))
            {
                checkNeighbours(checkedPositions.First(p => p.Value == false).Key);
            }

            void checkPos(Position pos)
            {
                try
                {
                    if (map.CoordinateFigureByPosition(pos) == (int)Figure.UncleanedArea)
                    {
                        map.RefreshCoordinate(pos, (int)Figure.Wall);
                        checkedPositions.Add(pos, false);
                        checkNeighbours(pos);
                    }
                }
                catch (IndexOutOfRangeException)
                {
                    ;
                }
            }

            void checkNeighbours(Position pos)
            {
                checkPos(pos.Up());
                checkPos(pos.Right());
                checkPos(pos.Down());
                checkPos(pos.Left());
                checkedPositions[pos] = true;
            }

        }
        private (int min, int max) YMinMax(List<Position> positions)
        {
            int yMini = positions.Aggregate((min, next) => next.Y < min.Y ? next : min).Y;
            int yMaxi = positions.Aggregate((max, next) => next.Y > max.Y ? next : max).Y;
            return (yMini, yMaxi);
        }
        private (int min, int max) XMinMax(List<Position> positions)
        {
            int xMini = positions.Aggregate((min, next) => next.X < min.X ? next : min).X;
            int xMaxi = positions.Aggregate((max, next) => next.X > max.X ? next : max).X;
            return (xMini, xMaxi);
        }


        //environment dependent
        private bool WallOnTheLeft()
        {
            return MapExternal.CoordinateFigureByPosition(MapExternal.AreaOnTheLeft(HeadingRC)).Equals((int)Figure.Wall);
        }
        private bool WallOnTheFront()
        {
            return MapExternal.CoordinateFigureByPosition(MapExternal.AreaOnTheFront(HeadingRC)).Equals((int)Figure.Wall);
        }
        private bool WallOnTheRight()
        {
            return MapExternal.CoordinateFigureByPosition(MapExternal.AreaOnTheRight(HeadingRC)).Equals((int)Figure.Wall);
        }



        private bool CleanedOnTheLeft()
        {
            return MapOwn.CoordinateFigureByPosition(MapOwn.AreaOnTheLeft(HeadingRC)).Equals((int)Figure.CleanedArea);
        }
        private bool CleanedOnTheFront()
        {
            return MapOwn.CoordinateFigureByPosition(MapOwn.AreaOnTheFront(HeadingRC)).Equals((int)Figure.CleanedArea);
        }
        private bool CleanedOnTheRight()
        {
            return MapOwn.CoordinateFigureByPosition(MapOwn.AreaOnTheRight(HeadingRC)).Equals((int)Figure.CleanedArea);
        }

        private void TurnLeftRC()
        {
            switch (HeadingRC)
            {
                case Heading.Up:
                    HeadingRC = Heading.Left;
                    break;
                case Heading.Down:
                    HeadingRC = Heading.Right;
                    break;
                case Heading.Left:
                    HeadingRC = Heading.Down;
                    break;
                case Heading.Right:
                    HeadingRC = Heading.Up;
                    break;
                    //default:
                    //    break;
            }
        }
        private void TurnRightRC()
        {
            switch (HeadingRC)
            {
                case Heading.Up:
                    HeadingRC = Heading.Right;
                    break;
                case Heading.Down:
                    HeadingRC = Heading.Left;
                    break;
                case Heading.Left:
                    HeadingRC = Heading.Up;
                    break;
                case Heading.Right:
                    HeadingRC = Heading.Down;
                    break;
                    //default:
                    //    break;
            }
        }
        private void SetHeadingToLongestWay()
        {
            int startHeading = (int)HeadingRC;
            Dictionary<Heading, int> freeWayDistance = new Dictionary<Heading, int>();

            Position pos = MapOwn.PositionRC;
            int startY = pos.Y;
            int startX = pos.X;

            int cnt;
            int[] notAllowed = new int[] { (int)Figure.Wall, (int)Figure.CleanedArea };

            do
            {
                cnt = 0;
                pos = new Position(startY, startX);

                while (nextPosFigure() != (int)Figure.Wall && nextPosFigure() != (int)Figure.CleanedArea)
                {
                    pos = MoveForwardRC(pos);
                    cnt++;
                }

                int nextPosFigure()
                {
                    return MapOwn.CoordinateFigureByPosition(MoveForwardRC(pos));
                }
                Position p = MapOwn.PositionRC; // mapown does not mutate while pos does... refrence type?!?!
                freeWayDistance.Add(HeadingRC, cnt);
                TurnRightRC();

            } while (startHeading != (int)HeadingRC);

            HeadingRC = freeWayDistance.Aggregate((stored, next) => stored.Value > next.Value ? stored : next).Key;
            //MapOwn.PutRoboCleanerOnMap(new Position(startY, startX), HeadingRC);

        }
        private void SetHeadingToStartEdgeDetection()
        {
            void turnRightWhileFrontNotFree()
            {
                int i = 0;
                while (i < 3 && WallOnTheFront())
                {
                    TurnRightRC();
                    i++;
                }
            }

            //find start direction

            //before the first move Robo is heading up
            HeadingRC = Heading.Up; // maybe not necessary

            if (!WallOnTheLeft() && !WallOnTheFront() && !WallOnTheRight())
            {
                TurnLeftRC();
            }
            else if (WallOnTheRight())
            {
                TurnRightRC();
                turnRightWhileFrontNotFree();
            }
            else
            {
                turnRightWhileFrontNotFree();
            }
        }
        private Position MoveForwardRC(Position actualPos)
        {
            Position newPos = null;
            switch (HeadingRC)
            {
                case Heading.Up:
                    newPos = actualPos.Up();
                    break;
                case Heading.Down:
                    newPos = actualPos.Down();
                    break;
                case Heading.Left:
                    newPos = actualPos.Left();
                    break;
                case Heading.Right:
                    newPos = actualPos.Right();
                    break;
                    //default:
                    //    break;
            }
            return newPos;
        }
        private void MoveAndUpdatePositonOnBothMap()
        {
            MapExternal.MoveOnMapAndUpdatePositionRC(HeadingRC, MoveForwardRC(MapExternal.PositionRC));
            MapOwn.MoveOnMapAndUpdatePositionRC(HeadingRC, MoveForwardRC(MapOwn.PositionRC));
        }


        private void DisplayMap(Map map)
        {
            Console.Clear();
            map.Display();
            Thread.Sleep(MapRefreshTime);
        }
        private void DisplayTwoMaps(Map map1, Map map2)
        {
            Console.Clear();
            map1.Display();
            Console.WriteLine();
            map2.Display();
            Thread.Sleep(MapRefreshTime);
        }

    }
}
