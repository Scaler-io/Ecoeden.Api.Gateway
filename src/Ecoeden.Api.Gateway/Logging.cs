using Destructurama;
using Ecoeden.Api.Gateway.Configurations;
using Serilog;
using Serilog.Events;

namespace Ecoeden.Api.Gateway;

public class Logging
{
    public static ILogger GetLogger(IConfiguration configuration, IWebHostEnvironment environment)
    {
        var loggingOption = configuration.GetSection(LoggingOption.OptionName).Get<LoggingOption>();
        var appConfigOption = configuration.GetSection(AppConfigOption.OptionName).Get<AppConfigOption>();
        var elasticOption = configuration.GetSection(ElasticSearchOption.OptionName).Get<ElasticSearchOption>();
        var logIndexPattern = $"Ecoeden.Api.Gateway-{environment.EnvironmentName}";

        Enum.TryParse(loggingOption.Console.LogLevel, false, out LogEventLevel minimumEventLevel);

        var loggerConfiguration = new LoggerConfiguration()
            .MinimumLevel.ControlledBy(new Serilog.Core.LoggingLevelSwitch(minimumEventLevel))
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information)
            .MinimumLevel.Override("System", LogEventLevel.Warning)
            .Enrich.FromLogContext()
            .Enrich.WithProperty(nameof(Environment.MachineName), Environment.MachineName)
            .Enrich.WithProperty(nameof(appConfigOption.ApplicationIdentifier), appConfigOption.ApplicationIdentifier)
            .Enrich.WithProperty(nameof(appConfigOption.ApplicationEnvironment), appConfigOption.ApplicationEnvironment);

        if (loggingOption.Console.Enabled)
        {
            loggerConfiguration.WriteTo.Console(minimumEventLevel, loggingOption.LogOutputTemplate);
        }
        if (loggingOption.Elastic.Enabled)
        {
            loggerConfiguration.WriteTo.Elasticsearch(elasticOption.Uri, logIndexPattern);
        }

        return loggerConfiguration
            .Destructure
            .UsingAttributes()
            .CreateLogger();
    }
}
