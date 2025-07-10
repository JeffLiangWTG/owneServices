using System;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.Pkcs;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using CargoWise.ApplicationManager.Common;
using CargoWise.IO;
using Enterprise.CryptoUtilities;

namespace Enterprise.CMREdifact.CryptoUtilities.Testing
{
	[NUnit.Framework.DatCapabilityRequirement("SOURCE_CODE")]
	sealed class StoreTest : NUnit.Framework.TestCase
	{
		public void TestConstructFromPFXFileWithWrongPassword_Sign()
		{
			string storeFileName = resourceRetriever.SaveResourceToFile("EagleDI_SEDI_R1_TRIAL_Conf.p12");
			using (var store = new Store(storeFileName, "wrongpassword"))
			{
				try
				{
					store.Sign("test");
					Fail("Exception should occur above");
				}
				catch (CryptoUtilitiesException e)
				{
					AssertStartsWith("ExceptionMessage", @"The supplied password for your private key file is incorrect or the certificate is not compatible with the current operating system -", e.Message);
				}
			}
		}

		public void TestConstructFromPFXFileWithWrongPassword_Decrypt()
		{
			string storeFileName = resourceRetriever.SaveResourceToFile("EagleDI_SEDI_R1_TRIAL_Conf.p12");
			using (var store = new Store(storeFileName, "wrongpassword"))
			{
				try
				{
					store.Decrypt(resourceRetriever.GetString("enveloped1.txt", Encoding.ASCII));
					Fail("Exception should occur above");
				}
				catch (CryptoUtilitiesException e)
				{
					AssertStartsWith("ExceptionMessage", @"The supplied password for your private key file is incorrect or the certificate is not compatible with the current operating system -", e.Message);
				}
			}
		}

		public void TestConstructFromPFXFileWithBadFile_Sign()
		{
			string storeFileName = resourceRetriever.SaveResourceToFile("acme_ca.crt");
			using (var store = new Store(storeFileName, "password"))
			{
				try
				{
					store.Sign("abc");
					Fail("Exception should occur above");
				}
				catch (CryptoUtilitiesException e)
				{
					Assert("ExceptionMessage", e.Message.StartsWith("The supplied file is not a valid private key file."));
				}
			}
		}

		public void TestConstructFromPFXFileWithBadFile_Decrypt()
		{
			string storeFileName = resourceRetriever.SaveResourceToFile("acme_ca.crt");
			using (var store = new Store(storeFileName, "password"))
			{
				try
				{
					store.Decrypt(resourceRetriever.GetString("enveloped1.txt", Encoding.ASCII));
					Fail("Exception should occur above");
				}
				catch (CryptoUtilitiesException e)
				{
					Assert("ExceptionMessage", e.Message.StartsWith("The supplied file is not a valid private key file."));
				}
			}
		}

		public void TestDispose()
		{
			string storeFileName = resourceRetriever.SaveResourceToFile("SEDI_Test_EncryptionKeyPair.p12");
			var store = new Store(storeFileName, "4Customs&SEDI");
			using (store)
			{
				// calling some methods to make sure that certificate and certificate collection are loaded
				store.Sign("test");
				store.Decrypt(resourceRetriever.GetString("enveloped1.txt", Encoding.ASCII));

				AssertNotEquals("(pre-condition) certificate.Handle", IntPtr.Zero, store.lazyCertificate.Handle);
				Assert("(pre-condition) certificates.Count", store.lazyCertificateCollection.Count > 0);
				Assert("(pre-condition) certificates.Handle", store.lazyCertificateCollection.Cast<X509Certificate2>().All(c => c.Handle != IntPtr.Zero));
			}

			AssertEquals("certificate.Handle", IntPtr.Zero, store.lazyCertificate.Handle);
			Assert("certificates.Count", store.lazyCertificateCollection.Count > 0);
			Assert("certificates.Handle", store.lazyCertificateCollection.Cast<X509Certificate2>().All(c => c.Handle == IntPtr.Zero));
		}

		public void TestSerialNumber()
		{
			string storeFileName = resourceRetriever.SaveResourceToFile("EagleDI_SEDI_R1_TRIAL_Conf.p12");
			using (var store = new Store(storeFileName, "EDITrialKeys03_04_2003"))
			{
				AssertEquals("SerialNumber", "3e 8b 8f c2", store.SerialNumber);
			}
		}

		public void TestName()
		{
			string storeFileName = resourceRetriever.SaveResourceToFile("EagleDI_SEDI_R1_TRIAL_Conf.p12");
			using (var store = new Store(storeFileName, "EDITrialKeys03_04_2003"))
			{
				AssertEquals("Name", "Eagle Datamation SEDI", store.Name);
			}
		}

		public void TestIssuerName()
		{
			string storeFileName = resourceRetriever.SaveResourceToFile("EagleDI_SEDI_R1_TRIAL_Conf.p12");
			using (var store = new Store(storeFileName, "EDITrialKeys03_04_2003"))
			{
				AssertEquals("IssuerName", "Acme Trust CA", store.IssuerName);
			}
		}

		public void TestEmailAddress()
		{
			string storeFileName = resourceRetriever.SaveResourceToFile("EagleDI_SEDI_R1_TRIAL_Conf.p12");
			using (var store = new Store(storeFileName, "EDITrialKeys03_04_2003"))
			{
				AssertEquals("EmailAddress", "cmr@acsedi.edi.net.au", store.EmailAddress);
			}
		}

		[NUnit.Framework.TestTimeZone]
		public void TestValidFromDate()
		{
			string storeFileName = resourceRetriever.SaveResourceToFile("EagleDI_SEDI_R1_TRIAL_Conf.p12");
			using (var store = new Store(storeFileName, "EDITrialKeys03_04_2003"))
			{
				AssertEquals("ValidFromDate", new DateTime(2003, 04, 3, 3, 34, 58), store.ValidFromDate);
			}
		}

		[NUnit.Framework.TestTimeZone]
		public void TestValidToDate()
		{
			string storeFileName = resourceRetriever.SaveResourceToFile("EagleDI_SEDI_R1_TRIAL_Conf.p12");
			using (var store = new Store(storeFileName, "EDITrialKeys03_04_2003"))
			{
				AssertEquals("ValidToDate", new DateTime(2004, 04, 2, 2, 34, 58), store.ValidToDate);
			}
		}

		[NUnit.Framework.TestTimeZone]
		public void TestGetCertificateState()
		{
			string storeFileName = resourceRetriever.SaveResourceToFile("EagleDI_SEDI_R1_TRIAL_Conf.p12");
			using (var store = new Store(storeFileName, "EDITrialKeys03_04_2003"))
			{
				var okState = store.GetCertificateState(store.ValidToDate.AddDays(-90));
				AssertEquals("okState.Errors", 0, okState.Errors.Length);
				AssertEquals("okState.Warnings", 0, okState.Warnings.Length);

				var errorState = store.GetCertificateState(store.ValidToDate.AddDays(2));
				AssertEquals("okState.Errors", "The certificate has expired as of " + new DateTime(2004, 04, 2, 2, 34, 58) + ".", string.Join("\r\n", errorState.Errors));
				AssertEquals("okState.Warnings", 0, errorState.Warnings.Length);
			}
		}

		#region Encryption/Decryption

		public void TestDecrypt()
		{
			string certificateFileName = resourceRetriever.SaveResourceToFile("SEDI_Test_EncryptionKeyPair.p12");
			using (X509Certificate2 cert = new X509Certificate2(certificateFileName, "4Customs&SEDI"))
			{
				RemoveCertificateFromWindowsStores(cert);
			}

			var inputString = resourceRetriever.GetString("enveloped1.txt", Encoding.ASCII);
			var inputString2 = resourceRetriever.GetString("enveloped1.results", Encoding.ASCII);

			string storeFileName = resourceRetriever.SaveResourceToFile("SEDI_Test_EncryptionKeyPair.p12");
			using (var decryptionCertificateStore = new Store(storeFileName, "4Customs&SEDI"))
			{
				AssertEquals("DecryptedMessage", inputString2, decryptionCertificateStore.Decrypt(inputString));
			}
		}

		public void TestDecrypt_InvalidMessage()
		{
			string certificateFileName = resourceRetriever.SaveResourceToFile("SEDI_Test_EncryptionKeyPair.p12");
			using (X509Certificate2 cert = new X509Certificate2(certificateFileName, "4Customs&SEDI"))
			{
				RemoveCertificateFromWindowsStores(cert);
			}

			var inputString = Convert.ToBase64String(Encoding.ASCII.GetBytes("not a valid CMS/PKCS#7 envelope"));
			string storeFileName = resourceRetriever.SaveResourceToFile("SEDI_Test_EncryptionKeyPair.p12");
			using (var decryptionCertificateStore = new Store(storeFileName, "4Customs&SEDI"))
			{
				AssertStartsWith
				(
					"exception message",
					"CryptoUtility Exception. ASN1 bad tag value met.",
					AssertExceptionThrown<CryptoUtilitiesException>(() => decryptionCertificateStore.Decrypt(inputString)).Message
				);
			}
		}

		public void TestDecrypt_WrongRecipient()
		{
			string certificateFileName = resourceRetriever.SaveResourceToFile("generated.out.sample-org.pfx");
			using (X509Certificate2 cert = new X509Certificate2(certificateFileName, "123"))
			{
				RemoveCertificateFromWindowsStores(cert);
			}

			const string message = "HELLO";

			string encryptedMessage;
			certificateFileName = resourceRetriever.SaveResourceToFile("generated.out.sample-org-2.cer");
			using (var recipient2Cert = new Certificate(certificateFileName))
			{
				encryptedMessage = recipient2Cert.Encrypt(message);
			}

			string storeFileName = resourceRetriever.SaveResourceToFile("generated.out.sample-org-2.pfx");
			using (var recipient2Key = new Store(storeFileName, "123"))
			{
				AssertEquals("(pre-condition)", message, recipient2Key.Decrypt(encryptedMessage));
			}

			storeFileName = resourceRetriever.SaveResourceToFile("generated.out.sample-org.pfx");
			using (var recipient1Key = new Store(storeFileName, "123"))
			{
				AssertStartsWith
				(
					"exception message",
					"CryptoUtility Exception. The enveloped-data message does not contain the specified recipient.",
					AssertExceptionThrown<CryptoUtilitiesException>(() => recipient1Key.Decrypt(encryptedMessage)).Message
				);
			}
		}

		static void RemoveCertificateFromWindowsStores(X509Certificate2 cert)
		{
			// todo: We no longer rely on windows store and can remove this method. Lets do it during the following clean-up.

			using (X509Store store = new X509Store(StoreLocation.CurrentUser))
			{
				store.Open(OpenFlags.ReadWrite);
				store.Remove(cert);
				store.Close();
			}

			using (var appManagerClient = AppManagerClientFactory.GetNewAppManager())
			{
				AssertEquals(
					AppManagerResultStatus.Success,
					appManagerClient.Invoke(
						typeof(X509CertificateManager).Assembly.Location,
						typeof(X509CertificateManager).FullName,
						new object[] { cert, "remove" },
						MutexRequest.Create("RemoveX509Certificate2")).Status);
			}
		}

		public void TestEncryptDecrypt()
		{
			// test comment

			string initialString = "Hello";
			string certificateFileName = resourceRetriever.SaveResourceToFile("EncryptionCert.crt");
			using (var encryptionCertificate = new Certificate(certificateFileName))
			{
				string storeFileName = resourceRetriever.SaveResourceToFile("SEDI_Test_EncryptionKeyPair.p12");
				using (var decryptionCertificateStore = new Store(storeFileName, "4Customs&SEDI"))
				{
					string envelopedContents = encryptionCertificate.Encrypt(initialString);
					FunctionsTest.AssertSplitToLines(envelopedContents);

					var decryptedString = decryptionCertificateStore.Decrypt(envelopedContents);
					AssertEquals("DecryptedContents", initialString, decryptedString);
				}
			}
		}

		public void TestDecryptOnly()
		{
			string initialString = "Hello";

			string envelopedContents = resourceRetriever.GetString("EncryptedData.txt", Encoding.ASCII);
			string storeFileName = resourceRetriever.SaveResourceToFile("SEDI_Test_EncryptionKeyPair.p12");
			using (Store decryptionCertificateStore = new Store(storeFileName, "4Customs&SEDI"))
			{
				AssertEquals("DecryptedContents", initialString, decryptionCertificateStore.Decrypt(envelopedContents));
			}
		}

		#endregion

		public void TestSign()
		{
			string storeFileName = resourceRetriever.SaveResourceToFile("SEDI_Test_SigningKeyPair.p12");
			using (var mySigningCertificateStore = new Store(storeFileName, "4Customs&SEDI"))
			{
				var signedContents = mySigningCertificateStore.Sign("HELLO");
				FunctionsTest.AssertSplitToLines(signedContents);
				string certificateFileName = resourceRetriever.SaveResourceToFile("acme_ca.crt");
				using (var cACert = new Certificate(certificateFileName))
				{
					AssertEquals("VerifiedContents", "HELLO", cACert.VerifyBytes(signedContents, null).Content);
				}
			}
		}

		public void TestClearSign()
		{
			string storeFileName = resourceRetriever.SaveResourceToFile("SEDI_Test_SigningKeyPair.p12");
			using (var mySigningCertificateStore = new Store(storeFileName, "4Customs&SEDI"))
			{
				var signatureBase64 = mySigningCertificateStore.ClearSign("HELLO");
				FunctionsTest.AssertSplitToLines(signatureBase64);
				string certificateFileName = resourceRetriever.SaveResourceToFile("acme_ca.crt");
				using (var cACert = new Certificate(certificateFileName))
				{
					AssertContains("signature is invalid", AssertExceptionThrown<CryptoUtilitiesException>(() => cACert.VerifyBytes(signatureBase64, null)).Message);
					AssertEquals(true, VerifyDetachedSignature("HELLO", signatureBase64, out var signerCertificate));
					AssertEquals("SEDI Test Keys", signerCertificate.GetNameInfo(X509NameType.SimpleName, false));
					AssertEquals(false, VerifyDetachedSignature("hELLo", signatureBase64, out signerCertificate));
				}
			}
		}

		bool VerifyDetachedSignature(string content, string signatureBase64, out X509Certificate2 signerCertificate)
		{
			var signedCms = new SignedCms(new ContentInfo(Encoding.ASCII.GetBytes(content)), true);
			signedCms.Decode(Convert.FromBase64String(signatureBase64));
			try
			{
				signedCms.CheckSignature(true);
				signerCertificate = signedCms.SignerInfos[0].Certificate;
				return true;
			}
			catch (CryptographicException)
			{
				signerCertificate = null;
				return false;
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			resourceRetriever = new EmbeddedResourceRetriever(GetType().Assembly);
		}

		protected override void TearDown()
		{
			base.TearDown();
			resourceRetriever.Dispose();
		}

		EmbeddedResourceRetriever resourceRetriever;
	}
}
