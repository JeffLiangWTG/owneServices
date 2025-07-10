using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.TR.Business.Declaration.Testing
{
	class InvoiceLineApportionChargeLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestParent()
		{
			var parent = invoiceLine.ApportionedCharges.AddNew();
			AssertEquals(parent.Lookups.Parent, parent);
		}

		protected override void SetUp()
		{
			base.SetUp();
			invoiceLine = Factory.New<JobDeclaration>().Invoices.AddNew().InvoiceLines.AddNew();
		}

		JobComInvoiceLine invoiceLine;
	}
}
