using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.PL.Business.Testing;

sealed class GlbStaffCertificatesValidationProviderTest : TestCaseWithFactory
{
	public void TestGetValidation()
	{
		AssertType<GenRegCertAccredMaintListValidation>(new GlbStaffCertificatesValidationProvider().GetValidation(Factory.New<GenRegCertAccredMaintList>()));
	}
}
