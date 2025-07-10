using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Messaging.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.Business
{
	public abstract class MultiMessageManager : IMessageManager
	{
		#region Constructor

		protected MultiMessageManager()
		{
		}

		public abstract IMessageManageableBizObj TopLevelBizObjToManage { get; }

		public BusinessObject TopLevelBusinessObject
		{
			get { return TopLevelBizObjToManage as BusinessObject; }
		}

		#endregion

		#region Validation

		public MessageSendingValidation Validation
		{
			get
			{
				return TopLevelBusinessObjectForValidation == null ?
					MessageSendingValidation.New(null, null) :
					MessageSendingValidation.New(TopLevelBusinessObjectForValidation, GetNewMessageErrorCollector());
			}
		}

		protected virtual IEnumerable<INotification> GetNewMessageErrorCollector()
		{
			return new CustomsNotificationCollector(TopLevelBusinessObjectForValidation, true, false, CustomsNotificationCollector.PropertyDescriptionType.HumanReadableName).GetMessageErrors();
		}

		protected virtual BusinessObject TopLevelBusinessObjectForValidation => TopLevelBusinessObject;

		protected virtual bool RunPreSaveValidationWhenSendingAMessage
		{
			get { return true; }
		}

		#endregion

		#region Public Interface

		public void ResetToOriginal(ISendsMessagesToCustoms sender)
		{
			if (AllowResetToOriginal())
			{
				ResetToOriginalCore(sender);
			}
			else
			{
				Env.Security.ShowError(ResetToOriginalSecurityCheckpoint);
			}
		}

		bool AllowResetToOriginal()
		{
			return ResetToOriginalSecurityCheckpoint?.IsAllowed ?? true;
		}

		protected virtual SecurityCheckpoint ResetToOriginalSecurityCheckpoint
		{
			get
			{
				return Env.Security.CustomsResetToOriginal;
			}
		}

		protected virtual void ResetToOriginalCore(ISendsMessagesToCustoms sender)
		{
			Initialise();
			SingleMessageManager[] messageManagersToReset = sender.WhichMessagesShouldWeReset(AllMessageManagers);
			if (messageManagersToReset.Length > 0)
			{
				if (sender.ShowUserConfirmation(ResetToOriginalWarning, Res.GetString("fe3405e9-f9e8-40eb-bd54-2597d690b9d9", "Warning - Reset to original?"), Res.GetString("952f3929-bb4f-4e0e-9f43-21b55e41e4af", "If you are sure you want to reset the message to original please type:") + " ", Res.GetString("6d5be67f-f143-4032-a982-d0acf04fdd98", "I UNDERSTAND THE CONSEQUENCE OF USING RESET TO ORIGINAL INCORRECTLY")))
				{
					foreach (SingleMessageManager messageManager in messageManagersToReset)
					{
						messageManager.ResetToOriginalWithLog(TopLevelBusinessObject);
					}
					SaveFactoryAfterSendingMessages(sender);
				}
			}
		}

		protected void SaveFactoryAfterSendingMessages(ISendsMessagesToCustoms sender)
		{
			try
			{
				if (TopLevelBusinessObject != null)
				{
					TopLevelBusinessObject.Factory.Save();
				}
				else
				{
					throw new NotSupportedException("TopLevelBusinessObject is null and cannot perform Factory.Save()");
				}
			}
			catch (ZSaveException e)
			{
				ZString notificationMessage = Res.GetString("5548a357-e63e-4ddf-bbb2-20d10cd13561", "Have to close and reopen since:\r\n{0}", e.Message);
				sender.WarnUserAboutSomething(notificationMessage, Res.GetString("c96d1080-c14f-4f39-98ce-668fc3bbda1f", "Unable to Save"));
			}
		}

		protected virtual internal string ResetToOriginalWarning
		{
			get { return Res.GetString("ac0cde65-65b9-473e-8d53-46308f44ad0c", "Warning - You are about to reset to original!") + "\r\n"; }
		}

		/// <summary>
		/// Called from a menu only - Menu should be called 'Send Messages'
		/// By Default, will show a dialog containing objects for which original messages may be sent
		/// Override SendWheneverPossibleOnceMessagingActive if sending / withdrawing are atomic actions
		/// </summary>
		/// <param name="sender"></param>
		/// <returns>True if any messages generated</returns>
		public virtual IList<EDIMessage> SendOriginalMessages(ISendsMessagesToCustoms sender)
		{
			Initialise();
			SingleMessageManager[] messagesToSend;
			if (SendWheneverPossibleOnceMessagingActive)
			{
				messagesToSend = DeclarableManagers;
			}
			else
			{
				messagesToSend = sender.WhichMessagesShouldWeSend(DeclarableManagers);
			}
			if (messagesToSend.Length > 0)
			{
				return SendOriginal(sender, messagesToSend);
			}
			else
			{
				return Array.Empty<EDIMessage>();
			}
		}

		/// <summary>
		/// Check RequiredMessagesInformation.HasMessagesToSend == false, then no messages are required to be sent and can be safely saved
		/// RequiredMessagesInformation.HasMessagesToSend == true, 
		/// then GUI is responsible for the following actions
		/// 1/ Check and show notifications before sending messages
		/// 2/ Get a saving option from users including back-door saving if required
		/// 3/ Take an Amendment/Withdrawal reason from users if required
		/// 4/ Call SendAnyMessagesRequired passing RequiredMessagesInformation and save
		/// 5/ For the actions above, if users cancel at any point, then Save should be cancelled.
		/// </summary>
		public
#if DEBUG
 virtual
#endif
 RequiredMessagesInformation DetermineRequiredMessagesWithPendingChanges()
		{
			Initialise();
			RequiredMessagesInformation result = null;

			if (TopLevelBizObjToManage != null)
			{
				if (TopLevelBizObjToManage.IsInAStatusAmendmentSendable &&
					TopLevelBizObjToManage.ProcessBeforeDetectingAmendmentAndContinue() == ContinueWithDetection.Yes)
				{
					result = GetRequiredMessagesInformationExceptForThosePassedIn(Array.Empty<SingleMessageManager>(), true, GetAllSingleMessageManagersDelegateForAmendmentDetection);
				}

				if (result == null)
				{
					result = new RequiredMessagesInformation(TopLevelBizObjToManage);
				}
			}
			else
			{
				throw new NotSupportedException("TopLevelBusinessObject is null");
			}

			return result;
		}

		public MessageGenerationResultCollection SendAnyMessagesRequired(RequiredMessagesInformation detectionResult)
		{
			return SendAnyMessagesRequiredCore(detectionResult);
		}

		MessageGenerationResultCollection SendAnyMessagesRequiredCore(RequiredMessagesInformation detectionResult)
		{
			List<EDIMessage> originalMessages = new List<EDIMessage>();
			List<EDIMessage> amendmentMessages = new List<EDIMessage>();
			List<EDIMessage> withdrawalMessages = new List<EDIMessage>();

			foreach (ManagerCollector collector in detectionResult.ManagerCollectors)
			{
				collector.DoAction();

				if (collector.IsOriginal)
				{
					originalMessagesSent += collector.GeneratedMessages.Length;
					originalMessages.AddRange(collector.GeneratedMessages);
				}
				else if (collector.IsAmendment)
				{
					amendmentMessagesSent += collector.GeneratedMessages.Length;
					amendmentMessages.AddRange(collector.GeneratedMessages);
				}
				else if (collector.IsWithdrawal)
				{
					withdrawalMessagesSent += collector.GeneratedMessages.Length;
					withdrawalMessages.AddRange(collector.GeneratedMessages);
				}
			}

			OneOrMoreOriginalsSent(originalMessagesSent, null);
			OneOrMoreAmendmentsSent(amendmentMessagesSent, null);
			OneOrMoreWithdrawalsSent(withdrawalMessagesSent, null);

			MessageGenerationResultCollection result = new MessageGenerationResultCollection();
			if (originalMessagesSent > 0)
			{
				result.Add(Res.GetString("ba78601c-e70b-4566-a52f-68d562beec2e", "original"), originalMessagesSent, originalMessages);
			}

			if (amendmentMessagesSent > 0)
			{
				result.Add(Res.GetString("61f8e8d6-48ec-4258-8bdc-5a2b942afd8f", "amendment"), amendmentMessagesSent, amendmentMessages);
			}

			if (withdrawalMessagesSent > 0)
			{
				result.Add(Res.GetString("21f37146-4e5e-44b5-8d8c-15e96277371e", "withdrawal"), withdrawalMessagesSent, withdrawalMessages);
			}

			return result;
		}

		public virtual bool AmendMessages(ISendsMessagesToCustoms sender)
		{
			Initialise();
			SingleMessageManager[] messagesToAmend;
			if (SendWheneverPossibleOnceMessagingActive)
			{
				messagesToAmend = WithdrawableManagers;
			}
			else
			{
				messagesToAmend = sender.WhichMessagesShouldWeSend(WithdrawableManagers);
			}
			if (messagesToAmend.Length > 0)
			{
				return Amend(sender, messagesToAmend);
			}
			return false;
		}

		public bool AmendMessages(ISendsMessagesToCustoms sender, SingleMessageManager[] messagesToAmend)
		{
			InitialiseMessageCountOnly();
			if (messagesToAmend.Length > 0)
			{
				return Amend(sender, messagesToAmend);
			}
			return false;
		}

		/// <summary>
		/// Called from a menu only - Menu should be called 'Withdraw Messages'
		/// By Default, will show a dialog containing objects for which Withdrawal messages may be sent
		/// Override SendWheneverPossibleOnceMessagingActive if sending / withdrawing are atomic actions
		/// </summary>
		/// <param name="sender"></param>
		/// <returns>True if any messages generated</returns>
		public virtual bool WithdrawMessages(ISendsMessagesToCustoms sender)
		{
			Initialise();
			SingleMessageManager[] messagesToWithdraw;
			if (SendWheneverPossibleOnceMessagingActive)
			{
				messagesToWithdraw = WithdrawableManagers;
			}
			else
			{
				messagesToWithdraw = sender.WhichMessagesShouldWeWithdraw(WithdrawableManagers);
			}
			if (messagesToWithdraw.Length > 0)
			{
				return Withdraw(sender, messagesToWithdraw);
			}
			return false;
		}

		public bool WithdrawMessages(ISendsMessagesToCustoms sender, SingleMessageManager[] messagesToWithdraw)
		{
			InitialiseMessageCountOnly();
			if (messagesToWithdraw.Length > 0)
			{
				return Withdraw(sender, messagesToWithdraw);
			}
			return false;
		}

		public virtual ZString MessagingApplicationName
		{
			get
			{
				return (NoResString)"Messaging";
			}
		}

		public BusinessObjectFactory Factory
		{
			get
			{
				return TopLevelBusinessObject.Factory;
			}
		}

		public virtual bool AllowManualAmendments
		{
			get { return false; }
		}

		public AmendmentWithdrawalReason AmendmentWithdrawalReason
		{
			get { return fAmendmentWithdrawalReason; }
			set { fAmendmentWithdrawalReason = value; }
		}

		#endregion

		#region Implementation

		internal void OneOrMoreOriginalsSent(int messageCount, ISendsMessagesToCustoms sender)
		{
			if (messageCount > 0)
			{
				if (sender != null)
				{
					ConfirmMessagesSent(messageCount, sender, OriginalMessageTypeUsedInConfirmation);
				}

				OnOneOrMoreOriginalsSent();
			}
		}

		internal void OneOrMoreWithdrawalsSent(int messageCount, ISendsMessagesToCustoms sender)
		{
			if (messageCount > 0)
			{
				if (sender != null)
				{
					ConfirmMessagesSent(messageCount, sender, WithdrawalMessageTypeUsedInConfirmation);
				}

				OnOneOrMoreWithdrawalsSent();
			}
		}

		internal void OneOrMoreAmendmentsSent(int messageCount, ISendsMessagesToCustoms sender)
		{
			if (messageCount > 0)
			{
				if (sender != null)
				{
					ConfirmMessagesSent(messageCount, sender, AmendmentMessageTypeUsedInConfirmation);
				}

				OnOneOrMoreAmendmentsSent();
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "message type identifier")]
		protected void ConfirmMessagesSent(int messageCount, ISendsMessagesToCustoms sender, string typeOfMessage)
		{
			if (messageCount > 0)
			{
				ZString notification = ZString.Empty;
				if (messageCount == 1)
				{
					notification = Res.GetString("7be207b8-3c61-47c9-be57-404e6a4a5656", "1 {0} message has been generated.", typeOfMessage);
				}
				else
				{
					notification = Res.GetString("18b82f7c-9342-46aa-9f5c-504a11b29e66", "{0} {1} messages have been generated.", messageCount.ToString(), typeOfMessage);
				}

				if (!AdditionalMessageOnOriginalAndAmendment.IsEmpty && (typeOfMessage == "original" || typeOfMessage == "amendment"))
				{
					notification += "\r\n" + AdditionalMessageOnOriginalAndAmendment;
				}

				if (ShowNotificationsAfterSave)
				{
					AfterSaveNotifications.Append(notification);
				}
				else
				{
					sender.NotifyUserOfASuccessfulSend(notification);
				}
			}
		}

		protected virtual string OriginalMessageTypeUsedInConfirmation
		{
			get { return Res.GetString("cc66778a-461a-4a4d-872f-e68016d169f0", "original"); }
		}

		protected virtual string WithdrawalMessageTypeUsedInConfirmation
		{
			get { return Res.GetString("6620adbf-f064-4066-9df0-c8c9efd13a6e", "withdrawal"); }
		}

		protected virtual string AmendmentMessageTypeUsedInConfirmation
		{
			get { return Res.GetString("baacd019-e012-4fc4-97f0-e7c97b0cdbef", "amendment"); }
		}

		protected virtual ZString AdditionalMessageOnOriginalAndAmendment
		{
			get { return ""; }
		}

		protected virtual void OnOneOrMoreOriginalsSent()
		{
		}

		protected virtual void OnOneOrMoreWithdrawalsSent()
		{
		}

		protected virtual void OnOneOrMoreAmendmentsSent()
		{
		}

		protected virtual void OnMessagesSent(ISendsMessagesToCustoms sender)
		{
		}

		protected virtual void OnMessagesSending(SingleMessageManager[] singleMessageManagers)
		{
		}

		protected abstract bool SendWheneverPossibleOnceMessagingActive { get; }

		internal SingleMessageManager[] WithdrawableManagers
		{
			get
			{
				ArrayList result = new ArrayList();
				foreach (SingleMessageManager manager in AllMessageManagers)
				{
					if (manager.CanSendWithdrawal)
					{
						result.Add(manager);
					}
				}
				return (SingleMessageManager[])result.ToArray(typeof(SingleMessageManager));
			}
		}

		protected internal SingleMessageManager[] DeclarableManagers
		{
			get
			{
				ArrayList result = new ArrayList();
				foreach (SingleMessageManager manager in AllMessageManagers)
				{
					if (manager.CanSendOriginal)
					{
						result.Add(manager);
					}
				}
				return (SingleMessageManager[])result.ToArray(typeof(SingleMessageManager));
			}
		}

		/// <summary>
		/// DO NOT OVERRIDE - Virtual for testing only.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="messagesToSendManagers"></param>
		/// <returns></returns>
		protected
#if DEBUG
		virtual
#endif
		IList<EDIMessage> SendOriginal(ISendsMessagesToCustoms sender, SingleMessageManager[] messagesToSendManagers)
		{
			TopLevelBORunPreSaveValidation();

			bool result = CanSendOriginal(sender, messagesToSendManagers) && CanAllMangersBeSavedExceptThosePassedIn(sender, Array.Empty<SingleMessageManager>(), false) && CheckDeniedParty(TopLevelBusinessObject);
			if (result)
			{
				AskQueryQuestions(sender, messagesToSendManagers);
				OnMessagesSending(messagesToSendManagers);
				OriginalManagerCollector collector = new OriginalManagerCollector(messagesToSendManagers, true);
				collector.DoAction();
				originalMessagesSent += collector.GeneratedMessages.Length;
				OneOrMoreOriginalsSent(collector.GeneratedMessages.Length, sender);
				OnMessagesSent(sender);
				return collector.GeneratedMessages;
			}

			return Array.Empty<EDIMessage>();
		}

		void AskQueryQuestions(ISendsMessagesToCustoms sender, SingleMessageManager[] messagesToSendManagers)
		{
			foreach (SingleMessageManager manager in messagesToSendManagers)
			{
				foreach (MessageSendingQuery query in manager.GetQueriesForSending())
				{
					query.Delegate(sender.YesNoQuery(query.Question, query.Caption));
				}
			}
		}

		protected
#if DEBUG
 virtual
#endif
 bool Amend(ISendsMessagesToCustoms sender, SingleMessageManager[] messagesToSendManagers)
		{
			TopLevelBORunPreSaveValidation();

			bool result = CanAmend(sender, messagesToSendManagers) && CanAllMangersBeSavedExceptThosePassedIn(sender, messagesToSendManagers, false);
			if (result)
			{
				AmendmentManagerCollector collector = new AmendmentManagerCollector(messagesToSendManagers, true);
				collector.DoAction();
				amendmentMessagesSent += collector.GeneratedMessages.Length;
				OneOrMoreAmendmentsSent(collector.GeneratedMessages.Length, sender);
				OnMessagesSent(sender);
			}
			return result;
		}

		protected
#if DEBUG
 virtual
#endif
 bool Withdraw(ISendsMessagesToCustoms sender, SingleMessageManager[] messagesToSendManagers)
		{
			TopLevelBORunPreSaveValidation();

			bool result = CanWithdraw(sender, messagesToSendManagers) && CanAllMangersBeSavedExceptThosePassedIn(sender, messagesToSendManagers, false);
			if (result)
			{
				WithdrawalManagerCollector collector = new WithdrawalManagerCollector(messagesToSendManagers, true);
				collector.DoAction();
				withdrawalMessagesSent += collector.GeneratedMessages.Length;
				OneOrMoreWithdrawalsSent(collector.GeneratedMessages.Length, sender);
				OnMessagesSent(sender);
			}
			return result;
		}

		void TopLevelBORunPreSaveValidation()
		{
			if (RunPreSaveValidationWhenSendingAMessage)
			{
				TopLevelBusinessObject.LoadChildEditableObjects();
				using (ActiveBusinessObjectCollection.DelayListChangedEvents(Factory))
				{
					TopLevelBusinessObject.RunPreSaveValidation();
				}
			}
		}

		protected ManagerCollector[] GetCollectors(SingleMessageManager[] managersToCheck)
		{
			if (SendWheneverPossibleOnceMessagingActive)
			{
				return new ManagerCollector[] {
												  new OriginalManagerCollector(managersToCheck, false),
												  new AmendmentManagerCollector(managersToCheck, false),
												  new WithdrawalManagerCollector(managersToCheck, false) };
			}
			else
			{
				return new ManagerCollector[] { new AmendmentManagerCollector(managersToCheck, false) };
			}
		}

		ArrayList GetManagersToCheck(SingleMessageManager[] managersToExclude, GetAllSingleMessageManagersDelegate getAllManagers)
		{
			ArrayList result = new ArrayList();

			IEnumerable<SingleMessageManager> managers = getAllManagers();
			foreach (SingleMessageManager manager in managers)
			{
				if (((IList)managersToExclude).IndexOf(manager) == -1)
				{
					result.Add(manager);
				}
			}

			return result;
		}

		protected virtual bool CheckDeniedParty(BusinessObject master)
		{
			return Customs.Business.MessageManagerCreditCheckWithSecurityHelper.CheckDeniedParty(master as BaseJobDeclaration);
		}

		protected virtual bool CanAllMangersBeSavedExceptThosePassedIn(ISendsMessagesToCustoms sender, SingleMessageManager[] managersToExclude, bool detectingAmendment)
		{
			RequiredMessagesInformation info = GetRequiredMessagesInformationExceptForThosePassedIn(managersToExclude, detectingAmendment);
			bool result = !info.HasMessagesToSend && !info.HasMessageManagersWaitingForResponses;
			if (!result)
			{
				sender.WarnUserAboutSomething(Res.GetString("02a4c77b-f745-497c-ab5d-affcf5fa17c9", "Cannot send message(s) because there are other messages to be sent, or messages waiting for responses"), Res.GetString("35726669-b8ce-4178-ac00-f667c5e975c5", "Cannot send"));
			}
			return result;
		}

		public delegate IEnumerable<SingleMessageManager> GetAllSingleMessageManagersDelegate();

		protected virtual GetAllSingleMessageManagersDelegate GetAllSingleMessageManagersDelegateForAmendmentDetection
		{
			get { return delegate { return AllMessageManagers; }; }
		}

		internal RequiredMessagesInformation GetRequiredMessagesInformationExceptForThosePassedIn(SingleMessageManager[] managersToExclude, bool detectingAmendment)
		{
			return GetRequiredMessagesInformationExceptForThosePassedIn(managersToExclude, detectingAmendment, delegate
			{ return AllMessageManagers; });
		}

		protected virtual internal RequiredMessagesInformation GetRequiredMessagesInformationExceptForThosePassedIn(SingleMessageManager[] managersToExclude, bool detectingAmendment, GetAllSingleMessageManagersDelegate getAllManagers)
		{
			RequiredMessagesInformation result = new RequiredMessagesInformation((IMessageManageableBizObj)TopLevelBusinessObject);

			result.IsInProcessOfDetectingRequiredMessages = true;

			ArrayList managersToCheck = GetManagersToCheck(managersToExclude, getAllManagers);

			ManagerCollector[] collectors = GetCollectors((SingleMessageManager[])managersToCheck.ToArray(typeof(SingleMessageManager)));

			foreach (ManagerCollector collector in collectors)
			{
				SingleMessageManager[] applicableManagers = collector.ApplicableManagers;

				if (applicableManagers.Length > 0)
				{
					ManagerCollectorSingleMessageManagerBridge bridge = new ManagerCollectorSingleMessageManagerBridge(collector, applicableManagers);
					result.AddBridge(bridge);
				}
			}
			result.IsInProcessOfDetectingRequiredMessages = false;

			return result;
		}

		protected virtual bool CanSendOriginal(ISendsMessagesToCustoms sender, params SingleMessageManager[] managersToSend)
		{
			bool result = false;
			var allNotifications = new OriginalManagerCollector(managersToSend, true).Notifications;
			if (allNotifications.ContainsError())
			{
				DisplayErrorNotification(sender, allNotifications);
			}
			else if (TopLevelBusinessObject != null && (CheckTopLevelBusinessObjectsChildren ? TopLevelBusinessObject.HasMessageErrors : TopLevelBusinessObject.Notifications.GetMessageErrors().Any()) && !CanSendWithMessageErrors)
			{   // We don't use TopLevelBusinessObject.HasMessageErrors, because we don't want the children errors.
				allNotifications.AddError(Res.GetString("56850df7-a21f-496f-ba9e-b50ececc2576", "There are message errors on this job and you don't have a security right to send with message errors."));
				DisplayErrorNotification(sender, allNotifications);
			}
			else if (!allNotifications.ContainsWarning() || AskUserToContinue(sender, allNotifications))
			{
				result = true;
			}
			return result;
		}

		protected bool CanSendWithMessageErrors
		{
			get
			{
				return SendWithMessageErrorsSecurityCheckpoint.IsAllowed;
			}
		}

		protected virtual SecurityCheckpoint SendWithMessageErrorsSecurityCheckpoint
		{
			get
			{
				return Env.Security.CustomsDeclarationSendWithMessageErrors;
			}
		}

		protected virtual bool CheckTopLevelBusinessObjectsChildren
		{
			get { return true; }
		}

		protected virtual bool CanAmend(ISendsMessagesToCustoms sender, params SingleMessageManager[] managers)
		{
			return CanContinue(sender, new AmendmentManagerCollector(managers, true));
		}

		protected virtual bool CanWithdraw(ISendsMessagesToCustoms sender, params SingleMessageManager[] managers)
		{
			return CanContinue(sender, new WithdrawalManagerCollector(managers, true));
		}

		bool CanContinue(ISendsMessagesToCustoms sender, ManagerCollector collector)
		{
			bool result = false;

			MessageSendingNotificationCollection allNotifications = collector.Notifications;
			if (allNotifications.ContainsError())
			{
				DisplayErrorNotification(sender, allNotifications);
			}
			else if (!allNotifications.ContainsWarning() || AskUserToContinue(sender, allNotifications))
			{
				result = true;
			}

			return result;
		}

		internal void DisplayErrorNotification(ISendsMessagesToCustoms sender, MessageSendingNotificationCollection notifications)
		{
			ZString errorText = Res.GetString("c9b477ed-47ff-4617-bdc4-48a7ec45fb1a", "You may not continue due to one or more critical problems:") + "\r\n\r\n";
			errorText += notifications.NotificationsAsString();
			sender.NotifyUserOfAnInvalidOperation(errorText);
		}

		protected internal bool AskUserToContinue(ISendsMessagesToCustoms sender, MessageSendingNotificationCollection notifications)
		{
			ZString questionText = Res.GetString("5f18c8f9-87a6-4a49-9f97-cf024f44ec7d", "Do you wish to continue despite the following?") + "\r\n\r\n";
			questionText += notifications.NotificationsAsString(ShowNotificationsAfterSave ? 100 : 0);
			return sender.AskUserToContinueWithAction(questionText, Res.GetString("71e93200-57e0-4fb6-80a6-f40d34a713b1", "Continue with Action?"), TopLevelBusinessObject);
		}

		protected SingleMessageManager[] AllMessageManagers
		{
			get
			{
				if (allMessageManagers == null)
				{
					allMessageManagers = GetAllMessageManagers();
				}
				return allMessageManagers;
			}
		}
		SingleMessageManager[] allMessageManagers;

		public ZStringBuilder AfterSaveNotifications { get; set; }

		void InitialiseMessageCountOnly()
		{
			originalMessagesSent = 0;
			amendmentMessagesSent = 0;
			withdrawalMessagesSent = 0;
			AfterSaveNotifications = new ZStringBuilder();
		}

		protected
#if DEBUG
 virtual
#endif
 void Initialise()
		{
			InitialiseMessageCountOnly();
			allMessageManagers = null;
		}

		internal int originalMessagesSent;
		internal int amendmentMessagesSent;
		internal int withdrawalMessagesSent;

		protected abstract SingleMessageManager[] GetAllMessageManagers();

		AmendmentWithdrawalReason fAmendmentWithdrawalReason;
		#endregion

		#region IMessageManager Members

		bool IMessageManager.DeferredAmendmentTillAfterSaveSuccessful
		{
			get { return DeferredAmendmentTillAfterSaveSuccessfulCore; }
		}

		protected virtual bool DeferredAmendmentTillAfterSaveSuccessfulCore
		{
			get { return false; }
		}

		RequiredMessagesInformation IMessageManager.GetRequiredMessagesInformation()
		{
			return DetermineRequiredMessagesWithPendingChanges();
		}

		IDeferredAmendmentSavingOptions IMessageManager.GetDeferredAmendmentSavingOptions()
		{
			if (!(TopLevelBizObjToManage is IBackDoorSavingSupportableBizObj))
			{
				throw new InvalidOperationException("");
			}
			return null;
		}

		MessageSendingNotificationCollection IMessageManager.CheckBusinessObjectLevelValidationIfRequired()
		{
			return new MessageSendingNotificationCollection();
		}

		MessageGenerationResultCollection IMessageManager.SendAnyMessagesRequired(RequiredMessagesInformation information)
		{
			return SendAnyMessagesRequired(information);
		}

		public virtual string CannotSaveWhenWaitingForResponse
		{
			get { return Res.GetString("ab803871-709e-4f9c-a89d-430514d91265", "There are messages waiting for responses and the system has detected you have made changes that affect Customs entries. The changes cannot be saved. Please wait until the messages are responded."); }
		}

		ZString IMessageManager.GetCriticalErrorsForCanSaveExcludingMessagingLevelNotifications(RequiredMessagesInformation information)
		{
			ZStringBuilder result = new ZStringBuilder();

			if (TopLevelBusinessObject != null && TopLevelBusinessObject.HasErrors)
			{
				IEnumerable<INotification> collector = new CustomsNotificationCollector(TopLevelBusinessObject, true, false, CustomsNotificationCollector.PropertyDescriptionType.HumanReadableName).GetErrors();
				result.Append(collector.ToUniqueMessageListString());
			}

			if (ShouldWaitUntilResponded && information.HasMessageManagersWaitingForResponses)
			{
				result.Append(CannotSaveWhenWaitingForResponse);
			}

			return result.ToStringWithNewLineBetweenAppends();
		}

		void IMessageManager.ProcessWhenChangesAreSavedWithoutSending(IDeferredAmendmentSavingOptions saveOptions, RequiredMessagesInformation information)
		{
			ProcessWhenChangesAreSavedWithoutSendingCore(saveOptions, information);
		}

		protected virtual void ProcessWhenChangesAreSavedWithoutSendingCore(IDeferredAmendmentSavingOptions saveOptions, RequiredMessagesInformation information)
		{
		}

		#endregion

		public bool ShowNotificationsAfterSave
		{
			get { return ShowNotificationsAfterSaveCore; }
		}

		protected virtual bool ShowNotificationsAfterSaveCore
		{
			get { return false; }
		}

		protected virtual bool ShouldWaitUntilResponded
		{
			get { return true; }
		}

		public virtual bool ShouldSendMessagesInTestMode
		{
			get { return false; }
		}
	}
}
