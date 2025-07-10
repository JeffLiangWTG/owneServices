using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business.Testing
{
	internal class USAMSAddInfoLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestIntendedUseCodeList()
		{
			var invoiceLine = Declaration.InvoiceLines.AddNew();
			var amsLine = invoiceLine.AMSLines.AddNew();
			amsLine.US_Program = AMSProgramList.Codes.MO1;
			var intendedUseCodeList = amsLine.AddInfoLookups.IntendedUseCodeList;
			AssertEquals(1, intendedUseCodeList.Count);
			AssertEquals(true, intendedUseCodeList.ContainsCode(AMSIntendedUseCodesList.Codes._230000));

			amsLine.US_Program = AMSProgramList.Codes.MO2;
			intendedUseCodeList = amsLine.AddInfoLookups.IntendedUseCodeList;
			AssertEquals(1, intendedUseCodeList.Count);
			AssertEquals(true, intendedUseCodeList.ContainsCode(AMSIntendedUseCodesList.Codes._230000));

			amsLine.US_Program = AMSProgramList.Codes.MO5;
			intendedUseCodeList = amsLine.AddInfoLookups.IntendedUseCodeList;
			AssertEquals(1, intendedUseCodeList.Count);
			AssertEquals(true, intendedUseCodeList.ContainsCode(AMSIntendedUseCodesList.Codes._230000));

			amsLine.US_Program = AMSProgramList.Codes.PN1;
			intendedUseCodeList = amsLine.AddInfoLookups.IntendedUseCodeList;
			AssertEquals(5, intendedUseCodeList.Count);
			AssertEquals(true, intendedUseCodeList.ContainsCode(AMSIntendedUseCodesList.Codes._010000));
			AssertEquals(true, intendedUseCodeList.ContainsCode(AMSIntendedUseCodesList.Codes._230000));
			AssertEquals(true, intendedUseCodeList.ContainsCode(AMSIntendedUseCodesList.Codes._240000));
			AssertEquals(true, intendedUseCodeList.ContainsCode(AMSIntendedUseCodesList.Codes._250000));
			AssertEquals(true, intendedUseCodeList.ContainsCode(AMSIntendedUseCodesList.Codes._980000));

			amsLine.US_Program = AMSProgramList.Codes.MO3;
			intendedUseCodeList = amsLine.AddInfoLookups.IntendedUseCodeList;
			AssertEquals(4, intendedUseCodeList.Count);
			AssertEquals(true, intendedUseCodeList.ContainsCode(AMSIntendedUseCodesList.Codes._010000));
			AssertEquals(true, intendedUseCodeList.ContainsCode(AMSIntendedUseCodesList.Codes._240000));
			AssertEquals(true, intendedUseCodeList.ContainsCode(AMSIntendedUseCodesList.Codes._250000));
			AssertEquals(true, intendedUseCodeList.ContainsCode(AMSIntendedUseCodesList.Codes._980000));

			amsLine.US_Program = AMSProgramList.Codes.MO6;
			intendedUseCodeList = amsLine.AddInfoLookups.IntendedUseCodeList;
			AssertEquals(1, intendedUseCodeList.Count);
			AssertEquals(true, intendedUseCodeList.ContainsCode(AMSIntendedUseCodesList.Codes._250000));

			amsLine.US_Program = AMSProgramList.Codes.MO7;
			intendedUseCodeList = amsLine.AddInfoLookups.IntendedUseCodeList;
			AssertEquals(0, intendedUseCodeList.Count);

			amsLine.US_Program = AMSProgramList.Codes.MO4;
			intendedUseCodeList = amsLine.AddInfoLookups.IntendedUseCodeList;
			AssertEquals(5, intendedUseCodeList.Count);
			AssertEquals(true, intendedUseCodeList.ContainsCode(AMSIntendedUseCodesList.Codes._010000));
			AssertEquals(true, intendedUseCodeList.ContainsCode(AMSIntendedUseCodesList.Codes._230000));
			AssertEquals(true, intendedUseCodeList.ContainsCode(AMSIntendedUseCodesList.Codes._240000));
			AssertEquals(true, intendedUseCodeList.ContainsCode(AMSIntendedUseCodesList.Codes._250000));
			AssertEquals(true, intendedUseCodeList.ContainsCode(AMSIntendedUseCodesList.Codes._980000));

			amsLine.US_Program = AMSProgramList.Codes.EG1;
			intendedUseCodeList = amsLine.AddInfoLookups.IntendedUseCodeList;
			AssertEquals(6, intendedUseCodeList.Count);
			AssertEquals(true, intendedUseCodeList.ContainsCode(AMSIntendedUseCodesList.Codes._010000));
			AssertEquals(true, intendedUseCodeList.ContainsCode(AMSIntendedUseCodesList.Codes._025000));
			AssertEquals(true, intendedUseCodeList.ContainsCode(AMSIntendedUseCodesList.Codes._180000));
			AssertEquals(true, intendedUseCodeList.ContainsCode(AMSIntendedUseCodesList.Codes._210000));
			AssertEquals(true, intendedUseCodeList.ContainsCode(AMSIntendedUseCodesList.Codes._230000));
			AssertEquals(true, intendedUseCodeList.ContainsCode(AMSIntendedUseCodesList.Codes._250000));
		}

		public void TestProgramList()
		{
			var invoiceLine = Declaration.InvoiceLines.AddNew();
			invoiceLine.US_AMSInd = OGAIndicatorList.Codes.Declared;
			var amsLine = invoiceLine.AMSLines.AddNew();
			var programList = amsLine.AddInfoLookups.ProgramList;

			AssertEquals(11, programList.Count);
			AssertEquals(true, programList.ContainsCode(AMSProgramList.Codes.PN1));
			AssertEquals(true, programList.ContainsCode(AMSProgramList.Codes.EG1));
			AssertEquals(true, programList.ContainsCode(AMSProgramList.Codes.EG2));
			AssertEquals(true, programList.ContainsCode(AMSProgramList.Codes.MO1));
			AssertEquals(true, programList.ContainsCode(AMSProgramList.Codes.MO2));
			AssertEquals(true, programList.ContainsCode(AMSProgramList.Codes.MO3));
			AssertEquals(true, programList.ContainsCode(AMSProgramList.Codes.MO4));
			AssertEquals(true, programList.ContainsCode(AMSProgramList.Codes.MO5));
			AssertEquals(true, programList.ContainsCode(AMSProgramList.Codes.MO6));
			AssertEquals(true, programList.ContainsCode(AMSProgramList.Codes.MO7));
			AssertEquals(true, programList.ContainsCode(AMSProgramList.Codes.MO8));
			AssertEquals(false, programList.ContainsCode(AMSProgramList.Codes.OR1));
			AssertEquals(false, programList.ContainsCode(AMSProgramList.Codes.OR2));

			invoiceLine.US_NOPInd = OGAIndicatorList.Codes.Declared;
			programList = amsLine.AddInfoLookups.ProgramList;
			AssertEquals(13, programList.Count);
			AssertEquals(true, programList.ContainsCode(AMSProgramList.Codes.OR1));
			AssertEquals(true, programList.ContainsCode(AMSProgramList.Codes.OR2));

			invoiceLine.US_NOPInd = OGAIndicatorList.Codes.Disclaimed;
			programList = amsLine.AddInfoLookups.ProgramList;
			AssertEquals(11, programList.Count);
			AssertEquals(false, programList.ContainsCode(AMSProgramList.Codes.OR1));
			AssertEquals(false, programList.ContainsCode(AMSProgramList.Codes.OR2));
		}

		public void TestNOPProgramList()
		{
			var tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "1000000001";
			tariff.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff.UE_DateTo = ZDateTime.Today;
			tariff.UE_PGACodes = "AM7";
			tariff.UE_OGACodes = "AM7";

			var product = Factory.NewWithValidTestData<OrgSupplierPart>();
			var pivot = product.PivotsForBinding.AddNew();
			var details = pivot.Details;
			pivot.CI_TariffNum = "1000000001";

			pivot.CD_NOPIndicator = OGAIndicatorList.Codes.Declared;
			var amsLine = pivot.AMSLines.AddNew();
			var programList = amsLine.AddInfoLookups.ProgramList;
			AssertEquals(2, programList.Count);
			AssertEquals(true, programList.ContainsCode(AMSProgramList.Codes.OR1));
			AssertEquals(true, programList.ContainsCode(AMSProgramList.Codes.OR2));
			pivot.CD_NOPIndicator = OGAIndicatorList.Codes.Disclaimed;
			pivot.CD_AMSIndicator = OGAIndicatorList.Codes.Declared;

			programList = amsLine.AddInfoLookups.ProgramList;
			AssertEquals(11, programList.Count);
			AssertEquals(true, programList.ContainsCode(AMSProgramList.Codes.PN1));
			AssertEquals(true, programList.ContainsCode(AMSProgramList.Codes.EG1));
			AssertEquals(true, programList.ContainsCode(AMSProgramList.Codes.EG2));
			AssertEquals(true, programList.ContainsCode(AMSProgramList.Codes.MO1));
			AssertEquals(true, programList.ContainsCode(AMSProgramList.Codes.MO2));
			AssertEquals(true, programList.ContainsCode(AMSProgramList.Codes.MO3));
			AssertEquals(true, programList.ContainsCode(AMSProgramList.Codes.MO4));
			AssertEquals(true, programList.ContainsCode(AMSProgramList.Codes.MO5));
			AssertEquals(true, programList.ContainsCode(AMSProgramList.Codes.MO6));
			AssertEquals(true, programList.ContainsCode(AMSProgramList.Codes.MO7));
			AssertEquals(true, programList.ContainsCode(AMSProgramList.Codes.MO8));
			AssertEquals(false, programList.ContainsCode(AMSProgramList.Codes.OR1));
			AssertEquals(false, programList.ContainsCode(AMSProgramList.Codes.OR2));

			var expectedList = new USAMSAddInfoLookups(amsLine).ProgramList;
			AssertContainsExactElementsInAnyOrder(expectedList, programList);

			pivot.CD_NOPIndicator = OGAIndicatorList.Codes.Declared;
			programList = amsLine.AddInfoLookups.ProgramList;
			AssertEquals(13, programList.Count);
			AssertEquals(true, programList.ContainsCode(AMSProgramList.Codes.OR1));
			AssertEquals(true, programList.ContainsCode(AMSProgramList.Codes.OR2));

			pivot.CD_NOPIndicator = OGAIndicatorList.Codes.Disclaimed;
			programList = amsLine.AddInfoLookups.ProgramList;
			AssertEquals(11, programList.Count);
			AssertEquals(false, programList.ContainsCode(AMSProgramList.Codes.OR1));

			var expectedList2 = new USAMSAddInfoLookups(amsLine).ProgramList;
			AssertContainsExactElementsInAnyOrder(expectedList2, programList);
		}

		public void TestOrganizations()
		{
			var invoiceLine = Declaration.InvoiceLines.AddNew();
			invoiceLine.US_AMSInd = OGAIndicatorList.Codes.Declared;
			var amsLine = invoiceLine.AMSLines.AddNew();
			AssertNotNull(amsLine.AddInfoLookups.Organizations);
		}

		public void TestUnitOfMeasureList()
		{
			var invoiceLine = Declaration.InvoiceLines.AddNew();
			invoiceLine.US_AMSInd = OGAIndicatorList.Codes.Declared;
			var amsLine = invoiceLine.AMSLines.AddNew();
			AssertNotNull(amsLine.AddInfoLookups.UnitOfMeasureList);
		}

		JobDeclaration Declaration
		{
			get
			{
				if (fDeclaration == null)
				{
					fDeclaration = Factory.New<JobDeclaration>();
				}
				fDeclaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
				fDeclaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
				return fDeclaration;
			}
		}
		JobDeclaration fDeclaration;
	}
}
