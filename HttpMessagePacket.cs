using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace PeakSWC.WebServerTransporter
{
    public class HttpMessagePacket
    {
        public HttpMessagePacket(HttpRequestPacket httpRequestPacket, ImmutableArray<byte> requestBody, HttpResponse httpResponse)
        {
            Request = httpRequestPacket;
            RequestBody = requestBody;
            HttpResponse = httpResponse;
        }
        public HttpRequestPacket Request { get; }
        public ImmutableArray<byte> RequestBody { get; }
        public SemaphoreSlim Sync { get; } = new SemaphoreSlim(0, 1);
        public HttpResponse HttpResponse { get; }
    }
}
