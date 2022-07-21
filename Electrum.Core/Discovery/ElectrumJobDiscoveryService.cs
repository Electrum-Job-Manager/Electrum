using Electrum.Core.Attributes;
using Electrum.Core.Execution;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Electrum.Core.Discovery
{
    public class ElectrumJobDiscoveryService : IJobDiscoveryService
    {

        public List<ExecutableJob> DiscoverJobExecutors()
        {
            var jobExecutorList = new List<ExecutableJob>();
            var typesWithJobNamespaces = from assembly in AppDomain.CurrentDomain.GetAssemblies()
                                         from type in assembly.GetTypes()
                                         where type.IsDefined(typeof(ElectrumJobNamespaceAttribute), true)
                                         select type;
            foreach (var type in typesWithJobNamespaces)
            {
                var nsAttr = type.GetCustomAttribute(typeof(ElectrumJobNamespaceAttribute)) as ElectrumJobNamespaceAttribute;
                if (nsAttr == null) continue;
                var ns = nsAttr.NamespaceName;

                var methods = from method in type.GetMethods()
                              where method.IsDefined(typeof(ElectrumJobAttribute), true)
                              select method;
                foreach (var method in methods)
                {
                    var jobAttr = method.GetCustomAttribute(typeof(ElectrumJobAttribute)) as ElectrumJobAttribute;
                    if (jobAttr == null) continue;
                    var jobName = jobAttr.JobName;
                    var jobDesc = jobAttr.JobDescription;
                    jobExecutorList.Add(new ExecutableJob(jobName, jobDesc, ns, type, method));
                }
            }
            return jobExecutorList;
        }

    }
}
