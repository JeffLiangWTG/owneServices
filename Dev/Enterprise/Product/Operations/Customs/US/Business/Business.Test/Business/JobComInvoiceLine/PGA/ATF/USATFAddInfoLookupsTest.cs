using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.US.Business.Testing
{
	internal class USATFAddInfoLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestCategoryCodeList()
		{
			ATF.InvoiceLine.Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			AssertNotNull(ATF.AddInfoLookups.CategoryCodeList);

			var exportCount = ATF.AddInfoLookups.CategoryCodeList.Count;
			Assert(ATF.AddInfoLookups.CategoryCodeList.ContainsCode(ATFCategoryCodeList.Codes.AW));
			Assert(ATF.AddInfoLookups.CategoryCodeList.ContainsCode(ATFCategoryCodeList.Codes.DD));
			Assert(ATF.AddInfoLookups.CategoryCodeList.ContainsCode(ATFCategoryCodeList.Codes.MG));
			Assert(ATF.AddInfoLookups.CategoryCodeList.ContainsCode(ATFCategoryCodeList.Codes.SI));
			Assert(ATF.AddInfoLookups.CategoryCodeList.ContainsCode(ATFCategoryCodeList.Codes.SR));
			Assert(ATF.AddInfoLookups.CategoryCodeList.ContainsCode(ATFCategoryCodeList.Codes.SS));

			ATF.InvoiceLine.Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertNotNull(ATF.AddInfoLookups.CategoryCodeList);
			Assert(ATF.AddInfoLookups.CategoryCodeList.Count > exportCount);
		}

		public void TestExemptionCodesList()
		{
			AssertNotNull(ATF.AddInfoLookups.ExemptionCodesList);
		}

		ATF ATF
		{
			get
			{
				if (atf == null)
				{
					var declaration = Factory.New<JobDeclaration>();
					declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
					var invoiceHeader = declaration.Invoices.AddNew();
					var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
					atf = invoiceLine.ATFLines.AddNew();
				}
				return atf;
			}
		}
		ATF atf;
	}
}
