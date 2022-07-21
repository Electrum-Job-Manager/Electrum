using Electrum.Core;
using Electrum.Core.API;
using Electrum.Core.Logging;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Electrum.Server.REST.v1.Controllers
{
    [ApiController]
    [Route("/api/v1/job")]
    public class JobController : ControllerBase
    {

        public JobController(JobAPIService service) => (Service) = (service);

        public JobAPIService Service { get; }

        [Route("{id}")]
        public ElectrumJob Get(string id)
        {
            return Service.GetJob(new Guid(id));
        }
        
        [Route("{id}/logs")]
        public List<JobLogger.JobLogRow> GetLogs(string id)
        {
            return Service.GetLogs(new Guid(id));
        }

        public ScheduledJob Post(ElectrumJob job)
        {
            return Service.QueueJob(job);
        }

    }
}
