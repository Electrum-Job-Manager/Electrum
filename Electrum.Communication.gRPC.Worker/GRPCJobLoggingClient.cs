using Electrum.Core.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Electrum.Communication.gRPC.Worker
{
    internal class GRPCJobLoggingClient : IJobLoggingClient
    {

        public GRPCJobLoggingClient(JobExecutionClient.JobExecutionClientClient client)
        {
            Client = client;
        }

        public JobExecutionClient.JobExecutionClientClient Client { get; }

        public void DeleteRows(Guid job)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<JobLogger.JobLogRow> GetRows(Guid job)
        {
            throw new NotImplementedException();
        }

        public JobLogRow ConvertRow(Guid job, JobLogger.JobLogRow row)
        {
            var nr = new JobLogRow
            {
                RowId = row.RowId.ToString(),
                JobId = job.ToString(),
                Template = row.Template ?? "",
                Error = row.Error != null ? new JobLogRow.Types.JobLogRowError
                {
                    Message = row.Error.Message ?? "",
                    StackTrace = row.Error.StackTrace ?? "",
                    TypeName = row.Error.TypeName ?? ""
                } : null,
                Level = (JobLogRow.Types.LogLevel)row.Level,
                Message = row.Message ?? "",
                UtcTimestamp = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(row.UtcTimestamp),
            };
            if(row.Properties != null)
                nr.Properties.Add(row.Properties);
            return nr;
        }

        public void WriteRow(Guid job, JobLogger.JobLogRow row)
        {
            Client.WriteLogRowAsync(ConvertRow(job, row));
        }

        public void WriteRows(Guid job, IEnumerable<JobLogger.JobLogRow> rows)
        {
            var res = new JobLogRowBatch
            {
                JobId = job.ToString()
            };
            res.Rows.Add(rows.Select(x => ConvertRow(job, x)).ToList());
            Client.WriteLogRowsAsync(res);
        }
    }
}
