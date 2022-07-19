using Electrum.Core;
using Electrum.Store.InMemory;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Electrum.Tests
{
    public class TestConfigurator
    {
        static IServiceProvider? serviceProvider;

        public static IServiceProvider ServiceProvider
        {
            get { 
                if (serviceProvider == null)
                {
                    var services = new ServiceCollection();
                    services.UseElectron();
                    services.UseElectronInMemoryStore();
                    serviceProvider = services.BuildServiceProvider();
                }
                return serviceProvider;
            }
        }

        public static ElectrumNamespace Namespace = new ElectrumNamespace
        {
            Id = System.Guid.NewGuid(),
            Name = "Testing"
        };

    }
}
