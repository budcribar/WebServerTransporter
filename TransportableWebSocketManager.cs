using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;



namespace PeakSWC.WebServerTransporter
{
    public class TransportableWebSocketManager 
    {
        private HttpContext Context { get; }
        private ServerUri ServerUri { get; }
       
        public TransportableWebSocketManager(HttpContext context, ServerUri serverUri)
        {
            this.Context = context;
            this.ServerUri = serverUri;
        }

        public bool IsWebSocketRequest => Context.Request.Headers.ContainsKey("IsWebSocketRequest") && bool.Parse(Context.Request.Headers["IsWebSocketRequest"]); 

        public IList<string> WebSocketRequestedProtocols => Context.WebSockets.WebSocketRequestedProtocols;

        public async Task<IWebSocket> AcceptWebSocketAsync()
        {
            if (ServerUri.Uri == null)
                throw new NullReferenceException(nameof(ServerUri.Uri));

            if (Context.Request.Headers.ContainsKey("Transporter") && !Context.Request.Path.Value.Contains(nameof(SocketHub), StringComparison.OrdinalIgnoreCase) && !Context.Request.Path.Value.Contains(nameof(TransporterHub), StringComparison.OrdinalIgnoreCase))
            {
                ClientWebSocket ws = new ClientWebSocket();

                Guid socketGuid = Guid.Parse(Context.Request.Headers["SocketGuid"]);
               
                return new TransporterWebSocket(ws, Context, socketGuid);
            }
            else
            {
                WebSocket ws = await Context.WebSockets.AcceptWebSocketAsync().ConfigureAwait(false);

                return new StandardWebSocket(ws, Context);
            }             

        }

    }
}
