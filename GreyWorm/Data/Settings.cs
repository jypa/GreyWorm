namespace GreyWorm
{
    public class Settings
    {
        public bool DontBlock { get; set; }
        public string Name { get; set; }
	    public int PathFinderGiveUpLimit { get; set; }
	    public int FillGiveUpLimit { get; set; }
    }
}