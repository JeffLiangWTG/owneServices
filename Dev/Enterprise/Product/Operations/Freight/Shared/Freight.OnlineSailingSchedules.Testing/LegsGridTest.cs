using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Module;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Freight.GUI.OnlineSailingSchedules.Testing
{
	public class LegsGridTest : TestCaseWithFactory
	{
		public void TestContextMenuItems()
		{
			using (var grid = new LegsGrid())
			{
				var updateVesselImoMenuItem = grid.UpdateVesselImoMenuItem;
				var createNewVesselNameMenuItem = grid.CreateNewVesselMenuItem;
				var assignScacCodeToCarrierMenuItem = grid.AssignScacCodeToCarrierMenuItem;

				AssertEquals("Update Vessel IMO", updateVesselImoMenuItem.Caption);
				AssertEquals("Create New Vessel", createNewVesselNameMenuItem.Caption);
				AssertEquals("Assign SCAC Code To Carrier", assignScacCodeToCarrierMenuItem.Caption);

				var menuItems = grid.ContextMenu.MenuItems;
				Assert(menuItems.Contains(updateVesselImoMenuItem));
				Assert(menuItems.Contains(createNewVesselNameMenuItem));
				Assert(menuItems.Contains(assignScacCodeToCarrierMenuItem));
			}
		}

		public void TestAssignScacCodeToCarrierModule()
		{
			using (var legsGrid = new LegsGridForTest())
			using (var module = legsGrid.GetOrgModule())
			{
				var filterBusinessObject = module.FilterBusinessObject;
				var alwaysVisibleModuleFilters = filterBusinessObject.AlwaysVisibleModuleFilters;

				AssertEquals(4, alwaysVisibleModuleFilters.Count);
				AssertCollectionContains((ModuleTextFilter)filterBusinessObject["Name"], alwaysVisibleModuleFilters);
				AssertCollectionContains((ModuleTextFilter)filterBusinessObject["Code"], alwaysVisibleModuleFilters);
				AssertCollectionContains((OrgTypeModuleFilter)filterBusinessObject["Organisation Types"], alwaysVisibleModuleFilters);
				AssertCollectionContains((OrgSecondaryTypeModuleFilter)filterBusinessObject["Secondary Type"], alwaysVisibleModuleFilters);

				var orgTypeModuleFilter = (OrgTypeModuleFilter)filterBusinessObject["Organisation Types"];
				AssertEquals(true, orgTypeModuleFilter.Property4);
				var orgSecondaryTypeModuleFilter = (OrgSecondaryTypeModuleFilter)filterBusinessObject["Secondary Type"];
				AssertEquals("Carrier - Shipping Line", orgSecondaryTypeModuleFilter.Property);
			}
		}

		class LegsGridForTest : LegsGrid
		{
			public new ZFilterModule GetOrgModule()
			{
				return base.GetOrgModule();
			}
		}
	}
}
