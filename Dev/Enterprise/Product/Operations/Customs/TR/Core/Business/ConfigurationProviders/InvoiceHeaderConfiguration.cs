using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.TR.Business.Declaration;

namespace Enterprise.Customs.TR.Business
{
	public class InvoiceHeaderConfiguration : EU.Business.InvoiceHeaderConfiguration
	{
		protected override ZBool InvoicePaymentSupportCore(BusinessObject businessObject)
		{
			var result = false;
			if (businessObject is JobDeclaration declaration)
			{
				result = declaration?.IsImport ?? false;
			}
			return result;
		}
	}
}
