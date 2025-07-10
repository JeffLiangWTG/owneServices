using CargoWise.Common;
using CargoWise.ComponentModel;
using Enterprise.Freight.Integration;
using Enterprise.Freight.QuotedBookings.Business;

namespace Enterprise.Freight.QuotedBookings.GUI
{
	public sealed class QuotedBookingContractAllocationNotificationProvider : IRatingContractSimulationNotificationProvider
	{
		public QuotedBookingContractAllocationNotificationProvider(QuotedBooking quotedBooking)
		{
			Argument.NotNull(quotedBooking, nameof(quotedBooking));
			this.quotedBooking = quotedBooking;
		}

		readonly QuotedBooking quotedBooking;

		public INotification GetContractAllocationNotification(IRatingContract contract)
		{
			var assignmentValidator = new CCAContractBookingAssignmentValidator();
			if (assignmentValidator.IsAllowedToAllocateToContract(quotedBooking, contract, out var notification))
			{
				return null;
			}

			return notification;
		}

		public INotification GetRouteAllocationNotification(IRatingContractAllocationLine route)
		{
			var assignmentValidator = new CCARouteBookingAssignmentValidator();
			if (assignmentValidator.IsAllowedToAllocateToRoute(quotedBooking, route, out var notification))
			{
				return null;
			}

			return notification;
		}
	}
}
