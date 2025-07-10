using System.Collections;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.Business
{
	public class AmendmentManagerCollector : ManagerCollector
	{
		public AmendmentManagerCollector(SingleMessageManager[] managersToCheck, bool amendAll) : base(managersToCheck)
		{
			this.amendAll = amendAll;
		}

		readonly bool amendAll;

		protected override void DoActionCore()
		{
			foreach (SingleMessageManager manager in ApplicableManagers)
			{
				EDIMessage[] messages = manager.GenerateAmendmentMessages(manager.BusinessObject);
				generatedMessages.AddRange(messages);
				manager.OnAmendmentSent();
			}
		}

		protected override SingleMessageManager[] GetApplicableManagers()
		{
			ArrayList result = new ArrayList();
			foreach (SingleMessageManager manager in ManagersToCheck)
			{
				if (amendAll || manager.RequiresAmendment())
				{
					result.Add(manager);
				}
			}
			return (SingleMessageManager[])result.ToArray(typeof(SingleMessageManager));
		}

		protected override MessageSendingNotificationCollection GetNotifications()
		{
			MessageSendingNotificationCollection result = new MessageSendingNotificationCollection();
			foreach (SingleMessageManager manager in ApplicableManagers)
			{
				MessageSendingNotificationCollection singleManagerNotifications = manager.GetNotificationsForSendingAReplacement();
				if (singleManagerNotifications.Count > 0)
				{
					result.AddWarning(manager.MessageFriendlyName);
					result.AddRange(singleManagerNotifications);
				}
			}
			return result;
		}

		public override bool IsAmendment
		{
			get { return true; }
		}
	}
}
