namespace GreyWorm
{
    public class PositionsUpdateDto
    {
        public PointArray<long> Map { get; set; }
        public Point MyHead { get; set; }
        public Point EnemyHead { get; set; }
        public int MyLength { get; set; }
    }
}