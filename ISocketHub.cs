using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PeakSWC.WebServerTransporter
{
    public interface ISocketHub
    {
        Task TransporterToServer(Guid id, SocketPacket packet);
        Task ServerToTransporter(Guid id, SocketPacket packet);
        Task Abort(Guid id);
        Task Dispose(Guid id);
        Task CloseOutputAsync(Guid id, WebSocketCloseStatus closeStatus, string statusDescription, CancellationToken cancellationToken);
        Task CloseAsync(Guid id, WebSocketCloseStatus closeStatus, string statusDescription);

    }
}
