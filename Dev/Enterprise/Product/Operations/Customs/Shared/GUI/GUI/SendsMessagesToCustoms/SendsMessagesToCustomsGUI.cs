using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.WarehouseExtensions;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GUI
{
	public class SendsMessagesToCustomsGUI : ISendMessagesToCustomsExtraMembersAndDetermineRequiredMessagesAndSendThem
	{
		public static string SendErrorAlertMessageText
		{
			get { return Res.GetString("d5bb92d3-2e48-4175-b8a7-e169c553eec7", "There are some critical problems with sending this message. You may not send until you resolve the following:") + "\r\n\r\n"; }
		}

		#region DetermineRequiredMessagesAndSendThem

		/// <summary>
		/// If amendableBizObj support back-door saving, then it does required actions
		/// It notifies users errors and warnings that are collected from single message managers
		/// It runs validations and then, sends messages if users have not cancelled any action in the course.
		/// </summary>
		public
 virtual//for mocking
			ContinueWithSave DetermineRequiredMessagesAndSendThem(IMessageManager messageManager)
		{
			if (lastMessageManager != null)
			{
				lastMessageManager.TopLevelBusinessObject.Factory.Saved -= SendMessageOnSaved;
				lastMessageManager = null;
				lastDetectionResult = null;
				lastSavingOptions = null;
			}
			RequiredMessagesInformation detectionResult = messageManager.GetRequiredMessagesInformation();

			//if there are messages to send, then it needs more processing and decision making from users
			ContinueWithSave result = detectionResult.HasMessagesToSend ? ContinueWithSave.No : ContinueWithSave.Yes;

			if (detectionResult.HasMessagesToSend)
			{
				bool sendMessages = true;
				ZString criticalErrorsForCanSave = messageManager.GetCriticalErrorsForCanSaveExcludingMessagingLevelNotifications(detectionResult);
				if (!criticalErrorsForCanSave.IsEmpty)
				{
					sendMessages = false;
					DisplayErrorNotification(criticalErrorsForCanSave);
				}
				else if (detectionResult.SupportBackDoorForSavingWhenAmendmentDetected)
				{
					lastSavingOptions = messageManager.GetDeferredAmendmentSavingOptions();
					sendMessages = ShouldSendCustomsMessages(detectionResult, lastSavingOptions, messageManager);
					if (sendMessages)
					{
						if (!LodgementAllowed)
						{
							sendMessages = false;
							result = ContinueWithSave.No;
							ShowLodgementError();
						}
						else if (!SendWithErrorsAllowed
								&& messageManager.TopLevelBusinessObject.HasMessageErrors)
						{
							sendMessages = false;
							result = ContinueWithSave.No;
							ShowSendWithMessageErrors();
						}
					}

					if (lastSavingOptions.ShouldSaveWithoutSendingAmendment)
					{
						result = ContinueWithSave.Yes;
					}

					if (lastSavingOptions.ShouldTakeReasonForSavingWithoutSendingSeparately)
					{
						result = GetAmendmentWithdrawalReason(detectionResult.AmendmentWithdrawalReason);
					}
				}

				if (sendMessages)
				{
					if (detectionResult.AllNotifications.ContainsError())
					{
						DisplayErrorNotification(detectionResult.AllNotifications.NotificationsAsString());
					}
					else
					{
						//detectionResult.AllNotifications contain errors and message errors from SingleMessageManager.BusinessObject
						//This works for air cargo/sea cargo stuff and messageManager.CheckBusinessObjectLevelValidationIfRequired()
						//does nothing for those modules
						//But declaration has a cusentryheader and cusentryline which are not parent of invoice header or invoice lines
						//as registeredEditableChild and thus, AllNotifications collected from SingleMessageManager whose business object is
						//cusEntryHeader dont have all the notifications from invoices and invoice lines.
						//Firing validation and collecting notifications from declaration here usually works for one entry situation
						//as all notifications are related to the entry.
						MessageSendingNotificationCollection notifications = messageManager.CheckBusinessObjectLevelValidationIfRequired();

						if (AdviseWhichMessageToSendAndConfirmWithNotifications(detectionResult, notifications) == ContinueWithSave.Yes &&
							DoActionsBeforeSendingRequiredMessages(messageManager, detectionResult) == ContinueWithSave.Yes)
						{
							result = ContinueWithSave.Yes;
							if (messageManager.DeferredAmendmentTillAfterSaveSuccessful)
							{
								foreach (var managerColletor in detectionResult.ManagerCollectors)
								{
									var managers = managerColletor.ApplicableManagers; // Force the right mananager to be selected before the data is saved.
								}
								lastMessageManager = messageManager;
								lastDetectionResult = detectionResult;
								lastMessageManager.TopLevelBusinessObject.Factory.Saved += SendMessageOnSaved;
							}
							else
							{
								MessageGenerationResultCollection messagesGenerated = messageManager.SendAnyMessagesRequired(detectionResult);

								NotifyUsersOfMessagesSent(messagesGenerated);
							}
						}
					}
				}

				//when Factory.Save() is to be performed
				if (result == ContinueWithSave.Yes && lastSavingOptions != null && lastSavingOptions.ShouldSaveWithoutSendingAmendment)
				{
					//this usually adds a log etc
					messageManager.ProcessWhenChangesAreSavedWithoutSending(lastSavingOptions, detectionResult);
				}
			}

			return result;
		}
		IMessageManager lastMessageManager;
		RequiredMessagesInformation lastDetectionResult;
		IDeferredAmendmentSavingOptions lastSavingOptions;

		public virtual bool ShouldSendCustomsMessages(RequiredMessagesInformation detectionResult, IDeferredAmendmentSavingOptions savingOptions, IMessageManager messageManager)
		{
			GetSavingOptionsFromUsers(detectionResult, savingOptions);
			return savingOptions.ShouldSendMessages;
		}

		public virtual bool LodgementAllowed
		{
			get { return Environment.Env.Security.CustomsDeclarationLodgement.IsAllowed; }
		}

		public virtual void ShowLodgementError()
		{
			Environment.Env.Security.CustomsDeclarationLodgement.ShowError();
		}

		public virtual bool SendWithErrorsAllowed
		{
			get { return Environment.Env.Security.CustomsDeclarationSendWithMessageErrors.IsAllowed; }
		}

		public virtual void ShowSendWithMessageErrors()
		{
			Environment.Env.Security.CustomsDeclarationSendWithMessageErrors.ShowError();
		}

		void SendMessageOnSaved(BusinessObjectFactory factory, bool savedSuccessfully)
		{
			if (lastMessageManager != null)
			{
				lastMessageManager.TopLevelBusinessObject.Factory.Saved -= SendMessageOnSaved;
				if (savedSuccessfully)
				{
					var declaration = lastMessageManager.TopLevelBusinessObject as BaseJobDeclaration;
					if (declaration == null)
					{
						var shipment = lastMessageManager.TopLevelBusinessObject as ForwardingShipment;
						declaration = shipment == null ? null : shipment.DeclarationForDocuments as BaseJobDeclaration;
					}

					IDisposable suspender = declaration == null ? null : declaration.SuspendSettingHasChanges();
					try
					{
						SendAmendmentMessage(declaration);
					}
					finally
					{
						if (suspender != null)
						{
							suspender.Dispose();
						}
					}
				}
			}
			lastMessageManager = null;
			lastDetectionResult = null;
			lastSavingOptions = null;
		}

		void SendAmendmentMessage(BaseJobDeclaration declaration)
		{
			var needsToLogAmendment = true;
			try
			{
				Func<bool> sendMessage = () =>
				{
					var messagesGenerated = lastMessageManager.SendAnyMessagesRequired(lastDetectionResult);
					NotifyUsersOfMessagesSent(messagesGenerated);
					return true;
				};
				Action saveFactory = () =>
				{
					lastMessageManager.TopLevelBusinessObject.Factory.Save();
					needsToLogAmendment = false;
				};
				if (declaration != null)
				{
					declaration.SendMessageWithBondedWarehouseAutomation(sendMessage, MessageAction.Amendment, saveFactory: saveFactory, reportHasChanges: false);
				}
				else if (sendMessage())
				{
					try
					{
						saveFactory();
					}
					catch (ZSaveException ex)
					{
						ZExceptionReporting.HandleSaveException(ex);
					}
				}
			}
			finally
			{
				if (needsToLogAmendment && lastSavingOptions != null)
				{
					var savingOptions = lastSavingOptions as DeferredAmendmentSavingOptions;
					if (savingOptions != null)
					{
						try
						{
							savingOptions.SaveWithEntryChanges = true;
							lastMessageManager.ProcessWhenChangesAreSavedWithoutSending(savingOptions, lastDetectionResult);
							lastMessageManager.TopLevelBusinessObject.Factory.Save();
						}
						catch (ZSaveException ex)
						{
							ZExceptionReporting.HandleSaveException(ex);
						}
					}
				}
			}
		}

		void NotifyUsersOfMessagesSent(MessageGenerationResultCollection messagesGenerated)
		{
			if (messagesGenerated.HasMessagesGenerated)
			{
				NotifyUserOfASuccessfulSend(Res.GetString("160a9df9-fe7a-4b0c-92c8-0a4dbd0d884d", "The following messages have been generated") + "\r\n" + messagesGenerated.FullDescriptionsOfMessagesGenerated("\r\n"));
			}
			else
			{
				NotifyUserOfAnInvalidOperation(Res.GetString("161a2e2d-4252-445b-81aa-a44ff0d34301", "No messages have been generated"));
			}
		}

		public bool NotifyUsersOfNotifications(MessageSendingNotificationCollection notification, BusinessObject topLevelBusinessObject = null)
		{
			bool result = true;
			if (notification.ContainsError())
			{
				result = false;
				NotifyUserOfAnInvalidOperation(notification.NotificationsAsString());
			}
			else if (notification.ContainsWarning())
			{
				result = AskUserToContinueWithAction(notification.NotificationsAsString(), Res.GetString("34c2d5b6-0c37-4b06-99c1-38690e8bdbc1", "Continue with Send?"), topLevelBusinessObject);
			}
			return result;
		}

		ContinueWithSave AdviseWhichMessageToSendAndConfirmWithNotifications(RequiredMessagesInformation detectionResult, MessageSendingNotificationCollection notifications)
		{
			var result = ContinueWithSave.No;

			if (notifications.Count > 0)
			{
				detectionResult.AllNotifications.AddRange(notifications);
			}

			if ((detectionResult.SupportBackDoorForSavingWhenAmendmentDetected || AdviseWhichMessagesToSend(detectionResult) == ContinueWithSave.Yes) &&
				(!detectionResult.AllNotifications.ContainsWarning() || (detectionResult.AllNotifications.ContainsWarning() && AskUserToContinue(detectionResult.AllNotifications))))
			{
				result = ContinueWithSave.Yes;
			}

			return result;
		}

#if DEBUG
		protected virtual//for mocking
#endif
		ContinueWithSave AdviseWhichMessagesToSend(RequiredMessagesInformation detectionResult)
		{
			var message = new ZStringBuilder();
			message.Append(Res.GetString("15f8ea74-b600-482d-b387-a6964e9a49d4", "Continue With Save?  Saving will result in the following {0} being sent:", detectionResult.MessageTypesToSend) + "\r\n");
			message.Append(detectionResult.FullDescriptionsForMessagesToSend);
			return ContinueWithAction(message.ToString(), Res.GetString("1efb0616-b1a4-49fa-90ec-29f4166257e1", "Continue With Save?")) ? ContinueWithSave.Yes : ContinueWithSave.No;
		}

		/// <summary>
		/// AU shows CP Dec questions and takes amendment/withdrawal reasons from users before sending messages
		/// </summary>
		protected virtual ContinueWithSave DoActionsBeforeSendingRequiredMessages(IMessageManager manager, RequiredMessagesInformation detectionResult)
		{
			return ContinueWithSave.Yes;
		}

		#endregion

		#region Backdoor saving form

		void GetSavingOptionsFromUsers(RequiredMessagesInformation detectionResult, IDeferredAmendmentSavingOptions savingOptions)
		{
			ZFormModaliser.ShowDialogAndDispose(GetBackDoorForSavingForm(detectionResult, savingOptions));
		}

		protected virtual ZForm GetBackDoorForSavingForm(RequiredMessagesInformation detectionResult, IDeferredAmendmentSavingOptions savingOptions)
		{
			if (savingOptions is DeferredAmendmentSavingOptions)
			{
				return new BackdoorForSavingOnAmendmentForm((DeferredAmendmentSavingOptions)savingOptions);
			}
			else
			{
				throw new NotSupportedException("The base form works with DeferredAmendmentSavingOptions.");
			}
		}

		#endregion

		#region Amendment/Withdrawal Reason

		public
#if DEBUG
 virtual//for mocking
#endif
			ContinueWithSave GetAmendmentWithdrawalReason(AmendmentWithdrawalReason amendmentWithdrawalReason)
		{
			ZFormModaliser.ShowDialogAndDispose(new AmendmentReasonForm(amendmentWithdrawalReason));
			return amendmentWithdrawalReason.IsCancelled ? ContinueWithSave.No : ContinueWithSave.Yes;
		}

		#endregion

		#region ISendsMessagesToCustoms Members

		public bool SuppressUserInteraction { get; set; }

		void DisplayErrorNotification(string errorText)
		{
			NotifyUserOfAnInvalidOperation(Res.GetString("6e7347a0-5d1d-433c-8b34-0fe4b9467bf6", "You may not continue due to one or more critical problems:\r\n\r\n{0}", errorText));
		}

		bool AskUserToContinue(MessageSendingNotificationCollection notifications)
		{
			ZString questionText = Res.GetString("49a3c802-1d94-4906-bdb0-b7f8f275fcf0", "Do you wish to continue despite the following?") + "\r\n\r\n";
			questionText += notifications.NotificationsAsString();
			return ContinueWithAction(questionText, Res.GetString("c3811737-34ae-4164-aab2-5f92dd54726c", "Continue with Action?"));
		}

		public virtual void NotifyUserOfASuccessfulSend(string text)
		{
			NotifyUserOfASuccessfulOperation(text, Res.GetString("956ca2b2-2aba-45d4-81e4-4a5a59d0b60e", "Message(s) Queued For Sending"));
		}

		public virtual void NotifyUserOfASuccessfulOperation(string text, string caption)
		{
			if (SuppressUserInteraction)
			{
			}
			else
			{
				UserNotification.ShowInformation(text, caption);
			}
		}

		public bool ContinueWithAction(string message, string caption)
		{
			return UserNotification.Show(message, caption, MessageBoxButtons.YesNo, MessageBoxIcon.Warning, DialogResult.No) == DialogResult.Yes;
		}

		public bool AskUserToContinueWithAction(string message, string caption, BusinessObject topLevelBusinessObject = null)
		{
			var result = ContinueWithAction(message, caption);
			if (result && topLevelBusinessObject is EnterpriseBusinessObject businessObject)
			{
				var supervisorOverrides = GetSupervisorOverrides(businessObject, SupervisorOverridesContext.SendingMessages);
				result = SupervisorOverridesHelper.IsSupervisorApproved(supervisorOverrides, businessObject.Logs);
			}

			return result;
		}

		protected virtual SupervisorOverrides GetSupervisorOverrides(IBusiness businessEntity, string context)
		{
			return new SupervisorOverrides(businessEntity, context);
		}

		public bool YesNoQuery(string message, string caption, MessageStyle messageStyle = MessageStyle.Question)
		{
			return UserNotification.Show(message, caption, MessageBoxButtons.YesNo, ConvertMesageStyleToIcon(messageStyle), DialogResult.Yes) == DialogResult.Yes;
		}

		MessageBoxIcon ConvertMesageStyleToIcon(MessageStyle messageStyle)
		{
			switch (messageStyle)
			{
				case MessageStyle.Error:
					return MessageBoxIcon.Error;

				case MessageStyle.Information:
					return MessageBoxIcon.Information;

				case MessageStyle.Warning:
					return MessageBoxIcon.Warning;

				case MessageStyle.Question:
					return MessageBoxIcon.Question;

				default:
					return MessageBoxIcon.None;
			}
		}

		public YesNoCancel YesNoCancelQuery(string message, string caption, MessageStyle messageStyle = MessageStyle.Question)
		{
			var result = UserNotification.Show(message, caption, MessageBoxButtons.YesNoCancel, ConvertMesageStyleToIcon(messageStyle), DialogResult.Yes);

			switch (result)
			{
				case DialogResult.Yes:
					return YesNoCancel.Yes;
				case DialogResult.No:
					return YesNoCancel.No;

				default:
					return YesNoCancel.Cancel;
			}
		}

		public bool ShowUserConfirmation(string message, string captionButton, string confirmationPrompt, string confirmationString)
		{
			return UserNotification.ShowConfirmation(message, captionButton, confirmationPrompt, confirmationString, MessageBoxIcon.Warning) == DialogResult.OK;
		}

		public virtual void NotifyUserOfAnInvalidOperation(string text)
		{
			UserNotification.ShowError(text, Res.GetString("a698c560-216a-48fc-8b52-30f36749d1ca", "Unable to complete your request..."));
		}

		public void WarnUserAboutSomething(string message, string caption)
		{
			UserNotification.ShowWarning(message, caption);
		}

		public bool ContinueWithSend(StringCollection warnings)
		{
			StringBuilder messageBuilder = new StringBuilder();
			messageBuilder.Append(Res.GetString("ee96cd9a-a38a-4555-8400-89306ec2fd55", "There are some problems with sending this message.  Do you want to send despite the following problems?") + "\r\n\r\n");
			foreach (string warning in warnings)
			{
				messageBuilder.Append(warning + "\r\n");
			}
			return UserNotification.Show(messageBuilder.ToString(), Res.GetString("317d1f7d-a7db-4b5b-8a13-d001282c5659", "Continue with send?"), MessageBoxButtons.YesNo, MessageBoxIcon.Warning, DialogResult.Yes) == DialogResult.Yes;
		}

		public void MessageSendErrorAlert(StringCollection errors)
		{
			StringBuilder messageBuilder = new StringBuilder();
			messageBuilder.Append(SendErrorAlertMessageText);
			foreach (string error in errors)
			{
				messageBuilder.Append(error + "\r\n");
			}

			UserNotification.ShowError(messageBuilder.ToString());
		}

		public SingleMessageManager[] WhichMessagesShouldWeSend(SingleMessageManager[] allManagers)
		{
			if (allManagers.Length == 0)
			{
				NotifyUserOfAnInvalidOperation(Res.GetString("f2eaca59-d521-4e0a-a951-e601521ceb6f", "There is nothing available for sending"));
				return allManagers;
			}
			else
			{
				var result = new List<SingleMessageManager>();
				ZBlob notesBlob = null;

				var chooser = CreateMessageChooser(allManagers, Res.GetString("d10c434d-3e3c-41e8-9079-69ebe29430ba", "Which messages do you want sent?"));
				foreach (SingleMessageManager singleManager in GetManagers(chooser))
				{
					var overdueCargoReportException = singleManager.OverdueCargoReportException;
					if (overdueCargoReportException != null)
					{
						if (notesBlob == null)
						{
							if (ZFormModaliser.ShowDialogAndDispose(new ExceptionReasonDialog(overdueCargoReportException)) == DialogResult.OK)
							{
								notesBlob = overdueCargoReportException.P9_Notes;
							}
							else
							{
								overdueCargoReportException.P9_Notes = (ZBlob)overdueCargoReportException.P9_NotesInfo.OriginalValue;
								continue;
							}
						}
						else
						{
							overdueCargoReportException.P9_Notes = notesBlob;
						}

						using (overdueCargoReportException.GetValidationSuspender())
						{
							overdueCargoReportException.IsExceptionActioned = true;
							overdueCargoReportException.P9_TaskCannotBeDeleted = true;
						}
					}

					result.Add(singleManager);
				}

				return result.ToArray();
			}
		}

		public SingleMessageManager[] WhichMessagesShouldWeWithdraw(SingleMessageManager[] allManagers)
		{
			if (allManagers.Length == 0)
			{
				NotifyUserOfAnInvalidOperation(Res.GetString("5823de9b-2669-417d-8449-7aa1466ae6a4", "There is nothing available for withdrawing"));
				return allManagers;
			}
			else
			{
				MessageChooserNonPersistent chooser = CreateMessageChooser(allManagers, Res.GetString("ee4fa70c-b113-4acb-811f-23f91a030f2e", "Which messages do you want withdrawn?"));
				return GetManagers(chooser);
			}
		}

		public SingleMessageManager[] WhichMessagesShouldWeReset(SingleMessageManager[] allManagers)
		{
			if (allManagers.Length == 0)
			{
				NotifyUserOfAnInvalidOperation(Res.GetString("26a27c0a-bfa2-450f-be32-a46558e6fe48", "There is nothing available for resetting"));
				return allManagers;
			}
			else
			{
				MessageChooserNonPersistent chooser = CreateMessageChooser(allManagers, Res.GetString("dd48d317-66a4-47b4-ab0e-af0c4f6ed668", "Which messages do you want reset?"), Res.GetString("3937eca6-dfd5-4033-ac0e-28d4e7c62277", "Reset"));
				return GetManagers(chooser);
			}
		}

		protected virtual SingleMessageManager[] GetManagers(MessageChooserNonPersistent chooser)
		{
			using (MessagesChooserDialog dialog = CreateMessageChooseDialog(chooser))
			{
				DataBoundControl.Get(CreateControlToBind(dialog)).SetDataBinding(chooser, "");
				ShowMessagesToSendDialogWithoutDispose(chooser, dialog);
				return dialog.SendPressed ? chooser.SelectedManagers : Array.Empty<SingleMessageManager>();
			}
		}

		protected virtual MessagesChooserDialog CreateMessageChooseDialog(MessageChooserNonPersistent chooser)
		{
			return new MessagesChooserDialog(chooser);
		}

		protected virtual MessageChooserNonPersistent CreateMessageChooser(SingleMessageManager[] managers, ZString question)
		{
			return new MessageChooserNonPersistent(managers, question);
		}

		protected virtual MessageChooserNonPersistent CreateMessageChooser(SingleMessageManager[] managers, ZString question, ZString action)
		{
			return new MessageChooserNonPersistent(managers, question, action);
		}

		protected virtual Control CreateControlToBind(MessagesChooserDialog dialog)
		{
			return dialog.MessagesCheckedListBox;
		}

		protected virtual void ShowMessagesToSendDialogWithoutDispose(MessageChooserNonPersistent chooser, MessagesChooserDialog dialog)
		{
			ZFormModaliser.ShowDialogWithoutDispose(dialog);
		}

		#endregion

		#region CustomsUserNotification

		public ICustomsUserNotification UserNotification => userNotification ?? (userNotification = GetUserNotificationCore());
		ICustomsUserNotification userNotification;

		protected virtual ICustomsUserNotification GetUserNotificationCore() => new FlexibleUserNotification();

		#endregion
	}
}
