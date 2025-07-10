#if DEBUG
using CargoWise.EntityFramework;
using Enterprise.Customs._CustomsTemplate_.Business;

namespace Enterprise.Customs._CustomsTemplate_.DocumentWrappers
{
	public class DocumentWrapperProvider : Integration.Customs._CustomsTemplate_.IDocumentWrapperProvider
	{
		public Integration.Customs._CustomsTemplate_.IDocDeclaration NewDocDeclaration(Integration.Customs._CustomsTemplate_.IJobDeclaration declaration, BusinessObjectFactory factoryToWrap) => DocDeclaration.New((JobDeclaration)declaration, factoryToWrap);

		public Integration.Customs._CustomsTemplate_.IDocJobComInvoiceLine NewDocJobComInvoiceLine(Integration.Customs._CustomsTemplate_.IJobComInvoiceLine invoiceLine, BusinessObjectFactory factoryToWrap) => DocJobComInvoiceLine.New((JobComInvoiceLine)invoiceLine, factoryToWrap);
	}
}
#endif
