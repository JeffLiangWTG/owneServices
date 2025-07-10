using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(CusLiquidation))]
	sealed class CusLiquidationTest : EnterpriseBusinessObjectTestCase
	{
		public void TestImporterOfRecordNumberForDisplay()
		{
			CusLiquidation liquidation = Factory.New<CusLiquidation>();
			liquidation.B8_ImportOfRecordNo = "123-45-6789";

			Env.Security.OrgDetailsViewPersonalInformation.IsAllowed = false;
			AssertEquals("***-**-****", liquidation.ImporterOfRecordNumberForDisplay);

			Env.Security.OrgDetailsViewPersonalInformation.IsAllowed = true;
			AssertEquals("123-45-6789", liquidation.ImporterOfRecordNumberForDisplay);

			liquidation.B8_ImportOfRecordNo = "12345-6789";
			Env.Security.OrgDetailsViewPersonalInformation.IsAllowed = false;
			AssertEquals("12345-6789", liquidation.ImporterOfRecordNumberForDisplay);
		}

		[TestDate(2021, 06, 28)]
		public void TestImporterNameAndCreateDate()
		{
			CusLiquidation liquidation = Factory.New<CusLiquidation>();
			AssertEquals(GlbCompany.CurrentCompany.PK, liquidation.B8_GC);
			AssertEquals(new ZDateTime(2021, 06, 28), liquidation.B8_SystemCreateDate);
			OrgHeader org = Factory.New<OrgHeader>();
			org.OH_FullName = "Test Org For Liquidation";
			org.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.EmployerIdentificationNumber, "91-013199000");
			liquidation.B8_ImportOfRecordNo = "91-013199000";
			AssertEquals(liquidation.ImporterName, "Test Org For Liquidation");
		}
	}
}
