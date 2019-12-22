using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Text.Json.Serialization;
using System.Collections.ObjectModel;
using System.Collections.Immutable;
using Newtonsoft.Json;

namespace PeakSWC.WebServerTransporter
{
    public class HttpResponsePacket
    {
       
        public HttpResponsePacket() { }

        // https://stackoverflow.com/questions/23017716/json-net-how-to-deserialize-without-using-the-default-constructor
        [JsonConstructor]
        public HttpResponsePacket(Guid id, ImmutableArray<byte> content, long contentLength, HttpStatusCode statusCode)
        {
            Id = id;
            Content = content;
            ContentLength = contentLength;
            StatusCode = statusCode;
        }
        public Guid Id { get; set; }
        public ImmutableArray<byte> Content { get; set; } = new ImmutableArray<byte>();    
        public long ContentLength { get; set; }
        public HttpStatusCode StatusCode { get; set; }
        public MediaTypeHeaderValue? ContentType { get; set; }  
        public string ContentEncoding { get; set; } = "";
        public CookieCollection Cookies { get; set; } = new CookieCollection();
        //public HttpContentHeaders ContentHeaders { get; set; }
        //public HttpResponseHeaders Headers { get; set;  }
        //public HttpResponseHeaders TrailingHeaders { get; set; }
        //public bool IsSuccessStatusCode { get; set; }
        //public string ReasonPhrase { get; set; }
        //public HttpRequestMessage RequestMessage { get; set; }
        //public Version Version { get; set; }
    }
}
