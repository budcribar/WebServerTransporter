using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace CPUTempMonitor.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CPUTempMonitorController : ControllerBase
    {
        private static List<Task> stressTask = new List<Task>();
        private static List<CancellationTokenSource> tokenSource = new List<CancellationTokenSource>();


        public CPUTempMonitorController()
        {
        }

        //[HttpPost]
        //public void Stressor ([FromBody]int heat)

        [HttpGet("stressor/{heat}")]
        public void Stressor(bool heat)
        {
            if (heat)
            {
                if (stressTask.Count() == 0)
                {

                    for (int i = 0; i < Environment.ProcessorCount; i++)
                    {
                        tokenSource.Add(new CancellationTokenSource());

                        // Prevent closing over loop variable
                        var j = i;
                        stressTask.Add(Task.Run(() =>
                        {
                            try
                            {
                                while (true)
                                {
                                    tokenSource[j].Token.ThrowIfCancellationRequested();
                                    double s = Math.PI;
                                    double t = Math.Atan(s);
                                }
                            }
                            catch (Exception) { }
                        }, tokenSource[j].Token)
                        );
                    }
                }
            }
            else
            {
                if (stressTask.Count() > 0)
                {
                    for (int i = 0; i < Environment.ProcessorCount; i++)                 
                        tokenSource[i].Cancel();

                    try
                    {
                        Task.WaitAll(stressTask.ToArray());
                    }
                    finally
                    {
                        for (int i = 0; i < tokenSource.Count(); i++)                          
                        {
                            tokenSource[i].Dispose();                         
                        }
                        tokenSource = new List<CancellationTokenSource>();
                        stressTask = new List<Task>();
                    }
                   
                }
            }
        }
    }
}
