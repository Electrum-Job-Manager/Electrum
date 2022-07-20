using Electrum.Core;
using Electrum.Core.Distribution;
using Electrum.Core.Store;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Electrum.Tests.Tests
{
    public class JobSchedulerTests
    {

        public IJobSchedulerService Scheduler => TestConfigurator.ServiceProvider.GetService<IJobSchedulerService>();
        public ElectrumObjectRepositoryFactory RepositoryFactory => TestConfigurator.ServiceProvider.GetService<ElectrumObjectRepositoryFactory>();

        public JobSchedulerTests(ITestOutputHelper output)
        {
            TestConfigurator.ConfigureLogger<JobSchedulerTests>(output);
        }

        [Fact]
        public void CanGetJobSchedulerService()
        {
            Assert.NotNull(Scheduler);
        }

        private Electrum.Core.ElectrumJob testJob = new Core.ElectrumJob
        {
            Id = Guid.NewGuid(),
            Namespace = new Core.ElectrumNamespace
            {
                Name = "TestingNS",
                Id = Guid.NewGuid()
            },
            JobName = "TestingJob",
            Parameters = new string[]
            {
                "Test"
            }
        };

        [Fact]
        public void CanScheduleRawJob()
        {
            var job = testJob;
            var savedJob = RepositoryFactory.GetRepo<ElectrumJob>().Add(job);
            var sJob = Scheduler.ScheduleJob(testJob);
            Assert.NotNull(sJob);
            Assert.Equal(job.Id, sJob.JobId);
        }

        [Fact]
        public void CanScheduleRawJobAheadOfTime()
        {
            var job = testJob;
            var savedJob = RepositoryFactory.GetRepo<ElectrumJob>().Add(job);
            var sJob = Scheduler.ScheduleJob(testJob, DateTime.Now.AddHours(1));
            Assert.NotNull(sJob);
            Assert.Equal(job.Id, sJob.JobId);
        }

        [Fact]
        public void CanScheduleSimpleJob()
        {
            var sJob = Scheduler.ScheduleJob(testJob.Namespace.Name, testJob.JobName);
            Assert.NotNull(sJob);
            Assert.NotNull(sJob.JobId);
        }

        [Fact]
        public void CanScheduleSimpleJobAheadOfTime()
        {
            var sJob = Scheduler.ScheduleJob(testJob.Namespace.Name, testJob.JobName, DateTime.UtcNow.AddSeconds(30));
            Assert.NotNull(sJob);
            Assert.NotNull(sJob.JobId);
        }

        [Fact]
        public void CanScheduleSimpleJobWithTimeout()
        {
            var sJob = Scheduler.ScheduleJob(testJob.Namespace.Name, testJob.JobName, TimeSpan.FromSeconds(30));
            Assert.NotNull(sJob);
            Assert.NotNull(sJob.JobId);
        }

        [Fact]
        public void CanScheduleSimpleJobWithTimeoutAheadOfTime()
        {
            var sJob = Scheduler.ScheduleJob(testJob.Namespace.Name, testJob.JobName, TimeSpan.FromSeconds(30), DateTime.UtcNow.AddSeconds(30));
            Assert.NotNull(sJob);
            Assert.NotNull(sJob.JobId);
        }

        [Fact]
        public void CanScheduleJobWithParameters()
        {
            var sJob = Scheduler.ScheduleJob(testJob.Namespace.Name, testJob.JobName, testJob.Parameters);
            Assert.NotNull(sJob);
            Assert.NotNull(sJob.JobId);
        }

        [Fact]
        public void CanScheduleJobWithParametersAheadOfTime()
        {
            var sJob = Scheduler.ScheduleJob(testJob.Namespace.Name, testJob.JobName, DateTime.UtcNow.AddSeconds(30), testJob.Parameters);
            Assert.NotNull(sJob);
            Assert.NotNull(sJob.JobId);
        }

        [Fact]
        public void CanScheduleJobWithParametersWithTimeout()
        {
            var sJob = Scheduler.ScheduleJob(testJob.Namespace.Name, testJob.JobName, TimeSpan.FromSeconds(30), testJob.Parameters);
            Assert.NotNull(sJob);
            Assert.NotNull(sJob.JobId);
        }

        [Fact]
        public void CanScheduleJobWithParametersWithTimeoutAheadOfTime()
        {
            var sJob = Scheduler.ScheduleJob(testJob.Namespace.Name, testJob.JobName, TimeSpan.FromSeconds(30), DateTime.UtcNow.AddSeconds(30), testJob.Parameters);
            Assert.NotNull(sJob);
            Assert.NotNull(sJob.JobId);
        }

        [Fact]
        public void CanScheduleRecurringJob()
        {
            var cJob = Scheduler.ScheduleRecurringJob(testJob.Namespace.Name, testJob.JobName, "0 * * * * *");
            Assert.NotNull(cJob);
            Assert.NotNull(cJob.GetNextDate());
            var date = cJob.GetNextDate();
            var now = DateTime.UtcNow;
            var d = new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute + (now.Second == 0 ? 0 : 1), 0);
            Assert.Equal(d, date);
        }

    }
}
