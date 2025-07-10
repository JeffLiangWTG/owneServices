using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class RequestToExtendTIBMessageBuilderTest : TestCaseWithFactory
	{
		public void TestGenerateMessages()
		{
			DeclarationTestHelper.SetEntryFilerCode("XJ5");
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryType = EntryTypeList.Codes.TemporaryImportationBond;
			declaration.US_EnableCRL = true;
			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;

			declaration.US_SchDEntry = "8888";
			entry.EntryNumber = "432987234";

			new RequestToExtendTIBMessageBuilder(entry, ImportMessageSendingMessageType.ExtendTIB).GenerateMessages();
			var result = (MQEDIMessage)entry.Messages[0];
			AssertEquals("message generated", "B      XJ5TE                                               <<MSGNO PLACEHOLDER>>XA8888XJ54329872341                                                             Y      XJ5TE", result.EM_MessageText);
		}
	}
}
