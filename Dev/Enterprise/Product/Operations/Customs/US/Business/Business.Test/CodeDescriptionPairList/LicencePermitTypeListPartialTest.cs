using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class LicencePermitTypeListTest : TestCaseWithFactory
	{
		public void TestIsCottonOrganicExemption()
		{
			USAESLicenseCodeTestHelper.CreateNewOrGetExistingCusCodeList(Factory, new ZString[] {
				USAESLicenseCode.Codes.C30, USAESLicenseCode.Codes.C31,USAESLicenseCode.Codes.C60, USAESLicenseCode.Codes.SAG, USAESLicenseCode.Codes.SAU,
				USAESLicenseCode.Codes.SCA, USAESLicenseCode.Codes.SGB, USAESLicenseCode.Codes.S00, USAESLicenseCode.Codes.S05, USAESLicenseCode.Codes.S61,
				USAESLicenseCode.Codes.S73, USAESLicenseCode.Codes.S85, USAESLicenseCode.Codes.S94, USAESLicenseCode.Codes.T10, USAESLicenseCode.Codes.VDS });

			var excepmt = new ZString[]
				{
					LicencePermitTypeList.Codes._12,
					LicencePermitTypeList.Codes._22,
					LicencePermitTypeList.Codes._23
				};

			var list = new USAESLicenseCodeCollection(Factory);
			list.Load();
			foreach (ZString code in excepmt)
			{
				AssertEquals(code, true, LicencePermitTypeList.DeclareInCottonOrganicExemptionFieldInACS(code));
			}

			foreach (ICodeDescription code in list)
			{
				if (!excepmt.Contains(code.Code))
				{
					AssertEquals(code.Code, false, LicencePermitTypeList.DeclareInCottonOrganicExemptionFieldInACS(code.Code));
				}
			}
		}
	}
}
