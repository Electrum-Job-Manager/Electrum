using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Electrum.Core.Enums
{
    public enum JobStatus
    {
        Scheduling = 1,
        Success = 2,
        Warning = 4,
        Error = 8,
        MissingExecutor = 16,
        SchedulingFailed = 32,
        TimedOut = 64,
    }
}
