namespace GreyWorm
{
    public class PointArray<T>
    {
        private readonly T[,] _array;

        public T this[Point p]
        {
            get
            {
                return _array[p.X, p.Y];
            }
            set
            {
                _array[p.X, p.Y] = value;
            }
        }

        public T this[long x, long y]
        {
            get
            {
                return _array[x,y];
            }
            set
            {
                _array[x,y] = value;
            }
        }

        public PointArray(int width, int height)
        {
            _array = new T[width, height];
        }

        public int Width { get {return _array.GetLength(0); } }
        public int Height { get {return _array.GetLength(1); } }

    }
}