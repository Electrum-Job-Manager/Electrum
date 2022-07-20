using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Electrum.Core.Services
{
    public interface IElectrumNamespaceService
    {

        public ElectrumNamespace? GetNamespace(string namespaceName);
        public ElectrumNamespace GetOrCreateNamespace(string namespaceName);
        public ElectrumNamespace CreateNamespace(string namespaceName);
        public List<ElectrumNamespace> GetAllNamespaces();
        public List<ElectrumNamespace> GetParentNamespaces(string namespaceName);
        public List<ElectrumNamespace> GetNamespacesMatchingRegexPattern(string pattern);

    }
}
