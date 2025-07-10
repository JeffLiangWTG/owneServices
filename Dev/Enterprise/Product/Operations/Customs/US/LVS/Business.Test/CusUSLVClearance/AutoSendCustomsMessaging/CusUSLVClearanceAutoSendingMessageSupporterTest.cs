using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business.BatchProcessor;
using static Enterprise.Integration.Customs;

namespace Enterprise.Customs.US.LVS.Business.Testing
{
	class CusUSLVClearanceAutoSendingMessageSupporterTest : TestCaseWithFactory
	{
		public void TestIJobDeclarationAutoSendingMessageSupporterMembers()
		{
			var clearance = Factory.New<CusUSLVClearance>();
			var supporter = (IJobDeclarationAutoSendingMessageSupporter)clearance;
			AssertEquals("SEM is not supported in LVS", false, supporter.SupportEntryDeclarationMessage);
			AssertEquals("SRM is supported in LVS", true, supporter.SupportReleaseMessage);
			AssertEquals("GetReasonForNotSupportEntryDeclarationMessage", "Send Entry/Declaration Message trigger action is not supported for Low Value Entries.", supporter.GetReasonForNotSupportEntryDeclarationMessage);
			AssertEquals("GetReasonForNotSupportReleaseMessage", "", supporter.GetReasonForNotSupportReleaseMessage);
			AssertNull("SEM is not supported in LVS, process should be null", supporter.CreateEntryDeclarationMessageProcessor());
			AssertType<LVSAutoSendCargoReleaseMessageProcessor>(supporter.CreateReleaseMessageProcessor());
			AssertType<CustomsStmProcessQueueCreatorProcessor>(supporter.CreateStmProcessQueueProcessor(null, "~T~"));
		}
	}
}
