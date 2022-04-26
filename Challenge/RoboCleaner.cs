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
            MapOwn = new Map(Map.initMap(10,10),new Position(0,0));
            MapRefreshTime = 1000;
        }
        public void SetStartHeading()
        {

        }

        public void StartCleaning()
        {
            MapOwn = createOwnMap();
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
        public (List<Position> cleanedPath, List<Position> barriers) DetectRoomEdges()
        {
            List<Position> cleanedPathCoordinates = new List<Position>();
            HashSet<Position> barriersCoordinates = new HashSet<Position>();
            Position PositionDetectingRC = new Position(0, 0);

            int[] figuresNotAllowed = { (int)Figure.Wall };
            bool freeLeftEdgeDetection()
            {
                return CheckIfFree(MapExternal, MapExternal.AreaOnTheLeft(HeadingRC), figuresNotAllowed);
            }
            bool freeFrontEdgeDetection()
            {
                return CheckIfFree(MapExternal, MapExternal.AreaOnTheFront(HeadingRC), figuresNotAllowed);
            }
            bool freeRightEdgeDetection()
            {
                return CheckIfFree(MapExternal, MapExternal.AreaOnTheRight(HeadingRC), figuresNotAllowed);
            }

            DisplayMap(MapExternal);

            //find start direction
            //before the first move Robo is heading up
            void turnRightWhileFrontNotFree()
            {
                int i = 0;
                while (i < 3 && !freeFrontEdgeDetection())
                {
                    TurnRightRC();
                    RecordBarrier();
                    i++;
                }
            }
            void RecordBarrier()
            {
                if (!freeLeftEdgeDetection())
                {
                    barriersCoordinates.Add(MapExternal.AreaOnTheLeft(HeadingRC, PositionDetectingRC));
                }
            }
            void RecordPath()
            {
                cleanedPathCoordinates.Add(PositionDetectingRC);
            }

            RecordPath();
            if (freeLeftEdgeDetection() && freeFrontEdgeDetection() && freeRightEdgeDetection())
            {
                TurnLeftRC();
                RecordBarrier();
            }
            else if (!freeRightEdgeDetection())
            {
                TurnRightRC();
                RecordBarrier();
                turnRightWhileFrontNotFree();
            }
            else
            {
                RecordBarrier();
                turnRightWhileFrontNotFree();
            }
            MapExternal.MoveOnMapAndUpdatePositionRC(HeadingRC, MoveForwardRC(MapExternal.PositionRC));
            PositionDetectingRC = MoveForwardRC(PositionDetectingRC);
            RecordBarrier();
            RecordPath();
            DisplayMap(MapExternal);

            //record room edges
            do
            {
                if (freeLeftEdgeDetection())
                {
                    TurnLeftRC();
                }
                else if (freeFrontEdgeDetection())
                {
                    ;
                }
                else if (freeRightEdgeDetection())
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
                PositionDetectingRC =  MoveForwardRC(PositionDetectingRC);
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
        private Map createOwnMap()
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

            Map newMap = new Map(Map.initMap(yMax - yMin + 1,xMax - xMin + 1), new Position(0,0));
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
