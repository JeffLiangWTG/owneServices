namespace Enterprise.Customs.NZ.Business.Declaration.Testing
{
	using CargoWise.EntityFramework.Testing;
	using Enterprise.Customs.NZ.Business.Declaration;

	public class TypeSafeJobComInvoiceLineTest : TestCaseWithFactory
	{
		public void TestCusEntryLine()
		{
			var entry = declaration.CustomsEntryHeaders.AddNew();
			var entryline = entry.MergedLines.AddNew();
			invoiceLine.JI_CL = entryline.PK;
			AssertEquals(typeof(CusEntryLine), invoiceLine.CusEntryLine.GetType());
		}

		protected JobDeclaration declaration;
		protected JobComInvoiceHeader invoice;
		protected JobComInvoiceLine invoiceLine;
		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			invoice = declaration.Invoices.AddNew();
			invoiceLine = invoice.JobComInvoiceLines.AddNew();
		}
	}
}
