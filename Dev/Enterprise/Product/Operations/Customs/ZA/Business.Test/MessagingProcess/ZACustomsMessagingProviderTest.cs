using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.MessagingProcess;
using Enterprise.Customs.Business.MessagingProcess.Declaration;
using Enterprise.Customs.Business.MessagingProcess.Testing;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ZA.Business.MessagingProcess.Testing
{
	sealed class ZACustomsMessagingProviderTest : TestCaseWithFactory
	{
		public void TestProviderFactory()
		{
			var declaration = CreateDeclaration(true);

			ICustomsMessagingProviderFactory providerFactory = new ZACustomsMessagingProviderFactory();
			var provider = providerFactory.CreateProvider(declaration);

			AssertType<ZACustomsMessagingProvider>("Provider created", provider);
		}

		public void TestNew()
		{
			var declaration = CreateDeclaration(true);
			var provider = ZACustomsMessagingProvider.New(declaration);

			CombineAssertions(() =>
			{
				AssertSame("Top Level BO", declaration, provider.DeclarationWrapper.ParentDeclaration);
			});
		}

		public void TestIsTestMode()
		{
			var branch = GlbBranch.CurrentBranch;
			var holdTestMode = Env.Registry.ZACustoms.GetIsTestMode(branch);
			try
			{
				AssertTestModeApplied(branch, testMode: true);
				AssertTestModeApplied(branch, testMode: false);
			}
			finally
			{
				Env.Registry.ZACustoms.SetIsTestMode(branch, holdTestMode);
			}
		}
		void AssertTestModeApplied(GlbBranch branch, bool testMode)
		{
			Env.Registry.ZACustoms.SetIsTestMode(branch, testMode);

			var declaration = CreateDeclaration(true);
			ICustomsMessagingProvider provider = ZACustomsMessagingProvider.New(declaration);

			AssertNotNull("supporter should not be null", provider);
			AssertEquals("supporter Test Mode", testMode, provider.IsInTestMode);
			AssertEquals("Test mode validation enabled", true, provider.EnableTestModeValidation);
		}

		public void TestMessengers()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			var instruction1 = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			instruction1.CEI_Style = "11";
			var invoice1 = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice1.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_CEI = instruction1.PK;
			invoiceLine1.JI_Procedure = $"{invoiceLine1.EntryInstruction.CEI_Style}00";

			var instruction2 = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			instruction2.CEI_Style = "22";
			var invoice2 = declaration.Invoices.AddNew();
			var invoiceLine2 = invoice2.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_CEI = instruction2.PK;
			invoiceLine2.JI_Procedure = $"{invoiceLine2.EntryInstruction.CEI_Style}00";

			new LineMerger(declaration).DoMerge();

			ICustomsMessagingProvider provider = ZACustomsMessagingProvider.New(declaration);

			var messengers = provider.GetMessengers().ToList();

			CombineAssertions(() =>
			{
				AssertEquals("2 Messaengers", 2, messengers.Count);
				AssertType<ZACustomsMessenger>("Messenger Type", messengers[0]);
			});
		}

		public void TestConfigureProcess()
		{
			using (CustomsDataRegistry.Instance.CreditCheckOnMessageSend.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
			{
				var declaration = CreateDeclaration(true);
				var provider = ZACustomsMessagingProvider.New(declaration);
				var sendProcess = new SendMessagesProcess();
				var sendChain = sendProcess.SendProcessChain;

				var showSend = sendChain.FindAction(SendMessagesProcess.SendMessageActions.ShowSendDialog);

				CombineAssertions("No WHS", () =>
				{
					AssertContains("Pre-req", "{ ShowSendDialog: success: { PreSendValidation: success:", showSend.GetChainAsString());
					provider.ConfigureProcess(sendChain);
					AssertContains("ZA Defer Dialog Step added", "{ ShowSendDialog: success: { ZADefermentUpdate: success: { PreSendValidation:", showSend.GetChainAsString());
				});

				declaration = CreateDeclaration(true, WarehouseTransactionStatusList.Codes.InwardCreatedPending);
				provider = ZACustomsMessagingProvider.New(declaration);
				sendProcess = new SendMessagesProcess();
				sendChain = sendProcess.SendProcessChain;

				showSend = sendChain.FindAction(SendMessagesProcess.SendMessageActions.ShowSendDialog);

				CombineAssertions("With WHS", () =>
				{
					provider.ConfigureProcess(sendChain);
					AssertContains("ZA Defer Dialog Step added", "{ ShowSendDialog: success: { ZADefermentUpdate: success: { PreSendValidation: success: { ShowPreSendValidationNotifications: success: { SendMessagesWithErrorsSecurityCheckpoint: success: { ShowSendMessagesWithErrorsSecurityCheckpointOverride: success: { CreditAndDPSCheck: success: { ShowCreditAndDPSCheckOverride: success: { WHSAutomationPreActions: success: { CreateMessages-WHSAutomationPostActions-SubChain:", showSend.GetChainAsString());
				});
			}
		}

		public void TestRunProcessNoWHS()
		{
			using (CustomsDataRegistry.Instance.CreditCheckOnMessageSend.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
			{
				var declaration = CreateDeclaration(true);
				var supporter = new CustomsMessagingSupporter(declaration, new ZACustomsMessagingProviderFactory());
				var sendProcess = new SendMessagesProcess(supporter.GetSendMessagesBusinessActionProvider());
				var sendChain = sendProcess.SendProcessChain;

				var results = ActionChainTestHelper.SimulateChain(sendChain);
				AssertContainsExactElementsInExactOrder(new[]
				{
					"Input: True Action: SendMessagesSecurityCheckpoint Method: Enterprise.Customs.Business.MessagingProcess.SendMessagesBusinessActionProvider.SendMessagesSecurityCheckpoint Output: True",
					"Input: True Action: ZADefermentUpdate Method: Enterprise.Customs.ZA.Business.MessagingProcess.ZACustomsMessagingProvider.UpdateDeferment Output: True",
					"Input: True Action: PreSendValidation Method: Enterprise.Customs.Business.MessagingProcess.SendMessagesBusinessActionProvider.PreSendValidation Output: True",
					"Input: True Action: SendMessagesWithErrorsSecurityCheckpoint Method: Enterprise.Customs.Business.MessagingProcess.SendMessagesBusinessActionProvider.SendMessagesWithErrorsSecurityCheckpoint Output: True",
					"Input: True Action: CreditAndDPSCheck Method: Enterprise.Customs.Business.MessagingProcess.SendMessagesBusinessActionProvider.CreditAndDPSCheck Output: True",
					"Input: True Action: CreateMessages Method: Enterprise.Customs.Business.MessagingProcess.SendMessagesBusinessActionProvider.CreateMessages Output: True",
					"Input: True Action: SignMessages Method: Enterprise.Customs.Business.MessagingProcess.SendMessagesBusinessActionProvider.SignMessages Output: True",
					"Input: True Action: ProcessUpdates Method: Enterprise.Customs.Business.MessagingProcess.SendMessagesBusinessActionProvider.ProcessUpdates Output: True"
				}, results);
			}
		}

		public void TestRunProcessWithWHS()
		{
			using (CustomsDataRegistry.Instance.CreditCheckOnMessageSend.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
			{
				var declaration = CreateDeclaration(true, WarehouseTransactionStatusList.Codes.InwardCreatedPending);
				var supporter = new CustomsMessagingSupporter(declaration, new ZACustomsMessagingProviderFactory());
				var sendProcess = new SendMessagesProcess(supporter.GetSendMessagesBusinessActionProvider());
				var sendChain = sendProcess.SendProcessChain;

				CombineAssertions(() =>
				{
					var results = ActionChainTestHelper.SimulateChain(sendChain);
					AssertContainsExactElementsInExactOrder("Successful Run", new[]
					{
						"Input: True Action: SendMessagesSecurityCheckpoint Method: Enterprise.Customs.Business.MessagingProcess.SendMessagesBusinessActionProvider.SendMessagesSecurityCheckpoint Output: True",
						"Input: True Action: ZADefermentUpdate Method: Enterprise.Customs.ZA.Business.MessagingProcess.ZACustomsMessagingProvider.UpdateDeferment Output: True",
						"Input: True Action: PreSendValidation Method: Enterprise.Customs.Business.MessagingProcess.SendMessagesBusinessActionProvider.PreSendValidation Output: True",
						"Input: True Action: SendMessagesWithErrorsSecurityCheckpoint Method: Enterprise.Customs.Business.MessagingProcess.SendMessagesBusinessActionProvider.SendMessagesWithErrorsSecurityCheckpoint Output: True",
						"Input: True Action: CreditAndDPSCheck Method: Enterprise.Customs.Business.MessagingProcess.SendMessagesBusinessActionProvider.CreditAndDPSCheck Output: True",
						"Input: True Action: WHSAutomationPreActions Method: Enterprise.Customs.Business.MessagingProcess.Declaration.JobDeclarationMessagingProvider`2[[Enterprise.Customs.ZA.Business.JobDeclarationMessageSendingObjectParent, Enterprise.Customs.ZA.Business, Version=2.0.0.0, Culture=neutral, PublicKeyToken=4f570df270576350],[Enterprise.Customs.ZA.Business.MessageSendingObject, Enterprise.Customs.ZA.Business, Version=2.0.0.0, Culture=neutral, PublicKeyToken=4f570df270576350]].WarehouseAutomationPreActions Output: True",
						"Input: True Action: CreateMessages Method: Enterprise.Customs.Business.MessagingProcess.SendMessagesBusinessActionProvider.CreateMessages Output: True",
						"Input: True Action: SignMessages Method: Enterprise.Customs.Business.MessagingProcess.SendMessagesBusinessActionProvider.SignMessages Output: True",
						"Input: True Action: ProcessUpdates Method: Enterprise.Customs.Business.MessagingProcess.SendMessagesBusinessActionProvider.ProcessUpdates Output: True",
						"Input: True Action: WHSAutomationPostActions Method: Enterprise.Customs.Business.MessagingProcess.Declaration.JobDeclarationMessagingProvider`2[[Enterprise.Customs.ZA.Business.JobDeclarationMessageSendingObjectParent, Enterprise.Customs.ZA.Business, Version=2.0.0.0, Culture=neutral, PublicKeyToken=4f570df270576350],[Enterprise.Customs.ZA.Business.MessageSendingObject, Enterprise.Customs.ZA.Business, Version=2.0.0.0, Culture=neutral, PublicKeyToken=4f570df270576350]].WarehouseAutomationPostActions Output: True"
					}, results);

					results = ActionChainTestHelper.SimulateChain(sendChain, new Dictionary<string, bool?>
					{
						{ SendMessagesProcess.SendMessageActions.CreateMessages, false }
					});
					AssertContainsExactElementsInExactOrder("Create Messages error", new[]
					{
						"Input: True Action: SendMessagesSecurityCheckpoint Method: Enterprise.Customs.Business.MessagingProcess.SendMessagesBusinessActionProvider.SendMessagesSecurityCheckpoint Output: True",
						"Input: True Action: ZADefermentUpdate Method: Enterprise.Customs.ZA.Business.MessagingProcess.ZACustomsMessagingProvider.UpdateDeferment Output: True",
						"Input: True Action: PreSendValidation Method: Enterprise.Customs.Business.MessagingProcess.SendMessagesBusinessActionProvider.PreSendValidation Output: True",
						"Input: True Action: SendMessagesWithErrorsSecurityCheckpoint Method: Enterprise.Customs.Business.MessagingProcess.SendMessagesBusinessActionProvider.SendMessagesWithErrorsSecurityCheckpoint Output: True",
						"Input: True Action: CreditAndDPSCheck Method: Enterprise.Customs.Business.MessagingProcess.SendMessagesBusinessActionProvider.CreditAndDPSCheck Output: True",
						"Input: True Action: WHSAutomationPreActions Method: Enterprise.Customs.Business.MessagingProcess.Declaration.JobDeclarationMessagingProvider`2[[Enterprise.Customs.ZA.Business.JobDeclarationMessageSendingObjectParent, Enterprise.Customs.ZA.Business, Version=2.0.0.0, Culture=neutral, PublicKeyToken=4f570df270576350],[Enterprise.Customs.ZA.Business.MessageSendingObject, Enterprise.Customs.ZA.Business, Version=2.0.0.0, Culture=neutral, PublicKeyToken=4f570df270576350]].WarehouseAutomationPreActions Output: True",
						"Input: True Action: CreateMessages Method: Enterprise.Customs.Business.MessagingProcess.SendMessagesBusinessActionProvider.CreateMessages Output: False",
						"Input: False Action: WHSAutomationPostActions Method: Enterprise.Customs.Business.MessagingProcess.Declaration.JobDeclarationMessagingProvider`2[[Enterprise.Customs.ZA.Business.JobDeclarationMessageSendingObjectParent, Enterprise.Customs.ZA.Business, Version=2.0.0.0, Culture=neutral, PublicKeyToken=4f570df270576350],[Enterprise.Customs.ZA.Business.MessageSendingObject, Enterprise.Customs.ZA.Business, Version=2.0.0.0, Culture=neutral, PublicKeyToken=4f570df270576350]].WarehouseAutomationPostActions Output: False",
						"Input: False Action: WHSAutomationRestoreActions Method: Enterprise.Customs.Business.MessagingProcess.Declaration.JobDeclarationMessagingProvider`2[[Enterprise.Customs.ZA.Business.JobDeclarationMessageSendingObjectParent, Enterprise.Customs.ZA.Business, Version=2.0.0.0, Culture=neutral, PublicKeyToken=4f570df270576350],[Enterprise.Customs.ZA.Business.MessageSendingObject, Enterprise.Customs.ZA.Business, Version=2.0.0.0, Culture=neutral, PublicKeyToken=4f570df270576350]].WarehouseAutomationRestoreActions Output: False"
					}, results);
				});
			}
		}

		public void TestICommonJobDeclarationSupporterProvider()
		{
			var declaration = CreateDeclaration(true);
			var providerFactory = (ICommonJobDeclarationProviderFactory)ZACustomsMessagingProvider.New(declaration);

			var provider = providerFactory.Provider;
			AssertType<JobDeclarationMessagingProvider<JobDeclarationMessageSendingObjectParent, MessageSendingObject>>("Supporter Type", provider);

			AssertEquals("ZAMessagingSupporter passed in as ISupportCreditAndDPSCheckOptions", JobDeclarationMessageSendingObjectParent.DocumentApprovalReasonDescription, provider.DefaultApprovalRequestReason);
		}

		public void TestIValidateAdditionalBusinessObjects()
		{
			var declaration = CreateDeclaration(true);
			var provider = ZACustomsMessagingProvider.New(declaration);
			var additional = provider as IValidateAdditionalBusinessObjects;

			AssertNotNull("provider is IValidateAdditionalBusinessObjects", additional);
			var objs = additional.AdditionalBusniessObjectsToValidate.ToList();

			AssertEquals("1 Additional", 1, objs.Count);
			AssertSame("Should be DecWrapper", provider.DeclarationWrapper, objs[0]);
		}

		public void TestISupportCreditAndDPSCheckOptions()
		{
			var declaration = CreateDeclaration(false);
			var provider = ZACustomsMessagingProvider.New(declaration);
			var checkerOptions = provider as ISupportCreditAndDPSCheckOptions;

			CombineAssertions(() =>
			{
				AssertNotNull("Supporter is ISupportCreditAndDPSCheckOptions", checkerOptions);
				AssertEquals("MustRunCreditCheck", true, checkerOptions.MustRunCreditCheck);
				AssertEquals("Credit Restriction message", declaration.CreditRestrictionMessageCaption, checkerOptions.CreditRestrictionMessageCaption);
				AssertEquals("Default Approval Reason", JobDeclarationMessageSendingObjectParent.DocumentApprovalReasonDescription, checkerOptions.DefaultApprovalRequestReason);
			});
		}

		JobDeclaration CreateDeclaration(bool doMerge, string warehouseStatus = "")
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			var instruction = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			instruction.CEI_Style = "11";
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_CEI = instruction.PK;
			invoiceLine.JI_Procedure = $"{invoiceLine.EntryInstruction.CEI_Style}00";

			if (doMerge)
			{
				new LineMerger(declaration).DoMerge();

				var entry = declaration.ActiveEntryHeaders[0];
				entry.CH_WarehouseTransactionStatus = warehouseStatus;
			}

			return declaration;
		}
	}
}
