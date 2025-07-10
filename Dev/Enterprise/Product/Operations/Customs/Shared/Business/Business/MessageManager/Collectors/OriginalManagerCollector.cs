using System.Collections;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.Business
{
	public class OriginalManagerCollector : ManagerCollector
	{
		public OriginalManagerCollector(SingleMessageManager[] managersToCheck, bool sendAll) : base(managersToCheck)
		{
			this.sendAll = sendAll;
		}

		#region Implementation

		protected override void DoActionCore()
		{
			foreach (SingleMessageManager manager in ApplicableManagers)
			{
				if (!manager.PreventSend)
				{
					EDIMessage[] messages = manager.GenerateOriginalMessages(manager.BusinessObject);
					if (messages.Length > 0)
					{
						generatedMessages.AddRange(messages);
						manager.OnOriginalSent();
					}
				}
			}
		}

		protected override SingleMessageManager[] GetApplicableManagers()
		{
			ArrayList arrayResult = new ArrayList();
			foreach (SingleMessageManager manager in ManagersToCheck)
			{
				if (sendAll || manager.ShouldSendOriginalOnSave)
				{
					arrayResult.Add(manager);
				}
			}
			return (SingleMessageManager[])arrayResult.ToArray(typeof(SingleMessageManager));
		}

		protected override MessageSendingNotificationCollection GetNotifications()
		{
			MessageSendingNotificationCollection result = new MessageSendingNotificationCollection();
			foreach (SingleMessageManager manager in ApplicableManagers)
			{
				MessageSendingNotificationCollection singleManagerNotifications = manager.GetNotificationsForSendingAnOriginal();
				if (singleManagerNotifications.Count > 0)
				{
					result.AddWarning(manager.MessageFriendlyName);
					result.AddRange(singleManagerNotifications);
				}
			}
			return result;
		}

		public override bool IsOriginal
		{
			get { return true; }
		}

		readonly bool sendAll;
		#endregion
	}
}
