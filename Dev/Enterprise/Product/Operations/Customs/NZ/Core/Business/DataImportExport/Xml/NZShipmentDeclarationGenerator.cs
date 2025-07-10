
using Enterprise.Customs.Business;
using Enterprise.Customs.DataTransfer;

namespace Enterprise.Customs.NZ.Business.Data
{
	public class NZShipmentDeclarationGenerator : ShipmentDeclarationGenerator, Integration.Customs.NZ.INZShipmentDeclarationGenerator
	{
		protected override void SetDefaultValues(BaseJobDeclaration declaration)
		{
			declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
		}

		protected override InvoicesGeneratorFromXSD GetNewInvoiceGenerator(BaseJobDeclaration declaration)
		{
			return new NZInvoicesGeneratorFromXSD(declaration);
		}
	}
}
