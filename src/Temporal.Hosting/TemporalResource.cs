using Aspire.Hosting.ApplicationModel;

namespace Temporal.Hosting
{
    public sealed class TemporalResource(string name) : ContainerResource(name), IResourceWithConnectionString
    {

        internal const string TemporalServerGRPCEndpointName = "temporal-server";
        internal const string TemporalServerUIEndpointName = "temporal-server-ui";

        private EndpointReference? _temporalServerReference;

        public EndpointReference TemporalServerEndpoint =>
       _temporalServerReference ??= new(this, TemporalServerGRPCEndpointName);

        public ReferenceExpression ConnectionStringExpression =>
        ReferenceExpression.Create(
            $"{TemporalServerEndpoint.Property(EndpointProperty.HostAndPort)}"
        );


    }



}
