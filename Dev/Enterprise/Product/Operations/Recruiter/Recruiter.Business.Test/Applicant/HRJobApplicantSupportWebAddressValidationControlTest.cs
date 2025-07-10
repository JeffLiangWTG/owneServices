using Enterprise.MasterFiles.Business.Testing;

namespace Enterprise.Recruiter.Business.Testing
{
	sealed class HRJobApplicantSupportWebAddressValidationControlTest : SupportWebAddressValidationTest<HRJobApplicant>
	{
		protected override HRJobApplicant GetBOToTest()
		{
			var applicant = Factory.NewWithValidTestData<HRJobApplicant>();
			applicant.HA_RN_NKCountry = "AU";
			return applicant;
		}

		protected override void AssertCityValidationResultWhenCityIsEmpty(HRJobApplicant testBO)
		{
			Assert(!testBO.CityInfo.HasNotifications());
		}
	}
}
