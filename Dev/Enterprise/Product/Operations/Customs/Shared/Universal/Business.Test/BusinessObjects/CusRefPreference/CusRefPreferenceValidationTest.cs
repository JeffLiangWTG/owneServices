using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.Universal.Testing
{
	class CusRefPreferenceValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckCR8_RN_NKCountryCode()
		{
			var info = refPreference.CR8_RN_NKCountryCodeInfo;
			refPreference.CR8_RN_NKCountryCode = ZString.Empty;
			AssertMandatoryValidationError(info, true);
			AssertListValidationInvalidCodeError(info, false);
			refPreference.CR8_RN_NKCountryCode = "XX";
			AssertMandatoryValidationError(info, false);
			AssertListValidationInvalidCodeError(info, true);
			refPreference.CR8_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			AssertNoErrors(info);
		}

		public void TestCheckCR8_Description()
		{
			var info = refPreference.CR8_DescriptionInfo;
			refPreference.CR8_Description = ZString.Empty;
			AssertMandatoryValidationError(info, true);
			refPreference.CR8_Description = "Description";
			AssertNoErrors(info);
		}

		public void TestCheckCR8_Preference()
		{
			var info = refPreference.CR8_PreferenceInfo;
			refPreference.CR8_Preference = ZString.Empty;
			AssertMandatoryValidationError(info, true);
			refPreference.CR8_Preference = "PRE001";
			AssertNoErrors(info);
		}

		public void TestCheckNotDuplicated()
		{
			var existingBo = Factory.New<CusRefPreference>();
			existingBo.CR8_RN_NKCountryCode = "CN";
			existingBo.CR8_Preference = "PRE001";
			var newBo = Factory.New<CusRefPreference>();
			newBo.CR8_RN_NKCountryCode = "CN";
			newBo.CR8_Preference = "PRE001";
			var targetInfo = newBo.CR8_PreferenceInfo;
			var messageText = @"There is already a Preference with the same Country/Region and Code.
The combination of Country and Preference must be unique.";
			AssertHasError(targetInfo, messageText);
			newBo.CR8_Preference = "PRE002";
			AssertNoError(targetInfo, messageText);
			newBo.CR8_Preference = "PRE001";
			AssertHasError(targetInfo, messageText);
			newBo.CR8_RN_NKCountryCode = "US";
			newBo.CR8_Preference = "";
			newBo.CR8_Preference = "PRE001";
			AssertNoError(targetInfo, messageText);
		}

		protected override void SetUp()
		{
			base.SetUp();
			refPreference = Factory.New<CusRefPreference>();
		}

		CusRefPreference refPreference;
	}
}
