using Electrum.Core.Store;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Electrum.Store.InMemory
{
    public static class ServiceCollectionExtension
    {

        public static IServiceCollection UseElectronInMemoryStore(this IServiceCollection services)
        {
            services.AddScoped<IElectrumObjectRepositoryProvider, InMemoryObjectRepositoryProvider>();
            return services;
        }

    }
}
