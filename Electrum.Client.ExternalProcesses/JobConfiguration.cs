using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Electrum.Client.ExternalProcesses
{
    public class JobConfiguration
    {

        public string Namespace { get; set; }
        public string Name { get; set; }
        public string ProcessPath { get; set; }
        public string Args { get; set; }

    }
}
