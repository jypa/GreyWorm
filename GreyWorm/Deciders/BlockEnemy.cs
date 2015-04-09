using System;
using System.Linq;
using Network;

namespace GreyWorm
{
    public class BlockEnemy : IMoveDecider
    {
        private readonly bool _dontBlock;

        public BlockEnemy(bool dontBlock)
        {
            _dontBlock = dontBlock;
        }

        public Maybe<Direction> Decide(Level level)
        {
            if (_dontBlock || !level.EnemyShortestPath.Any()) return Maybe.Empty<Direction>();

            if (IsBetween(level.MyHead.Y, level.Apple.Y, level.EnemyHead.Y))
            {
                foreach (var point in level.EnemyShortestPath)
                {
                    if (point.Y != level.MyHead.Y) continue;

                    var direction = BlockHorizontal(point, level);
                    if (direction.Any()) return direction;
                    break;
                }
            }

            if (IsBetween(level.MyHead.X, level.Apple.X, level.EnemyHead.X))
            {
                foreach (var point in level.EnemyShortestPath)
                {
                    if (point.X != level.MyHead.X) continue;

                    var direction = BlockVertical(point, level);
                    if (direction.Any()) return direction;
                    break;
                }
            }

            return Maybe.Empty<Direction>();
        }

        private static Maybe<Direction> BlockVertical(Point point, Level level)
        {
            var direction = point.Y > level.MyHead.Y ? Direction.Up : Direction.Down;

            var y = level.MyHead.Y;

            if (direction == Direction.Up)
            {
                do
                {
                    y += 1;
                } while (y < level.Map.Height && level.Map[level.MyHead.X, y] == 0);
                y -= 1;
            }

            if (direction == Direction.Down)
            {
                do
                {
                    y -= 1;
                } while (y >= 0 && level.Map[level.MyHead.X, y] == 0);
                y += 1;
            }

            var blockPoint = new Point(level.MyHead.X, y);

            return TryBlocking(level, blockPoint, direction);
        }

        private static Maybe<Direction> BlockHorizontal(Point point, Level level)
        {
            var direction = point.X > level.MyHead.X ? Direction.Right : Direction.Left;

            var x = level.MyHead.X;

            if (direction == Direction.Right)
            {
                do
                {
                    x += 1;
                } while (x < level.Map.Width && level.Map[x, level.MyHead.Y] == 0);
                x -= 1;
            }

            if (direction == Direction.Left)
            {
                do
                {
                    x -= 1;
                } while (x >= 0 && level.Map[x, level.MyHead.Y] == 0);
                x += 1;
            }

            var blockPoint = new Point(x, level.MyHead.Y);

            return TryBlocking(level, blockPoint, direction);
        }

        private static Maybe<Direction> TryBlocking(Level level, Point blockPoint, Direction direction)
        {
            const int maxBlockingDistance = 30;
            const int maxEnemyDistance = 60;

            if (blockPoint == level.MyHead) return Maybe.Empty<Direction>();

            var myDistance = DistanceBetween(level.MyHead, blockPoint);
            var enemyDistance = DistanceBetween(level.EnemyHead, blockPoint);
            var newPoint = PointAfterMove(direction, level.MyHead);

	        if (myDistance == 1 && EnemyNotInShoot(level))
	        {
		        Direction newDirection;

		        if (direction == Direction.Up || direction == Direction.Down)
		        {
					newDirection = level.MyHead.X > level.Apple.X ? Direction.Left : Direction.Right;
		        }
		        else
		        {
					newDirection = level.MyHead.Y > level.Apple.Y ? Direction.Down : Direction.Up;
				}

		        if (CanGo(level, PointAfterMove(newDirection, level.MyHead)))
		        {
					return newDirection.ToMaybe();
		        }
			}


            if (myDistance + 2 < enemyDistance && 
                myDistance < level.MyLength && 
                enemyDistance < maxEnemyDistance &&
                myDistance < maxBlockingDistance && 
                level.Map[newPoint] < 2)
            {
                return direction.ToMaybe();
            }

            return Maybe.Empty<Direction>();
        }

	    private static bool EnemyNotInShoot(Level level)
	    {
		    var possibleDirectionCount = (Enum.GetValues(typeof (Direction))
				.Cast<Direction>()
			    .Select(direction => PointAfterMove(direction, level.EnemyHead)))
				.Count(point => CanGo(level, point));

		    return possibleDirectionCount != 1;
	    }

	    private static bool CanGo(Level level, Point point)
	    {
		    return point.X >= 0 && point.X < level.Map.Width && point.Y >= 0 && point.Y < level.Map.Height && level.Map[point] < 2;
	    }

	    private static long DistanceBetween(Point point, Point point2)
        {
            return (Math.Abs(point.X - point2.X) + Math.Abs(point.Y - point2.Y));
        }

        private static bool IsBetween(long l, long l1, long l2)
        {
            return l > Math.Min(l1, l2) && l < Math.Max(l1, l2);
        }

        private static Point PointAfterMove(Direction direction, Point point)
        {
            switch (direction)
            {
            case Direction.Up:
                return new Point(point.X, point.Y + 1);
            case Direction.Down:
                return new Point(point.X, point.Y - 1);
            case Direction.Left:
                return new Point(point.X - 1, point.Y);
            case Direction.Right:
                return new Point(point.X + 1, point.Y);
            default:
                throw new ArgumentOutOfRangeException("direction");
            }
        }
    }
}