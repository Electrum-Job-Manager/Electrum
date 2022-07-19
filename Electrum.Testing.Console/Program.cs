
using Electrum.Core.Store;
using Microsoft.Extensions.DependencyInjection;

var services = new ServiceCollection();
services.AddSingleton<IElectrumObjectRepositoryProvider>();