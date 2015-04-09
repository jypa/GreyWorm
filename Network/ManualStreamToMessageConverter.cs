using System;
using System.IO;
using System.Text;
using Network.Messages;
using Newtonsoft.Json;

namespace Network
{
    public class ManualStreamToMessageConverter : IStreamToMessageConverter
    {
        private const int ReadBufferSize = 2014;
        private const int ExpectedStringbuilderSize = ReadBufferSize * 6;

        public void HandleStream(Stream stream, Action<BaseMessage> messageReceived)
        {
            using (var sr = new StreamReader(stream, Encoding.ASCII, false, 2014, true))
            {
                var readBuffer = new char[ReadBufferSize];
                var builder = new StringBuilder(ExpectedStringbuilderSize);
                var notEndedBrackets = 0;
                var read = sr.Read(readBuffer, 0, readBuffer.Length);

                try
                {
                    while (read > 0)
                    {
                        for (var i = 0; i < read; i++)
                        {
                            notEndedBrackets += BracketStatusDelta(readBuffer[i]);
                            builder.Append(readBuffer[i]);

                            if (JsonMessageHasEnded(notEndedBrackets))
                            {
                                var message = JsonConvert.DeserializeObject<BaseMessage>(builder.ToString());
                                messageReceived(message);
                                builder = new StringBuilder(ExpectedStringbuilderSize);
                            }
                        }

                        read = sr.Read(readBuffer, 0, readBuffer.Length);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
        }

        private static bool JsonMessageHasEnded(int notEndedBrackets)
        {
            return notEndedBrackets == 0;
        }

        private static int BracketStatusDelta(char c)
        {
            switch (c)
            {
            case '{':
                return 1;
            case '}':
                return -1;
            default:
                return 0;
            }
        }
    }
}