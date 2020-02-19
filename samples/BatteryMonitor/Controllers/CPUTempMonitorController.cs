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
        private TaskState taskState;
      
        public CPUTempMonitorController(TaskState taskState)
        {
            this.taskState = taskState;
        }

        [HttpPost("stressor/{heat}")]
        public void Stressor (bool heat)
        {
            if (heat)
            {
                if (taskState.taskList.Count() == 0)
                {

                    for (int i = 0; i < Environment.ProcessorCount; i++)
                    {
                        taskState.tokenSourceList.Add(new CancellationTokenSource());

                        // Prevent closing over loop variable
                        var j = i;
                        taskState.taskList.Add(Task.Run(() =>
                        {
                            try
                            {
                                while (true)
                                {
                                    taskState.tokenSourceList[j].Token.ThrowIfCancellationRequested();
                                    double s = Math.PI;
                                    double t = Math.Atan(s);
                                }
                            }
                            catch (Exception) { }
                        }, taskState.tokenSourceList[j].Token)
                        );
                    }
                }
            }
            else
            {
                if (taskState.taskList.Count() > 0)
                {
                    for (int i = 0; i < Environment.ProcessorCount; i++)
                        taskState.tokenSourceList[i].Cancel();

                    try
                    {
                        Task.WaitAll(taskState.taskList.ToArray());
                    }
                    finally
                    {
                        for (int i = 0; i < taskState.tokenSourceList.Count(); i++)                          
                        {
                            taskState.tokenSourceList[i].Dispose();                         
                        }
                        taskState.tokenSourceList = new List<CancellationTokenSource>();
                        taskState.taskList = new List<Task>();
                    }
                   
                }
            }
        }
    }
}
