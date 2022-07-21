using Electrum.Core.Distribution;
using Electrum.Core.Enums;
using Electrum.Core.Logging;
using Electrum.Core.Store;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Electrum.Core.API
{
    public class JobAPIService
    {
        public ElectrumObjectRepositoryFactory ElectrumObjectRepositoryFactory { get; }
        public IElectrumObjectRepository<ElectrumJob> JobRepository { get; }
        public IJobSchedulerService JobScheduler { get; }
        public ILogger<JobAPIService> Logger { get; }
        public JobLog JobLog { get; }

        public JobAPIService(ILogger<JobAPIService> logger, ElectrumObjectRepositoryFactory electrumObjectRepositoryFactory, IJobSchedulerService jobSchedulerService, JobLog jobLog)
        {
            Logger = logger;
            ElectrumObjectRepositoryFactory = electrumObjectRepositoryFactory;
            JobRepository = ElectrumObjectRepositoryFactory.GetRepo<ElectrumJob>();
            JobScheduler = jobSchedulerService;
            JobLog = jobLog;
        }

        public ElectrumJob? GetJob(Guid id)
        {
            return JobRepository.FirstOrDefault(x => x.Id == id);
        }

        public List<ElectrumJob> AllJobs(string @namespace, string name)
        {
            return JobRepository.Where(x => x.Namespace.Name == @namespace && x.JobName == name).ToList();
        }

        public List<ElectrumJob> AllJobsByStatus(JobStatus status)
        {
            return JobRepository.Where(x => x.Status == status).ToList();
        }

        public List<ElectrumJob> GetRunningJobs()
        {
            return JobRepository.Where(x => x.Status == JobStatus.Running).ToList();
        }

        public List<ElectrumJob> GetLatestJobs()
        {
            return JobRepository.OrderByDescending(x => x.JobStart).Take(50).ToList();
        }

        public ScheduledJob QueueJob(ElectrumJob job)
        {
            if(job.Timeout == TimeSpan.Zero)
            {
                job.Timeout = JobScheduler.GetDefaultTimeout();
            }
            job.Id = Guid.NewGuid();
            JobRepository.Add(job);
            return JobScheduler.ScheduleJob(job);
        }

        public List<JobLogger.JobLogRow> GetLogs(Guid id)
        {
            return JobLog.GetRowsForJob(id);
        }

    }
}
