using Electrum.Core.Store;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Electrum.Core
{
    public class ElectrumJobManager
    {

        ElectrumObjectRepositoryFactory RepositoryFactory { get; set; }

        public ElectrumJobManager(ElectrumObjectRepositoryFactory repositoryFactory) { 
            RepositoryFactory = repositoryFactory;
        }

        public ElectrumJob Enqueue(ElectrumJob job)
        {
            return RepositoryFactory.GetRepo<ElectrumJob>().Add(job);
        }

    }
}
