using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Electrum.Core
{
    public class ScheduledJob
    {

        public Guid JobId { get; set; }
        public DateTime PlannedExecution { get; set; }
        public int MaxSchedulingRetries { get; set; }
        public int TimesTriedScheduling { get; set; }
        public DateTime LastTriedScheduling { get; set; }

    }
}
