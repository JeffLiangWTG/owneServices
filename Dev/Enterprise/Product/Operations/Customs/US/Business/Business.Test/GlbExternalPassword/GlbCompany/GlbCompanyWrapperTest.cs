using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(GlbCompanyWrapper))]
	sealed class GlbCompanyWrapperTest : MasterFiles.Business.Testing.GlbCompanyWrapperTest<GlbCompanyWrapper>
	{
		[TestDate(2018, 10, 01)]
		public void TestIsValidWrapper()
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedStates;
			var wrapper = GlbCompanyWrapper.GetWrapper<MasterFiles.Business.GlbCompanyWrapper>(company);
			AssertType<GlbCompanyWrapper>(wrapper);

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedStates))
			{
				AssertEquals("Should be true as the country code is US.", true, wrapper.IsValidWrapper);
			}
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.PuertoRico))
			{
				AssertEquals("Should be false as the country code is not equal to the current country code.", false, wrapper.IsValidWrapper);
				company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.PuertoRico;
				AssertEquals("Should be true as the country code is PR.", true, wrapper.IsValidWrapper);
			}
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			{
				company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
				AssertEquals("Should be false as the wrapper is only valid for US or PR.", false, wrapper.IsValidWrapper);
			}
		}
	}
}
