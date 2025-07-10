using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.TR.Business
{
	public class DeclarationConfiguration : EU.Business.DeclarationConfiguration
	{
		protected override ZBool UseUniversalFeeCalculationCore(BusinessObject businessObject) => true;

		protected override EU.Business.EntryLineConfiguration GetNewEntryLineConfiguration() => new EntryLineConfiguration();

		protected override EU.Business.InvoiceHeaderConfiguration GetNewInvoiceHeaderConfiguration() => new InvoiceHeaderConfiguration();

		protected override EU.Business.InvoiceLineConfiguration GetNewInvoiceLineConfiguration() => new InvoiceLineConfiguration();

		protected override ZBool DV1DetailsSupportCore(BusinessObject businessObject) => true;
	}
}
