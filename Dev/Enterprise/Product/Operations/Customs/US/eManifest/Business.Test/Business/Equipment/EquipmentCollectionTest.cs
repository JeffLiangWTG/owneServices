using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.eManifest.Business.Testing
{
	[TestedType(typeof(EquipmentCollection))]
	sealed class EquipmentCollectionTest : ActiveBusinessObjectCollectionTestCase<EquipmentCollection>
	{
		public void TestCollection()
		{
			var trip = Factory.New<Trip>();
			var equipment = trip.Equipment.AddNew();
			equipment.FillWithValidTestData();
			equipment.BJ_EmptyIITsCoveredByCarrier = true;
			AssertEquals("BJ_BH_Header", trip.PK, equipment.BJ_BH_Header);
			AssertEquals("BJ_IsConveyance", false, equipment.BJ_IsConveyance);
			var conveyance = Factory.NewWithValidTestData<Equipment>();
			conveyance.BJ_BH_Header = trip.PK;
			conveyance.BJ_IsConveyance = true;
			Factory.Save();
			trip = new BusinessObjectFactory().Load<Trip>(trip.PK);
			AssertEquals("Equipment.Count", 1, trip.Equipment.Count);
			AssertNotEquals("Conveyance should not be loaded into collection", conveyance.PK, trip.Equipment[0].PK);
			AssertEquals("BJ_EmptyIITsCoveredByCarrier", true, trip.Equipment[0].BJ_EmptyIITsCoveredByCarrier);
		}

		public void Test_AddAllCommoditiesEquipment_WhenAddEquipment()
		{
			var trip = Factory.New<Trip>();

			var shipment = trip.Shipments.AddNew();
			var commodity = Factory.New<Commodity>();
			shipment.Commodities.Add(commodity);

			Assert("Commodity has not been given a default equiment value", commodity.BY_BJ_Equipment.IsEmpty);

			var equipment1 = trip.AllEquipmentIncludingMainConveyance.AddNew();
			var equipment2 = trip.AllEquipmentIncludingMainConveyance.AddNew();
			var equipment3 = trip.AllEquipmentIncludingMainConveyance.AddNew();

			AssertEquals(equipment1.Trip, trip);
			AssertEquals(equipment2.Trip, trip);
			AssertEquals(equipment3.Trip, trip);
			
			Assert("Commodity has been set a default equiment value", !commodity.BY_BJ_Equipment.IsEmpty);
			AssertEquals("Equipment of the commodity has been set to first equipment, that is equiment1", commodity.BY_BJ_Equipment, equipment1.PK);

			AssertEquals(equipment3.Trip.Equipment.Count, 3);
			equipment3.BJ_IsConveyance = true;
			AssertEquals("Equipment of the commodity has been changed into the first conveyance, that is equipment3", commodity.BY_BJ_Equipment, equipment3.PK);

			equipment2.BJ_IsConveyance = true;
			AssertEquals("Equipment of the commodity is still equipment3", commodity.BY_BJ_Equipment, equipment3.PK);
		}

		public void Test_DeleteAllCommoditiesEquipment_WhenDeleteEquipment()
		{
			var trip = Factory.New<Trip>();
			var shipment = trip.Shipments.AddNew();
			var commodity = Factory.New<Commodity>();
			shipment.Commodities.Add(commodity);
			Assert("Commodity has not been given a default equiment value", commodity.BY_BJ_Equipment.IsEmpty);

			var equipment1 = trip.AllEquipmentIncludingMainConveyance.AddNew();
			var equipment2 = trip.AllEquipmentIncludingMainConveyance.AddNew();

			equipment1.BJ_IsConveyance = false;
			equipment2.BJ_IsConveyance = false;

			Assert("Commodity has been set a default equiment value", !commodity.BY_BJ_Equipment.IsEmpty);
			AssertEquals("Equipment of the commodity has been set to first equipment, that is equiment1", commodity.BY_BJ_Equipment, equipment1.PK);

			trip.Equipment.Delete(equipment1);
			Assert("Equipment of the commodity is still empty", commodity.BY_BJ_Equipment.IsEmpty);

			trip.Equipment.Delete(equipment2);
			Assert("Equipment of the commodity is still empty", commodity.BY_BJ_Equipment.IsEmpty);
		}

		protected override EquipmentCollection GetCollectionToTest() => new EquipmentCollection(Factory.New<Trip>(), true);
	}
}
