using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.Business;
using Enterprise.Freight.LocalCartage.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Freight.LocalCartage.Module.Testing
{
	[TestedType(typeof(CartageWorkSheetFilterStripBusinessObject))]
	public class CartageWorkSheetFilterStripBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		public void TestRunSheetNo()
		{
			CommonWorkSheet runSheet1 = Factory.New<CommonWorkSheet>();
			CommonWorkSheet runSheet2 = Factory.New<CommonWorkSheet>();
			CommonWorkSheet runSheet3 = Factory.New<CommonWorkSheet>();
			runSheet1.EY_RunSheetNumber = "RS10001001";
			runSheet2.EY_RunSheetNumber = "RS10001002";
			runSheet3.EY_RunSheetNumber = "T10001001";
			Factory.Save();
			CartageWorkSheetFilterStripBusinessObject filter = new CartageWorkSheetFilterStripBusinessObject();
			((ModuleTextFilter)filter["Run Sheet #"]).Property = "RS10001001";
			((ModuleTextFilter)filter["Run Sheet #"]).IsActive = true;
			CommonWorkSheetCollection collection = new CommonWorkSheetCollection(Factory);
			collection.AdditionalFilter = filter.Filter;
			AssertEquals("Should have 1 CommonWorkSheet", 1, collection.Count);
			AssertCollectionContains("Should have runSheet1", runSheet1, collection);
			AssertCollectionNotContains("Should not have runSheet2", runSheet2, collection);
			AssertCollectionNotContains("Should not have runSheet3", runSheet3, collection);
		}

		public void TestContainerNo()
		{
			CommonCartageBehaviorStrategyProvider.SetProvider(Factory, new CartageBehaviorStrategyProvider());
			CommonWorkSheet runSheet1 = Factory.New<CommonWorkSheet>();
			CommonWorkSheet runSheet2 = Factory.New<CommonWorkSheet>();
			CommonWorkSheet runSheet3 = Factory.New<CommonWorkSheet>();
			CommonCartage cartage = Factory.New<CommonCartage>();
			cartage.JJ_E3_NKJobType = Constants.CartageJobType.NEW_FCLImportUnpack;
			CommonBookedCtgMove move1 = cartage.ContainerBookedMoves.AddNew();
			CommonBookedCtgMove move2 = cartage.ContainerBookedMoves.AddNew();
			CommonBookedCtgMove move3 = cartage.LooseBookedMoves.AddNew();
			CommonContainer container1 = move1.Container;
			CommonContainer container2 = move2.Container;
			CommonCartageLeg leg1 = move1.CartageLegs[0];
			CommonCartageLeg leg2 = move2.CartageLegs[0];
			CommonCartageLeg leg3 = move3.CartageLegs.AddNew();
			container1.JC_ContainerNum = "Container1";
			container2.JC_ContainerNum = "Container2";
			leg1.JU_EY_RunSheet = runSheet1.PK;
			leg2.JU_EY_RunSheet = runSheet2.PK;
			leg3.JU_EY_RunSheet = runSheet3.PK;
			Factory.Save();
			CartageWorkSheetFilterStripBusinessObject filter = new CartageWorkSheetFilterStripBusinessObject();
			((ModuleTextFilter)filter[NumberFilterTypes.ContainerNumber]).Property = "Container1";
			((ModuleTextFilter)filter[NumberFilterTypes.ContainerNumber]).IsActive = true;
			CommonWorkSheetCollection collection = new CommonWorkSheetCollection(Factory);
			collection.AdditionalFilter = filter.Filter;
			AssertEquals("Should have 1 CommonWorkSheet", 1, collection.Count);
			AssertCollectionContains("Should have runSheet1", runSheet1, collection);
			AssertCollectionNotContains("Should not have runSheet2", runSheet2, collection);
			AssertCollectionNotContains("Should not have runSheet3", runSheet3, collection);
			((ModuleTextFilter)filter[NumberFilterTypes.ContainerNumber]).Property = "Container2";
			collection.AdditionalFilter = filter.Filter;
			AssertEquals("Should have 1 CommonWorkSheet", 1, collection.Count);
			AssertCollectionNotContains("Should not have runSheet1", runSheet1, collection);
			AssertCollectionContains("Should have runSheet2", runSheet2, collection);
			AssertCollectionNotContains("Should not have runSheet3", runSheet3, collection);
			((ModuleTextFilter)filter[NumberFilterTypes.ContainerNumber]).Property = "Container";
			collection.AdditionalFilter = filter.Filter;
			AssertEquals("Should have 2 CommonWorkSheet", 2, collection.Count);
			AssertCollectionContains("Should have runSheet1", runSheet1, collection);
			AssertCollectionContains("Should have runSheet2", runSheet2, collection);
			AssertCollectionNotContains("Should not have runSheet3", runSheet3, collection);
		}

		public void TestStartTime()
		{
			CommonWorkSheet runSheet1 = Factory.New<CommonWorkSheet>();
			CommonWorkSheet runSheet2 = Factory.New<CommonWorkSheet>();
			CommonWorkSheet runSheet3 = Factory.New<CommonWorkSheet>();
			ZDateTime now = ZDateTime.Now;
			runSheet1.EY_StartTime = now.AddDays(5);
			runSheet2.EY_StartTime = now.AddDays(7);
			runSheet3.EY_StartTime = now.AddDays(9);
			Factory.Save();
			CartageWorkSheetFilterStripBusinessObject filter = new CartageWorkSheetFilterStripBusinessObject();
			((ModuleDateFilter)filter["Start Time"]).Property1 = now.AddDays(6);
			((ModuleDateFilter)filter["Start Time"]).Property2 = now.AddDays(8);
			((ModuleDateFilter)filter["Start Time"]).PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			((ModuleDateFilter)filter["Start Time"]).IsActive = true;
			CommonWorkSheetCollection collection = new CommonWorkSheetCollection(Factory);
			collection.AdditionalFilter = filter.Filter;
			AssertEquals("Should have 1 CommonWorkSheet", 1, collection.Count);
			AssertCollectionNotContains("Should not have runSheet1", runSheet1, collection);
			AssertCollectionContains("Should have runSheet2", runSheet2, collection);
			AssertCollectionNotContains("Should not have runSheet3", runSheet3, collection);
		}

		public void TestEndTime()
		{
			CommonWorkSheet runSheet1 = Factory.New<CommonWorkSheet>();
			CommonWorkSheet runSheet2 = Factory.New<CommonWorkSheet>();
			CommonWorkSheet runSheet3 = Factory.New<CommonWorkSheet>();
			ZDateTime now = ZDateTime.Now;
			runSheet1.EY_EndTime = now.AddDays(5);
			runSheet2.EY_EndTime = now.AddDays(7);
			runSheet3.EY_EndTime = now.AddDays(9);
			Factory.Save();
			CartageWorkSheetFilterStripBusinessObject filter = new CartageWorkSheetFilterStripBusinessObject();
			((ModuleDateFilter)filter["End Time"]).Property1 = now.AddDays(6);
			((ModuleDateFilter)filter["End Time"]).Property2 = now.AddDays(8);
			((ModuleDateFilter)filter["End Time"]).PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			((ModuleDateFilter)filter["End Time"]).IsActive = true;
			CommonWorkSheetCollection collection = new CommonWorkSheetCollection(Factory);
			collection.AdditionalFilter = filter.Filter;
			AssertEquals("Should have 1 CommonWorkSheet", 1, collection.Count);
			AssertCollectionNotContains("Should not have runSheet1", runSheet1, collection);
			AssertCollectionContains("Should have runSheet2", runSheet2, collection);
			AssertCollectionNotContains("Should not have runSheet3", runSheet3, collection);
		}

		public void TestDriversName()
		{
			CommonWorkSheet runSheet1 = Factory.New<CommonWorkSheet>();
			CommonWorkSheet runSheet2 = Factory.New<CommonWorkSheet>();
			CommonWorkSheet runSheet3 = Factory.New<CommonWorkSheet>();
			runSheet1.EY_DriversName = "Bob David";
			runSheet2.EY_DriversName = "Bob Bob";
			runSheet3.EY_DriversName = "David Bob";
			Factory.Save();
			CartageWorkSheetFilterStripBusinessObject filter = new CartageWorkSheetFilterStripBusinessObject();
			((ModuleTextFilter)filter["Drivers Name"]).Property = "Bob Bob";
			((ModuleTextFilter)filter["Drivers Name"]).IsActive = true;
			CommonWorkSheetCollection collection = new CommonWorkSheetCollection(Factory);
			collection.AdditionalFilter = filter.Filter;
			AssertEquals("Should have 1 CommonWorkSheet", 1, collection.Count);
			AssertCollectionNotContains("Should not have runSheet1", runSheet1, collection);
			AssertCollectionContains("Should have runSheet2", runSheet2, collection);
			AssertCollectionNotContains("Should not have runSheet3", runSheet3, collection);
		}

		public void TestDriversLicence()
		{
			CommonWorkSheet runSheet1 = Factory.New<CommonWorkSheet>();
			CommonWorkSheet runSheet2 = Factory.New<CommonWorkSheet>();
			CommonWorkSheet runSheet3 = Factory.New<CommonWorkSheet>();
			runSheet1.EY_DriversLicence = "1234567";
			runSheet2.EY_DriversLicence = "7654321";
			runSheet3.EY_DriversLicence = "9999991";
			Factory.Save();
			CartageWorkSheetFilterStripBusinessObject filter = new CartageWorkSheetFilterStripBusinessObject();
			((ModuleTextFilter)filter["Drivers License"]).Property = "7654321";
			((ModuleTextFilter)filter["Drivers License"]).IsActive = true;
			CommonWorkSheetCollection collection = new CommonWorkSheetCollection(Factory);
			collection.AdditionalFilter = filter.Filter;
			AssertEquals("Should have 1 CommonWorkSheet", 1, collection.Count);
			AssertCollectionNotContains("Should not have runSheet1", runSheet1, collection);
			AssertCollectionContains("Should have runSheet2", runSheet2, collection);
			AssertCollectionNotContains("Should not have runSheet3", runSheet3, collection);
		}

		public void TestTransportCompanyName_Blank_NotBlank()
		{
			OrgHeader header = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, SQLComparisonOperator.StartsWith, "A"));
			CommonWorkSheet runSheet1 = Factory.New<CommonWorkSheet>();
			CommonWorkSheet runSheet2 = Factory.New<CommonWorkSheet>();
			CommonWorkSheet runSheet3 = Factory.New<CommonWorkSheet>();
			runSheet1.EY_TransportCoName = "Davids Transport Company";
			runSheet2.EY_OH_TransportCo = header.PK;
			Factory.Save();
			CartageWorkSheetFilterStripBusinessObject filter = new CartageWorkSheetFilterStripBusinessObject();
			((ModuleTextFilter)filter["Transport Company Name"]).SqlComparisonOperator = SpecialComparisonOperator.IsNotBlank;
			((ModuleTextFilter)filter["Transport Company Name"]).Property = "' '";
			((ModuleTextFilter)filter["Transport Company Name"]).IsActive = true;
			CommonWorkSheetCollection collection = new CommonWorkSheetCollection(Factory);
			collection.AdditionalFilter = filter.Filter;
			AssertEquals("Should have 1 CommonWorkSheet", 2, collection.Count);
			AssertCollectionContains("Should have runSheet1", runSheet1, collection);
			AssertCollectionContains("Should have runSheet2", runSheet2, collection);
			AssertCollectionNotContains("Should not have runSheet3", runSheet3, collection);
			((ModuleTextFilter)filter["Transport Company Name"]).SqlComparisonOperator = SpecialComparisonOperator.IsBlank;
			collection = new CommonWorkSheetCollection(Factory);
			collection.AdditionalFilter = filter.Filter;
			AssertCollectionNotContains("Should not have runSheet1", runSheet1, collection);
			AssertCollectionNotContains("Should not have runSheet2", runSheet2, collection);
			AssertCollectionContains("Should have runSheet3", runSheet3, collection);
		}

		public void TestTransportCompanyName()
		{
			CommonWorkSheet runSheet1 = Factory.New<CommonWorkSheet>();
			CommonWorkSheet runSheet2 = Factory.New<CommonWorkSheet>();
			CommonWorkSheet runSheet3 = Factory.New<CommonWorkSheet>();
			runSheet1.EY_TransportCoName = "Davids Transport Company";
			runSheet2.EY_TransportCoName = "Bobs Transport Company";
			runSheet3.EY_TransportCoName = "Brians Transport Company";
			Factory.Save();
			CartageWorkSheetFilterStripBusinessObject filter = new CartageWorkSheetFilterStripBusinessObject();
			((ModuleTextFilter)filter["Transport Company Name"]).Property = "Bobs Transport Company";
			((ModuleTextFilter)filter["Transport Company Name"]).IsActive = true;
			CommonWorkSheetCollection collection = new CommonWorkSheetCollection(Factory);
			collection.AdditionalFilter = filter.Filter;
			AssertEquals("Should have 1 CommonWorkSheet", 1, collection.Count);
			AssertCollectionNotContains("Should not have runSheet1", runSheet1, collection);
			AssertCollectionContains("Should have runSheet2", runSheet2, collection);
			AssertCollectionNotContains("Should not have runSheet3", runSheet3, collection);
			OrgHeader header = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, SQLComparisonOperator.StartsWith, "A"));
			runSheet1.EY_OH_TransportCo = header.PK;
			runSheet2.EY_TransportCoName = header.OH_FullName;
			Factory.Save();
			((ModuleTextFilter)filter["Transport Company Name"]).Property = header.OH_FullName;
			((ModuleTextFilter)filter["Transport Company Name"]).IsActive = true;
			collection = new CommonWorkSheetCollection(Factory);
			collection.AdditionalFilter = filter.Filter;
			AssertEquals("Should have 1 CommonWorkSheet", 2, collection.Count);
			AssertCollectionContains("Should not have runSheet1", runSheet1, collection);
			AssertCollectionContains("Should have runSheet2", runSheet2, collection);
			AssertCollectionNotContains("Should not have runSheet3", runSheet3, collection);
		}

		public void TestVehicleRegistration()
		{
			CommonWorkSheet runSheet1 = Factory.New<CommonWorkSheet>();
			CommonWorkSheet runSheet2 = Factory.New<CommonWorkSheet>();
			CommonWorkSheet runSheet3 = Factory.New<CommonWorkSheet>();
			runSheet1.EY_TruckRegistration = "1234567";
			runSheet2.EY_TruckRegistration = "7654321";
			runSheet3.EY_TruckRegistration = "9999991";
			Factory.Save();
			CartageWorkSheetFilterStripBusinessObject filter = new CartageWorkSheetFilterStripBusinessObject();
			((ModuleTextFilter)filter["Vehicle Registration"]).Property = "7654321";
			((ModuleTextFilter)filter["Vehicle Registration"]).IsActive = true;
			CommonWorkSheetCollection collection = new CommonWorkSheetCollection(Factory);
			collection.AdditionalFilter = filter.Filter;
			AssertEquals("Should have 1 CommonWorkSheet", 1, collection.Count);
			AssertCollectionNotContains("Should not have runSheet1", runSheet1, collection);
			AssertCollectionContains("Should have runSheet2", runSheet2, collection);
			AssertCollectionNotContains("Should not have runSheet3", runSheet3, collection);
		}

		public void TestStaffDriver()
		{
			CommonWorkSheet runSheet1 = Factory.New<CommonWorkSheet>();
			CommonWorkSheet runSheet2 = Factory.New<CommonWorkSheet>();
			CommonWorkSheet runSheet3 = Factory.New<CommonWorkSheet>();
			GlbStaff bob = Factory.New<GlbStaff>();
			bob.GS_Code = "BB1";
			bob.GS_LoginName = "Bob111";
			GlbStaff david = Factory.New<GlbStaff>();
			david.GS_Code = "DD2";
			david.GS_LoginName = "David222";
			GlbStaff brian = Factory.New<GlbStaff>();
			brian.GS_Code = "BN3";
			brian.GS_LoginName = "Brian333";
			runSheet1.EY_GS_NKTruckDriver = bob.GS_Code;
			runSheet2.EY_GS_NKTruckDriver = david.GS_Code;
			runSheet3.EY_GS_NKTruckDriver = brian.GS_Code;
			Factory.Save();
			CartageWorkSheetFilterStripBusinessObject filter = new CartageWorkSheetFilterStripBusinessObject();
			((ModuleNkFilter)filter["Staff Driver"]).Property = david.GS_Code;
			((ModuleNkFilter)filter["Staff Driver"]).IsActive = true;
			CommonWorkSheetCollection collection = new CommonWorkSheetCollection(Factory);
			collection.AdditionalFilter = filter.Filter;
			AssertEquals("Should have 1 CommonWorkSheet", 1, collection.Count);
			AssertCollectionNotContains("Should not have runSheet1", runSheet1, collection);
			AssertCollectionContains("Should have runSheet2", runSheet2, collection);
			AssertCollectionNotContains("Should not have runSheet3", runSheet3, collection);
		}

		public void TestTransportCompany()
		{
			var runSheet1 = Factory.New<CommonWorkSheet>();
			var runSheet2 = Factory.New<CommonWorkSheet>();
			var runSheet3 = Factory.New<CommonWorkSheet>();
			var org1 = Factory.New<OrgHeader>();
			org1.OH_Code = "org1";
			org1.MainAddress.OA_Address1 = "org1 Address";
			var org2 = Factory.New<OrgHeader>();
			org2.OH_Code = "org2";
			org2.MainAddress.OA_Address1 = "org2 Address";
			var org3 = Factory.New<OrgHeader>();
			org3.OH_Code = "org3";
			org3.MainAddress.OA_Address1 = "org3 Address";
			runSheet1.EY_OH_TransportCo = org1.PK;
			runSheet2.EY_OH_TransportCo = org2.PK;
			runSheet3.EY_OH_TransportCo = org3.PK;
			Factory.Save();
			var filter = new CartageWorkSheetFilterStripBusinessObject();
			var transportCoFilter = ((ModuleGuidFilter)filter["Transport Company"]);
			transportCoFilter.Property = org2.PK;
			transportCoFilter.IsActive = true;
			AssertEquals("Should be of type LocalTransportCollection.", typeof(LocalTransportCollection), transportCoFilter.List.GetType());
			var collection = new CommonWorkSheetCollection(Factory);
			collection.AdditionalFilter = filter.Filter;
			AssertEquals("Should have 1 CommonWorkSheet", 1, collection.Count);
			AssertCollectionNotContains("Should not have runSheet1", runSheet1, collection);
			AssertCollectionContains("Should have runSheet2", runSheet2, collection);
			AssertCollectionNotContains("Should not have runSheet3", runSheet3, collection);
		}

		public void TestVehicle()
		{
			CommonWorkSheet runSheet1 = Factory.New<CommonWorkSheet>();
			CommonWorkSheet runSheet2 = Factory.New<CommonWorkSheet>();
			CommonWorkSheet runSheet3 = Factory.New<CommonWorkSheet>();
			RefEquipment car = Factory.New<RefEquipment>();
			car.RQ_IsVehicle = true;
			car.RQ_ShortCode = "Car1";
			car.RQ_Registration = "111";
			RefEquipment truck = Factory.New<RefEquipment>();
			truck.RQ_IsVehicle = true;
			truck.RQ_ShortCode = "Truck2";
			truck.RQ_Registration = "222";
			RefEquipment semi = Factory.New<RefEquipment>();
			semi.RQ_IsVehicle = true;
			semi.RQ_ShortCode = "Semi3";
			semi.RQ_Registration = "333";
			runSheet1.EY_RQ_Truck = car.PK;
			runSheet2.EY_RQ_Truck = truck.PK;
			runSheet3.EY_RQ_Truck = semi.PK;
			Factory.Save();
			CartageWorkSheetFilterStripBusinessObject filter = new CartageWorkSheetFilterStripBusinessObject();
			((ModuleGuidFilter)filter["Vehicle"]).Property = truck.PK;
			((ModuleGuidFilter)filter["Vehicle"]).IsActive = true;
			CommonWorkSheetCollection collection = new CommonWorkSheetCollection(Factory);
			collection.AdditionalFilter = filter.Filter;
			AssertEquals("Should have 1 CommonWorkSheet", 1, collection.Count);
			AssertCollectionNotContains("Should not have runSheet1", runSheet1, collection);
			AssertCollectionContains("Should have runSheet2", runSheet2, collection);
			AssertCollectionNotContains("Should not have runSheet3", runSheet3, collection);
		}

		public void TestFilterMaxLength()
		{
			var filterBizO = new CartageWorkSheetFilterStripBusinessObject();
			CombineAssertions(() =>
			{
				AssertEquals("MaxLength of Transport Company Name should be set correctly.", Math.Min(JobCartageRunSheetSchema.EY_TransportCoName.MaxLength, OrgHeaderSchema.OH_FullName.MaxLength), filterBizO["Transport Company Name"].MaxLength);
				AssertEquals("MaxLength of Container # should be set correctly.", ModuleNumberFilter.MultiplyMaxLength(JobContainerSchema.JC_ContainerNum.MaxLength), filterBizO["Container #"].MaxLength);
			});
		}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new CartageWorkSheetFilterStripBusinessObject();
		}
	}
}
