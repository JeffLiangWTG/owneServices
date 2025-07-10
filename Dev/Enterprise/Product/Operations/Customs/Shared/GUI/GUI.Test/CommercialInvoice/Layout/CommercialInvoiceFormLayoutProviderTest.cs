using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.GUI.Testing
{
	sealed class CommercialInvoiceFormLayoutProviderTest : TestCaseWithFactory
	{
		public void TestGetLayoutProvider()
		{
			var decl = Factory.NewWithValidTestData<BaseJobDeclaration>();
			var invoice = decl.Invoices.AddNew();

			var provider = CommercialInvoiceFormLayoutProvider.GetLayoutProvider(null);
			AssertNull("Should be null", provider);

			provider = CommercialInvoiceFormLayoutProvider.GetLayoutProvider(invoice);
			AssertType<CommercialInvoiceFormLayoutProvider>(provider);

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Germany))
			{
				provider = CommercialInvoiceFormLayoutProvider.GetLayoutProvider(invoice);
				AssertType<CommercialInvoiceFormLayoutProvider>(provider);
			}
		}
	}
}
