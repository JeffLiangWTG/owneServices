using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.NO.Business.Testing;

[TestedType(typeof(GlbCompanyWrapper))]
sealed class GlbCompanyWrapperTest : MasterFiles.Business.Testing.GlbCompanyWrapperTest<GlbCompanyWrapper>
{
	public void TestCredentialType() => AssertType<GlbExternalPassword_NOD>(wrapper.Credential);

	public void TestIsValidWrapper()
	{
		Assert("Should be true as the country code is NO.", wrapper.IsValidWrapper);
	}

	public void TestGetCachedValue()
	{
		var wrapperdup = GlbCompanyWrapper.GetWrapper<GlbCompanyWrapper>(company);
		AssertSame(wrapper, wrapperdup);
	}

	protected override void SetUp()
	{
		base.SetUp();
		company = Factory.NewWithValidTestData<GlbCompany>();
		company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Norway;

		wrapper = GetWrapper(company);
	}

	GlbCompany company;
	GlbCompanyWrapper wrapper;
}
