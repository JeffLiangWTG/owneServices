using System.IO;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(GlbExternalPasswordValidation))]
	sealed class GlbExternalPasswordValidationTest : GlbExternalPasswordValidationBase_TWTest<GlbExternalPassword, GlbExternalPasswordValidation>
	{
		[DatCapabilityRequirement("SOURCE_CODE")]
		[ExpectNoExceptions]
		public new void TestCheckGP_Certificate()
		{
			var errorMessage = "A staff can only have one certificate file.";
			var collection = new GlbExternalPasswordCollection(Factory.New<GlbStaff>());
			var sub1 = collection.AddNew();
			sub1.CurrentDecryptedCertificatePassphrase = X509Certificate2TestHelper.ValidPassword;
			sub1.GP_Certificate = X509Certificate2TestHelper.ValidCertificate;
			NUnit.Framework.Assert.That(sub1.CertificateStatus, NUnit.Framework.Is.EqualTo(GlbExternalPasswordWithCertificate.CertificateLoaded).Using(CustomComparers.TypeComparison));
			var path = Path.Combine(BaseSourcePath, @"Enterprise\Product\Documents\DocumentEngine\Testing\DigitalSignature\Certificate_ValidWithPassword (the password is p455w0rd).pfx");
			var certificate = File.ReadAllBytes(path);
			var sub2 = collection.AddNew();
			sub2.CurrentDecryptedCertificatePassphrase = "p455w0rd";
			sub2.GP_Certificate = certificate;
			NUnit.Framework.Assert.That(sub2.CertificateStatus, NUnit.Framework.Is.EqualTo(GlbExternalPasswordWithCertificate.CertificateLoaded).Using(CustomComparers.TypeComparison));
			AssertHasErrorContaining(sub2.GP_CertificateInfo, errorMessage);
			AssertNoErrorContaining(sub1.GP_CertificateInfo, errorMessage);
			sub2.CurrentDecryptedCertificatePassphrase = "invalid";
			NUnit.Framework.Assert.That(sub2.CertificateStatus, NUnit.Framework.Is.EqualTo(GlbExternalPasswordWithCertificate.CertificateInvalid).Using(CustomComparers.TypeComparison));
			AssertNoErrorContaining(sub2.GP_CertificateInfo, errorMessage);
			sub2.CurrentDecryptedCertificatePassphrase = X509Certificate2TestHelper.ValidPassword;
			sub2.GP_Certificate = X509Certificate2TestHelper.ValidCertificate;
			AssertNoErrorContaining(sub2.GP_CertificateInfo, errorMessage);
		}

		public void TestCheckGP_MailBoxID()
		{
			var errorMessageTVA = "The entered Mail Box is invalid, the Mail Box format must be Mail Box - Mail Sub Box and use '-' to separate. For example: CBK0001-0.";
			var errorMessageUVC = "The entered Mail Box is invalid, the Mail Box format must be Mail Box - Mail Sub Box and use '-' to separate. For example: PABKN00001-G.";
			var collection = new GlbExternalPasswordCollection(Factory.New<GlbStaff>());
			var sub = collection.AddNew();
			sub.GP_MailBoxID = ZString.Empty;
			AssertHasErrorContaining(sub.GP_MailBoxIDInfo, MandatoryValidation.MustBeEntered);
			sub.GP_PasswordType = PasswordTypesList.Codes.TVA;
			sub.GP_MailBoxID = "111";
			AssertNoErrorContaining(sub.GP_MailBoxIDInfo, MandatoryValidation.MustBeEntered);
			AssertHasErrorContaining(sub.GP_MailBoxIDInfo, errorMessageTVA);
			sub.GP_MailBoxID = "CBK0123-A";
			AssertNoErrorContaining(sub.GP_MailBoxIDInfo, errorMessageTVA);
			sub.GP_MailBoxID = "CBKZ0123-A";
			AssertNoErrorContaining(sub.GP_MailBoxIDInfo, errorMessageTVA);
			sub.GP_MailBoxID = "CBKZ0123A";
			AssertHasErrorContaining(sub.GP_MailBoxIDInfo, errorMessageTVA);
			sub.GP_MailBoxID = "CBKZ123-AA";
			AssertHasErrorContaining(sub.GP_MailBoxIDInfo, errorMessageTVA);
			sub.GP_MailBoxID = "TBK046-0";
			AssertHasErrorContaining(sub.GP_MailBoxIDInfo, errorMessageTVA);
			sub.GP_MailBoxID = "TBK0461-0";
			AssertNoErrorContaining(sub.GP_MailBoxIDInfo, errorMessageTVA);
			sub.GP_MailBoxID = "tbk0461-0";
			AssertHasErrorContaining(sub.GP_MailBoxIDInfo, errorMessageTVA);
			var sub2 = collection.AddNew();
			sub2.GP_PasswordType = PasswordTypesList.Codes.TVA;
			AssertNoErrorContaining(sub2.GP_MailBoxIDInfo, MailboxMustBeUnique);
			sub2.GP_MailBoxID = "TBK0461-0";
			AssertHasErrorContaining(sub2.GP_MailBoxIDInfo, MailboxMustBeUnique);
			sub.GP_PasswordType = PasswordTypesList.Codes.UVC;
			sub.GP_MailBoxID = "1111111111";
			AssertHasErrorContaining(sub.GP_MailBoxIDInfo, errorMessageUVC);
			sub.GP_MailBoxID = "1111111111-";
			AssertHasErrorContaining(sub.GP_MailBoxIDInfo, errorMessageUVC);
			sub.GP_MailBoxID = "1111111111-1";
			AssertNoErrorContaining(sub.GP_MailBoxIDInfo, errorMessageUVC);
			sub.GP_MailBoxID = "A111111111-1";
			AssertNoErrorContaining(sub.GP_MailBoxIDInfo, errorMessageUVC);
			sub.GP_MailBoxID = "a111111111-1";
			AssertHasErrorContaining(sub.GP_MailBoxIDInfo, errorMessageUVC);
			sub.GP_MailBoxID = "a111111111-A";
			AssertHasErrorContaining(sub.GP_MailBoxIDInfo, errorMessageUVC);
			sub.GP_MailBoxID = "A11111111-A";
			AssertHasErrorContaining(sub.GP_MailBoxIDInfo, errorMessageUVC);
		}

		internal const string MailboxMustBeUnique = "Each subscription must have a unique Mailbox.";

		public void TestCheckGP_UserID()
		{
			var errorMessage = "The same Sender ID already exists. Please enter a different Sender ID.";
			var collection = new GlbExternalPasswordCollection(Factory.New<GlbStaff>());
			var sub1 = collection.AddNew();
			sub1.GP_UserID = ZString.Empty;
			AssertHasErrorContaining(sub1.GP_UserIDInfo, MandatoryValidation.MustBeEntered);
			sub1.GP_UserID = "abcd";
			AssertNoErrorContaining(sub1.GP_UserIDInfo, MandatoryValidation.MustBeEntered);
			var sub2 = collection.AddNew();
			AssertNoError(sub2.GP_UserIDInfo, errorMessage);
			sub2.GP_UserID = "ABCD";
			AssertHasErrorContaining(sub2.GP_UserIDInfo, errorMessage);
			sub2.GP_UserID = "abce";
			AssertNoErrorContaining(sub2.GP_UserIDInfo, errorMessage);
		}
	}
}
