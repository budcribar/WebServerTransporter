using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CPUTempMonitor.Controllers
{
    public class TaskState
    {
        public List<Task> taskList = new List<Task>();
        public List<CancellationTokenSource> tokenSourceList = new List<CancellationTokenSource>();
    }
}
