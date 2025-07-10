using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(GlbCompanyExternalPasswordHUIValidation))]
	public sealed class GlbCompanyExternalPasswordHUIValidationTest : GlbExternalPasswordValidationTest<GlbCompanyExternalPasswordHUI, GlbCompanyExternalPasswordHUIValidation>
	{
		public void TestPasswordHashValidation()
		{
			var credential = Factory.NewWithValidTestData<GlbCompanyExternalPasswordHUI>();

			credential.PasswordHash = ZString.Empty;
			AssertNoError(credential.PasswordHashInfo, "Password Hash must be a hexadecimal string (e.g. 'F45D6BA0702E2E5F').");

			credential.PasswordHash = "this is not a hex string";
			AssertHasError(credential.PasswordHashInfo, "Password Hash must be a hexadecimal string (e.g. 'F45D6BA0702E2E5F').");

			credential.PasswordHash = "F45D6BA0702E2E5F";
			AssertNoError(credential.PasswordHashInfo, "Password Hash must be a hexadecimal string (e.g. 'F45D6BA0702E2E5F').");

			credential.PasswordHash = "3581511529344BF49B6B93A44BDEDBE8AED53942D04F45D6BA0702E2E5F7A928667FB0C11CDEBA39A45D010FBD89C1F951CE9FF1B6789412780C70613CDAB56B";
			AssertNoError(credential.PasswordHashInfo, "Password Hash must be a hexadecimal string (e.g. 'F45D6BA0702E2E5F').");

			AssertExceptionThrown<MaxLengthExceededException>(() => credential.PasswordHash = "3581511529344BF49B6B93A44BDEDBE8AED53942D04F45D6BA0702E2E5F7A928667FB0C11CDEBA39A45D010FBD89C1F951CE9FF1B6789412780C70613CDAB56B00");
			AssertContains("The maximum length of 'PasswordHash' has been exceeded", ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
			AssertNoError(credential.PasswordHashInfo, "Password Hash must be a hexadecimal string (e.g. 'F45D6BA0702E2E5F').");
		}

		public void TestRequiredFieldValidation()
		{
			var credential = Factory.NewWithValidTestData<GlbCompanyExternalPasswordHUI>();
			credential.Login = ZString.Empty;
			credential.PasswordHash = ZString.Empty;
			credential.SignatureKey = ZString.Empty;
			credential.ReplacementKey = ZString.Empty;
			credential.Validation.ValidateAll();
			AssertNoError(credential.LoginInfo, "Please enter a Login.");
			AssertNoError(credential.PasswordHashInfo, "Please enter a Password Hash.");
			AssertNoError(credential.SignatureKeyInfo, "Please enter a Signature Key.");
			AssertNoError(credential.ReplacementKeyInfo, "Please enter a Replacement Key.");

			credential.Login = "login";
			AssertNoError(credential.LoginInfo, "Please enter a Login.");
			AssertHasError(credential.PasswordHashInfo, "Please enter a Password Hash.");
			AssertHasError(credential.SignatureKeyInfo, "Please enter a Signature Key.");
			AssertHasError(credential.ReplacementKeyInfo, "Please enter a Replacement Key.");
			credential.Login = ZString.Empty;

			credential.PasswordHash = "F45D6BA0702E2E5F";
			AssertHasError(credential.LoginInfo, "Please enter a Login.");
			AssertNoError(credential.PasswordHashInfo, "Please enter a Password Hash.");
			AssertHasError(credential.SignatureKeyInfo, "Please enter a Signature Key.");
			AssertHasError(credential.ReplacementKeyInfo, "Please enter a Replacement Key.");
			credential.PasswordHash = ZString.Empty;

			credential.SignatureKey = "key";
			AssertHasError(credential.LoginInfo, "Please enter a Login.");
			AssertHasError(credential.PasswordHashInfo, "Please enter a Password Hash.");
			AssertNoError(credential.SignatureKeyInfo, "Please enter a Signature Key.");
			AssertHasError(credential.ReplacementKeyInfo, "Please enter a Replacement Key.");
			credential.SignatureKey = ZString.Empty;

			credential.ReplacementKey = "replacement";
			AssertHasError(credential.LoginInfo, "Please enter a Login.");
			AssertHasError(credential.PasswordHashInfo, "Please enter a Password Hash.");
			AssertHasError(credential.SignatureKeyInfo, "Please enter a Signature Key.");
			AssertNoError(credential.ReplacementKeyInfo, "Please enter a Replacement Key.");
			credential.ReplacementKey = ZString.Empty;

			credential.Login = "login";
			credential.PasswordHash = "F45D6BA0702E2E5F";
			credential.SignatureKey = "key";
			credential.ReplacementKey = "replacement";
			AssertNoError(credential.LoginInfo, "Please enter a Login.");
			AssertNoError(credential.PasswordHashInfo, "Please enter a Password Hash.");
			AssertNoError(credential.SignatureKeyInfo, "Please enter a Signature Key.");
			AssertNoError(credential.ReplacementKeyInfo, "Please enter a Replacement Key.");
		}

		public void TestCurrentDecryptedPassword_WesternEuropeanRule()
		{
			// Base GlbExternalPasswordValidation applies validation rule, which is overridden in GlbCompanyExternalPasswordHUIValidation.
			// Because of how AssertHasError/AssertNoError work, we need to call them both in the same test, even if its on different instances (and yes, this feels like it will be amnestied one day).
			var pw = Factory.New<GlbExternalPassword>();
			pw.CurrentDecryptedPassword = "ƤǠȜзшѳяԀ";
			AssertHasError(pw.CurrentDecryptedPasswordInfo, "Current Decrypted Password only accepts Western European languages characters.");

			var huPw = Factory.New<GlbCompanyExternalPasswordHUI>();
			huPw.CurrentDecryptedPassword = ZString.Empty;
			AssertNoError(huPw.CurrentDecryptedPasswordInfo, "Current Decrypted Password only accepts Western European languages characters.");

			huPw.CurrentDecryptedPassword = "password";
			AssertNoError(huPw.CurrentDecryptedPasswordInfo, "Current Decrypted Password only accepts Western European languages characters.");

			huPw.CurrentDecryptedPassword = "ƤǠȜзшѳяԀ";
			AssertNoError(huPw.CurrentDecryptedPasswordInfo, "Current Decrypted Password only accepts Western European languages characters.");
		}

		public void TestDuplicateRule()
		{
			var savedCredential = Factory.New<GlbCompanyExternalPasswordHUI>();
			savedCredential.Login = "login";
			savedCredential.PasswordHash = "F45D6BA0702E2E5F";
			savedCredential.SignatureKey = "key";
			savedCredential.ReplacementKey = "replacement";
			savedCredential.Validation.ValidateAll();
			AssertNoErrors("Precondition: no errors before creating duplicate", savedCredential);
			Factory.Save();

			var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var duplicateCredential = newFactory.New<GlbCompanyExternalPasswordHUI>();
			duplicateCredential.Login = "userid";
			duplicateCredential.PasswordHash = "012345678";
			duplicateCredential.SignatureKey = "signature";
			duplicateCredential.ReplacementKey = "key";

			duplicateCredential.Validation.ValidateAll();
			AssertHasRowError("A duplicate error should be recorded against the bizo", duplicateCredential, "Duplicate credential found; there is already another Hungary E-Invoicing Credential with the same data.");
		}
	}
}
