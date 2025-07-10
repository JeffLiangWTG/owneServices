using CargoWise.Common;
using CargoWise.ComponentModel;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Integration;

namespace Enterprise.Freight.Forwarding.GUI
{
	sealed class ConsolContractAndRouteAllocationNotificationProvider : IRatingContractSimulationNotificationProvider
	{
		public ConsolContractAndRouteAllocationNotificationProvider(ForwardingConsol consol)
		{
			Argument.NotNull(consol, nameof(consol));
			this.consol = consol;
		}

		readonly ForwardingConsol consol;

		public INotification GetContractAllocationNotification(IRatingContract contract)
		{
			var assignmentValidator = new CCAContractConsolAssignmentValidator();
			if (!assignmentValidator.IsAllowedToAllocateToContract(consol, contract, out var notification))
			{
				return notification;
			}

			return null;
		}

		public INotification GetRouteAllocationNotification(IRatingContractAllocationLine route)
		{
			var assignmentValidator = new CCARouteConsolAssignmentValidator();
			if (!assignmentValidator.IsAllowedToAllocateToRoute(consol, route, out var notification))
			{
				return notification;
			}

			return null;
		}
	}
}
