using CargoWise.EntityFramework;

namespace Enterprise.Customs.US.Business
{
	public struct NotificationInfo
	{
		public NotificationInfo(string message, NotificationTypes notificationType)
		{
			this.message = message;
			this.notificationType = notificationType;
		}

		public readonly string message;
		public readonly NotificationTypes notificationType;
	}
}