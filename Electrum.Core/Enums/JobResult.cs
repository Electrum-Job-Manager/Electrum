using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Electrum.Core.Enums
{
    public enum JobResult
    {

        Success = 1,
        Warning = 2,
        Error = 4,
        MissingExecutor = 8,
    }
}
