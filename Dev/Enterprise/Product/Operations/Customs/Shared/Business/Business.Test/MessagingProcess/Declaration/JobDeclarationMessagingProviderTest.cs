using System;
using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.MessagingProcess.Testing;
using Enterprise.Customs.Business.WarehouseExtensions;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Moq;

namespace Enterprise.Customs.Business.MessagingProcess.Declaration.Testing
{
	sealed class JobDeclarationMessagingProviderTest : TestCaseWithFactory
	{
		public void TestRunPreSendValidation()
		{
			var validator = messagingProvider as ISupportPreSendValidation;

			var results = validator.RunPreSendValidation(new ActionResult(true)).Select(x => x.MessageIncludingPrefix).Distinct().ToList();
			AssertEquals("No results", 0, results.Count);

			warehouseAutomation.PrepareForProcessingResultForTesting = true;
			messagingProvider = new JobDeclarationMessagingProvider<JobDecMsgSendingObjParentForTest, JobDecMsgSendingObjForTest>((e, a) => warehouseAutomation, sendingObjParent);
			validator = messagingProvider;

			results = validator.RunPreSendValidation(new ActionResult(true)).Select(x => x.MessageIncludingPrefix).Distinct().ToList();
			AssertEquals("No results", 0, results.Count);

			warehouseAutomation.GetValidationMessageResultForTesting = "Got a Validation error";
			results = validator.RunPreSendValidation(new ActionResult(true)).Select(x => x.MessageIncludingPrefix).Distinct().ToList();
			AssertEquals("1 result", 1, results.Count);

			var msgs = string.Join(",", results);
			AssertContains("WarehouseAutomation error", "Error: Got a Validation error", msgs);

			declaration.JE_MessageType = JobMessageTypeList.Codes.ExWarehouse;
			declaration.SetSupportsBondedWarehousingForTesting(true);
			results = validator.RunPreSendValidation(new ActionResult(true)).Select(x => x.MessageIncludingPrefix).Distinct().ToList();
			AssertEquals("2 result", 2, results.Count);

			msgs = string.Join(",", results);
			AssertContains("bonded warehousing error", "Error: Inventory recording/Inventory Management Integration is active", msgs);
		}

		public void TestConfigureProcess_All()
		{
			using (CustomsDataRegistry.Instance.CreditCheckOnMessageSend.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				var sendProcess = new SendMessagesProcess();
				var messagingChain = sendProcess.SendProcessChain;

				warehouseAutomation.PrepareForProcessingResultForTesting = true;
				sendingObjParent.SetShouldProcessWarehouseForTest(true);
				messagingProvider = new JobDeclarationMessagingProvider<JobDecMsgSendingObjParentForTest, JobDecMsgSendingObjForTest>((e, a) => warehouseAutomation, sendingObjParent);
				var config = messagingProvider as ISupportConfigureProcess;
				CombineAssertions(() =>
				{
					AssertContains("Original chain - pre-req", "{ ShowCreditAndDPSCheckOverride: success: { CreateMessages: success: { ShowPreviewDialog: success:", messagingChain.GetChainAsString());
					config.ConfigureProcess(messagingChain);
					AssertContains("Create mmessages chain wrapped in WHS chain - PRE", "{ ShowCreditAndDPSCheckOverride: success: { AutoRateDeclaration: success: { WHSAutomationPreActions: success: { CreateMessages-WHSAutomationPostActions-SubChain: { CreateMessages: success: { ShowPreviewDialog: success:", messagingChain.GetChainAsString());
					AssertContains("Create mmessages chain wrapped in WHS chain - POST", "PreviewDialogFailure: } } } common: { WHSAutomationPostActions: failure: { WHSAutomationRestoreActions: } } } } } } }", messagingChain.GetChainAsString());
				});
			}
		}

		public void TestConfigureProcess_WHSAutomation()
		{
			using (CustomsDataRegistry.Instance.CreditCheckOnMessageSend.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
			{
				var sendProcess = new SendMessagesProcess();
				var messagingChain = sendProcess.SendProcessChain;
				sendingObjParent.SetShouldProcessWarehouseForTest(true);
				warehouseAutomation.PrepareForProcessingResultForTesting = false;
				messagingProvider = new JobDeclarationMessagingProvider<JobDecMsgSendingObjParentForTest, JobDecMsgSendingObjForTest>((e, a) => warehouseAutomation, sendingObjParent);
				var config = messagingProvider as ISupportConfigureProcess;
				CombineAssertions(() =>
				{
					AssertContains("Original chain - pre-req", "{ ShowCreditAndDPSCheckOverride: success: { CreateMessages: success: { ShowPreviewDialog: success:", messagingChain.GetChainAsString());

					config.ConfigureProcess(messagingChain);
					AssertContains("Prep:False ShouldProcess:True - No Change", "{ ShowCreditAndDPSCheckOverride: success: { CreateMessages: success: { ShowPreviewDialog: success:", messagingChain.GetChainAsString());

					warehouseAutomation.PrepareForProcessingResultForTesting = true;
					sendingObjParent.SetShouldProcessWarehouseForTest(false);
					config.ConfigureProcess(messagingChain);
					AssertContains("Prep:True ShouldProcess:False - No Change", "{ ShowCreditAndDPSCheckOverride: success: { CreateMessages: success: { ShowPreviewDialog: success:", messagingChain.GetChainAsString());

					sendingObjParent.SetShouldProcessWarehouseForTest(true);

					messagingProvider = new JobDeclarationMessagingProvider<JobDecMsgSendingObjParentForTest, JobDecMsgSendingObjForTest>((e, a) => warehouseAutomation, sendingObjParent);
					config = messagingProvider;
					config.ConfigureProcess(messagingChain);
					AssertContains("Create mmessages chain wrapped in WHS chain - PRE", "{ ShowCreditAndDPSCheckOverride: success: { WHSAutomationPreActions: success: { CreateMessages-WHSAutomationPostActions-SubChain: { CreateMessages: success: { ShowPreviewDialog: success:", messagingChain.GetChainAsString());
					AssertContains("Create mmessages chain wrapped in WHS chain - POST", "PreviewDialogFailure: } } } common: { WHSAutomationPostActions: failure: { WHSAutomationRestoreActions: } } } } } } }", messagingChain.GetChainAsString());
				});
			}
		}

		public void TestConfigureProcess_AutoRating()
		{
			var sendProcess = new SendMessagesProcess();
			var messagingChain = sendProcess.SendProcessChain;

			using (CustomsDataRegistry.Instance.CreditCheckOnMessageSend.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
			{
				messagingProvider = new JobDeclarationMessagingProvider<JobDecMsgSendingObjParentForTest, JobDecMsgSendingObjForTest>((e, a) => warehouseAutomation, sendingObjParent);
				var config = messagingProvider as ISupportConfigureProcess;
				CombineAssertions(() =>
				{
					AssertContains("Original chain", "{ ShowCreditAndDPSCheckOverride: success: { CreateMessages: ", messagingChain.GetChainAsString());
					config.ConfigureProcess(messagingChain);
					AssertContains("Auto-rating step excluded", "{ ShowCreditAndDPSCheckOverride: success: { CreateMessages: ", messagingChain.GetChainAsString());
				});
			}

			using (CustomsDataRegistry.Instance.CreditCheckOnMessageSend.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				messagingProvider = new JobDeclarationMessagingProvider<JobDecMsgSendingObjParentForTest, JobDecMsgSendingObjForTest>((e, a) => warehouseAutomation, sendingObjParent);
				var config = messagingProvider as ISupportConfigureProcess;
				CombineAssertions(() =>
				{
					AssertContains("Original chain", "{ ShowCreditAndDPSCheckOverride: success: { CreateMessages: ", messagingChain.GetChainAsString());
					config.ConfigureProcess(messagingChain);
					AssertContains("Auto-rating step included", "{ ShowCreditAndDPSCheckOverride: success: { AutoRateDeclaration: success: { CreateMessages: ", messagingChain.GetChainAsString());
				});
			}
		}
		public void TestRunWarehouseAutomationValidation()
		{
			warehouseAutomation.PrepareForProcessingResultForTesting = true;
			messagingProvider = new JobDeclarationMessagingProvider<JobDecMsgSendingObjParentForTest, JobDecMsgSendingObjForTest>((e, a) => warehouseAutomation, sendingObjParent);

			var results = messagingProvider.RunWarehouseAutomationValidation().ToList();
			AssertEquals("No results", 0, results.Count);

			warehouseAutomation.GetValidationMessageResultForTesting = "Got a Validation error";
			var validator = messagingProvider as ISupportPreSendValidation;
			results = validator.RunPreSendValidation(new ActionResult(true)).ToList();
			AssertEquals("1 result", 1, results.Count);
			AssertEquals("Error notification", "Error: Got a Validation error", results[0].MessageIncludingPrefix.ToString());
		}

		public void TestCreateWarehouseAutomationChain()
		{
			var warehouseChain = messagingProvider.CreateWarehouseAutomationChain();
			AssertEquals("WareHouse Chain", "{ WHSAutomationPreActions: success: { WHSAutomationPostActions: failure: { WHSAutomationRestoreActions: } } }", warehouseChain.GetChainAsString());
		}

		public void TestWarehouseAutomationPreActions()
		{
			var whsAutoList = messagingProvider.warehouseAutomations.Cast<JobDecWarehouseAutomationForTesting>().ToList();
			foreach (var whsAuto in whsAutoList)
			{
				whsAuto.PreActionResultForTesting = true;
				whsAuto.PreActionExecuted = false;
			}
			var result = messagingProvider.WarehouseAutomationPreActions(new ActionResult(false));

			AssertEquals("No actions should be called successfully", false, result.Success);
			foreach (var whsAuto in whsAutoList)
			{
				AssertEquals("Pre-action not run", false, whsAuto.PreActionExecuted);
			}
			result = messagingProvider.WarehouseAutomationPreActions(new ActionResult(true));

			AssertEquals("All actions should be called successfully", true, result.Success);
			foreach (var whsAuto in whsAutoList)
			{
				AssertEquals("Pre-action run", true, whsAuto.PreActionExecuted);
			}

			foreach (var whsAuto in whsAutoList)
			{
				whsAuto.PreActionResultForTesting = whsAuto != whsAutoList[0];
				whsAuto.PreActionExecuted = false;
			}

			result = messagingProvider.WarehouseAutomationPreActions(new ActionResult(true));

			AssertEquals("First action called 'failed'", false, result.Success);
			AssertEquals("First action executed", true, whsAutoList[0].PreActionExecuted);
			AssertEquals("2nd action did not execute", false, whsAutoList[1].PreActionExecuted);
		}

		public void TestWarehouseAutomationsPostActions()
		{
			var whsAutoList = messagingProvider.warehouseAutomations.Cast<JobDecWarehouseAutomationForTesting>().ToList();
			foreach (var whsAuto in whsAutoList)
			{
				whsAuto.PostActionExecuted = false;
			}
			var result = messagingProvider.WarehouseAutomationPostActions(new ActionResult(false));
			AssertEquals("Result passed in should not change", false, result.Success);
			foreach (var whsAuto in whsAutoList)
			{
				AssertEquals("Post-action run for success", true, whsAuto.PostActionExecuted);
				whsAuto.PostActionExecuted = false;
			}
			result = messagingProvider.WarehouseAutomationPostActions(new ActionResult(true));
			AssertEquals("Result passed in should not change", true, result.Success);
			foreach (var whsAuto in whsAutoList)
			{
				AssertEquals("Post-action run for failure", true, whsAuto.PostActionExecuted);
			}
		}

		public void TestWarehouseAutomationRestoreActions()
		{
			var whsAutoList = messagingProvider.warehouseAutomations.Cast<JobDecWarehouseAutomationForTesting>().ToList();
			foreach (var whsAuto in whsAutoList)
			{
				whsAuto.RestoreActionExecuted = false;
			}
			var result = messagingProvider.WarehouseAutomationRestoreActions(new ActionResult(true));
			AssertEquals("Result passed in should not change", true, result.Success);
			foreach (var whsAuto in whsAutoList)
			{
				AssertEquals("Restore not run", false, whsAuto.RestoreActionExecuted);
			}
			result = messagingProvider.WarehouseAutomationRestoreActions(new ActionResult(false));
			AssertEquals("Result passed in should not change", false, result.Success);
			foreach (var whsAuto in whsAutoList)
			{
				AssertEquals("Restore run", true, whsAuto.RestoreActionExecuted);
			}
		}

		public void TestSetupWarehouseAutomation()
		{
			var sendingObjParent2 = new JobDecMsgSendingObjParentForTest(declaration);
			var messagingProvider2 = new JobDeclarationMessagingProvider<JobDecMsgSendingObjParentForTest, JobDecMsgSendingObjForTest>(GetTestJobDeclarationBondedWarehouseAutomation, sendingObjParent2);

			var whsAutomationList = messagingProvider2.SetupWarehouseAutomation();
			AssertEquals("Only 2 WHS Entries", 2, whsAutomationList.Count);
			AssertSame("1st Entry", entryHeader1, (whsAutomationList[0] as JobDecWarehouseAutomationForTesting).Header);
			AssertSame("3rd Entry", entryHeader3, (whsAutomationList[1] as JobDecWarehouseAutomationForTesting).Header);
		}

		public void TestGetJobDeclarationBondedWarehouseAutomation()
		{
			var whsAutomation = JobDeclarationMessagingProvider<JobDecMsgSendingObjParentForTest, JobDecMsgSendingObjForTest>.GetJobDeclarationBondedWarehouseAutomation(entryHeader1, MessageAction.Original);

			AssertType<JobDeclarationBondedWarehouseAutomation>("whsAutomation Type", whsAutomation);
		}

		public void TestSendMessagesSecurityCheckpoint()
		{
			AssertNull("No SecurityCheckpoint", (messagingProvider as ISupportSecurityCheckpoints).SendMessagesSecurityCheckpoint);
		}

		public void TestSendMessagesWithErrorsSecurityCheckpoint()
		{
			AssertSame("Customs Dec Security Checkpoint", Env.Security.CustomsDeclarationSendWithMessageErrors, (messagingProvider as ISupportSecurityCheckpoints).SendMessagesWithErrorsSecurityCheckpoint);
		}

		public void TestSendMessagesWithErrorsOverrideSecurityCheckpoint()
		{
			AssertSame("AllowMessageErrors", Env.Security.AllowMessageErrors, (messagingProvider as ISupportSecurityCheckpoints).SendMessagesWithErrorsOverrideSecurityCheckpoint);
		}

		public void TestICommonJobDeclarationProvider()
		{
			CombineAssertions("JobDeclarationMessagingProvider Composition", () =>
			{
				var jobDecProvider = messagingProvider as ICommonJobDeclarationProvider;
				AssertNotNull("ICommonJobDeclarationProvider", jobDecProvider);
				var validationProvider = messagingProvider as ISupportPreSendValidation;
				AssertNotNull("ISupportPreSendValidation", validationProvider);
				var configProvider = messagingProvider as ISupportConfigureProcess;
				AssertNotNull("ISupportConfigureProcess", configProvider);
				var securityProvider = messagingProvider as ISupportSecurityCheckpoints;
				AssertNotNull("ISupportSecurityCheckpoints", securityProvider);
				var creditAndDPSCheckProvider = messagingProvider as ISupportCreditAndDPSCheck;
				AssertNotNull("ISupportCreditAndDPSCheck", creditAndDPSCheckProvider);
			});
		}

		public void TestAutoRating()
		{
			using (CustomsDataRegistry.Instance.CreditCheckOnMessageSend.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
			{
				messagingProvider = new JobDeclarationMessagingProvider<JobDecMsgSendingObjParentForTest, JobDecMsgSendingObjForTest>((e, a) => warehouseAutomation, sendingObjParent);
				CombineAssertions("Credit Check disabled", () =>
				{
					var result = messagingProvider.AutoRateDeclaration(new ActionResult(true));
					AssertEquals("Success", true, result.Success);
					AssertEquals("No notification", 0, result.Notifications.Count);
				});
			}

			using (CustomsDataRegistry.Instance.CreditCheckOnMessageSend.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				messagingProvider = new JobDeclarationMessagingProvider<JobDecMsgSendingObjParentForTest, JobDecMsgSendingObjForTest>((e, a) => warehouseAutomation, sendingObjParent);
				CombineAssertions("Credit Check enabled", () =>
				{
					var result = messagingProvider.AutoRateDeclaration(new ActionResult(true));
					AssertEquals("Success, integrsation not supported", true, result.Success);
					AssertEquals("No notification", 0, result.Notifications.Count);

					declaration.SupportAccountingIntegrationForTest = true;

					result = messagingProvider.AutoRateDeclaration(new ActionResult(true));
					AssertEquals("Unsaved declaration causes failure", false, result.Success);
					AssertEquals("1 notification", 1, result.Notifications.Count);
					AssertContains("Exception", "Auto-rating of Customs Disbursement failed:", result.Notifications[0].Message);
					ErrorReporter.Clear();

					declaration.Factory.Save();

					result = messagingProvider.AutoRateDeclaration(new ActionResult(true));
					AssertEquals("Success, no errors", true, result.Success);
					AssertEquals("No notification", 0, result.Notifications.Count);
				});
			}
		}

		public void TestMustRunCreditCheck()
		{
			var mockOptions = new Mock<ISupportCreditAndDPSCheckOptions>();
			mockOptions.Setup(x => x.MustRunCreditCheck).Returns(true);

			using (CustomsDataRegistry.Instance.CreditCheckOnMessageSend.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
			{
				var checkProviderNoOptions = new JobDeclarationMessagingProvider<JobDecMsgSendingObjParentForTest, JobDecMsgSendingObjForTest>((e, a) => warehouseAutomation, sendingObjParent) as ISupportCreditAndDPSCheckOptions;
				var checkProviderWithOptions = new JobDeclarationMessagingProvider<JobDecMsgSendingObjParentForTest, JobDecMsgSendingObjForTest>((e, a) => warehouseAutomation, sendingObjParent, mockOptions.Object) as ISupportCreditAndDPSCheckOptions;

				CombineAssertions("Registry Disabled", () =>
				{
					AssertEquals("No ISupportCreditAndDPSCheckOptions", false, checkProviderNoOptions.MustRunCreditCheck);
					AssertEquals("With ISupportCreditAndDPSCheckOptions", false, checkProviderWithOptions.MustRunCreditCheck);
				});
			}

			using (CustomsDataRegistry.Instance.CreditCheckOnMessageSend.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				var checkProviderNoOptions = new JobDeclarationMessagingProvider<JobDecMsgSendingObjParentForTest, JobDecMsgSendingObjForTest>((e, a) => warehouseAutomation, sendingObjParent) as ISupportCreditAndDPSCheckOptions;
				var checkProviderWithOptions = new JobDeclarationMessagingProvider<JobDecMsgSendingObjParentForTest, JobDecMsgSendingObjForTest>((e, a) => warehouseAutomation, sendingObjParent, mockOptions.Object) as ISupportCreditAndDPSCheckOptions;

				CombineAssertions("Registry Enabled", () =>
				{
					AssertEquals("No ISupportCreditAndDPSCheckOptions", true, checkProviderNoOptions.MustRunCreditCheck);
					AssertEquals("With ISupportCreditAndDPSCheckOptions - true", true, checkProviderWithOptions.MustRunCreditCheck);

					mockOptions.Setup(x => x.MustRunCreditCheck).Returns(false);
					AssertEquals("With ISupportCreditAndDPSCheckOptions - false", false, checkProviderWithOptions.MustRunCreditCheck);
				});
			}
		}

		public void TestCreditRestrictionMessageCaption()
		{
			var mockOptions = new Mock<ISupportCreditAndDPSCheckOptions>();
			mockOptions.Setup(x => x.CreditRestrictionMessageCaption).Returns("Mock Credit Check");

			var declarationCreditMessage = sendingObjParent.ParentDeclaration.CreditRestrictionMessageCaption;

			var checkProviderNoOptions = new JobDeclarationMessagingProvider<JobDecMsgSendingObjParentForTest, JobDecMsgSendingObjForTest>((e, a) => warehouseAutomation, sendingObjParent) as ISupportCreditAndDPSCheckOptions;
			var checkProviderWithOptions = new JobDeclarationMessagingProvider<JobDecMsgSendingObjParentForTest, JobDecMsgSendingObjForTest>((e, a) => warehouseAutomation, sendingObjParent, mockOptions.Object) as ISupportCreditAndDPSCheckOptions;

			CombineAssertions(() =>
			{
				AssertEquals("No ISupportCreditAndDPSCheckOptions", declarationCreditMessage, checkProviderNoOptions.CreditRestrictionMessageCaption);
				AssertEquals("With ISupportCreditAndDPSCheckOptions", "Mock Credit Check", checkProviderWithOptions.CreditRestrictionMessageCaption);
			});
		}

		public void TestDefaultApprovalRequestReason()
		{
			var mockOptions = new Mock<ISupportCreditAndDPSCheckOptions>();
			mockOptions.Setup(x => x.DefaultApprovalRequestReason).Returns("Mock Approval Request Reason");

			var checkProviderNoOptions = new JobDeclarationMessagingProvider<JobDecMsgSendingObjParentForTest, JobDecMsgSendingObjForTest>((e, a) => warehouseAutomation, sendingObjParent) as ISupportCreditAndDPSCheckOptions;
			var checkProviderWithOptions = new JobDeclarationMessagingProvider<JobDecMsgSendingObjParentForTest, JobDecMsgSendingObjForTest>((e, a) => warehouseAutomation, sendingObjParent, mockOptions.Object) as ISupportCreditAndDPSCheckOptions;

			CombineAssertions(() =>
			{
				AssertEquals("No ISupportCreditAndDPSCheckOptions", string.Empty, checkProviderNoOptions.DefaultApprovalRequestReason);
				AssertEquals("With ISupportCreditAndDPSCheckOptions", "Mock Approval Request Reason", checkProviderWithOptions.DefaultApprovalRequestReason);
			});
		}

		public void TestDocumentDeliveryObject()
		{
			var declaration = sendingObjParent.ParentDeclaration;
			var checkProvider = new JobDeclarationMessagingProvider<JobDecMsgSendingObjParentForTest, JobDecMsgSendingObjForTest>((e, a) => warehouseAutomation, sendingObjParent) as ISupportCreditAndDPSCheck;
			AssertSame(declaration, checkProvider.DocumentDeliveryObject);
		}

		protected override void SetUp()
		{
			declaration = Factory.NewWithValidTestData<BaseJobDeclarationForTest>();
			entryHeader1 = declaration.ActiveEntryHeaders.AddNew();
			entryHeader2 = declaration.ActiveEntryHeaders.AddNew();
			entryHeader2.CH_Status = JobDecWarehouseAutomationForTesting.NotForWarehousing;
			entryHeader3 = declaration.ActiveEntryHeaders.AddNew();
			sendingObjParent = new JobDecMsgSendingObjParentForTest(declaration);
			warehouseAutomation = new JobDecWarehouseAutomationForTesting();

			messagingProvider = new JobDeclarationMessagingProvider<JobDecMsgSendingObjParentForTest, JobDecMsgSendingObjForTest>(GetTestJobDeclarationBondedWarehouseAutomation, sendingObjParent);
		}

		IJobDeclarationBondedWarehouseAutomation GetTestJobDeclarationBondedWarehouseAutomation(CusEntryHeader header, MessageAction messageAction) => new JobDecWarehouseAutomationForTesting(header);

		BaseJobDeclarationForTest declaration;
		CusEntryHeader entryHeader1;
		CusEntryHeader entryHeader2;
		CusEntryHeader entryHeader3;
		JobDecMsgSendingObjParentForTest sendingObjParent;
		JobDeclarationMessagingProvider<JobDecMsgSendingObjParentForTest, JobDecMsgSendingObjForTest> messagingProvider;
		JobDecWarehouseAutomationForTesting warehouseAutomation;
	}

	class JobDecMsgSendingObjForTest : JobDeclarationMessageSendingObject, IJobDeclarationSendingObjectWarehouseProvider
	{
		public JobDecMsgSendingObjForTest(CusEntryHeader header) : base(header)
		{
		}

		MessageAction IJobDeclarationSendingObjectWarehouseProvider.GetMessageAction() => MessageAction.Original;

		public bool ShouldProcessWarehouseForTest { get; set; } = true;
		bool IJobDeclarationSendingObjectWarehouseProvider.ShouldProcessWarehouse => ShouldProcessWarehouseForTest;
	}

	class JobDecMsgSendingObjParentForTest : JobDeclarationMessageSendingObjectParent<JobDecMsgSendingObjForTest>
	{
		public JobDecMsgSendingObjParentForTest(BaseJobDeclaration declaration) : base(declaration)
		{
		}

		protected override JobDeclarationMessageSendingObject CreateNewJobDeclarationMessageSendingObject(CusEntryHeader header)
		{
			return new JobDecMsgSendingObjForTest(header);
		}

		public void SetShouldProcessWarehouseForTest(bool value)
		{
			foreach (JobDecMsgSendingObjForTest sendObj in SendingObjectsCollection)
			{
				sendObj.ShouldProcessWarehouseForTest = value;
			}
		}
	}

	class JobDecWarehouseAutomationForTesting : IJobDeclarationBondedWarehouseAutomation
	{
		public JobDecWarehouseAutomationForTesting() { }
		public JobDecWarehouseAutomationForTesting(CusEntryHeader header)
		{
			Header = header;
		}
		public readonly CusEntryHeader Header;

		public bool PreActionResultForTesting { get; set; }
		public bool PreActionExecuted { get; set; }
		public bool ExecutePreAction()
		{
			PreActionExecuted = true;
			return PreActionResultForTesting;
		}

		public bool PostActionExecuted { get; set; }
		public void ExecutePostAction()
		{
			PostActionExecuted = true;
		}

		public bool RestoreActionExecuted { get; set; }
		public void ExecuteRestoreAction()
		{
			RestoreActionExecuted = true;
		}

		public ZString GetValidationMessageResultForTesting { get; set; } = ZString.Empty;
		public ZString GetPendingTransactionError() => GetValidationMessageResultForTesting;

		public bool PrepareForProcessingResultForTesting { get; set; }
		public bool PrepareForProcessing(InventoryAutomationAction inventoryAutomationAction, bool additionalBondedWarehouseRequirement)
		{
			var result = PrepareForProcessingResultForTesting;
			if (Header != null)
			{
				result = Header.CH_Status != NotForWarehousing;
			}

			return result;
		}

		public const string NotForWarehousing = "NFW";
	}

	class BaseJobDeclarationForTest : BaseJobDeclaration
	{
		public BaseJobDeclarationForTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public bool SupportAccountingIntegrationForTest { get; set; }

		protected internal override bool IsIntegrationWithAccountingSupported => SupportAccountingIntegrationForTest;
	}
}
