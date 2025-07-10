using Enterprise.Freight.Integration;

namespace Enterprise.Freight.OnlineSailingSchedules
{
	public interface IRoutesProvider
	{
		Route[] GetRoutes(string searchParams, IServiceRequestManager requestManager);
	}
}
