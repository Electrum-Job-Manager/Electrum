using Electrum.Core;
using Electrum.Core.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Electrum.Client.ExternalProcesses
{
    public class GenericJobExecutor
    {

        public void ExecuteConfiguredJob(JobLogger logger, ElectrumJob job)
        {
            logger.Debug("Looking for command for job {Namespace}/{JobName}", job.Namespace.Name, job.JobName);
        }

    }
}
