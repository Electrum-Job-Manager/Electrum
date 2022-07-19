using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Electrum.Core.Logging.JobLogger;

namespace Electrum.Core.Logging
{
    public interface IJobLoggingClient
    {

        public void WriteRow(Guid job, JobLogRow row);
        public void WriteRows(Guid job, IEnumerable<JobLogRow> rows);
        public IEnumerable<JobLogRow> GetRows(Guid job);
        public void DeleteRows(Guid job);

    }
}
