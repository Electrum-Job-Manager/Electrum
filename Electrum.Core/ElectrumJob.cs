using Electrum.Core.Enums;

namespace Electrum.Core
{
    public class ElectrumJob
    {

        public Guid Id { get; set; }
        public ElectrumNamespace Namespace { get; set; }
        public string JobName { get; set; }

        public string[] Parameters { get; set; }
        public TimeSpan ExecutionTime { get; internal set; }
        public JobResult Result { get; internal set; }
        public string? Error { get; internal set; }
    }
}