using System.Collections;
using System.ComponentModel;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Warehouse.Environment.Business.Testing
{
	public class LocationComparerTest : WhsTestCaseWithFactoryEnv
	{
		public void TestLocationComparer()
		{
			var warehouse = Helper.CreateWarehouse("1");
			Helper.CreateRowAndGenerateLocations(warehouse, "A", 10, 10, 10);
			Factory.Save();

			var dummy1 = Factory.New<DummyWithLocation>();
			dummy1.Location = warehouse.FindLocation("A-1-1-1");
			var dummy2 = Factory.New<DummyWithLocation>();
			dummy2.Location = warehouse.FindLocation("A-1-1-2");
			var dummy3 = Factory.New<DummyWithLocation>();
			dummy3.Location = warehouse.FindLocation("A-1-1-10");
			var dummy4 = Factory.New<DummyWithLocation>();
			dummy4.Location = warehouse.FindLocation("A-1-2-1");
			var dummy5 = Factory.New<DummyWithLocation>();
			dummy5.Location = warehouse.FindLocation("A-1-10-1");
			var dummy6 = Factory.New<DummyWithLocation>();
			dummy6.Location = warehouse.FindLocation("A-2-1-1");
			var dummy7 = Factory.New<DummyWithLocation>();
			dummy7.Location = warehouse.FindLocation("A-10-1-1");

			var dummyWithLocationCollection = new DummyWithLocationCollection(Factory);
			dummyWithLocationCollection.ApplySort(nameof(DummyWithLocation.Location), ListSortDirection.Ascending);
			AssertEquals("A-1-1-1", dummyWithLocationCollection[0].Location.ToLocationString());
			AssertEquals("A-1-1-2", dummyWithLocationCollection[1].Location.ToLocationString());
			AssertEquals("A-1-1-10", dummyWithLocationCollection[2].Location.ToLocationString());
			AssertEquals("A-1-2-1", dummyWithLocationCollection[3].Location.ToLocationString());
			AssertEquals("A-1-10-1", dummyWithLocationCollection[4].Location.ToLocationString());
			AssertEquals("A-2-1-1", dummyWithLocationCollection[5].Location.ToLocationString());
			AssertEquals("A-10-1-1", dummyWithLocationCollection[6].Location.ToLocationString());

			dummyWithLocationCollection.ApplySort(nameof(DummyWithLocation.Location), ListSortDirection.Descending);
			AssertEquals("A-10-1-1", dummyWithLocationCollection[0].Location.ToLocationString());
			AssertEquals("A-2-1-1", dummyWithLocationCollection[1].Location.ToLocationString());
			AssertEquals("A-1-10-1", dummyWithLocationCollection[2].Location.ToLocationString());
			AssertEquals("A-1-2-1", dummyWithLocationCollection[3].Location.ToLocationString());
			AssertEquals("A-1-1-10", dummyWithLocationCollection[4].Location.ToLocationString());
			AssertEquals("A-1-1-2", dummyWithLocationCollection[5].Location.ToLocationString());
			AssertEquals("A-1-1-1", dummyWithLocationCollection[6].Location.ToLocationString());
		}

		public void TestLocationComparer_FixedWidthLocation()
		{
			var warehouse = Helper.CreateFixedWidthLocationWarehouse("1", 2, 2, 2);
			Helper.CreateRowAndGenerateLocations(warehouse, "A", 10, 10, 10);
			Factory.Save();

			var dummy1 = Factory.New<DummyWithLocation>();
			dummy1.Location = warehouse.FindLocation("A010101");
			var dummy2 = Factory.New<DummyWithLocation>();
			dummy2.Location = warehouse.FindLocation("A010102");
			var dummy3 = Factory.New<DummyWithLocation>();
			dummy3.Location = warehouse.FindLocation("A010110");
			var dummy4 = Factory.New<DummyWithLocation>();
			dummy4.Location = warehouse.FindLocation("A010201");
			var dummy5 = Factory.New<DummyWithLocation>();
			dummy5.Location = warehouse.FindLocation("A011001");
			var dummy6 = Factory.New<DummyWithLocation>();
			dummy6.Location = warehouse.FindLocation("A020101");
			var dummy7 = Factory.New<DummyWithLocation>();
			dummy7.Location = warehouse.FindLocation("A100101");

			var dummyWithLocationCollection = new DummyWithLocationCollection(Factory);
			dummyWithLocationCollection.ApplySort(nameof(DummyWithLocation.Location), ListSortDirection.Ascending);
			AssertEquals("A010101", dummyWithLocationCollection[0].Location.ToLocationString());
			AssertEquals("A010102", dummyWithLocationCollection[1].Location.ToLocationString());
			AssertEquals("A010110", dummyWithLocationCollection[2].Location.ToLocationString());
			AssertEquals("A010201", dummyWithLocationCollection[3].Location.ToLocationString());
			AssertEquals("A011001", dummyWithLocationCollection[4].Location.ToLocationString());
			AssertEquals("A020101", dummyWithLocationCollection[5].Location.ToLocationString());
			AssertEquals("A100101", dummyWithLocationCollection[6].Location.ToLocationString());

			dummyWithLocationCollection.ApplySort(nameof(DummyWithLocation.Location), ListSortDirection.Descending);
			AssertEquals("A100101", dummyWithLocationCollection[0].Location.ToLocationString());
			AssertEquals("A020101", dummyWithLocationCollection[1].Location.ToLocationString());
			AssertEquals("A011001", dummyWithLocationCollection[2].Location.ToLocationString());
			AssertEquals("A010201", dummyWithLocationCollection[3].Location.ToLocationString());
			AssertEquals("A010110", dummyWithLocationCollection[4].Location.ToLocationString());
			AssertEquals("A010102", dummyWithLocationCollection[5].Location.ToLocationString());
			AssertEquals("A010101", dummyWithLocationCollection[6].Location.ToLocationString());
		}

		public void TestLocationComparer_NoLocation()
		{
			var warehouse = Helper.CreateWarehouse("1");
			Helper.CreateRowAndGenerateLocations(warehouse, "A", 10, 10, 10);
			Factory.Save();

			var dummy1 = Factory.New<DummyWithLocation>();
			dummy1.Location = warehouse.FindLocation("A-1-1-1");
			var dummy2 = Factory.New<DummyWithLocation>();
			dummy2.Location = warehouse.FindLocation("A-1-1-2");
			var dummy3 = Factory.New<DummyWithLocation>();
			dummy3.Location = null;
			var dummy4 = Factory.New<DummyWithLocation>();
			dummy4.Location = null;

			var dummyWithLocationCollection = new DummyWithLocationCollection(Factory);
			dummyWithLocationCollection.ApplySort(nameof(DummyWithLocation.Location), ListSortDirection.Ascending);
			AssertNull(dummyWithLocationCollection[0].Location);
			AssertNull(dummyWithLocationCollection[1].Location);
			AssertEquals("A-1-1-1", dummyWithLocationCollection[2].Location.ToLocationString());
			AssertEquals("A-1-1-2", dummyWithLocationCollection[3].Location.ToLocationString());

			dummyWithLocationCollection.ApplySort(nameof(DummyWithLocation.Location), ListSortDirection.Descending);
			AssertEquals("A-1-1-2", dummyWithLocationCollection[0].Location.ToLocationString());
			AssertEquals("A-1-1-1", dummyWithLocationCollection[1].Location.ToLocationString());
			AssertNull(dummyWithLocationCollection[2].Location);
			AssertNull(dummyWithLocationCollection[3].Location);
		}
	}

	class DummyWithLocation : DummyBusinessObject
	{
		public DummyWithLocation(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public WhsLocation Location { get; set; }
	}

	class DummyWithLocationCollection : ActiveBusinessObjectCollection<DummyWithLocation>
	{
		public DummyWithLocationCollection(BusinessObjectFactory factory)
				: base(factory)
		{
		}

		protected override IComparer GetSortComparerForProperty(PropertyDescriptor property, ListSortDirection direction)
		{
			return new LocationComparer<DummyWithLocation>(property, direction, objectWithLocation => objectWithLocation.Location);
		}
	}
}
