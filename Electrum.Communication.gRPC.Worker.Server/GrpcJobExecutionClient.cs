using Electrum.Core.Distribution;
using Electrum.Core.Logging;
using Google.Protobuf.Collections;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Linq;

namespace Electrum.Communication.gRPC.Worker.Server
{
    public class GrpcJobExecutionClient : JobExecutionClient.JobExecutionClientBase
    {

        private static Dictionary<string, GrpcJobExecutor> jobExecutors = new Dictionary<string, GrpcJobExecutor>();
        
        public JobLog JobLog { get; }
        public ILogger<GrpcJobExecutionClient> Logger { get; }
        public JobDistributionService JobDistributionService { get; set; }
        private IHttpContextAccessor _httpContextAccessor;

        public GrpcJobExecutionClient(ILogger<GrpcJobExecutionClient> logger, JobDistributionService jobDistributionService, JobLog jobLog, IHttpContextAccessor httpContextAccessor)
        {
            JobLog = jobLog;
            Logger = logger;
            JobDistributionService = jobDistributionService;
            _httpContextAccessor = httpContextAccessor;
        }

        private string AuthKey
        {
            get
            {
                var accessKey = _httpContextAccessor.HttpContext.Request.Headers["AccessKey"];
                var clientId = _httpContextAccessor.HttpContext.Request.Headers["Client-Id"];
                return clientId +":" + accessKey;
            }
        }


        public override Task SubscribeToJobs(ClientInfo request, IServerStreamWriter<Job> responseStream, ServerCallContext context)
        {
            return Task.Run(async () =>
            {
                var clientInfo = new Core.Distribution.ClientInfo
                {
                    Id = new Guid(request.Id),
                    Name = request.Name,
                    MachineName = request.MachineName,
                    MaxConcurrentJobs = request.MaxConcurrentJobs,
                    AccessKey = request.AccessKey,
                    ElectrumClientVersion = request.ElectrumClientVersion,
                };
                var jobs = request.AvailableJobs.ToList();
                var jobsDict = jobs.GroupBy(x => string.Join("/", x.Split('/').SkipLast(1).ToList())).ToDictionary(x => x.Key, x => x.Select(y => y.Split('/').TakeLast(1).FirstOrDefault() ?? "-").ToList());
                var jobExecutor = new GrpcJobExecutor(clientInfo, jobsDict);
                jobExecutors.Add(AuthKey, jobExecutor);
                JobDistributionService.AddClient(jobExecutor);
                while (!context.CancellationToken.IsCancellationRequested)
                {
                    while (jobExecutor.jobQueue.Count > 0)
                    {
                        var job = jobExecutor.jobQueue.Dequeue();
                        var grpcJob = new Job()
                        {
                            Id = job.Id.ToString(),
                            JobName = job.JobName,
                            Namespace = job.Namespace.Name,
                            Timeout = Duration.FromTimeSpan(job.Timeout),
                            Status = (Job.Types.JobStatus)System.Enum.Parse(typeof(Job.Types.JobStatus), System.Enum.GetName(typeof(Core.Enums.JobStatus), job.Status))
                        };
                        grpcJob.Parameters.AddRange(job.Parameters);
                        await responseStream.WriteAsync(grpcJob);
                    }
                    await Task.Delay(50);
                }
                JobDistributionService.RemoveClient(clientInfo.Id);
            });
        }

        

        public override Task<Empty> JobCompleted(Job job, ServerCallContext context)
        {
            var elJob = new Core.ElectrumJob
            {
                Id = new Guid(job.Id),
                Namespace = new Core.ElectrumNamespace
                {
                    Name = job.Namespace
                },
                JobName = job.JobName,
                JobStart = job.JobStart.ToDateTime(),
                Timeout = job.Timeout.ToTimeSpan(),
                Parameters = job.Parameters.ToArray(),
                ExecutionTime = job.ExecutionTime.ToTimeSpan(),
                Status = (Core.Enums.JobStatus)System.Enum.Parse(typeof(Core.Enums.JobStatus), System.Enum.GetName(typeof(Job.Types.JobStatus), job.Status))
            };
            jobExecutors[AuthKey].finishedJobs.Add(elJob.Id, elJob);
            return Task.FromResult(new Empty());
        }

        private JobLogger.JobLogRow GrpcRowToElectrum(JobLogRow row)
        {
            var n = new JobLogger.JobLogRow((Electrum.Core.Logging.LogLevel)row.Level, row.Message)
            {
                JobId = new Guid(row.JobId),
                RowId = new Guid(row.RowId),
                Error = row.Error != null ? new JobLogger.JobLogRow.JobLogRowError
                {
                    Message = row.Error.Message.Length > 0 ? row.Error.Message : null,
                    StackTrace = row.Error.StackTrace.Length > 0 ? row.Error.StackTrace : null,
                    TypeName = row.Error.TypeName.Length > 0 ? row.Error.TypeName : null,
                } : null,
                Level = (Electrum.Core.Logging.LogLevel)row.Level,
                Message = row.Message.Length > 0 ? row.Message : null,
                Template = row.Template.Length > 0 ? row.Template : null,
                Properties = row.Properties.ToDictionary(x => x.Key, x => x.Value),
                UtcTimestamp = row.UtcTimestamp.ToDateTime()
            };
            return n;
        }

        public override Task<Empty> WriteLogRow(JobLogRow request, ServerCallContext context)
        {
            var jobId = new Guid(request.JobId);
            var row = GrpcRowToElectrum(request);
            JobLog.WriteRow(row);
            return Task.FromResult(new Empty());
        }

        public override Task<Empty> WriteLogRows(JobLogRowBatch request, ServerCallContext context)
        {
            var jobId = new Guid(request.JobId);
            var rows = request.Rows.Select(x => GrpcRowToElectrum(x)).ToList();
            JobLog.WriteRows(rows);
            return Task.FromResult(new Empty());
        }

    }
}