using CargoWise.EntityFramework;
using Enterprise.Customs.TW.Business;
using Enterprise.Customs.TW.Manifest.MessageProcessors;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Manifest.Business.Testing
{
	sealed class TWManifestMessageHelperTest : TransactionedTestCase
	{
		public void TestNewOutgoingHelper()
		{
			var factory = new BusinessObjectFactory();
			var testMessage = factory.NewWithValidTestData<AsycudaMessage>();
			testMessage.EM_MessageType = MessageTypeList.Codes.FHM;
			AssertType<N5101HMessageHelper>(TWManifestMessageHelper.NewOutgoingHelper(testMessage));
			testMessage.EM_MessageType = "AMD";
			AssertNull(TWManifestMessageHelper.NewOutgoingHelper(testMessage));
		}
	}
}
