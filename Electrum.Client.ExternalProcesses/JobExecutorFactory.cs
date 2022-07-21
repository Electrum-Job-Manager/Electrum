using Electrum.Core.Discovery;
using Electrum.Core.Execution;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Electrum.Client.ExternalProcesses
{
    /// <summary>
    /// This is a custom factory to substitute the <see cref="Electrum.Core.Discovery.ElectrumJobDiscoveryService"/> that depends on classes
    /// </summary>
    public class ConfigJobDiscoveryService : IJobDiscoveryService
    {
        public List<ExecutableJob> DiscoverJobExecutors()
        {
            throw new NotImplementedException();
        }
    }
}
