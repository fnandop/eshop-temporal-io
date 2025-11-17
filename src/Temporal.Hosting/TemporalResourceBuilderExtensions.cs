using System;
using System.Collections.Generic;
using System.Text;
using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;

namespace Temporal.Hosting
{
    public static class TemporalResourceBuilderExtensions
    {


        public static IResourceBuilder<TemporalResource> AddTemporal(
       this IDistributedApplicationBuilder builder,
       string name,
        int? grpcPort = null,
        int? uiPort = null)
        {


            var temporalDbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Temporal", "StartDev","DB");
            Directory.CreateDirectory(temporalDbPath);

            // The AddResource method is a core API within Aspire and is
            // used by resource developers to wrap a custom resource in an
            // IResourceBuilder<T> instance. Extension methods to customize
            // the resource (if any exist) target the builder interface.
            var resource = new TemporalResource(name);
            // check https://github.com/temporalio/cli
            return builder.AddResource(resource)
                          .WithImage(TemporalContainerImageTags.Image)
                          .WithImageRegistry(TemporalContainerImageTags.Registry)
                          .WithImageTag(TemporalContainerImageTags.Tag)
                           //.WithVolume(target: "/var/opt/temporal")
                             .WithBindMount(source: temporalDbPath, target: "/var/opt/temporal")
                           .WithArgs("server", "start-dev",
                                     "--ip", "0.0.0.0" ,
                                     "--db-filename", "/var/opt/temporal/temporal.db"
                           )


                          // Expose port 7233 (Temporal gRPC server)
                          .WithEndpoint(targetPort: 7233,
                                        port: grpcPort,
                                        name: TemporalResource.TemporalServerGRPCEndpointName)

                             // Expose port 8233 (Temporal Web UI)
                             .WithHttpEndpoint(targetPort: 8233,
                                        port: uiPort,
                                            name: TemporalResource.TemporalServerUIEndpointName);

        }

        internal static class TemporalContainerImageTags
        {
            internal const string Registry = "docker.io";

            internal const string Image = "temporalio/temporal";

            internal const string Tag = "latest";
        }
    }
}
