using Electrum.Core.Enums;
using Electrum.Core.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Electrum.Core.Execution
{
    public class ExecutableJob
    {
        public ExecutableJob(string jobName, string? jobDesc, string ns, Type type, MethodInfo method)
        {
            JobName = jobName;
            JobDescription = jobDesc;
            Namespace = ns;
            Type = type;
            Method = method;
        }

        public string Namespace { get; set; }
        public string JobName { get; set; }
        public string? JobDescription { get; }
        public Type Type { get; }
        public MethodInfo Method { get; }

        private object? _typeInstance;
        public object? TypeInstance { get
            {
                if(_typeInstance == null)
                {
                    _typeInstance = Activator.CreateInstance(Type);
                }
                return _typeInstance;
            }
        }

        public ElectrumJob Execute(JobLogger jobLogger, ElectrumJob job)
        {
            var sw = Stopwatch.StartNew();
            try
            {
                var result = Method.Invoke(this, new object[] { jobLogger, job });
                job.Status = JobStatus.Success;
            } catch (Exception ex)
            {
                job.Status = JobStatus.Error;
                job.Error = ex.Message;
            }
            sw.Stop();
            job.ExecutionTime = sw.Elapsed;
            return job;
        }

    }
}
