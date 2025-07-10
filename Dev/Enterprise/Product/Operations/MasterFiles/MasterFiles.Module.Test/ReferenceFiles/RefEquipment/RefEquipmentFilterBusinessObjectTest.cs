using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(RefEquipmentFilterBusinessObject))]
	sealed class RefEquipmentFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		#region CheckboxFiltersTests

		public void TestStatusVehicleFilter()
		{
			RefEquipment isVehicleEquipment = Factory.NewWithValidTestData<RefEquipment>();
			RefEquipment isNotVehicleEquipment = Factory.NewWithValidTestData<RefEquipment>();
			isNotVehicleEquipment.RQ_IsVehicle = false;

			RefEquipmentFilterBusinessObject filter = new RefEquipmentFilterBusinessObject();
			((ModuleTextFilter)filter["Vehicle Status"]).IsActive = true;
			AssertEquals("Default value", "Is a Vehicle", ((ModuleTextFilter)filter["Vehicle Status"]).Property);

			RefEquipmentCollection equipments = new RefEquipmentCollection(Factory);
			equipments.AdditionalFilter = filter.Filter;
			AssertCollectionContains(isVehicleEquipment, equipments);
			AssertCollectionNotContains(isNotVehicleEquipment, equipments);

			((ModuleTextFilter)filter["Vehicle Status"]).Property = "Is not a Vehicle";
			((ModuleTextFilter)filter["Vehicle Status"]).IsActive = true;

			equipments.AdditionalFilter = filter.Filter;
			AssertCollectionNotContains(isVehicleEquipment, equipments);
			AssertCollectionContains(isNotVehicleEquipment, equipments);

			((ModuleTextFilter)filter["Vehicle Status"]).Property = "All";
			((ModuleTextFilter)filter["Vehicle Status"]).IsActive = true;

			equipments.AdditionalFilter = filter.Filter;
			AssertCollectionContains(isVehicleEquipment, equipments);
			AssertCollectionContains(isNotVehicleEquipment, equipments);
		}

		#endregion

		#region TextFiltersTests

		public void TestDriverFilter()
		{
			RefEquipment truck1 = Factory.NewWithValidTestData<RefEquipment>();
			RefEquipment truck2 = Factory.NewWithValidTestData<RefEquipment>();
			RefEquipment truck3 = Factory.NewWithValidTestData<RefEquipment>();

			truck1.RQ_ShortCode = "tr1";
			truck2.RQ_ShortCode = "tr2";
			truck3.RQ_ShortCode = "tr3";

			GlbStaff driver1 = Factory.New<GlbStaff>();
			driver1.GS_Code = "dr1";
			driver1.GS_LoginName = "dr1";
			GlbStaff driver2 = Factory.New<GlbStaff>();
			driver2.GS_Code = "dr2";
			driver2.GS_LoginName = "dr2";

			truck1.RQ_GS_NKPreferredDriver = "dr1";
			truck2.RQ_GS_NKPreferredDriver = "dr2";
			truck3.RQ_GS_NKPreferredDriver = "dr2";

			Factory.Save();

			RefEquipmentFilterBusinessObject filter = new RefEquipmentFilterBusinessObject();
			((ModuleTextFilter)filter["Driver"]).Property = "dr1";
			((ModuleTextFilter)filter["Driver"]).IsActive = true;

			RefEquipmentCollection equipments = new RefEquipmentCollection(Factory);
			equipments.AdditionalFilter = filter.Filter;

			AssertCollectionContains(truck1, equipments);
			AssertCollectionNotContains(truck2, equipments);
			AssertCollectionNotContains(truck3, equipments);

			filter = new RefEquipmentFilterBusinessObject();
			((ModuleTextFilter)filter["Driver"]).Property = "dr2";
			((ModuleTextFilter)filter["Driver"]).IsActive = true;

			equipments = new RefEquipmentCollection(Factory);
			equipments.AdditionalFilter = filter.Filter;

			AssertCollectionNotContains(truck1, equipments);
			AssertCollectionContains(truck2, equipments);
			AssertCollectionContains(truck3, equipments);
		}

		public void TestEquipmentDescriptionFilter()
		{
			RefEquipment equipmentDescription1 = Factory.NewWithValidTestData<RefEquipment>();
			RefEquipment equipmentDescription2 = Factory.NewWithValidTestData<RefEquipment>();
			equipmentDescription1.RQ_Description = "Indescriptive";
			equipmentDescription2.RQ_Description = "SomeDescription";

			Factory.Save();

			RefEquipmentFilterBusinessObject filter = new RefEquipmentFilterBusinessObject();
			((ModuleTextFilter)filter["Description"]).Property = "Indescriptive";
			((ModuleTextFilter)filter["Description"]).IsActive = true;

			RefEquipmentCollection equipments = new RefEquipmentCollection(Factory);
			equipments.AdditionalFilter = filter.Filter;

			AssertCollectionContains(equipmentDescription1, equipments);
			AssertCollectionNotContains(equipmentDescription2, equipments);
		}

		public void TestEquipmentShortCodeFilter()
		{
			RefEquipment equipmentShortCode1 = Factory.NewWithValidTestData<RefEquipment>();
			RefEquipment equipmentShortCode2 = Factory.NewWithValidTestData<RefEquipment>();
			equipmentShortCode1.RQ_ShortCode = "Indescri";
			equipmentShortCode2.RQ_ShortCode = "SomeDescri";

			Factory.Save();

			RefEquipmentFilterBusinessObject filter = new RefEquipmentFilterBusinessObject();
			((ModuleTextFilter)filter["Short Code"]).Property = "Indescri";
			((ModuleTextFilter)filter["Short Code"]).IsActive = true;

			RefEquipmentCollection equipments = new RefEquipmentCollection(Factory);
			equipments.AdditionalFilter = filter.Filter;

			AssertCollectionContains(equipmentShortCode1, equipments);
			AssertCollectionNotContains(equipmentShortCode2, equipments);
		}

		public void TestEquipmentGroupFilter()
		{
			RefEquipment equipmentGroup1 = Factory.NewWithValidTestData<RefEquipment>();
			RefEquipment equipmentGroup2 = Factory.NewWithValidTestData<RefEquipment>();
			equipmentGroup1.RQ_EquipmentGroup = "Ind";
			equipmentGroup2.RQ_EquipmentGroup = "Som";

			Factory.Save();

			RefEquipmentFilterBusinessObject filter = new RefEquipmentFilterBusinessObject();
			((ModuleTextFilter)filter["Equipment Group"]).Property = "Ind";
			((ModuleTextFilter)filter["Equipment Group"]).IsActive = true;

			RefEquipmentCollection equipments = new RefEquipmentCollection(Factory);
			equipments.AdditionalFilter = filter.Filter;

			AssertCollectionContains(equipmentGroup1, equipments);
			AssertCollectionNotContains(equipmentGroup2, equipments);
		}

		public void TestEquipmentTypeFilter()
		{
			RefContainer container1 = Factory.New<RefContainer>();
			container1.RC_Code = "XYZA";

			RefContainer container2 = Factory.New<RefContainer>();
			container2.RC_Code = "1234";

			RefEquipment equipmentType1 = Factory.NewWithValidTestData<RefEquipment>();
			RefEquipment equipmentType2 = Factory.NewWithValidTestData<RefEquipment>();
			equipmentType1.RQ_RC_RoadContainerType = container1.PK;
			equipmentType2.RQ_RC_RoadContainerType = container2.PK;

			Factory.Save();

			RefEquipmentFilterBusinessObject filter = new RefEquipmentFilterBusinessObject();
			((ModuleGuidFilter)filter["Equipment Type"]).Property = container1.PK;
			((ModuleGuidFilter)filter["Equipment Type"]).IsActive = true;

			RefEquipmentCollection equipments = new RefEquipmentCollection(Factory);
			equipments.AdditionalFilter = filter.Filter;

			AssertCollectionContains(equipmentType1, equipments);
			AssertCollectionNotContains(equipmentType2, equipments);
		}

		#endregion

		#region Implementation

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new RefEquipmentFilterBusinessObject();
		}

		#endregion
	}
}
