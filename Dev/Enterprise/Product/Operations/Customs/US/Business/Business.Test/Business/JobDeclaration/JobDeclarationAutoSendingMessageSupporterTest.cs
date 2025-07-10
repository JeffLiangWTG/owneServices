namespace Enterprise.Customs.US.Business.Testing
{
	sealed class JobDeclarationAutoSendingMessageSupporterTest : Customs.Business.Testing.JobDeclarationMessageSupporterTest<JobDeclaration>
	{
		public override void TestIJobDeclarationMessageSupporterMembers()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EnableENS = false;
			declaration.US_EnableCRL = false;
			var supporter = (Integration.Customs.IJobDeclarationAutoSendingMessageSupporter)declaration;
			AssertEquals(false, supporter.SupportEntryDeclarationMessage);
			AssertEquals(JobDeclaration.SendEntryDeclarationTriggerForNonSupportedImportMessage, supporter.GetReasonForNotSupportEntryDeclarationMessage);
			AssertNull(supporter.CreateEntryDeclarationMessageProcessor());
			AssertEquals(false, supporter.SupportReleaseMessage);
			AssertEquals(JobDeclaration.SendReleaseTriggerNotSupportedMessage, supporter.GetReasonForNotSupportReleaseMessage);
			AssertNull(supporter.CreateReleaseMessageProcessor());

			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			AssertEquals(false, supporter.SupportEntryDeclarationMessage);
			AssertEquals(JobDeclaration.SendEntryDeclarationTriggerForNonSupportedImportMessage, supporter.GetReasonForNotSupportEntryDeclarationMessage);
			AssertNull(supporter.CreateEntryDeclarationMessageProcessor());
			AssertEquals(false, supporter.SupportReleaseMessage);
			AssertEquals(JobDeclaration.SendReleaseTriggerNotSupportedMessage, supporter.GetReasonForNotSupportReleaseMessage);
			AssertNull(supporter.CreateReleaseMessageProcessor());

			declaration.US_EnableENS = true;
			AssertEquals(true, supporter.SupportEntryDeclarationMessage);
			AssertEquals(string.Empty, supporter.GetReasonForNotSupportEntryDeclarationMessage);
			AssertNotNull(supporter.CreateEntryDeclarationMessageProcessor());
			AssertEquals(typeof(AutoSendEntrySummaryMessageProcessor), supporter.CreateEntryDeclarationMessageProcessor().GetType());
			AssertEquals(false, supporter.SupportReleaseMessage);
			AssertEquals(JobDeclaration.SendReleaseTriggerNotSupportedMessage, supporter.GetReasonForNotSupportReleaseMessage);
			AssertNull(supporter.CreateReleaseMessageProcessor());

			declaration.US_EnableCRL = true;
			AssertEquals(true, supporter.SupportReleaseMessage);
			AssertEquals(string.Empty, supporter.GetReasonForNotSupportReleaseMessage);
			AssertNotNull(supporter.CreateReleaseMessageProcessor());
			AssertEquals(typeof(AutoSendCargoReleaseMessageProcessor), supporter.CreateReleaseMessageProcessor().GetType());

			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			AssertEquals(true, supporter.SupportEntryDeclarationMessage);
			AssertEquals(string.Empty, supporter.GetReasonForNotSupportEntryDeclarationMessage);
			AssertNotNull(supporter.CreateEntryDeclarationMessageProcessor());
			AssertEquals(typeof(AutoSendAESTIRMessageProcessor), supporter.CreateEntryDeclarationMessageProcessor().GetType());
			AssertEquals(false, supporter.SupportReleaseMessage);
			AssertEquals(JobDeclaration.SendReleaseTriggerNotSupportedMessage, supporter.GetReasonForNotSupportReleaseMessage);
			AssertNull(supporter.CreateReleaseMessageProcessor());

			declaration.JE_MessageType = JobMessageTypeList.Codes.Drawback;
			AssertEquals(false, supporter.SupportEntryDeclarationMessage);
			AssertEquals(string.Format(JobDeclaration.SendEntryDeclarationTriggerForNonSupportedShipmentTypeMessage, JobMessageTypeList.Codes.Drawback), supporter.GetReasonForNotSupportEntryDeclarationMessage);
			AssertNull(supporter.CreateEntryDeclarationMessageProcessor());
			AssertEquals(false, supporter.SupportReleaseMessage);
			AssertEquals(JobDeclaration.SendReleaseTriggerNotSupportedMessage, supporter.GetReasonForNotSupportReleaseMessage);
			AssertNull(supporter.CreateReleaseMessageProcessor());

			declaration.JE_MessageType = JobMessageTypeList.Codes.Recon;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			var reconDeclaration = ReconDeclaration.Get(declaration);
			AssertEquals(true, supporter.SupportEntryDeclarationMessage);
			AssertEquals(string.Format(JobDeclaration.SendEntryDeclarationTriggerForNonSupportedShipmentTypeMessage, JobMessageTypeList.Codes.Recon), supporter.GetReasonForNotSupportEntryDeclarationMessage);
			AssertNotNull(supporter.CreateEntryDeclarationMessageProcessor());
			AssertEquals(false, supporter.SupportReleaseMessage);
			AssertEquals(JobDeclaration.SendReleaseTriggerNotSupportedMessage, supporter.GetReasonForNotSupportReleaseMessage);
			AssertNull(supporter.CreateReleaseMessageProcessor());
		}
	}
}
