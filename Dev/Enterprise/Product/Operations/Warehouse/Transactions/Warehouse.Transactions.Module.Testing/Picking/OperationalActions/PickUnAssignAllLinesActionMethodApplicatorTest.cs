using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Warehouse.Environment.Business.Testing;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.Warehouse.Transactions.CodeLists;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Module.Testing
{
	[TestedType(typeof(PickUnAssignAllLinesActionMethodApplicator))]
	public class PickUnAssignAllLinesActionMethodApplicatorTest : UnAssignAllLinesActionMethodApplicatorTest<PickUnAssignAllLinesActionMethodApplicator, WhsPick>
	{
		protected override IEnumerable<WhsPickLine> OperationalActionSampleData()
		{
			var data = new TestDataSimpleEnvironment(Factory, 1, 5);
			var locations = data.Whs1.Rows.Single(r => r.WR_Name == "A").Locations;
			var notify = new TestNotificationBuffer();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "Receive", notify);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, locations[0]);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 5m, locations[1]);

			receive.AllocateLocationsWithMock();
			receive.FinaliseDocket();

			Factory.Save();

			var order1 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			Helper.CreateWhsOrderLine(order1, data.Part1, 10m);
			Helper.CreateWhsOrderLine(order1, data.Part2, 5m);

			var createdPick = Helper.CreatePickByAttachingOrders(order1);

			var orderedInvetoryForCreatedPick1 = createdPick.OrderedInventories[0];
			var orderedInvetoryForCreatedPick2 = createdPick.OrderedInventories[1];

			var allocatedForCreatedPick1 = Helper.SetAvailableInventory(orderedInvetoryForCreatedPick1.AvailableInventories[0], true, ZDateTimeOffset.Empty);
			var allocatedForCreatedPick2 = Helper.SetAvailableInventory(orderedInvetoryForCreatedPick2.AvailableInventories[0], true, ZDateTimeOffset.Empty);

			createdPick.OrderedInventories.Add(orderedInvetoryForCreatedPick1);
			createdPick.OrderedInventories.Add(orderedInvetoryForCreatedPick2);
			Factory.Save();

			createdPick.WP_PickStatus = PickStatus.Codes.Created;

			foreach (var line in createdPick.GetAllPickLines())
			{
				line.WZ_GS_NKAssignedTo = SelectedUser.GS_Code;
			}

			Factory.Save();

			CreatedDocket = createdPick;

			return createdPick.GetAllPickLines();
		}

		protected override string OperationName => "Pick";

		protected override string JobNo => CreatedDocket.WP_PickNo;

		protected override string FinaliseStatus => PickStatus.Codes.Finalised;

		protected override string CancelledStatus => PickStatus.Codes.Cancelled;

		protected override void SetDocketStatus(string status) => CreatedDocket.WP_PickStatus = status;

		protected override PickUnAssignAllLinesActionMethodApplicator GetNewApplicator()
		{
			return new PickUnAssignAllLinesActionMethodApplicator(Factory);
		}
	}
}
