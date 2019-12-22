using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace PeakSWC.WebServerTransporter
{
    public class SocketHub : Hub<ISocketHub> 
    {
        private readonly ConcurrentDictionary<Guid, SocketConnection> ConnectionDictionary;

        public SocketHub(ConcurrentDictionary<Guid, SocketConnection> connectionDictionary) {
            this.ConnectionDictionary = connectionDictionary;
        }

        // TODO Pass in cancellation token??
        public async Task ServerToTransporter(Guid id, SocketPacket packet)
        {
            if (packet == null)
                throw new ArgumentNullException(nameof(packet));

            await ConnectionDictionary[id].Socket.SendAsync(packet.Data.AsSpan().ToArray(), (WebSocketMessageType)packet.MessageType, packet.EndOfMessage, CancellationToken.None).ConfigureAwait(false);
        }

        public Task<WebSocketState> State(Guid id)
        {
            return Task.FromResult<WebSocketState>( ConnectionDictionary[id].Socket.State);
        }

        public  Task<string> CloseStatusDescription(Guid id)
        {
            return Task.FromResult<string> (ConnectionDictionary[id].Socket.CloseStatusDescription);
        }

        public Task<WebSocketCloseStatus?> CloseStatus(Guid id)
        {
            return Task.FromResult(ConnectionDictionary[id].Socket.CloseStatus);
        }

        public Task<string> SubProtocol(Guid id)
        {
            return Task.FromResult<string>( ConnectionDictionary[id].Socket.SubProtocol);
        }

        public Task Abort(Guid id)
        {
            ConnectionDictionary[id].Socket.Abort();
            return Task.CompletedTask;
        }
        public Task Dispose(Guid id)
        {
            ConnectionDictionary[id].Socket.Dispose();
            return Task.CompletedTask;
        }

        public Task CloseOutputAsync(Guid id, WebSocketCloseStatus closeStatus, string statusDescription, CancellationToken cancellationToken)
        {
            ConnectionDictionary[id].Socket.CloseOutputAsync(closeStatus, statusDescription, cancellationToken);
            return Task.CompletedTask;
        }
        public Task CloseAsync(Guid id, WebSocketCloseStatus closeStatus, string statusDescription)
        {
            ConnectionDictionary[id].Socket.CloseAsync(closeStatus, statusDescription, CancellationToken.None);
            return Task.CompletedTask;
        }

    }
}
