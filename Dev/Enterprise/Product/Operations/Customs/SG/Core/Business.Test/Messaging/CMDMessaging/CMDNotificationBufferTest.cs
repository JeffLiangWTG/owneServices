using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture;

namespace Enterprise.Customs.SG.V4.Business.CMDMessaging.Testing
{
	class CMDNotificationBufferTest : TestCaseWithDummy
	{
		public void TestGetErrrorMessages()
		{
			SetupNotificationBufferForTest();
			ZString errorMessages = NotificationBuffer.GetErrorMessages();
			ZString expectedErrorMessages = @"
DummyParent
CMD Error
Error

DummyChild1
CMD Error From Child 1

DummyChild2
CMD Error From Child 2".Trim();
			AssertEquals(expectedErrorMessages, errorMessages);
		}

		public void TestGetErrorMessages_ParentBizOHasNoNotifications()
		{
			SetNotificationBufferForTest_ParentBizOHasNoNotifications();
			ZString errorMessages = NotificationBuffer.GetErrorMessages();
			ZString expectedErrorMessages = @"
DummyChild1
CMD Error From Child 1

DummyChild2
CMD Error From Child 2".Trim();
			AssertEquals(expectedErrorMessages, errorMessages);
		}

		public void TestGetErrorMessages_ChildrenBizOsHaveNoNotifications()
		{
			SetNotificationBufferForTest_ChildrenBizOsHaveNoNotifications();
			ZString errorMessages = NotificationBuffer.GetErrorMessages();
			ZString expectedErrorMessages = @"
DummyParent
CMD Error
Error".Trim();
			AssertEquals(expectedErrorMessages, errorMessages);
		}

		public void TestGetInfoMessages()
		{
			SetupNotificationBufferForTest();
			ZString infoMessages = NotificationBuffer.GetInfoMessages();
			ZString expectedInfoMessages = @"
DummyParent
CMD Info
Info

DummyChild1
CMD Info From Child 1

DummyChild2
CMD Info From Child 2".Trim();
			AssertEquals(expectedInfoMessages, infoMessages);
		}

		public void TestGetInfoMessages_ParentBizOHasNoNotifications()
		{
			SetNotificationBufferForTest_ParentBizOHasNoNotifications();
			ZString infoMessages = NotificationBuffer.GetInfoMessages();
			ZString expectedInfoMessages = @"
DummyChild1
CMD Info From Child 1

DummyChild2
CMD Info From Child 2".Trim();
			AssertEquals(expectedInfoMessages, infoMessages);
		}

		public void TestGetInfoMessages_ChildrenBizOsHaveNoNotifications()
		{
			SetNotificationBufferForTest_ChildrenBizOsHaveNoNotifications();
			ZString infoMessages = NotificationBuffer.GetInfoMessages();
			ZString expectedInfoMessages = @"
DummyParent
CMD Info
Info".Trim();
			AssertEquals(expectedInfoMessages, infoMessages);
		}

		#region Implementation
		protected override void SetUp()
		{
			base.SetUp();
			Dummy.HumanReadableNameForTest = "DummyParent";
			dummyChildren1 = Dummy.Collection.AddNew();
			dummyChildren1.HumanReadableNameForTest = "DummyChild1";
			dummyChildren2 = Dummy.Collection.AddNew();
			dummyChildren2.HumanReadableNameForTest = "DummyChild2";
		}

		void SetupNotificationBufferForTest()
		{
			NotificationBuffer.Clear();
			AddCMDNotificationToBuffer("CMD Error", Dummy, ErrorType.Error);
			AddCMDNotificationToBuffer("CMD Error From Child 1", dummyChildren1, ErrorType.Error);
			AddCMDNotificationToBuffer("CMD Error From Child 2", dummyChildren2, ErrorType.Error);
			AddCMDNotificationToBuffer("CMD Info From Child 1", dummyChildren1, NotificationSubscriberType.Info);
			AddCMDNotificationToBuffer("CMD Info", Dummy, NotificationSubscriberType.Info);
			AddCMDNotificationToBuffer("CMD Info From Child 2", dummyChildren2, NotificationSubscriberType.Info);
			AddInfoNotificationToBuffer("Info");
			AddErrorNotificationToBuffer("Error");
		}

		void SetNotificationBufferForTest_ParentBizOHasNoNotifications()
		{
			NotificationBuffer.Clear();
			AddCMDNotificationToBuffer("CMD Error From Child 1", dummyChildren1, ErrorType.Error);
			AddCMDNotificationToBuffer("CMD Error From Child 2", dummyChildren2, ErrorType.Error);
			AddCMDNotificationToBuffer("CMD Info From Child 1", dummyChildren1, NotificationSubscriberType.Info);
			AddCMDNotificationToBuffer("CMD Info From Child 2", dummyChildren2, NotificationSubscriberType.Info);
		}

		void SetNotificationBufferForTest_ChildrenBizOsHaveNoNotifications()
		{
			NotificationBuffer.Clear();
			AddCMDNotificationToBuffer("CMD Error", Dummy, ErrorType.Error);
			AddCMDNotificationToBuffer("CMD Info", Dummy, NotificationSubscriberType.Info);
			AddInfoNotificationToBuffer("Info");
			AddErrorNotificationToBuffer("Error");
		}

		void AddCMDNotificationToBuffer(string message, BusinessObject bizO, NotificationSubscriberType type)
		{
			CMDNotification notification = new CMDNotification(message, bizO, type);
			NotificationBuffer.Notify(notification);
		}

		void AddErrorNotificationToBuffer(string message)
		{
			NotificationBuffer.Notify(new ErrorNotification(ErrorType.Error, message));
		}

		void AddInfoNotificationToBuffer(string message)
		{
			NotificationBuffer.Notify(new InfoNotification(message));
		}

		CMDNotificationBuffer NotificationBuffer
		{
			get
			{
				if (fNotificationBuffer == null)
				{
					fNotificationBuffer = new CMDNotificationBuffer(Dummy);
				}

				return fNotificationBuffer;
			}
		}

		CMDNotificationBuffer fNotificationBuffer;
		DummyChildBusinessObject dummyChildren1;
		DummyChildBusinessObject dummyChildren2;
		#endregion
	}
}
