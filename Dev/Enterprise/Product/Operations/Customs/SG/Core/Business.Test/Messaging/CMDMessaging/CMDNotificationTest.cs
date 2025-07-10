using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.SG.V4.Business.CMDMessaging.Testing
{
	[TestedType(typeof(CMDNotification))]
	class CMDNotificationTest : NotificationTest<CMDNotification>
	{
		public void TestConstructor()
		{
			CMDNotification notification = new CMDNotification("Test123", Dummy, ErrorType.Error);
			AssertEquals("Should be assigned in the constructor", Dummy, notification.BizO);
			AssertEquals("Should be assigned in the constructor", "Test123", notification.AdditionalInfo);
			AssertEquals(ErrorType.Error, notification.Type);
		}

		#region Implementation
		protected override CMDNotification NewTestNotification()
		{
			return new CMDNotification("Message", Dummy, NotificationSubscriberType.Info);
		}

		protected override bool IsSerializable
		{
			get
			{
				return false;
			}
		}

		DummyBusinessObject Dummy
		{
			get
			{
				if (dummy == null)
				{
					dummy = Factory.New<DummyBusinessObject>();
				}

				return dummy;
			}
		}

		DummyBusinessObject dummy;
		#endregion
	}
}
