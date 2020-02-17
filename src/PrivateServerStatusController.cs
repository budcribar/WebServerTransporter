using Microsoft.AspNetCore.Http;
using PeaskSWC.WebServerTransporter;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace PeakSWC.WebServerTransporter
{
    public class PrivateServerStatusController : IMiddleware
    {
        private readonly ConcurrentDictionary<Guid, SocketConnection> socketConnectionDictionary;
        private readonly PrivateServerHubConnection<TransporterHub> connection;
        private readonly PrivateServerHubConnection<SocketHub> socketConnection;

        private static string BuildFade
        {
            get
            {
                return "<h1>Connecting... <h1>";
            }
        }
        private string BuildVersionError(string transporterVer, string serverVer)
        {
            var r = "<h1>Fatal Error:<h1>";
            r += "<h2>Version Mismatch<h2>";
            r += $"Expected Transporter Version: {serverVer} but found Version: {transporterVer}";
            return r;
        }
        private string BuildStatus
        {
            get
            {
                var r = "<h1>Private Server Status:<h1>";
                r += "<table style='text-align:left'>";
                //foreach (var rq in ResponseQueue.Keys)
                //{
                //    var mp = ResponseQueue[rq];
                //    r += $"<tr><td> {rq.ToString()} </td> <td> {mp.Request.Method} </td> <td> {mp.Request.Host} </td> <td> {mp.Request.Path} </td> <td> {mp.Request.QueryString} </td> <td> {mp.Request.Scheme} </td> <td> {mp.Request.Protocol} </td> <td> {mp.Request.Body} </td> </tr>";
                //}
                r += $"<tr><th>SignalR Transporter Hub</th><th>Status</th></tr>";
                var state = connection.HubConnection == null ? "unconnected" : connection.HubConnection.State.ToString();
                r += $"<tr><td> {connection.HubUrl} </td><td>{state}</td></tr>";
                r += "</table>";

                r += "<table style='text-align:left'>";
                r += $"<tr><th>SignalR Socket Hub</th><th>Status</th></tr>";
                state = socketConnection.HubConnection == null ? "unconnected" : socketConnection.HubConnection.State.ToString();
                r += $"<tr><td> {socketConnection.HubUrl} </td><td>{state}</td></tr>";
                r += "</table>";


                r += "<table>";
                if (socketConnectionDictionary != null)
                    foreach (var rq in socketConnectionDictionary.Keys)
                    {
                        var mp = socketConnectionDictionary[rq];
                        r += $"<tr><td> {rq.ToString()} </td> <td> {mp.Id} </td> </tr>";
                    }
                r += "</table>";

                return r;
            }
        }

        public PrivateServerStatusController(PrivateServerHubConnection<TransporterHub> connection, PrivateServerHubConnection<SocketHub> socketConnection, ConcurrentDictionary<Guid, SocketConnection> socketConnectionDictionary)
        {
            this.connection = connection;
            this.socketConnection = socketConnection;
            this.socketConnectionDictionary = socketConnectionDictionary;
        }

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));
            if (next == null)
                throw new ArgumentNullException(nameof(next));


            if (context.Request.Path.Value.StartsWith(Strings.TransporterPath, StringComparison.OrdinalIgnoreCase))
            {
                context.Response.StatusCode = 200;
                context.Response.Headers.Add(Strings.RefreshHeader, "1");
                await context.Response.WriteAsync(BuildStatus).ConfigureAwait(false);
            }
            else if (context.Request.Headers.ContainsKey(Strings.TransporterHeader) && (context.Request.Headers[Strings.TransporterHeader] != (this.GetType().Assembly.GetName().Version?.ToString() ?? "")))
            {
                context.Response.StatusCode = 200;
                await context.Response.WriteAsync(BuildVersionError(context.Request.Headers[Strings.TransporterHeader], this.GetType().Assembly.GetName().Version?.ToString() ?? "")).ConfigureAwait(false);
            }
            else if (this.connection.HubConnection?.State == Microsoft.AspNetCore.SignalR.Client.HubConnectionState.Connected)
                await next(context).ConfigureAwait(false);
            else
            {
                context.Response.Headers.Add(Strings.RefreshHeader, "1");
                //await next(context).ConfigureAwait(false);

                await context.Response.WriteAsync(BuildFade).ConfigureAwait(false);
            }
        }

      
    }
}
