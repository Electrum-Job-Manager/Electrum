using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Electrum.Core.Distribution
{
    public interface IJobSchedulerService
    {
        public TimeSpan GetDefaultTimeout();
        public ScheduledJob ScheduleJob(ElectrumJob job);
        public ScheduledJob ScheduleJob(ElectrumJob job, DateTime executeAt);
        public ScheduledJob ScheduleJob(string jobNamespace, string jobName);
        public ScheduledJob ScheduleJob(string jobNamespace, string jobName, DateTime executeAt);
        public ScheduledJob ScheduleJob(string jobNamespace, string jobName, TimeSpan timeout);
        public ScheduledJob ScheduleJob(string jobNamespace, string jobName, TimeSpan timeout, DateTime executeAt);
        public ScheduledJob ScheduleJob(string jobNamespace, string jobName, params string[] args);
        public ScheduledJob ScheduleJob(string jobNamespace, string jobName, DateTime executeAt, params string[] args);
        public ScheduledJob ScheduleJob(string jobNamespace, string jobName, TimeSpan timeout, params string[] args);
        public ScheduledJob ScheduleJob(string jobNamespace, string jobName, TimeSpan timeout, DateTime executeAt, params string[] args);
        List<ScheduledJob> GetScheduledJobs();
        public void RemoveFromSchedule(ScheduledJob job);


        public ScheduledCronJob ScheduleRecurringJob(ScheduledCronJob cronJob);
        public ScheduledCronJob ScheduleRecurringJob(string jobNamespace, string jobName, string cronSyntax);
        public ScheduledCronJob ScheduleRecurringJob(string jobNamespace, string jobName, string cronSyntax, params string[] args);
        public ScheduledCronJob ScheduleRecurringJob(string jobNamespace, string jobName, string cronSyntax, TimeSpan timeout);
        public ScheduledCronJob ScheduleRecurringJob(string jobNamespace, string jobName, string cronSyntax, TimeSpan timeout, params string[] args);
        public Dictionary<DateTime, List<ScheduledCronJob>> GetNextExecutionTimesForRecurringJobs();

    }
}
