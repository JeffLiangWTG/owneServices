using System;
using CargoWise.ComponentModel;
using CargoWise.Types;

namespace Enterprise.Customs.US.AMS.Business
{
	[Serializable]
	public class StowPlanNotification : Notification
	{
		public StowPlanNotification(IStowPlanNotificationProvider provider, INotificationType type, string message, ZString detail)
			: this(provider.TargetPK, provider.TargetCode, provider.TargetSubject, type, message, detail)
		{
		}

		public StowPlanNotification(ZString targetCode, ZString subject, INotificationType type, string message, ZString detail)
			: this(Guid.Empty, targetCode, subject, type, message, detail)
		{
		}

		StowPlanNotification(Guid targetPK, ZString targetCode, ZString subject, INotificationType type, string message, ZString detail)
			: base(type, message)
		{
			this.TargetPK = targetPK;
			this.Subject = subject;
			this.TargetCode = targetCode;
			this.Detail = detail;
		}

		public readonly Guid TargetPK;
		public readonly string TargetCode;
		public readonly string Subject;
		public readonly string Detail;

		public override INotification ReplaceMessage(string message)
		{
			return new StowPlanNotification(TargetPK, TargetCode, Subject, Type, Message, Detail);
		}
	}
}
