using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Freight.Integration
{
	public interface IDeliveryDueDateCalculatorManager
	{
		ZDateTime Calculate(
			BusinessObjectFactory factory,
			ZDateTime readyDate,
			ZString serviceLevel,
			ZString hblDeliveryMode,
			ZString pickupOrgCode,
			ZString pickupAddressCode,
			ZString pickupCFSOrgCode,
			ZString pickupCFSAddressCode,
			ZString deliveryCFSOrgCode,
			ZString deliveryCFSAddressCode,
			ZString deliveryOrgCode,
			ZString deliveryAddressCode,
			ZString mode,
			ZString deliveryType);

		IDeliveryDueDateCalculationResult Calculate(BusinessObject bizObj);
	}
}
