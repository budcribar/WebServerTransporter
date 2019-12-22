using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;

namespace PeakSWC.WebServerTransporter
{
    public class HttpTransporterContext
    {
        private readonly HttpContext context;
        private readonly TransportableWebSocketManager socketManager;
       

        public HttpTransporterContext(HttpContext context, ServerUri serverUri)
        {
            this.context = context;
           
            this.socketManager = new TransportableWebSocketManager(context, serverUri);
        }

        public IFeatureCollection Features => context.Features;

        public HttpRequest Request => context.Request;

        public HttpResponse Response => context.Response;

        public ConnectionInfo Connection => context.Connection;

        public TransportableWebSocketManager WebSockets => socketManager;

        //public AuthenticationManager Authentication => context.Authentication;

        public ClaimsPrincipal User { get => context.User; set => context.User = value; }
        public IDictionary<object, object> Items { get => context.Items; }
        public IServiceProvider RequestServices { get => context.RequestServices; set => context.RequestServices = value; }
        public CancellationToken RequestAborted { get => context.RequestAborted; set => context.RequestAborted = value; }
        public string TraceIdentifier { get => context.TraceIdentifier; set => context.TraceIdentifier = value; }
        public ISession Session { get => context.Session; set => context.Session = value; }

        public void Abort()
        {
            context.Abort();
        }
    }
}
