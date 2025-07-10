using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Business.BatchProcessor;
using Enterprise.Customs.Common.US;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.LVS.Business;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.LVS.GUI.Testing
{
	[TestedType(typeof(MessagesSubmitForm))]
	public class MessagesSubmitFormTest : ZFormBasherTest
	{
		const string rejectNotification = "It is likely that your message(s) will be rejected by Customs, as they have the following message errors";
		const string cancelledByUserNotification = "Submission Cancelled By User";

		public void TestValidateAllChildren()
		{
			string tariffNumber = "8542996328";

			var tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = tariffNumber;
			tariff.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff.UE_DateTo = ZDate.Today;
			tariff.UE_PGACodes = "FD1";

			var clearance = Factory.New<CusUSLVClearance>();
			clearance.ULH_EntryFilerCode = "ABC";

			var stmNums = Factory.New<CustomsNumberViewStmNums>();
			stmNums.Provider = CustomsNumberViewStmNumsBusinessProviderHelper.GetProvider(Factory, clearance.Branch.Company.GC_RN_NKCountryCode, clearance.Branch.Company.PK);
			stmNums.SN_Owner = clearance.Branch.PK;
			stmNums.SN_Type = "ENS";
			var wrapper = stmNums.Wrapper as USCustomsNumberViewStmNumsWrapper;
			wrapper.AppliesTo = "ABC";
			wrapper.IsBranchLevel = true;
			Factory.Save();

			var consignment1 = clearance.CusUSLVConsignments.AddNew();

			var cusUSLVItem1 = consignment1.CusUSLVItems.AddNew();
			var cusUSLVItem2 = consignment1.CusUSLVItems.AddNew();
			cusUSLVItem2.ULI_Tariff = tariffNumber;

			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var reloadClearance = newFactory.Load<CusUSLVClearance>(clearance.PK);
			var reloadConsignment = reloadClearance.CusUSLVConsignments[0];
			reloadConsignment.CusUSLVItems.Cast<CusUSLVItem>().First(x => !x.ULI_Tariff.IsEmpty).MarkLightValidationAsValidForTesting();
			reloadConsignment.InitAction(UpdateActionCode.Add);
			using (var form = new MessagesSubmitForm(new CusUSLVClearanceMessageWrapper(reloadClearance), UpdateActionCode.Add))
			{
				form.Show();

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

				form.FindSingleOrDefault<ZButton>("ButtonSelectAll").PerformClick();

				form.AcceptButton.PerformClick();
				AssertContains("Agency Program declarations are not supported in this module and will need to be completed using a stand alone declaration.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestSubmitToCustoms_UnlockSendCustomsMessageMutexDirectly()
		{
			AssertUnlockSendCustomsMessageMutexDirectly(UpdateActionCode.Add);
			AssertUnlockSendCustomsMessageMutexDirectly(UpdateActionCode.Update);

			void AssertUnlockSendCustomsMessageMutexDirectly(UpdateActionCode updateActionCode)
			{
				var clearance = Factory.New<CusUSLVClearance>();
				if (updateActionCode == UpdateActionCode.Add)
				{
					clearance.ULH_EntryFilerCode = "ABC";

					var stmNums = Factory.New<CustomsNumberViewStmNums>();
					stmNums.Provider = CustomsNumberViewStmNumsBusinessProviderHelper.GetProvider(Factory, clearance.Branch.Company.GC_RN_NKCountryCode, clearance.Branch.Company.PK);
					stmNums.SN_Owner = clearance.Branch.PK;
					stmNums.SN_Type = "ENS";
					var wrapper = stmNums.Wrapper as USCustomsNumberViewStmNumsWrapper;
					wrapper.AppliesTo = "ABC";
					wrapper.IsBranchLevel = true;
				}

				var consignment = clearance.CusUSLVConsignments.AddNew();
				if (updateActionCode == UpdateActionCode.Update)
				{
					consignment.ULB_MessageStatus = ImportMessageStatusList.Codes.OriginalRequestPending;
					consignment.CE_EntryStatus = CRLReleaseStatusList.Codes.REL;
				}

				consignment.InitAction(updateActionCode);
				Factory.Save();

				using (new DisposableAction(() => Globals.SetIsUnitTestingProductionFunctionality(true), () => Globals.SetIsUnitTestingProductionFunctionality(false)))
				using (var form = new MessagesSubmitForm(new CusUSLVClearanceMessageWrapper(clearance), updateActionCode))
				{
					form.Show();

					ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs((obj) =>
					{
						Assert("Mutex should be released", !clearance.IsSendCustomsMessageMutexLocked);
					});

					form.FindSingleOrDefault<ZButton>("ButtonSelectAll").PerformClick();
					form.AcceptButton.PerformClick();
				}
			}
		}

		public void TestSubmitToCustoms_MutexLockedErrorMessage()
		{
			var updateActionCode = UpdateActionCode.Add;
			var clearance = Factory.New<CusUSLVClearance>();

			clearance.ULH_EntryFilerCode = "ABC";

			var stmNums = Factory.New<CustomsNumberViewStmNums>();
			stmNums.Provider = CustomsNumberViewStmNumsBusinessProviderHelper.GetProvider(Factory, clearance.Branch.Company.GC_RN_NKCountryCode, clearance.Branch.Company.PK);
			stmNums.SN_Owner = clearance.Branch.PK;
			stmNums.SN_Type = "ENS";
			var wrapper = stmNums.Wrapper as USCustomsNumberViewStmNumsWrapper;
			wrapper.AppliesTo = "ABC";
			wrapper.IsBranchLevel = true;

			var consignment = clearance.CusUSLVConsignments.AddNew();

			consignment.InitAction(updateActionCode);
			Factory.Save();
			clearance.LockSendCustomsMessageMutex();

			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			using (var form = new MessagesSubmitForm(new CusUSLVClearanceMessageWrapper(clearance), updateActionCode))
			{
				form.Show();

				form.FindSingleOrDefault<ZButton>("ButtonSelectAll").PerformClick();
				form.AcceptButton.PerformClick();
			}

			clearance.UnlockSendCustomsMessageMutex();
			AssertEndsWith("mutex error message", "is sending messages for this Low Value Entries job, please try again later.", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestSubmitToCustoms_AllConsignmentsHasQueuedToBeSentToCustoms_ClickNoInWarningForm_NoConsigmentWillBeSend()
		{
			var clearance = Factory.New<CusUSLVClearance>();

			var consignment = clearance.CusUSLVConsignments.AddNew();
			consignment.ULB_MessageStatus = ImportMessageStatusList.Codes.OriginalRequestPending;
			consignment.InitAction(UpdateActionCode.Add);

			Factory.Save();

			var clearanceWrapper = new CusUSLVClearanceMessageWrapper(clearance);
			using (var form = new MessagesSubmitForm(clearanceWrapper, UpdateActionCode.Add))
			{
				form.Show();

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);

				var consignmentForMessaging = clearanceWrapper.CusUSLVConsignmentsToSend.OfType<CusUSLVConsignmentForMessaging>().Single();
				consignmentForMessaging.SendToCustoms = true;

				form.AcceptButton.PerformClick();
				AssertEquals("Submission Cancelled By User", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestSubmitToCustoms_SomeConsignmentsHasQueuedToBeSentToCustoms_ClickNoInWarningForm_SendConsignmentsWhichNotQueued()
		{
			var clearance = Factory.New<CusUSLVClearance>();
			clearance.ULH_EntryFilerCode = "ABC";

			var stmNums = Factory.New<CustomsNumberViewStmNums>();
			stmNums.Provider = CustomsNumberViewStmNumsBusinessProviderHelper.GetProvider(Factory, clearance.Branch.Company.GC_RN_NKCountryCode, clearance.Branch.Company.PK);
			stmNums.SN_Owner = clearance.Branch.PK;
			stmNums.SN_Type = "ENS";
			var wrapper = stmNums.Wrapper as USCustomsNumberViewStmNumsWrapper;
			wrapper.AppliesTo = "ABC";
			wrapper.IsBranchLevel = true;
			Factory.Save();

			var consignment1 = clearance.CusUSLVConsignments.AddNew();
			consignment1.ULB_MessageStatus = ImportMessageStatusList.Codes.OriginalRequestPending;
			consignment1.InitAction(UpdateActionCode.Add);

			var consignment2 = clearance.CusUSLVConsignments.AddNew();
			consignment2.InitAction(UpdateActionCode.Add);

			Factory.Save();

			using (var form = new MessagesSubmitForm(new CusUSLVClearanceMessageWrapper(clearance), UpdateActionCode.Add))
			{
				form.Show();

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);

				form.FindSingleOrDefault<ZButton>("ButtonSelectAll").PerformClick();

				form.AcceptButton.PerformClick();
				AssertContains(rejectNotification, UnitTestUserNotification.Instance.LastMessage.Text);
			}

			consignment1.InitAction(UpdateActionCode.Add);
			consignment2.InitAction(UpdateActionCode.Add);
			using (var form = new MessagesSubmitForm(new CusUSLVClearanceMessageWrapper(clearance), UpdateActionCode.Add))
			{
				form.Show();

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);

				form.FindSingleOrDefault<ZButton>("ButtonSelectAll").PerformClick();

				form.AcceptButton.PerformClick();
				AssertContains(rejectNotification, UnitTestUserNotification.Instance.LastMessage.Text);
			}

			consignment1.InitAction(UpdateActionCode.Add);
			consignment2.InitAction(UpdateActionCode.Add);
			using (var form = new MessagesSubmitForm(new CusUSLVClearanceMessageWrapper(clearance), UpdateActionCode.Add))
			{
				form.Show();

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);

				form.FindSingleOrDefault<ZButton>("ButtonSelectAll").PerformClick();

				form.AcceptButton.PerformClick();
				AssertEquals("1 original message(s) has been queued.", UnitTestUserNotification.Instance.LastMessage.Text);
			}

			new AutoSendCustomsMessagingBatchProcessor(new LoggingInformation()).ExecuteBatch();
			Factory.Save();

			var message1 = consignment1.Messages.Last() as EDIMessage;
			AssertEquals(true, message1.EM_SendWithMessageErrors);

			consignment1.CE_EntryStatus = CRLReleaseStatusList.Codes.REL;
			consignment2.CE_EntryStatus = CRLReleaseStatusList.Codes.REL;
			consignment1.InitAction(UpdateActionCode.Update);
			consignment2.InitAction(UpdateActionCode.Update);
			using (var form = new MessagesSubmitForm(new CusUSLVClearanceMessageWrapper(clearance), UpdateActionCode.Update))
			{
				form.Show();

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);

				form.FindSingleOrDefault<ZButton>("ButtonSelectAll").PerformClick();

				form.AcceptButton.PerformClick();
				AssertEquals("2 update message(s) has been generated.", UnitTestUserNotification.Instance.LastMessage.Text);
			}

			var message2 = consignment2.Messages.Last() as EDIMessage;
			AssertNotSame(message1, message2);
			AssertEquals(true, message2.EM_SendWithMessageErrors);
		}

		public void TestSubmitToCustoms_SomeConsignmentsHasQueuedToBeSentToCustoms_ClickYesInWarningForm_SendAllConsignments()
		{
			var clearance = Factory.New<CusUSLVClearance>();
			clearance.ULH_EntryFilerCode = "ABC";

			var stmNums = Factory.New<CustomsNumberViewStmNums>();
			stmNums.Provider = CustomsNumberViewStmNumsBusinessProviderHelper.GetProvider(Factory, clearance.Branch.Company.GC_RN_NKCountryCode, clearance.Branch.Company.PK);
			stmNums.SN_Owner = clearance.Branch.PK;
			stmNums.SN_Type = "ENS";
			var wrapper = stmNums.Wrapper as USCustomsNumberViewStmNumsWrapper;
			wrapper.AppliesTo = "ABC";
			wrapper.IsBranchLevel = true;
			Factory.Save();

			var consignment1 = clearance.CusUSLVConsignments.AddNew();
			consignment1.ULB_MessageStatus = ImportMessageStatusList.Codes.OriginalRequestPending;
			consignment1.InitAction(UpdateActionCode.Add);

			var consignment2 = clearance.CusUSLVConsignments.AddNew();
			consignment2.InitAction(UpdateActionCode.Add);

			Factory.Save();

			using (var form = new MessagesSubmitForm(new CusUSLVClearanceMessageWrapper(clearance), UpdateActionCode.Add))
			{
				form.Show();

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);

				form.FindSingleOrDefault<ZButton>("ButtonSelectAll").PerformClick();

				form.AcceptButton.PerformClick();
				AssertContains(rejectNotification, UnitTestUserNotification.Instance.LastMessage.Text);
			}

			consignment1.InitAction(UpdateActionCode.Add);
			consignment2.InitAction(UpdateActionCode.Add);
			using (var form = new MessagesSubmitForm(new CusUSLVClearanceMessageWrapper(clearance), UpdateActionCode.Add))
			{
				form.Show();

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);

				form.FindSingleOrDefault<ZButton>("ButtonSelectAll").PerformClick();

				form.AcceptButton.PerformClick();
				AssertContains(rejectNotification, UnitTestUserNotification.Instance.LastMessage.Text);
			}

			consignment1.InitAction(UpdateActionCode.Add);
			consignment2.InitAction(UpdateActionCode.Add);
			using (var form = new MessagesSubmitForm(new CusUSLVClearanceMessageWrapper(clearance), UpdateActionCode.Add))
			{
				form.Show();

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);

				form.FindSingleOrDefault<ZButton>("ButtonSelectAll").PerformClick();

				form.AcceptButton.PerformClick();
				AssertEquals("2 original message(s) has been queued.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestSubmitToCustoms_SomeConsignmentsHasQueuedToBeSentToCustoms_ClickCancelInWarningForm_SendNothing()
		{
			var clearance = Factory.New<CusUSLVClearance>();

			var consignment1 = clearance.CusUSLVConsignments.AddNew();
			consignment1.ULB_MessageStatus = ImportMessageStatusList.Codes.OriginalRequestPending;
			consignment1.InitAction(UpdateActionCode.Add);

			var consignment2 = clearance.CusUSLVConsignments.AddNew();
			consignment2.InitAction(UpdateActionCode.Add);

			Factory.Save();

			using (var form = new MessagesSubmitForm(new CusUSLVClearanceMessageWrapper(clearance), UpdateActionCode.Add))
			{
				form.Show();

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Cancel);

				form.FindSingleOrDefault<ZButton>("ButtonSelectAll").PerformClick();

				form.AcceptButton.PerformClick();
				AssertEquals(ZString.Empty, consignment2.ULB_MessageStatus);
			}
		}

		public void TestSubmitToCustoms_SomeConsignmentsHasQueuedToBeSentToCustoms_ClickCancelInWarningForm_ReturnsDialogResultNone()
		{
			var clearance = Factory.New<CusUSLVClearance>();

			var consignment1 = clearance.CusUSLVConsignments.AddNew();
			consignment1.ULB_MessageStatus = ImportMessageStatusList.Codes.OriginalRequestPending;
			consignment1.InitAction(UpdateActionCode.Add);

			var consignment2 = clearance.CusUSLVConsignments.AddNew();
			consignment2.InitAction(UpdateActionCode.Add);

			Factory.Save();

			using (var form = new MessagesSubmitForm(new CusUSLVClearanceMessageWrapper(clearance), UpdateActionCode.Add))
			{
				form.Show();

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Cancel);

				form.FindSingleOrDefault<ZButton>("ButtonSelectAll").PerformClick();

				form.AcceptButton.PerformClick();
				AssertEquals(DialogResult.None, form.DialogResult);
			}
		}

		public void TestSubmitToCustoms_SomeConsignmentsHasQueuedToBeSentToCustoms_ClickSendInWarningForm_ReturnsDialogResultYes()
		{
			var clearance = Factory.New<CusUSLVClearance>();
			clearance.ULH_EntryFilerCode = "ABC";

			var stmNums = Factory.New<CustomsNumberViewStmNums>();
			stmNums.Provider = CustomsNumberViewStmNumsBusinessProviderHelper.GetProvider(Factory, clearance.Branch.Company.GC_RN_NKCountryCode, clearance.Branch.Company.PK);
			stmNums.SN_Owner = clearance.Branch.PK;
			stmNums.SN_Type = "ENS";
			var wrapper = stmNums.Wrapper as USCustomsNumberViewStmNumsWrapper;
			wrapper.AppliesTo = "ABC";
			wrapper.IsBranchLevel = true;
			Factory.Save();

			var consignment1 = clearance.CusUSLVConsignments.AddNew();
			consignment1.ULB_MessageStatus = ImportMessageStatusList.Codes.OriginalRequestPending;
			consignment1.InitAction(UpdateActionCode.Add);

			var consignment2 = clearance.CusUSLVConsignments.AddNew();
			consignment2.InitAction(UpdateActionCode.Add);

			Factory.Save();

			using (var form = new MessagesSubmitForm(new CusUSLVClearanceMessageWrapper(clearance), UpdateActionCode.Add))
			{
				form.Show();

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);

				form.FindSingleOrDefault<ZButton>("ButtonSelectAll").PerformClick();

				form.AcceptButton.PerformClick();
				AssertEquals(DialogResult.None, form.DialogResult);
			}

			consignment1.InitAction(UpdateActionCode.Add);
			consignment2.InitAction(UpdateActionCode.Add);
			using (var form = new MessagesSubmitForm(new CusUSLVClearanceMessageWrapper(clearance), UpdateActionCode.Add))
			{
				form.Show();

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);

				form.FindSingleOrDefault<ZButton>("ButtonSelectAll").PerformClick();

				form.AcceptButton.PerformClick();
				AssertEquals(DialogResult.None, form.DialogResult);
			}

			consignment1.InitAction(UpdateActionCode.Add);
			consignment2.InitAction(UpdateActionCode.Add);
			using (var form = new MessagesSubmitForm(new CusUSLVClearanceMessageWrapper(clearance), UpdateActionCode.Add))
			{
				form.Show();

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);

				form.FindSingleOrDefault<ZButton>("ButtonSelectAll").PerformClick();

				form.AcceptButton.PerformClick();
				AssertEquals(DialogResult.Yes, form.DialogResult);
			}
		}

		public void TestSubmitToCustoms_AllConsignmentsHasSentToCustoms_ClickNoInWarningForm_NoConsigmentWillBeSend()
		{
			var clearance = Factory.New<CusUSLVClearance>();

			var consignment = clearance.CusUSLVConsignments.AddNew();
			consignment.ULB_MessageStatus = ImportMessageStatusList.Codes.AwaitingEntrySummaryOriginal;
			consignment.InitAction(UpdateActionCode.Add);

			Factory.Save();

			var clearanceWrapper = new CusUSLVClearanceMessageWrapper(clearance);
			using (var form = new MessagesSubmitForm(clearanceWrapper, UpdateActionCode.Add))
			{
				form.Show();

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);

				var consignmentForMessaging = clearanceWrapper.CusUSLVConsignmentsToSend.OfType<CusUSLVConsignmentForMessaging>().Single();
				consignmentForMessaging.SendToCustoms = true;

				form.AcceptButton.PerformClick();
				AssertEquals("Submission Cancelled By User", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestSubmitToCustoms_SomeConsignmentsHasSentToCustoms_ClickNoInWarningForm_SendConsignmentsWhichNotSentBefore()
		{
			var clearance = Factory.New<CusUSLVClearance>();
			clearance.ULH_EntryFilerCode = "ABC";

			var stmNums = Factory.New<CustomsNumberViewStmNums>();
			stmNums.Provider = CustomsNumberViewStmNumsBusinessProviderHelper.GetProvider(Factory, clearance.Branch.Company.GC_RN_NKCountryCode, clearance.Branch.Company.PK);
			stmNums.SN_Owner = clearance.Branch.PK;
			stmNums.SN_Type = "ENS";
			var wrapper = stmNums.Wrapper as USCustomsNumberViewStmNumsWrapper;
			wrapper.AppliesTo = "ABC";
			wrapper.IsBranchLevel = true;
			Factory.Save();

			var consignment1 = clearance.CusUSLVConsignments.AddNew();
			consignment1.ULB_MessageStatus = ImportMessageStatusList.Codes.AwaitingEntrySummaryOriginal;
			consignment1.InitAction(UpdateActionCode.Add);

			var consignment2 = clearance.CusUSLVConsignments.AddNew();
			consignment2.InitAction(UpdateActionCode.Add);

			Factory.Save();

			using (var form = new MessagesSubmitForm(new CusUSLVClearanceMessageWrapper(clearance), UpdateActionCode.Add))
			{
				form.Show();

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);

				form.FindSingleOrDefault<ZButton>("ButtonSelectAll").PerformClick();

				form.AcceptButton.PerformClick();
				AssertContains(rejectNotification, UnitTestUserNotification.Instance.LastMessage.Text);
			}

			consignment1.InitAction(UpdateActionCode.Add);
			consignment2.InitAction(UpdateActionCode.Add);
			using (var form = new MessagesSubmitForm(new CusUSLVClearanceMessageWrapper(clearance), UpdateActionCode.Add))
			{
				form.Show();

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);

				form.FindSingleOrDefault<ZButton>("ButtonSelectAll").PerformClick();

				form.AcceptButton.PerformClick();
				AssertContains(rejectNotification, UnitTestUserNotification.Instance.LastMessage.Text);
			}

			consignment1.InitAction(UpdateActionCode.Add);
			consignment2.InitAction(UpdateActionCode.Add);
			using (var form = new MessagesSubmitForm(new CusUSLVClearanceMessageWrapper(clearance), UpdateActionCode.Add))
			{
				form.Show();

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);

				form.FindSingleOrDefault<ZButton>("ButtonSelectAll").PerformClick();

				form.AcceptButton.PerformClick();
				AssertEquals("1 original message(s) has been queued.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestSubmitToCustoms_SomeConsignmentsHasSentToCustoms_ClickYesInWarningForm_SendAllConsignments()
		{
			var clearance = Factory.New<CusUSLVClearance>();
			clearance.ULH_EntryFilerCode = "ABC";

			var stmNums = Factory.New<CustomsNumberViewStmNums>();
			stmNums.Provider = CustomsNumberViewStmNumsBusinessProviderHelper.GetProvider(Factory, clearance.Branch.Company.GC_RN_NKCountryCode, clearance.Branch.Company.PK);
			stmNums.SN_Owner = clearance.Branch.PK;
			stmNums.SN_Type = "ENS";
			var wrapper = stmNums.Wrapper as USCustomsNumberViewStmNumsWrapper;
			wrapper.AppliesTo = "ABC";
			wrapper.IsBranchLevel = true;
			Factory.Save();

			var consignment1 = clearance.CusUSLVConsignments.AddNew();
			consignment1.ULB_MessageStatus = ImportMessageStatusList.Codes.AwaitingEntrySummaryOriginal;
			consignment1.InitAction(UpdateActionCode.Add);

			var consignment2 = clearance.CusUSLVConsignments.AddNew();
			consignment2.InitAction(UpdateActionCode.Add);

			Factory.Save();

			using (var form = new MessagesSubmitForm(new CusUSLVClearanceMessageWrapper(clearance), UpdateActionCode.Add))
			{
				form.Show();

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);

				form.FindSingleOrDefault<ZButton>("ButtonSelectAll").PerformClick();

				form.AcceptButton.PerformClick();
				AssertContains(rejectNotification, UnitTestUserNotification.Instance.LastMessage.Text);
			}

			consignment1.InitAction(UpdateActionCode.Add);
			consignment2.InitAction(UpdateActionCode.Add);
			using (var form = new MessagesSubmitForm(new CusUSLVClearanceMessageWrapper(clearance), UpdateActionCode.Add))
			{
				form.Show();

				form.FindSingleOrDefault<ZButton>("ButtonSelectAll").PerformClick();

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);

				form.AcceptButton.PerformClick();
				AssertContains(rejectNotification, UnitTestUserNotification.Instance.LastMessage.Text);
			}

			consignment1.InitAction(UpdateActionCode.Add);
			consignment2.InitAction(UpdateActionCode.Add);
			using (var form = new MessagesSubmitForm(new CusUSLVClearanceMessageWrapper(clearance), UpdateActionCode.Add))
			{
				form.Show();

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);

				form.FindSingleOrDefault<ZButton>("ButtonSelectAll").PerformClick();

				form.AcceptButton.PerformClick();
				AssertEquals("2 original message(s) has been queued.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestSubmitToCustoms_SomeConsignmentsHasSentToCustoms_ClickCancelInWarningForm_SendNothing()
		{
			var clearance = Factory.New<CusUSLVClearance>();

			var consignment1 = clearance.CusUSLVConsignments.AddNew();
			consignment1.ULB_MessageStatus = ImportMessageStatusList.Codes.AwaitingEntrySummaryOriginal;
			consignment1.CE_EntryStatus = CRLReleaseStatusList.Codes.REL;
			consignment1.InitAction(UpdateActionCode.Add);

			var consignment2 = clearance.CusUSLVConsignments.AddNew();
			consignment2.CE_EntryStatus = CRLReleaseStatusList.Codes.REL;
			consignment2.InitAction(UpdateActionCode.Add);

			Factory.Save();

			using (var form = new MessagesSubmitForm(new CusUSLVClearanceMessageWrapper(clearance), UpdateActionCode.Add))
			{
				form.Show();

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Cancel);

				form.FindSingleOrDefault<ZButton>("ButtonSelectAll").PerformClick();

				form.AcceptButton.PerformClick();
				AssertEquals(ZString.Empty, consignment2.ULB_MessageStatus);
			}
		}

		public void TestSubmitToCustoms_UpdateActionIsUpdate_SuccessfulNotification()
		{
			var clearance = Factory.New<CusUSLVClearance>();

			var consignment = clearance.CusUSLVConsignments.AddNew();
			consignment.ULB_MessageStatus = ImportMessageStatusList.Codes.OriginalRequestPending;
			consignment.CE_EntryStatus = CRLReleaseStatusList.Codes.REL;
			consignment.InitAction(UpdateActionCode.Update);

			Factory.Save();

			using (var form = new MessagesSubmitForm(new CusUSLVClearanceMessageWrapper(clearance), UpdateActionCode.Update))
			{
				form.Show();

				form.FindSingleOrDefault<ZButton>("ButtonSelectAll").PerformClick();

				form.AcceptButton.PerformClick();
				AssertContains(rejectNotification, UnitTestUserNotification.Instance.LastMessage.Text);
			}

			consignment.InitAction(UpdateActionCode.Update);
			using (var form = new MessagesSubmitForm(new CusUSLVClearanceMessageWrapper(clearance), UpdateActionCode.Update))
			{
				form.Show();

				form.FindSingleOrDefault<ZButton>("ButtonSelectAll").PerformClick();

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);

				form.AcceptButton.PerformClick();
				AssertContains(cancelledByUserNotification, UnitTestUserNotification.Instance.LastMessage.Text);
			}

			consignment.InitAction(UpdateActionCode.Update);
			using (var form = new MessagesSubmitForm(new CusUSLVClearanceMessageWrapper(clearance), UpdateActionCode.Update))
			{
				form.Show();

				form.FindSingleOrDefault<ZButton>("ButtonSelectAll").PerformClick();

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);

				form.AcceptButton.PerformClick();
				AssertEquals("1 update message(s) has been generated.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestSubmitToCustoms_UpdateActionIsReplace_SuccessNotification()
		{
			var clearance = Factory.New<CusUSLVClearance>();

			var consignment = clearance.CusUSLVConsignments.AddNew();
			consignment.ULB_MessageStatus = ImportMessageStatusList.Codes.OriginalRequestPending;
			consignment.CE_EntryStatus = CRLReleaseStatusList.Codes.HLD;
			consignment.InitAction(UpdateActionCode.Replace);

			Factory.Save();

			using (var form = new MessagesSubmitForm(new CusUSLVClearanceMessageWrapper(clearance), UpdateActionCode.Replace))
			{
				form.Show();

				form.FindSingleOrDefault<ZButton>("ButtonSelectAll").PerformClick();

				form.AcceptButton.PerformClick();
				AssertContains(rejectNotification, UnitTestUserNotification.Instance.LastMessage.Text);
			}

			consignment.InitAction(UpdateActionCode.Replace);
			using (var form = new MessagesSubmitForm(new CusUSLVClearanceMessageWrapper(clearance), UpdateActionCode.Replace))
			{
				form.Show();

				form.FindSingleOrDefault<ZButton>("ButtonSelectAll").PerformClick();

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);

				form.AcceptButton.PerformClick();
				AssertContains(cancelledByUserNotification, UnitTestUserNotification.Instance.LastMessage.Text);
			}

			consignment.InitAction(UpdateActionCode.Replace);
			using (var form = new MessagesSubmitForm(new CusUSLVClearanceMessageWrapper(clearance), UpdateActionCode.Replace))
			{
				form.Show();

				form.FindSingleOrDefault<ZButton>("ButtonSelectAll").PerformClick();

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);

				form.AcceptButton.PerformClick();
				AssertEquals("1 replacement message(s) has been generated.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestSubmitToCustoms_UpdateActionIsDelete_SuccessNotification()
		{
			var clearance = Factory.New<CusUSLVClearance>();

			var consignment = clearance.CusUSLVConsignments.AddNew();
			consignment.ULB_MessageStatus = ImportMessageStatusList.Codes.OriginalRequestPending;
			consignment.CE_EntryStatus = CRLReleaseStatusList.Codes.HLD;
			consignment.InitAction(UpdateActionCode.Delete);

			Factory.Save();

			using (var form = new MessagesSubmitForm(new CusUSLVClearanceMessageWrapper(clearance), UpdateActionCode.Delete))
			{
				form.Show();

				form.FindSingleOrDefault<ZButton>("ButtonSelectAll").PerformClick();

				form.AcceptButton.PerformClick();
				AssertContains(rejectNotification, UnitTestUserNotification.Instance.LastMessage.Text);
			}

			consignment.InitAction(UpdateActionCode.Delete);
			using (var form = new MessagesSubmitForm(new CusUSLVClearanceMessageWrapper(clearance), UpdateActionCode.Delete))
			{
				form.Show();

				form.FindSingleOrDefault<ZButton>("ButtonSelectAll").PerformClick();

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);

				form.AcceptButton.PerformClick();
				AssertContains(cancelledByUserNotification, UnitTestUserNotification.Instance.LastMessage.Text);
			}

			consignment.InitAction(UpdateActionCode.Delete);
			using (var form = new MessagesSubmitForm(new CusUSLVClearanceMessageWrapper(clearance), UpdateActionCode.Delete))
			{
				form.Show();

				form.FindSingleOrDefault<ZButton>("ButtonSelectAll").PerformClick();

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);

				form.AcceptButton.PerformClick();
				AssertEquals("1 deletion message(s) has been generated.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestGenerateEntryNumbersRightBeforeMessageSending()
		{
			var clearance = Factory.New<CusUSLVClearance>();
			clearance.ULH_EntryFilerCode = "ABC";
			var consignment1 = clearance.CusUSLVConsignments.AddNew();
			var consignment2 = clearance.CusUSLVConsignments.AddNew();
			consignment1.InitAction(UpdateActionCode.Add);

			var stmNums = Factory.New<CustomsNumberViewStmNums>();
			stmNums.Provider = CustomsNumberViewStmNumsBusinessProviderHelper.GetProvider(Factory, clearance.Branch.Company.GC_RN_NKCountryCode, clearance.Branch.Company.PK);
			stmNums.SN_Owner = clearance.Branch.PK;
			stmNums.SN_Type = "ENS";
			var wrapper = stmNums.Wrapper as USCustomsNumberViewStmNumsWrapper;
			wrapper.AppliesTo = "ABC";
			wrapper.IsBranchLevel = true;
			Factory.Save();

			var clearanceWrapper = new CusUSLVClearanceMessageWrapper(clearance);
			var consignmentToSend = clearanceWrapper.CusUSLVConsignmentsToSend.OfType<CusUSLVConsignmentForMessaging>().Single(c => c.Consignment.PK == consignment1.PK);
			consignmentToSend.SendToCustoms = true;

			using (var form = new MessagesSubmitForm(clearanceWrapper, UpdateActionCode.Add))
			{
				form.Show();

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				form.AcceptButton.PerformClick();
				var lastMessage = UnitTestUserNotification.Instance.LastMessage;
				CombineAssertions(() =>
				{
					AssertEquals("1 original message(s) has been queued.", lastMessage.Text);
					AssertNotNullOrEmpty(consignment1.CE_EntryNum);
					AssertNullOrEmpty(consignment2.CE_EntryNum);
				});
			}
		}

		public void TestGenerateEntryNumbersRightBeforeMessageSending_NumberRangeNotExists()
		{
			var clearance = Factory.New<CusUSLVClearance>();
			clearance.ULH_EntryFilerCode = "ABC";
			var consignment1 = clearance.CusUSLVConsignments.AddNew();
			consignment1.InitAction(UpdateActionCode.Add);
			Factory.Save();

			var clearanceWrapper = new CusUSLVClearanceMessageWrapper(clearance);
			var consignmentForMessaging = clearanceWrapper.CusUSLVConsignmentsToSend.OfType<CusUSLVConsignmentForMessaging>().Single();
			consignmentForMessaging.SendToCustoms = true;

			using (var form = new MessagesSubmitForm(clearanceWrapper, UpdateActionCode.Add))
			{
				form.Show();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				form.AcceptButton.PerformClick();
				var lastMessage = UnitTestUserNotification.Instance.LastMessage;
				CombineAssertions(() =>
				{
					AssertMultilineASCIIEquals(@"The Entry Number Range of Branch 'BNE' (Entry Filer Code:'ABC') has not been setup correctly.
Please set it up in Maintain -> User Admin -> Companies -> Company 'EDI' -> Number Ranges.", lastMessage.Text);
					Assert(lastMessage.WasWarning);
					AssertNullOrEmpty(consignment1.CE_EntryNum);
				});
			}
		}

		public void TestGenerateEntryNumbersRightBeforeMessageSending_NotEnoughNumbers()
		{
			var clearance = Factory.New<CusUSLVClearance>();
			clearance.ULH_EntryFilerCode = "ABC";
			var consignment1 = clearance.CusUSLVConsignments.AddNew();
			var consignment2 = clearance.CusUSLVConsignments.AddNew();
			consignment1.InitAction(UpdateActionCode.Add);
			consignment2.InitAction(UpdateActionCode.Add);

			var stmNums = Factory.New<CustomsNumberViewStmNums>();
			stmNums.Provider = CustomsNumberViewStmNumsBusinessProviderHelper.GetProvider(Factory, clearance.Branch.Company.GC_RN_NKCountryCode, clearance.Branch.Company.PK);
			stmNums.SN_Owner = clearance.Branch.PK;
			stmNums.SN_Type = "ENS";
			stmNums.SN_Value = USCustomsNumberViewStmNumsSetting.USMaximumFormalEntryNumber;
			var wrapper = stmNums.Wrapper as USCustomsNumberViewStmNumsWrapper;
			wrapper.AppliesTo = "ABC";
			wrapper.IsBranchLevel = true;
			Factory.Save();

			var clearanceWrapper = new CusUSLVClearanceMessageWrapper(clearance);
			clearanceWrapper.CusUSLVConsignmentsToSend.OfType<CusUSLVConsignmentForMessaging>().ToList().ForEach(c => c.SendToCustoms = true);

			using (var form = new MessagesSubmitForm(clearanceWrapper, UpdateActionCode.Add))
			{
				form.Show();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				form.AcceptButton.PerformClick();
				var lastMessage = UnitTestUserNotification.Instance.LastMessage;
				CombineAssertions(() =>
				{
					AssertMultilineASCIIEquals(@"There is not enough entry numbers in Branch 'BNE' (Entry Filer Code:'ABC').
Please either increase the Branch's 'Entry Range' or contact Customs for a new range of numbers.", lastMessage.Text);
					Assert(lastMessage.WasWarning);
					AssertNullOrEmpty(consignment1.CE_EntryNum);
					AssertNullOrEmpty(consignment2.CE_EntryNum);
				});
			}
		}

		public void TestSendButton()
		{
			GlbStaff.CurrentUser.GS_IsController = false;
			var group = Factory.New<GlbGroup>();
			group.GG_Code = "ZZZ";
			var user = Factory.New<GlbStaff>();
			user.FillWithValidTestData();
			user.GS_Code = "YYY";
			user.GS_LoginName = "admin";
			user.GS_IsController = true;
			user.StaffPlainTextPassword = "password";
			user.GS_GB_HomeBranch = GlbBranch.CurrentBranch.PK;
			group.Staff.Add(user);
			Factory.Save();

			var clearance = Factory.New<CusUSLVClearance>();
			var consignment = clearance.CusUSLVConsignments.AddNew();
			consignment.CE_EntryStatus = CRLReleaseStatusList.Codes.ADM;
			consignment.ULB_HouseBill = "123456";
			consignment.ULB_MessageStatus = ImportMessageStatusList.Codes.AwaitingACECargoReleaseAdd;
			consignment.InitAction(UpdateActionCode.Replace);

			var clearanceWrapper = new CusUSLVClearanceMessageWrapper(clearance);
			using (var form = new MessagesSubmitForm(clearanceWrapper, UpdateActionCode.Replace))
			{
				form.Show();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);

				var consignmentForMessaging = clearanceWrapper.CusUSLVConsignmentsToSend.OfType<CusUSLVConsignmentForMessaging>().Single();

				AssertEquals(false, consignmentForMessaging.SendToCustoms);
				form.AcceptButton.PerformClick();
				AssertNull(ZFormModaliser.ActiveForm);
				AssertEquals("No Bill has been flagged for submission", UnitTestUserNotification.Instance.LastMessage.Text);

				consignmentForMessaging.SendToCustoms = true;
				Env.Security.USLVClearanceSendWithMessageErrors.IsAllowed = false;
				form.AcceptButton.PerformClick();
				AssertEquals("There are message errors on this job and you don't have security rights to send with message errors.", UnitTestUserNotification.Instance.LastMessage.Text);

				Env.Security.USLVClearanceSendWithMessageErrors.IsAllowed = true;
				form.AcceptButton.PerformClick();
				AssertNull(ZFormModaliser.ActiveForm);
				AssertEquals("Submission Cancelled By User", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(false, consignmentForMessaging.SendToCustoms);

				Env.Security.AllowMessageErrors.IsAllowed = false;
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				consignmentForMessaging.SendToCustoms = true;
				form.AcceptButton.PerformClick();
				AssertType<Customs.GUI.SupervisorOverridesForm>(ZFormModaliser.LastFormShownDialogForTest);
			}
		}

		public void TestSelectAllButtonClick_SelectDeselectAllHouseBills()
		{
			var clearance = Factory.New<CusUSLVClearance>();
			var consignment1 = clearance.CusUSLVConsignments.AddNew();
			consignment1.InitAction(UpdateActionCode.Add);
			var consignment2 = clearance.CusUSLVConsignments.AddNew();
			consignment2.InitAction(UpdateActionCode.Add);

			var clearanceWrapper = new CusUSLVClearanceMessageWrapper(clearance);
			var consignmentsForMessaging = clearanceWrapper.CusUSLVConsignmentsToSend.OfType<CusUSLVConsignmentForMessaging>();
			var consignmentForMessaging1 = consignmentsForMessaging.Single(c => c.Consignment.PK == consignment1.PK);
			var consignmentForMessaging2 = consignmentsForMessaging.Single(c => c.Consignment.PK == consignment2.PK);

			consignmentForMessaging1.SendToCustoms = false;
			consignmentForMessaging2.SendToCustoms = false;

			using (var form = new MessagesSubmitForm(clearanceWrapper, UpdateActionCode.Replace))
			{
				form.Show();

				var selectAllButton = form.FindSingleOrDefault<ZButton>("ButtonSelectAll");
				selectAllButton.PerformClick();

				CombineAssertions("select all consignments when they all unselected", () =>
				{
					Assert(consignmentForMessaging1.SendToCustoms);
					Assert(consignmentForMessaging2.SendToCustoms);
				});

				consignmentForMessaging1.SendToCustoms = true;
				consignmentForMessaging2.SendToCustoms = false;
				selectAllButton.PerformClick();

				CombineAssertions("select all consignments when they are not all selected", () =>
				{
					Assert(consignmentForMessaging1.SendToCustoms);
					Assert(consignmentForMessaging2.SendToCustoms);
				});

				consignmentForMessaging1.SendToCustoms = true;
				consignmentForMessaging2.SendToCustoms = true;
				selectAllButton.PerformClick();

				CombineAssertions("deSelect all consignments when they are all selected", () =>
				{
					Assert(!consignmentForMessaging1.SendToCustoms);
					Assert(!consignmentForMessaging2.SendToCustoms);
				});
			}
		}

		public void TestSelectAllValidButtonClick_SelectDeselectAllValidHouseBills()
		{
			var clearance = Factory.New<CusUSLVClearance>();
			var consignment1 = clearance.CusUSLVConsignments.AddNew();
			consignment1.InitAction(UpdateActionCode.Add);
			var consignment2 = clearance.CusUSLVConsignments.AddNew();
			consignment2.InitAction(UpdateActionCode.Add);
			var consignment3 = clearance.CusUSLVConsignments.AddNew();
			consignment3.InitAction(UpdateActionCode.Add);
			consignment3.AddRowMessageError("not valid");

			var clearanceWrapper = new CusUSLVClearanceMessageWrapper(clearance);
			var consignmentsForMessaging = clearanceWrapper.CusUSLVConsignmentsToSend.OfType<CusUSLVConsignmentForMessaging>();
			var consignmentForMessaging1 = consignmentsForMessaging.Single(c => c.Consignment.PK == consignment1.PK);
			var consignmentForMessaging2 = consignmentsForMessaging.Single(c => c.Consignment.PK == consignment2.PK);
			var consignmentForMessaging3 = consignmentsForMessaging.Single(c => c.Consignment.PK == consignment3.PK);

			consignmentForMessaging1.SendToCustoms = false;
			consignmentForMessaging2.SendToCustoms = false;
			consignmentForMessaging3.SendToCustoms = false;

			using (var form = new MessagesSubmitForm(clearanceWrapper, UpdateActionCode.Replace))
			{
				form.Show();

				var selectAllButton = form.FindSingleOrDefault<ZButton>("ButtonSelectAllValid");
				selectAllButton.PerformClick();

				CombineAssertions("select all valid consignments when they all unselected", () =>
				{
					Assert(consignmentForMessaging1.SendToCustoms);
					Assert(consignmentForMessaging2.SendToCustoms);
					Assert(!consignmentForMessaging3.SendToCustoms);
				});

				consignmentForMessaging1.SendToCustoms = true;
				consignmentForMessaging2.SendToCustoms = false;
				selectAllButton.PerformClick();

				CombineAssertions("select all valid consignments when they are not all selected", () =>
				{
					Assert(consignmentForMessaging1.SendToCustoms);
					Assert(consignmentForMessaging2.SendToCustoms);
					Assert(!consignmentForMessaging3.SendToCustoms);
				});

				consignmentForMessaging1.SendToCustoms = true;
				consignmentForMessaging2.SendToCustoms = true;

				selectAllButton.PerformClick();

				CombineAssertions("deSelect all consignments when they are all selected", () =>
				{
					Assert(!consignmentForMessaging1.SendToCustoms);
					Assert(!consignmentForMessaging2.SendToCustoms);
					Assert(!consignmentForMessaging3.SendToCustoms);
				});
			}
		}

		public void TestConsignmentGridColorContextKey()
		{
			var clearance = Factory.New<CusUSLVClearance>();
			using (var form = new MessagesSubmitForm(new CusUSLVClearanceMessageWrapper(clearance), UpdateActionCode.Add))
			{
				form.Show();

				var consignmentGrid = form.Controls.Find("GridCusUSLVConsignments", true).Single() as ZGrid;

				AssertEquals("CusUSLVConsignmentMessageSubmit", consignmentGrid.ColorContextKey);
			}
		}

		public void TestAdditionalColumns_NotVisibleForAdd()
		{
			TestAdditionalColumnsVisibilityForAction(UpdateActionCode.Add, expectVisible: false);
		}
		public void TestAdditionalColumns_NotVisibleForReplace()
		{
			TestAdditionalColumnsVisibilityForAction(UpdateActionCode.Replace, expectVisible: false);
		}
		public void TestAdditionalColumns_NotVisibleForUpdate()
		{
			TestAdditionalColumnsVisibilityForAction(UpdateActionCode.Update, expectVisible: false);
		}

		public void TestAdditionalColumns_VisibleForDelete()
		{
			TestAdditionalColumnsVisibilityForAction(UpdateActionCode.Delete, expectVisible: true);
		}

		void TestAdditionalColumnsVisibilityForAction(UpdateActionCode updateActionCode, bool expectVisible)
		{
			var clearance = Factory.New<CusUSLVClearance>();
			var consignment = clearance.CusUSLVConsignments.AddNew();
			consignment.InitAction(updateActionCode);

			using (var form = new MessagesSubmitForm(new CusUSLVClearanceMessageWrapper(clearance), updateActionCode))
			{
				form.Show();

				var consignmentGrid = form.Controls.Find("GridCusUSLVConsignments", true).Single() as ZGrid;

				CombineAssertions(() =>
				{
					AssertColumnVisibleForAction(consignmentGrid, CusUSLVConsignmentForMessaging.Schema.ReasonCode, updateActionCode, expectVisible);
					AssertColumnVisibleForAction(consignmentGrid, CusUSLVConsignmentForMessaging.Schema.RequiredReference, updateActionCode, expectVisible);
					AssertColumnVisibleForAction(consignmentGrid, CusUSLVConsignmentForMessaging.Schema.ReferenceNumber, updateActionCode, expectVisible);
					AssertColumnVisibleForAction(consignmentGrid, CusUSLVConsignmentForMessaging.Schema.FilesSubmittedToDIS, updateActionCode, expectVisible);
					AssertColumnVisibleForAction(consignmentGrid, CusUSLVConsignmentForMessaging.Schema.DISReference, updateActionCode, expectVisible);
				});
			}
		}

		void AssertColumnVisibleForAction(ZGrid grid, string columnName, UpdateActionCode updateActionCode, bool expectVisible)
		{
			AssertEquals($"{columnName} visible for {updateActionCode}", expectVisible, grid.Columns.Contains(columnName));
		}

		public void TestFormText()
		{
			var clearance = Factory.New<CusUSLVClearance>();
			var consignment = clearance.CusUSLVConsignments.AddNew();
			consignment.InitAction(UpdateActionCode.Add);

			using (var form = new MessagesSubmitForm(new CusUSLVClearanceMessageWrapper(clearance), UpdateActionCode.Add))
			{
				form.Show();
				AssertEquals("Low Value Entries Messages", form.Text);
			}
		}

		#region Implementation

		protected override Form GetFormToBashCore() => new MessagesSubmitForm(new CusUSLVClearanceMessageWrapper(Factory.New<CusUSLVClearance>()), UpdateActionCode.Delete);

		#endregion
	}
}
