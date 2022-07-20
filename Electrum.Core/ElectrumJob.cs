using Electrum.Core.Enums;
using Electrum.Core.Store;

namespace Electrum.Core
{
    public class ElectrumJob
    {
        [ElectrumStoreKey]
        public Guid Id { get; set; }
        public ElectrumNamespace Namespace { get; set; }
        public string JobName { get; set; }
        public TimeSpan Timeout { get; set; }

        public DateTime JobStart { get; set; }
        public string[] Parameters { get; set; }
        public TimeSpan ExecutionTime { get; set; }
        public JobStatus Status { get; set; }
        public string? Error { get; set; }
    }
}