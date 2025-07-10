using CargoWise.IO;
using Enterprise.CryptoUtilities;
using Enterprise.CryptoUtilities.SMIME;

namespace Enterprise.CMREdifact.CryptoUtilities.SMIME.Testing
{
	[NUnit.Framework.DatCapabilityRequirement("SOURCE_CODE")]
	sealed class ClearSignedSMIMEMessageTest : NUnit.Framework.TestCase
	{
		public void TestClearSignedSMIMEMesage()
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
				var storeFilename = resourceRetriever.SaveResourceToFile("SEDI_Test_EncryptionKeyPair.p12");
				store = new Store(storeFilename, "4Customs&SEDI");
				myMessage = new ClearSignedSMIMEMessage("HELLO", store);
			}
		}

		protected override void TearDown()
		{
			base.TearDown();
			store.Dispose();
		}

		ClearSignedSMIMEMessage myMessage;
		Store store;
	}
}
