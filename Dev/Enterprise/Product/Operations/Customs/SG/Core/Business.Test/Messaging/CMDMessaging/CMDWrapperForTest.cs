using System;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.SG.V4.Business.CMDMessaging.Testing
{
	class CMDWrapperForTest : CMDWrapperBase
	{
		public CMDWrapperForTest(BusinessObjectFactory factory) : base(factory)
		{
		}

		public new void NotifyCMDError(INotifications notifications, ZString message, BusinessObject bizO)
		{
			base.NotifyCMDError(notifications, message, bizO);
		}

		public new void NotifyCMDInfo(INotifications notifications, ZString message, BusinessObject bizO)
		{
			base.NotifyCMDInfo(notifications, message, bizO);
		}

		public override CMDShipmentWrapper[] CMDShipments
		{
			get
			{
				throw new Exception("The method or operation is not implemented.");
			}
		}

		public override void SendMessage(INotifications notifications)
		{
			throw new Exception("The method or operation is not implemented.");
		}

		public override void DeleteExistingCMDMessages(INotifications notifications)
		{
			throw new Exception("The method or operation is not implemented.");
		}

		public override void SendMessage(string recipient, INotifications notifications)
		{
			throw new Exception("The method or operation is not implemented.");
		}

		public override void RunPreSendValidation(INotifications notifications)
		{
			throw new Exception("The method or operation is not implemented.");
		}

		public override void RunPreDeleteValidation(INotifications notifications)
		{
			throw new Exception("The method or operation is not implemented.");
		}
	}
}
