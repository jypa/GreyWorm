using System.Linq;
using Network;

namespace GreyWorm
{
    public class GotoApple : IMoveDecider
    {
        private readonly IMoveDecider _another;

        public GotoApple(IMoveDecider another)
        {
            _another = another;
        }

        public Maybe<Direction> Decide(Level level)
        {
            var direction = _another.Decide(level);

            if (direction.Any()) return direction;

            if (!level.MyShortestPath.Any() || (!level.MyShortestPath.Any() && !level.EnemyShortestPath.Any())) return Maybe.Empty<Direction>();

            if (level.MyLength < 40)
            {
                return level.EnemyShortestPath.Count < level.MyShortestPath.Count ? Maybe.Empty<Direction>() : DecideDirection(level.MyHead, level.MyShortestPath.First());
            }

            return DecideDirection(level.MyHead, level.MyShortestPath.First());
        }

        private static Maybe<Direction> DecideDirection(Point from, Point to)
        {
            if (from.Y > to.Y) return Direction.Down.ToMaybe();
            if (from.Y < to.Y) return Direction.Up.ToMaybe();
            if (from.X > to.X) return Direction.Left.ToMaybe();
            if (from.X < to.X) return Direction.Right.ToMaybe();

            return Maybe.Empty<Direction>();
        }
    }
}