using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	[TestedType(typeof(WhsDockDoorAssignment))]
	class WhsDockDoorAssignmentTest : WhsBusinessObjectTestCase
	{
		public void TestAssignedDockDoor()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var assignment = Factory.New<WhsDockDoorAssignment>();
			assignment.WDA_WL_AssignedDockDoor = data.Whs1.WW_DefaultOutboundDockDoor;
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 1m);
			var pick = Helper.CreatePickByAttachingOrders(order);
			pick.WP_WL_DockDoor = ZGuid.Empty;
			pick.WP_WDA_DockDoorAssignment = assignment.PK;
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var doorAssignment = newFactory.Load<WhsDockDoorAssignment>(new ZQuery(WhsDockDoorAssignmentSchema.PK, assignment.PK)).Single();
			var ddlInNewFactory = doorAssignment.AssignedDockDoor;
			AssertNotNull(ddlInNewFactory);
			AssertEquals(data.Whs1.WW_DefaultOutboundDockDoor, ddlInNewFactory.PK);
		}

		public void TestTG_WhsDockDoorAssignment_EnsureDockDoorAssignmentsAreReferenced_ByAPick()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var assignment1 = Factory.New<WhsDockDoorAssignment>();
			assignment1.WDA_WL_AssignedDockDoor = data.Whs1.DefaultOutboundDockDoorLocation.PK;

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 1m);
			var pick = Helper.CreatePickByAttachingOrders(order);
			pick.WP_WL_DockDoor = ZGuid.Empty;
			pick.WP_WDA_DockDoorAssignment = assignment1.PK;

			AssertNoExceptionThrown("Expect no error", Factory.Save);

			var whs2 = Helper.CreateWarehouse("WH2");
			var assignment2 = Factory.New<WhsDockDoorAssignment>();
			assignment2.WDA_WL_AssignedDockDoor = whs2.WW_DefaultOutboundDockDoor;

			var exception = AssertExceptionThrown<ZSaveException>("Trigger should have prevented save.", Factory.Save);
			AssertEquals(true, exception.Message.Contains("Attempt to leave a Dock Door Assignment with no referencing Picks."));
		}

		public override void TestSaveAndDeleteBusinessObject()
		{
			var assignment = (WhsDockDoorAssignment)GetNewBusinessObjectForDeleteTest(Factory);
			Factory.Save();

			AssertEquals("assignment.IsDeleted", false, assignment.IsDeleted);

			var separateFactory = NewFactory();
			AssertNotNull("The BizO is saved and should have been persisted", separateFactory.Load<WhsDockDoorAssignment>(assignment.PK));

			assignment.Delete();
			AssertNoExceptionThrown(Factory.Save);
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			var result = Factory.New<WhsDockDoorAssignment>();
			var whs = Helper.CreateWarehouse("1", "A", 1, 1);
			result.WDA_WL_AssignedDockDoor = whs.WW_DefaultOutboundDockDoor;
			return result;
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var data = new TestDataSimpleEnvironment(factory, 2, 1);
			factory.Save();

			var result = factory.New<WhsDockDoorAssignment>();
			result.WDA_WL_AssignedDockDoor = data.Whs1.WW_DefaultOutboundDockDoor;
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 1m);
			var pick = Helper.CreatePickByAttachingOrders(order);
			pick.WP_WL_DockDoor = ZGuid.Empty;
			pick.WP_WDA_DockDoorAssignment = result.PK;
			return result;
		}

		#endregion
	}

	#region DeferrableTriggers_WhsDockDoorAssignmentTest Class

	[TestedType(typeof(WhsDockDoorAssignment))]
	class DeferrableTriggers_WhsDockDoorAssignmentTest : DeferrableTriggerTestCase<WhsDockDoorAssignment>
	{
	}

	#endregion
}
