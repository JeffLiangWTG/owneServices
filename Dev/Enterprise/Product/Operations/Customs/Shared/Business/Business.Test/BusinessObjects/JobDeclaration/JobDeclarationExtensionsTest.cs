using CargoWise.EntityFramework.Testing;
using Enterprise.UniversalDataBuss.Management;

namespace Enterprise.Customs.Business.JobDeclarationExtensions.Testing
{
	sealed class JobDeclarationExtensionsTest : TestCaseWithFactory
	{
		public void TestIsPublishToUniversalTransactionOK()
		{
			var messageInitiator = new SendsMessagesToCustomsShutterUpperer(false);
			messageInitiator.IsPublishToUniversalTransactionOK(PublishToUniversalResult.New("error", "more error"));
			AssertEquals("error\r\nmore error", messageInitiator.InvalidOperationText);
		}

		public void TestIsPublishToUniversalTransactionOK_null()
		{
			var messageInitiator = new SendsMessagesToCustomsShutterUpperer(false);
			var result = messageInitiator.IsPublishToUniversalTransactionOK(null);
			AssertEquals(expected: false, result);
			AssertEquals("Something went wrong during creation of the transaction", messageInitiator.InvalidOperationText);
		}
	}
}
