using PaymentProcessor;
using Temporalio.Extensions.Hosting;



var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.AddApplicationServices();
builder.Services.AddTemporalClient(clientTargetHost: "localhost:7233");
builder.Services.AddProblemDetails();

var withApiVersioning = builder.Services.AddApiVersioning();

builder.AddDefaultOpenApi(withApiVersioning);

// Temporal worker
builder.Services
    .AddHostedTemporalWorker(
        clientTargetHost: "localhost:7233",
        clientNamespace: "default",
        taskQueue: "eshop-payment-mock-task-queue")
    .AddScopedActivities<PaymentWorkflowMockDelayActivities>()
    .AddWorkflow<PaymentWorkflowMockDelay>();


var app = builder.Build();

app.MapDefaultEndpoints();

app.UseStatusCodePages();

app.MapPaymentProcessorEndpoints();

app.UseDefaultOpenApi();

await app.RunAsync();





