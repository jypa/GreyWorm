using System;
using Network;
using Network.Messages;

namespace GreyWorm
{
    public class GameClient
    {
        private int _width;
        private int _height;
        private readonly string _myName;
        
        private readonly Communicator _communicator;
        private readonly GreyWorm _greyWorm;

        private int _playerNo;
        private int _enemyNo;

        public GameClient(Communicator communicator, Settings settings)
        {
            _communicator = communicator;
            _communicator.MessageEvent = HandleMessage;
            _myName = settings.Name;
            _greyWorm = new GreyWorm(communicator, settings);

            if (settings.DontBlock)
            {
                Console.WriteLine("Blocking is disabled");
            }
        }

        private void HandleMessage(BaseMessage message)
        {
            if (message == null) throw new ArgumentNullException("message");

            try
            {
                dynamic data = message.data;

                switch (message.msg)
                {
                    case "create":
                        break;
                    case "start":
                        StartMessage(data);
                        break;
                    case "positions":
                        PositionsMessage(data);
                        break;
                    case "apple":
                        AppleMessage(data);
                        break;
                    case "end":
                        EndMessage(data);
                        break;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine();
                Console.WriteLine();
                Console.WriteLine(e);
            }
        }

        private void EndMessage(dynamic data)
        {
            Console.WriteLine("Game endend");

            if (data.winners.Count != 1)
            {
                Console.WriteLine("Game is a tie");
            }
            else if (data.winners[0] == _playerNo)
            {
                Console.WriteLine("I won");
            }
            else if (data.winners[0] == _enemyNo)
            {
                Console.WriteLine("I lost");
            }
        }

        private void AppleMessage(dynamic data)
        {
            long x = data[0];
            long y = data[1];
            var apple = new Point(x, y);
            _greyWorm.UpdateApple(apple);
        }

        private void PositionsMessage(dynamic data)
        {
            var map = new PointArray<long>(_width, _height);

            var snakes = data.snakes;

            for (var i = 0; i < snakes.Count; i++)
            {
                for (var j = 0; j < snakes[i].body.Count; j++)
                {
                    long x = snakes[i].body[j][0];
                    long y = snakes[i].body[j][1];

                    if (x < 0 || y < 0 || x >= _width || y >= _height)
                    {
                        return;
                    }

                    map[x, y] = snakes[i].body.Count - i;
                }
            }

            long headX = snakes[_playerNo].body[0][0];
            long headY = snakes[_playerNo].body[0][1];
            var myHeadPoisition = new Point(headX, headY);

            headX = snakes[_enemyNo].body[0][0];
            headY = snakes[_enemyNo].body[0][1];
            var enemyHeadPoisition = new Point(headX, headY);

            var positionsUpdateDto = new PositionsUpdateDto
            {
                Map = map,
                MyHead = myHeadPoisition,
                EnemyHead = enemyHeadPoisition,
                MyLength = snakes[_playerNo].body.Count
            };

            _greyWorm.UpdatePositions(positionsUpdateDto);
        }

        private void StartMessage(dynamic data)
        {
            _width = data.level.width;
            _height = data.level.height;

            for (var i = 0; i < data.players.Count; i++)
            {
                if (data.players[i].name == _myName)
                {
                    _playerNo = i;
                }
                else
                {
                    _enemyNo = i;
                }
            }

            Console.WriteLine("Game started, size {0}x{1}",_width, _height);
        }

        public void StartGame()
        {
            _communicator.Send(new JoinMessage(_myName));
        }
    }
}
