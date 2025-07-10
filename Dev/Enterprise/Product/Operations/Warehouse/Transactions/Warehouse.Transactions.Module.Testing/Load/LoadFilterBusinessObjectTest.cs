using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Module.Testing
{
	[TestedType(typeof(LoadFilterBusinessObject))]
	public class LoadFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		#region TestGetModuleFilterThatOverridesAllOtherFilters

		public void TestGetModuleFilterThatOverridesAllOtherFilters()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var dockDoorLocation = data.Whs1.DefaultOutboundDockDoorLocation;

			var transportCompany = Helper.CreateClient("TC1");
			var carrierServicelevel = transportCompany.MiscServ.CarrierServiceLevels.AddNew();
			carrierServicelevel.PL_Code = "RD";
			carrierServicelevel.PL_CarrierServiceLevelDescription = "Road";

			var truck = Helper.CreateEquipment("T001", 1m, Core.Constants.Weight.Kilograms, 1m, Core.Constants.Volume.CubicMetres);
			var load1 = Helper.CreateWhsLoad(transportCompany, dockDoorLocation, "WL00000001", carrierServicelevel.PL_Code, truck);
			var load2 = Helper.CreateWhsLoad(transportCompany, dockDoorLocation, "WL00000002", carrierServicelevel.PL_Code, truck);
			Factory.Save();
			Asserter.AddToScope(load1, load2);

			var filter = (ModuleFountainFilter)FilterStrip.ModuleFilters[LoadFilterBusinessObject.FilterConstants.LoadJobID];
			filter.Property = "WL00000001";
			Asserter.AssertMatches("", filter, load1);

			filter.Property = "WL00000002";
			Asserter.AssertMatches("", filter, load2);
		}

		#endregion

		#region

		#endregion

		#region Implementation

		WhsTestHelperFunctions Helper => helper = helper ?? new WhsTestHelperFunctions(Factory);
		WhsTestHelperFunctions helper;

		FilterStripAsserter<WhsLoad> Asserter => asserter ?? (asserter = new FilterStripAsserter<WhsLoad>(Factory, v => v.WLO_JobID));
		FilterStripAsserter<WhsLoad> asserter;

		LoadFilterBusinessObject FilterStrip => filterStrip ?? (filterStrip = GetNewFilterStrip());
		LoadFilterBusinessObject filterStrip;

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject() => new LoadFilterBusinessObject();

		LoadFilterBusinessObject GetNewFilterStrip()
		{
			var filterStrip = new LoadFilterBusinessObject();
			filterStrip.AddActiveStatusFilters(typeof(WhsLoad));
			return filterStrip;
		}

		#endregion
	}
}
