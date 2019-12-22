using Microsoft.AspNetCore.Hosting.Server.Features;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PeakSWC.WebServerTransporter
{
    public class ServerUri
    {
        public IServerAddressesFeature? ServerAddressesFeature { get; set; } = null;
        
        private Uri? uri = null;

        public Uri Uri
        {
            get
            {
                if (uri == null)
                {
                    if (ServerAddressesFeature == null || ServerAddressesFeature.Addresses.Count == 0)
                        uri = new Uri($"http://localhost:5000");
                    else
                        uri = new Uri($"{ServerAddressesFeature.Addresses.First()}");
                };
                return uri;

            }
        }       
    }
}
