using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PeakSWC.WebServerTransporter
{
    public class StandardWebSocket : WebSocket, IWebSocket
    {
        public HttpContext Context { get;  }
        private WebSocket? WebSocket { get; set; }

        public StandardWebSocket(WebSocket webSocket, HttpContext context)
        {
            this.Context = context;
            this.WebSocket = webSocket ?? throw new ArgumentNullException(nameof(webSocket));
        }

        public override WebSocketCloseStatus? CloseStatus => WebSocket?.CloseStatus;

        public override string CloseStatusDescription => WebSocket?.CloseStatusDescription ?? "";

        public override WebSocketState State => WebSocket?.State ?? WebSocketState.None;

        public override string SubProtocol => WebSocket?.SubProtocol ?? "";

        public override void Abort()
        {
            WebSocket?.Abort();
        }

        public override Task CloseAsync(WebSocketCloseStatus closeStatus, string statusDescription, CancellationToken cancellationToken)
        {
            return WebSocket?.CloseAsync(closeStatus, statusDescription, cancellationToken) ?? Task.CompletedTask;
        }

        public override Task CloseOutputAsync(WebSocketCloseStatus closeStatus, string statusDescription, CancellationToken cancellationToken)
        {
            return WebSocket?.CloseOutputAsync(closeStatus, statusDescription, cancellationToken) ?? Task.CompletedTask;
        }

        public override void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (WebSocket != null)
                {
                    WebSocket.Dispose();
                    WebSocket = null;
                }
            }
        }

        public override Task<WebSocketReceiveResult> ReceiveAsync(ArraySegment<byte> buffer, CancellationToken cancellationToken)
        {
            return WebSocket?.ReceiveAsync(buffer, cancellationToken) ?? Task.FromResult( new WebSocketReceiveResult(0, WebSocketMessageType.Close, true));
        }

        public override Task SendAsync(ArraySegment<byte> buffer, WebSocketMessageType messageType, bool endOfMessage, CancellationToken cancellationToken)
        {
            return WebSocket?.SendAsync(buffer, messageType, endOfMessage, cancellationToken) ?? Task.CompletedTask;
        }
    }
}
