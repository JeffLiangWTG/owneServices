using CargoWise.ComponentModel;
using Enterprise.ContractManagement.Business;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Integration;

namespace Enterprise.Freight.QuotedBookings.Business
{
	public sealed class CCARouteBookingContainerAssignmentValidator
	{
		public bool IsAllowedToAllocateToRoute(
			ForwardingContainer container,
			IRatingContractAllocationLine allocationRoute,
			out Notification notification)
		{
			notification = null;
			var booking = container.QuotedBooking;
			if (booking is not ICCACommonAssignmentValidationData validationData ||
				allocationRoute is not RatingContractAllocationLine route)
			{
				return true;
			}

			if (route.RCA_JX_SailingSchedule.IsEmpty)
			{
				if (CCARouteValidationHelper.IsETDBeforeRouteStartDate(route, validationData))
				{
					notification = new Notification(NotificationType.Error, CCAValidationMessageProvider.AllocationRouteStartDateViolated(route, validationData));
					return false;
				}
				else if (CCARouteValidationHelper.IsETDAfterRouteExpiryDate(route, validationData))
				{
					notification = new Notification(NotificationType.Error, CCAValidationMessageProvider.AllocationRouteExpiryDateViolated(route, validationData));
					return false;
				}

				if (CCARouteValidationHelper.IsLoadPortNotCoveredByRouteLoadLocation(route, validationData))
				{
					notification = new Notification(NotificationType.Error, CCAValidationMessageProvider.InvalidLoadPort(route, validationData));
					return false;
				}

				if (CCARouteValidationHelper.IsDischargePortNotCoveredByRouteDischargeLocation(route, validationData))
				{
					notification = new Notification(NotificationType.Error, CCAValidationMessageProvider.InvalidDischargePort(route, validationData));
					return false;
				}

				if (CCARouteValidationHelper.IsVoyageNumberMismatch(route, validationData))
				{
					notification = new Notification(NotificationType.Error, CCAValidationMessageProvider.InvalidVoyageNumber(route, validationData));
					return false;
				}

				if (CCARouteValidationHelper.IsVesselMismatch(route, validationData))
				{
					notification = new Notification(NotificationType.Error, CCAValidationMessageProvider.InvalidVessel(route, validationData));
					return false;
				}
			}
			else if (!booking.SailingJX.IsEmpty && booking.SailingJX != route.RCA_JX_SailingSchedule)
			{
				notification = new Notification(NotificationType.Error, CCAValidationMessageProvider.InvalidBookingContainerForLinkedAllocationRoute(route));
				return false;
			}

			if (route.RCA_HasBookingLimit)
			{
				var outstandingUtilization = ContractAllocationHelper.CalculateOutstandingUtilisation(container.Factory, route);
				if (outstandingUtilization < 0)
				{
					if (route.RCA_AllocatedUQ.EqualsIgnoringCase(Core.Constants.AllocationQuantityUnits.Containers))
					{
						notification = new Notification(NotificationType.Error, CCAValidationMessageProvider.ContainerBookingLimitExceeded(route, -outstandingUtilization));
						return false;
					}
					else
					{
						notification = new Notification(NotificationType.Error, CCAValidationMessageProvider.TEUBookingLimitExceeded(route, -outstandingUtilization));
						return false;
					}
				}
			}

			if (CCARouteValidationHelper.IsContainerTypeInvalidForContract(route.Contract, container))
			{
				notification = new Notification(NotificationType.Error, CCAValidationMessageProvider.InvalidContainerTypeForContract(route, container.RefContainer, validationData));
				return false;
			}
			else if (!route.RCA_RC_ContainerType.IsEmpty
				&& !ContractAllocationHelper.DoesContainerTypeMatchAllocation(container, route))
			{
				notification = new Notification(NotificationType.Error, CCAValidationMessageProvider.InvalidContainerCodeForRoute(route, container.RefContainer, booking));
				return false;
			}
			else if (!route.RCA_StorageOrFreightRateClass.IsEmpty
				&& !ContractAllocationHelper.DoesContainerTypeMatchAllocation(container, route))
			{
				notification = new Notification(NotificationType.Error, CCAValidationMessageProvider.InvalidContainerClassForRoute(route, container.RefContainer, booking));
				return false;
			}

			if (!route.RCA_ContainerOwner.IsEmpty
				&& !CCARouteValidationHelper.IsValidContainerOwner(route, container))
			{
				notification = new Notification(NotificationType.Error, CCAValidationMessageProvider.InvalidContainerOwnerForRoute(route));
				return false;
			}

			if (CCARouteValidationHelper.IsBookingInvalidForAllocationRouteNamedAccounts(route, booking))
			{
				notification = new Notification(NotificationType.Warning, CCAValidationMessageProvider.InvalidBookingForAllocationRouteNamedAccounts(route));
				return false;
			}

			if (FreightConfigurationRegistry.Instance.EnablePlaceOfReceiptAndDeliverySupportOnAllocationRoutes.Value)
			{
				if (!CCARouteValidationHelper.IsBookingLordPortValidForPlaceOfReceipt(route, booking))
				{
					notification = new Notification(NotificationType.Error, CCAValidationMessageProvider.InvalidPlaceOfReceiptLoadPortForBooking(route, booking));
					return false;
				}

				if (!CCARouteValidationHelper.IsBookingDischargePortValidForPlaceOfDelievery(route, booking))
				{
					notification = new Notification(NotificationType.Error, CCAValidationMessageProvider.InvalidPlaceOfDeliveryDestinationForBooking(route, booking));
					return false;
				}
			}

			return true;
		}
	}
}
