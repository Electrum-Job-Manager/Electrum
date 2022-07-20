using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Electrum.Communication.Tcp
{
    public class RPCPacket
    {
        public Guid Id { get; set; }
        public string ServiceName { get; set; }
        public string MethodName { get; set; }
        public string[] ParameterTypes { get; set; }
        public object[] Parameters { get; set; }

        public object[] GetCorrectParameters()
        {
            // Redo the parameters into their correct types
            var newParams = new object[Parameters.Length];
            for (int i = 0; i < Parameters.Length; i++)
            {
                // Reformat as json
                var json = JsonConvert.SerializeObject(Parameters[i]);
                var type = Type.GetType(ParameterTypes[i]);
                var reformattedType = JsonConvert.DeserializeObject(json, type);
                newParams[i] = reformattedType;
            }
            return newParams;
        }

    }
}
