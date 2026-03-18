using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace OrderingInfrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services,IConfiguration configure)
        {
        var connectionstring = configure.GetConnectionString("OrderingConnectionString");
            return services;
        }
    }
}
