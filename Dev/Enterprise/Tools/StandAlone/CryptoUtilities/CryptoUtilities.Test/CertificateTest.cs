using System;
using System.Linq;
using System.Text;
using CargoWise.IO;
using Enterprise.CryptoUtilities;

namespace Enterprise.CMREdifact.CryptoUtilities.Testing
{
	[NUnit.Framework.DatCapabilityRequirement("SOURCE_CODE")]
	sealed class CertificateTest : NUnit.Framework.TestCase
	{
		public void TestConstruct()
		{
			string certificateFileName = resourceRetriever.SaveResourceToFile("Conf.cer");
			using (var cert = new Certificate(certificateFileName))
			{
				AssertNotNull("Certificate", cert);
			}
		}

		public void TestConstructFromBadDataFile()
		{
			try
			{
				string certificateFileName = resourceRetriever.SaveResourceToFile("EagleDI_SEDI_R1_TRIAL_Conf.p12");
				using (var cert = new Certificate(certificateFileName))
				{
					Fail("Certificate construction should have failed.");
				}
			}
			catch (CryptoUtilitiesException e)
			{
				AssertStartsWith
				(
					"exception message",
					"The supplied file does not appear to be a valid certificate.",
					e.Message
				);
			}
		}

		public void TestVerifyCertificateContextFromBytes()
		{
			Certificate.VerifyCertificateContextFromBytes(resourceRetriever.GetBytes("Conf.cer"));
			AssertStartsWith
			(
				"exception message",
				"The supplied file does not appear to be a valid certificate.",
				AssertExceptionThrown<CryptoUtilitiesException>(() => Certificate.VerifyCertificateContextFromBytes(resourceRetriever.GetBytes("EagleDI_SEDI_R1_TRIAL_Conf.p12"))).Message
			);
		}

		public void TestDispose()
		{
			string certificateFileName = resourceRetriever.SaveResourceToFile("Conf.cer");
			var cert = new Certificate(certificateFileName);
			using (cert)
			{
				AssertNotEquals("pre-condition", IntPtr.Zero, cert.certificate.Handle);
			}

			AssertEquals(IntPtr.Zero, cert.certificate.Handle);
		}

		public void TestName()
		{
			string certificateFileName = resourceRetriever.SaveResourceToFile("Conf.cer");
			using (var cert = new Certificate(certificateFileName))
			{
				AssertEquals("Name", "Eagle Datamation SEDI", cert.Name);
			}
		}

		public void TestIssuerName()
		{
			string certificateFileName = resourceRetriever.SaveResourceToFile("Conf.cer");
			using (var cert = new Certificate(certificateFileName))
			{
				AssertEquals("Name", "Acme Trust CA", cert.IssuerName);
			}
		}

		public void TestSerialNumber()
		{
			string certificateFileName = resourceRetriever.SaveResourceToFile("Conf.cer");
			using (var cert = new Certificate(certificateFileName))
			{
				AssertEquals("SerialNumber", "3e 8b 8f c2", cert.SerialNumber);
			}
		}

		[NUnit.Framework.TestTimeZone]
		public void TestValidToDate()
		{
			string certificateFileName = resourceRetriever.SaveResourceToFile("Conf.cer");
			using (var cert = new Certificate(certificateFileName))
			{
				AssertEquals("ValidToDate", new DateTime(2004, 4, 2, 2, 34, 58), cert.ValidToDate);
			}
		}

		[NUnit.Framework.TestTimeZone]
		public void TestValidFromDate()
		{
			string certificateFileName = resourceRetriever.SaveResourceToFile("Conf.cer");
			using (var cert = new Certificate(certificateFileName))
			{
				AssertEquals("ValidFromDate", new DateTime(2003, 4, 3, 3, 34, 58), cert.ValidFromDate);
			}
		}

		public void TestEmailAddress()
		{
			string certificateFileName = resourceRetriever.SaveResourceToFile("Conf.cer");
			using (var cert = new Certificate(certificateFileName))
			{
				AssertEquals("EmailAddress", "cmr@acsedi.edi.net.au", cert.EmailAddress);
			}
		}

		[NUnit.Framework.TestTimeZone]
		public void TestGetCertificateState()
		{
			string certificateFileName = resourceRetriever.SaveResourceToFile("Conf.cer");
			using (var cert = new Certificate(certificateFileName))
			{
				var okState = cert.GetCertificateState(cert.ValidToDate.AddDays(-90));
				AssertEquals("okState.Errors", 0, okState.Errors.Length);
				AssertEquals("okState.Warnings", 0, okState.Warnings.Length);

				var errorState = cert.GetCertificateState(cert.ValidToDate.AddDays(2));
				AssertEquals("okState.Errors", "The certificate has expired as of " + new DateTime(2004, 4, 2, 2, 34, 58) + ".", string.Join("\r\n", errorState.Errors));
				AssertEquals("okState.Warnings", 0, errorState.Warnings.Length);
			}
		}

		#region Signing

		public void TestVerifyWithWrongTrustPointCertificate()
		{
			var inputString = resourceRetriever.GetString("signed1.txt", Encoding.ASCII);
			try
			{
				string certificateFileName = resourceRetriever.SaveResourceToFile("trustedcert.cer");
				using (var cert = new Certificate(certificateFileName))
				{
					cert.VerifyBytes(inputString, null);
					Fail("ExceptionShouldHaveBeenThrown");
				}
			}
			catch (CryptoUtilitiesException e)
			{
				AssertStartsWith("ExceptionText", "The documents signing certificate is not trusted by the supplied trust point certificate.", e.Message);
			}
		}

		public void TestVerifyWithWrongSignerName()
		{
			var inputString = resourceRetriever.GetString("signed1.txt", Encoding.ASCII);
			try
			{
				string certificateFileName = resourceRetriever.SaveResourceToFile("acme_ca.crt");
				using (var cert = new Certificate(certificateFileName))
				{
					cert.VerifyBytes(inputString, "Wrong Signer");
					Fail("ExceptionShouldHaveBeenThrown");
				}
			}
			catch (CryptoUtilitiesException e)
			{
				Assert("ExceptionText", e.Message.StartsWith("The documents signer was not the expected signer. Expected Signer = 'Wrong Signer', Actual Signer = 'SEDI Test Keys'."));
			}
		}

		public void TestVerifyInvalidSignature()
		{
			var inputString = resourceRetriever.GetString("signed1.txt", Encoding.ASCII);
			var message = Convert.FromBase64String(inputString);

			var expectedText = Encoding.ASCII.GetBytes("qwertyuiop");
			var messageTextOffset = Enumerable.Range(0, message.Length - expectedText.Length + 1).Single(ofs =>
			{
				for (int i = 0; i < expectedText.Length; i++)
				{
					if (message[ofs + i] != expectedText[i])
					{
						return false;
					}
				}

				return true;
			});

			message[messageTextOffset + 3] = (byte)'R';
			inputString = Convert.ToBase64String(message);

			string certificateFileName = resourceRetriever.SaveResourceToFile("acme_ca.crt");
			using (Certificate cert = new Certificate(certificateFileName))
			{
				AssertContains(
					"The hash value is not correct.",
					AssertExceptionThrown<CryptoUtilitiesException>(() => cert.VerifyBytes(inputString, null)).Message
				);
			}
		}

		public void TestVerify()
		{
			string inputString = resourceRetriever.GetString("signed1.txt", Encoding.ASCII);
			string inputString2 = resourceRetriever.GetString("signed1.results", Encoding.UTF8);

			string certificateFileName = resourceRetriever.SaveResourceToFile("acme_ca.crt");
			using (Certificate cert = new Certificate(certificateFileName))
			{
				AssertEquals("VerifiedMessage", inputString2, cert.VerifyBytes(inputString, null).Content);
			}
		}

		#endregion

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
