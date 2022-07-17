using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Wpf
{
    enum Heading { Up = 3, Down = 4, Left = 5, Right = 6 };

    class RoboCleaner
    {
        public Rectangle RC { get; set; }
        public Heading HeadingRC { get; set; }
        public Map MapOwn { get; set; }

        private Canvas MapGUI { get; set; }

        //public RoboCleaner(Map mapExternal, Rectangle roboCleaner)
        public RoboCleaner(Rectangle roboCleaner)
        {
            RC = roboCleaner;
            MapGUI = (Canvas)RC.Parent;
            HeadingRC = Heading.Up;
        }

        public void CleanTheHouse()
        {
            //DisplayMap(MapExternal);
            MapOwn = CreateMap();
            //DisplayTwoMaps(MapExternal, MapOwn);

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
            MessageBox.Show("ready!");
        }

        /// <summary>
        /// if no uncleaned zones found retuns an empty List
        /// </summary>
        /// <returns></returns>
        private List<Position> FindNearestUncleanedZones()
        {
            HashSet<Position> closestPoints = new HashSet<Position>();
            sbyte[][] mapCoords = MapOwn.Coordinates;
            Position posRC = MapOwn.PositionRC;

            int xMin, xMax, yMin, yMax;

            yMax = mapCoords.Length - 1;
            yMin = yMax;
            xMax = mapCoords[0].Length - 1;
            xMin = xMax;

            int longestMeasurableDirection()
            {
                return Math.Max(Math.Max(xMin, xMax), Math.Max(yMin, yMax));
            }


            int planeShift = 1;
            int positionShift = 0;

            void checkPositions(Func<Position, int, Position> shiftPlane, Func<Position, int, Position> shiftPosition, ref int planeShiftMax, ref int positionShiftMax)
            {
                Position posRClocal = MapOwn.PositionRC;
                while (positionShift <= planeShift && planeShift <= planeShiftMax && positionShift <= positionShiftMax)
                {
                    Position newPos = shiftPosition(shiftPlane(posRClocal, planeShift), positionShift);

                    if (newPos.Y == 0 || newPos.Y == MapOwn.Coordinates.Length - 1 ||
                        newPos.X == 0 || newPos.X == MapOwn.Coordinates[0].Length - 1)
                    {
                        // position point to map edge. it is a wall. no need to check
                    }

                    if (MapOwn.CoordinateFigureByPosition(newPos) == -1) //if newPos is out of range
                    {
                        Position planeIsInRange = shiftPosition(shiftPlane(posRClocal, 0), positionShift);
                        if (MapOwn.CoordinateFigureByPosition(planeIsInRange) != -1)
                        {
                            planeShiftMax = planeShift - 1;
                        }
                        else
                        {
                            positionShiftMax = positionShift - 1;
                        }
                    }
                    else if (MapOwn.CoordinateFigureByPosition(newPos) == (sbyte)Figure.UncleanedArea)
                    {
                        closestPoints.Add(newPos);
                    }
                    positionShift++;
                }
            }

            while (closestPoints.Count == 0 && planeShift <= longestMeasurableDirection())
            {
                //check
                /*  
                 *      p
                 *     xxx
                 */
                //down-rigth
                positionShift = 0;
                checkPositions((p, offset) => p.ShiftDown(offset), (p, offset) => p.ShiftRight(offset), ref yMax, ref xMax);

                //down-left
                positionShift = 1;
                checkPositions((p, offset) => p.ShiftDown(offset), (p, offset) => p.ShiftLeft(offset), ref yMax, ref xMin);

                /*   
                 *   x
                 *   x p
                 *   x
                 */
                //left-down
                positionShift = 0;
                checkPositions((p, offset) => p.ShiftLeft(offset), (p, offset) => p.ShiftDown(offset), ref xMin, ref yMax);

                //left-up
                positionShift = 1;
                checkPositions((p, offset) => p.ShiftLeft(offset), (p, offset) => p.ShiftUp(offset), ref xMin, ref yMin);


                /*     xxx
                 *      p
                 */
                //up-left
                positionShift = 0;
                checkPositions((p, offset) => p.ShiftUp(offset), (p, offset) => p.ShiftLeft(offset), ref yMin, ref xMin);

                //up-right
                positionShift = 1;
                checkPositions((p, offset) => p.ShiftUp(offset), (p, offset) => p.ShiftRight(offset), ref yMin, ref xMax);


                /*   
                 *      x
                 *    p x
                 *      x
                 */
                //right-up
                positionShift = 0;
                checkPositions((p, offset) => p.ShiftRight(offset), (p, offset) => p.ShiftUp(offset), ref xMax, ref yMin);

                //right-down
                positionShift = 1;
                checkPositions((p, offset) => p.ShiftRight(offset), (p, offset) => p.ShiftDown(offset), ref xMax, ref yMax);

                planeShift++;
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
                    next = last.ShiftRight();
                    evaluateNext();
                }
                methodNumber = 4; //increase direction left
                if (ongoin() && last.X == targetPosition.X)
                {
                    next = last.ShiftLeft();
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
                    next = last.ShiftUp();
                    evaluateNext();
                }
                methodNumber = 8; //increase direction down
                if (ongoin() && last.Y == targetPosition.Y)
                {
                    next = last.ShiftDown();
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
                    shortestWays.Add(wayWithMethods.Keys.ToList());

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
            int turnsTotal = 0;
            Heading heading = HeadingRC;

            for (int i = 1; i < way.Count; i++)
            {
                Position robo = way[i - 1];
                Position next = way[i];

                int turnsLocal = 0;
                while (Map.AreaOnTheFront(heading, robo) != next)
                {
                    heading = TurnRightRC(heading);
                    turnsLocal++;
                }
                if (turnsLocal == 3) turnsLocal = 1;
                turnsTotal += turnsLocal;
            }

            return turnsTotal;
        }

        /// <summary>
        /// returns true if the end of the way is reached, else stops before wall and returns false
        /// </summary>
        /// <param name="way"></param>
        private bool MoveOnWay(List<Position> way)
        {
            int i = 1;
            bool wallAhead = false;

            while (i < way.Count && !wallAhead)
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

                wallAhead = WallOnTheFront();
                if (!wallAhead) MoveAndUpdatePositonOnBothMap();

                //DisplayTwoMaps(MapExternal, MapOwn);
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
                //DisplayTwoMaps(MapExternal, MapOwn);

            } while (!WallOnTheFront() && !CleanedOnTheFront() ||
                    !WallOnTheLeft() && !CleanedOnTheLeft() ||
                    !WallOnTheRight() && !CleanedOnTheRight());

            return CleanedOnTheFront() && CleanedOnTheLeft() && CleanedOnTheRight();
        }
        private void RecordBarriersAround()
        {
            sbyte figWall = (sbyte)Figure.Wall;

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
            //DisplayTwoMaps(MapExternal, barrierMap);

            int shiftY = MapOwn.PositionRC.Y - barrierMap.PositionRC.Y;
            int shiftX = MapOwn.PositionRC.X - barrierMap.PositionRC.X;

            for (int y = 0; y < barrierMap.Coordinates.Length; y++)
            {
                for (int x = 0; x < barrierMap.Coordinates[y].Length; x++)
                {
                    Position p = new Position(x, y);
                    sbyte figure = barrierMap.CoordinateFigureByPosition(p);
                    if (figure != 0) MapOwn.RefreshCoordinate(new Position(p.Y + shiftY, p.X + shiftX), figure);
                }
            }
            //DisplayTwoMaps(MapExternal, MapOwn);


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
            //DisplayTwoMaps(MapExternal, newMap);

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

                MoveOnMapGUI();
                PositionDetectingRC = Map.AreaOnTheFront(HeadingRC, PositionDetectingRC);

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
                checkPos(pos.ShiftUp());
                checkPos(pos.ShiftRight());
                checkPos(pos.ShiftDown());
                checkPos(pos.ShiftLeft());
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
        private Point RcPositionOnMapGUI()
        {
            return RC.TranslatePoint(new Point(0, 0), MapGUI);
        }
        private Point RcMiddleOnMapGUI()
        {
            return RC.TranslatePoint(new Point(RC.Width / 2, RC.Height / 2), MapGUI);
        }
        private bool IsPointHitsWall(Point p)
        {
            HitTestResult res = VisualTreeHelper.HitTest(MapGUI, p);
            return res.VisualHit is Rectangle rect && rect.Name == Application.Current.FindResource("strWall").ToString();
        }
        private bool WallOnTheLeft()
        {
            Position left = Map.AreaOnTheLeft(HeadingRC, RcMiddleOnMapGUI().ToPosition(), (int)RC.Width);
            return IsPointHitsWall(left.ToPoint());
        }
        private bool WallOnTheFront()
        {
            Position front = Map.AreaOnTheFront(HeadingRC, RcMiddleOnMapGUI().ToPosition(), (int)RC.Width);
            return IsPointHitsWall(front.ToPoint());
        }
        private bool WallOnTheRight()
        {
            Position right = Map.AreaOnTheRight(HeadingRC, RcMiddleOnMapGUI().ToPosition(), (int)RC.Width);
            return IsPointHitsWall(right.ToPoint());
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

        private Heading TurnLeftRC(Heading heading)
        {

            switch (heading)
            {
                case Heading.Up:
                    heading = Heading.Left;
                    break;
                case Heading.Down:
                    heading = Heading.Right;
                    break;
                case Heading.Left:
                    heading = Heading.Down;
                    break;
                case Heading.Right:
                    heading = Heading.Up;
                    break;
                    //default:
                    //    break;
            }
            return heading;
        }
        private void TurnLeftRC()
        {
            HeadingRC = TurnLeftRC(HeadingRC);
        }
        private Heading TurnRightRC(Heading heading)
        {
            switch (heading)
            {
                case Heading.Up:
                    heading = Heading.Right;
                    break;
                case Heading.Down:
                    heading = Heading.Left;
                    break;
                case Heading.Left:
                    heading = Heading.Up;
                    break;
                case Heading.Right:
                    heading = Heading.Down;
                    break;
                    //default:
                    //    break;
            }
            return heading;
        }
        private void TurnRightRC()
        {
            HeadingRC = TurnRightRC(HeadingRC);
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
                    pos = Map.AreaOnTheFront(HeadingRC, pos);
                    cnt++;
                }

                int nextPosFigure()
                {
                    return MapOwn.CoordinateFigureByPosition(Map.AreaOnTheFront(HeadingRC, pos));
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
        private void MoveAndUpdatePositonOnBothMap()
        {
            MapOwn.MoveOnMapAndUpdatePositionRC(HeadingRC, MapOwn.AreaOnTheFront(HeadingRC));
            MoveOnMapGUI();
        }
        private void MoveOnMapGUI()
        {
            Rectangle cleanedArea = new Rectangle
            {
                Width = RC.Width,
                Height = RC.Height,
                Fill = Brushes.GreenYellow,
                Name = "cleaned"
            };

            Point oldPos = RcPositionOnMapGUI();

            Canvas.SetTop(cleanedArea, oldPos.Y);
            Canvas.SetLeft(cleanedArea, oldPos.X);
            MapGUI.Children.Add(cleanedArea);

            Point newPos = Map.AreaOnTheFront(HeadingRC, RcPositionOnMapGUI().ToPosition(), (int)RC.Width).ToPoint();
            MapGUI.Children.Remove(RC);
            Canvas.SetTop(RC, newPos.Y);
            Canvas.SetLeft(RC, newPos.X);
            MapGUI.Children.Add(RC);

            //MapGUI.UpdateLayout();

            Action update = () => MapGUI.UpdateLayout();
            MapGUI.Dispatcher.Invoke(update, System.Windows.Threading.DispatcherPriority.Render);
            Thread.Sleep(200);

        }

        private void DisplayMap(Map map)
        {
            //Console.Clear();
            //map.Display();
            //Thread.Sleep(MapRefreshTime);
        }
        private void DisplayTwoMaps(Map map1, Map map2)
        {
            //Console.Clear();
            //map1.Display();
            //Console.WriteLine();
            //map2.Display();
            //Thread.Sleep(MapRefreshTime);
        }

    }
}
