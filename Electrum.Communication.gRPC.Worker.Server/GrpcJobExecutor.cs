using Electrum.Core;
using Electrum.Core.Distribution;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Electrum.Communication.gRPC.Worker.Server
{
    internal class GrpcJobExecutor : IJobExecutionClient
    {
        internal Queue<Core.ElectrumJob> jobQueue = new Queue<Core.ElectrumJob>();
        internal Dictionary<Guid, Core.ElectrumJob> finishedJobs = new Dictionary<Guid, ElectrumJob>();
        private Core.Distribution.ClientInfo clientInfo;
        private Dictionary<string, List<string>> clientJobs;

        public GrpcJobExecutor(Core.Distribution.ClientInfo clientInfo, Dictionary<string, List<string>> clientJobs)
        {
            this.clientInfo = clientInfo;
            this.clientJobs = clientJobs;
        }

        public void Execute(ElectrumJob job)
        {
            ExecuteAsync(job).Wait();
        }

        public async Task ExecuteAsync(ElectrumJob job)
        {
            jobQueue.Enqueue(job);
            await WaitForJob(job);
        }

        private async Task WaitForJob(ElectrumJob job)
        {
            var timeoutAt = DateTime.UtcNow.Add(job.Timeout).AddSeconds(5);
            while(!finishedJobs.ContainsKey(job.Id) && DateTime.UtcNow < timeoutAt)
            {
                await Task.Delay(50);
            }
            if(!finishedJobs.ContainsKey(job.Id))
            {
                job.Status = Core.Enums.JobStatus.TimedOut;
                return;
            }
            var fJob = finishedJobs[job.Id];
            job.Status = fJob.Status;
            job.Error = fJob.Error;
            job.ExecutionTime = fJob.ExecutionTime;
            job.JobStart = fJob.JobStart;
            finishedJobs.Remove(job.Id);
        }

        public Dictionary<string, List<string>> GetAvailableJobs()
        {
            return clientJobs;
        }

        public Core.Distribution.ClientInfo GetInfo()
        {
            return clientInfo;
        }
    }
}
