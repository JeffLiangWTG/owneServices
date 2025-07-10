using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	[TestedType(typeof(WhsStocktakeInvoicingSupporter))]
	public class WhsStocktakeInvoicingSupporterTest : WhsJobInvoicingSupporterTest<WhsStocktakeInvoicingSupporter, WhsStocktake>
	{
		protected override IJobInvoicingPlugIn GetNewBusinessObject()
		{
			WhsStocktake whsStocktake = Factory.NewWithValidTestData<WhsStocktake>();
			return whsStocktake;
		}

		protected override WhsStocktakeInvoicingSupporter GetNewSupporter(WhsStocktake parent)
		{
			return new WhsStocktakeInvoicingSupporter(parent);
		}
	}
}
