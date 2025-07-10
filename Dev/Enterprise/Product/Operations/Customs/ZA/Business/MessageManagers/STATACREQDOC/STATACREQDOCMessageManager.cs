using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.ZA.Business.MessageManagers
{
	public class STATACREQDOCMessageManager
	{
		#region Constructor

		public STATACREQDOCMessageManager(STATACREQDOCSendingObjectParent parent, IMessageNotificationCollector notification)
		{
			this.parent = Argument.NotNull(parent, "parent");
			this.notification = Argument.NotNull(notification, "notification");
		}

		readonly STATACREQDOCSendingObjectParent parent;

		#endregion

		readonly IMessageNotificationCollector notification;

		protected SingleMessageManager[] GetAllMessageManagers()
		{
			var result = new List<SingleMessageManager>();
			foreach (var sendingObject in parent.SendingObjectsCollection.Cast<STATACREQDOCSendingObject>().Where(x => x.ShouldSend))
			{
				result.Add(new REQDOCMessageManager(sendingObject, notification));
			}
			return result.ToArray();
		}

		#region Implementation

		public void SendMessages()
		{
			if (CanSendMessages())
			{
				notification.Notifications?.Clear();
				foreach (var messageManager in GetAllMessageManagers())
				{
					(messageManager as REQDOCMessageManager)?.Send();
				}
			}
			ShowMessageSendingResult();
		}

		bool CanSendMessages()
		{
			var result = true;
			var notifications = notification?.Notifications;
			if (notifications != null)
			{
				CollectNotificationsFromMessageManagers(notifications);

				if (notifications.ContainsError())
				{
					result = false;
				}
				if (notifications.ContainsWarning())
				{
					result = notification.ShowConfirmation(notifications.WarningNotificationsAsString(), Res.GetString("2D25643B-B0BC-47A8-A225-697A237ED71D", "Continue sending with warning?"));
				}

				notifications.Clear();
			}
			return result;
		}

		void CollectNotificationsFromMessageManagers(MessageSendingNotificationCollection notifications)
		{
			var allmanagers = GetAllMessageManagers();
			if (allmanagers.Any(x => (x as REQDOCMessageManager)?.IsTestMessage ?? false))
			{
				notifications.AddWarning(MessageSendingValidation.WarningWhenInTestModeText);
			}
		}

		void ShowMessageSendingResult()
		{
			var notifications = notification?.Notifications;
			if (notifications != null)
			{
				if (notifications.ContainsError())
				{
					notification.ShowError(notifications.ErrorNotificationsAsString(), Res.GetString("7E374F2E-6A64-4066-9DA2-778C7574F0F9", "Error in Message Sending"));
				}
				if (notifications.ContainsInformation())
				{
					notification.ShowInformation(notifications.InformationNotificationsAsString(), Res.GetString("F0655D94-E567-479A-B301-DC40F51D0E66", "Message Sending Result"));
				}
			}
		}

		#endregion
	}
}
