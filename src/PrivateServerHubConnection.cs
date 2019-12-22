using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Client;

namespace PeakSWC.WebServerTransporter
{
    public class PrivateServerHubConnection<T>
    {
        private Uri? hubUrl = null;

        public Uri? HubUrl
        {

            get
            { 
                return hubUrl;
            }
            set
            {
                if (value == null)
                    throw new ArgumentException(nameof(HubUrl));
                
                if (value.PathAndQuery.Length == 1)
                {
                    var n = typeof(T).Name;
                    n = n.Contains("`", StringComparison.OrdinalIgnoreCase) ? n.Substring(0, n.IndexOf("`", StringComparison.OrdinalIgnoreCase)) : n;
                    hubUrl =  new Uri(value, n);
                }
                    
                else
                    hubUrl = value;
            }
        }

        public HubConnection? HubConnection { get; set; }

        public PrivateServerHubConnection(Uri url)
        {
            HubUrl = url;
        }
    }

}
