using Electrum.Core.Store;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Electrum.Core.Distribution
{
    public class JobDistributionService
    {
        #region Client Store
        private static Dictionary<Guid, IJobExecutionClient> ExecutionClients { get; } = new Dictionary<Guid, IJobExecutionClient>();
        private static Dictionary<Guid, ClientInfo> Clients { get; } = new Dictionary<Guid, ClientInfo>();
        /// <summary>
        /// The Guid is the client id
        /// </summary>
        private static Dictionary<Guid, List<ElectrumJob>> RunningJobsOnClients = new Dictionary<Guid, List<ElectrumJob>>();
        #endregion

        #region Job Availability
        private static Dictionary<string, Dictionary<string, List<Guid>>> ClientsByJobs { get; } = new Dictionary<string, Dictionary<string, List<Guid>>>();
        #endregion

        private ILogger<JobDistributionService> Logger { get; }
        private IJobSchedulerService JobSchedulerService { get; }
        private IElectrumObjectRepository<ElectrumJob> JobRepo { get; }

        public JobDistributionService(ILogger<JobDistributionService> logger, IJobSchedulerService jobSchedulerService, ElectrumObjectRepositoryFactory repoFactory)
        {
            Logger = logger;
            JobSchedulerService = jobSchedulerService;
            JobRepo = repoFactory.GetRepo<ElectrumJob>();
        }

        public void AddClient(IJobExecutionClient client)
        {
            var clientInfo = client.GetInfo();
            Clients.Add(clientInfo.Id, clientInfo);
            ExecutionClients.Add(clientInfo.Id, client);
            RunningJobsOnClients.Add(clientInfo.Id, new List<ElectrumJob>());
            var jobs = client.GetAvailableJobs();
            foreach (var jobNamespace in jobs)
            {
                if(!ClientsByJobs.ContainsKey(jobNamespace.Key))
                {
                    ClientsByJobs.Add(jobNamespace.Key, new Dictionary<string, List<Guid>>());
                }
                var jobClientNamespace = ClientsByJobs[jobNamespace.Key];
                foreach (var job in jobNamespace.Value)
                {
                    if(!jobClientNamespace.ContainsKey(job))
                    {
                        jobClientNamespace.Add(job, new List<Guid>());
                    }
                    var jobClients = jobClientNamespace[job];
                    jobClients.Add(clientInfo.Id);
                }
            }
            Logger.LogInformation("Client {ClientId} joined the job distribution pool", clientInfo.Id);
        }

        public void RemoveClient(Guid id)
        {
            Logger.LogInformation("Client {ClientId} is being disconnected", id);
            // Reschedule the active jobs on the client
            foreach (var item in RunningJobsOnClients[id])
            {
                Logger.LogInformation("Rescheduling job {JobId}", item.Id);
                JobSchedulerService.ScheduleJob(item);
            }


            Clients.Remove(id);
            ExecutionClients.Remove(id);
            RunningJobsOnClients.Remove(id);
            foreach (var jobNamespace in ClientsByJobs)
            {
                var jobClientNamespace = jobNamespace.Value;
                foreach (var job in jobClientNamespace.Keys)
                {
                    var jobClients = jobClientNamespace[job];
                    jobClients.Remove(id);
                }
            }
            Logger.LogInformation("Client {ClientId} left the job distribution pool", id);
        }

        private List<Guid> GetClientsThatHasJob(string jobNamespace, string name)
        {
            if(ClientsByJobs.TryGetValue(jobNamespace, out Dictionary<string, List<Guid>> jobs)) {
                if(jobs.TryGetValue(name, out List<Guid> clients))
                {
                    return clients;
                }
            }
            return new List<Guid>();
        }

        private Guid? GetClientToExecuteJob(string jobNamespace, string name)
        {
            var possibleClients = GetClientsThatHasJob(jobNamespace, name);
            var runningJobCount = possibleClients.ToDictionary(x => x, x => RunningJobsOnClients[x].Count);
            var remainingJobs = possibleClients.ToDictionary(x => x, x => Clients[x].MaxConcurrentJobs - runningJobCount[x]);
            return runningJobCount
                .Where(x => remainingJobs[x.Key] > 0) // Has enough spots to execute the job
                .OrderBy(x => x.Value) // Order by the current running jobs, this is to equally distribute the jobs
                .ThenByDescending(x => remainingJobs[x.Key]) // Then by the most remaining jobs, as it supposedly has more computational power
                .Select(x => x.Key) // Get the id
                .FirstOrDefault(); // Get the first one
        }

        public bool HasClientForJob(ElectrumJob job)
        {
            var clientId = GetClientToExecuteJob(job.Namespace.Name, job.JobName);
            return clientId != null && clientId != Guid.Empty;
        }

        public async void ExecuteJob(ElectrumJob job)
        {
            var clientId = GetClientToExecuteJob(job.Namespace.Name, job.JobName);
            if (clientId == null || clientId == Guid.Empty)
            {
                throw new Exception("No client found to execute the job");
            }
            var client = ExecutionClients[clientId.Value];
            job.Status = Enums.JobStatus.Running;
            JobRepo.Save(job);
            RunningJobsOnClients[clientId.Value].Add(job);
            await client.ExecuteAsync(job); // This will wait until the job is finished
            if (RunningJobsOnClients.ContainsKey(clientId.Value)) // If this does not contain the id, the client was disconnected
            {
                RunningJobsOnClients[clientId.Value].Remove(job);
                JobRepo.Save(job);
            }
        }

    }
}
