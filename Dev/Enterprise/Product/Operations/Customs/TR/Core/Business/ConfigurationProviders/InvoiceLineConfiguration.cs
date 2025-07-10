using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.TR.Business
{
	public class InvoiceLineConfiguration : EU.Business.InvoiceLineConfiguration
	{
		protected override ZBool VehicleSupportCore(BusinessObject businessObject) => true;

		protected override ZBool InvoiceLinePaymentSupportCore(BusinessObject businessObject) => true;

		protected override ZBool AdditionalSupplyChainActorSupportCore(BusinessObject businessObject) => false;
	}
}
