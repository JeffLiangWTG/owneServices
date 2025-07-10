using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.ZA.Business.Testing
{
	sealed class BondedWarehousingHelperTest : TestCaseWithFactory
	{
		public void TestHasBondedWarehouseEntryDetails()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			var helper = new BondedWarehousingHelper(declaration);
			AssertEquals(false, helper.HasBondedWarehouseEntryDetails(invoiceLine, false, false, false));
			AssertEquals(false, helper.HasBondedWarehouseEntryDetails(invoiceLine, true, false, false));
			AssertEquals(false, helper.HasBondedWarehouseEntryDetails(invoiceLine, false, true, false));
			AssertEquals(false, helper.HasBondedWarehouseEntryDetails(invoiceLine, false, false, true));
			var entryLine = Factory.New<CusEntryLine>();
			invoiceLine.JI_CL = entryLine.PK;
			AssertEquals(false, helper.HasBondedWarehouseEntryDetails(invoiceLine, true, false, false));
			AssertEquals(false, helper.HasBondedWarehouseEntryDetails(invoiceLine, false, true, false));
			AssertEquals(false, helper.HasBondedWarehouseEntryDetails(invoiceLine, false, false, true));
			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.MergedLines.Add(entryLine);
			AssertEquals(true, helper.HasBondedWarehouseEntryDetails(invoiceLine, true, false, false));
			AssertEquals(false, helper.HasBondedWarehouseEntryDetails(invoiceLine, false, true, false));
			AssertEquals(false, helper.HasBondedWarehouseEntryDetails(invoiceLine, false, false, true));
			invoiceLine.JI_PreviousEntryNumber = "ENST";
			AssertEquals(true, helper.HasBondedWarehouseEntryDetails(invoiceLine, true, false, false));
			AssertEquals(true, helper.HasBondedWarehouseEntryDetails(invoiceLine, false, true, false));
			AssertEquals(true, helper.HasBondedWarehouseEntryDetails(invoiceLine, false, false, true));
			invoiceLine.JI_CL = ZGuid.Empty;
			AssertEquals(false, helper.HasBondedWarehouseEntryDetails(invoiceLine, true, false, false));
			AssertEquals(true, helper.HasBondedWarehouseEntryDetails(invoiceLine, false, true, false));
			AssertEquals(false, helper.HasBondedWarehouseEntryDetails(invoiceLine, false, false, true));
		}
	}
}
