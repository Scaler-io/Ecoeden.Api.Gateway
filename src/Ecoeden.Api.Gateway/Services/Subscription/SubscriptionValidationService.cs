
using Ecoeden.Api.Gateway.Extensions;

namespace Ecoeden.Api.Gateway.Services.Subscription;

public class SubscriptionValidationService(ILogger logger) : ISubscriptionValidationService
{
    private readonly ILogger _logger = logger;

    private readonly Dictionary<string, List<string>> _subscriptions = new Dictionary<string, List<string>>
    {
        { "F340FE8EA8604456AC4E66F31A87574C", [ "/catalogue" ] },
        { "79AE4A5B04CC48B887E38FAE7D1282C0", [ "/search" ] },
        { "7B6AD94DCC3C4E9F891C52C8C340D99E", [ "/user" ] }
    };

    public Task<bool> CheckSubscriptionValidity(string subscriptionKey, string apiPath)
    {

        if (_subscriptions.TryGetValue(subscriptionKey, out var subscribedApis))
        {
            return Task.FromResult(subscribedApis.Contains(apiPath));
        }

        _logger.Here().Error("No subscribed api found");
        return Task.FromResult(false);
    }
}
