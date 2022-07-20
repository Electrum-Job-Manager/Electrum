using Electrum.Core.Store;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Electrum.Core
{
    public class ScheduledJob
    {
        [ElectrumStoreKey]
        public Guid JobId { get; set; }
        public ElectrumJob Job { get; set; }
        public DateTime PlannedExecution { get; set; }
        public int MaxSchedulingRetries { get; set; }
        public int TimesTriedScheduling { get; set; }
        public DateTime LastTriedScheduling { get; set; }

    }
}
