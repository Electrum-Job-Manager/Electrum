using Electrum.Core.Discovery;
using Serilog;
using Serilog.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Electrum.Core.Execution
{
    public class JobExecutorService
    {

        public ElectrumJobDiscoveryService JobDiscoveryService { get; }

        private Dictionary<string, List<ExecutableJob>>? _executableJobsInNamespaces;

        public Dictionary<string, List<ExecutableJob>> ExecutableJobsInNamespaces
        {
            get
            {
                if(_executableJobsInNamespaces == null)
                {
                    _executableJobsInNamespaces = JobDiscoveryService.DiscoverJobExecutors().GroupBy(x => x.Namespace).ToDictionary(x => x.Key, x => x.ToList());
                }
                return _executableJobsInNamespaces;
            }
        }

        public JobExecutorService(ElectrumJobDiscoveryService jobDiscoveryService) => (JobDiscoveryService) = (jobDiscoveryService);

        public ElectrumJob ExecuteJob(ElectrumJob job)
        {
            if (job == null) return null;
            using (LogContext.PushProperty("JobId", job.Id.ToString()))
            {
                // Find namespace
                var hasExecutorInThatNamespace = ExecutableJobsInNamespaces.ContainsKey(job.Namespace.Name);
                if (!hasExecutorInThatNamespace)
                {
                    job.Result = Enums.JobResult.MissingExecutor;
                    Log.Warning("Attempted to execute job {JobId} in namespace {Namespace} but no executor was found.", job.Id, job.Namespace.Name);
                    return job;
                }
                var jobsInNamespace = ExecutableJobsInNamespaces[job.Namespace.Name];
                var executor = jobsInNamespace.FirstOrDefault(x => x.JobName == job.JobName);
                if (executor == null)
                {
                    job.Result = Enums.JobResult.MissingExecutor;
                    Log.Warning("Attempted to execute job {JobId} in namespace {Namespace} with name {JobName} but no executor was found with that name.", job.Id, job.Namespace.Name, job.JobName);
                    return job;
                }
                Log.Information("Executing job {JobId} in namespace {Namespace} with name {JobName} and with {ParameterCount} parameter(s).", job.Id, job.Namespace.Name, job.JobName, job.Parameters.Length);
                var result = executor.Execute(job);
                if (job.Result == Enums.JobResult.Warning)
                {
                    Log.Warning("Job {JobId} executed in {JobExecutionTime} with result '{JobResult}'. Message: ", job.Id, job.ExecutionTime, job.Result, job.Error);
                }
                else if (job.Result == Enums.JobResult.Error)
                {
                    Log.Error("Job {JobId} executed in {JobExecutionTime} with result '{JobResult}'. Message: ", job.Id, job.ExecutionTime, job.Result, job.Error);
                }
                else
                {
                    Log.Information("Executed job {JobId} in {JobExecutionTime} with result '{JobResult}'", job.Id, job.ExecutionTime, job.Result);
                }
            }
            return job;
        }

    }
}
