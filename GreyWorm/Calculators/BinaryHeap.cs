using System;

namespace GreyWorm
{
    public class BinaryHeap
    {
        private readonly Point[] _open;
        private readonly long[] _summedCost;
        private readonly int[] _openList;
        private int _openListItemId;

        public int NumberOfOpenListItems { get; private set; }

        public BinaryHeap(int mapWidth, int mapHeight)
        {
            _open = new Point[mapWidth * mapHeight];
            _openList = new int[mapWidth * mapHeight];
            _summedCost = new long[mapWidth * mapHeight];
        }

        public void AddStartPoint(Point point)
        {
            NumberOfOpenListItems = 1;
            _openList[1] = 1;
            _open[1] = point;
        }

        public Point Pop()
        {
            var point = _open[_openList[1]];

            _openList[1] = _openList[NumberOfOpenListItems];
            NumberOfOpenListItems -= 1;

            HandleDeleteFromBinaryHeap();
            return point;
        }

        public int AddPoint(Point point, long cost)
        {
            _openListItemId += 1;
            _open[_openListItemId] = point;

            NumberOfOpenListItems += 1;
            _openList[NumberOfOpenListItems] = _openListItemId;

            _summedCost[_openListItemId] = cost;
            UpdateBinaryHeap(NumberOfOpenListItems);

            return _openListItemId;
        }

        public int Find(Point point)
        {
            for (var i = 1; i <= NumberOfOpenListItems; i++)
            {
                if (_open[_openList[i]] == point) return i;
            }

            throw new InvalidOperationException("Point not found from open list");
        }

        public void UpdatePoint(int id, long cost)
        {
            _summedCost[_openList[id]] = cost;
            UpdateBinaryHeap(id);
        }

        private void HandleDeleteFromBinaryHeap()
        {
            var v = 1;
            while (true)
            {
                var u = v;
                if (2 * u + 1 <= NumberOfOpenListItems)
                {
                    if (_summedCost[_openList[u]] >= _summedCost[_openList[2 * u]])
                        v = 2 * u;
                    if (_summedCost[_openList[v]] >= _summedCost[_openList[2 * u + 1]])
                        v = 2 * u + 1;
                }
                else
                {
                    if (2 * u <= NumberOfOpenListItems)
                    {
                        if (_summedCost[_openList[u]] >= _summedCost[_openList[2 * u]])
                            v = 2 * u;
                    }
                }

                if (u != v)
                {
                    var temp = _openList[u];
                    _openList[u] = _openList[v];
                    _openList[v] = temp;
                }
                else break;
            }
        }


        private void UpdateBinaryHeap(int m)
        {
            while (m != 1)
            {
                if (_summedCost[_openList[m]] <= _summedCost[_openList[m / 2]])
                {
                    var temp = _openList[m / 2];
                    _openList[m / 2] = _openList[m];
                    _openList[m] = temp;
                    m = m / 2;
                }
                else break;
            }
        }
    }
}