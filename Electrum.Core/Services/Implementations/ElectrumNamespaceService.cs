using Electrum.Core.Store;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Electrum.Core.Services.Implementations
{
    internal class ElectrumNamespaceService : IElectrumNamespaceService
    {

        public IDistributedCache Cache { get; }
        public ElectrumObjectRepositoryFactory RepositoryFactory { get; }
        private ILogger<ElectrumNamespaceService> Logger { get; }

        public ElectrumNamespaceService(ILogger<ElectrumNamespaceService> logger, IDistributedCache cache, ElectrumObjectRepositoryFactory repositoryFactory)
        {
            Cache = cache;
            RepositoryFactory = repositoryFactory;
            Logger = logger;
        }

        private bool ValidateName(string name)
        {
            return !name.Contains(",") && !name.Contains("&") && !name.Contains(" ");
        }

        public ElectrumNamespace CreateNamespace(string namespaceName)
        {
            if(!ValidateName(namespaceName))
            {
                throw new ArgumentException("Invalid name, cannot contain [,& ]", nameof(namespaceName));
            }
            Logger.LogInformation("Creating namespace {Name}", namespaceName);
            var parentParts = namespaceName.Split('/').SkipLast(1).ToList();
            if(parentParts.Count > 0)
            {
                GetOrCreateNamespace(string.Join("/", parentParts));
            }
            var ns = new ElectrumNamespace
            {
                Id = Guid.NewGuid(),
                Name = namespaceName
            };
            var nsRepo = RepositoryFactory.GetRepo<ElectrumNamespace>();
            return nsRepo.Add(ns);
        }

        public List<ElectrumNamespace> GetAllNamespaces()
        {
            var nsRepo = RepositoryFactory.GetRepo<ElectrumNamespace>();
            return nsRepo.ToList();
        }

        public ElectrumNamespace? GetNamespace(string namespaceName)
        {
            Logger.LogTrace("Getting namespace '{NamespaceName}'", namespaceName);
            var nsRepo = RepositoryFactory.GetRepo<ElectrumNamespace>();
            return nsRepo.FirstOrDefault(x => x.Name == namespaceName);
        }

        public List<ElectrumNamespace> GetNamespacesMatchingRegexPattern(string pattern)
        {
            Logger.LogTrace("Fetching namespace by regex pattern '{RegexPattern}'", pattern);
            var cacheKey = $"Electrum-NamespaceService-NSPattern-" + pattern;
            var cachedValue = Cache.GetString(cacheKey);
            var nsRepo = RepositoryFactory.GetRepo<ElectrumNamespace>();
            if (cachedValue != null)
            {
                var nsIds = cachedValue.Split(',').Select(x => new Guid(x)).ToList();
                return nsRepo.Where(x => nsIds.Contains(x.Id)).ToList();
            }
            var regex = new Regex(pattern, RegexOptions.Compiled);
            var matchingPattern = GetAllNamespaces().Where(x => regex.IsMatch(x.Name)).ToList();
            Cache.SetStringAsync(cacheKey, string.Join(",", matchingPattern.Select(x => x.Id.ToString())), new DistributedCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromMinutes(5)));
            return matchingPattern;
        }

        public ElectrumNamespace GetOrCreateNamespace(string namespaceName)
        {
            var ns = GetNamespace(namespaceName);
            if(ns == null)
            {
                ns = CreateNamespace(namespaceName);
            }
            return ns;
        }

        public List<ElectrumNamespace> GetParentNamespaces(string namespaceName)
        {
            Logger.LogTrace("Getting parent namespaces for '{NamespaceName}'", namespaceName);
            // application1/level1/level2 should return:
            // application1/level1
            // application1
            var parts = namespaceName.Split('/');
            if (parts.Length == 1) return new List<ElectrumNamespace>(); // If there only is one part, the namespace should not have any parents
            var possibleNames = new List<string>();
            string name = parts[0];
            foreach (var part in parts)
            {
                possibleNames.Add(name);
                name += "/" + part;
            }
            possibleNames.Add(name);
            var nsRepo = RepositoryFactory.GetRepo<ElectrumNamespace>();
            return nsRepo.Where(x => possibleNames.Contains(x.Name)).ToList();
        }
    }
}
