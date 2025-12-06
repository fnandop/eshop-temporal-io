using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;

namespace Temporal.Hosting
{
    public static class TemporalResourceBuilderExtensions
    {

        public static IResourceBuilder<TemporalResource> AddTemporal(this IDistributedApplicationBuilder builder, string name, int? grpcPort = null)
        {

            var temporalVersion = builder.Configuration["TEMPORAL_VERSION"] ?? "latest";
            // temporal core
            var resource = new TemporalResource(name);
            var temporal = builder.AddResource(resource).WithImage("temporalio/auto-setup", temporalVersion)
            .WithContainerName("temporal")
            .WithEnvironment("DYNAMIC_CONFIG_FILE_PATH", "config/dynamicconfig/development-sql.yaml")
            //.WithEnvironment("TEMPORAL_ADDRESS", "temporal:7233")
            //.WithEnvironment("TEMPORAL_CLI_ADDRESS", "temporal:7233")
            // ./dynamicconfig:/etc/temporal/config/dynamicconfig
            .WithBindMount(
                source: "./dynamicconfig",
                target: "/etc/temporal/config/dynamicconfig",
                isReadOnly: false
            )
            // 7233:7233
            .WithEndpoint(
                name: TemporalResource.TemporalServerGRPCEndpointName,
                port: grpcPort ?? 7233,
                targetPort: 7233,
                scheme: "grpc"
            );

            return temporal;
        }

        public static IResourceBuilder<TemporalResource> WithPostgres(this IResourceBuilder<TemporalResource> builder,
                                                                           IResourceBuilder<PostgresServerResource> postgres)
        {
            var postgresDatabase = postgres.Resource;
            builder
                .WithReference(postgres).WaitFor(postgres)
                .WithEnvironment("DB", "postgres12")
                .WithEnvironment("DB_PORT", postgresDatabase.PrimaryEndpoint.TargetPort.ToString())
                .WithEnvironment("POSTGRES_USER", postgresDatabase.UserNameReference!)
                .WithEnvironment("POSTGRES_PWD", postgresDatabase.PasswordParameter!)
                .WithEnvironment("POSTGRES_SEEDS", postgresDatabase.Name);
            return builder;
        }

        public static IResourceBuilder<TemporalResource> WithtTemporalAdminTools(this IResourceBuilder<TemporalResource> temporal)
        {
            var builder = temporal.ApplicationBuilder;
            var temporalAdminToolsVersion = builder.Configuration["TEMPORAL_ADMINTOOLS_VERSION"] ?? "latest";
            // temporal-admin-tools
            var temporalAdminTools = builder
                .AddContainer("temporal-admin-tools", "temporalio/admin-tools", temporalAdminToolsVersion)
                .WithContainerName("temporal-admin-tools")
                .WithReference(temporal)
                .WithEnvironment("TEMPORAL_ADDRESS", temporal.Resource.ConnectionStringExpression)
                .WithEnvironment("TEMPORAL_CLI_ADDRESS", temporal.Resource.ConnectionStringExpression);
            // stdin_open / tty are interactive runtime flags; Aspire doesn't need explicit mapping here..Name);
            return temporal;
        }


        public static IResourceBuilder<TemporalResource> WithtTemporalUi(this IResourceBuilder<TemporalResource> temporal, int? uiPort = null)
        {
            var builder = temporal.ApplicationBuilder;
            var temporalUiVersion = builder.Configuration["TEMPORAL_UI_VERSION"] ?? "latest";
            var temporalUi = builder
                  .AddContainer("temporal-ui", "temporalio/ui", temporalUiVersion)
                  .WithContainerName("temporal-ui")
                  .WithReference(temporal)
                  .WithEnvironment("TEMPORAL_ADDRESS", temporal.Resource.ConnectionStringExpression)
                  .WithEnvironment("TEMPORAL_CORS_ORIGINS", "http://localhost:3000")
                  // 8233:8080
                  .WithEndpoint(
                      name: TemporalResource.TemporalServerUIEndpointName,
                      port: uiPort ?? 8233,
                      targetPort: 8080,
                      scheme: "http"
                  );

            return temporal;
        }
    }
}
