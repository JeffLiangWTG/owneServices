using CargoWise.EntityFramework.Testing;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.TR.Business.Testing
{
	class ETradeAutoReceiveResponseMessageProviderTest : TestCaseWithFactory
	{
		public void TestIMessageSenderMembers()
		{
			var header = Factory.New<Integration.Customs.ASYCUDA.TRETrade.IAsycudaManifestHeader>();
			header.AMA_JobReference = "ETG0000001";
			var message = (EDIMessage)header.Messages.AddNew();
			message.EM_MessageNum = "TEST001";
			var provider = new ETradeAutoReceiveResponseMessageProvider(header);
			CombineAssertions(() =>
			{
				AssertEquals("Parent", header, provider.Parent);
				AssertEquals("Messages.Count", 1, provider.Messages.Count);
				AssertEquals("Messages[0].EM_MessageNum", "TEST001", ((EDIMessage)provider.Messages[0]).EM_MessageNum);
				AssertEquals("JobReference", "ETG0000001", provider.JobReference);
			});
		}
	}
}
