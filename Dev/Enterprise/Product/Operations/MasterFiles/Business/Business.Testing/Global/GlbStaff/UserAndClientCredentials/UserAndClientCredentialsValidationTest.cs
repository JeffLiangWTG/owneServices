using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business.Testing
{
	public class UserAndClientCredentialsValidationTest : BusinessObjectValidationTestCase
	{
		public void TestPasswordConfirmation()
		{
			var branch = CreateIndiaBranch();

			branch.BranchCredentialsIndia.Password = ZString.Empty;
			branch.BranchCredentialsIndia.PasswordConfirmation = ZString.Empty;
			branch.BranchCredentialsIndia.Validation.ValidateAll();
			AssertNoError(branch.BranchCredentialsIndia.PasswordInfo, "Password and confirmation password do not match");
			AssertNoError(branch.BranchCredentialsIndia.PasswordConfirmationInfo, "Password and confirmation password do not match");

			branch.BranchCredentialsIndia.Password = "pass";
			AssertNoError(branch.BranchCredentialsIndia.PasswordInfo, "Password and confirmation password do not match");
			AssertHasError(branch.BranchCredentialsIndia.PasswordConfirmationInfo, "Password and confirmation password do not match");

			branch.BranchCredentialsIndia.PasswordConfirmation = "word";
			AssertNoError(branch.BranchCredentialsIndia.PasswordInfo, "Password and confirmation password do not match");
			AssertHasError(branch.BranchCredentialsIndia.PasswordConfirmationInfo, "Password and confirmation password do not match");

			branch.BranchCredentialsIndia.Password = "secret";
			AssertNoError(branch.BranchCredentialsIndia.PasswordInfo, "Password and confirmation password do not match");
			AssertHasError(branch.BranchCredentialsIndia.PasswordConfirmationInfo, "Password and confirmation password do not match");

			branch.BranchCredentialsIndia.PasswordConfirmation = "secret";
			AssertNoError(branch.BranchCredentialsIndia.PasswordInfo, "Password and confirmation password do not match");
			AssertNoError(branch.BranchCredentialsIndia.PasswordConfirmationInfo, "Password and confirmation password do not match");
		}

		public void TestStatusReasonValidation()
		{
			// Note that password status reason properties are readonly, and are expected to be set via a service task on GlbExternalPassword.
			var taxpayerBranchWithError = CreateIndiaBranch(prefix: "I1");
			taxpayerBranchWithError.WithExternalPassword<GlbBranchExternalPasswordINT>(userId: "taxpayer.user", passwordStatus: PasswordStatusList.Codes.Invalid, "error details for INT");
			var taxpayerBranchWithoutError = CreateIndiaBranch(prefix: "I2");
			var serviceProviderBranchWithError = CreateIndiaBranch(prefix: "I3");
			serviceProviderBranchWithError.WithExternalPassword<GlbBranchExternalPasswordINS>(userId: "service.provider.id", passwordStatus: PasswordStatusList.Codes.Invalid, "error details for INS");
			var serviceProviderBranchWithoutError = CreateIndiaBranch(prefix: "I4");
			Factory.Save();

			AssertHasWarning(taxpayerBranchWithError.BranchCredentialsIndia.UserCredentialPasswordStatusReasonInfo, "Please correct the error and re-enter your Password");
			AssertNoWarning(taxpayerBranchWithoutError.BranchCredentialsIndia.UserCredentialPasswordStatusReasonInfo, "Please correct the error and re-enter your Password");

			AssertHasWarning(serviceProviderBranchWithError.BranchCredentialsIndia.ClientCredentialPasswordStatusReasonInfo, "Please correct the error and re-enter your Client Secret");
			AssertNoWarning(serviceProviderBranchWithoutError.BranchCredentialsIndia.ClientCredentialPasswordStatusReasonInfo, "Please correct the error and re-enter your Client Secret");
		}

		public void TestMustHaveTaxpayerCredentialToHaveServiceProviderCredentialValidation()
		{
			var branch = CreateIndiaBranch();

			branch.BranchCredentialsIndia.Validation.ValidateAll();
			AssertNoError(branch.BranchCredentialsIndia.UsernameInfo, "User Id is required when Client Id is entered");

			branch.BranchCredentialsIndia.ClientId = "client";
			branch.BranchCredentialsIndia.ClientSecret = "secret";
			AssertHasError(branch.BranchCredentialsIndia.UsernameInfo, "User Id is required when Client Id is entered");

			branch.BranchCredentialsIndia.Username = "user";
			branch.BranchCredentialsIndia.Password = "pass";
			branch.BranchCredentialsIndia.PasswordConfirmation = "pass";
			AssertNoError(branch.BranchCredentialsIndia.UsernameInfo, "User Id is required when Client Id is entered");
		}

		GlbBranch CreateIndiaBranch(string prefix = "IN")
			=> Factory.NewCompanyAndBranchWith(countryCode: Core.Constants.CountryCodes.India, companyCode: prefix + "C", branchCode: prefix + "B");
	}
}
