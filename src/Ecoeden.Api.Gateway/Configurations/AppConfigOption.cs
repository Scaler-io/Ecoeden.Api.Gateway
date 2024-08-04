namespace Ecoeden.Api.Gateway.Configurations;

public class AppConfigOption
{
    public const string OptionName = "AppConfigurations";
    public string ApplicationIdentifier { get; set; }
    public string ApplicationEnvironment { get; set; }
}
