using System;
using CommandLine;
using Network;

namespace GreyWorm.Console
{
    class Program
    {
        class Options
        {
            [Option('n', DefaultValue = "GreyWorm", HelpText = "Name of the worm")]
            public string Name { get; set; }

            [Option('a', DefaultValue = "localhost", HelpText = "Server address")]
            public string Address { get; set; }

            [Option('p', DefaultValue = 6969, HelpText = "Server port")]
            public int Port { get; set; }

            [Option('b', DefaultValue = false, HelpText = "Disable blocking other worms")]
            public bool Block { get; set; }

			[Option('t', DefaultValue = 100, HelpText = "Adjust tick")]
			public int TickAdjust { get; set; }
		}

        static void Main(string[] args)
        {
            var options = new Options();
            if (!Parser.Default.ParseArguments(args, options)) return;

            var settings = new Settings
            {
                Name = options.Name,
                DontBlock = options.Block,
				PathFinderGiveUpLimit = options.TickAdjust * 9,
				FillGiveUpLimit = options.TickAdjust * 4
			};

            var converter = new ManualStreamToMessageConverter();
            var communicator = new Communicator(options.Address, options.Port, converter);
            var gameClient = new GameClient(communicator, settings);

            try
            {
                communicator.Connect();
                gameClient.StartGame();
            }
            catch (Exception e)
            {
                System.Console.WriteLine("Exception: " + e.Message);
            }
            finally
            {
                System.Console.Read();
                communicator.Dispose();
            }
        }
    }
}
