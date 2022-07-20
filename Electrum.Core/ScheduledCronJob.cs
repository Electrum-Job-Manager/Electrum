using Cronos;
using Electrum.Core.Store;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Electrum.Core
{
    public class ScheduledCronJob
    {
        [ElectrumStoreKey]
        public Guid Id { get; set; }
        public string JobNamespace { get; set; }
        public string JobName { get; set; }
        public string CronSyntax { get; set; }
        public string[] Parameters { get; set; }
        public TimeSpan Timeout { get; set; }

        public DateTime? GetNextDate()
        {
            var expression = CronExpression.Parse(CronSyntax, CronFormat.IncludeSeconds);
            return expression.GetNextOccurrence(DateTime.UtcNow, true);
        }
    }
}
