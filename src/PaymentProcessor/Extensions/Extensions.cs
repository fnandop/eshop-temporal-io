
public static class Extensions
{
    public static void AddApplicationServices(this IHostApplicationBuilder builder)
    {
        // Avoid loading full database config and migrations if startup
        // is being invoked from build-time OpenAPI generation
        if (builder.Environment.IsBuild())
            return;
        builder.Services.AddOptions<PaymentOptions>().BindConfiguration(nameof(PaymentOptions));

        var temporalServerHost = builder.Configuration.GetConnectionString("temporal");
        builder.Services.AddTemporalClient(clientTargetHost: temporalServerHost);
    }
}
