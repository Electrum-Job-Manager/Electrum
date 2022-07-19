using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Electrum.Core.Attributes
{
    public class ElectrumJobNamespaceAttribute : Attribute
    {

        public string NamespaceName { get; set; }

    }
}
