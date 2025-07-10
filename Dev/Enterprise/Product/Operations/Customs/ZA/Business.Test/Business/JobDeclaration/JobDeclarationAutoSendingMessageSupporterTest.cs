namespace Enterprise.Customs.ZA.Business.Testing
{
	sealed class JobDeclarationAutoSendingMessageSupporterTest
		: Customs.Business.Testing.JobDeclarationMessageSupporterTest<JobDeclaration>
	{
		public override void TestIJobDeclarationMessageSupporterMembers()
		{
			var declaration = Factory.New<JobDeclaration>();
			var supporter = (Integration.Customs.IJobDeclarationAutoSendingMessageSupporter)declaration;

			Assert(supporter.SupportEntryDeclarationMessage);
			Assert(!supporter.SupportReleaseMessage);
			AssertNotNull(supporter.CreateEntryDeclarationMessageProcessor());
			AssertType<ZAAutoSendCustomsMessageProcessor>(supporter.CreateEntryDeclarationMessageProcessor());
		}
	}
}
