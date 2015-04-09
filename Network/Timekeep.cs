using System;
using System.Diagnostics;

namespace Network
{
    public static class Timekeep
    {
	    private static Stopwatch _stopwatch;

	    public static void Start()
        {
	        _stopwatch = new Stopwatch();
			_stopwatch.Restart();
        }

        public static void Stop(int alertLimit)
        {
			_stopwatch.Stop();
            DisplayTime(_stopwatch.ElapsedMilliseconds, alertLimit);
        }

        private static void DisplayTime(double millis, int alertLimit)
        {
            if (millis < alertLimit) return;
            
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(millis);
            Console.ResetColor();
        }
    }
}