using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture;

namespace Enterprise.Customs.SG.V4.Business.CMDMessaging.Testing
{
	sealed class CMDWrapperTest : TestCaseWithDummy
	{
		public void TestNotifyCMDError()
		{
			WrapperBizO.NotifyCMDError(NotificationBuffer, "Error", Dummy);
			CMDNotification notification = NotificationBuffer.Events[0] as CMDNotification;
			AssertNotNull("Should be of type CMDNotification", notification);
			AssertEquals("Error", notification.AdditionalInfo);
			AssertEquals(ErrorType.Error, notification.Type);
			AssertEquals(Dummy, notification.BizO);
		}

		public void TestNotifyCMDInfo()
		{
			WrapperBizO.NotifyCMDInfo(NotificationBuffer, "Info", Dummy);
			CMDNotification notification = NotificationBuffer.Events[0] as CMDNotification;
			AssertNotNull("Should be of type CMDNotification", notification);
			AssertEquals("Info", notification.AdditionalInfo);
			AssertEquals(NotificationSubscriberType.Info, notification.Type);
			AssertEquals(Dummy, notification.BizO);
		}

		#region Implementation

		NotificationBuffer NotificationBuffer
		{
			get
			{
				if (fNotificationBuffer == null)
				{
					fNotificationBuffer = new NotificationBuffer();
				}
				return fNotificationBuffer;
			}
		}

		CMDWrapperForTest WrapperBizO
		{
			get
			{
				if (fWrapperBizO == null)
				{
					fWrapperBizO = new CMDWrapperForTest(Factory);
				}
				return fWrapperBizO;
			}
		}

		NotificationBuffer fNotificationBuffer;
		CMDWrapperForTest fWrapperBizO;
		#endregion
	}
}
