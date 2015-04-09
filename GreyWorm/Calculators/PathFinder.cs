using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace GreyWorm
{
    public class PathFinder
    {
        // Map
        private readonly PointArray<long> _wormLocations;
        private readonly int _mapWidth;
        private readonly int _mapHeight;

        // Look up & cache arrays
        private readonly PointArray<int> _movementCost;
        private readonly long[] _estimatedDistanceCost;
        private readonly PointArray<ListStatus> _pointFlag;
        
        private readonly PointArray<Point> _parent;
        private readonly BinaryHeap _binaryHeap;


        private PathFinder(PointArray<long> wormLocations)
        {
            _mapWidth = wormLocations.Width;
            _mapHeight = wormLocations.Height;

            _wormLocations = wormLocations;

            _movementCost = new PointArray<int>(_mapWidth, _mapHeight);

            _parent = new PointArray<Point>(_mapWidth, _mapHeight);
            _pointFlag = new PointArray<ListStatus>(_mapWidth, _mapHeight);
            _estimatedDistanceCost = new long[_mapWidth * _mapHeight];


            _binaryHeap = new BinaryHeap(_mapWidth, _mapHeight);
        }

        public static PathFinder Init(PointArray<long> wormLocations)
        {
            return new PathFinder(wormLocations);
        }

        public List<Point> FindPath(Point start, Point target, int giveUpLimit)
        {
            //	If starting location and target are in the same location...
            if (start.X == target.X && start.Y == target.Y) return new List<Point>();

            _movementCost[start] = 0;
            _binaryHeap.AddStartPoint(start);
            var counter = 0;

            while (true)
            {
                if (counter++ > giveUpLimit) return new List<Point>();

                if (_binaryHeap.NumberOfOpenListItems == 0) return new List<Point>();

                // Pop first item from ALREADY SORTED open list
                var parent = _binaryHeap.Pop();
                _pointFlag[parent] = ListStatus.OnClosedList;

                // Up
                TryToAddOrUpdate(target, new Point(parent.X, parent.Y + 1), parent);

                // Down
                TryToAddOrUpdate(target, new Point(parent.X, parent.Y - 1), parent);

                // Right
                TryToAddOrUpdate(target, new Point(parent.X + 1, parent.Y), parent);

                // Left
                TryToAddOrUpdate(target, new Point(parent.X - 1, parent.Y), parent);

                //If target is added to open list then path has been found.
                if (_pointFlag[target] == ListStatus.OnOpenList)
                {
                    break;
                }
            }

            return ReadPath(start, target);
        }

        private List<Point> ReadPath(Point start, Point target)
        {
            var path = new List<Point>();
            var pathPoint = target;
            do
            {
                path.Insert(0, pathPoint);
                pathPoint = _parent[pathPoint];
            } while (pathPoint != start);
            return path;
        }

        private void TryToAddOrUpdate(Point target, Point newPoint, Point parentPoint)
        {
            if (newPoint.X < 0 || newPoint.Y < 0 || newPoint.X >= _mapWidth || newPoint.Y >= _mapHeight) return;

            if (_pointFlag[newPoint] == ListStatus.OnClosedList) return;
            
            // Check if point is clear when we arrive there
            if (_wormLocations[newPoint] >= _movementCost[parentPoint] + 1) return;

            if (_pointFlag[newPoint] != ListStatus.OnOpenList)
            {
                AddToOpenList(target, newPoint, parentPoint);
            }
            else 
            {
                UpdateOpenList(newPoint, parentPoint);
            }
        }

        private void UpdateOpenList(Point newPoint, Point parentPoint)
        {
            var newMovementCost = _movementCost[parentPoint] + 1;

            // Update Parent, movement cost and summed costs if this path is shorter
            if (newMovementCost >= _movementCost[newPoint]) return;

            _parent[newPoint] = parentPoint;
            _movementCost[newPoint] = newMovementCost;

            // We need to update summed cost in open list
            var id = _binaryHeap.Find(newPoint);
            var newEstimatedDistanceCost = newMovementCost + _estimatedDistanceCost[id];

            _binaryHeap.UpdatePoint(id, newEstimatedDistanceCost);
        }

        private void AddToOpenList(Point target, Point newPoint, Point parentPoint)
        {
            var estimatedDistance = (Math.Abs(newPoint.X - target.X) + Math.Abs(newPoint.Y - target.Y));
            var movementCost = _movementCost[parentPoint] + 1;

            _parent[newPoint] = parentPoint;
            _movementCost[newPoint] = movementCost;
            var id = _binaryHeap.AddPoint(newPoint, movementCost + estimatedDistance);

            _estimatedDistanceCost[id] = estimatedDistance;

            _pointFlag[newPoint] = ListStatus.OnOpenList;
        }

        private enum ListStatus
        {
            NotSet = 0,
            OnOpenList = 1,
            OnClosedList = 2
        }
    }
}