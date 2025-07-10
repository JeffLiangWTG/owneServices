using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture;

namespace Enterprise.Customs.SG.V4.Business.CMDMessaging
{
	public abstract class CMDWrapperBase : NonPersistentBusinessObject, IObsoleteValidation
	{
		public CMDWrapperBase(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		protected void NotifyCMDError(INotifications notifications, ZString message, BusinessObject bizO)
		{
			notifications.Notify(new CMDNotification(message, bizO, ErrorType.Error));
		}

		protected void NotifyCMDInfo(INotifications notifications, ZString message, BusinessObject bizO)
		{
			notifications.Notify(new CMDNotification(message, bizO, NotificationSubscriberType.Info));
		}

		public abstract CMDShipmentWrapper[] CMDShipments { get; }

		public abstract void SendMessage(INotifications notifications);
		public abstract void DeleteExistingCMDMessages(INotifications notifications);
		public abstract void SendMessage(string recipient, INotifications notifications);

		public abstract void RunPreSendValidation(INotifications notifications);
		public abstract void RunPreDeleteValidation(INotifications notifications);
	}
}
