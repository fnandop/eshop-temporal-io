using Aspire.Hosting.ApplicationModel;

namespace Temporal.Hosting
{
    public sealed class TemporalResource(string name) : ContainerResource(name), IResource
    {
       
    }
}
