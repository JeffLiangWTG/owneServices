using Enterprise.Customs.Common.US;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class JobDeclarationFormalEntryMessageManagerTest : JobDeclarationMessageManagerTest
	{
		public void TestSendOriginalMessagesDoesNotSave()
		{
			declaration.Invoices.AddNew().JobComInvoiceLines.AddNew();
			declaration.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
			declaration.DoMerge();

			var actions = new ImportMessageSendingActionCollection(declaration, ImportMessageSendingMessageType.Original);
			actions[0].US_SendMessage = true;
			actions.SendMessagesWithoutSaving(declaration.MessageInitiator);

			var entry = declaration.CustomsEntryHeaders[0];
			entry.CH_Status = ImportMessageStatusList.Codes.ClearEntrySummaryOriginal;

			AssertEquals("isActive", true, entry.IsActive);
			AssertEquals(1, declaration.CustomsEntryHeaders.Count);
			AssertEquals(1, declaration.ActiveEntryHeaders.Count);

			//Factory.Save gets called in the place that triggers message building so that user notification is done after save succeeds
			AssertEquals(false, entry.IsInDatabase);
		}

		public void TestDoNotRunPreSaveValidationWhenSendingAMessage()
		{
			using (declaration.SuspendValidationTesting())
			{
				declaration.JE_TransportMode = "SEA";
				declaration.Invoices.AddNew().JobComInvoiceLines.AddNew();
				declaration.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
				declaration.DoMerge();

				AssertHasMessageErrorContaining(declaration.JE_VesselNameInfo, ValidationConstants.Declaration.VesselRequired);
				declaration.JE_VesselNameInfo.ClearAllNotifications();

				var actions = new ImportMessageSendingActionCollection(declaration, ImportMessageSendingMessageType.Original);
				actions[0].US_SendMessage = true;
				actions.SendMessagesWithoutSaving(declaration.MessageInitiator);

				AssertNoMessageErrors(declaration.JE_VesselNameInfo);
			}
		}

		protected override JobDeclaration GetNewJobDeclaration()
		{
			var result = base.GetNewJobDeclaration();
			result.JE_MessageType = JobMessageTypeList.Codes.Import;
			result.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			result.US_EnableENS = true;
			return result;
		}

		protected override JobDeclarationMessageManager GetNewJobDeclarationMessageManager(JobDeclaration declaration)
			=> new JobDeclarationImportMessageManager(declaration, new ImportMessageSendingActionCollection(declaration, ImportMessageSendingMessageType.Original));

		protected override void SetUp()
		{
			base.SetUp();
			DeclarationTestHelper.SetEntryFilerCode("XJ5");
		}
	}
}
