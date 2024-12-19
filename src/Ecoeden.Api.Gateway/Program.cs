using Ecoeden.Api.Gateway;
using Ecoeden.Api.Gateway.Configurations;
using Ecoeden.Api.Gateway.Middlewares;
using Ecoeden.Api.Gateway.Services.Subscription;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

var identityGroupAccess = builder.Configuration
    .GetSection(IdentityGroupAccessOption.OptionName)
    .Get<IdentityGroupAccessOption>();

var logger = Logging.GetLogger(builder.Configuration, builder.Environment);

builder.Services.AddOpenTelemetry()
    .WithTracing(tracing =>
    {
        tracing.SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("Ecoeden.ApiGateway"));
        tracing.AddAspNetCoreInstrumentation();
        tracing.AddHttpClientInstrumentation();
        tracing.AddZipkinExporter(options =>
        {
            options.Endpoint = new Uri(builder.Configuration["Zipkin:Url"]);
        });
    });

builder.Services.AddSingleton(x => logger);
builder.Host.UseSerilog(logger);

builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("customPolicy", policy =>
    {
        policy.PermitLimit = 4;
        policy.Window = TimeSpan.FromSeconds(12);
        policy.QueueProcessingOrder = System.Threading.RateLimiting.QueueProcessingOrder.OldestFirst;
        policy.QueueLimit = 2;
    });
});

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = identityGroupAccess.Authority;
        options.Audience = identityGroupAccess.Audience;
        options.RequireHttpsMetadata = false;
        options.TokenValidationParameters = new()
        {
            ValidateAudience = true,
            ValidateIssuer = true,
            ClockSkew = TimeSpan.Zero
        };
    });

// builder.Services.AddAuthorization(options =>
// {
//     options.AddPolicy("RequireScope", policy =>
//     {
//         policy.RequireClaim("scope");
//     });
// });

builder.Services.AddTransient<ISubscriptionValidationService, SubscriptionValidationService>();
builder.Services.AddTransient<SubscriptionValidationMiddlewar>();
builder.Services.AddCors(options =>
{
    options.AddPolicy("ecoedencors", policy =>
    {
        policy.WithOrigins("http://localhost:4200").AllowAnyMethod().AllowAnyHeader();
    });
});

var app = builder.Build();

app.UseCors("ecoedencors");
app.UseMiddleware<SubscriptionValidationMiddlewar>();
app.UseRateLimiter();
app.MapReverseProxy();
app.UseAuthentication();
app.UseAuthorization();

app.Run();
