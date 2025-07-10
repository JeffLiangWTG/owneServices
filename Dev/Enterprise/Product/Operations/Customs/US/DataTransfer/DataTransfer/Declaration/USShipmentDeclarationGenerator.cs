
using Enterprise.Customs.Business;
using Enterprise.Customs.DataTransfer;

namespace Enterprise.Customs.US.DataTransfer
{
	public class USShipmentDeclarationGenerator : ShipmentDeclarationGenerator, Integration.Customs.US.IUSShipmentDeclarationGenerator
	{
		protected override InvoicesGeneratorFromXSD GetNewInvoiceGenerator(BaseJobDeclaration declaration)
		{
			return new USInvoicesGeneratorFromXSD(declaration);
		}
	}
}
