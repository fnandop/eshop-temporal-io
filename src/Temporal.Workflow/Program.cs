// using YOUR ServiceDefaults namespace
using Duende.AccessTokenManagement;
using eShop.ServiceDefaults;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Refit;
using Temporal.Workflow;
using Temporalio.Extensions.Hosting;


var builder = Host.CreateApplicationBuilder(args);

// Let Aspire ServiceDefaults wire up:
// - OpenTelemetry
// - Health checks
// - Service discovery
// - HttpClient defaults (AddServiceDiscovery, resilience, etc.)
builder.AddServiceDefaults(); // keep this if you're using Aspire / service defaults

//builder.Services.AddHttpClient<IOrderService, OrderService>(client =>
//{
//    // IMPORTANT: use the Aspire service discovery scheme + resource name
//    client.BaseAddress = new Uri("https+http://ordering-api");
//});

var identitySection = builder.Configuration.GetSection("Identity");
var identityUrl = identitySection.GetRequiredValue("Url");

builder.Services.AddClientCredentialsTokenManagement()
    .AddClient(ClientCredentialsClientName.Parse("temporalworkflow.client"), client =>
{

    client.TokenEndpoint = new Uri($"{identityUrl}/connect/token");
    
    client.ClientId = ClientId.Parse("temporal-workflow-worker");
    client.ClientSecret = ClientSecret.Parse("batata");

    client.Scope = Scope.Parse("orders");
});

//// Refit client that uses Aspire service discovery
builder.Services
    .AddRefitClient<IOrderService>()
    .ConfigureHttpClient(c =>
    {
        // Logical service name. Aspire will resolve this via service discovery.
        // `https+http` means "prefer https, fall back to http".
        c.BaseAddress = new Uri("https+http://ordering-api");
    }).
    AddClientCredentialsTokenHandler(ClientCredentialsClientName.Parse("temporalworkflow.client"));
;



//// Refit client that uses Aspire service discovery
builder.Services
    .AddRefitClient<ICatalogService>()
    .ConfigureHttpClient(c =>
    {
        // Logical service name. Aspire will resolve this via service discovery.
        // `https+http` means "prefer https, fall back to http".
        c.BaseAddress = new Uri("http://catalog-api");
    }).
    AddClientCredentialsTokenHandler(ClientCredentialsClientName.Parse("temporalworkflow.client"));
;

builder.Services
    .AddRefitClient<IPaymentsService>()
    .ConfigureHttpClient(c =>
    {
        // Logical service name. Aspire will resolve this via service discovery.
        // `https+http` means "prefer https, fall back to http".
        c.BaseAddress = new Uri("http://payment-processor");
    }).
    AddClientCredentialsTokenHandler(ClientCredentialsClientName.Parse("temporalworkflow.client"));
;


// Temporal worker
var temporalServerHost = builder.Configuration.GetConnectionString("temporal");
builder.Services
    .AddHostedTemporalWorker(
        clientTargetHost: temporalServerHost!,
        clientNamespace: "default",
        taskQueue: "eshop-task-queue")
    .AddScopedActivities<EShopActivities>()
    .AddWorkflow<EShopWorkflow>();

var host = builder.Build();
await host.RunAsync();
