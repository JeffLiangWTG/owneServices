using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.Integration.Freight;

namespace Enterprise.Freight.Agency.Business
{
	public class AgencyRegistryWrapper : IAgencyRegistry
	{
		bool IAgencyRegistry.PostBothPrepaidAndCollectShipmentRevenueCharges
		{
			get { return AgencyRegistry.Instance.PostAllShipmentRevenueCharges.Value; }
		}

		bool IAgencyRegistry.PostBothPrepaidAndCollectShipmentCostCharges
		{
			get { return AgencyRegistry.Instance.PostAllShipmentCostCharges.Value; }
		}

		bool IAgencyRegistry.ElectronicBookingAndShippingInstructions
		{
			get { return AgencyRegistry.Instance.ElectronicBookingAndShippingInstructions.Value; }
		}

		bool IAgencyRegistry.IsDeliveryAgentAllowedSendingBookingConfirmationEDIAfterATD(ZGuid principal)
		{
			return AllowSendingBookingConfirmationRegistryHelper.IsAllowed(principal);
		}

		IRegistryItem IAgencyRegistry.UpdateEmptyReturnByWhenAvailabilityDatesChange
		{
			get { return AgencyRegistry.Instance.UpdateEmptyReturnByWhenAvailabilityDatesChange; }
		}

		public ICodeDescriptionPairList ContainerCleanCodes
		{
			get { return AgencyRegistry.Instance.ContainerCleanCodes.Value; }
		}

		public ICodeDescriptionPairList ContainerDamageCodes
		{
			get { return AgencyRegistry.Instance.ContainerDamageCodes.Value; }
		}
	}
}


