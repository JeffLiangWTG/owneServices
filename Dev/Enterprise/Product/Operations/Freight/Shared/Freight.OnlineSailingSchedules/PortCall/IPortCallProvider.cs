using Enterprise.Freight.Integration;

namespace Enterprise.Freight.OnlineSailingSchedules.PortCall
{
	public interface IPortCallProvider
	{
		PortCall GetPortCall(string searchParams, IServiceRequestManager requestManager);
	}
}
