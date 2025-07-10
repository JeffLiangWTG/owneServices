using System;
using System.Windows.Forms;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.SG.V4.Business.CMDMessaging;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.SG.V4.GUI.Testing
{
	sealed class CMDPlugInBaseOnlyTest : TestCaseWithDummy
	{
		public void TestName()
		{
			AssertEquals("CMD", plugIn.Name);
		}

		public void TestLicenceCheckPoint()
		{
			AssertEquals(Env.Licence.Core, plugIn.LicenceCheckPoint);
		}

		public void TestSetEnabledCalledInTheConstructor()
		{
			Dummy.Z0_Code = Core.Constants.TransportModes.Air;
			using (CMDPlugIn plugIn = new CMDPlugInForTest(Dummy))
			{
				Assert("Valid transport mode", plugIn.Enabled);
			}

			Dummy.Z0_Code = Core.Constants.TransportModes.SeaAir;
			using (CMDPlugIn plugIn = new CMDPlugInForTest(Dummy))
			{
				Assert("Valid transport mode", plugIn.Enabled);
			}

			Dummy.Z0_Code = Core.Constants.TransportModes.AirSea;
			using (CMDPlugIn plugIn = new CMDPlugInForTest(Dummy))
			{
				Assert("Valid transport mode", plugIn.Enabled);
			}

			Dummy.Z0_Code = Core.Constants.TransportModes.Sea;
			using (CMDPlugIn plugIn = new CMDPlugInForTest(Dummy))
			{
				Assert("Invalid transport mode", !plugIn.Enabled);
			}
		}

		public void TestHookEventsCalledInTheConstructor()
		{
			Assert(plugIn.HookEventsCalled);
		}

		public void TestUnhookEventsCalledOnDispose()
		{
			Assert("Pre-condition", !plugIn.UnhookEventsCalled);

			plugIn.Dispose();
			Assert(plugIn.UnhookEventsCalled);
		}

		public void TestMenuItems()
		{
			AssertEquals("CMD", plugIn.TopLevelMenu.Text);
			AssertEquals(3, plugIn.TopLevelMenu.MenuItems.Count);
			AssertEquals("Send CMD", plugIn.TopLevelMenu.MenuItems[0].Text);
			AssertEquals("Delete CMD", plugIn.TopLevelMenu.MenuItems[1].Text);
			AssertEquals("Send CMD to Specific GHA", plugIn.TopLevelMenu.MenuItems[2].Text);
		}

		public void TestSendMenuItemClick()
		{
			AssertNull("Pre-condition", plugIn.BusinessEntity.LastMethodCalled);
			AssertNull("Pre-condition", plugIn.BusinessEntity.LastMethodCallArguments);

			MenuItem sendCMDMenuItem = plugIn.TopLevelMenu.MenuItems[0];
			sendCMDMenuItem.PerformClick();
			AssertEquals("SendMessage", plugIn.BusinessEntity.LastMethodCalled);
			AssertEquals(1, plugIn.BusinessEntity.LastMethodCallArguments.Length);
			CMDNotificationBuffer notificationBuffer = plugIn.BusinessEntity.LastMethodCallArguments[0] as CMDNotificationBuffer;
			AssertEquals("Should be the HostBusinessEntity", Dummy, notificationBuffer.Parent);
		}

		public void TestDeleteMenuItemClick()
		{
			AssertNull("Pre-condition", plugIn.BusinessEntity.LastMethodCalled);
			AssertNull("Pre-condition", plugIn.BusinessEntity.LastMethodCallArguments);

			MenuItem deleteCMDMenuItem = plugIn.TopLevelMenu.MenuItems[1];
			deleteCMDMenuItem.PerformClick();
			AssertEquals("DeleteExistingCMDMessages", plugIn.BusinessEntity.LastMethodCalled);
			AssertEquals(1, plugIn.BusinessEntity.LastMethodCallArguments.Length);
			CMDNotificationBuffer notificationBuffer = plugIn.BusinessEntity.LastMethodCallArguments[0] as CMDNotificationBuffer;
			AssertEquals("Should be the HostBusinessEntity", Dummy, notificationBuffer.Parent);
		}

		public void TestSendToSpecificGHAMenuItemClick()
		{
			AssertNull("Pre-condition", plugIn.BusinessEntity.LastMethodCalled);
			AssertNull("Pre-condition", plugIn.BusinessEntity.LastMethodCallArguments);

			MenuItem sendCMDMenuItem = plugIn.TopLevelMenu.MenuItems[2];
			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
			sendCMDMenuItem.PerformClick();
			AssertEquals("SendMessageWithRecipient", plugIn.BusinessEntity.LastMethodCalled);
			AssertEquals(2, plugIn.BusinessEntity.LastMethodCallArguments.Length);
			AssertEquals("", plugIn.BusinessEntity.LastMethodCallArguments[0].ToString());
			CMDNotificationBuffer notificationBuffer = plugIn.BusinessEntity.LastMethodCallArguments[1] as CMDNotificationBuffer;
			AssertEquals("Should be the HostBusinessEntity", Dummy, notificationBuffer.Parent);
		}

		public void TestSendToSpecificGHAMenuItemClick_Cancelled()
		{
			AssertNull("Pre-condition", plugIn.BusinessEntity.LastMethodCalled);
			AssertNull("Pre-condition", plugIn.BusinessEntity.LastMethodCallArguments);

			MenuItem sendCMDMenuItem = plugIn.TopLevelMenu.MenuItems[2];
			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Cancel;
			sendCMDMenuItem.PerformClick();
			AssertNull("Should be cancelled", plugIn.BusinessEntity.LastMethodCalled);
			AssertNull("Should be cancelled", plugIn.BusinessEntity.LastMethodCallArguments);
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();

			Dummy.Factory.Save();
			plugIn = new CMDPlugInForTest(Dummy);
		}

		protected override void TearDown()
		{
			plugIn.Dispose();
			base.TearDown();
		}

		CMDPlugInForTest plugIn;

		#endregion

		#region class CMDPlugInForTest

		class CMDPlugInForTest : CMDPlugIn
		{
			public CMDPlugInForTest(DummyBusinessObject dummy)
				: base(dummy)
			{
			}

			public new LicenceCheckpoint LicenceCheckPoint
			{
				get { return base.LicenceCheckPoint; }
			}

			protected internal override ZString TransportMode
			{
				get { return ((DummyBusinessObject)HostBusinessEntity).Z0_Code; }
			}

			protected internal override void HookEvents()
			{
				HookEventsCalled = true;
			}

			protected internal override void UnhookEvents()
			{
				UnhookEventsCalled = true;
			}

			protected override ZBool HasUserControl
			{
				get { return false; }
			}

			protected override CMDWrapperBase GetCMDWrapperBizO()
			{
				return new CMDWrapperForTest(HostBusinessEntity.Factory);
			}

			public new CMDWrapperForTest BusinessEntity
			{
				get { return (CMDWrapperForTest)base.BusinessEntity; }
			}

			public bool HookEventsCalled;
			public bool UnhookEventsCalled;
		}

		#endregion

		#region class CMDWrapperForTest

		class CMDWrapperForTest : CMDWrapperBase
		{
			public CMDWrapperForTest(BusinessObjectFactory factory)
				: base(factory)
			{
			}

			public override void SendMessage(INotifications notifications)
			{
				LastMethodCalled = "SendMessage";
				LastMethodCallArguments = new object[] { notifications };
			}

			public override void SendMessage(string recipient, INotifications notifications)
			{
				LastMethodCalled = "SendMessageWithRecipient";
				LastMethodCallArguments = new object[] { recipient, notifications };
			}

			public override void DeleteExistingCMDMessages(INotifications notifications)
			{
				LastMethodCalled = "DeleteExistingCMDMessages";
				LastMethodCallArguments = new object[] { notifications };
			}

			public string LastMethodCalled;
			public object[] LastMethodCallArguments;

			public override CMDShipmentWrapper[] CMDShipments
			{
				get { throw new Exception("The method or operation is not implemented."); }
			}

			public override void RunPreSendValidation(INotifications notifications)
			{
			}

			public override void RunPreDeleteValidation(INotifications notifications)
			{
			}
		}

		#endregion
	}
}
