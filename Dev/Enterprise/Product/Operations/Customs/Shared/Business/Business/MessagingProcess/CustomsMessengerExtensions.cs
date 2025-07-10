using System.Collections.Generic;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.Business.MessagingProcess
{
	public static class CustomsMessengerExtensions
	{
		public static EDIMessage CreateMessage(this ICustomsMessenger messenger) => messenger.MessageGenerator.GenerateMessage();
		public static bool HasMessagePlaceholderGenerator(this ICustomsMessenger messenger) => messenger.MessageGenerator is ICustomsMessageWithPlaceholdersGenerator;
		public static void OnEDIMessageSaving(this ICustomsMessenger messenger, EDIMessage message)
		{
			if (messenger.MessageGenerator is ICustomsMessageWithPlaceholdersGenerator placeholderGen)
			{
				placeholderGen.UpdateMessagePlaceholders(message);
			}
		}

		public static IReadOnlyCollection<MessageSendingNotification> RunCommonPreSendValidation(this ICustomsMessenger messenger)
		{
			var notifications = new List<MessageSendingNotification>();

			if (messenger is ISupportPermitProcessing permitSupporter)
			{
				var permitProcessor = permitSupporter.PermitProcessor;
				permitProcessor.AddPermitRecordsAndLockMutexIfNeeded();
				foreach (var error in permitProcessor.ErrorList)
				{
					notifications.Add(new MessageSendingError(error));
				}
			}

			return notifications;
		}

		public static void ProcessCommonUpdates(this ICustomsMessenger messenger, EDIMessage message)
		{
			if (messenger is ISupportPermitProcessing permitSupporter)
			{
				permitSupporter.PermitProcessor.AddPermitTransactions(message, permitSupporter.GetPermitAppIdForMessage);
			}
		}

		public static string SignMessages(this ICustomsMessenger messenger, IReadOnlyCollection<EDIMessage> messages, ActionResult previousResult)
		{
			var result = string.Empty;

			if (messenger is ISupportMessageSigning signingSupporter)
			{
				result = signingSupporter.SignMessages(messages, previousResult);
			}

			return result;
		}

		public static void CleanUp(this ICustomsMessenger messenger)
		{
			if (messenger is ISupportPermitProcessing permitSupporter)
			{
				permitSupporter.PermitProcessor.UnlockPermitMutexes();
			}
		}
	}
}
