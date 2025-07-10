using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.US.Business.Testing;

namespace Enterprise.Customs.US.Business.LicenseManager.Testing
{
	sealed class LicenseAndLicenseExemptionTypeManagerTest : TestCaseWithFactory
	{
		public void TestGetLicenseNumberOrLicenseExemptionMessage()
		{
			USAESLicenseCodeTestHelper.CreateNewOrGetExistingCusCodeList(Factory, new ZString[] {
				USAESLicenseCode.Codes.C30, USAESLicenseCode.Codes.C32, USAESLicenseCode.Codes.OPA, USAESLicenseCode.Codes.VDS, USAESLicenseCode.Codes.VDO });

			ZString licenseNumber = "LIC123";
			AssertEquals("GetLicenseNumberOrLicenseExemptionMessage", string.Format("{0} - {1}", LicenseExemptionTypeList.Codes.NLR, LicenseExemptionTypeList.Descriptions.NLR), LicenseAndLicenseExemptionTypeManager.GetLicenseNumberOrLicenseExemptionMessage(USAESLicenseCode.Codes.C32, "", new BusinessObjectFactory(), ZDateTime.Today));
			AssertEquals("GetLicenseNumberOrLicenseExemptionMessage", licenseNumber, LicenseAndLicenseExemptionTypeManager.GetLicenseNumberOrLicenseExemptionMessage(USAESLicenseCode.Codes.C30, licenseNumber, new BusinessObjectFactory(), ZDateTime.Today));

			AssertEquals("GetLicenseNumberOrLicenseExemptionMessage", "", LicenseAndLicenseExemptionTypeManager.GetLicenseNumberOrLicenseExemptionMessage("ZZ", licenseNumber, new BusinessObjectFactory(), ZDateTime.Today));

			AssertEquals("GetLicenseNumberOrLicenseExemptionMessage", "LIC123", LicenseAndLicenseExemptionTypeManager.GetLicenseNumberOrLicenseExemptionMessage(USAESLicenseCode.Codes.OPA, "LIC123", new BusinessObjectFactory(), ZDateTime.Today));

			AssertEquals("GetLicenseNumber with existed string", "", LicenseAndLicenseExemptionTypeManager.GetLicenseNumberOrLicenseExemptionMessage(USAESLicenseCode.Codes.VDO, licenseNumber, new BusinessObjectFactory(), ZDateTime.Today));
			licenseNumber = "";
			AssertEquals("GetLicenseNumber with an empty string", "", LicenseAndLicenseExemptionTypeManager.GetLicenseNumberOrLicenseExemptionMessage(USAESLicenseCode.Codes.VDS, licenseNumber, new BusinessObjectFactory(), ZDateTime.Today));
			AssertEquals("GetLicenseNumber with an empty string", "", LicenseAndLicenseExemptionTypeManager.GetLicenseNumberOrLicenseExemptionMessage(USAESLicenseCode.Codes.VDS, licenseNumber, new BusinessObjectFactory(), ZDateTime.Invalid));
		}

		public void TestNoExceptionWithInvalidExporDate()
		{
			USAESLicenseCodeTestHelper.CreateNewOrGetExistingCusCodeList(Factory, new ZString[] { USAESLicenseCode.Codes.C30 });
			Factory.Save();
			AssertEquals("1", "", LicenseAndLicenseExemptionTypeManager.GetLicenseNumberOrLicenseExemptionMessage(USAESLicenseCode.Codes.VDS, "", new BusinessObjectFactory(), ZDateTime.Invalid));
			AssertEquals("2", "213", LicenseAndLicenseExemptionTypeManager.GetLicenseNumberOrLicenseExemptionCode(USAESLicenseCode.Codes.VDS, "213", new BusinessObjectFactory(), ZDateTime.Invalid));
			AssertEquals("3", false, LicenseAndLicenseExemptionTypeManager.IsRequiredSpaceCode(USAESLicenseCode.Codes.VDS, new BusinessObjectFactory(), ZDateTime.Invalid));
		}
	}
}
