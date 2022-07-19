using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Electrum.Core.Distribution
{
    public interface IJobExecutionClient
    {
        Dictionary<string, List<string>> GetAvailableJobs();
        void Execute(ElectrumJob job);
        Task ExecuteAsync(ElectrumJob job);
        ClientInfo GetInfo();
    }
}
