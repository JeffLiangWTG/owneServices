using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business.Testing
{
	class USTTBLineAddInfoLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestProgramCodes()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			var ttbLine = invoiceLine.TTBLines.AddNew();
			AssertEquals("ttbLine.Lookups.ProgramCodes", Factory.GetCachedValue<TTBProgramCodeList>(), ttbLine.AddInfoLookups.ProgramCodes);
		}

		public void TestProcessingCodes()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			var ttbLine = invoiceLine.TTBLines.AddNew();
			ttbLine.US_ProgramCode = ZString.Empty;
			var list = ttbLine.AddInfoLookups.ProcessingCodes;
			AssertEquals(0, list.Count);
			ttbLine.US_ProgramCode = TTBProgramCodeList.Codes.Beverage;
			AssertEquals("ttbLine.Lookups.ProcessingCodes", Factory.GetCachedValue<TTBBERProcessingCodeList>(), ttbLine.AddInfoLookups.ProcessingCodes);
			ttbLine.US_ProgramCode = TTBProgramCodeList.Codes.DistilledSpirits;
			AssertEquals("ttbLine.Lookups.ProcessingCodes", Factory.GetCachedValue<TTBDSPProcessingCodeList>(), ttbLine.AddInfoLookups.ProcessingCodes);
			ttbLine.US_ProgramCode = TTBProgramCodeList.Codes.Tobacco;
			AssertEquals("ttbLine.Lookups.ProcessingCodes", Factory.GetCachedValue<TTBTOBProcessingCodeList>(), ttbLine.AddInfoLookups.ProcessingCodes);
			ttbLine.US_ProgramCode = TTBProgramCodeList.Codes.Wine;
			AssertEquals("ttbLine.Lookups.ProcessingCodes", Factory.GetCachedValue<TTBWINProcessingCodeList>(), ttbLine.AddInfoLookups.ProcessingCodes);
		}

		public void TestPermitExemptionCodes()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			var ttbLine = invoiceLine.TTBLines.AddNew();
			ttbLine.US_ProgramCode = TTBProgramCodeList.Codes.Beverage;
			var list1 = ttbLine.AddInfoLookups.PermitExemptionCodes;
			var list2 = ttbLine.AddInfoLookups.PermitExemptionCodes;
			AssertEquals("Cached List", list1, list2);
			var list = new TTBExemptionCodeList();
			AssertEquals(4, list1.Count);
			foreach (var code in new[]
			{
				TTBExemptionCodeList.Codes.TTBEX1,
				TTBExemptionCodeList.Codes.TTBEX2,
				TTBExemptionCodeList.Codes.TTBEX14,
				TTBExemptionCodeList.Codes.TTBEX15
			})
			{
				AssertEquals(code, list.GetDescriptionFromCode(code), list1.GetDescriptionFromCode(code));
			}
		}
	}
}
