using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PeakSWC.WebServerTransporter
{
    public class TextMapCarrier
    {
        public Dictionary<string, string> Map { get; } = new Dictionary<string, string>();

        public void Set(string key, string value)
        {
            this.Map[key] = value;
        }
    }
}
