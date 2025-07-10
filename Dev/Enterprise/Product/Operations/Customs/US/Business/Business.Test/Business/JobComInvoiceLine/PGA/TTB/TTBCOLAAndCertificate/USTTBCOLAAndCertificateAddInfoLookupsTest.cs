using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.US.Business.Testing
{
	class USTTBCOLAAndCertificateAddInfoLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestCOLAExemptionCodesPermitExemptionCodes()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			var ttbLine = invoiceLine.TTBLines.AddNew();
			ttbLine.US_ProgramCode = TTBProgramCodeList.Codes.Beverage;
			var cola = ttbLine.COLAAndCertificates.AddNew();
			var list1 = cola.AddInfoLookups.COLAExemptionCodes;
			var list2 = cola.AddInfoLookups.COLAExemptionCodes;
			AssertEquals("Cached List", list1, list2);
			var list = new TTBExemptionCodeList();
			AssertEquals(5, list1.Count);
			foreach (var code in new[]
			{
				TTBExemptionCodeList.Codes.TTBEX2,
				TTBExemptionCodeList.Codes.TTBEX7,
				TTBExemptionCodeList.Codes.TTBEX8,
				TTBExemptionCodeList.Codes.TTBEX11,
				TTBExemptionCodeList.Codes.TTBEX12
			})
			{
				AssertEquals(code, list.GetDescriptionFromCode(code), list1.GetDescriptionFromCode(code));
			}
		}
	}
}
