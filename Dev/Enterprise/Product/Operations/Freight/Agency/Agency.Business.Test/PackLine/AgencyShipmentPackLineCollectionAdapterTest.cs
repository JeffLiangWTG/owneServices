using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Agency.Business.Testing
{
	[TestedType(typeof(AgencyShipmentPackLineCollectionAdapter))]
	internal class AgencyShipmentPackLineCollectionAdapterTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var agencyShipment = Factory.New<AgencyShipment>();
			return new AgencyShipmentPackLineCollectionAdapter(agencyShipment.BookedContainers);
		}

		public void TestRemoveAndDelete()
		{
			var agencyShipment = Factory.New<AgencyShipment>();
			var container = agencyShipment.BookedContainers.AddNew();
			var collection = new AgencyShipmentPackLineCollectionAdapter(agencyShipment.BookedContainers);
			AssertEquals("prerequisite", 1, collection.Count);
			var adapter = collection[0];
			collection.RemoveAndDelete(adapter);
			AssertEquals(0, collection.Count);
			AssertEquals(0, agencyShipment.BookedContainers.Count);
			AssertEquals(true, adapter.IsDeleted);
			AssertEquals(true, container.IsDeleted);
		}

		public void TestRemove()
		{
			var agencyShipment = Factory.New<AgencyShipment>();
			var container = agencyShipment.BookedContainers.AddNew();
			var collection = new AgencyShipmentPackLineCollectionAdapter(agencyShipment.BookedContainers);
			AssertEquals("prerequisite", 1, collection.Count);
			var adapter = collection[0];
			collection.Remove(adapter);
			AssertEquals(0, collection.Count);
			AssertEquals(0, agencyShipment.BookedContainers.Count);
			AssertEquals(false, adapter.IsDeleted);
			AssertEquals(false, container.IsDeleted);
		}
	}
}
