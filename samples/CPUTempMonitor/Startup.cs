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

namespace CPUTempMonitor
{
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
                        IWebSocket webSocket = await context.WebSockets.AcceptWebSocketAsync();
                        await SendCPUTemp(context, webSocket);
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

        #region Temperature

        //private double BatteryStatus()
        //{
        //    BatteryChargeStatus
        //}

        private double ReadWMITemp ()
        {
            ManagementObjectSearcher searcher = new ManagementObjectSearcher(@"root\WMI", "SELECT * FROM MSAcpi_ThermalZoneTemperature");
            foreach (ManagementObject obj in searcher.Get())
            {
                Double temp = Convert.ToDouble(obj["CurrentTemperature"].ToString());
                temp = (temp - 2732) / 10.0;
                return temp;
            }
            return 0;
        }

        private double ReadWMIBattery()
        {
            ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM Win32_Battery");
            foreach (ManagementObject obj in searcher.Get())
            {
                Double temp = Convert.ToDouble(obj["EstimatedChargeRemaining"].ToString());
                
                return temp;
            }
            return 0;
        }

        private async Task SendCPUTemp(HttpTransporterContext _, IWebSocket webSocket)
        {
            //PerformanceCounterCategory category = PerformanceCounterCategory.GetCategories().First(c => c.CategoryName == "Thermal Zone Information");
            //PerformanceCounter tempCounter = new PerformanceCounter("Thermal Zone Information", "Temperature", category.GetInstanceNames().Last());
            
            while (true)
            {
                //double temperature = (tempCounter.NextValue() - 273.15f);
                //var r = new Random();
                //string temp = (temperature + r.NextDouble()).ToString();
                //string temp = temperature.ToString();
                string temp = ReadWMIBattery().ToString();

                // -273.15 is the conversion from degrees Kelvin to degrees Celsius

               

                byte[] data = Encoding.ASCII.GetBytes(temp);

                Console.WriteLine("Temperature: {0} \u00B0C", temp);
                await webSocket.SendAsync(new ArraySegment<byte>(data, 0, data.Length), WebSocketMessageType.Text, endOfMessage:true, CancellationToken.None);

                Thread.Sleep(1000);
            }
        }
        #endregion
    }
}
