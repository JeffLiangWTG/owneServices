using CargoWise.EntityFramework.Testing;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.TR.Business.Testing
{
	class TRSPTSAutoReceiveResponseMessageProviderTest : TestCaseWithFactory
	{
		public void TestIMessageMembers()
		{
			var manifestHeader = Factory.New<Integration.Customs.TR.ICusInBondSPTSHeader>();
			manifestHeader.BH_JobReference = "MAN0000001";
			var message = (EDIMessage)manifestHeader.Messages.AddNew();
			message.EM_MessageNum = "TEST001";
			var provider = new TRSPTSAutoReceiveResponseMessageProvider(manifestHeader);
			CombineAssertions(() =>
			{
				AssertEquals("Parent", manifestHeader, provider.Parent);
				AssertEquals("Messages.Count", 1, provider.Messages.Count);
				AssertEquals("Messages[0].EM_MessageNum", "TEST001", ((EDIMessage)provider.Messages[0]).EM_MessageNum);
				AssertEquals("JobReference", "MAN0000001", provider.JobReference);
			});
		}
	}
}
