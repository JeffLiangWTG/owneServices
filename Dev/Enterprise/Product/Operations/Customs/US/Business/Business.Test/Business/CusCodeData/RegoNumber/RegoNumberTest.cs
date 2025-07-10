using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(RegoNumber))]
	sealed class RegoNumberTest : Customs.Business.Testing.CusCodeDataTest<RegoNumber>
	{
		public void TestSetDefaultValues()
		{
			RegoNumber number = Factory.New<RegoNumber>();
			AssertEquals(CusCodeDataTypeList.Codes.RegoNumber, number.CY_Type);
		}

		public void TestParent()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			JobComInvoiceLine line = declaration.InvoiceLines.AddNew();
			AIILine aiiLine = line.AIILines.AddNew();
			RegoNumber number = aiiLine.RegoNumbers.AddNew();
			AssertEquals(aiiLine, number.Parent);
		}

		public void TestLookupsAndValidation()
		{
			RegoNumber number = Factory.New<RegoNumber>();
			AssertEquals(typeof(RegoNumberLookups), number.Lookups.GetType());
			AssertEquals(typeof(RegoNumberValidation), number.Validation.GetType());
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var declaration = factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EnableAII = true;
			declaration.US_IsInvoiceByRequest = true;
			var invoice = declaration.Invoices.AddNew();
			invoice.US_IsLineGrouping = true;
			var line = declaration.InvoiceLines.AddNew();
			var range = line.LineGroupingRanges.AddNew(1, 2);
			var aiiLine = line.AIILines.AddNew(range);
			var regoNumber = aiiLine.RegoNumbers.AddNew();
			regoNumber.CY_Data = "KD32";
			return regoNumber;
		}
	}
}
