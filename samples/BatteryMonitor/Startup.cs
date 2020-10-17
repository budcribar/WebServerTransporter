using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.SpaServices.AngularCli;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PeakSWC.WebServerTransporter;
using System;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics.PerformanceData;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Management;
using BatteryMonitor.Controllers;
using Newtonsoft.Json;

namespace BatteryMonitor
{
    public class BatteryCharge
    {
        public BatteryCharge (double level, DateTime date) { ChargeLevel = level; Date = date; }
        public double ChargeLevel { get; }
        public DateTime Date { get; }
    }
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllersWithViews();
            // In production, the Angular files will be served from this directory
            services.AddSpaStaticFiles(configuration =>
            {
                configuration.RootPath = "ClientApp/dist";
            });

            services.AddSingleton<TaskState>();

            services.AddWebServerTransporter(new Uri("http://localhost:53343"));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            //app.UseHttpsRedirection();

            app.UseWebSockets();

            app.UseWebServerTransporter(async (context, next) =>
            {
                if (context.Request.Path == "/ws")
                {

                    if (context.WebSockets.IsWebSocketRequest)
                    {
                        using (IWebSocket webSocket = await context.WebSockets.AcceptWebSocketAsync())
                            await SendBatteryStatus(context, webSocket);
                    }
                    else
                    {
                        context.Response.StatusCode = 400;
                    }
                }
                else
                {
                    await next();
                }

            });

            app.UseStaticFiles();
            if (!env.IsDevelopment())
            {
                app.UseSpaStaticFiles();
            }

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller}/{action=Index}/{id?}");
            });

            app.UseSpa(spa =>
            {
                // To learn more about options for serving an Angular SPA from ASP.NET Core,
                // see https://go.microsoft.com/fwlink/?linkid=864501

                spa.Options.SourcePath = "ClientApp";

                if (env.IsDevelopment())
                {
                    spa.UseAngularCliServer(npmScript: "start");
                }
            });
        }

        #region Battery Charge

        private BatteryCharge ReadWMIBattery()
        {
            ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM Win32_Battery");
            foreach (ManagementObject obj in searcher.Get())
            {
                Double temp = Convert.ToDouble(obj["EstimatedChargeRemaining"].ToString());

                return new BatteryCharge(temp, DateTime.Now);

            }
            return new BatteryCharge(-1, DateTime.Now);
        }

        private async Task SendBatteryStatus(HttpTransporterContext _, IWebSocket webSocket)
        {
            while (true)
            {
                var status = ReadWMIBattery();
                byte[] data = Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(status));

                Console.WriteLine("Battery Status: {0}", status.ChargeLevel);
                await webSocket.SendAsync(new ArraySegment<byte>(data, 0, data.Length), WebSocketMessageType.Text, endOfMessage:true, CancellationToken.None);

                try
                {
                    await webSocket.ReceiveAsync(data, CancellationToken.None);
                }
                catch (WebSocketException) { break; }        

                Thread.Sleep(1000);
            }
        }
        #endregion
    }
}
