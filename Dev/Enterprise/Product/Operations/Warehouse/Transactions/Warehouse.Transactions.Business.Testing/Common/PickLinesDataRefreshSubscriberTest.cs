using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	class PickLinesDataRefreshSubscriberTest : WhsTestCaseWithFactory
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>(
				"Exception is thrown if null IPickLineUpdatesRefreshable is passed in.",
				() => new PickLinesDataRefreshBusSubscriber(Factory, null));
			AssertExceptionThrown<ArgumentNullException>("Exception is thrown if null factory is passed in.",
				() => new PickLinesDataRefreshBusSubscriber(null, new MockPickLineUpdatesRefreshable()));
		}

		public void TestPickLinesDataRefreshSubscriber()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			order.WD_RequiredDate = ZDateTimeOffset.Now;
			order.ConsigneePK = data.Org1.PK;

			var orderLine = Helper.CreateWhsPickableDocketLine(order, data.Part1, 10m);
			Helper.CreatePickNew(order);
			AssertEquals("Order line has pick line.", true, orderLine.PickLines.Count > 0);
			var pickLine1 = orderLine.PickLines[0];
			var pickLine1PK = pickLine1.PK;
			Factory.Save();

			var newFactory = new BusinessObjectFactory() { RefreshEnabled = true };

			var pickLine1InNewFactory = newFactory.Load<WhsPickLine>(pickLine1PK);
			pickLine1InNewFactory.Delete();

			var newPickLineInNewFactory = newFactory.New<WhsPickLine>();
			newPickLineInNewFactory.WZ_WE_InventoryLine = receive.Lines[0].PK;
			newPickLineInNewFactory.WZ_WE_TransactionLine = orderLine.PK;
			newPickLineInNewFactory.WZ_Units = 10m;

			var pickLineUpdatesRefreshable = new MockPickLineUpdatesRefreshable();
			new PickLinesDataRefreshBusSubscriber(Factory, pickLineUpdatesRefreshable);
			newFactory.Save();

			AssertEquals("Pick line is deleted.", true, pickLine1.IsDeleted);
			AssertEquals("Refresh is called.", true, pickLineUpdatesRefreshable.CalledRefresh);

			var updatedPickLines = pickLineUpdatesRefreshable.GetUpdatedPickLines();
			AssertContainsExactElementsInAnyOrder("Updated pick lines from data refresh excludes deleted pick lines.",
				new[] { newPickLineInNewFactory.PK }, updatedPickLines.Select(pl => pl.PK));
		}
	}

	class MockPickLineUpdatesRefreshable : IPickLineUpdatesRefreshable
	{
		public MockPickLineUpdatesRefreshable()
		{
			updatedPickLines = new List<WhsPickLine>();
		}

		public bool CalledRefresh;
		public List<WhsPickLine> GetUpdatedPickLines() => updatedPickLines;
		readonly List<WhsPickLine> updatedPickLines;

		public void Refresh(IEnumerable<WhsPickLine> pickLines)
		{
			CalledRefresh = true;
			updatedPickLines.AddRange(pickLines);
		}
	}
}
