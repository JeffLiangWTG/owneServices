using CargoWise.Types;

namespace Enterprise.Freight.LocalCartage.Integration
{
	public interface ICommonCartageLeg
	{
		ZString MessageStatus { get; }
		ZDateTime JU_PickupTimeIn { get; set; }
		ZDateTime JU_PickupTimeOut { get; set; }
		ZDateTime JU_WaitPointTimeIn { get; set; }
		ZDateTime JU_WaitPointTimeOut { get; set; }
		ZDateTime JU_DeliverTimeIn { get; set; }
		ZDateTime JU_DeliverTimeOut { get; set; }
		ZInt JU_DisplayOrder { get; }

		ZGuid JU_E2PickupAddressID { get; }
		ZGuid JU_E2DeliveryAddressID { get; }

		ZInt TotalPackages { get; }
		ZString TotalPackagesUnit { get; }
		ZString ContainerDescription { get; }
	}
}
