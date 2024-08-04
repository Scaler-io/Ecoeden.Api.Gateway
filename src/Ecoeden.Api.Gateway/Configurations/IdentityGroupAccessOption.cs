namespace Ecoeden.Api.Gateway.Configurations;

public class IdentityGroupAccessOption
{
    public const string OptionName = "IdentityGroupAccess";
    public string Authority { get; set; }
    public string Audience { get; set; }
}
