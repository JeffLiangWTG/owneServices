using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Threading;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Business.MessageBuilders;
using Enterprise.Messaging.MessageBuilders;

namespace Enterprise.Customs.Business
{
	/// <summary>
	/// This class is the base for Export AU Messages. ??? It is used in several countries, not just AU.
	/// </summary>
	public abstract class MessageManager
	{
		protected MessageManager(bool detectErrorsDuringMessageCreation)
		{
			this.detectErrorsDuringMessageCreation = detectErrorsDuringMessageCreation;
		}
		readonly bool detectErrorsDuringMessageCreation;

		#region Delegates

		public delegate IMessageBuilder MessageBuilderDelegate(BusinessObject master);

		public delegate bool SendAmendmentIfNeededDelegate(ISendsMessagesToCustoms sender);

		#endregion

		public bool SendAnyAmendmentsNeeded(ISendsMessagesToCustoms sender)
		{
			foreach (SendAmendmentIfNeededDelegate amendmentSender in SendAmendmentIfNeededDelegates)
			{
				if (!amendmentSender(sender))
				{
					return false;
				}
			}
			return true;
		}

		#region Implementation

		protected abstract SendAmendmentIfNeededDelegate[] SendAmendmentIfNeededDelegates
		{
			get;
		}

		protected abstract BusinessObject Master
		{
			get;
		}

#if DEBUG

		protected internal bool SendMessage(ISendsMessagesToCustoms sender, StringCollection errors, StringCollection warnings, MessageBuilderDelegate[] builderDelegates, string messageName)
		{
			return SendMessage(sender, errors, warnings, builderDelegates, messageName, CancellationToken.None);
		}

#endif

		protected virtual internal bool SendMessage(ISendsMessagesToCustoms sender, StringCollection errors, StringCollection warnings, MessageBuilderDelegate[] builderDelegates, string messageName, CancellationToken token)
		{
			var success = false;
			MessageSendingAction messageSendingAction = CheckErrorsAndWarningsBeforeSendingMessages(sender, errors, warnings);
			if (messageSendingAction == MessageSendingAction.ShouldSend)
			{
				if (detectErrorsDuringMessageCreation)
				{
					var messageBuilderResults = new List<IMessageBuilderResult>();
					success = true;

					foreach (MessageBuilderDelegate builderDelegate in builderDelegates)
					{
						token.ThrowIfCancellationRequested();
						IMessageBuilder builder = builderDelegate(Master);
						var messageBuilderResult = builder.PopulateMessages();
						if (messageBuilderResult is null)
						{
							success = false;
							break;
						}

						messageBuilderResults.Add(messageBuilderResult);
						if (!messageBuilderResult.IsSuccess)
						{
							success = false;
						}
					}

					foreach (IMessageBuilderResult messageBuilderResult in messageBuilderResults)
					{
						token.ThrowIfCancellationRequested();
						foreach (IBuilderResult builderResult in messageBuilderResult.GetBuilderResults())
						{
							token.ThrowIfCancellationRequested();
							if (success)
							{
								builderResult.AfterFullSuccess();
							}
							else
							{
								errors.AddRange(builderResult.Errors);
								builderResult.Message.Delete();
							}
						}
					}

					if (success)
					{
						SaveFactoryAfterSendingMessages(sender, token);
					}
					else
					{
						sender.MessageSendErrorAlert(errors);
					}
				}
				else
				{
					foreach (MessageBuilderDelegate builderDelegate in builderDelegates)
					{
						token.ThrowIfCancellationRequested();
						IMessageBuilder builder = builderDelegate(Master);
						builder.PopulateMessages();
					}
					SaveFactoryAfterSendingMessages(sender, token);
				}
			}
			return success;
		}

		protected enum MessageSendingAction { ShouldSend, DoNotSend }

		protected MessageSendingAction CheckErrorsAndWarningsBeforeSendingMessages(ISendsMessagesToCustoms sender, StringCollection errors, StringCollection warnings)
		{
			MessageSendingAction result = MessageSendingAction.DoNotSend;

			Master.LoadChildEditableObjects();
			Master.RunPreSaveValidation();
			if (errors.Count > 0)
			{
				sender.MessageSendErrorAlert(errors);
			}
			else
			{
				if (ShouldSendMessagesInTestMode)
				{
					warnings.Add(MessageSendingValidation.WarningWhenInTestModeText);
				}
				if (warnings.Count == 0 || sender.ContinueWithSend(warnings))
				{
					result = MessageSendingAction.ShouldSend;
				}
			}

			if (result == MessageSendingAction.ShouldSend && !CheckDeniedParty(Master))
			{
				result = MessageSendingAction.DoNotSend;
			}

			return result;
		}

		ZString[] GetMessageStrings(IMessageBuilderResult messageBuilderResult)
		{
			List<ZString> result = new List<ZString>();
			foreach (IBuilderResult builderResult in messageBuilderResult.GetBuilderResults())
			{
				EDIMessage message = builderResult.Message;
				result.Add(message.EM_MessageText);
				message.Delete();
			}
			return result.ToArray();
		}

		protected internal bool DoChangeResultInADifferentMessage(MessageBuilderDelegate builderDelegate)
		{
			bool result = true;
			try
			{
				BusinessObjectFactory newFactory = new BusinessObjectFactory();
				BusinessObject newFactoryBusinessObject = newFactory.Load(Master.GetType(), Master.PK);
				if (newFactoryBusinessObject != null)
				{
					IMessageBuilder databaseBuilder = builderDelegate(newFactoryBusinessObject);
					IMessageBuilderResult inDatabaseMessageResults = databaseBuilder.PopulateMessages();
					ZString[] inDatabaseMessageStrings = GetMessageStrings(inDatabaseMessageResults);

					IMessageBuilder inMemoryBuilder = builderDelegate(Master);
					IMessageBuilderResult inMemoryMessageResults = inMemoryBuilder.PopulateMessages();
					ZString[] inMemoryMessageStrings = GetMessageStrings(inMemoryMessageResults);

					if (inDatabaseMessageStrings.Length == inMemoryMessageStrings.Length)
					{
						bool allMessagesEqual = true;
						for (int i = 0; i < inMemoryMessageStrings.Length; i++)
						{
							if (inMemoryMessageStrings[i] != inDatabaseMessageStrings[i])
							{
								allMessagesEqual = false;
								break;
							}
						}
						if (allMessagesEqual)
						{
							result = false;
						}
					}
				}
			}
			catch (Exception e) when (!e.IsCriticalException())
			{
				ErrorReporter.ReportOnce("An exception occured whilst trying to compare manifest messages.  Returning that there are changes.", e);
			}
			return result;
		}

		protected internal bool SendAnAmendmentIfNeeded(ISendsMessagesToCustoms sender, bool isMessageDeclared, bool isWaitingForResponse, MessageBuilderDelegate builderDelegate, string messageName, StringCollection warnings, StringCollection errors)
		{
			bool result = true;
			if (isMessageDeclared || isWaitingForResponse)
			{
				if (DoChangeResultInADifferentMessage(builderDelegate))
				{
					if (isWaitingForResponse)
					{
						sender.NotifyUserOfAnInvalidOperation(Res.GetString("22dc73b9-5b47-413c-b4b3-e367e65dbc1b", "Changes you have made affect the {0} already declared to Customs.  You may not save because you are still waiting for a response from Customs.", messageName));
						result = false;
					}
					else if (sender.ContinueWithAction(Res.GetString("346a7f4b-69ca-458a-a908-2365905e9e69", "Changes you have made affect the {0} already declared to Customs.  If you continue with the save, an amendment message will be sent.  Are you sure you want to save?", messageName), Res.GetString("e5c2bb28-2779-43bb-9e48-1c4616bd3e6e", "Continue with save?")))
					{
						if (errors.Count > 0)
						{
							sender.MessageSendErrorAlert(errors);
							result = false;
						}
						else
						{
							if (warnings.Count == 0 || sender.ContinueWithSend(warnings))
							{
								IMessageBuilder builder = builderDelegate(Master);
								builder.PopulateMessages();
								sender.NotifyUserOfASuccessfulSend(Res.GetString("c594f91e-8c7b-4cdd-a4cf-ea5815c9d510", "{0} amendment message sent", messageName));
								OnMessageSent(sender);
							}
							else
							{
								result = false;
							}
						}
					}
					else
					{
						result = false;
					}
				}
			}
			return result;
		}

		protected virtual bool CheckDeniedParty(BusinessObject master)
		{
			return Customs.Business.MessageManagerCreditCheckWithSecurityHelper.CheckDeniedParty(master as BaseJobDeclaration);
		}

		protected virtual void SaveFactoryAfterSendingMessages(ISendsMessagesToCustoms sender, CancellationToken token)
		{
			if (ShouldSuspendFactorySave)
			{
				return;
			}

			try
			{
				Master.Factory.Save();
				sender.NotifyUserOfASuccessfulSend(MessageSentNotificationText);
				OnMessageSent(sender);
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				ZExceptionReporting.HandleSaveException(ex);
			}
		}

		protected virtual string MessageSentNotificationText
		{
			get { return Res.GetString("15df7ed7-6004-4141-aa89-639590478f2b", "Message Sent."); }
		}

		protected virtual void OnMessageSent(ISendsMessagesToCustoms sender)
		{
		}

		protected virtual bool ShouldSendMessagesInTestMode
		{
			get { return false; }
		}

		internal bool ShouldSuspendFactorySave { get; set; }

		#endregion
	}
}
