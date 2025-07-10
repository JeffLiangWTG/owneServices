using CargoWise.IO;
using Enterprise.CryptoUtilities;
using Enterprise.CryptoUtilities.SMIME;

namespace Enterprise.CMREdifact.CryptoUtilities.SMIME.Testing
{
	[NUnit.Framework.DatCapabilityRequirement("SOURCE_CODE")]
	sealed class SignedSMIMEMesageTest : NUnit.Framework.TestCase
	{
		public void TestSignedSMIMEMesage()
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
				var innerMessage = new SingleAttachmentMIMEMessage("msg.txt", new byte[5] { 0, 1, 2, 3, 4 }, "ContentType", "ContentDisposition");
				var p12Filename = resourceRetriever.SaveResourceToFile("customs_au_business.SEDI_Test_SigningKeyPair.p12");
				signingStore = new Store(p12Filename, "4Customs&SEDI");
				myMessage = new SignedSMIMEMessage(innerMessage, signingStore);
			}
		}

		protected override void TearDown()
		{
			base.TearDown();
			signingStore.Dispose();
		}
		Store signingStore;
		SignedSMIMEMessage myMessage;
	}
}
