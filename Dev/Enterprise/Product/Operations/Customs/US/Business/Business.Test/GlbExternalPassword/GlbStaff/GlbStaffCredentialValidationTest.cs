using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(GlbStaffCredentialValidation))]
	sealed class GlbStaffCredentialValidationTest : MasterFiles.Business.Testing.GlbExternalPasswordValidationTest<GlbStaffCredential, GlbStaffCredentialValidation>
	{
		public void TestCheckGP_MailBoxID()
		{
			var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
			helper.CreateCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.InsuranceAgent, "Insurance Agent");
			helper.CreateCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.InsuranceAgent, "ABC", ZDateTime.Today.AddYears(-1), ZDateTime.Today.AddYears(1));
			Factory.Save();

			var collection = new GlbStaffCredentialCollection(Factory.New<GlbStaff>());
			var credential = collection.AddNew();

			credential.GP_MailBoxID = "";
			AssertHasMessageError(credential.GP_MailBoxIDInfo, "You have not entered a value.");

			credential.GP_MailBoxID = "123";
			AssertHasMessageError(credential.GP_MailBoxIDInfo, "The code you have selected is not in the list.");

			credential.GP_MailBoxID = "ABC";
			AssertNoMessageError(credential.GP_MailBoxIDInfo, "You have not entered a value.");
			AssertNoMessageError(credential.GP_MailBoxIDInfo, "The code you have selected is not in the list.");
		}

		public void TestCheckGP_UserID()
		{
			var collection = GlbStaffWrapper.Get(Factory.New<GlbStaff>()).PasswordCollection;
			var credential = collection.AddNew();

			credential.GP_UserID = "";
			AssertHasMessageError(credential.GP_UserIDInfo, "You have not entered a value.");

			credential.GP_UserID = "12";
			AssertHasMessageError(credential.GP_UserIDInfo, "User Name must be at least 3 characters.");

			credential.GP_UserID = "124";
			AssertNoMessageError(credential.GP_UserIDInfo, "You have not entered a value.");
			AssertNoMessageError(credential.GP_UserIDInfo, "User Name must be at least 3 characters.");

			var credential2 = collection.AddNew();
			credential2.GP_UserID = "124";
			AssertHasError(credential2.GP_UserIDInfo, "User Name must be unique.");

			credential.Validation.ValidateGP_UserID();
			AssertHasError(credential.GP_UserIDInfo, "User Name must be unique.");
		}

		public void TestCheckCurrentDecryptedCertificatePassphrase()
		{
			var collection = new GlbStaffCredentialCollection(Factory.New<GlbStaff>());
			var credential = collection.AddNew();

			credential.CurrentDecryptedCertificatePassphrase = "";
			AssertHasMessageError(credential.CurrentDecryptedCertificatePassphraseInfo, "You have not entered a value.");

			credential.CurrentDecryptedCertificatePassphrase = "123";
			AssertHasMessageError(credential.CurrentDecryptedCertificatePassphraseInfo, "Password must greater than 3 characters.");

			credential.CurrentDecryptedCertificatePassphrase = "1234";
			AssertNoMessageError(credential.CurrentDecryptedCertificatePassphraseInfo, "You have not entered a value.");
			AssertNoMessageError(credential.CurrentDecryptedCertificatePassphraseInfo, "Password must greater than 3 characters.");
		}
	}
}
