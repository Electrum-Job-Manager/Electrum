using Electrum.Core.Discovery;
using Electrum.Core.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog.Context;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Electrum.Core.Execution
{
    public class JobExecutorService
    {

        public IJobDiscoveryService JobDiscoveryService { get; }
        public IServiceProvider ServiceProvider { get; }

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

        public JobExecutorService(IServiceProvider serviceProvider, IJobDiscoveryService jobDiscoveryService) => (JobDiscoveryService, ServiceProvider) = (jobDiscoveryService, serviceProvider.CreateScope().ServiceProvider);

        public ElectrumJob ExecuteJob(ElectrumJob job, IJobLoggingClient jobLoggingClient = null)
        {
            if (job == null) return null;
            if (jobLoggingClient == null) jobLoggingClient = ServiceProvider.GetService<IJobLoggingClient>();
            var jobLogger = new JobLogger(ServiceProvider.GetService<ILogger<JobLogger>>(), job.Id, jobLoggingClient, true);
            // Find namespace
            var hasExecutorInThatNamespace = ExecutableJobsInNamespaces.ContainsKey(job.Namespace.Name);
            if (!hasExecutorInThatNamespace)
            {
                job.Status = Enums.JobStatus.MissingExecutor;
                jobLogger.Warning("Attempted to execute job {JobId} in namespace {Namespace} but no executor was found.", job.Id, job.Namespace.Name);
                return job;
            }
            var jobsInNamespace = ExecutableJobsInNamespaces[job.Namespace.Name];
            var executor = jobsInNamespace.FirstOrDefault(x => x.JobName == job.JobName);
            if (executor == null)
            {
                job.Status = Enums.JobStatus.MissingExecutor;
                jobLogger.Warning("Attempted to execute job {JobId} in namespace {Namespace} with name {JobName} but no executor was found with that name.", job.Id, job.Namespace.Name, job.JobName);
                return job;
            }
            jobLogger.Info("Executing job {JobId} in namespace {Namespace} with name {JobName} and with {ParameterCount} parameter(s).", job.Id, job.Namespace.Name, job.JobName, job.Parameters.Length);
            job.JobStart = DateTime.UtcNow;
            var sw = Stopwatch.StartNew();
            try
            {
                var result = executor.Execute(jobLogger, job);
            } catch (Exception ex)
            {
                job.Status = Enums.JobStatus.Error;
                job.Error = ex.ToString();
                jobLogger.Fatal(ex, "Failed to execute job {Id} ({NamespaceName}/{JobName}) an error occured", job.Id, job.Namespace.Name, job.JobName);
            }
            sw.Stop();
            job.ExecutionTime = sw.Elapsed;
            jobLogger.SaveRows();
            if (job.Status == Enums.JobStatus.Warning)
            {
                jobLogger.Warning("Job {JobId} executed in {JobExecutionTime} with status '{JobStatus}'. Message: ", job.Id, job.ExecutionTime, job.Status, job.Error);
            }
            else if (job.Status == Enums.JobStatus.Error)
            {
                jobLogger.Error("Job {JobId} executed in {JobExecutionTime} with status '{JobStatus}'. Message: ", job.Id, job.ExecutionTime, job.Status, job.Error);
            }
            else
            {
                jobLogger.Info("Executed job {JobId} in {JobExecutionTime} with status '{JobStatus}'", job.Id, job.ExecutionTime, job.Status);
            }
            return job;
        }

    }
}
