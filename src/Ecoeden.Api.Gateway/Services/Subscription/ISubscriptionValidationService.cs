namespace Ecoeden.Api.Gateway.Services.Subscription;

public interface ISubscriptionValidationService
{
    public Task<bool> CheckSubscriptionValidity(string subscriptionKey, string apiPath);
}
