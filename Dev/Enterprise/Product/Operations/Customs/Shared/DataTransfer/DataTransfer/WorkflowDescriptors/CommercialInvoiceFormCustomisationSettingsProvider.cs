using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.Business
{
	public class CommercialInvoiceFormCustomisationSettingsProvider : FormCustomisationSettingsProvider
	{
		protected override string[] GetPropertiesThatAffectWorkflow()
		{
			return new[]
			{
				BaseJobComInvoiceHeader.Schema.JZ_OH_Buyer,
				BaseJobComInvoiceHeader.Schema.JZ_OH_Supplier,
				BaseJobComInvoiceHeader.Schema.JZ_MessageType
			};
		}
	}
}
