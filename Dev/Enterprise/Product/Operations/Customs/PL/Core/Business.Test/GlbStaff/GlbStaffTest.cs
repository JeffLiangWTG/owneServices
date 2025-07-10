using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.PL.Business.Testing;

sealed class GlbStaffTest : TestCaseWithFactory
{
	public void TestGlbStaffCertificatesValidationType()
	{
		var glbStaff = Factory.New<GlbStaff>();
		var certificate = glbStaff.Certificates.AddNew();

		CombineAssertions(() =>
		{
			AssertType<MasterFiles.Business.GenRegCertAccredMaintListValidation>("Base type", certificate.Validation);
			certificate.XZ_RN_NKCountryOfIssuance = Core.Constants.CountryCodes.Poland;
			AssertType<GenRegCertAccredMaintListValidation>("PL type", certificate.Validation);
		});
	}
}
