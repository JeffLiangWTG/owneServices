using CargoWise.ComponentModel;
using Enterprise.ContractManagement.Business;
using Enterprise.Freight.Integration;
using static Enterprise.Integration.Forwarding;

namespace Enterprise.Freight.Forwarding.Business
{
	public sealed class CCAContractConsolAssignmentValidator
	{
		public bool IsAllowedToAllocateToContract(
			IForwardingConsol forwardingConsol,
			IRatingContract ratingContract,
			out Notification notification)
		{
			notification = null;

			if (forwardingConsol is not ForwardingConsol consol || ratingContract is not RatingContract contract)
			{
				return true;
			}

			if (!CCAContractValidationHelper.DoesJobTransportModeMatchContract(contract, consol))
			{
				notification = new Notification(NotificationType.Error, CCAValidationMessageProvider.InvalidTransportMode(contract, consol));
				return false;
			}

			if (!CCAContractValidationHelper.IsConsolValidForContractValidDateRanges(contract, consol))
			{
				notification = new Notification(NotificationType.Error, CCAValidationMessageProvider.InvalidConsolDatesForContract(contract));
				return false;
			}

			if (CCAContractValidationHelper.AreAnyContainerTypesInvalidForContract(contract, consol))
			{
				notification = new Notification(NotificationType.Error, CCAValidationMessageProvider.ContainerTypesInvalid(contract, consol));
				return false;
			}

			if (CCAContractValidationHelper.IsConsolInvalidForNonHazardousContract(contract, consol))
			{
				notification = new Notification(NotificationType.Error, CCAValidationMessageProvider.InvalidConsolForNonHazardousContract(contract, consol));
				return false;
			}

			if (CCAContractValidationHelper.AreAnyContainerCommoditiesInvalidForNonHazardousContract(contract, consol))
			{
				notification = new Notification(NotificationType.Error, CCAValidationMessageProvider.HazardousContainerCommoditiesInvalid(contract, consol));
				return false;
			}

			if (CCAContractValidationHelper.IsConsolInvalidForContractNamedAccounts(contract, consol))
			{
				notification = new Notification(NotificationType.Warning, CCAValidationMessageProvider.InvalidConsolForContractNamedAccounts(contract));
				return false;
			}

			return true;
		}
	}
}
