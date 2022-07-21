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

        [Route("{id}")]
        public dynamic Get(string id)
        {
            return new
            {
                id = new Guid(id),
                result = "Nah"
            };
        }

    }
}
