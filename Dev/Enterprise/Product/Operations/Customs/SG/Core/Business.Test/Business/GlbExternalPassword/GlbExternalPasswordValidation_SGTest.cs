using CargoWise.ComponentModel;

namespace Enterprise.Customs.SG.V4.Business.Testing
{
	abstract class GlbExternalPasswordValidation_SGTest<T, TValidation> : MasterFiles.Business.Testing.GlbExternalPasswordValidationTest<T, TValidation> where T : GlbExternalPassword_SG where TValidation : GlbExternalPasswordValidation_SG
	{
		public void TestValidateCurrentDecryptedPassword()
		{
			GlbExternalPassword.GP_MailBoxID = "TEST";
			GlbExternalPassword.CurrentDecryptedPassword = "";
			GlbExternalPassword.Validation.ValidateGP_CurrentPassword();
			AssertEquals(true, GlbExternalPassword.CurrentDecryptedPasswordInfo.HasErrors());
			GlbExternalPassword.CurrentDecryptedPassword = "s01D5m18VerF4q";
			AssertEquals(false, GlbExternalPassword.CurrentDecryptedPasswordInfo.HasErrors());
			GlbExternalPassword.GP_MailBoxID = "";
			GlbExternalPassword.CurrentDecryptedPassword = "ABC";
			AssertEquals(false, GlbExternalPassword.CurrentDecryptedPasswordInfo.HasErrors());
			GlbExternalPassword.CurrentDecryptedPassword = "1234ABC";
			AssertEquals(false, GlbExternalPassword.CurrentDecryptedPasswordInfo.HasErrors());
			GlbExternalPassword.CurrentDecryptedPassword = "ABCDEFGH";
			AssertEquals(false, GlbExternalPassword.CurrentDecryptedPasswordInfo.HasErrors());
			GlbExternalPassword.CurrentDecryptedPassword = "12345678";
			AssertEquals(false, GlbExternalPassword.CurrentDecryptedPasswordInfo.HasErrors());
			GlbExternalPassword.CurrentDecryptedPassword = "1234ABCD";
			AssertEquals(false, GlbExternalPassword.CurrentDecryptedPasswordInfo.HasErrors());
		}

		public void TestValidateAll()
		{
			GlbExternalPassword.GP_MailBoxID = "TEST";
			GlbExternalPassword.Validation.ValidateAll();
			AssertEquals(true, GlbExternalPassword.CurrentDecryptedPasswordInfo.HasErrors());
			GlbExternalPassword.GP_MailBoxID = "";
			GlbExternalPassword.CurrentDecryptedPassword = "ABCD1234abcd";
			GlbExternalPassword.Validation.ValidateAll();
			AssertEquals(false, GlbExternalPassword.CurrentDecryptedPasswordInfo.HasErrors());
			GlbExternalPassword.NextDecryptedPassword = "1234abcdEFGH";
			GlbExternalPassword.Validation.ValidateAll();
			AssertEquals(false, GlbExternalPassword.NextDecryptedPasswordInfo.HasErrors());
		}
	}
}
