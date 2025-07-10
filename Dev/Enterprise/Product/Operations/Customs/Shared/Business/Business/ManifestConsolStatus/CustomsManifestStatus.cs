using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.MessageBuilders;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.MessageBuilders;
using Enterprise.Edifact;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business
{
	public abstract class CustomsManifestStatus : NonPersistentBusinessObject, IObsoleteValidation
	{
		#region Schema/Constants

		public abstract class Schema
		{
			public const string E2_MessageStatus = "E2_MessageStatus";
			public const string E2_CustomsEntryNumber = "E2_CustomsEntryNumber";
			public const string E2_CustomsEntryNumberHumanReadableName = "E2_CustomsEntryNumberHumanReadableName";
			public const string E2_LastResponseDate = "E2_LastResponseDate";
		}

		public enum Action { Declare, Withdraw }

		#endregion

		protected CustomsManifestStatus(IManifestProvider manifestProvider) : base(manifestProvider.Factory)
		{
			this.ManifestProviderCore = manifestProvider;
		}

		protected readonly IManifestProvider ManifestProviderCore;
		public IManifestProvider ManifestProvider
		{
			get { return ManifestProviderCore; }
		}

		#region MessageStatus

		public bool IsClear
		{
			get { return IsClearCore; }
		}

		public IEnumerable<ManifestStatus> GetMessageStatusHistory()
		{
			IList<EDIMessage> orderedMessages = this.OrderedMessages;
			IList<EDIMessage> transmittedMessages = this.TransmittedMessages;
			List<EDIMessage> nextReceivedMessages = new List<EDIMessage>();

			EDIMessage firstNonDiscardedMessage = null;
			foreach (EDIMessage message in orderedMessages)
			{
				if (message.EM_Status != EDIMessage.Status.Discarded)
				{
					firstNonDiscardedMessage = firstNonDiscardedMessage ?? message;
					if (transmittedMessages.Contains(message))
					{
						bool isLastTransmittedMessage = (message == firstNonDiscardedMessage);
						if (nextReceivedMessages.Count > 0 || isLastTransmittedMessage)
						{
							yield return nextReceivedMessages.Count == 0 ? ManifestStatus.AwaitingResponse : GetMessageStatus(nextReceivedMessages.ToArray());
						}
						nextReceivedMessages.Clear();
					}
					else
					{
						nextReceivedMessages.Add(message);
					}
				}
			}
			if (nextReceivedMessages.Count > 0)
			{
				yield return GetMessageStatus(nextReceivedMessages.ToArray());
			}
		}

		public ManifestStatus LastMessageStatus
		{
			get
			{
				IEnumerator<ManifestStatus> statusHistory = GetMessageStatusHistory().GetEnumerator();
				return statusHistory.MoveNext() ? statusHistory.Current : ManifestStatus.NotSent;
			}
		}

		#endregion

		#region New Bound Properties

		#region E2_CustomsEntryNumber

		public abstract ZString E2_CustomsEntryNumber { get; }

		public virtual void ValidateE2_CustomsEntryNumber()
		{
			E2_CustomsEntryNumberInfo.ClearAllNotifications();
		}

		public ZPropertyInfo E2_CustomsEntryNumberInfo
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get { return GetZPropertyInfo(Schema.E2_CustomsEntryNumber); }
		}

		#endregion

		#region E2_CustomsEntryNumberHumanReadableName

		public abstract ZString E2_CustomsEntryNumberHumanReadableName { get; }

		public virtual void ValidateE2_CustomsEntryNumberHumanReadableName()
		{
			E2_CustomsEntryNumberHumanReadableNameInfo.ClearAllNotifications();
		}

		public ZPropertyInfo E2_CustomsEntryNumberHumanReadableNameInfo
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get { return GetZPropertyInfo(Schema.E2_CustomsEntryNumberHumanReadableName); }
		}

		#endregion

		#region E2_MessageStatus

		public ZString E2_MessageStatus
		{
			get { return LastMessageStatus.AsString; }
		}

		public ZPropertyInfo E2_MessageStatusInfo
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get { return GetZPropertyInfo(Schema.E2_MessageStatus); }
		}

		#endregion

		#region E2_LastResponseDate

		public virtual ZDateTime E2_LastResponseDate
		{
			get
			{
				ZDateTime result = ZDateTime.Invalid;
				EDIMessage[] receivedMessages = this.ReceivedMessages;
				if (receivedMessages.Length > 0)
				{
					foreach (EDIMessage message in receivedMessages)
					{
						ZDateTime messageSentTime = GetMessageSentTime(message);
						if (!messageSentTime.IsEmpty)
						{
							result = messageSentTime;
						}
					}
				}
				return result;
			}
		}

		public virtual void ValidateE2_LastResponseDate()
		{
			E2_LastResponseDateInfo.ClearAllNotifications();
			TypeValidation.CheckValidZDateTimeAndRange(E2_LastResponseDateInfo);
		}

		public ZPropertyInfo E2_LastResponseDateInfo
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get { return GetZPropertyInfo(Schema.E2_LastResponseDate); }
		}

		#endregion

		#endregion

		#region Messages

		protected virtual EDIMessageCollection AllMessages
		{
			get { return ManifestProvider.Messages; }
		}

		public EDIMessageFlattenedCollection MessagesIncludingInterchangeRejections
		{
			get { return fMessagesIncludingInterchangeRejections ?? (fMessagesIncludingInterchangeRejections = AllMessages.MessagesIncludingInterchangeRejections(MessageApplicationCode)); }
		}
		EDIMessageFlattenedCollection fMessagesIncludingInterchangeRejections;

		public EDIMessage[] OrderedMessages
		{
			get { return AllMessages.GetMatchingMessages(MessageApplicationCode, MessageTypes, ZString.Empty); }
		}

		public EDIMessage[] TransmittedMessages
		{
			get
			{
				ZQuery transmittedMessagesFilter = new ZQuery();
				transmittedMessagesFilter.AddToFilter(BaseMessageFilter);
				transmittedMessagesFilter.AddToFilter(JoinCondition.And, EDIMessageSchema.EM_ReceiveTransmit, SQLComparisonOperator.Equal, EDIMessage.Direction.Transmit);
				return EDIMessageComparer.GetSortedMessages(AllMessages.Find(new Func<EDIMessage, bool>(x => x.MatchesFilter(transmittedMessagesFilter))), System.ComponentModel.ListSortDirection.Descending);
			}
		}

		public EDIMessage[] ReceivedMessages
		{
			get
			{
				ZQuery receivedMessagesFilter = new ZQuery();
				receivedMessagesFilter.AddToFilter(BaseMessageFilter);

				receivedMessagesFilter.AddToFilter(JoinCondition.And, EDIMessageSchema.EM_ReceiveTransmit, SQLComparisonOperator.Equal, EDIMessage.Direction.Receive);
				return EDIMessageComparer.GetSortedMessages(AllMessages.Find(new Func<EDIMessage, bool>(x => x.MatchesFilter(receivedMessagesFilter))), System.ComponentModel.ListSortDirection.Descending);
			}
		}

		#endregion

		#region DeclareManifest / WithdrawManifest / ResetToOriginal

		public void DeclareManifest(ISendsMessagesToCustoms sender)
		{
			string errorsForSendingManifests = ValidateEnvironmentForSendingManifests(sender);
			if (!string.IsNullOrEmpty(errorsForSendingManifests))
			{
				sender.NotifyUserOfAnInvalidOperation(errorsForSendingManifests);
			}
			else if (ManifestProvider.HasChanges)
			{
				sender.NotifyUserOfAnInvalidOperation(Res.GetString("0d8f580f-6757-499f-ad3b-ad654aa4b06f", "You must save the current record before generating a message"));
			}
			else if (IsWaitingForResponse)
			{
				sender.NotifyUserOfAnInvalidOperation(Res.GetString("95381a50-ee73-40ee-aacf-e22de4d4657e", "There is a message pending. Please wait for the Customs response."));
			}
			else if (ContinueWithAction(sender, Action.Declare))
			{
				IManifestMessageBuilder builder = NewCreateOrReplaceMessageBuilder();

				if (builder.ErrorCount == 0)
				{
					AttemptToSendMessageAndHandleSaveException(new IManifestMessageBuilder[] { builder }, sender, Res.GetString("db147b8e-e4fc-4b1d-bb7c-d6279b9baac9", "Manifest Sent"));
				}
				else
				{
					sender.NotifyUserOfAnInvalidOperation((Res.GetString("b71da669-87c5-4307-ae5c-a751e8c2c0ac", "Unable to send {0} due to the following errors:", builder.ManifestMessageTypeCode)) + "\r\n\r\n" + builder.Errors);
				}
			}
		}

		public void WithdrawManifest(ISendsMessagesToCustoms sender)
		{
			try
			{
				string errorsForSendingManifests = ValidateEnvironmentForSendingManifests(sender);
				if (!string.IsNullOrEmpty(errorsForSendingManifests))
				{
					throw new ManifestException(errorsForSendingManifests);
				}
				if (ManifestProvider.HasChanges)
				{
					throw new ManifestException("Please save the changes first. Otherwise messages cannot be sent.");
				}
				if (IsStatusNotSent)
				{
					throw new ManifestException("There are no messages to withdraw.");
				}
				if (IsWaitingForResponse)
				{
					throw new ManifestException("There is a message pending. Please wait for the Customs response.");
				}

				if (ContinueWithAction(sender, Action.Withdraw))
				{
					IManifestMessageBuilder[] builders = NewMessageBuilders(MessageSubTypes.Withdraw);

					int totalErrors = 0;
					string errorText = "";
					foreach (IManifestMessageBuilder builder in builders)
					{
						if (builder.ErrorCount > 0)
						{
							totalErrors += builder.ErrorCount;
							errorText += builder.Errors;
						}
					}
					if (totalErrors == 0)
					{
						AttemptToSendMessageAndHandleSaveException(builders, sender, Res.GetString("00e87392-6491-4a46-8147-314c4e6b0a4a", "Manifest Withdrawn"));
					}
					else
					{
						sender.NotifyUserOfAnInvalidOperation((Res.GetString("e0815e90-5bec-43e6-9085-51b3d74c3ea3", "Unable to withdraw due to the following errors:")) + "\r\n" + errorText);
					}
				}
			}
			catch (ManifestException ex)
			{
				sender.NotifyUserOfAnInvalidOperation(ex.Message);
			}
		}

		protected void AttemptToSendMessageAndHandleSaveException(IManifestMessageBuilder[] builders, ISendsMessagesToCustoms sender, string messageToNotifyUsersOnSuccessfulSend)
		{
			try
			{
				foreach (IManifestMessageBuilder builder in builders)
				{
					builder.PopulateMessages();
				}
				ManifestProvider.Factory.Save();
				sender.NotifyUserOfASuccessfulSend(messageToNotifyUsersOnSuccessfulSend);
				OnMessageSent(sender);
				AllMessages.Load();
			}
			catch (ZSaveException ex)
			{
				HandleSaveException(ex);
			}
		}

		protected virtual void HandleSaveException(ZSaveException ex)
		{
			ZExceptionReporting.HandleSaveException(ex);
		}

		public void ResetToOriginal(ISendsMessagesToCustoms sender)
		{
			if (sender.ContinueWithAction(Res.GetString("891d68f7-a4ff-4a19-a220-980e947eca72", "Are you sure you want to reset to original?  This may lead to duplicate entries existing inside Customs."), Res.GetString("9c74a9f7-ca50-4d1c-81b7-788bb31fefb7", "Reset to Original?")))
			{
				DoResetToOriginal();
				E2_CustomsEntryNumberInfo.RefreshBinding();
				E2_MessageStatusInfo.RefreshBinding();
				E2_LastResponseDateInfo.RefreshBinding();
			}
		}

		[Serializable]
		protected class ManifestException : Exception
		{
			public ManifestException(string message)
				: base(message)
			{
			}

#if NETFRAMEWORK
			protected ManifestException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
				: base(info, context)
			{
			}
#endif
		}

		#endregion

		#region Implementation

		protected virtual string ValidateEnvironmentForSendingManifests(ISendsMessagesToCustoms sender)
		{
			return "";
		}

		protected virtual bool ContinueWithAction(ISendsMessagesToCustoms sender, Action action)
		{
			return true;
		}

		protected virtual void OnMessageSent(ISendsMessagesToCustoms sender)
		{
			E2_MessageStatusInfo.RefreshBinding();
		}

		public abstract IManifestMessageBuilder NewCreateOrReplaceMessageBuilder();
		protected abstract IManifestMessageBuilder[] NewMessageBuilders(MessageSubTypes messageSubType);

		protected internal bool CanWeSendAnOriginal
		{
			get { return NoEntryNumber && LastMessageStatus != ManifestStatus.AwaitingResponse; }
		}

		protected virtual bool NoEntryNumber
		{
			get { return E2_CustomsEntryNumber.IsEmpty; }
		}

		protected ZQuery BaseMessageFilter
		{
			get
			{
				if (fBaseMessageFilter == null)
				{
					fBaseMessageFilter = new ZQuery();
					fBaseMessageFilter.AddToFilter(JoinCondition.And, EDIMessageSchema.EM_ApplicationCode, SQLComparisonOperator.Equal, MessageApplicationCode);

					fBaseMessageFilter.AddToFilter(MessageTypeFilter);
					fBaseMessageFilter.AddToFilter(MessageStatusFilter);
				}
				return fBaseMessageFilter;
			}
		}
		protected ZQuery fBaseMessageFilter;

		protected ZQuery MessageStatusFilter
		{
			get
			{
				if (fMessageStatusFilter == null)
				{
					fMessageStatusFilter = new ZQuery();
					fMessageStatusFilter.AddToFilter(JoinCondition.And, EDIMessageSchema.EM_Status, SQLComparisonOperator.NotEqual, EDIMessage.Status.Rejected);
					fMessageStatusFilter.AddToFilter(JoinCondition.And, EDIMessageSchema.EM_Status, SQLComparisonOperator.NotEqual, EDIMessage.Status.Discarded);
				}
				return fMessageStatusFilter;
			}
		}
		protected ZQuery fMessageStatusFilter;

		protected ZQuery MessageTypeFilter
		{
			get
			{
				if (fMessageTypeFilter == null)
				{
					fMessageTypeFilter = new ZQuery();
					fMessageTypeFilter.DefaultJoinCondition = JoinCondition.Or;
					foreach (ZString messageType in MessageTypes)
					{
						fMessageTypeFilter.AddToFilter(EDIMessageSchema.EM_MessageType, messageType);
					}
				}
				return fMessageTypeFilter;
			}
		}
		protected ZQuery fMessageTypeFilter;

		protected virtual ManifestStatus GetMessageStatus(EDIMessage lastReceivedMessage)
		{
			throw new NotSupportedException("You must override 1 (and 1 only) of the GetMessageStatus overrides");
		}

		protected virtual ManifestStatus GetMessageStatus(EDIMessage[] lastReceivedMessages)
		{
			return GetMessageStatus(lastReceivedMessages[0]);
		}

		protected virtual ZString[] MessageTypes
		{
			get { return Array.Empty<ZString>(); }
		}

		protected abstract string MessageApplicationCode { get; }

		protected virtual ZDateTime GetMessageSentTime(EDIMessage message)
		{
			return message.EM_SystemCreateTimeUtc;
		}

		protected static string[] ConvertDBMessageToUNOA(string uNOBMessageText)
		{
			return UNOACharacterSet.FromUNOB(uNOBMessageText)
				.TrimEnd('\'').Replace("\n", "").Replace("\r", "").Split('\'');
		}

		protected virtual void DoResetToOriginal()
		{
			throw new ApplicationException("Not Supported. Override this method in concrete Status.");
		}

		protected virtual bool IsWaitingForResponse
		{
			get { return LastMessageStatus == ManifestStatus.AwaitingResponse || LastMessageStatus == ManifestStatus.PartiallyCleared; }
		}

		protected virtual bool IsStatusNotSent
		{
			get { return LastMessageStatus == ManifestStatus.NotSent; }
		}

		protected virtual bool IsClearCore
		{
			get { return LastMessageStatus == ManifestStatus.Cleared; }
		}
		#endregion
	}
}
