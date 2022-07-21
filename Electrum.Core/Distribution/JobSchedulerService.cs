using Electrum.Core.Services;
using Electrum.Core.Store;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Electrum.Core.Distribution
{
    public class JobSchedulerService : IJobSchedulerService
    {
        internal static TimeSpan DefaultTimeout = TimeSpan.FromMinutes(1);
        private ElectrumObjectRepositoryFactory RepositoryFactory { get; }
        private IElectrumNamespaceService NamespaceService { get; }
        
        public JobSchedulerService(ElectrumObjectRepositoryFactory repositoryFactory, IElectrumNamespaceService namespaceService)
        {
            RepositoryFactory = repositoryFactory;
            NamespaceService = namespaceService;
        }

        public TimeSpan GetDefaultTimeout() => DefaultTimeout;

        public ScheduledJob ScheduleJob(ElectrumJob job) => ScheduleJob(job, DateTime.UtcNow);

        public ScheduledJob ScheduleJob(ElectrumJob job, DateTime executeAt)
        {
            // Actually do something
            job.Status = Enums.JobStatus.Scheduling;
            var jobRepo = RepositoryFactory.GetRepo<ElectrumJob>();
            job = jobRepo.Save(job);
            var scheduledJob = new ScheduledJob
            {
                JobId = job.Id,
                Job = job,
                PlannedExecution = executeAt,
                LastTriedScheduling = DateTime.MinValue,
                MaxSchedulingRetries = 3,
                TimesTriedScheduling = 0
            };
            var scheduledJobRepo = RepositoryFactory.GetRepo<ScheduledJob>();
            return scheduledJobRepo.Add(scheduledJob);
        }

        public ScheduledJob ScheduleJob(string jobNamespace, string jobName) => ScheduleJob(jobNamespace, jobName, DateTime.UtcNow);
        public ScheduledJob ScheduleJob(string jobNamespace, string jobName, DateTime executeAt) => ScheduleJob(jobNamespace, jobName, DefaultTimeout, executeAt);

        public ScheduledJob ScheduleJob(string jobNamespace, string jobName, TimeSpan timeout) => ScheduleJob(jobNamespace, jobName, timeout, DateTime.UtcNow);

        public ScheduledJob ScheduleJob(string jobNamespace, string jobName, TimeSpan timeout, DateTime executeAt) => ScheduleJob(jobNamespace, jobName, timeout, executeAt, new string[0]);

        public ScheduledJob ScheduleJob(string jobNamespace, string jobName, params string[] args) => ScheduleJob(jobNamespace, jobName, DefaultTimeout, DateTime.UtcNow, args);

        public ScheduledJob ScheduleJob(string jobNamespace, string jobName, DateTime executeAt, params string[] args) => ScheduleJob(jobNamespace, jobName, DefaultTimeout, executeAt, args);

        public ScheduledJob ScheduleJob(string jobNamespace, string jobName, TimeSpan timeout, params string[] args) => ScheduleJob(jobNamespace, jobName, timeout, DateTime.UtcNow, args);

        public ScheduledJob ScheduleJob(string jobNamespace, string jobName, TimeSpan timeout, DateTime executeAt, params string[] args)
        {
            var storedNamespace = NamespaceService.GetOrCreateNamespace(jobNamespace);
            var job = new ElectrumJob
            {
                Id = Guid.NewGuid(),
                Namespace = storedNamespace,
                JobName = jobName,
                Timeout = timeout,
                Parameters = args
            };
            var jobRepo = RepositoryFactory.GetRepo<ElectrumJob>();
            job = jobRepo.Add(job);
            return ScheduleJob(job, executeAt);
        }

        public List<ScheduledJob> GetScheduledJobs()
        {
            var scheduledJobRepo = RepositoryFactory.GetRepo<ScheduledJob>();
            return scheduledJobRepo.ToList();
        }

        public void RemoveFromSchedule(ScheduledJob job)
        {
            var scheduledJobRepo = RepositoryFactory.GetRepo<ScheduledJob>();
            scheduledJobRepo.Remove(job);
        }

        public ScheduledCronJob ScheduleRecurringJob(ScheduledCronJob cronJob)
        {
            return RepositoryFactory.GetRepo<ScheduledCronJob>().Add(cronJob);
        }

        public ScheduledCronJob ScheduleRecurringJob(string jobNamespace, string jobName, string cronSyntax) => ScheduleRecurringJob(jobNamespace, jobName, cronSyntax, DefaultTimeout);

        public ScheduledCronJob ScheduleRecurringJob(string jobNamespace, string jobName, string cronSyntax, params string[] args) => ScheduleRecurringJob(jobNamespace, jobName, cronSyntax, DefaultTimeout, args);

        public ScheduledCronJob ScheduleRecurringJob(string jobNamespace, string jobName, string cronSyntax, TimeSpan timeout) => ScheduleRecurringJob(jobNamespace, jobName, cronSyntax, timeout, new string[0]);

        public ScheduledCronJob ScheduleRecurringJob(string jobNamespace, string jobName, string cronSyntax, TimeSpan timeout, params string[] args)
        {
            var cronJobRepo = RepositoryFactory.GetRepo<ScheduledCronJob>();
            var job = new ScheduledCronJob
            {
                Id = Guid.NewGuid(),
                JobNamespace = jobNamespace,
                JobName = jobName,
                CronSyntax = cronSyntax,
                Timeout = timeout,
                Parameters = args
            };
            return cronJobRepo.Add(job);
        }

        public Dictionary<DateTime, List<ScheduledCronJob>> GetNextExecutionTimesForRecurringJobs()
        {
            var cronJobRepo = RepositoryFactory.GetRepo<ScheduledCronJob>();
            return cronJobRepo.ToList().GroupBy(x => x.GetNextDate()).Where(x => x.Key != null).ToDictionary(x => (DateTime) x.Key, x => x.ToList());
        }
    }
}
