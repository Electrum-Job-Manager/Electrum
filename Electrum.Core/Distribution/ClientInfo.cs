using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Electrum.Core.Distribution
{
    public class ClientInfo
    {

        public Guid Id { get; set; }
        public int MaxConcurrentJobs { get; set; }
        public int ElectrumClientVersion { get; set; }
        public string Name { get; set; }
        public string MachineName { get; set; }
        public string AccessKey { get; set; }
        public string RemoteAddress { get; set; }

    }
}
