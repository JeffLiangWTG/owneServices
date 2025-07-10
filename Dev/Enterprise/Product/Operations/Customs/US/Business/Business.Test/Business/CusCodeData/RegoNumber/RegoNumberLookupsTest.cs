using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class RegoNumberLookupsTest : TestCaseWithFactory
	{
		public void TestCY_CodeList()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = declaration.InvoiceLines.AddNew();
			AIILine aiiLine = invoiceLine.AIILines.AddNew();
			RegoNumber regoNumber = aiiLine.RegoNumbers.AddNew();
			AssertEquals(typeof(RegoNumberCodeList), regoNumber.Lookups.CY_CodeList.GetType());
		}

		public void TestCheckCY_Code()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EnableAII = true;
			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = declaration.InvoiceLines.AddNew();
			AIILine aiiLine = invoiceLine.AIILines.AddNew();
			RegoNumber regoNumber = aiiLine.RegoNumbers.AddNew();
			regoNumber.CY_Code = "~";
			AssertHasMessageError(regoNumber.CY_CodeInfo, ListValidation.InvalidCodeMessageError);
			regoNumber.CY_Code = RegoNumberCodeList.Codes.ChassisNumber;
			AssertNoMessageError(regoNumber.CY_CodeInfo, ListValidation.InvalidCodeMessageError);
		}
	}
}
