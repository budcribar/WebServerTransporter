using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Net.WebSockets;
using System.Threading.Tasks.Dataflow;
using System.Net.Http.Headers;
using System.Net;
using System.IO;
using System.Diagnostics;
#if DEBUG
using OpenTelemetry.Trace;
#endif
using Microsoft.AspNetCore.Http;
using System.Collections.Concurrent;
using System.Threading;
using System.Collections.Immutable;
using Microsoft.Extensions.Hosting;
using PeaskSWC.WebServerTransporter;

namespace PeakSWC.WebServerTransporter
{
#region ExtensionMethod
    public static class WebServerTransporterExtension
    {
        public static IServiceCollection AddWebServerTransporter(this IServiceCollection services, Uri transporterUrl)  
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }
            
            services.AddSingleton<ServerUri>();
            services.AddSingleton(typeof(ConcurrentDictionary<Guid, SocketConnection>));
            services.AddWebServerTransporterSocket(transporterUrl);
            services.AddSingleton(new PrivateServerHubConnection<TransporterHub>(transporterUrl));
            services.AddSingleton(typeof(PrivateServerStatusController));
            services.AddHostedService<PrivateServer>();

            return services;
        }

        public static IApplicationBuilder UseWebServerTransporterMiddlewareInterface(IApplicationBuilder app, Type middlewareType)
        {
            if (app == null)
            {
                throw new ArgumentNullException(nameof(app));
            }

            return app.Use(next =>
            {
                return async context =>
                {
                    var middlewareFactory = (IMiddlewareFactory)context.RequestServices.GetService(typeof(IMiddlewareFactory));
                    if (middlewareFactory == null)
                    {
                        // No middleware factory
                        throw new InvalidOperationException("NoMiddlewareFactory");
                    }

                    var middleware = middlewareFactory.Create(middlewareType);
                    if (middleware == null)
                    {
                        // The factory returned null, it's a broken implementation
                        throw new InvalidOperationException("UnableToCreateMiddleware");
                    }

                    try
                    {
                        await middleware.InvokeAsync(context, next).ConfigureAwait(false);
                    }
                    finally
                    {
                        middlewareFactory.Release(middleware);
                    }
                };
            });
        }

        public static IApplicationBuilder UseWebServerTransporter(this IApplicationBuilder app, Func<HttpTransporterContext, Func<Task>, Task>? middleware=null) 
        {

            if (app == null)
            {
                throw new ArgumentNullException(nameof(app));
            }
           
           
            // Need to create connection before use
            app.UseMiddleware<PrivateServerStatusController>();
            var suri = app.ApplicationServices.GetService<ServerUri>();
            suri.ServerAddressesFeature = app.ServerFeatures.Get<IServerAddressesFeature>();

            if (middleware != null)
                app.Use((c,t) => middleware(new HttpTransporterContext(c,suri), t));

            return app;
        }


    }
#endregion


    public class PrivateServer : IHostedService
    {
        private readonly PrivateServerHubConnection<TransporterHub> connection;
        private readonly ServerUri serverUri;
      

        // Path part of URI
        public Uri Url (string path)
        {
            if (path == null)
                throw new ArgumentNullException(nameof(path));

            if (serverUri == null || serverUri.Uri == null)
                throw new Exception(nameof(ServerUri));


            if (path.StartsWith("/", StringComparison.OrdinalIgnoreCase))
                return new Uri(serverUri.Uri, path);

            return new Uri( path );
        }


        public PrivateServer(PrivateServerHubConnection<TransporterHub> connection, ServerUri serverUri)
        {
            this.connection = connection;
            this.serverUri = serverUri;

            if (connection == null)
                throw new NullReferenceException(nameof(connection));         

            connection.HubConnection = new HubConnectionBuilder().WithUrl(connection.HubUrl).WithAutomaticReconnect().Build();

            connection.HubConnection.On(Strings.GetRequestPacket, (Func<HttpRequestPacket, TextMapCarrier, Task>)(async (packet, carrier) =>
            {
                var stream = connection.HubConnection.StreamAsync<byte[]>(Strings.SendRequestBody, packet.Id);
                await WebRequest(packet, carrier, async (s) =>
                {
                    await foreach (var ba in stream)
                        await s.WriteAsync(ba);
                }).ConfigureAwait(false);            

            }));

            connection.HubConnection.On<HttpRequestPacket, byte[], TextMapCarrier>(Strings.WebRequest, (packet, body, carrier) => WebrequestTask(packet, body, carrier));
        }

      
        private void WebrequestTask(HttpRequestPacket packet, byte[] body, TextMapCarrier carrier)
        {
            // Web requests may be long running
            Task.Run(async () => await WebRequest(packet, carrier,async (s) =>
            {
                await s.WriteAsync(body).ConfigureAwait(false);
               
            }).ConfigureAwait(false));
        }


#if ZIPKIN
        private async Task WebRequest(HttpRequestPacket packet, TextMapCarrier carrier, Func<Stream,Task> WriteBody)
        {
            var tracer = TracerFactoryBase.Default.GetTracer("Transporter");
            var context = tracer.Context(carrier);
#else
        private async Task WebRequest(HttpRequestPacket packet, TextMapCarrier _, Func<Stream, Task> WriteBody)
        {
#endif
            HttpResponsePacket? response = null;
            var errorMessage = "Internal Server Error";

#if ZIPKIN
            using (tracer.StartActiveSpan("Transporter -> Server", context, SpanKind.Server, out _))
#endif
            {
                try
                {
                    if (!(HttpWebRequest.Create(new Uri(Url(packet.Path), packet.QueryString)) is HttpWebRequest httpWebRequest)) throw new Exception("Failed to create web request");

                    httpWebRequest.Method = packet.Method;
                    httpWebRequest.ContentType = packet.ContentType;
                    httpWebRequest.KeepAlive = true; // https://stackoverflow.com/questions/734404/error-using-httpwebrequest-to-upload-files-with-put


                    // TODO This caused a problem for UploadFile !!!!
                    //foreach (var key in packet.Headers.Keys)  
                    //{
                    //    httpWebRequest.Headers.Add(key, packet.Headers[key]);
                    //}


                    foreach (var key in packet.Headers.Keys.Intersect(new string[] { Strings.TransporterHeader, Strings.UserAgentHeader, Strings.IsWebSocketRequestHeader, Strings.SocketGuidHeader }))  // TODO Others??
                    {
                        httpWebRequest.Headers.Add(key, packet.Headers[key]);
                    }


                    // You could add authentication here as well if needed: //TODO
                    // request.PreAuthenticate = true;
                    // request.AuthenticationLevel = System.Net.Security.AuthenticationLevel.MutualAuthRequested;
                    // request.Headers.Add("Authorization", "Basic " + Convert.ToBase64String(System.Text.Encoding.Default.GetBytes("username" + ":" + "password")));

                    if (packet.BodyLength > 0)
                    {
                        httpWebRequest.ContentLength = packet.BodyLength;


                        using (Stream requestStream = httpWebRequest.GetRequestStream())
                        {
                            await WriteBody(requestStream).ConfigureAwait(false);

                        }
                    }

                    if (packet.Headers.ContainsKey(Strings.IsWebSocketRequestHeader))
                        httpWebRequest.Timeout = 60 * 50 * 1000;

                    // Required to accept cookies
                    httpWebRequest.CookieContainer = new CookieContainer();

                    if (await httpWebRequest.GetResponseAsync().ConfigureAwait(false) is HttpWebResponse webResponse)
                        using (var ms = new MemoryStream())
                        {
                            await webResponse.GetResponseStream().CopyToAsync(ms).ConfigureAwait(false);

                            response = new HttpResponsePacket(id: packet.Id, content: ImmutableArray.Create<byte>(ms.ToArray()), contentLength: webResponse.ContentLength, statusCode: webResponse.StatusCode);

                            response.Cookies.Add(webResponse.Cookies);
                            if (!string.IsNullOrEmpty(webResponse.ContentType))
                                response.ContentType = MediaTypeHeaderValue.Parse(webResponse.ContentType);

                            if (!string.IsNullOrEmpty(webResponse.ContentEncoding))
                            {
                                response.ContentEncoding = webResponse.ContentEncoding;
                            }

                        }
                }

                catch (WebException ex)
                {
                    errorMessage = ex.Message;
                }

            }

            if (response == null)
            {
                response = new HttpResponsePacket(id: packet.Id, content: ImmutableArray.Create<byte>(Encoding.ASCII.GetBytes(errorMessage)), contentLength: errorMessage.Length, statusCode: HttpStatusCode.InternalServerError);
            }

#if ZIPKIN
            using (tracer.StartActiveSpan("Server -> Transporter Response", context, SpanKind.Server, out var _))
#endif
            {
                var byteData = response.Content.ToArray();
                response.Content = ImmutableArray<byte>.Empty;
                await connection.HubConnection.SendAsync(Strings.PutHttpResponseStream, response, ClientStreamData(byteData)).ConfigureAwait(false);
            }
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            if (connection.HubConnection == null)
                throw new NullReferenceException(nameof(connection.HubConnection));

            Task.Run(async () =>
            {
                // Loop is here to wait until the server is running
                while (true)
                {
                    try
                    {
                        await connection.HubConnection.StartAsync(cancellationToken).ConfigureAwait(false);

                        break;
                    }

                    catch (HttpRequestException)
                    {
                        await Task.Delay(1000).ConfigureAwait(false);
                    }
                }
            });

            return Task.CompletedTask;
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            if (connection.HubConnection == null)
                throw new NullReferenceException(nameof(connection.HubConnection));

            await connection.HubConnection.DisposeAsync().ConfigureAwait(false);
        }

#region Helpers


        public static async IAsyncEnumerable<byte[]> ClientStreamData(byte[] data)
        {
            // TODO Increasing this past 16k causes System.Net.Sockets.SocketException : The I/O operation has been aborted because of either a thread exit or an application request.
            foreach (var s in data.SplitByLength(16 * 1024))
            {
                yield return s.ToArray();
                await Task.Delay(1).ConfigureAwait(false);  
            }

        }

#endregion
    }

}
