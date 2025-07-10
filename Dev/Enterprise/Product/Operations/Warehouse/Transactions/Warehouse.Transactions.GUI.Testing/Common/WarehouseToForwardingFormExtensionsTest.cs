using System;
using CargoWise.Windows.UI;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Warehouse.Transactions.GUI.Testing
{
	class WarehouseToForwardingFormExtensionsTest : WhsGuiTestCaseWithFactory
	{
		#region TestHookCartageClickEvent

		public void TestHookCartageClickEvent()
		{
			using (var form = new ModuleToModuleFormExtensionsTest.DummyIWarehouseToForwardingForm(Factory.New<WhsOrder>()))
			using (var relatedJobsMenuItem = new ZMenuItem("test"))
			{
				form.PlugIns.Add(ControllerIDs.CartagePlugin);

				bool clickWasCalled = false;
				var plugin = form.PlugIns.GetPlugIn(ControllerIDs.CartagePlugin);
				plugin.TopLevelMenu.Click += (sender, e) => clickWasCalled = true;

				form.HookCartageClickEvent(relatedJobsMenuItem);
				relatedJobsMenuItem.OnPopup(EventArgs.Empty);
				AssertEquals(true, clickWasCalled);
			}
		}

		#endregion
	}
}
