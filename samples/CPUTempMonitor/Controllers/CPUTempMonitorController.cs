using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace CPUTempMonitor.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CPUTempMonitorController : ControllerBase
    {
        public CPUTempMonitorController()
        {
        }

        [HttpGet]
        public double Get()
        {
            return 2.2;
        }
    }
}
