using CargoWise.Common;
using CargoWise.ComponentModel;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Integration;
using Enterprise.Freight.QuotedBookings.Business;

namespace Enterprise.Freight.QuotedBookings.GUI
{
	public sealed class QuotedBookingContainerContractAllocationNotificationProvider : IRatingContractSimulationNotificationProvider
	{
		public QuotedBookingContainerContractAllocationNotificationProvider(ForwardingContainer container)
		{
			this.container = Argument.NotNull(container, nameof(container));
		}

		readonly ForwardingContainer container;

		public INotification GetContractAllocationNotification(IRatingContract contract)
		{
			var validator = new CCAContractBookingAssignmentValidator();
			if (validator.IsAllowedToAllocateToContract(container.QuotedBooking, contract, out var notification))
			{
				return null;
			}

			return notification;
		}

		public INotification GetRouteAllocationNotification(IRatingContractAllocationLine route)
		{
			var validator = new CCARouteBookingContainerAssignmentValidator();
			if (validator.IsAllowedToAllocateToRoute(container, route, out var notification))
			{
				return null;
			}

			return notification;
		}
	}
}
