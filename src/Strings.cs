using PeakSWC.WebServerTransporter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PeaskSWC.WebServerTransporter
{

    public static class Strings
    {
      

        public static string PutHttpResponse => nameof(ITransporterHub.PutHttpResponse);
        public static string PutHttpResponseStream => nameof(ITransporterHub.PutHttpResponseStream);
        public static string SendRequestBody => nameof(ITransporterHub.SendRequestBody);
        public static string GetRequestPacket => nameof(ITransporterHub.GetRequestPacket);
        public static string WebRequest => nameof(ITransporterHub.WebRequest);

        public static string TransporterToServer => nameof(ISocketHub.TransporterToServer);
        public static string ServerToTransporter => nameof(ISocketHub.ServerToTransporter);
        public static string Abort => nameof(ISocketHub.Abort);
        public static string CloseAsync => nameof(ISocketHub.CloseAsync);
        public static string CloseOutputAsync => nameof(ISocketHub.CloseOutputAsync);
        public static string Dispose => nameof(ISocketHub.Dispose);

        public static string TransporterHeader => "Transporter";
        public static string IsWebSocketRequestHeader => "IsWebSocketRequest";
        public static string SocketGuidHeader => "SocketGuid";
        public static string RefreshHeader => "Refresh";
        public static string UserAgentHeader => "User-Agent";

        public static string TransporterPath => "/transporter";
    }
}
