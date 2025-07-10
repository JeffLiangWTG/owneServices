using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Business.Testing
{
	public class GlbExternalPasswordBase_TWForTest : GlbExternalPasswordBase_TW
	{
		public GlbExternalPasswordBase_TWForTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}
	}

	[TestedType(typeof(GlbExternalPasswordValidationBase_TW))]
	public sealed class GlbExternalPasswordValidationBase_TWBaseOnlyTest : GlbExternalPasswordValidationBase_TWTest<GlbExternalPasswordBase_TWForTest, GlbExternalPasswordValidationBase_TW>
	{
	}

	[TestsSubclassesOf(typeof(GlbExternalPasswordValidationBase_TW))]
	public abstract class GlbExternalPasswordValidationBase_TWTest<T, TValidation> : GlbExternalPasswordWithCertificateValidationTest<T, TValidation>
		where T : GlbExternalPasswordBase_TW
		where TValidation : GlbExternalPasswordValidationBase_TW
	{
		public virtual void TestCheckGP_PasswordType()
		{
			var sub = GlbExternalPassword;
			sub.GP_PasswordType = ZString.Empty;
			AssertHasErrorContaining(sub.GP_PasswordTypeInfo, MandatoryValidation.MustBeEntered);
			sub.GP_PasswordType = "AAA";
			AssertNoErrorContaining(sub.GP_PasswordTypeInfo, MandatoryValidation.MustBeEntered);
			AssertHasError("GP_PasswordType", sub.GP_PasswordTypeInfo, SelectionMustBeValid);
			sub.GP_PasswordType = "TVA";
			AssertNoError("GP_PasswordType", sub.GP_PasswordTypeInfo, SelectionMustBeValid);
			sub.GP_PasswordType = "UVC";
			AssertNoError("GP_PasswordType", sub.GP_PasswordTypeInfo, SelectionMustBeValid);
		}

		protected const string SelectionMustBeValid = "Enter a valid selection.";

		public virtual void TestCheckCurrentDecryptedPassword()
		{
			ValidationTestHelper.AssertErrorIfNotEntered(GlbExternalPassword.CurrentDecryptedPasswordInfo);
		}

		[ExpectNoExceptions]
		public void TestValidateDuplicateConstraint()
		{
			var errorMessage = "Duplicate credential found; there is already another GlbExternalPassword with the same data.";
			var staff = Factory.New<GlbStaff>();
			var pw1 = Factory.New<T>();
			pw1.GP_GS = staff.PK;
			pw1.GP_MailBoxID = "12345678-9";
			pw1.GP_UserID = "abcd";

			var pw2 = Factory.New<T>();
			pw2.GP_GS = staff.PK;
			pw2.GP_UserID = "ABCD";
			pw2.GP_MailBoxID = "12345678-0";
			pw2.RunPreSaveValidation();
			NUnit.Framework.Assert.That(!pw2.RowErrors.Contains(errorMessage), NUnit.Framework.Is.True);

			pw2.GP_MailBoxID = "12345678-9";
			pw2.RunPreSaveValidation();
			NUnit.Framework.Assert.That(pw2.RowErrors.Contains(errorMessage), NUnit.Framework.Is.True);
		}

		[ExpectNoExceptions]
		public void TestIsCertificateMandatory()
		{
			NUnit.Framework.Assert.That(IsCertificateMandatory, NUnit.Framework.Is.True);
			var password = Factory.New<T>();
			password.GP_MailBoxID = "12345678-9";
			password.GP_UserID = "abcd";
			password.RunPreSaveValidation();
			AssertHasError(password.GP_CertificateInfo, MandatoryValidation.MustBeEnteredMessage(password.GP_CertificateInfo.HumanReadableName));

			password.GP_UserID = "abcd";
			password.GP_Certificate = X509Certificate2TestHelper.ValidCertificate;
			password.RunPreSaveValidation();
			AssertNoError(password.GP_CertificateInfo, MandatoryValidation.MustBeEnteredMessage(password.GP_CertificateInfo.HumanReadableName));
			AssertHasError(password.GP_CertificateInfo, "The Certificate or accompanying password is invalid.");
			password.CurrentDecryptedCertificatePassphrase = X509Certificate2TestHelper.ValidPassword;
			password.RunPreSaveValidation();
			AssertNoError(password.GP_CertificateInfo, MandatoryValidation.MustBeEnteredMessage(password.GP_CertificateInfo.HumanReadableName));
			AssertNoError(password.GP_CertificateInfo, "The Certificate or accompanying password is invalid.");
		}
	}
}
