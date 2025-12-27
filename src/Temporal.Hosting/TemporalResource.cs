using Aspire.Hosting.ApplicationModel;

namespace Temporal.Hosting
{
    public sealed class TemporalResource(string name) : ContainerResource(name), IResourceWithConnectionString
    {

        internal const string TemporalServerGRPCEndpointName = "temporal-server";
        internal const string TemporalServerUIEndpointName = "temporal-server-ui";

        private EndpointReference? _temporalServerReference;
        private EndpointReference? _temporalServerUIReference;

        public EndpointReference TemporalServerEndpoint =>
            _temporalServerReference ??= new(this, TemporalServerGRPCEndpointName);

        public EndpointReference TemporalServerUiEndpoint =>
            _temporalServerUIReference ??= new(this, TemporalServerUIEndpointName);

        public ReferenceExpression ConnectionStringExpression =>
        ReferenceExpression.Create(
            $"{TemporalServerEndpoint.Property(EndpointProperty.HostAndPort)}"
        );

        /// <summary>
        /// Gets the connection string for the Temporal Server
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public ValueTask<string?> GetConnectionStringAsync(CancellationToken cancellationToken = default)
        {
            if (this.TryGetLastAnnotation<ConnectionStringRedirectAnnotation>(out var connectionStringAnnotation))
            {
                return connectionStringAnnotation.Resource.GetConnectionStringAsync(cancellationToken);
            }

            return ConnectionStringExpression.GetValueAsync(cancellationToken);
        }
    }



}
