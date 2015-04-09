using System;
using System.Linq;
using Network;

namespace GreyWorm
{
    public class GotoCenter : IMoveDecider
    {
        private readonly IMoveDecider _another;

        public GotoCenter(IMoveDecider another)
        {
            _another = another;
        }

        public Maybe<Direction> Decide(Level level)
        {
            var direction = _another.Decide(level);

            if (direction.Any())
            {
                return direction;
            }

            return level.MyLength > 40 ? Maybe.Empty<Direction>() : MoveToCenter(level);
        }

        private static Maybe<Direction> MoveToCenter(Level level)
        {
            // If we already are in center of map
            if (level.MyHead.X > level.Map.Width/2 - 10 && level.MyHead.X < level.Map.Width/2 + 10 &&
                level.MyHead.Y > level.Map.Height/2 - 10 && level.MyHead.Y < level.Map.Height/2 + 10)
            {
                return Maybe.Empty<Direction>();
            }

            var i = 0;
            var r = new Random();
            var randomCenterPoint = new Point(level.Map.Width/2, level.Map.Height/2);
            while (i++ < 3)
            {
                var path = PathFinder.Init(level.Map).FindPath(level.MyHead, randomCenterPoint, 100);
                if (path.Any())
                {
                    return DecideDirection(level.MyHead, path.First());
                }

                randomCenterPoint = new Point(level.Map.Width / 2 + r.Next(-10, 10), level.Map.Height / 2 + r.Next(-10, 10));
            }

            return Maybe.Empty<Direction>();
        }


        private static Maybe<Direction> DecideDirection(Point from, Point to)
        {
            if (from.Y > to.Y) return Direction.Down.ToMaybe();
            if (from.Y < to.Y) return Direction.Up.ToMaybe();
            if (from.X > to.X) return Direction.Left.ToMaybe();
            if (from.X < to.X) return Direction.Right.ToMaybe();

            throw new InvalidOperationException("Can't decide direction because the points are same");
        }
    }
}