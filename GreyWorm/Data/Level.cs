using System.Collections.Generic;
using System.Diagnostics;

namespace GreyWorm
{
    public class Level
    {
        public Point MyHead { get; set; }
        public Point Apple { get; set; }
        public Point EnemyHead { get; set; }
        public PointArray<long> Map { get; set; }
        public int MyLength { get; set; }
        public List<Point> MyShortestPath { get; set; }
        public List<Point> EnemyShortestPath { get; set; }
        public Level()
        {
            MyShortestPath = new List<Point>();
            EnemyShortestPath = new List<Point>();
        }
    }
}