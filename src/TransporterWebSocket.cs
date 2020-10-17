using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR.Client;
using PeaskSWC.WebServerTransporter;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace PeakSWC.WebServerTransporter
{
    public class TransporterWebSocket : IWebSocket, IDisposable
    {
        public HttpContext Context { get; set; }

        public WebSocketCloseStatus? CloseStatus => privateServerHubConnection.HubConnection.InvokeAsync<WebSocketCloseStatus?>("CloseStatus", this.socketConnection.Id).Result;

        public string CloseStatusDescription => privateServerHubConnection.HubConnection.InvokeAsync<string>("CloseStatusDescription", this.socketConnection.Id).Result;

        public WebSocketState State => privateServerHubConnection.HubConnection.InvokeAsync<WebSocketState>("State", this.socketConnection.Id).Result;

        public string SubProtocol => privateServerHubConnection.HubConnection.InvokeAsync<string>("SubProtocol", this.socketConnection.Id).Result;

        private readonly SocketConnection socketConnection;

        private readonly ConcurrentDictionary<Guid, SocketConnection> ConnectionDictionary;
        private readonly PrivateServerHubConnection<SocketHub> privateServerHubConnection;

        public TransporterWebSocket(WebSocket webSocket, HttpContext context, Guid id)
        {
            this.Context = context ?? throw new ArgumentNullException(nameof(context));

            if (!(Context.RequestServices.GetService(typeof(ConcurrentDictionary<Guid, SocketConnection>)) is ConcurrentDictionary<Guid, SocketConnection> connectionDictionary)) throw new Exception("Requires ConnectionDictionary");
            ConnectionDictionary = connectionDictionary;

            if(!(Context.RequestServices.GetService(typeof(PrivateServerHubConnection<SocketHub>)) is PrivateServerHubConnection<SocketHub> pshc)) throw new Exception($"Requires dependency {typeof(PrivateServerHubConnection<SocketHub>)}");
            this.privateServerHubConnection = pshc;

            this.socketConnection = new SocketConnection(webSocket, id, context);
            ConnectionDictionary.TryAdd(this.socketConnection.Id, this.socketConnection);
        }

        public async void Abort()
        {
            await privateServerHubConnection.HubConnection.InvokeAsync(Strings.Abort, this.socketConnection.Id).ConfigureAwait(false);
        }

        public async Task CloseAsync(WebSocketCloseStatus closeStatus, string statusDescription, CancellationToken cancellationToken)
        {
            ConnectionDictionary.Remove(socketConnection.Id, out _);
            await privateServerHubConnection.HubConnection.InvokeAsync(Strings.CloseAsync, this.socketConnection.Id, closeStatus, statusDescription).ConfigureAwait(false);
        }

        public async Task CloseOutputAsync(WebSocketCloseStatus closeStatus, string statusDescription, CancellationToken cancellationToken)
        {
            await privateServerHubConnection.HubConnection.InvokeAsync(Strings.CloseOutputAsync, this.socketConnection.Id, closeStatus, statusDescription, cancellationToken).ConfigureAwait(false);
        }

        public async void Dispose()
        {
            await privateServerHubConnection.HubConnection.InvokeAsync(Strings.Dispose, this.socketConnection.Id).ConfigureAwait(false);
        }

        public async Task<WebSocketReceiveResult> ReceiveAsync(ArraySegment<byte> buffer, CancellationToken cancellationToken)
        {
            // Read data from the BufferBlock
            await socketConnection.SocketWriter.OutputAvailableAsync().ConfigureAwait(false);

            var socketPacket = await socketConnection.SocketWriter.ReceiveAsync().ConfigureAwait(false);

            // TODO
            Contract.Assert(buffer.Count >= socketPacket.Count);
            socketPacket.Data.AsSpan().Slice(0, socketPacket.Count).CopyTo(buffer);
            //socketPacket.Data.AsSpan().CopyTo(buffer.AsSpan());

            return new WebSocketReceiveResult(socketPacket.Count, socketPacket.MessageType, socketPacket.EndOfMessage, socketPacket.CloseStatus, socketPacket.CloseStatusDescription);
        }

        public async Task SendAsync(ArraySegment<byte> buffer, WebSocketMessageType messageType, bool endOfMessage, CancellationToken cancellationToken)
        {
            if (buffer.Array == null)
                throw new ArgumentNullException(nameof(buffer));

            var socketPacket = new SocketPacket() { MessageType = messageType, EndOfMessage = endOfMessage, Data=ImmutableArray.Create(buffer.Array), Count=buffer.Count };

            await privateServerHubConnection.HubConnection.InvokeAsync(Strings.ServerToTransporter, this.socketConnection.Id, socketPacket).ConfigureAwait(false);
        }
    }
}