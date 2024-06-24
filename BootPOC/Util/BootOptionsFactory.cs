using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;

namespace BootPOC.Util
{

    // Create a BootOptions according to the provided job identity. This is used to get around the Hangfire dependency injection problem
    public class BootOptionsFactory : IBootOptionsFactory
    {
        private readonly IServiceProvider _provider;

        public BootOptionsFactory(IServiceProvider provider)
        {
            _provider = provider;
        }

        public IBootOptions Create(string jobIdentity)
        {
            return _provider.GetServices<IBootOptions>().FirstOrDefault(b => b.Job.Identity == jobIdentity);
        }
    }
}