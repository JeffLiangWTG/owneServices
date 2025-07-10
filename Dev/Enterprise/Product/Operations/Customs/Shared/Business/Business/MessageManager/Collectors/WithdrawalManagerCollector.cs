using System.Collections;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.Business
{
	public class WithdrawalManagerCollector : ManagerCollector
	{
		public WithdrawalManagerCollector(SingleMessageManager[] managersToCheck, bool sendAll) : base(managersToCheck)
		{
			this.sendAll = sendAll;
		}

		readonly bool sendAll;

		protected override void DoActionCore()
		{
			foreach (SingleMessageManager manager in ApplicableManagers)
			{
				EDIMessage[] messages = manager.GenerateWithdrawalMessages(manager.BusinessObject);
				generatedMessages.AddRange(messages);
				manager.OnWithdrawalSent();
			}
		}

		protected override SingleMessageManager[] GetApplicableManagers()
		{
			ArrayList arrayResult = new ArrayList();
			foreach (SingleMessageManager manager in ManagersToCheck)
			{
				if (sendAll || manager.ShouldSendWithdrawalOnSave)
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
				MessageSendingNotificationCollection singleManagerNotifications = manager.GetNotificationsForSendingAWithdrawal();
				if (singleManagerNotifications.Count > 0)
				{
					result.AddWarning(manager.MessageFriendlyName);
					result.AddRange(singleManagerNotifications);
				}
			}
			return result;
		}

		public override bool IsWithdrawal
		{
			get { return true; }
		}
	}
}
