using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.Business
{
	public struct MessageGenerationResult
	{
		public MessageGenerationResult(string messageType, int messageCount, IEnumerable<EDIMessage> messages)
		{
			this.MessageType = messageType;
			this.MessageCount = messageCount;
			this.Messages = messages;
		}

		public readonly string MessageType;
		public readonly IEnumerable<EDIMessage> Messages;
		public readonly int MessageCount;
	}

	public class MessageGenerationResultCollection
	{
		public MessageGenerationResultCollection()
		{
			this.messagesGenerated = new List<MessageGenerationResult>();
		}

		readonly List<MessageGenerationResult> messagesGenerated;

		public bool HasMessagesGenerated
		{
			get { return messagesGenerated.Count > 0; }
		}

		public MessageGenerationResult this[int index]
		{
			get { return messagesGenerated[index]; }
		}

		public void Add(string messageType, int messageCount, IEnumerable<EDIMessage> messages)
		{
			if (messageCount > 0)
			{
				messagesGenerated.Add(new MessageGenerationResult(messageType, messageCount, messages));
			}
		}

		public void DeleteAllMessagesGenerated()
		{
			foreach (MessageGenerationResult result in messagesGenerated)
			{
				foreach (EDIMessage message in result.Messages)
				{
					message.Delete();
				}
			}
			messagesGenerated.Clear();
		}

		public string FullDescriptionsOfMessagesGenerated(string delimiter)
		{
			ZStringBuilder result = new ZStringBuilder();
			foreach (MessageGenerationResult generated in messagesGenerated)
			{
				result.Append(string.Format((NoResString)"{0} {1}(s)", generated.MessageCount, generated.MessageType));
			}
			return result.ToStringWithDelimiterBetweenAppends(delimiter);
		}
	}

	public interface IMessageManager
	{
		/// <summary>
		/// It calculates which messages need be sent and returns the result.
		/// </summary>
		RequiredMessagesInformation GetRequiredMessagesInformation();

		/// <summary>
		/// Users are presented with several options ie. Send messages, Save without sending etc 
		/// </summary>
		IDeferredAmendmentSavingOptions GetDeferredAmendmentSavingOptions();

		MessageSendingNotificationCollection CheckBusinessObjectLevelValidationIfRequired();

		MessageGenerationResultCollection SendAnyMessagesRequired(RequiredMessagesInformation information);

		ZString GetCriticalErrorsForCanSaveExcludingMessagingLevelNotifications(RequiredMessagesInformation information);

		void ProcessWhenChangesAreSavedWithoutSending(IDeferredAmendmentSavingOptions saveOptions, RequiredMessagesInformation information);

		BusinessObject TopLevelBusinessObject { get; }
		bool DeferredAmendmentTillAfterSaveSuccessful { get; }
	}

	public interface IDeferredAmendmentSavingOptions
	{
		ZBool ShouldSendMessages { get; }

		ZBool ShouldTakeReasonForSavingWithoutSendingSeparately { get; }

		ZBool ShouldSaveWithoutSendingAmendment { get; }

		ZBool IsCancelled { get; set; }

		ZBool SignificantAmendmentsHaveBeenMade { get; }

#if DEBUG

		void SetSaveWithoutEntryChangesValueForTestingTo(ZBool value);
		void SetSaveWithEntryChangesValueForTestingTo(ZBool value);
		void SetSendAmendmentValueForTestingTo(ZBool value);

#endif
	}
}
