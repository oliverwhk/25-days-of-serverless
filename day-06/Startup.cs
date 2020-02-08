using day_06;
using day_06.Model;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

[assembly: FunctionsStartup(typeof(Startup))]
namespace day_06
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.AddHttpClient();

            builder.Services.AddOptions<SchedulerOptions>()
                .Configure<IConfiguration>((settings, configuration) =>
                {
                    configuration.GetSection("SchedulerOptions").Bind(settings);
                });
        }
    }
}
