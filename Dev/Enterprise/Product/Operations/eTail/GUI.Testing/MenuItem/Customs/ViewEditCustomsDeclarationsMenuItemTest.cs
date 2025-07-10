using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.eTail.GUI.Testing
{
	public class ViewEditCustomsDeclarationsMenuItemTest : TestCaseWithFactory
	{
		public void TestViewEditFormalDeclarationMenuItem_ShouldShowJobDeclarationModule()
		{
			var shipment = HVLVMenuItemTestHelper.CreateShipmentValidForNewETailData(Factory, true);
			using (var form = new ZForm(shipment))
			{
				form.PlugIns.Add(ControllerIDs.ETailShipment);
				form.Show();

				var plugin = form.PlugIns.GetPlugIn(ControllerIDs.ETailShipment);
				var menuHVLV = plugin.TopLevelMenu;
				menuHVLV.PerformSelect();

				var customsMenu = menuHVLV.MenuItems.OfType<HVLVCustomsMenuGroup>().Single();
				var declarationMenuGroup = customsMenu.MenuItems.OfType<DeclarationMenuGroup>().Single();
				var menuItem = declarationMenuGroup.MenuItems.OfType<ViewEditCustomsDeclarationsMenuItem>().Single();

				AssertNotNull("View/Edit Customs Declaration Menu Item should be added", menuItem);
				menuItem.PerformClick();

				var modulePopup = ZFormModaliser.LastFormShownDialogForTest as EmbeddedModulePopup;
				AssertNotNull("Should have shown an EmbeddedModulePopup", modulePopup);

				var module = modulePopup.Module_ForTest;
				AssertEquals("ModulePopup should be for JobDeclarations", ModuleIDs.Customs.JobDeclaration, module.ID);
				AssertEquals("A filter default should have been added", true, module.FilterBusinessObject.ContainsDefaults);

				var shipmentFilter = module.FilterBusinessObject["Related HVL Shipment"] as ModuleGuidFilter;
				AssertNotNull("Related HVL Shipment filter should be added", shipmentFilter);
				AssertEquals("Filter Default should be populated with current shipment PK", shipment.PK, shipmentFilter.Property);
			}
		}
	}
}
