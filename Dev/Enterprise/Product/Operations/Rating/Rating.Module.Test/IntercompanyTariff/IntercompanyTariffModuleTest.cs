using System.Linq;
using System.Windows.Forms;
using CargoWise.Definitions;
using CargoWise.Windows.UI;
using Enterprise.Rating.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Rating.Module.Testing
{
	[TestedType(typeof(IntercompanyTariffModule))]
	public class IntercompanyTariffModuleTest : ZModuleBasherTest
	{
		protected override ModuleIdentifier GetModuleID() =>
			ModuleIDs.IntercompanyTariffs;

		public void TestBusinessContexts()
		{
			using (ClientRatesModule module = new ClientRatesModule())
			{
				AssertEquals("Only one business context should be returned", 1, module.BusinessContexts.Length);
				AssertEquals("Rating business context should be returned", BusinessContext.Rating, module.BusinessContexts[0]);
				Assert("Business context array should be returned", module.BusinessContexts is BusinessContext[]);
			}
		}

		public void TestUpdatesSubMenuAndItems()
		{
			using (var module = new IntercompanyTariffModuleForTest())
			{
				var updateSubMenu = module.ToolBarButtons.FindByText("Actions").DropDownMenu.MenuItems.Cast<MenuItem>().FirstOrDefault(item => item.Text == "Updates");
				AssertNotNull("Should find the Updates sub menu", updateSubMenu);

				var menuItem = updateSubMenu.MenuItems.Cast<MenuItem>().FirstOrDefault(item => item.Text == "Bulk Rate/Cost Update");
				AssertNotNull("Should find the Bulk Rate/Cost Update menu item", menuItem);

				var controller = module.GetNewBulkRateUpdatesControllerForTest();
				Assert("controller should be BulkRateUpdatesController", controller is BulkRateUpdatesController);
				AssertEquals("Default Rate Type should be IntercompanyTariff", RatingConstants.RatingHeaderTypes.IntercompanyTariff, (controller as BulkRateUpdatesController).DefaultRateType);
			}
		}
	}

	class IntercompanyTariffModuleForTest : IntercompanyTariffModule
	{
		public ZController GetNewBulkRateUpdatesControllerForTest()
		{
			return GetNewBulkRateUpdatesController();
		}
	}
}
