using Microsoft.AspNetCore.Http;
using System;
using System.Threading.Tasks.Dataflow;

namespace PeakSWC.WebServerTransporter
{
    public interface IWebSocketController
    {
        void Execute(Guid id, ISourceBlock<byte[]> socketReader, ITargetBlock<byte[]> socketWriter, HttpContext context);
    }
}
