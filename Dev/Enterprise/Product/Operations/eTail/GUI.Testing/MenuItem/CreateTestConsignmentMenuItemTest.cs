using System;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.Environment;
using Enterprise.Integration.Licensing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.eTail.GUI.Testing
{
	public class CreateTestConsignmentMenuItemTest : TestCaseWithFactory
	{
		public void TestCreateTestConsignmentMenuItem_OnlyAvailableToRunAfterDataSaved()
		{
			var unsavedShipment = HVLVMenuItemTestHelper.CreateShipmentWithAddress(Factory, out _, out _, out _);

			using (var form = new ZForm(unsavedShipment))
			{
				form.PlugIns.Add(ControllerIDs.ETailShipment);
				form.Show();
				Assert("Pre-condition: Shipment is not saved", form.BusinessEntity.HasChanges);

				var plugin = form.PlugIns.GetPlugIn(ControllerIDs.ETailShipment);
				var hVLVMenu = plugin.TopLevelMenu;
				hVLVMenu.PerformSelect();

				var menuItem = hVLVMenu.MenuItems.OfType<CreateTestConsignmentMenuItem>().Single();
				AssertNotNull(menuItem);
				menuItem.PerformClick();
				AssertEquals("Should pop up saving notification before creating test consignments", "Please save the form before creating test consignments", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestCreateTestConsignmentMenuItem_OnlyVisibleInTestEnvioronmentAndSupportUser()
		{
			var shipment = HVLVMenuItemTestHelper.CreateShipmentValidForNewETailData(Factory, true);

			ObjectFactory.Get<IProductRegistration>().KeyForTest.DatabaseTypeForTest = DatabaseTypes.Codes.Production;
			using (EnvProxy.Instance.SetTemporaryUserContext(User.SupportUserName, Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
			{
				AssertCreateTestConsignmentMenuItemVisible(false);
			}

			ObjectFactory.Get<IProductRegistration>().KeyForTest.DatabaseTypeForTest = DatabaseTypes.Codes.Test;
			using (EnvProxy.Instance.SetTemporaryUserContext(User.UnKnownUserName, Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
			{
				AssertCreateTestConsignmentMenuItemVisible(false);
			}

			ObjectFactory.Get<IProductRegistration>().KeyForTest.DatabaseTypeForTest = DatabaseTypes.Codes.Test;
			using (EnvProxy.Instance.SetTemporaryUserContext(User.SupportUserName, Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
			{
				AssertCreateTestConsignmentMenuItemVisible(true);
			}

			void AssertCreateTestConsignmentMenuItemVisible(bool isVisible)
			{
				using (var form = new ZForm(shipment))
				{
					form.PlugIns.Add(ControllerIDs.ETailShipment);
					form.Show();
					var plugin = form.PlugIns.GetPlugIn(ControllerIDs.ETailShipment);

					var hVLVMenu = plugin.TopLevelMenu;
					hVLVMenu.PerformSelect();
					hVLVMenu.OnPopup(EventArgs.Empty);

					var menuItem = hVLVMenu.MenuItems.OfType<CreateTestConsignmentMenuItem>().Single();
					if (isVisible)
					{
						AssertEquals(true, menuItem.Visible);
					}
					else
					{
						AssertEquals(false, menuItem.Visible);
					}
				}
			}
		}
	}
}
