
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture;

namespace Enterprise.Customs.SG.V4.Business.CMDMessaging
{
	[Core.NonSerializedClass]
	public class CMDNotification : NotificationSubscriberNotification
	{
		public CMDNotification(string message, BusinessObject bizO, NotificationSubscriberType type)
			: base(type, message)
		{
			this.BizO = bizO;
		}

		public readonly BusinessObject BizO;
	}
}
