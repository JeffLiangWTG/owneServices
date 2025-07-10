using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestsSubclassesOf(typeof(GlbExternalPasswordWithCertificate))]
	public abstract class GlbExternalPasswordWithCertificateTest<T> : GlbExternalPasswordTest<T>
			where T : GlbExternalPasswordWithCertificate
	{
		public virtual void TestSetDefaultValues()
		{
			AssertEquals("GP_PasswordStatus", PasswordStatusList.Codes.Invalid, GlbExternalPassword.GP_PasswordStatus);
		}

		public void TestCertificateStatus()
		{
			GlbExternalPassword.CurrentDecryptedCertificatePassphrase = ValidPassword;
			GlbExternalPassword.GP_Certificate = null;
			AssertEquals(GlbExternalPasswordWithCertificate.CertificateEmpty, GlbExternalPassword.CertificateStatus);
			GlbExternalPassword.GP_Certificate = new byte[] { 241, 40 };
			AssertEquals(GlbExternalPasswordWithCertificate.CertificateInvalid, GlbExternalPassword.CertificateStatus);
			GlbExternalPassword.GP_Certificate = ValidCertificate;
			AssertEquals(GlbExternalPasswordWithCertificate.CertificateLoaded, GlbExternalPassword.CertificateStatus);
			GlbExternalPassword.CurrentDecryptedCertificatePassphrase = ZString.Empty;
			AssertEquals(GlbExternalPasswordWithCertificate.CertificateInvalid, GlbExternalPassword.CertificateStatus);
			GlbExternalPassword.CurrentDecryptedCertificatePassphrase = ValidPassword;
			AssertEquals(GlbExternalPasswordWithCertificate.CertificateLoaded, GlbExternalPassword.CertificateStatus);
		}

		public void TestCurrentDecryptedCertificatePassphrase()
		{
			GlbExternalPassword.GP_Certificate = ValidCertificate;
			GlbExternalPassword.CurrentDecryptedCertificatePassphrase = "!!!";
			AssertHasError(GlbExternalPassword.GP_CertificateInfo, "The Certificate or accompanying password is invalid.");
			GlbExternalPassword.CurrentDecryptedCertificatePassphrase = ValidPassword;
			AssertNoError(GlbExternalPassword.GP_CertificateInfo, "The Certificate or accompanying password is invalid.");
		}

		protected virtual string ValidPassword => X509Certificate2TestHelper.ValidPassword;
		protected virtual byte[] ValidCertificate => X509Certificate2TestHelper.ValidCertificate;

		public void TestCryptographicExceptionsToIgnore()
		{
			var exceptionsToIgnore = new string[]
			{
				"The specified network password is not correct",
				"Cannot find the requested object"
			};

			GlbExternalPassword.GP_Certificate = X509Certificate2TestHelper.ValidCertificate;
			GlbExternalPassword.CurrentDecryptedCertificatePassphrase = "wrong";

			GlbExternalPassword.GP_Certificate = new byte[] { 241, 40 };

			foreach (var exToIgnore in exceptionsToIgnore)
			{
				Assert(
					$@"The following cryptographic exception should have been ignored: ""{exToIgnore}"".",
					!ErrorReporter.LastExceptionsReported().Any(ex => ex.Contains(exToIgnore)));
			}
		}

		public void TestInvalidPasswordDoesntReportError()
		{
			GlbExternalPassword.GP_Certificate = X509Certificate2TestHelper.ValidCertificate;
			GlbExternalPassword.CurrentDecryptedCertificatePassphrase = "wrong";
			Assert("Incorrect password errors should not be reported.", !ErrorReporter.LastExceptionsReported().Any(ex => ex.Contains("The specified network password is not correct")));
		}

		public virtual void TestDataDefaultFromCertificate()
		{
			var issueDate = new ZDateTime(2017, 7, 21, 18, 14, 15);
			var expiryDate = new ZDateTime(2020, 7, 21, 18, 24, 00);
			var userID = "D36659B690BD3539E6B545594F854D69EE959BBF";

			GlbExternalPassword.CurrentDecryptedCertificatePassphrase = X509Certificate2TestHelper.ValidPassword;
			GlbExternalPassword.GP_Certificate = X509Certificate2TestHelper.ValidCertificate;
			AssertEquals("GP_IssueDate", issueDate, GlbExternalPassword.GP_IssueDate);
			AssertEquals("GP_ExpiryDate", expiryDate, GlbExternalPassword.GP_ExpiryDate);
			AssertEquals("GP_UserID", userID, GlbExternalPassword.GP_UserID);

			GlbExternalPassword.GP_Certificate = new byte[] { 241, 40 };
			AssertEquals("GP_IssueDate", ZDateTime.Empty, GlbExternalPassword.GP_IssueDate);
			AssertEquals("GP_ExpiryDate", ZDateTime.Empty, GlbExternalPassword.GP_ExpiryDate);
			AssertEquals("GP_UserID", ZString.Empty, GlbExternalPassword.GP_UserID);

			GlbExternalPassword.GP_Certificate = X509Certificate2TestHelper.ValidCertificate;
			GlbExternalPassword.CurrentDecryptedCertificatePassphrase = ZString.Empty;
			AssertEquals("GP_IssueDate", ZDateTime.Empty, GlbExternalPassword.GP_IssueDate);
			AssertEquals("GP_ExpiryDate", ZDateTime.Empty, GlbExternalPassword.GP_ExpiryDate);
			AssertEquals("GP_UserID", ZString.Empty, GlbExternalPassword.GP_UserID);
		}

		public virtual void TestReadOnly()
		{
			AssertEquals("GP_IssueDateInfo.ReadOnly", true, GlbExternalPassword.GP_IssueDateInfo.ReadOnly);
			AssertEquals("GP_ExpiryDateInfo.ReadOnly", true, GlbExternalPassword.GP_ExpiryDateInfo.ReadOnly);
			AssertEquals("GP_UserIDInfo.ReadOnly", true, GlbExternalPassword.GP_UserIDInfo.ReadOnly);
		}
	}
}
