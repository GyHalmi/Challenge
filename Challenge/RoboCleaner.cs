using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Challenge
{
    class RoboCleaner
    {
        public static Heading defaultHeadingUP = Heading.Up;
        public Heading HeadingRC { get; set; }
        public Map MapExternal { get; set; }
        public Map MapOwn { get; set; }
        private int MapRefreshTime { get; set; }

        public RoboCleaner(Map mapExternal)
        {
            MapExternal = mapExternal;
            HeadingRC = defaultHeadingUP;
            MapOwn = new Map(Map.initMap(10, 10), new Position(0, 0));
            MapRefreshTime = 1000;
        }


        public void MoveAndUpdatePositonOnBothMap()
        {
            MapExternal.MoveOnMapAndUpdatePositionRC(HeadingRC, MoveForwardRC(MapExternal.PositionRC));
            MapOwn.MoveOnMapAndUpdatePositionRC(HeadingRC, MoveForwardRC(MapOwn.PositionRC));
        }
        public void StartCleaning()
        {
            MapOwn = CreateOwnMap();
            
            List<Position> nearestUncleanedZones = FindNearestUncleanedZones();

            while (nearestUncleanedZones.Count > 0)
            {
                List<List<Position>> possibleWays = new List<List<Position>>();
                foreach (Position pos in nearestUncleanedZones)
                {
                    possibleWays.Add(FindShortestWayTo(pos));
                }

                List<Position> shortestWay = possibleWays.Aggregate((stored, next) => stored.Count > next.Count ? next : stored);
                MoveOnWay(shortestWay);
                CleanActualZone();
                nearestUncleanedZones = FindNearestUncleanedZones();
            }

        }

        private void MoveOnWay(List<Position> way)
        {
            int i = 1;

            while (i < way.Count && CheckIfFree(MapExternal, way[i], (int)Figure.Wall))
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

                RecordBarriersAround();
                MoveAndUpdatePositonOnBothMap();

                DisplayTwoMaps(MapExternal, MapOwn);
                i++;
            }

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
                    success && MapOwn.CoordinateFigureByPosition(last).Equals((int)Figure.UncleanedArea))) //if an uncleanedArea entered before target, then do not reach target
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
                    if (CheckIfFree(MapOwn, next, (int)Figure.Wall) && !wayWithMethods.ContainsKey(next))
                    {
                        if (shortestWays.Count > 0)
                        {
                            if (wayWithMethods.Count < shortestWays[0].Count)
                            {
                                storeAndSetSuccess();
                            }
                        }
                        else
                        {
                            storeAndSetSuccess();
                        }
                    }

                    void storeAndSetSuccess()
                    {
                        wayWithMethods[wayWithMethods.Last().Key] = methodNumber;
                        wayWithMethods.Add(next, -1);

                        success = true;
                    }
                }

                bool ongoin()
                {
                    return !success && wayWithMethods.Values.Last() < methodNumber;
                }

            }

            if (shortestWays.Count > 1)
            {
                ;
            }
            
            return shortestWays[0];
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
                     *    xxxxx
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
                     *     px
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



        private void CleanActualZone()
        {
            SetHeadingToLongestWay();

            do
            {
                RecordBarriersAround();

                if (!WallOnTheFront() && !CleanedOnTheFront())
                {
                    MoveAndUpdatePositonOnBothMap();
                }
                else if (!WallOnTheRight() && !CleanedOnTheRight())
                {
                    TurnRightRC();
                    MoveAndUpdatePositonOnBothMap();
                    TurnRightRC();
                }
                else if (!WallOnTheLeft() && !CleanedOnTheLeft())
                {
                    TurnLeftRC();
                    MoveAndUpdatePositonOnBothMap();
                    TurnLeftRC();
                }
                DisplayTwoMaps(MapExternal, MapOwn);

            } while (!WallOnTheFront() && !CleanedOnTheFront() ||
                    !WallOnTheLeft() && !CleanedOnTheLeft() ||
                  !WallOnTheRight() && !CleanedOnTheRight());
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
                while (CheckIfFree(MapOwn, MoveForwardRC(pos), notAllowed))
                {
                    pos = MoveForwardRC(pos);
                    cnt++;
                }
                Position p = MapOwn.PositionRC; // mapown does not mutate while pos does... refrence type?!?!
                freeWayDistance.Add(HeadingRC, cnt);
                TurnRightRC();

            } while (startHeading != (int)HeadingRC);

            HeadingRC = freeWayDistance.Aggregate((stored, next) => stored.Value > next.Value ? stored : next).Key;
            //MapOwn.PutRoboCleanerOnMap(new Position(startY, startX), HeadingRC);

        }


        private void SetHeadingToDetectRoomEdges()
        {
            void turnRightWhileFrontNotFree()
            {
                int i = 0;
                while (i < 3 && WallOnTheFront())
                {
                    TurnRightRC();
                    RecordBarrierOnTheLeft();
                    i++;
                }
            }

            //find start direction
            //before the first move Robo is heading up
            if (!WallOnTheLeft() && !WallOnTheFront() && !WallOnTheRight())
            {
                TurnLeftRC();
                RecordBarrierOnTheLeft();
            }
            else if (WallOnTheRight())
            {
                TurnRightRC();
                RecordBarrierOnTheLeft();
                turnRightWhileFrontNotFree();
            }
            else
            {
                RecordBarrierOnTheLeft();
                turnRightWhileFrontNotFree();
            }
        }

        /// <summary>
        /// returns null if no barriers found
        /// </summary>
        /// <param name="basePosition"></param>
        /// <returns></returns>
        Position RecordBarrierOnTheLeft(Position basePosition)
        {
            Position barrier = null;
            if (WallOnTheLeft())
            {
                barrier = MapExternal.AreaOnTheLeft(HeadingRC, basePosition);
            }
            return barrier;
        }

        public (List<Position> cleanedPath, List<Position> barriers) DetectRoomEdges()
        {
            List<Position> cleanedPathCoordinates = new List<Position>();
            HashSet<Position> barriersCoordinates = new HashSet<Position>();
            Position PositionDetectingRC = new Position(0, 0);

            DisplayMap(MapExternal);
            void RecordBarrier()
            {
                if (WallOnTheLeft())
                {
                    barriersCoordinates.Add(MapExternal.AreaOnTheLeft(HeadingRC, PositionDetectingRC));
                }
            }
            void RecordPath()
            {
                cleanedPathCoordinates.Add(PositionDetectingRC);
            }


            SetHeadingToDetectRoomEdges();

            RecordPath();

            MapExternal.MoveOnMapAndUpdatePositionRC(HeadingRC, MoveForwardRC(MapExternal.PositionRC));
            PositionDetectingRC = MoveForwardRC(PositionDetectingRC);
            RecordBarrier();
            RecordPath();
            DisplayMap(MapExternal);

            //record room edges
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
            //DisplayMapExt();

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
        public SortedDictionary<int, List<int>> PositionListToSortedDictionary(List<Position> positions)
        {
            //sort Y coordinates
            SortedDictionary<int, List<int>> sortedCoordinates = new SortedDictionary<int, List<int>>();

            for (int i = 0; i < positions.Count; i++)
            {
                int key = positions[i].Y;
                int value = positions[i].X;
                if (sortedCoordinates.ContainsKey(key))
                {
                    sortedCoordinates[key].Add(value);
                }
                else
                {
                    sortedCoordinates.Add(key, new List<int> { value });
                }
            }
            // sort X coordinates
            foreach (var item in sortedCoordinates)
            {
                item.Value.Sort();
            }

            return sortedCoordinates;
        }
        private void PlaceSideWalls(int[][] mapCoordinates)
        {
            for (int i = 0; i < mapCoordinates.Length; i++)
            {
                int lenghtMapRow = mapCoordinates[i].Length;
                buildWall(i, 0, indexFirstWall(i));
                buildWall(i, indexLastWall(i), lenghtMapRow);
            }

            int indexFirstWall(int y)
            {
                int i = 0;
                while (i < mapCoordinates[y].Length
                    && mapCoordinates[y][i] != (int)Figure.Wall)
                {
                    i++;
                }
                return i;
            }
            int indexLastWall(int y)
            {
                int i = mapCoordinates[y].Length;
                while (i > 0
                    && mapCoordinates[y][i - 1] != (int)Figure.Wall)
                {
                    i--;
                }
                return i;
            }
            void buildWall(int row, int startIndex, int length)
            {
                for (int i = startIndex; i < length; i++)
                {
                    mapCoordinates[row][i] = 1;
                }
            }
        }
        private Map CreateOwnMap()
        {
            (List<Position> cleanedPath, List<Position> barriers) roomDetection = DetectRoomEdges();
            SortedDictionary<int, List<int>> barrierCoordinates = PositionListToSortedDictionary(roomDetection.barriers);

            //measure map dimensons /array lenghts
            int yMin = barrierCoordinates.Keys.ElementAt(0);
            int yMax = barrierCoordinates.Keys.Last();
            int xMax = int.MinValue;
            int xMin = int.MaxValue;

            for (int i = barrierCoordinates.Keys.First(); i <= barrierCoordinates.Keys.Last(); i++)
            {
                if (xMax < barrierCoordinates[i].Max()) xMax = barrierCoordinates[i].Max();
                if (xMin > barrierCoordinates[i].Min()) xMin = barrierCoordinates[i].Min();
            }

            Map newMap = new Map(Map.initMap(yMax - yMin + 1, xMax - xMin + 1), new Position(0, 0));
            DisplayTwoMaps(MapExternal, newMap);

            //add detected barriers to MapOwn
            foreach (KeyValuePair<int, List<int>> barrierRow in barrierCoordinates)
            {
                for (int i = 0; i < barrierRow.Value.Count; i++)
                {
                    Position p = new Position(barrierRow.Key - yMin, barrierRow.Value[i] - xMin);
                    newMap.RefreshCoordinate(p, (int)Figure.Wall);
                }
            }
            DisplayTwoMaps(MapExternal, newMap);

            PlaceSideWalls(newMap.Coordinates);
            DisplayTwoMaps(MapExternal, newMap);

            //load cleaned area
            List<Position> cleanedPath = roomDetection.cleanedPath;

            for (int i = 0; i < cleanedPath.Count; i++)
            {
                Position p = new Position(cleanedPath[i].Y - yMin, cleanedPath[i].X - xMin);
                newMap.RefreshCoordinate(p, (int)Figure.CleanedArea);
            }
            DisplayTwoMaps(MapExternal, newMap);

            //put robo on the map
            newMap.PutRoboCleanerOnMap(new Position(0 - yMin, 0 - xMin), HeadingRC);
            DisplayTwoMaps(MapExternal, newMap);

            return newMap;
        }


        private bool WallOnTheLeft()
        {
            return !CheckIfFree(MapExternal, MapExternal.AreaOnTheLeft(HeadingRC), (int)Figure.Wall);
        }
        private bool WallOnTheFront()
        {
            return !CheckIfFree(MapExternal, MapExternal.AreaOnTheFront(HeadingRC), (int)Figure.Wall);
        }
        private bool WallOnTheRight()
        {
            return !CheckIfFree(MapExternal, MapExternal.AreaOnTheRight(HeadingRC), (int)Figure.Wall);
        }

        private bool CleanedOnTheLeft()
        {
            return !CheckIfFree(MapOwn, MapOwn.AreaOnTheLeft(HeadingRC), (int)Figure.CleanedArea);
        }
        private bool CleanedOnTheFront()
        {
            return !CheckIfFree(MapOwn, MapOwn.AreaOnTheFront(HeadingRC), (int)Figure.CleanedArea);
        }
        private bool CleanedOnTheRight()
        {
            return !CheckIfFree(MapOwn, MapOwn.AreaOnTheRight(HeadingRC), (int)Figure.CleanedArea);
        }

        private bool CheckIfFree(Map map, Position positionToCheck, int figureNotAllowed)
        {
            return map.CoordinateFigureByPosition(positionToCheck) != figureNotAllowed;
        }
        private bool CheckIfFree(Map map, Position positionToCheck, int[] figuresNotAllowed)
        {
            bool free = true;

            int i = 0;
            do
            {
                free = map.CoordinateFigureByPosition(positionToCheck) != figuresNotAllowed[i];
                i++;
            } while (i < figuresNotAllowed.Length && free);

            return free;
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
