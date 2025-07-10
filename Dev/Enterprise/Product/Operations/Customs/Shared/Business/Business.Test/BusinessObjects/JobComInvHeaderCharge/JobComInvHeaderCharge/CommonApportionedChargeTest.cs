using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	sealed class CommonApportionedChargeTest : TestCaseWithFactory
	{
		public void TestSetDefaultValues()
		{
			BaseJobDeclaration testDec = BaseJobDeclaration.New(Factory);
			BaseJobComInvoiceHeader invoice = testDec.Invoices.AddNew();
			BaseApportionedCharge apportionedCharge = invoice.GroupCharges.AddNew();
			AssertEquals("isApportionedCharge is set", true, apportionedCharge.J7_IsApportionedCharge);
		}

		public void TestApportionedChargesIsReadOnly()
		{
			BaseJobDeclaration testDec = BaseJobDeclaration.New(Factory);
			BaseJobComInvoiceHeader invoice = testDec.Invoices.AddNew();
			BaseInvoiceCharge charge = invoice.Charges.AddNew();
			BaseApportionedCharge apportionedCharge = invoice.GroupCharges.AddNew();

			BaseJobComInvoiceLine line = invoice.JobComInvoiceLines.AddNew();
			BaseInvoiceLineCharge lineCharge = line.Charges.AddNew();
			var lineApportionedCharge = line.ApportionedCharges.AddNew();

			AssertEquals("Invoice charge is not readonly", false, charge.ReadOnly);
			AssertEquals("Invoice Apportioned charge is not readonly", true, apportionedCharge.ReadOnly);
			AssertEquals("Invoice charge is not readonly", false, lineCharge.ReadOnly);
			AssertEquals("Invoice Apportioned charge is not readonly", true, lineApportionedCharge.ReadOnly);
		}

		[ExpectNoExceptions]
		public void TestSetHasChangesDoestCauseException()
		{
			BaseJobComInvoiceHeader invoice = Factory.New<BaseJobComInvoiceHeader>();
			CommonApportionedCharge testCharge = invoice.GroupCharges.AddNew();
			testCharge.HasChanges = true;
		}
	}
}
