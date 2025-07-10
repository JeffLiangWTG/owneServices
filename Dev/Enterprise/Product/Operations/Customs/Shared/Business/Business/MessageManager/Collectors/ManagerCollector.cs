using System.Collections;
using System.Collections.Generic;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.Business
{
	public abstract class ManagerCollector
	{
		protected ManagerCollector(IEnumerable<SingleMessageManager> managersToCheck)
		{
			this.ManagersToCheck = managersToCheck;
			generatedMessages = new ArrayList();
		}

		public readonly IEnumerable<SingleMessageManager> ManagersToCheck;
		public EDIMessage[] GeneratedMessages
		{
			get { return (EDIMessage[])generatedMessages.ToArray(typeof(EDIMessage)); }
		}

		public void DoAction()
		{
			generatedMessages.Clear();
			DoActionCore();
		}

		public MessageSendingNotificationCollection Notifications
		{
			get { return GetNotifications(); }
		}

		public SingleMessageManager[] ApplicableManagers
		{
			get { return applicableManagers ?? (applicableManagers = GetApplicableManagers()); }
		}
		SingleMessageManager[] applicableManagers;

		#region Implementation

		public virtual bool IsOriginal { get { return false; } }
		public virtual bool IsAmendment { get { return false; } }
		public virtual bool IsWithdrawal { get { return false; } }

		#region Abstract
		protected abstract MessageSendingNotificationCollection GetNotifications();
		protected abstract SingleMessageManager[] GetApplicableManagers();
		protected abstract void DoActionCore();
		#endregion

		protected ArrayList generatedMessages;
		#endregion

	}
}
