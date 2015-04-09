﻿using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Network.Messages;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Threading.Tasks;

namespace Network
{
    /// <summary>
    /// Purpose of this class is to contain communication implementation details
    /// </summary>
    public class Communicator
    {
        private const int ReadBufferSize = 2014;

        private bool _running;
        private readonly JsonSerializerSettings _settings;
        private readonly TcpClient _sender;
        private readonly IStreamToMessageConverter _streamToMessageConverter;

        public Action<BaseMessage> MessageEvent { get; set; }

        public Communicator(string address, int port, IStreamToMessageConverter streamToMessageConverter)
        {
            _streamToMessageConverter = streamToMessageConverter;
            _settings = new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };
            _sender = new TcpClient(address, port)
            {
                ReceiveBufferSize = ReadBufferSize
            };
        }

        public void Connect()
        {
            _running = true;
            var thread = new Thread(ReadMessagesFromServer);
            thread.Start();
        }

        public void Dispose()
        {
            _running = false;
            if (_sender != null)
            {
                _sender.Close();
            }
        }

        public void Send(BaseMessage message)
        {
            var converted = JsonConvert.SerializeObject(message, _settings);
            _sender.Client.Send(Encoding.ASCII.GetBytes(converted));
        }



        private void ReadMessagesFromServer()
        {
            while (_running)
            {
                var stream = _sender.GetStream();
                _streamToMessageConverter.HandleStream(stream, MessageEvent);
            }
        }
    }
}