using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace PeakSWC.WebServerTransporter
{
    public class VerifyPrivateServer : IMiddleware
    {       
        public bool IsPrivateServerConnected { get; set; } = false;
        private string dots = "...";

        private string BuildStatus()
        {
            return $"<h1 style=\"padding: 70px 0;text-align:center\">Waiting on the Private Server to be connected{dots}</h1>";
        }

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));
            if (next == null)
                throw new ArgumentNullException(nameof(next));


            if (IsPrivateServerConnected)
            {
                dots = "...";
                await next(context).ConfigureAwait(false);
            }
            else
            {
                context.Response.StatusCode = 200;
                context.Response.Headers.Add("Refresh", "1");
                await context.Response.WriteAsync(BuildStatus()).ConfigureAwait(false);
                dots += ".";
            }
        }
    }
}
