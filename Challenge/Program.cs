using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Challenge
{
    class Program
    {
        static void Main(string[] args)
        {
            new Presentation().Start();

            Console.Read();
        }
    }

    class Presentation
    {
        static Random Rnd;
        static Map MapExternal;
        public void Start()
        {
            Rnd = new Random();
            Map MapExternal = new Map(TwoRooms(15, 20, 10, 5), new Position(1,1));
            //MapExternal.PutOnMapRandom();
            RoboCleaner rc = new RoboCleaner(MapExternal);
            rc.StartCleaning();
            
            
        }
        public int[][] MapCoordinates1()
        {
            int[][] Map;
            int Rows = 14;
            int Columns = 15;
            int Columns2 = 18;

            Map = new int[Rows][];
            for (int i = 0; i < Rows; i++)
            {
                if (i >= 8) Map[i] = new int[Columns2];
                else Map[i] = new int[Columns];


            }

            for (int row = 0; row < Map.Length; row++)
            {
                for (int col = 0; col < Map[row].Length; col++)
                {
                    if (row == 0 || col == 0 || row == Map.Length - 1 || col == Map[row].Length - 1
                        || row == 8)
                    {
                        Map[row][col] = 1;
                    }
                }
            }

            Map[8][5] = 0;
            Map[8][6] = 0;
            Map[8][7] = 0;
            Map[8][8] = 0;

            //Map[1][1] = 1;

            //Map[1][7] = 1;
            //Map[2][7] = 1;


            //Map[1][9] = 1;

            Map[1][10] = 1;
            Map[1][11] = 1;
            Map[1][12] = 1;

            Map[10][13] = 1;
            Map[10][14] = 1;
            Map[10][15] = 1;
            Map[10][16] = 1;

            Map[11][13] = 1;
            Map[11][14] = 1;
            Map[11][15] = 1;
            Map[11][16] = 1;

            Map[10][1] = 1;
            Map[11][1] = 1;
            Map[12][1] = 1;



            //figure =x
            Map[3][5] = 1;
            Map[3][6] = 1;
            Map[5][5] = 1;
            Map[5][6] = 1;

            Map[4][7] = 1;

            Map[3][8] = 1;
            Map[5][8] = 1;

            return Map;

        }

        public int[][] MapCoordinates2()
        {
            int[][] Map;
            int Rows = 8;
            int Columns = 6;
            int Columns2 = 8;

            Map = new int[Rows][];
            for (int i = 0; i < Rows; i++)
            {
                if (i >= 3) Map[i] = new int[Columns2];
                else Map[i] = new int[Columns];


            }

            for (int row = 0; row < Map.Length; row++)
            {
                for (int col = 0; col < Map[row].Length; col++)
                {
                    if (row == 0 || col == 0 || row == Map.Length - 1 || col == Map[row].Length - 1
                        || row == 3)
                    {
                        Map[row][col] = 1;
                    }
                }
            }

            Map[3][3] = 0;

            return Map;
        }

        public int[][] TwoRooms(int xRoom1, int xRoom2, int yFlat, int yWall)
        {
            int[][] Map;
            (int bigRoom, int smallRoom) roomSize()
            {
                if (xRoom1 > xRoom2)
                {
                    return (xRoom1, xRoom2);
                }
                else
                {
                    return (xRoom2, xRoom1);
                }
            }

            Map = new int[yFlat][];
            for (int i = 0; i < yFlat; i++)
            {
                if (i > yWall) Map[i] = new int[xRoom2];
                else if (i == yWall) Map[i] = new int[roomSize().bigRoom];
                else Map[i] = new int[xRoom1];
            }

            for (int row = 0; row < Map.Length; row++)
            {
                for (int col = 0; col < Map[row].Length; col++)
                {
                    if (row == 0 || col == 0 || row == Map.Length - 1 || col == Map[row].Length - 1
                        || row == yWall)
                    {
                        Map[row][col] = 1;
                    }
                }
            }


            int xDoor = Rnd.Next(2, roomSize().smallRoom - 1);

            Map[yWall][xDoor] = 0;
            Map[yWall][xDoor - 1] = 0;

            return Map;
        }

    }
}
