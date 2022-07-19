using Electrum.Core.Enums;

namespace Electrum.Core
{
    public class ElectrumJob
    {

        public Guid Id { get; set; }
        public ElectrumNamespace Namespace { get; set; }
        public string JobName { get; set; }

        public string[] Parameters { get; set; }
        public TimeSpan ExecutionTime { get; set; }
        public JobStatus Status { get; set; }
        public string? Error { get; set; }
    }
}