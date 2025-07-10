using Enterprise.Freight.Integration;
using Enterprise.Freight.OnlineSailingSchedules;

namespace Enterprise.Freight.Forwarding.GUI.Testing
{
	public class EmptyRoutesProviderForTest : IRoutesProvider
	{
		public Route[] GetRoutes(string searchParams, IServiceRequestManager requestManager)
		{
			return System.Array.Empty<Route>();
		}
	}
}
