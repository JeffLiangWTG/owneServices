using CargoWise.ComponentModel;

namespace Enterprise.Freight.Integration
{
	public interface IRatingContractSimulationNotificationProvider
	{
		INotification GetContractAllocationNotification(IRatingContract contract);
		INotification GetRouteAllocationNotification(IRatingContractAllocationLine route);
	}
}
