using System.Linq;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.EventManagement;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	class WhsPickProcessHandlingInfoTest : WhsTestCaseWithFactory
	{
		public void TestPopulatePropagationTargets()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 30m);
			var order1 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			Helper.CreateWhsOrderLine(order1, data.Part1, 10m);
			var order2 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O2");
			Helper.CreateWhsOrderLine(order2, data.Part1, 10m);
			Factory.Save();

			var pick = Helper.CreatePickNew(order1, order2);
			Factory.Save();

			var eventLog = Factory.New<StmALog>();
			using (eventLog.LockForUpdatingKeyFieldsForTesting())
			{
				eventLog.SL_SE_NKEvent = Events.PackingCommencedCode;
			}

			AssertEquals("Order1 belongs to pick.", pick.PK, order1.WD_WP);
			AssertEquals("Order2 belongs to pick.", pick.PK, order2.WD_WP);

			var pickProcessHandlingInfo = new WhsPickProcessHandlingInfo(pick);
			AssertEquals(Enumerable.Empty<PropagationLink>(), pickProcessHandlingInfo.GetPropagationTargets(eventLog));
		}

		public void TestPopulateCascadingTargets()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 60m);
			var order1 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			Helper.CreateWhsOrderLine(order1, data.Part1, 10m);
			var order2 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O2");
			Helper.CreateWhsOrderLine(order2, data.Part1, 20m);
			var order3 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O3");
			Helper.CreateWhsOrderLine(order3, data.Part1, 30m);
			Factory.Save();

			var pick = Helper.CreatePickNew(order1, order2, order3);
			Factory.Save();

			var processTaskForOrder1 = Factory.New<ProcessTask>();
			processTaskForOrder1.P9_ParentID = order1.PK;
			processTaskForOrder1.TriggerConditions.TriggerEventCode = Events.PackingCommencedCode;
			processTaskForOrder1.P9_ParentTableCode = WhsDocketSchema.Constants.Prefix;
			processTaskForOrder1.P9_RespondToCascadedEvents = true;

			var processTaskForOrder2 = Factory.New<ProcessTask>();
			processTaskForOrder2.P9_ParentID = order2.PK;
			processTaskForOrder2.TriggerConditions.TriggerEventCode = Events.PackingCommencedCode;
			processTaskForOrder2.P9_ParentTableCode = WhsDocketSchema.Constants.Prefix;
			processTaskForOrder2.P9_RespondToCascadedEvents = false;

			var eventLog = Factory.New<StmALog>();
			using (eventLog.LockForUpdatingKeyFieldsForTesting())
			{
				eventLog.SL_SE_NKEvent = Events.PackingCommencedCode;
			}

			AssertEquals("Order1 belongs to pick.", pick.PK, order1.WD_WP);
			AssertEquals("Order2 belongs to pick.", pick.PK, order2.WD_WP);
			AssertEquals("Order3 belongs to pick.", pick.PK, order3.WD_WP);

			var pickProcessHandlingInfo = new WhsPickProcessHandlingInfo(pick);
			var cascadingTargets = pickProcessHandlingInfo.GetCascadingTargets(eventLog);
			AssertEquals("3 cascading targets are returned.", 3, cascadingTargets.Count());
			AssertEquals(processTaskForOrder1,
				cascadingTargets.Single(c => c.Parent.LogsParentPK == order1.PK).Triggers.Single());
			AssertEquals(false, cascadingTargets.Single(c => c.Parent.LogsParentPK == order2.PK).Triggers.Length > 0);
			AssertEquals(false, cascadingTargets.Single(c => c.Parent.LogsParentPK == order3.PK).Triggers.Length > 0);
		}

		public void TestPopulateCascadingTargets_NotPackingCommencedEvent()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 30m);
			var order1 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			Helper.CreateWhsOrderLine(order1, data.Part1, 10m);
			var order2 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O2");
			Helper.CreateWhsOrderLine(order2, data.Part1, 10m);
			Factory.Save();

			var pick = Helper.CreatePickNew(order1, order2);
			Factory.Save();

			var eventLog = Factory.New<StmALog>();
			using (eventLog.LockForUpdatingKeyFieldsForTesting())
			{
				eventLog.SL_SE_NKEvent = Events.PackingCompletedCode;
			}

			AssertEquals("Order1 belongs to pick.", pick.PK, order1.WD_WP);
			AssertEquals("Order2 belongs to pick.", pick.PK, order2.WD_WP);

			var pickProcessHandlingInfo = new WhsPickProcessHandlingInfo(pick);
			AssertEquals("Cascading targets is not populated.", null,
				pickProcessHandlingInfo.GetCascadingTargets(eventLog));
		}
	}
}
