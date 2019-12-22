using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace PeakSWC.WebServerTransporter
{
    public interface ITransporterHub
    {
        Task PutHttpResponse(string packet);
        Task PutHttpResponseStream(HttpResponsePacket packet, IAsyncEnumerable<byte[]> stream);
        Task WebRequest(HttpRequestPacket packet, byte[] body, TextMapCarrier carrier);
        Task WriteToClientReaderSocket(string data);
        Task WebRequestStream(IAsyncEnumerable<byte[]> asyncEnumerable);
        Task GetRequestPacket(HttpRequestPacket packet, TextMapCarrier carrier);
        Task SendRequestBody(Guid id);
    }
}
