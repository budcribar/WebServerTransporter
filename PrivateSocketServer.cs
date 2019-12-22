using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PeaskSWC.WebServerTransporter;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Net.Http;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace PeakSWC.WebServerTransporter
{
    #region ExtensionMethod
    public static class WebServerTransporterSocketExtension
    {
        public static IServiceCollection AddWebServerTransporterSocket(this IServiceCollection services, Uri transporterUrl)  
        {
          
            services.AddSingleton(new PrivateServerHubConnection<SocketHub>(transporterUrl));

            services.AddHostedService<PrivateSocketServer>();
            
            return services;
        }

    }
    #endregion

    public class PrivateSocketServer : IHostedService

    {
        private readonly PrivateServerHubConnection<SocketHub> connection;
        private readonly ServerUri serverUri;
        private readonly ConcurrentDictionary<Guid, SocketConnection> connectionDictionary;


        public Uri Url (string path)
        {
            if (path == null)
                throw new ArgumentNullException(nameof(path));

            if (serverUri == null || serverUri.Uri == null)
                throw new Exception(nameof(ServerUri));

            if (path.StartsWith("/", StringComparison.OrdinalIgnoreCase))
                return new Uri(serverUri.Uri, path);

            return new Uri(path);
        }

        public PrivateSocketServer(PrivateServerHubConnection<SocketHub> connection, ServerUri serverUri, ConcurrentDictionary<Guid, SocketConnection> connectionDictionary )
        {
            this.connection = connection;
            this.serverUri = serverUri;
            this.connectionDictionary = connectionDictionary;

            if (connection == null)
                throw new NullReferenceException(nameof(connection));


            connection.HubConnection = new HubConnectionBuilder().WithUrl(connection.HubUrl).WithAutomaticReconnect().Build();

            connection.HubConnection.On<Guid, SocketPacket>(Strings.TransporterToServer, (guid, packet) => {

                connectionDictionary[guid].SocketWriter.Post(packet);
            });

        }


        public async Task StartAsync(CancellationToken cancellationToken)
        {
            if (connection.HubConnection == null)
                throw new NullReferenceException(nameof(connection.HubConnection));

            while (true)
            {
                try
                {
                    await connection.HubConnection.StartAsync().ConfigureAwait(false);

                    break;
                }
                catch (HttpRequestException)
                {
                    Thread.Sleep(1000);
                }

            }
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            if (connection.HubConnection == null)
                throw new NullReferenceException(nameof(connection.HubConnection));

            await connection.HubConnection.DisposeAsync().ConfigureAwait(false);
        }
    }
       
}
