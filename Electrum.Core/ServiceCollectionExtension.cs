using Electrum.Core.Store;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Electrum.Core
{
    public static class ServiceCollectionExtension
    {

        public static IServiceCollection UseElectron(this IServiceCollection services)
        {
            services.AddScoped<ElectrumJobManager>();
            services.AddScoped<ElectrumObjectRepositoryFactory>();
            return services;
        }

    }
}
