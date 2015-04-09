using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Network;
using Network.Messages;

namespace GreyWorm
{
    public class GreyWorm
    {
        private readonly Communicator _communicator;
		private readonly Settings _settings;
		private readonly IMoveDecider _decider;
		private readonly Level _level;

        private bool _hasApple;
        private bool _hasPositions;

	    public GreyWorm(Communicator communicator, Settings settings)
        {
            _communicator = communicator;
			_settings = settings;

			_level = new Level();
	        
            var blockEnemy = new BlockEnemy(settings.DontBlock);
            var gotoApple = new GotoApple(blockEnemy);
            var gotoCenter = new GotoCenter(gotoApple);
            var avoidCollosion = new AvoidCollosion(gotoCenter, settings.FillGiveUpLimit);
            _decider = avoidCollosion;
        }

        public void UpdatePositions(PositionsUpdateDto positionsUpdate)
        {
            _level.MyHead = positionsUpdate.MyHead;
            _level.EnemyHead = positionsUpdate.EnemyHead;
            _level.Map = positionsUpdate.Map;
            _level.MyLength = positionsUpdate.MyLength;

            _hasPositions = true;
            DecideMove();
        }
        public void UpdateApple(Point apple)
        {
            _level.Apple = apple;

            _hasApple = true;
            DecideMove();
        }

        private void DecideMove()
        {
            if (!_hasPositions) return;

            Timekeep.Start();

            if (_hasApple)
            {
                FindPaths();
            }
            
            var direction = _decider.Decide(_level);
            
            if (direction.Any())
            {
                _communicator.Send(new ControlMessage(direction.Single()));
            }

            Timekeep.Stop(10);
        }

        private void FindPaths()
        {
            if (_level.Map[_level.Apple] != 0)
            {
                _level.MyShortestPath = new List<Point>();
                _level.EnemyShortestPath = new List<Point>();
            }

            try
            {
                var t1 = Task.Factory.StartNew(() => PathFinder.Init(_level.Map).FindPath(_level.MyHead, _level.Apple, _settings.PathFinderGiveUpLimit));
                var t2 = Task.Factory.StartNew(() => PathFinder.Init(_level.Map).FindPath(_level.EnemyHead, _level.Apple, _settings.PathFinderGiveUpLimit));

                Task.WaitAll(t1, t2);

                _level.MyShortestPath = t1.Result;
                _level.EnemyShortestPath = t2.Result;

            }
            catch (AggregateException)
            {
                
            }
        }
    }
}