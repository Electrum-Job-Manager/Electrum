using Electrum.Core.Execution;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Electrum.Core.Discovery
{
    public interface IJobDiscoveryService
    {

        public List<ExecutableJob> DiscoverJobExecutors();

    }
}
