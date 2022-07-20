using Electrum.Core.Discovery;
using Electrum.Core.Execution;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Grpc.Net.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Electrum.Communication.gRPC.Worker
{
    public class ElectrumGRPCJobWorker
    {

        public string Host { get; set; }
        public JobExecutorService ExecutorService { get; }
        public GrpcChannel Channel { get; set; }
        public JobExecutionClient.JobExecutionClientClient Client { get; set; }
        public Core.Distribution.ClientInfo ElectrumClientInfo { get; set; }
        public CancellationToken CancellationToken { get; set; }
        public List<ExecutableJob> executableJobs = new List<ExecutableJob>();

        public ElectrumGRPCJobWorker(string electrumHost, string clientName, int maxConcurrentJobs, string accessKey, JobExecutorService executorService)
        {
            ElectrumClientInfo = new Core.Distribution.ClientInfo
            {
                Id = Guid.NewGuid(),
                ElectrumClientVersion = 1,
                AccessKey = accessKey,
                Name = clientName,
                MaxConcurrentJobs = maxConcurrentJobs,
                MachineName = Environment.MachineName
            };
            Host = electrumHost;
            ExecutorService = executorService;

            var credentials = CallCredentials.FromInterceptor((context, metadata) =>
            {
                if (!string.IsNullOrEmpty(accessKey))
                {
                    metadata.Add("Authorization", $"Key {accessKey}");
                }
                metadata.Add("Client-Id", $"{ElectrumClientInfo.Id.ToString()}");
                return Task.CompletedTask;
            });

            Channel = GrpcChannel.ForAddress(Host, new GrpcChannelOptions
            {
                Credentials = ChannelCredentials.Create(new SslCredentials(), credentials)
            });
            Client = new JobExecutionClient.JobExecutionClientClient(Channel);
        }

        public async void StartListener()
        {
            var grpcClientInfo = new ClientInfo()
            {
                Id = ElectrumClientInfo.Id.ToString(),
                Name = ElectrumClientInfo.Name,
                AccessKey = ElectrumClientInfo.AccessKey,
                ElectrumClientVersion = ElectrumClientInfo.ElectrumClientVersion,
                MachineName = ElectrumClientInfo.MachineName,
                MaxConcurrentJobs = ElectrumClientInfo.MaxConcurrentJobs,
            };
            grpcClientInfo.AvailableJobs.AddRange(ExecutorService.ExecutableJobsInNamespaces.ToDictionary(x => x.Key, x => x.Value.Select(x => x.JobName).ToList()).SelectMany(x => x.Value.Select(y => x.Key + "/" + y)).ToList());
            var request = Client.SubscribeToJobs(grpcClientInfo, deadline: DateTime.MaxValue, cancellationToken: CancellationToken);
            var response = request.ResponseStream;
            var jobLoggingClient = new GRPCJobLoggingClient(Client);
            while(!CancellationToken.IsCancellationRequested)
            {
                var next = await response.MoveNext(CancellationToken);
                if(next)
                {
                    var job = response.Current;
                    var elJob = new Core.ElectrumJob
                    {
                        Id = new Guid(job.Id),
                        Namespace = new Core.ElectrumNamespace
                        {
                            Name = job.Namespace
                        },
                        JobName = job.JobName,
                        Timeout = job.Timeout.ToTimeSpan(),
                        Parameters = job.Parameters.ToArray(),
                        Status = (Core.Enums.JobStatus) System.Enum.Parse(typeof(Core.Enums.JobStatus), System.Enum.GetName(typeof(Job.Types.JobStatus), job.Status))
                    };
                    Task.Run(() =>
                    {
                        var returnJob = ExecutorService.ExecuteJob(elJob, jobLoggingClient);
                        var grpcJob = new Job()
                        {
                            Id = returnJob.Id.ToString(),
                            JobName = returnJob.JobName,
                            Namespace = returnJob.Namespace.Name,
                            Timeout = Duration.FromTimeSpan(returnJob.Timeout),
                            Status = (Job.Types.JobStatus) System.Enum.Parse(typeof(Job.Types.JobStatus), System.Enum.GetName(typeof(Core.Enums.JobStatus), returnJob.Status))
                        };
                        Client.JobCompleted(grpcJob, deadline: DateTime.Now.AddMinutes(5));
                    });
                }
            }
        }

    }
}
