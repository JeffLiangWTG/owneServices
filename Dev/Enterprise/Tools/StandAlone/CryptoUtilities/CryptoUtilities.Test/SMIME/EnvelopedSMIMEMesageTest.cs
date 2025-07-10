using CargoWise.IO;
using Enterprise.CryptoUtilities;
using Enterprise.CryptoUtilities.SMIME;

namespace Enterprise.CMREdifact.CryptoUtilities.SMIME.Testing
{
	[NUnit.Framework.DatCapabilityRequirement("SOURCE_CODE")]
	sealed class EnvelopedSMIMEMesageTest : NUnit.Framework.TestCase
	{
		public void TestEnvelopedSMIMEMesage()
		{
			AssertNotNull("MyMessage", myMessage);
		}

		public void TestRawMIMEString()
		{
			AssertNoExceptionThrown(() => _ = myMessage.RawMIMEString);
		}

		protected override void SetUp()
		{
			base.SetUp();
			using (var resourceRetriever = new EmbeddedResourceRetriever(GetType().Assembly))
			{
				SingleAttachmentMIMEMessage innerMessage = new SingleAttachmentMIMEMessage("msg.txt", new byte[5] { 0, 1, 2, 3, 4 }, "ContentType", "ContentDisposition");
				var crtFilename = resourceRetriever.SaveResourceToFile("customs_au_business.SigningCert.crt");
				encryptionCertificate = new Certificate(crtFilename);
				myMessage = new EnvelopedSMIMEMessage(innerMessage, encryptionCertificate);
			}
		}

		protected override void TearDown()
		{
			base.TearDown();
			encryptionCertificate.Dispose();
		}

		EnvelopedSMIMEMessage myMessage;
		Certificate encryptionCertificate;
	}
}
