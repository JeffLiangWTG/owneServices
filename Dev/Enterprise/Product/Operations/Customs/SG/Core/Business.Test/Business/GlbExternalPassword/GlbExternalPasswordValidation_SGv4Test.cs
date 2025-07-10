using System;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.SG.V4.Business.Testing
{
	[TestedType(typeof(GlbExternalPasswordValidation_SGv4))]
	class GlbExternalPasswordValidation_SGv4Test : GlbExternalPasswordValidation_SGTest<GlbExternalPassword_SGv4, GlbExternalPasswordValidation_SGv4>
	{
		public void TestValidateMailboxID()
		{
			GlbExternalPassword.GP_MailBoxID = "TEST";
			GlbExternalPassword.GP_CurrentPassword = "TEST";
			Factory.Save();
			var newGlbExternalPassword_SGv4 = CreateNewGlbExternalPassword();
			newGlbExternalPassword_SGv4.GP_MailBoxID = "TEST";
			AssertEquals(true, newGlbExternalPassword_SGv4.GP_MailBoxIDInfo.HasErrors());
		}

		public void TestCheckGP_MailBoxID()
		{
			string msg = "Declarant Code should start with 'S', T', 'M' or 'P'";
			GlbExternalPassword.GP_MailBoxID = "TEST";
			GlbExternalPassword.Validation.ValidateGP_MailBoxID();
			AssertEquals(false, GlbExternalPassword.GP_MailBoxIDInfo.HasWarning(msg));
			GlbExternalPassword.GP_MailBoxID = "Key";
			GlbExternalPassword.Validation.ValidateGP_MailBoxID();
			AssertEquals(true, GlbExternalPassword.GP_MailBoxIDInfo.HasWarning(msg));
		}

		public void TestValidateUserID()
		{
			GlbExternalPassword.GP_MailBoxID = "TEST";
			GlbExternalPassword.Validation.ValidateGP_UserID();
			AssertEquals(true, GlbExternalPassword.GP_UserIDInfo.HasErrors());
			GlbExternalPassword.GP_UserID = "DECCODE";
			AssertEquals(false, GlbExternalPassword.GP_UserIDInfo.HasErrors());
		}

		public void TestValidateTradenetPassword()
		{
			CombineAssertions(() =>
			{
				GlbExternalPassword.GP_MailBoxID = "TEST";
				GlbExternalPassword.CurrentDecryptedPassword = ZString.Empty;
				AssertHasError("Empty Current Decrypted Password", GlbExternalPassword.CurrentDecryptedPasswordInfo, "Please enter a Current Password.");
				AssertPasswordComplexity(GlbExternalPassword.CurrentDecryptedPasswordInfo);
			}

			);
		}

		public void TestValidateNextTradenetPassword()
		{
			CombineAssertions(() =>
			{
				GlbExternalPassword.GP_MailBoxID = "TEST";
				GlbExternalPassword.NextDecryptedPassword = ZString.Empty;
				AssertNoErrors("Empty Next Decrypted Password Allowed", GlbExternalPassword.NextDecryptedPasswordInfo);
				AssertPasswordComplexity(GlbExternalPassword.NextDecryptedPasswordInfo);
			}

			);
		}

		public void TestEnhancedPasswordAllowsSpecialCharacters()
		{
			GlbExternalPassword.GP_MailBoxID = "TEST";
			GlbExternalPassword.CurrentDecryptedPassword = "m62Q8&01U*lB1i";
			AssertEquals("Special characters are allowed", false, GlbExternalPassword.CurrentDecryptedPasswordInfo.HasErrors());
		}

		void AssertPasswordComplexity(ZPropertyInfo passwordInfo)
		{
			passwordInfo.Value = (ZString)"1234ABCDabc";
			AssertPasswordMessageErrrors("Password not long enough", passwordInfo, new Action<string, ZPropertyInfo, string>(AssertHasMessageError));
			passwordInfo.Value = (ZString)"abcdefghijkl";
			AssertPasswordMessageErrrors("Password only lowercase", passwordInfo, new Action<string, ZPropertyInfo, string>(AssertHasMessageError));
			passwordInfo.Value = (ZString)"ABCDEFGHIJKL";
			AssertPasswordMessageErrrors("Password only uppercase", passwordInfo, new Action<string, ZPropertyInfo, string>(AssertHasMessageError));
			passwordInfo.Value = (ZString)"123456789012";
			AssertPasswordMessageErrrors("Password only numerics", passwordInfo, new Action<string, ZPropertyInfo, string>(AssertHasMessageError));
			passwordInfo.Value = (ZString)"!@#$%^&*()-=";
			AssertPasswordMessageErrrors("Password only punctuation", passwordInfo, new Action<string, ZPropertyInfo, string>(AssertHasMessageError));
			passwordInfo.Value = (ZString)"ABCDEFGhijkl";
			AssertPasswordMessageErrrors("Password is a mixture of upper and lower", passwordInfo, new Action<string, ZPropertyInfo, string>(AssertHasMessageError));
			passwordInfo.Value = (ZString)"123456ABCDEFG";
			AssertPasswordMessageErrrors("Password is a mixture of numeric and upper", passwordInfo, new Action<string, ZPropertyInfo, string>(AssertHasMessageError));
			passwordInfo.Value = (ZString)"123456abcdefg";
			AssertPasswordMessageErrrors("Password is a mixture of numeric and lower", passwordInfo, new Action<string, ZPropertyInfo, string>(AssertHasMessageError));
			passwordInfo.Value = (ZString)"1*3456a#cdefg";
			AssertPasswordMessageErrrors("Password is a mixture of numeric, punctuation and lower", passwordInfo, new Action<string, ZPropertyInfo, string>(AssertHasMessageError));
			passwordInfo.Value = (ZString)"y53E7d10PtkG1eAfj34Klyd9";
			AssertPasswordMessageErrrors("Password valid with all requirements met using numerics", passwordInfo, new Action<string, ZPropertyInfo, string>(AssertNoMessageError));
			passwordInfo.Value = (ZString)"1234ABCDabcd1234ABCDabcd!";
			AssertPasswordMessageErrrors("Password to long", passwordInfo, new Action<string, ZPropertyInfo, string>(AssertHasMessageError));
			passwordInfo.Value = (ZString)"y&E?d!PtkG}efj*MHxfu";
			AssertPasswordMessageErrrors("Password valid with all requirements met using puntuation", passwordInfo, new Action<string, ZPropertyInfo, string>(AssertNoMessageError));
		}

		void AssertPasswordMessageErrrors(ZString errorMessage, ZPropertyInfo propertyToCheck, Action<string, ZPropertyInfo, string> assertComplexityMessage)
		{
			var complexityMessage = "Password is required to be at least 12 and not greater than 24 characters, must contain at least one of each of capital, lowercase and non-alphabetic characters.";
			assertComplexityMessage.Invoke(errorMessage, propertyToCheck, complexityMessage);
		}
	}
}
