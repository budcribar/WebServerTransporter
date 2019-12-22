using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Pipelines;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PeakSWC.WebServerTransporter
{
    public class TransporterHub : Hub<ITransporterHub>
    {
        private readonly VerifyPrivateServer verifyPrivateServer;
        private readonly ConcurrentDictionary<Guid, HttpMessagePacket> responseQueue;

        public TransporterHub(VerifyPrivateServer verifyPrivateServer, ConcurrentDictionary<Guid, HttpMessagePacket> responseQueue)
        {
            this.verifyPrivateServer = verifyPrivateServer;
            this.responseQueue = responseQueue;
        }

        public override Task OnConnectedAsync()
        {
            Debug.WriteLine("Connected to transporter hub");
            verifyPrivateServer.IsPrivateServerConnected = true;

            return base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception exception)
        {
            Debug.WriteLine("Disconnected from transporter hub");
            verifyPrivateServer.IsPrivateServerConnected = false;

            return base.OnDisconnectedAsync(exception);
        }

        public async IAsyncEnumerable<byte[]> SendRequestBody(string id)
        {
            var body = responseQueue[Guid.Parse(id)].RequestBody;
            
            foreach (var s in body.SplitByLength(64 * 1024))  // TODO make this a constant
            {
                yield return s.ToArray();
                await Task.Delay(1).ConfigureAwait(false);
            }
        }

        public async Task PutHttpResponseStream(HttpResponsePacket packet, IAsyncEnumerable<byte[]> stream)
        {
            if (packet == null)
                throw new ArgumentNullException(nameof(packet));
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));          

            var httpResponse = responseQueue[packet.Id].HttpResponse;

            if (packet.ContentType != null)
                httpResponse.ContentType = $"{packet.ContentType.MediaType}; charset={packet.ContentType.CharSet}";

            httpResponse.StatusCode = (int)packet.StatusCode;

            foreach (var c in packet.Cookies.AsEnumerable())
                httpResponse.Cookies.Append(c.Name, c.Value, new Microsoft.AspNetCore.Http.CookieOptions { Domain = c.Domain, Expires = c.Expires, HttpOnly = c.HttpOnly, Path = c.Path, Secure = c.Secure });
         
            await foreach (var ba in stream)
                await httpResponse.Body.WriteAsync(ba).ConfigureAwait(false);

            responseQueue[packet.Id].Sync.Release();
        }
    }
}
