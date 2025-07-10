using CargoWise.ComponentModel;
using Enterprise.ContractManagement.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Integration;
using Enterprise.Freight.Integration.QuotedBooking;

namespace Enterprise.Freight.QuotedBookings.Business
{
	public sealed class CCAContractBookingAssignmentValidator
	{
		public bool IsAllowedToAllocateToContract(
			IQuotedBooking quotedBooking,
			IRatingContract ratingContract,
			out Notification notification)
		{
			notification = null;
			if (quotedBooking is not QuotedBooking booking ||
				ratingContract is not RatingContract contract)
			{
				return true;
			}

			if (CCAContractValidationHelper.IsETDBeforeContractStart(contract, booking))
			{
				notification = new Notification(NotificationType.Error, CCAValidationMessageProvider.ContractStartDateViolated(contract, booking));
				return false;
			}
			else if (CCAContractValidationHelper.IsETDAfterContractExpiry(contract, booking))
			{
				notification = new Notification(NotificationType.Error, CCAValidationMessageProvider.ContractExpiryDateViolated(contract, booking));
				return false;
			}

			if (CCAContractValidationHelper.AreAnyContainerTypesInvalidForContract(contract, booking))
			{
				notification = new Notification(NotificationType.Error, CCAValidationMessageProvider.ContainerTypesInvalid(contract, booking));
				return false;
			}

			if (CCAContractValidationHelper.AreAnyContainerCommoditiesInvalidForNonHazardousContract(contract, booking))
			{
				notification = new Notification(NotificationType.Error, CCAValidationMessageProvider.HazardousContainerCommoditiesInvalid(contract, booking));
				return false;
			}

			if (!CCAContractValidationHelper.IsBookingValidForContractNamedAccounts(contract, booking))
			{
				notification = new Notification(NotificationType.Warning, CCAValidationMessageProvider.InvalidBookingForContractNamedAccounts(contract));
				return false;
			}

			if (!CCAContractValidationHelper.DoesJobTransportModeMatchContract(contract, booking))
			{
				notification = new Notification(NotificationType.Error, CCAValidationMessageProvider.InvalidBookingTransportMode(contract, booking));
				return false;
			}

			return true;
		}
	}
}
