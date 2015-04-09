using System;
using System.IO;
using Network.Messages;

namespace Network
{
    public interface IStreamToMessageConverter
    {
        void HandleStream(Stream stream, Action<BaseMessage> messageReceived);
    }
}