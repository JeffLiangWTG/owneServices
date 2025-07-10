using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	sealed class MultiMessageManagerTest : TestCaseWithFactory
	{
		public void TestGetCriticalErrorsForCanSave()
		{
			using (TopLevel.SuspendValidationTesting())
			{
				TopLevel.Z0_AnotherDateInfo.AddError("Test Errors");
				AssertEquals("PreCondition:HasErrors", true, TopLevel.HasErrors);

				ZString criticalErrors = ((IMessageManager)Manager).GetCriticalErrorsForCanSaveExcludingMessagingLevelNotifications(new RequiredMessagesInformation(TopLevel));
				AssertEquals(true, criticalErrors.Contains("Test Errors"));
			}
		}

		#region Validation Testing

		public void TestValidation()
		{
			AssertNotNull(Manager.Validation);
			AssertEquals(TopLevel, Manager.Validation.TopLevelBusinessObjectForValidation);
			AssertEquals(true, Manager.Validation.MessageErrors is IEnumerable<INotification>);
			using (TopLevel.SuspendValidationTesting())
			{
				TopLevel.Z0_DescriptionInfo.AddMessageError("Description is bad.");
			}
			AssertEquals("Description: Description is bad.", Manager.Validation.MessageErrors.ToUniqueMessageListString());
		}

		#endregion

		#region SendOriginalMessages Testing

		public void TestSendMessagesForZeroMessages()
		{
			Manager.allMessageManagers = new TestHelperSingleMessageManager[] { SingleManager };
			bool result = Manager.SendOriginalMessages(Sender).Any();
			AssertEquals("SendMessagesResult", false, result);
		}

		public void TestSendMessagesWhenSendReturnsTrue()
		{
			Manager.allMessageManagers = new TestHelperSingleMessageManager[] { SingleManager };
			Manager.allMessageManagers[0].canSendOriginal = true;
			Sender.ReturnAllForWhichMessagesShouldWeSend = true;
			Manager.SendResultOverride = new[] { Factory.NewWithValidTestData<EDIMessage>() };

			AssertEquals("Children not registered editable yet", 0, ((IBusiness)TopLevel).Children.Length);
			bool result = Manager.SendOriginalMessages(Sender).Any();
			AssertEquals("SendMessagesResult", true, result);
			AssertEquals("Children registered editable before validating", true, ((IBusiness)TopLevel).Children.Length > 0);
		}

		public void TestSendMessagesCallsNotifyUserOfASuccessfulSend()
		{
			Manager.allMessageManagers = new TestHelperSingleMessageManager[] { SingleManager };
			Manager.allMessageManagers[0].canSendOriginal = true;
			Sender.ReturnAllForWhichMessagesShouldWeSend = true;
			bool result = Manager.SendOriginalMessages(Sender).Any();
			AssertEquals("SuccessfulSendOccured", true, Sender.SuccessfulSendOccured);
		}

		public void TestNotifyCannotSendDueToRequiredAmendment()
		{
			TestHelperSingleMessageManager otherManager = new TestHelperSingleMessageManager();
			otherManager.RequiresAmendmentCoreExposed = true;
			Manager.allMessageManagers = new TestHelperSingleMessageManager[] { SingleManager, otherManager };
			Manager.allMessageManagers[0].canSendOriginal = true;
			Sender.ReturnAllForWhichMessagesShouldWeSend = true;
			Manager.SendOriginalMessages(Sender);
			AssertEquals("No SuccessfulSendOccured", false, Sender.SuccessfulSendOccured);
			AssertEquals("Warning text", "Cannot send message(s) because there are other messages to be sent, or messages waiting for responses", Sender.Warning);

			Sender.Warning = string.Empty;
			otherManager.RequiresAmendmentCoreExposed = false;
			Manager.SendOriginalMessages(Sender);
			AssertEquals("SuccessfulSendOccured", true, Sender.SuccessfulSendOccured);
			AssertEquals("No warning text", string.Empty, Sender.Warning);
		}

		[GuiTest]
		public void TestNotifyCannotSendDueToDeniedParty()
		{
			var dec = Factory.New<MessageManageableJobDeclaration>();
			dec.DPSFreightMovementRestricted = true;
			dec.MessageInitiator = Sender;

			var multiMessageManager = new TestHelperMultiMessageManagerForTest(dec);
			multiMessageManager.allMessageManagers = new TestHelperSingleMessageManager[] { SingleManager };
			multiMessageManager.allMessageManagers[0].canSendOriginal = true;

			Sender.ReturnAllForWhichMessagesShouldWeSend = true;
			bool result = multiMessageManager.SendOriginalMessages(Sender).Any();
			AssertEquals("No SuccessfulSendOccured", false, Sender.SuccessfulSendOccured);
			AssertEquals("Warning text", "Unable to submit message due to Denied Party Screening cancellation.", Sender.InvalidOperationText);

			dec.DPSFreightMovementRestricted = false;
			Sender.InvalidOperationText = "";
			result = multiMessageManager.SendOriginalMessages(Sender).Any();
			AssertEquals("SuccessfulSendOccured", true, Sender.SuccessfulSendOccured);
			AssertEquals("No warning text", string.Empty, Sender.InvalidOperationText);
		}

		public void TestSendMessagesWhenSendReturnsFalse()
		{
			Manager.allMessageManagers = new TestHelperSingleMessageManager[] { SingleManager };
			Sender.ReturnAllForWhichMessagesShouldWeSend = true;
			Manager.SendResultOverride = Array.Empty<EDIMessage>();
			bool result = Manager.SendOriginalMessages(Sender).Any();
			AssertEquals("SendMessagesResult", false, result);
		}

		#endregion

		#region AmendMessages Testing

		public void TestAmendMessagesForZeroMessages()
		{
			Manager.allMessageManagers = new TestHelperSingleMessageManager[] { SingleManager };
			bool result = Manager.AmendMessages(Sender);
			AssertEquals("AmendMessagesResult", false, result);
		}

		public void TestAmendMessagesWhenAmendReturnsTrue()
		{
			Manager.allMessageManagers = new TestHelperSingleMessageManager[] { SingleManager };
			Manager.allMessageManagers[0].canSendWithdrawal = true;
			Sender.ReturnAllForWhichMessagesShouldWeSend = true;
			Manager.AmendResultOverride = true;

			AssertEquals("Children not registered editable yet", 0, ((IBusiness)TopLevel).Children.Length);
			bool result = Manager.AmendMessages(Sender);
			AssertEquals("AmendMessagesResult", true, result);
			AssertEquals("Children registered editable before validating", true, ((IBusiness)TopLevel).Children.Length > 0);
		}

		public void TestAmendMessagesWhenAmendReturnsFalse()
		{
			Manager.allMessageManagers = new TestHelperSingleMessageManager[] { SingleManager };
			Sender.ReturnAllForWhichMessagesShouldWeSend = true;
			Manager.AmendResultOverride = false;
			bool result = Manager.AmendMessages(Sender);
			AssertEquals("AmendMessagesResult", false, result);
		}

		public void TestAmend()
		{
			TestAmend(false, false);
			TestAmend(true, false);
			TestAmend(false, true);
			TestAmend(true, true);
		}

		#endregion

		#region WithdrawMessages Testing

		public void TestWithdrawMessagesForZeroMessages()
		{
			Manager.allMessageManagers = new TestHelperSingleMessageManager[] { SingleManager };
			bool result = Manager.WithdrawMessages(Sender);
			AssertEquals("WithdrawMessagesResult", false, result);
		}

		public void TestWithdrawMessagesWhenWithdrawReturnsTrue()
		{
			Manager.allMessageManagers = new TestHelperSingleMessageManager[] { SingleManager };
			Manager.allMessageManagers[0].canSendWithdrawal = true;
			Sender.ReturnAllForWhichMessagesShouldWeWithdraw = true;
			Manager.WithdrawResultOverride = true;

			AssertEquals("Children not registered editable yet", 0, ((IBusiness)TopLevel).Children.Length);
			bool result = Manager.WithdrawMessages(Sender);
			AssertEquals("WithdrawMessagesResult", true, result);
			AssertEquals("Children registered editable before validating", true, ((IBusiness)TopLevel).Children.Length > 0);
		}

		public void TestWithdrawMessagesWhenWithdrawReturnsFalse()
		{
			Manager.allMessageManagers = new TestHelperSingleMessageManager[] { SingleManager };
			Sender.ReturnAllForWhichMessagesShouldWeWithdraw = true;
			Manager.WithdrawResultOverride = false;
			bool result = Manager.WithdrawMessages(Sender);
			AssertEquals("WithdrawMessagesResult", false, result);
		}

		#endregion

		#region AmendmentWithdrawalReason
		public void TestAmendmentWithdrawalReason()
		{
			AssertNull(Manager.AmendmentWithdrawalReason);
			AmendmentWithdrawalReason reason = new AmendmentWithdrawalReason();
			Manager.AmendmentWithdrawalReason = reason;
			AssertEquals(reason, Manager.AmendmentWithdrawalReason);
			Manager.AmendmentWithdrawalReason.ReasonText = "Something";
			AssertEquals("Something", Manager.AmendmentWithdrawalReason.ReasonText);
		}

		#endregion

		public void TestAmendMessagesWithPassedEntries()
		{
			Manager.allMessageManagers = new TestHelperSingleMessageManager[] { SingleManagerWithChanges, SingleManager };
			Manager.InitialisedCalled = false;

			bool result = Manager.AmendMessages(Sender, new SingleMessageManager[] { SingleManagerWithChanges });
			AssertEquals("Initialise sets allManagers to null which leads to a different set of instances of sinlgeMessageManager and when GetManagersToCheck is called, it fails as it checks instances", false, Manager.InitialisedCalled);
			AssertEquals("Expect no problems for sending an amendment", true, result);
			AssertEquals("Only one amendment message is expected", 1, Manager.amendmentMessagesSent);
		}

		public void TestWithdrawlMessagesWithPassedEntries()
		{
			Manager.allMessageManagers = new TestHelperSingleMessageManager[] { SingleManagerWithChanges, SingleManager };
			Manager.InitialisedCalled = false;

			bool result = Manager.WithdrawMessages(Sender, new SingleMessageManager[] { SingleManagerWithChanges });
			AssertEquals("Initialise sets allManagers to null which leads to a different set of instances of sinlgeMessageManager and when GetManagersToCheck is called, it fails as it checks instances", false, Manager.InitialisedCalled);
			AssertEquals("Expect no problems for sending a withdrawal message", true, result);
			AssertEquals("Only one Withdrawal message is expected", 1, Manager.withdrawalMessagesSent);
		}

		public void TestAllNotificationsFromSingleMessageManagersAreCollected()
		{
			Manager.allMessageManagers = new TestHelperSingleMessageManager[] { SingleManagerWithChanges, SingleManager };

			List<SingleMessageManager> activeManagers = new List<SingleMessageManager>();
			activeManagers.Add(SingleManagerWithChanges);
			activeManagers.Add(SingleManager);

			SingleManagerWithChanges.AdditionalCommonNotifications.AddError("TEST");

			RequiredMessagesInformation result = Manager.GetRequiredMessagesInformationExceptForThosePassedIn(Array.Empty<SingleMessageManager>(), true);
			AssertEquals("All notifications have errors added for SingleManagersWithChanges", true, result.AllNotifications.ContainsError("TEST"));
		}

		public void TestResetToOriginalWarning()
		{
			AssertEquals("Warning - You are about to reset to original!\r\n", Manager.ResetToOriginalWarning);
		}

		public void TestQuestionsAskedOnSending()
		{
			TestHelperSingleMessageManager singleManager = new TestHelperSingleMessageManager();
			Manager.allMessageManagers = new TestHelperSingleMessageManager[] { singleManager };
			Manager.CanSendOverride = true;
			Manager.CanWithdrawOverride = true;
			Manager.CanAmendOverride = true;

			AssertEquals(false, singleManager.QueryActionWasHit);
			Manager.SendOriginal(Sender, Manager.allMessageManagers);
			AssertEquals("OnMessageSent", true, Manager.MessageSentCalled);
			AssertEquals(true, singleManager.QueryActionWasHit);
		}

		public void TestAllowManualAmendments()
		{
			AssertEquals("AllowManualAmendments", false, Manager.AllowManualAmendments);
		}

		public void TestSend()
		{
			TestSend(false, false);
			TestSend(true, false);
			TestSend(false, true);
			TestSend(true, true);
		}

		public void TestWithdraw()
		{
			TestWithdraw(false, false);
			TestWithdraw(true, false);
			TestWithdraw(false, true);
			TestWithdraw(true, true);
		}

		public void TestCanSaveWithNoChanges()
		{
			Sender.AnswerToContinueWithAction = false;
			Manager.allMessageManagers = new TestHelperSingleMessageManager[] { SingleManagerWithoutChanges };

			RequiredMessagesInformation detectionResult = Manager.DetermineRequiredMessagesWithPendingChanges();
			AssertEquals("HasMessagesToSend", false, detectionResult.HasMessagesToSend);
			Manager.SendAnyMessagesRequired(detectionResult);
			AssertEquals("GeneratedAmendments.Count", 0, Manager.amendmentMessagesSent);
			AssertEquals("OnOneOrMoreAmendmentsSent should not been called", false, Manager.AmendmentMessagesSentCalled);
		}

		public void TestCanSaveExcludingManagerInError()
		{
			SingleManagerWithChanges.IsWaitingForResponseExposed = true;
			Manager.allMessageManagers = new TestHelperSingleMessageManager[] { SingleManagerWithChanges };

			RequiredMessagesInformation detectionResult = Manager.GetRequiredMessagesInformationExceptForThosePassedIn(new SingleMessageManager[] { SingleManagerWithChanges }, false);
			AssertEquals("No manager is waiting for responses as SingleManagerWithChanges is passed to be excluded", false, detectionResult.HasMessageManagersWaitingForResponses);

			Manager.SendAnyMessagesRequired(detectionResult);
			AssertEquals("GeneratedAmendments.Count", 0, Manager.amendmentMessagesSent);
			AssertEquals("OnOneOrMoreAmendmentsSent should not been called", false, Manager.AmendmentMessagesSentCalled);
		}

		public void TestCanSaveWithAmendmentWithNoWarnings()
		{
			Manager.allMessageManagers = new TestHelperSingleMessageManager[] { SingleManagerWithChanges };
			Sender.AnswerToContinueWithAction = true;

			RequiredMessagesInformation detectionResult = Manager.DetermineRequiredMessagesWithPendingChanges();
			AssertEquals("HasMessagesToSend", true, detectionResult.HasMessagesToSend);
			Manager.SendAnyMessagesRequired(detectionResult);
			AssertEquals("GeneratedAmendments.Count", 1, Manager.amendmentMessagesSent);
			AssertEquals("OnOneOrMoreAmendmentsSent should have been called", true, Manager.AmendmentMessagesSentCalled);
			AssertEquals("OnOneOrMoreWithdrawalSent should not been called", false, Manager.WithdrawalMessageSentCalled);
		}

		public void TestSendAnyMessagesRequiredCallsOriginalMessageSent()
		{
			Manager.allMessageManagers = new TestHelperSingleMessageManager[] { SingleManagerWithoutChanges };
			Sender.AnswerToContinueWithAction = true;
			SingleManagerWithoutChanges.shouldSendOriginalOnSave = true;
			Manager.SendWheneverPossibleOnceMessagingActiveExposedCore = true;
			RequiredMessagesInformation detectionResult = Manager.DetermineRequiredMessagesWithPendingChanges();
			AssertEquals("CanSave", true, detectionResult.HasMessagesToSend);
			Manager.SendAnyMessagesRequired(detectionResult);
			AssertEquals("OnOneOrMoreOriginalSent should has been called", true, Manager.OriginalMessageSentCalled);
			AssertEquals("OnOneOrMoreAmendmentsSent should not been called", false, Manager.AmendmentMessagesSentCalled);
			AssertEquals("OnOneOrMoreWithdrawalSent should not been called", false, Manager.WithdrawalMessageSentCalled);
		}

		public void TestSendAnyMessagesRequiredCallsAmendmentMessageSent()
		{
			Manager.allMessageManagers = new TestHelperSingleMessageManager[] { SingleManagerWithoutChanges, SingleManagerWithChanges };
			Sender.AnswerToContinueWithAction = true;
			Manager.SendWheneverPossibleOnceMessagingActiveExposedCore = true;
			RequiredMessagesInformation detectionResult = Manager.DetermineRequiredMessagesWithPendingChanges();
			AssertEquals("CanSave", true, detectionResult.HasMessagesToSend);
			Manager.SendAnyMessagesRequired(detectionResult);
			AssertEquals("OnOneOrMoreOriginalSent should not been called", false, Manager.OriginalMessageSentCalled);
			AssertEquals("OnOneOrMoreAmendmentsSent should have been called", true, Manager.AmendmentMessagesSentCalled);
			AssertEquals("OnOneOrMoreWithdrawalSent should not been called", false, Manager.WithdrawalMessageSentCalled);
		}

		public void TestSendAnyMessagesRequiredCallsWithdrawalMessageSent()
		{
			Manager.allMessageManagers = new TestHelperSingleMessageManager[] { SingleManagerWithoutChanges };
			Sender.AnswerToContinueWithAction = true;
			SingleManagerWithoutChanges.shouldSendWithdrawalOnSave = true;
			Manager.SendWheneverPossibleOnceMessagingActiveExposedCore = true;
			RequiredMessagesInformation detectionResult = Manager.DetermineRequiredMessagesWithPendingChanges();
			AssertEquals("CanSave", true, detectionResult.HasMessagesToSend);
			Manager.SendAnyMessagesRequired(detectionResult);
			AssertEquals("OnOneOrMoreOriginalSent should not been called", false, Manager.OriginalMessageSentCalled);
			AssertEquals("OnOneOrMoreAmendmentsSent should not been called", false, Manager.AmendmentMessagesSentCalled);
			AssertEquals("OnOneOrMoreWithdrawalSent should have been called", true, Manager.WithdrawalMessageSentCalled);
		}

		public void TestSendAnyMessagesRequiredCallsMessagesSent()
		{
			Manager.allMessageManagers = new TestHelperSingleMessageManager[] { SingleManagerWithoutChanges, SingleManagerWithChanges, this.SingleManager };
			Sender.AnswerToContinueWithAction = true;
			SingleManagerWithoutChanges.shouldSendOriginalOnSave = true;
			this.SingleManager.shouldSendWithdrawalOnSave = true;
			Manager.SendWheneverPossibleOnceMessagingActiveExposedCore = true;
			RequiredMessagesInformation detectionResult = Manager.DetermineRequiredMessagesWithPendingChanges();
			AssertEquals("CanSave", true, detectionResult.HasMessagesToSend);
			Manager.SendAnyMessagesRequired(detectionResult);
			AssertEquals("OnOneOrMoreOriginalSent should have been called", true, Manager.OriginalMessageSentCalled);
			AssertEquals("OnOneOrMoreAmendmentsSent should have been called", true, Manager.AmendmentMessagesSentCalled);
			AssertEquals("OnOneOrMoreWithdrawalSent should have been called", true, Manager.WithdrawalMessageSentCalled);
		}

		public void TestCanSend()
		{
			Sender.AnswerToContinueWithAction = true;
			AssertEquals("CanSend", false, Manager.CanSendOriginal(Sender, SingleManagerWithErrors));
			AssertEquals("CanSend", true, Manager.CanSendOriginal(Sender, SingleManagerWithWarnings));
			Sender.AnswerToContinueWithAction = false;
			AssertEquals("CanSend", false, Manager.CanSendOriginal(Sender, SingleManagerWithWarnings));
		}

		public void TestSendWithMessageErrors_WithSecurityRight()
		{
			Env.Security.CustomsDeclarationSendWithMessageErrors.IsAllowed = false;
			Manager.allMessageManagers = new TestHelperSingleMessageManager[] { SingleManager };
			Manager.TopLevelBusinessObject.AddRowMessageError("TEST");
			AssertEquals("PreCondition: Top BizO has a message errors", true, Manager.TopLevelBusinessObject.HasMessageErrors);
			AssertEquals("CanSend", false, Manager.CanSendOriginal(Sender, SingleManager));

			Env.Security.CustomsDeclarationSendWithMessageErrors.IsAllowed = true;
			AssertEquals("PreCondition: Top BizO has a message errors", true, Manager.TopLevelBusinessObject.HasMessageErrors);
			AssertEquals("CanSend", true, Manager.CanSendOriginal(Sender, SingleManager));
		}

		public void TestSendWithMessageErrorsTopLevel()
		{
			Env.Security.CustomsDeclarationSendWithMessageErrors.IsAllowed = false;
			Manager.checkTopLevelBusinessObjectsChildrenOverride = false;
			Manager.allMessageManagers = new TestHelperSingleMessageManager[] { SingleManager };
			Manager.TopLevelBusinessObject.AddRowMessageError("TEST");
			AssertEquals("CanSend", false, Manager.CanSendOriginal(Sender, SingleManager));

			Manager.checkTopLevelBusinessObjectsChildrenOverride = true;
			AssertEquals("CanSend", false, Manager.CanSendOriginal(Sender, SingleManager));
		}

		public void TestSendWithMessageErrorsTopLevelsChildren()
		{
			Env.Security.CustomsDeclarationSendWithMessageErrors.IsAllowed = false;
			Manager.checkTopLevelBusinessObjectsChildrenOverride = false;
			Manager.allMessageManagers = new TestHelperSingleMessageManager[] { SingleManager };
			DummyBusinessObject topLevel = (DummyBusinessObject)Manager.TopLevelBusinessObject;
			topLevel.RegisterEditableChildObject(topLevel.Collection);
			DummyChildBusinessObject child = topLevel.Collection.AddNew();
			child.AddRowMessageError("foo");
			AssertEquals("CanSend", true, Manager.CanSendOriginal(Sender, SingleManager));

			Manager.checkTopLevelBusinessObjectsChildrenOverride = true;
			AssertEquals("CanSend", false, Manager.CanSendOriginal(Sender, SingleManager));
		}

		public void TestCanWithdrawWithErrors()
		{
			SingleManagerWithErrors.canSendWithdrawal = true;
			AssertEquals("CanWithdraw", false, Manager.CanWithdraw(Sender, SingleManagerWithErrors));
		}

		public void TestCanWithdrawWithWarningsAnswerYes()
		{
			Sender.AnswerToContinueWithAction = true;
			SingleManagerWithWarnings.canSendWithdrawal = true;
			AssertEquals("CanWithdraw", true, Manager.CanWithdraw(Sender, SingleManagerWithWarnings));
		}

		public void TestCanWithdrawWithWarningsAnswerNo()
		{
			Sender.AnswerToContinueWithAction = false;
			SingleManagerWithWarnings.canSendWithdrawal = true;
			Sender.AnswerToContinueWithAction = false;
			AssertEquals("CanWithdraw", false, Manager.CanWithdraw(Sender, SingleManagerWithWarnings));
		}

		public void TestGetAllManagersWithChanges()
		{
			SingleMessageManager[] result = Manager.GetAllManagersWithChanges(new SingleMessageManager[] { SingleManagerWithChanges, SingleManagerWithoutChanges }, Array.Empty<SingleMessageManager>());
			AssertEquals("Result.Length", 1, result.Length);
			AssertEquals("Result[0]", SingleManagerWithChanges, result[0]);
			result = Manager.GetAllManagersWithChanges(new SingleMessageManager[] { SingleManagerWithChanges, SingleManagerWithoutChanges }, new SingleMessageManager[] { SingleManagerWithChanges });
			AssertEquals("Result.Length", 0, result.Length);
		}

		public void TestGetAllAmendmentNotifications()
		{
			AssertEquals(0, Manager.GetAllAmendmentNotifications(SingleManagerWithChanges).Count);
			SingleManagerWithChanges.AdditionalAmendmentNotifications.Add(new MessageSendingError("message"));
			AssertEquals(2, Manager.GetAllAmendmentNotifications(SingleManagerWithChanges).Count);
		}

		public void TestGetAllOriginalNotifications()
		{
			AssertEquals(0, Manager.GetAllOriginalNotifications(SingleManager).Count);
			SingleManager.AdditionalOriginalNotifications.Add(new MessageSendingError("message"));
			AssertEquals(2, Manager.GetAllOriginalNotifications(SingleManager).Count);
		}

		public void TestGetAllWithdrawalNotifications()
		{
			SingleManager.canSendWithdrawal = true;
			AssertEquals(0, Manager.GetAllWithdrawalNotifications(SingleManager).Count);
			SingleManager.AdditionalWithdrawalNotifications.Add(new MessageSendingError("message"));
			AssertEquals(2, Manager.GetAllWithdrawalNotifications(SingleManager).Count);
		}

		public void TestDisplayErrorNotification()
		{
			Manager.DisplayErrorNotification(Sender, Warnings);
			AssertEquals("InvalidOperationText", "You may not continue due to one or more critical problems:\r\n\r\nmessage\r\n", Sender.InvalidOperationText);
		}

		public void TestAskUserToContinue()
		{
			Sender.AnswerToContinueWithAction = true;
			bool result = Manager.AskUserToContinue(Sender, Warnings);
			Assert(result);
			Sender.AnswerToContinueWithAction = false;
			result = Manager.AskUserToContinue(Sender, Warnings);
			Assert(!result);
			AssertEquals("ContinueWithActionMessage", "Do you wish to continue despite the following?\r\n\r\nmessage\r\n", Sender.ContinueWithActionMessage);
		}

		public void TestDeclarableManagers()
		{
			Manager.allMessageManagers = new TestHelperSingleMessageManager[] { SingleManagerWithChanges };
			Manager.allMessageManagers[0].canSendOriginal = false;
			AssertEquals("DeclarableManagers.Length", 0, Manager.DeclarableManagers.Length);
			Manager.allMessageManagers[0].canSendOriginal = true;
			AssertEquals("DeclarableManagers.Length", 1, Manager.DeclarableManagers.Length);
		}

		public void TestWithdrawableManagers()
		{
			TestHelperSingleMessageManager singleMessageManager = new TestHelperSingleMessageManager();
			Manager.allMessageManagers = new TestHelperSingleMessageManager[] { singleMessageManager };
			singleMessageManager.canSendWithdrawal = false;
			AssertEquals("WithdrawableManagers.Length", 0, Manager.WithdrawableManagers.Length);
			singleMessageManager.canSendWithdrawal = true;
			AssertEquals("WithdrawableManagers.Length", 1, Manager.WithdrawableManagers.Length);
		}

		public void TestOneOrMoreOriginalsSent()
		{
			Manager.Initialise();
			Manager.OneOrMoreOriginalsSent(1, Sender);
			AssertEquals("SuccessfulSendText", "1 original message has been generated.", Sender.SuccessfulSendText);
			AssertEquals("OnOneOrMoreOriginalSent should have been called", true, Manager.OriginalMessageSentCalled);
			Manager.OriginalMessageSentCalled = false;
			Manager.OneOrMoreOriginalsSent(2, Sender);
			AssertEquals("SuccessfulSendText", "2 original messages have been generated.", Sender.SuccessfulSendText);
			AssertEquals("OnOneOrMoreOriginalSent should have been called", true, Manager.OriginalMessageSentCalled);
			AssertEquals("No AfterSaveNotification should have been saved", 0, Manager.AfterSaveNotifications.Length);

			Manager.ShowNotificationsAfterSaveExposed = true;
			Manager.OriginalMessageSentCalled = false;
			Manager.OneOrMoreOriginalsSent(3, Sender);
			AssertEquals("OnOneOrMoreOriginalSent should have been called", true, Manager.OriginalMessageSentCalled);
			Assert("An AfterSaveNotification should have been saved", Manager.AfterSaveNotifications.Length > 0);
			AssertEquals("3 original messages have been generated.", Manager.AfterSaveNotifications.ToString());
		}

		public void TestOneOrMoreAmendmentsSent()
		{
			Manager.OneOrMoreAmendmentsSent(1, Sender);
			AssertEquals("SuccessfulSendText", "1 amendment message has been generated.", Sender.SuccessfulSendText);
			AssertEquals("OnOneOrMoreAmendmentsSent should have been called", true, Manager.AmendmentMessagesSentCalled);
			Manager.AmendmentMessagesSentCalled = false;
			Manager.OneOrMoreAmendmentsSent(2, Sender);
			AssertEquals("SuccessfulSendText", "2 amendment messages have been generated.", Sender.SuccessfulSendText);
			AssertEquals("OnOneOrMoreAmendmentsSent should have been called", true, Manager.AmendmentMessagesSentCalled);
		}

		public void TestOneOrMoreWithdrawalsSent()
		{
			Manager.Initialise();
			Manager.OneOrMoreWithdrawalsSent(1, Sender);
			AssertEquals("SuccessfulSendText", "1 withdrawal message has been generated.", Sender.SuccessfulSendText);
			AssertEquals("OnOneOrMoreWithdrawalsSent should have been called", true, Manager.WithdrawalMessageSentCalled);
			Manager.WithdrawalMessageSentCalled = false;
			Manager.OneOrMoreWithdrawalsSent(2, Sender);
			AssertEquals("SuccessfulSendText", "2 withdrawal messages have been generated.", Sender.SuccessfulSendText);
			AssertEquals("OnOneOrMoreWithdrawalsSent should have been called", true, Manager.WithdrawalMessageSentCalled);
			AssertEquals("No AfterSaveNotification should have been saved", 0, Manager.AfterSaveNotifications.Length);

			Manager.ShowNotificationsAfterSaveExposed = true;
			Manager.OriginalMessageSentCalled = false;
			Manager.OneOrMoreWithdrawalsSent(3, Sender);
			AssertEquals("OnOneOrMoreOriginalSent should have been called", true, Manager.WithdrawalMessageSentCalled);
			Assert("An AfterSaveNotification should have been saved", Manager.AfterSaveNotifications.Length > 0);
			AssertEquals("3 withdrawal messages have been generated.", Manager.AfterSaveNotifications.ToString());
		}

		public void TestResetToOriginal()
		{
			Manager.allMessageManagers = new TestHelperSingleMessageManager[] { SingleManagerWithChanges };
			Sender.ReturnAllForWhichMessagesShouldWeReset = false;
			Manager.ResetToOriginal(Sender);
			AssertEquals("ResetToOriginalCalled", false, Manager.allMessageManagers[0].ResetToOriginalCalled);
			Sender.ReturnAllForWhichMessagesShouldWeReset = true;
			Manager.ResetToOriginal(Sender);
			AssertEquals("ResetToOriginalCalled", true, Manager.allMessageManagers[0].ResetToOriginalCalled);
		}

		public void TestResetToOriginal_WithSecurityRight()
		{
			Env.Security.CustomsResetToOriginal.IsAllowed = false;
			Manager.allMessageManagers = new TestHelperSingleMessageManager[] { SingleManager };
			Sender.ReturnAllForWhichMessagesShouldWeReset = true;
			Manager.ResetToOriginal(Sender);
			AssertEquals("ResetToOriginalCalled", false, Manager.allMessageManagers[0].ResetToOriginalCalled);
			string securityWarning = @"You do not have the appropriate security rights to run this function.

If you require access to this function, ask your system administrator to change either your Staff or Group Security Rights to allow access to:

Operate -> Customs -> Customs Declarations -> Reset to Original";
			AssertEquals(securityWarning, UnitTestUserNotification.Instance.LastMessage.Text);
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

			Env.Security.CustomsResetToOriginal.IsAllowed = true;
			Manager.ResetToOriginal(Sender);
			AssertEquals("ResetToOriginalCalled", true, Manager.allMessageManagers[0].ResetToOriginalCalled);
		}

		public void TestOnMessageSentCalled()
		{
			Manager.allMessageManagers = new TestHelperSingleMessageManager[] { SingleManager };
			Manager.CanSendOverride = true;
			Manager.CanWithdrawOverride = true;
			Manager.CanAmendOverride = true;

			Manager.SendOriginal(Sender, Manager.allMessageManagers);
			AssertEquals("OnMessageSent", true, Manager.MessageSentCalled);

			Manager.MessageSentCalled = false;
			Manager.Amend(Sender, Manager.allMessageManagers);
			AssertEquals("OnMessageSent", true, Manager.MessageSentCalled);

			Manager.MessageSentCalled = false;
			Manager.Withdraw(Sender, Manager.allMessageManagers);
			AssertEquals("OnMessageSent", true, Manager.MessageSentCalled);
		}

		public void TestMessagingApplicationName()
		{
			AssertEquals("MessagingApplicationName", "Messaging", Manager.MessagingApplicationName);
		}

		public void TestFactory()
		{
			AssertEquals("Factory", Manager.TopLevelBusinessObject.Factory, Manager.Factory);
		}

		public void TestShouldSendMessagesInTestModeDefault()
		{
			Assert(!Manager.ShouldSendMessagesInTestModeForTesting);
		}

		public void TestDelayListChangedEvents()
		{
			CombineAssertions(() =>
			{
				ManagerForDelayListChangedEvents.SendOriginal(Sender, Manager.allMessageManagers);
				ManagerForDelayListChangedEvents.Amend(Sender, Manager.allMessageManagers);
				ManagerForDelayListChangedEvents.Withdraw(Sender, Manager.allMessageManagers);
			});
		}

		TestHelperMultiMessageManagerForTest ManagerForDelayListChangedEvents
		{
			get
			{
				var topl = Factory.New<MessageManageableBusinessObjectForDelayListChangedEvents>();
				var manager = new TestHelperMultiMessageManagerForTest(topl);
				manager.allMessageManagers = new TestHelperSingleMessageManager[] { SingleManager };
				manager.CanSendOverride = false;
				manager.CanWithdrawOverride = false;
				manager.CanAmendOverride = false;
				return manager;
			}
		}

		SendsMessagesToCustomsShutterUpperer sender;
		SendsMessagesToCustomsShutterUpperer Sender
		{
			get
			{
				if (sender == null)
				{
					sender = new SendsMessagesToCustomsShutterUpperer(false);
				}
				return sender;
			}
		}

		MessageSendingNotificationCollection warnings;
		MessageSendingNotificationCollection Warnings
		{
			get
			{
				if (warnings == null)
				{
					warnings = new MessageSendingNotificationCollection();
					warnings.Add(new MessageSendingWarning("message"));
				}
				return warnings;
			}
		}

		TestHelperSingleMessageManager singleManager;
		TestHelperSingleMessageManager SingleManager
		{
			get
			{
				if (singleManager == null)
				{
					singleManager = new TestHelperSingleMessageManager();
				}
				return singleManager;
			}
		}

		TestHelperSingleMessageManager singleManagerWithoutChanges;
		TestHelperSingleMessageManager SingleManagerWithoutChanges
		{
			get
			{
				if (singleManagerWithoutChanges == null)
				{
					singleManagerWithoutChanges = new TestHelperSingleMessageManager();
					singleManagerWithoutChanges.BusinessObject.Factory.Save();
					singleManagerWithoutChanges.canSendWithdrawal = true;
				}
				return singleManagerWithoutChanges;
			}
		}

		TestHelperSingleMessageManager singleManagerWithChanges;
		TestHelperSingleMessageManager SingleManagerWithChanges
		{
			get
			{
				if (singleManagerWithChanges == null)
				{
					singleManagerWithChanges = new TestHelperSingleMessageManager();
					singleManagerWithChanges.BusinessObject.Factory.Save();
					singleManagerWithChanges.canSendWithdrawal = true;
					singleManagerWithChanges.ReturnDifferentOriginalMessages = true;
				}
				return singleManagerWithChanges;
			}
		}

		TestHelperSingleMessageManager singleManagerWithErrors;
		TestHelperSingleMessageManager SingleManagerWithErrors
		{
			get
			{
				if (singleManagerWithErrors == null)
				{
					singleManagerWithErrors = new TestHelperSingleMessageManager();
					singleManagerWithErrors.AdditionalCommonNotifications.Add(new MessageSendingError("message"));
				}
				return singleManagerWithErrors;
			}
		}

		TestHelperSingleMessageManager singleManagerWithWarnings;
		TestHelperSingleMessageManager SingleManagerWithWarnings
		{
			get
			{
				if (singleManagerWithWarnings == null)
				{
					singleManagerWithWarnings = new TestHelperSingleMessageManager();
					singleManagerWithWarnings.AdditionalCommonNotifications.Add(new MessageSendingWarning("message"));
				}
				return singleManagerWithWarnings;
			}
		}

		TestHelperMultiMessageManagerForTest fManager;
		TestHelperMultiMessageManagerForTest Manager
		{
			get
			{
				if (fManager == null)
				{
					fManager = new TestHelperMultiMessageManagerForTest(TopLevel);
				}
				return fManager;
			}
		}

		MessageManageableBusinessObject topLevel;
		MessageManageableBusinessObject TopLevel
		{
			get
			{
				if (topLevel == null)
				{
					topLevel = Factory.New<MessageManageableBusinessObject>();
				}
				return topLevel;
			}
		}

		void TestSend(bool canSend, bool canSave)
		{
			Manager.CanSendOverride = canSend;
			Manager.CanSaveOverride = canSave;
			bool result = Manager.SendOriginal(Sender, new SingleMessageManager[] { SingleManager }).Any();
			AssertEquals("SendResult", canSend && canSave, result);
			AssertEquals("GeneratedOriginalMessages", result, Manager.originalMessagesSent > 0);
		}

		void TestAmend(bool canAmend, bool canSave)
		{
			Manager.CanAmendOverride = canAmend;
			Manager.CanSaveOverride = canSave;
			SingleManager.canSendWithdrawal = canAmend;
			bool result = Manager.Amend(Sender, new SingleMessageManager[] { SingleManager });
			AssertEquals("AmendResult", canAmend && canSave, result);
			AssertEquals("GeneratedAmendMessages", result, Manager.amendmentMessagesSent > 0);
		}

		void TestWithdraw(bool canWithdraw, bool canSave)
		{
			Manager.CanWithdrawOverride = canWithdraw;
			Manager.CanSaveOverride = canSave;
			SingleManager.canSendWithdrawal = canWithdraw;
			bool result = Manager.Withdraw(Sender, new SingleMessageManager[] { SingleManager });
			AssertEquals("WithdrawResult", canWithdraw && canSave, result);
			AssertEquals("GeneratedWithdrawMessages", result, Manager.withdrawalMessagesSent > 0);
		}

		public class MessageManageableBusinessObjectForDelayListChangedEvents : MessageManageableBusinessObject
		{
			public MessageManageableBusinessObjectForDelayListChangedEvents(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			protected override void RunPreSaveValidationCore()
			{
				Assert(Factory.AreCollectionListChangedEventsDelayed);
				base.RunPreSaveValidationCore();
			}
		}
	}

	class TestHelperMultiMessageManagerForTest : TestHelperMultiMessageManager
	{
		public TestHelperMultiMessageManagerForTest(IMessageManageableBizObj topLevelBusinessObject) : base(topLevelBusinessObject)
		{
		}

		new internal IList<EDIMessage> SendOriginal(ISendsMessagesToCustoms sender, SingleMessageManager[] messagesToSendManagers) => base.SendOriginal(sender, messagesToSendManagers);
		new internal bool CanSendOriginal(ISendsMessagesToCustoms sender, params SingleMessageManager[] managersToSend) => base.CanSendOriginal(sender, managersToSend);
		new internal bool CanWithdraw(ISendsMessagesToCustoms sender, params SingleMessageManager[] managers) => base.CanWithdraw(sender, managers);
		new internal void Initialise() => base.Initialise();
		new internal bool Amend(ISendsMessagesToCustoms sender, SingleMessageManager[] messagesToSendManagers) => base.Amend(sender, messagesToSendManagers);
		new internal bool Withdraw(ISendsMessagesToCustoms sender, SingleMessageManager[] messagesToSendManagers) => base.Withdraw(sender, messagesToSendManagers);
	}
}
