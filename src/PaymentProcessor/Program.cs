using PaymentProcessor;



var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.AddApplicationServices();
builder.Services.AddTemporalClient(clientTargetHost: "localhost:7233");
builder.Services.AddProblemDetails();

var withApiVersioning = builder.Services.AddApiVersioning();

builder.AddDefaultOpenApi(withApiVersioning);

var app = builder.Build();

app.MapDefaultEndpoints();

app.UseStatusCodePages();

app.MapPaymentProcessorEndpoints();

app.UseDefaultOpenApi();
app.Run();



