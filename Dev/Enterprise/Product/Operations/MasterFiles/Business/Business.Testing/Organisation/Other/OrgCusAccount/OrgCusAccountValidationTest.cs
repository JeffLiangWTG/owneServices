using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class OrgCusAccountValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckDecryptedPassword()
		{
			var orgCusAccount = Factory.NewWithValidTestData<OrgCusAccount>();
			orgCusAccount.CZ_RN_NKCountryCode = Core.Constants.CountryCodes.Eritrea;
			AssertNoErrors(orgCusAccount.DecryptedPasswordInfo);

			orgCusAccount.DecryptedPassword = ZString.Empty;
			AssertHasErrorContaining(orgCusAccount.DecryptedPasswordInfo, MandatoryValidation.MustBeEntered);

			orgCusAccount.DecryptedPassword = "XXX";
			AssertNoMessageErrors(orgCusAccount.DecryptedPasswordInfo);
		}

		public void TestCheckCZ_Account()
		{
			var orgCusAccount = Factory.New<OrgCusAccount>();
			orgCusAccount.Validation.ValidateCZ_Account();
			AssertHasErrorContaining(orgCusAccount.CZ_AccountInfo, MandatoryValidation.MustBeEntered);

			orgCusAccount.CZ_Account = "XXX";
			AssertNoErrorContaining(orgCusAccount.CZ_AccountInfo, MandatoryValidation.MustBeEntered);
		}

		public void TestCheckCZ_Code()
		{
			var orgCusAccount = Factory.NewWithValidTestData<OrgCusAccount>();
			AssertNoErrors(orgCusAccount.CZ_CodeInfo);

			orgCusAccount.CZ_Code = ZString.Empty;
			AssertHasErrorContaining(orgCusAccount.CZ_CodeInfo, MandatoryValidation.MustBeEntered);
		}
	}
}
