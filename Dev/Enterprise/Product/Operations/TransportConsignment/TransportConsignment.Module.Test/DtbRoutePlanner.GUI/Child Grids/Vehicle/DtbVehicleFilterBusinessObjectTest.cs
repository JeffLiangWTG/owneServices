using System;
using CargoWise.Application;
using CargoWise.Schema;
using Enterprise.MasterFiles.Business;
using Enterprise.TransportCommon.Integration;
using Enterprise.TransportConsignment.Business.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.TransportConsignment.Module.Testing
{
	[TestedType(typeof(DtbVehicleFilterBusinessObject))]
	public class DtbVehicleFilterBusinessObjectTest : DtbChildFilterBusinessObjectTest
	{
		#region TestVehicleBranch

		public void TestVehicleBranch()
		{
			var driver1 = Helper.CreateDriver("BOB", "BOB");
			var driversGroup = helper.CreateDriverGroup("Drivers", driver1);
			var transportRegistry = ObjectFactory.Get<ITransportRegistry>();
			transportRegistry.TransportDriversGroup.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, driversGroup.PK.ToGuid());

			var proxy1 = Factory.NewWithValidTestData<OrgHeader>();
			var proxy2 = Factory.NewWithValidTestData<OrgHeader>();
			var proxy3 = Factory.NewWithValidTestData<OrgHeader>();

			var vehicle1 = Helper.CreateVehicle(driver1, "TRK");
			var vehicle2 = Helper.CreateVehicle(driver1, "TR2");
			var vehicle3 = Helper.CreateVehicle(driver1, "TR3");
			var vehicle4 = Helper.CreateVehicle(driver1, "TR4");

			vehicle1.RQ_OH_Owner = proxy1.PK;
			vehicle2.RQ_OH_Owner = proxy2.PK;
			vehicle3.RQ_OH_Owner = proxy3.PK;
			vehicle4.RQ_OH_Owner = proxy3.PK;

			Asserter.AddToScope(vehicle1);
			Asserter.AddToScope(vehicle2);
			Asserter.AddToScope(vehicle3);
			Asserter.AddToScope(vehicle4);

			Factory.Save();

			var filterBizO = GetNewFilterStripBusinessObject();
			var branchFilter = (ModuleGuidFilter)filterBizO[DtbVehicleFilterBusinessObject.FilterConstants.Owner];
			AssertEquals(DtbVehicleFilterBusinessObject.FilterConstants.VehicleCategory, branchFilter.Category);

			branchFilter.IsActive = true;
			branchFilter.Property = proxy1.PK;
			Asserter.AssertMatches("vehicle1 has branch of branch1.", filterBizO.Filter, vehicle1);

			branchFilter.Property = proxy2.PK;
			Asserter.AssertMatches("vehicle2 has branch of branch2.", filterBizO.Filter, vehicle2);

			branchFilter.Property = proxy3.PK;
			Asserter.AssertMatches("vehicle3, vehicle4 has branch of branch3.", filterBizO.Filter, vehicle3, vehicle4);
		}

		#endregion

		#region TestVehicleState

		public void TestVehicleState()
		{
			var driver1 = Helper.CreateDriver("BOB", "BOB");
			var driversGroup = helper.CreateDriverGroup("Drivers", driver1);
			var transportRegistry = ObjectFactory.Get<ITransportRegistry>();
			transportRegistry.TransportDriversGroup.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, driversGroup.PK.ToGuid());

			var vehicle1 = Helper.CreateVehicle(driver1, "TRK");
			var vehicle2 = Helper.CreateVehicle(driver1, "TR2");
			var vehicle3 = Helper.CreateVehicle(driver1, "TR3");
			var vehicle4 = Helper.CreateVehicle(driver1, "TR4");

			vehicle1.RQ_RegState = "QLD";
			vehicle2.RQ_RegState = "VIC";
			vehicle3.RQ_RegState = "NSW";
			vehicle4.RQ_RegState = "NSW";

			Asserter.AddToScope(vehicle1);
			Asserter.AddToScope(vehicle2);
			Asserter.AddToScope(vehicle3);
			Asserter.AddToScope(vehicle4);

			Factory.Save();

			var filterBizO = GetNewFilterStripBusinessObject();
			var stateFilter = (ModuleTextFilter)filterBizO[DtbVehicleFilterBusinessObject.FilterConstants.State];
			AssertEquals(DtbVehicleFilterBusinessObject.FilterConstants.VehicleCategory, stateFilter.Category);

			stateFilter.IsActive = true;
			stateFilter.Property = "QLD";
			Asserter.AssertMatches("vehicle1 is registered in QLD.", filterBizO.Filter, vehicle1);

			stateFilter.Property = "VIC";
			Asserter.AssertMatches("vehicle2 is registered in VIC.", filterBizO.Filter, vehicle2);

			stateFilter.Property = "NSW";
			Asserter.AssertMatches("vehicle3, vehicle4 are registered in NSW.", filterBizO.Filter, vehicle3, vehicle4);
		}

		#endregion

		#region TestOrCategoryFilters

		public void TestOrCategoryFilters()
		{
			var driver1 = Helper.CreateDriver("BOB", "BOB");
			var driversGroup = Helper.CreateDriverGroup("Drivers", driver1);
			var transportRegistry = ObjectFactory.Get<ITransportRegistry>();
			transportRegistry.TransportDriversGroup.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, driversGroup.PK.ToGuid());

			var vehicle1 = Helper.CreateVehicle(driver1, "TRK");
			var vehicle2 = Helper.CreateVehicle(driver1, "TR2");
			var vehicle3 = Helper.CreateVehicle(driver1, "TR3");
			vehicle1.RQ_RegState = "QLD";
			vehicle2.RQ_RegState = "VIC";
			vehicle3.RQ_RegState = "NSW";
			Asserter.AddToScope(vehicle1, vehicle2, vehicle3);

			Factory.Save();

			var filterBizO = new DtbRoutePlannerFilterBusinessObject();
			var childFilterBizO = new DtbVehicleFilterBusinessObject();
			filterBizO.AddChildFilterBusinessObject(childFilterBizO);

			var stateFilter = (ModuleTextFilter)filterBizO[DtbVehicleFilterBusinessObject.FilterConstants.State];
			stateFilter.IsActive = true;
			stateFilter.Property = "QLD";
			stateFilter.OrCategory = FilterOrCategory.Red;

			var stateFilter2Description = stateFilter.Description + " (1) ";
			var stateFilter2 = new ModuleTextFilter(stateFilter2Description, RefEquipmentSchema.RQ_RegState);
			stateFilter2.Category = DtbVehicleFilterBusinessObject.FilterConstants.VehicleCategory;
			stateFilter2.IsActive = true;
			stateFilter2.Property = "VIC";
			stateFilter2.OrCategory = FilterOrCategory.Red;
			filterBizO.ModuleFilters.AddFilter(stateFilter2);

			Asserter.AssertMatches("should return both of vehicle1  and vehicle2.", childFilterBizO.Filter, vehicle2, vehicle1);

			var stateFilter3Description = stateFilter.Description + " (2) ";
			var stateFilter3 = new ModuleTextFilter(stateFilter3Description, RefEquipmentSchema.RQ_RegState);
			stateFilter3.Category = DtbVehicleFilterBusinessObject.FilterConstants.VehicleCategory;
			stateFilter3.IsActive = true;
			stateFilter3.Property = "NSW";
			stateFilter3.OrCategory = FilterOrCategory.Green;
			filterBizO.ModuleFilters.AddFilter(stateFilter3);
			Asserter.AssertMatches("should not return any vehicle.", childFilterBizO.Filter);
		}

		#endregion

		#region Implementation

		protected TransportBookingConsignmentTestHelper Helper
		{
			get { return helper ?? (helper = new TransportBookingConsignmentTestHelper(Factory)); }
		}

		TransportBookingConsignmentTestHelper helper;

		protected FilterStripAsserter<RefEquipment> Asserter
		{
			get { return asserter ?? (asserter = new FilterStripAsserter<RefEquipment>(Factory, r => r.RQ_ShortCode)); }
		}

		FilterStripAsserter<RefEquipment> asserter;

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new DtbVehicleFilterBusinessObject();
		}

		protected override Type TypeOfBusinessObjectToQuery
		{
			get { return typeof(RefEquipment); }
		}

		protected override ModuleTextFilter DuplicateFilter(string description)
		{
			var stateFilterDescription = description + " (1) ";
			return new ModuleTextFilter(stateFilterDescription, RefEquipmentSchema.RQ_RegState);
		}

		protected override FilterCategory FilterCategoryOfChildFilter
		{
			get { return DtbVehicleFilterBusinessObject.FilterConstants.VehicleCategory; }
		}

		protected override string ChildFilterName
		{
			get { return DtbVehicleFilterBusinessObject.FilterConstants.State; }
		}

		protected override DtbChildFilterBusinessObject GetNewChildFilterBusinessObject
		{
			get { return (DtbChildFilterBusinessObject)GetNewFilterStripBusinessObject(); }
		}

		protected override SchemaColumn ExpectedFieldOnRunsheet
		{
			get { return DtbConsignmentRunSheetSchema.KG_RQ_Truck; }
		}

		protected override SchemaColumn ExpectedChildBizOPKOrNK
		{
			get { return RefEquipmentSchema.PK; }
		}

		#endregion
	}
}
