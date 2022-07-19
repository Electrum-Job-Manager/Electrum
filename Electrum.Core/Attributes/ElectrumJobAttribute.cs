using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Electrum.Core.Attributes
{
    public class ElectrumJobAttribute : Attribute
    {

        public string JobName { get; set; }
        public string? JobDescription { get; set; }

        public ElectrumJobAttribute(string name, string? description = null)
        {
            this.JobName = name;
            this.JobDescription = description;
        }

    }
}
