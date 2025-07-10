using CargoWise.ComponentModel;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.MessageManagers;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.ZA.Business
{
	public class NotificationAccumulatorForAutomatedMessageSending : IMessageNotificationCollector, IUserNotification, INotifications
	{
		public NotificationAccumulatorForAutomatedMessageSending()
		{
			Notifications = new MessageSendingNotificationCollection();
		}

		public MessageSendingNotificationCollection Notifications { get; }

		public void Add(INotification notification)
		{
			var sendingNotificationMessage = new SendingNotificationMessage(notification.Message);
			Notifications.Add(sendingNotificationMessage);
		}

		public bool ShowConfirmation(string message, string caption, string confirmationPrompt, string confirmationString)
		{
			return true;
		}

		public bool ShowConfirmation(string message, string caption, bool warning = false)
		{
			return true;
		}

		public void ShowError(string message, string caption)
		{
		}

		public void ShowInformation(string message, string caption)
		{
		}

		public string ShowQuestion(string message, string caption, int answerLength, CodeDescriptionPairList answerList, string defaultAnswer = null)
		{
			return defaultAnswer;
		}

		public void ShowWarning(string message, string caption)
		{
		}
	}
}
