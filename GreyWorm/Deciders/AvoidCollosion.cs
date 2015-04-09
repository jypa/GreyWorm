using System;
using System.Collections.Generic;
using System.Linq;
using Network;

namespace GreyWorm
{
    public class AvoidCollosion : IMoveDecider
    {
        private readonly IMoveDecider _another;
	    private readonly int _fillGiveUpLimit;
	    private static Level _level;

        public AvoidCollosion(IMoveDecider another, int fillGiveUpLimit)
        {
	        _another = another;
	        _fillGiveUpLimit = fillGiveUpLimit;
        }

	    public Maybe<Direction> Decide(Level level)
        {
            _level = level;

            var direction = _another.Decide(level);

            if (direction.Any())
            {
                return IsTie(MyPointAfterMove(direction.Single())) ? SafeMove() : direction;
            }

            return SafeMove();
        }

        private Maybe<Direction> SafeMove()
        {
            var possibleDirections = Enum.GetValues(typeof(Direction))
                .Cast<Direction>()
                .Where(CanGo)
                .ToList();

            if (possibleDirections.Count < 1) return Maybe.Empty<Direction>();

            if (possibleDirections.Count == 1) return possibleDirections.Single().ToMaybe();

            return ChooseLargerArea(possibleDirections);
        }

        private Maybe<Direction> ChooseLargerArea(IEnumerable<Direction> directions)
        {
            var orderMap = new Dictionary<Direction, int>()
            {
                {Direction.Down, 0},
                {Direction.Right, 1},
                {Direction.Up, 2},
                {Direction.Left, 3}
            };
      
            return directions.AsParallel().Select(direction =>
                new Fill
                {
                    Direction = direction,
                    Count = FloodFill(MyPointAfterMove(direction))
                })
                .OrderByDescending(fill => fill.Count)
                .ThenBy(fill => orderMap[fill.Direction])
                .First().Direction.ToMaybe();

        }

        private static bool CanGo(Direction direction)
        {
            var newPoint = MyPointAfterMove(direction);
            return PointIsOpen(newPoint);
        }

        private static bool PointIsOpen(Point point)
        {
            return IsOnMap(point) && _level.Map[point] < 2 && !IsTie(point);
        }

        private static bool IsOnMap(Point point)
        {
            return point.X >= 0 && point.X < _level.Map.Width && point.Y >= 0 && point.Y < _level.Map.Height;
        }

        private static bool IsTie(Point point)
        {
            return _level.EnemyShortestPath.Any() && _level.EnemyShortestPath.First() == point;
        }

        private static Point MyPointAfterMove(Direction direction)
        {
            switch (direction)
            {
                case Direction.Up:
                    return new Point(_level.MyHead.X, _level.MyHead.Y + 1);
                case Direction.Down:
                    return new Point(_level.MyHead.X, _level.MyHead.Y - 1);
                case Direction.Left:
                    return new Point(_level.MyHead.X - 1, _level.MyHead.Y);
                case Direction.Right:
                    return new Point(_level.MyHead.X + 1, _level.MyHead.Y);
                default:
                    throw new ArgumentOutOfRangeException("direction");
            }
        }

        private class Fill
        {
            public Direction Direction { get; set; }
            public int Count { get; set; }
        }

		public int FloodFill(Point startPoint)
		{
			var count = 0;

			var map = _level.Map;

			if (map[startPoint] != 0) return count;

			var painted = new PointArray<bool>(map.Width, map.Height);
			var queue = new Queue<Point>();
			queue.Enqueue(startPoint);

			while (queue.Count > 0)
			{
				var point = queue.Dequeue();

				var west = point.X;
				while (west >= 0 && map[west, point.Y] == 0 && !painted[west, point.Y])
				{
					west -= 1;
				}

				var east = point.X;
				while (east < map.Width && map[east, point.Y] == 0 && !painted[east, point.Y])
				{
					east += 1;
				}

				for (var x = west + 1; x < east; x++)
				{
					painted[x, point.Y] = true;
					count++;

					var north = new Point(x, point.Y + 1);
					if (north.Y < map.Height && map[north] == 0 && !painted[north]) queue.Enqueue(north);

					var south = new Point(x, point.Y - 1);
					if (south.Y >= 0 && map[south] == 0 && !painted[south]) queue.Enqueue(south);
				}

				if (count > _fillGiveUpLimit) return _fillGiveUpLimit;
			}

			return count;
		}

	}
}