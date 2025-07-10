using System.Linq;
using CargoWise.ComponentModel;
using Enterprise.ContractManagement.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Integration;
using Enterprise.Freight.Integration.QuotedBooking;

namespace Enterprise.Freight.QuotedBookings.Business
{
	public sealed class CCARouteBookingAssignmentValidator
	{
		public bool IsAllowedToAllocateToRoute(
			IQuotedBooking quotedBooking,
			IRatingContractAllocationLine ratingContractAllocationRoute,
			out Notification notification)
		{
			notification = null;
			if (quotedBooking is not QuotedBooking booking || ratingContractAllocationRoute is not RatingContractAllocationLine allocationRoute)
			{
				return true;
			}

			if (allocationRoute.RCA_JX_SailingSchedule.IsEmpty)
			{
				if (CCARouteValidationHelper.IsETDBeforeRouteStartDate(allocationRoute, booking))
				{
					notification = new Notification(NotificationType.Error, CCAValidationMessageProvider.AllocationRouteStartDateViolated(allocationRoute, booking));
					return false;
				}
				else if (CCARouteValidationHelper.IsETDAfterRouteExpiryDate(allocationRoute, booking))
				{
					notification = new Notification(NotificationType.Error, CCAValidationMessageProvider.AllocationRouteExpiryDateViolated(allocationRoute, booking));
					return false;
				}

				if (CCARouteValidationHelper.IsLoadPortNotCoveredByRouteLoadLocation(allocationRoute, booking))
				{
					notification = new Notification(NotificationType.Error, CCAValidationMessageProvider.InvalidLoadPort(allocationRoute, booking));
					return false;
				}

				if (CCARouteValidationHelper.IsDischargePortNotCoveredByRouteDischargeLocation(allocationRoute, booking))
				{
					notification = new Notification(NotificationType.Error, CCAValidationMessageProvider.InvalidDischargePort(allocationRoute, booking));
					return false;
				}

				if (CCARouteValidationHelper.IsVoyageNumberMismatch(allocationRoute, booking))
				{
					notification = new Notification(NotificationType.Error, CCAValidationMessageProvider.InvalidVoyageNumber(allocationRoute, booking));
					return false;
				}

				if (CCARouteValidationHelper.IsVesselMismatch(allocationRoute, booking))
				{
					notification = new Notification(NotificationType.Error, CCAValidationMessageProvider.InvalidVessel(allocationRoute, booking));
					return false;
				}
			}
			else if ((!booking.Booking?.JS_JX.IsEmpty ?? false) && allocationRoute.RCA_JX_SailingSchedule != booking.Booking?.JS_JX)
			{
				notification = new Notification(NotificationType.Error, CCAValidationMessageProvider.InvalidBookingForLinkedAllocationRoute(allocationRoute));
				return false;
			}

			if (allocationRoute.RCA_HasBookingLimit)
			{
				var outstandingUtilization = ContractAllocationHelper.CalculateOutstandingUtilisation(booking.Factory, allocationRoute);
				if (outstandingUtilization < 0)
				{
					if (allocationRoute.RCA_AllocatedUQ.EqualsIgnoringCase(Core.Constants.AllocationQuantityUnits.Containers))
					{
						notification = new Notification(NotificationType.Error, CCAValidationMessageProvider.ContainerBookingLimitExceeded(allocationRoute, -outstandingUtilization));
						return false;
					}
					else
					{
						notification = new Notification(NotificationType.Error, CCAValidationMessageProvider.TEUBookingLimitExceeded(allocationRoute, -outstandingUtilization));
						return false;
					}
				}
			}

			if (CCARouteValidationHelper.IsBookingInvalidForAllocationRouteNamedAccounts(allocationRoute, booking))
			{
				notification = new Notification(NotificationType.Warning, CCAValidationMessageProvider.InvalidBookingForAllocationRouteNamedAccounts(allocationRoute));
				return false;
			}

			foreach (var container in booking.QuotedBookingContainers.OfType<ForwardingContainer>())
			{
				if (!allocationRoute.RCA_RC_ContainerType.IsEmpty
					&& !ContractAllocationHelper.DoesContainerTypeMatchAllocation(container, allocationRoute))
				{
					notification = new Notification(NotificationType.Error, CCAValidationMessageProvider.InvalidContainerCodeInCollection(allocationRoute, booking));
					return false;
				}
				else if (!allocationRoute.RCA_StorageOrFreightRateClass.IsEmpty
					&& !ContractAllocationHelper.DoesContainerTypeMatchAllocation(container, allocationRoute))
				{
					notification = new Notification(NotificationType.Error, CCAValidationMessageProvider.InvalidContainerClassInCollection(allocationRoute, booking));
					return false;
				}

				if (!allocationRoute.RCA_ContainerOwner.IsEmpty
				&& !CCARouteValidationHelper.IsValidContainerOwner(allocationRoute, container))
				{
					notification = new Notification(NotificationType.Error, CCAValidationMessageProvider.InvalidContainerOwnerInCollection(allocationRoute));
					return false;
				}
			}

			return true;
		}
	}
}
