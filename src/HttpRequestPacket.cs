using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using Newtonsoft.Json;
using System.Collections.Immutable;

namespace PeakSWC.WebServerTransporter
{
    
   
    public class HttpRequestPacket
    {
        private HttpRequestPacket() { }

        [JsonConstructor]
        public HttpRequestPacket(Guid id, string path, string queryString, string? contentType, string method, string protocol, string host, int bodyLength, bool isHttps, string scheme, Dictionary<string, string> headers)
        {
            Id = id;
            Path = path;
            QueryString = queryString;
            ContentType = contentType;
            Method = method;
            Protocol = protocol;
            Host = host;         
            BodyLength = bodyLength;
            IsHttps = isHttps;
            Scheme = scheme;
            Headers = headers;
        }
        public Guid Id { get; set; }
        public string Path { get; set; } = "";
        public string QueryString { get; set;  } = "";
        public string? ContentType { get; set; }
        public string Method { get; set; } = "";
        public string Protocol { get; set; } = "";
        public string Host { get; set;  } = "";
        public int BodyLength { get; set; }
        public bool IsHttps { get; set; }
        public string Scheme { get; set; } = "";

        public Dictionary<string, string> Headers { get; set; } = new Dictionary<string, string>(); // content headers
    }
}
