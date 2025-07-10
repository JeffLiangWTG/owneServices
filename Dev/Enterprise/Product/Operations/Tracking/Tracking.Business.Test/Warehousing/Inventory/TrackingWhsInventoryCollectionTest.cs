using System.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Tracking.Business.Testing
{
	[TestedType(typeof(TrackingWhsInventoryCollection))]
	sealed class TrackingWhsInventoryCollectionTest : WhsInventoryViewCollectionTest
	{
		[TestDate(2019, 11, 11)]
		public void TestIModuleManualSort_Load()
		{
			var today = ZDateTimeOffset.Now;
			var data = new TestDataSimpleEnvironment(Factory, 3, 1);
			var location = data.Whs1.FindLocation("A-1");
			Factory.Save();

			var receive1 = Helper.CreateWhsReceive(data.Org1.PK, data.Whs1.PK, "R1", today);
			var receiveLine11 = Helper.CreateWhsReceiveLine(receive1, data.Part1, 10m, location, "PLT1");
			receive1.FinaliseDocketWithoutUserConfirmation();

			var receive2 = Helper.CreateWhsReceive(data.Org1.PK, data.Whs1.PK, "R2", today.AddDays(-1));
			var receiveLine21 = Helper.CreateWhsReceiveLine(receive2, data.Part1, 10m, location, "PLT1");
			receive2.FinaliseDocketWithoutUserConfirmation();

			var collection = new TrackingWhsInventoryCollection(Factory);
			collection.Load(new ZQuery(), new ListSortDescriptionCollection(new[] { new ListSortDescription(((ITypedList)collection).GetItemProperties(null)[WhsInventoryViewSchema.WI_ArrivalDate.Name], ListSortDirection.Ascending) }));
			AssertEquals("Should load all inventories", 2, collection.Count);
			AssertEquals("Order by Arrival Date", receiveLine21.PK, collection[0].PK);
			AssertEquals("Order by Arrival Date", receiveLine11.PK, collection[1].PK);
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new TrackingWhsInventoryCollection(Factory);
		}
	}
}
