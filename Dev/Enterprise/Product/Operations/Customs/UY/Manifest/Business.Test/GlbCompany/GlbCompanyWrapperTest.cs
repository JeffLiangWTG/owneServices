using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.UY.Manifest.Business.Testing
{
	[TestedType(typeof(GlbCompanyWrapper))]
	public class GlbCompanyWrapperTest : MasterFiles.Business.Testing.GlbCompanyWrapperTest<GlbCompanyWrapper>
	{
		public void TestIsValidWrapper()
		{
			var company1 = Factory.NewWithValidTestData<GlbCompany>();
			company1.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Uruguay;

			var wrapper1 = GetWrapper(company1);
			Assert("Should be true as the country code is UY.", wrapper1.IsValidWrapper);
		}
	}
}
