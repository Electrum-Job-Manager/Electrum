using Electrum.Core.Store;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Electrum.Core.Logging
{
    public class JobLog
    {

        public JobLog(ILogger<JobLog> logger, ElectrumObjectRepositoryFactory repositoryFactory)
        {
            Logger = logger;
            RepositoryFactory = repositoryFactory;
            LogRepository = repositoryFactory.GetRepo<JobLogger.JobLogRow>();
        }

        private ILogger<JobLog> Logger { get; }
        private ElectrumObjectRepositoryFactory RepositoryFactory { get; }
        private IElectrumObjectRepository<JobLogger.JobLogRow> LogRepository { get; }

        public void WriteRow(JobLogger.JobLogRow row)
        {
            LogRepository.Add(row);
        }

        public void WriteRows(IEnumerable<JobLogger.JobLogRow> allRows)
        {
            var rowsPerJob = allRows.GroupBy(x => x.JobId).ToDictionary(x => x.Key, x => x.ToList());
            foreach (var job in rowsPerJob)
            {
                var jobId = job.Key;
                var rows = job.Value;
                var jobRows = LogRepository.Where(x => x.JobId == jobId).ToList();
                var newRows = rows.Where(x => !jobRows.Any(y => y.RowId == x.RowId)).ToList();
                if (newRows.Any())
                {
                    Logger.LogDebug("Writing {Count} rows for job {JobId}", newRows.Count(), jobId);
                }
                foreach (var row in newRows)
                {
                    LogRepository.Add(row);
                }
            }
        }

        public List<JobLogger.JobLogRow> GetRowsForJob(Guid jobId)
        {
            return LogRepository.Where(x => x.JobId == jobId).OrderBy(x => x.UtcTimestamp).ToList();
        }
    }
}
