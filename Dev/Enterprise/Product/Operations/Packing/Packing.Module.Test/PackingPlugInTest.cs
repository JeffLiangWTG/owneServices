using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.Integration.TransportBooking;
using Enterprise.Packing.Business.Testing;
using Enterprise.Packing.GUI;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.PlugIn;
using Enterprise.ZArchitecture.PlugIn.Testing;

namespace Enterprise.Packing.Module.Testing
{
	public class PackingPlugInTest : ZPlugInGenericTest
	{
		#region TestBusinessEntity

		public void TestBusinessEntity()
		{
			using (var plugin = GetPlugInToTest())
			{
				AssertEquals(Data.Dummy, plugin.BusinessEntity);
			}
		}

		#endregion

		#region TestHookSelectedTabChangedEvent

		public void TestHookSelectedTabChangedEvent()
		{
			Data.CreatePackingData();
			using (var form = new ZForm(Data.Dummy))
			{
				var tabControl = new ZTabControl();
				form.Controls.Add(tabControl);
				tabControl.TabPages.Insert(new ZTabPage(), 0);

				form.PlugIns.AddPlugInAtTabPageIndex(ControllerIDs.PackingPlugIn, 1);
				var plugin = form.PlugIns.GetPlugIn(ControllerIDs.PackingPlugIn); // sets up form hooks
				var userControl = (PackingUserControl)plugin.UserControl;

				// hook an event afterward to ensure tab changing bound the PackingUserControl
				bool controlWasBoundByTabIndexChanging = false;
				tabControl.SelectedIndexChanging += (sender, e) => controlWasBoundByTabIndexChanging = userControl.IsBound;

				form.Show();
				AssertEquals("Precondition", false, userControl.IsBound);
				AssertEquals("Precondition", false, controlWasBoundByTabIndexChanging);

				plugin.SelectTabPage();
				AssertEquals(true, userControl.IsBound);
				AssertEquals(true, controlWasBoundByTabIndexChanging);
			}
		}

		#endregion

		#region TestUpdateTabPageMinimumAutoSized

		public void TestUpdateTabPageMinimumAutoSized()
		{
			Data.CreatePackingData();
			using (var form = new ZForm(Data.Dummy))
			{
				var tabControl = new ZTabControl();
				form.Controls.Add(tabControl);
				tabControl.TabPages.Insert(new ZTabPage(), 0);

				form.PlugIns.AddPlugInAtTabPageIndex(ControllerIDs.PackingPlugIn, 1);
				var plugin = form.PlugIns.GetPlugIn(ControllerIDs.PackingPlugIn); // sets up form hooks
				var userControl = (PackingUserControl)plugin.UserControl;

				form.Show();
				plugin.SelectTabPage();

				AssertEquals(userControl.MinimumSize.Width + ControlDpiScalingHelper.ScaleToCurrentDpiX(4), plugin.TabPage.MinimumAutoSizedWidth);
				AssertEquals(userControl.MinimumSize.Height, plugin.TabPage.MinimumAutoSizedHeight);
			}
		}

		#endregion

		#region TestName

		public void TestName()
		{
			using (var plugin = GetPlugInToTest())
			{
				AssertEquals("Packing", plugin.Name);
			}
		}

		#endregion

		#region TestUserControl

		public void TestUserControl()
		{
			using (var plugin = GetPlugInToTest())
			{
				AssertEquals(ExpectedUserControlType, plugin.UserControl.GetType());
			}
		}

		protected virtual Type ExpectedUserControlType => typeof(PackingUserControl);

		#endregion

		public void TestWrongType()
		{
			var booking = Factory.New<IDtbBooking>();
			using (var form = new ZForm(booking))
			{
				var tabControl = new ZTabControl();
				form.Controls.Add(tabControl);
				tabControl.TabPages.Insert(new ZTabPage(), 0);

				form.PlugIns.AddPlugInAtTabPageIndex(ControllerIDs.PackingPlugIn, 1);
				var plugin = form.PlugIns.GetPlugIn(ControllerIDs.PackingPlugIn); // sets up form hooks
				var userControl = (PackingUserControl)plugin.UserControl;

				form.Show();
				AssertEquals("Precondition", false, userControl.IsBound);

				AssertNoExceptionThrown("Should not throw exception with invalid type", plugin.SelectTabPage);

				AssertEquals("IsBound should remain false, as PackingUserControl method SetDataBinding should not do anything if the business object for the form is not packing related", false, userControl.IsBound);
			}
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			DummyBusinessObject.TypeDecider.TypeForLoadOverride = typeof(DummyWithPacking);
		}

		protected override void TearDown()
		{
			base.TearDown();
			DummyBusinessObject.TypeDecider.TypeForLoadOverride = null;
		}

		protected override ZPlugIn GetPlugInToTest()
		{
			Data.CreatePackingData();
			return new PackingPlugIn(Data.Dummy);
		}

		protected TestDataForPacking Data => data ?? (data = new TestDataForPacking(Factory));
		TestDataForPacking data;

		#endregion
	}
}
