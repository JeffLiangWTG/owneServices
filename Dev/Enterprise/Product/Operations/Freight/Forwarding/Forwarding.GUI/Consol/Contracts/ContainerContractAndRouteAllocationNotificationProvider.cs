using CargoWise.Common;
using CargoWise.ComponentModel;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Integration;

namespace Enterprise.Freight.Forwarding.GUI
{
	sealed class ConsolContainerAllocationNotificationProvider : IRatingContractSimulationNotificationProvider
	{
		public ConsolContainerAllocationNotificationProvider(ForwardingContainer container)
		{
			Argument.NotNull(container, nameof(container));
			this.container = container;
		}

		readonly ForwardingContainer container;

		public INotification GetContractAllocationNotification(IRatingContract contract)
		{
			var assignmentValidator = new CCAContractConsolAssignmentValidator();
			if (!assignmentValidator.IsAllowedToAllocateToContract(container.Consol, contract, out var notification))
			{
				return notification;
			}

			return null;
		}

		public INotification GetRouteAllocationNotification(IRatingContractAllocationLine route)
		{
			var assignmentValidator = new CCARouteConsolContainerAssignmentValidator();
			if (!assignmentValidator.IsAllowedToAllocateToRoute(container, route, out var notification))
			{
				return notification;
			}

			return null;
		}
	}
}
