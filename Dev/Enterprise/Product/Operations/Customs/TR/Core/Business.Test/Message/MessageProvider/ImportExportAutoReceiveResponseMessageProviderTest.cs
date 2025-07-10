using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.TR.Business.Declaration;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.TR.Business.Testing
{
	class ImportExportAutoReceiveResponseMessageProviderTest : TestCaseWithFactory
	{
		public void TestIMessageSenderMembers()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_DeclarationReference = "JE1111";
			var cusEntryHeader = declaration.CustomsEntryHeaders.AddNew();
			cusEntryHeader.EntryNumber = "ENT001000";
			var message = cusEntryHeader.Messages.AddNew();
			message.EM_MessageNum = "TEST001";
			var provider = new DeclarationAutoReceiveResponseMessageProvider(cusEntryHeader);
			CombineAssertions(() =>
			{
				AssertSame(cusEntryHeader, provider.Parent);
				AssertEquals("Messages.Count", 1, provider.Messages.Count);
				AssertEquals("Messages[0].EM_MessageNum", "TEST001", ((EDIMessage)provider.Messages[0]).EM_MessageNum);
				AssertEquals("JobReference", "ULU-JE1111", provider.JobReference);
			});
		}
	}
}
