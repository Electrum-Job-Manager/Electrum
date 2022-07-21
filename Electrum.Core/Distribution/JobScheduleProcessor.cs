using Electrum.Core.Store;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Electrum.Core.Distribution
{
    internal class JobScheduleProcessor : IHostedService
    {

        private IJobSchedulerService JobSchedulerService { get; }
        private JobDistributionService JobDistributionService { get; }
        private IElectrumObjectRepository<ElectrumJob> JobRepo { get; }
        private ILogger<JobScheduleProcessor> Logger { get; }

        public JobScheduleProcessor(IJobSchedulerService jobSchedulerService, JobDistributionService jobDistributionService, ElectrumObjectRepositoryFactory repoFactory, ILogger<JobScheduleProcessor> logger)
        {
            JobSchedulerService = jobSchedulerService;
            JobDistributionService = jobDistributionService;
            JobRepo = repoFactory.GetRepo<ElectrumJob>();
            Logger = logger;
        }

        private bool shouldStop = false;
        private Task scheduleProcessor;
        private Task recurringJobProcessor;

        public Task ProcessSchedule(CancellationToken cancellationToken)
        {
            scheduleProcessor = ProcessScheduleQueue(cancellationToken);
            recurringJobProcessor = ProcessRecurringJobs(cancellationToken);
            return Task.CompletedTask;
        }

        public Task ProcessScheduleQueue(CancellationToken cancellationToken)
        {
            return Task.Run(() =>
            {
                Logger.LogInformation("Starting job schedule processing service");
                while (!cancellationToken.IsCancellationRequested && !shouldStop)
                {
                    // Get the job schedule
                    var schedule = JobSchedulerService.GetScheduledJobs().Where(x => DateTime.UtcNow >= x.PlannedExecution).Where(x => JobDistributionService.HasClientForJob(x.Job)).ToList();
                    foreach (var job in schedule)
                    {
                        JobSchedulerService.RemoveFromSchedule(job);
                        JobDistributionService.ExecuteJob(job.Job);
                    }
                }
                Logger.LogWarning("Job schedule processing service is stopping...");
            });
        }

        // TODO: Process recurring jobs and schedule them
        public Task ProcessRecurringJobs(CancellationToken cancellationToken)
        {
            return Task.Run(async () =>
            {
                Logger.LogInformation("Starting recurring job service");
                while (!cancellationToken.IsCancellationRequested && !shouldStop)
                {
                    var nextExecution = JobSchedulerService.GetNextExecutionTimesForRecurringJobs();
                    if(!nextExecution.Any())
                    {
                        Task.Delay(TimeSpan.FromSeconds(30)).Wait(cancellationToken);
                        continue;
                    }
                    var nextJobs = nextExecution.OrderBy(x => x.Key).FirstOrDefault();
                    var nextInOrder = nextJobs.Key;
                    if (nextJobs.Key.AddSeconds(-1) <= DateTime.UtcNow)
                    {
                        var jobs = nextJobs.Value;
                        foreach (var job in jobs)
                        {
                            JobSchedulerService.ScheduleJob(job.JobNamespace, job.JobName, job.Timeout, nextJobs.Key, job.Parameters);
                        }
                        nextInOrder = nextExecution.OrderBy(x => x.Key).Skip(1).FirstOrDefault().Key;
                    }
                    var sleepTime = Math.Min(TimeSpan.FromSeconds(5).TotalMilliseconds, (nextInOrder - DateTime.UtcNow).TotalMilliseconds - 500);
                    Task.Delay((int)sleepTime).Wait(cancellationToken);
                }
                Logger.LogWarning("Recurring job service is stopping...");
            });
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            return ProcessSchedule(cancellationToken);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            shouldStop = true;
            return Task.WhenAll(scheduleProcessor, recurringJobProcessor);
        }
    }
}
