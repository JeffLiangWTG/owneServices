using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.Business.Testing
{
	sealed class CusPackableItemLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestPackTypes()
		{
			var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			Factory.Save();
			var cusPackingList = declaration.LoadOrCreateCusPackingList(Factory);
			Factory.Save();
			var cusPackage = cusPackingList.PackageJob.Packages.AddNew();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			var cusPackableItem = Factory.New<CusPackableItem>();
			cusPackableItem.CUI_JI = invoiceLine.PK;

			var lookups = new CusPackableItemLookups(cusPackableItem);
			AssertSame("PackTypes same as in the invoiceLine's CustomsUQList", invoiceLine.Lookups.CustomsUQList, lookups.PackTypes);

			cusPackableItem.CUI_JI = ZGuid.Empty;
			AssertSame("The defalut PackTypes should be the Standard Units List when the PackableItem isn't linked to an InvoiceLine.", RefPackTypeCollection.GetAsCodeDescriptionPairWithStandardUnits(Factory), lookups.PackTypes);
		}
	}
}
