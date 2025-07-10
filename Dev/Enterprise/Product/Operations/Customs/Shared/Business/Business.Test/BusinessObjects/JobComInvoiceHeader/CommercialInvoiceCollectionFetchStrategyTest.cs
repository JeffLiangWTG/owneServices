using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business.FetchStrategies.Testing
{
	sealed class CommercialInvoiceCollectionFetchStrategyTest : TestCaseWithFactory
	{
		public void TestFetchForViewActiveTableFetchHints()
		{
			var declaration = BaseJobDeclaration.New(Factory);
			var invoice1 = declaration.Invoices.AddNew();
			invoice1.Charges.AddNew();
			var line1 = invoice1.JobComInvoiceLines.AddNew();
			line1.Charges.AddNew();

			var declaration2 = BaseJobDeclaration.New(Factory);
			var invoice2 = declaration2.Invoices.AddNew();
			invoice2.Charges.AddNew();
			var line2 = invoice2.JobComInvoiceLines.AddNew();
			line2.Charges.AddNew();
			Factory.Save();

			var factory = new BusinessObjectFactory();
			var collection = new CommercialInvoiceCollection(factory);
			AssertEquals(0, factory.ActiveFetchHintsForTable(JobComInvHeaderChargeSchema.Constants.TableName));

			var strategy = new CommercialInvoiceCollectionFetchStrategy(collection);
			var tc1 = new TableColumn(JobComInvoiceHeaderSchema.Constants.TableName, BaseJobComInvoiceHeader.Schema.JZ_Calc_TNI);
			strategy.FetchForView(collection.ToArray(), new[] { tc1 });
			AssertEquals(3, factory.ActiveFetchHintsForTable(JobComInvHeaderChargeSchema.Constants.TableName));
		}
	}
}
