using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Electrum.Core.Distribution
{
    public interface IJobSchedulerService
    {

        public void ScheduleJob(ElectrumJob job);
        public void ScheduleJob(ElectrumJob job, DateTime executeAt);
        public void ScheduleJob(string jobNamespace, string jobName, DateTime executeAt);
        public void ScheduleJob(string jobNamespace, string jobName, TimeSpan timeout);
        public void ScheduleJob(string jobNamespace, string jobName, TimeSpan timeout, DateTime executeAt);
        public void ScheduleJob(string jobNamespace, string jobName, params string[] args);
        public void ScheduleJob(string jobNamespace, string jobName, DateTime executeAt, params string[] args);
        public void ScheduleJob(string jobNamespace, string jobName, TimeSpan timeout, params string[] args);
        public void ScheduleJob(string jobNamespace, string jobName, TimeSpan timeout, DateTime executeAt, params string[] args);
        public void ScheduleRecurringJob(string jobNamespace, string jobName, string cronSyntax);
        public void ScheduleRecurringJob(string jobNamespace, string jobName, string cronSyntax, params string[] args);
        public void ScheduleRecurringJob(string jobNamespace, string jobName, string cronSyntax, TimeSpan timeout);
        public void ScheduleRecurringJob(string jobNamespace, string jobName, string cronSyntax, TimeSpan timeout, params string[] args);

    }
}
