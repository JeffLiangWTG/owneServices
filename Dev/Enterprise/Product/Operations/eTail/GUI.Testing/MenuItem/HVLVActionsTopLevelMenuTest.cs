using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.eTail.GUI.Testing
{
	public class HVLVActionsTopLevelMenuTest : TestCaseWithFactory
	{
		public void TestSubMenuItemsShouldNotBeBuiltOnCreation()
		{
			AssertEquals(0, topLevelMenu.MenuItems.Count);
		}

		public void TestSubMenuItemsShouldBeBuiltOnSelect()
		{
			topLevelMenu.PerformSelect();

			CombineAssertions(() =>
			{
				AssertEquals(6, topLevelMenu.MenuItems.Count);
				AssertContainsExactElementsInAnyOrder(new[]
				{
					typeof(CalculateLMCDepotDetailsMenuItem),
					typeof(HVLVPreScreenMenuGroup),
					typeof(HVLVManifestMenuItem),
					typeof(HVLVCustomsMenuGroup),
					typeof(NavigateToEcommerceWebPortalsMenuGroup),
					typeof(CreateTestConsignmentMenuItem),
				}, topLevelMenu.MenuItems.OfType<ZMenuItem>().Select(x => x.GetType()).ToList());
			});
		}

		public void TestSubMenuItemsShouldNotBeBuiltIfExist()
		{
			topLevelMenu.PerformSelect();
			topLevelMenu.PerformSelect();

			AssertEquals(6, topLevelMenu.MenuItems.Count);
		}

		protected override void SetUp()
		{
			base.SetUp();

			var shipment = HVLVMenuItemTestHelper.CreateShipmentWithAddress(Factory, out _, out _, out _);
			Factory.Save();

			form = new ZForm(shipment);
			form.PlugIns.Add(ControllerIDs.ETailShipment);
			form.Show();

			var plugin = form.PlugIns.GetPlugIn(ControllerIDs.ETailShipment);
			topLevelMenu = plugin.TopLevelMenu;
		}

		protected override void TearDown()
		{
			form?.Dispose();
			base.TearDown();
		}

		ZForm form;
		MenuItem topLevelMenu;
	}
}
