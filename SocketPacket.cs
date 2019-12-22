using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Net.WebSockets;
using System.Threading.Tasks;

namespace PeakSWC.WebServerTransporter
{
    public class SocketPacket
    {
        // Parameterless constructor is needed for SignalR serialization
        public SocketPacket() { }
        public SocketPacket(WebSocketReceiveResult result, ImmutableArray<byte> data)
        {
            if (result == null)
                throw new ArgumentNullException(nameof(result));

            CloseStatus = result.CloseStatus;
            CloseStatusDescription = result.CloseStatusDescription;
            Count = result.Count;
            EndOfMessage = result.EndOfMessage;
            MessageType = result.MessageType;

            Data = data;
        }
       
        public WebSocketCloseStatus? CloseStatus { get; set; }

        public string CloseStatusDescription { get; set; } = "";
       
        public int Count { get; set; }
        
        public bool EndOfMessage { get; set; }

        public WebSocketMessageType MessageType { get; set; }

        public ImmutableArray<byte> Data { get; set; }
    }
}
