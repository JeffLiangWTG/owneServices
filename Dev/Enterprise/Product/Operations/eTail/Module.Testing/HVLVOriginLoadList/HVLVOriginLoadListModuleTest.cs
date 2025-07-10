using System;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.eTail.Module.Testing
{
	[TestedType(typeof(HVLVOriginLoadListModule))]
	class HVLVOriginLoadListModuleTest : ZModuleBasherTest
	{
		public void TestActionMenuOperationalActions()
		{
			using (var module = GetModule())
			{
				var grid = module.DisplayGrid as ZDisplayGrid;
				var actionMenu = grid.ContextMenu.MenuItems.FindByText("Actions");
				actionMenu.ShowPopupMenu();
				var operationalActionsMenu = actionMenu.MenuItems.FindByText("Operational Actions");
				AssertNotNull(operationalActionsMenu);
			}
		}

		public void TestNewButtonIsRemoved_UnlessInTestingMode()
		{
			CombineAssertions(() =>
			{
				assertNewMenuItemVisible(false, "New button should be removed by default");
				using (HVLVDataRegistry.Instance.HVLVOriginLoadListTestingMode.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
				{
					assertNewMenuItemVisible(true, "New button should exist in testing mode");
				}
			});

			void assertNewMenuItemVisible(bool expectToExist, string message)
			{
				using (var form = new ZForm())
				using (var module = GetModule())
				{
					form.Controls.Add(module.EmbeddedControl);
					form.Show();

					AssertEquals(message, expectToExist, null != module.FormActionMenu.FindByText("New"));
				}
			}
		}

		#region Implementations

		protected override ModuleIdentifier GetModuleID() => ModuleIDs.HVLVOriginLoadList;

		protected override bool HasController() => true;

		#endregion
	}
}
