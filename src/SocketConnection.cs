using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks.Dataflow;

namespace PeakSWC.WebServerTransporter
{
    
    public class SocketConnection
    {
        public Guid Id { get; }
        public WebSocket Socket { get; }
        public BufferBlock<SocketPacket> SocketWriter { get; } = new BufferBlock<SocketPacket>();
        public HttpContext Context { get; }

        public SocketConnection(WebSocket websocket, HttpContext context)
        {
            Id = Guid.NewGuid();
            Socket = websocket;
            Context = context;
        }

        public SocketConnection(WebSocket websocket, Guid id, HttpContext context)
        {
            Id = id;
            Socket = websocket;
            Context = context;
        }
    }
}
