using Network;

namespace GreyWorm
{
    public interface IMoveDecider
    {
        Maybe<Direction> Decide(Level level);
    }
}