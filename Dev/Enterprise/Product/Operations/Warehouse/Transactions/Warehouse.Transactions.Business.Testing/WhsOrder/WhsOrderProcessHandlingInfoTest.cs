using System.Linq;
using Enterprise.Packing.Business;
using Enterprise.Packing.Business.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Business.EventManagement;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	class WhsOrderProcessHandlingInfoTest : WhsTestCaseWithFactory
	{
		public void TestPopulateCascadingTargets()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			Factory.Save();

			var handlingInfo = new WhsOrderProcessHandlingInfo(order);
			var eventLog = Factory.New<StmALog>();
			using (eventLog.LockForUpdatingKeyFieldsForTesting())
			{
				eventLog.SL_SE_NKEvent = Events.ServiceCancelledCode;
			}

			AssertEquals(Enumerable.Empty<CascadingLink>(), handlingInfo.GetCascadingTargets(eventLog));
		}

		public void TestPropagationTargets_FreightLoaded()
		{
			AssertPropagationTargets(Events.FreightLoadedCode);
		}

		public void TestPropagationTargets_FreightDeparted()
		{
			AssertPropagationTargets(Events.DepartureCode);
		}

		public void TestPropagationTargets_PackingCompleted()
		{
			AssertPropagationTargets(Events.PackingCompletedCode);
		}

		public void TestPropagationTargets_PickedUp()
		{
			AssertPropagationTargets(Events.PickedUpCode);
		}

		void AssertPropagationTargets(string eventCode)
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);
			Factory.Save();

			AssertEquals($"Precondition: No {eventCode} event on Order.", 0,
				order.Logs.Find(Helper.GetLogFilter(eventCode)).Length);
			AssertEquals($"Precondition: No {eventCode} event on Pick.", 0,
				pick.Logs.Find(Helper.GetLogFilter(eventCode)).Length);

			order.Logs.AddNew(Events.All[eventCode]);
			Factory.Save();

			AssertEquals($"{eventCode} event exists on Order.", 1,
				order.Logs.Find(Helper.GetLogFilter(eventCode)).Length);
			AssertEquals($"{eventCode} event has been propagated to Pick.", 1,
				pick.Logs.Find(Helper.GetLogFilter(eventCode)).Length);
		}

		public void TestPropagationTargets_PickHasMultipleOrders_FreightLoaded()
		{
			AssertPropagationTargets_PickHasMultipleOrders(Events.FreightLoadedCode);
		}

		public void TestPropagationTargets_PickHasMultipleOrders_FreightDeparted()
		{
			AssertPropagationTargets_PickHasMultipleOrders(Events.DepartureCode);
		}

		public void TestPropagationTargets_PickHasMultipleOrders_PackingCompleted()
		{
			AssertPropagationTargets_PickHasMultipleOrders(Events.PackingCompletedCode);
		}

		public void TestPropagationTargets_PickHasMultipleOrders_PickedUp()
		{
			AssertPropagationTargets_PickHasMultipleOrders(Events.PickedUpCode);
		}

		void AssertPropagationTargets_PickHasMultipleOrders(string eventCode)
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 5m);
			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part1, 5m);
			var pick = Helper.CreatePickNew(order1, order2);
			Factory.Save();

			order1.Logs.AddNew(Events.All[eventCode]);
			Factory.Save();

			AssertEquals("Packing Completed event exists on Order.", 1,
				order1.Logs.Find(Helper.GetLogFilter(eventCode)).Length);
			AssertEquals("No Packing Completed event on Pick.", 0,
				pick.Logs.Find(Helper.GetLogFilter(eventCode)).Length);

			order2.Logs.AddNew(Events.All[eventCode]);
			Factory.Save();

			AssertEquals($"{eventCode} event exists on Order.", 1,
				order2.Logs.Find(Helper.GetLogFilter(eventCode)).Length);
			AssertEquals($"{eventCode} event has been propagated to Pick: Propagated: All Orders",
				1, pick.Logs.Find(Helper.GetLogFilter(eventCode, "Propagated: All Orders")).Length);
		}

		public void TestPropagationTargets_PropagationFromPackages_FreightLoaded()
		{
			AssertPropagationTargets_PropagationFromPackages(Events.FreightLoadedCode);
		}

		public void TestPropagationTargets_PropagationFromPackages_FreightDeparted()
		{
			AssertPropagationTargets_PropagationFromPackages(Events.DepartureCode);
		}

		public void TestPropagationTargets_PropagationFromPackages_PackingCompleted()
		{
			AssertPropagationTargets_PropagationFromPackages(Events.PackingCompletedCode);
		}

		public void TestPropagationTargets_PropagationFromPackages_PickedUp()
		{
			AssertPropagationTargets_PropagationFromPackages(Events.PickedUpCode);
		}

		void AssertPropagationTargets_PropagationFromPackages(string eventCode)
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 5m);
			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part1, 5m);
			var pick = Helper.CreatePickNew(order1, order2);
			Factory.Save();

			var pkgJob = PkgPackageJob.LoadOrCreatePackageJob(order1);
			var package1 = PackingHelper.CreatePackage(pkgJob, "P1", 1, "BOX");

			package1.Logs.AddNew(Events.All[eventCode]);
			Factory.Save();

			AssertEquals($"{eventCode} event has been propagated to Order.", 1,
				order1.Logs.Find(Helper.GetLogFilter(eventCode)).Length);
			AssertEquals($"No {eventCode} event on Pick.", 0,
				pick.Logs.Find(Helper.GetLogFilter(eventCode)).Length);

			var pkgJob2 = PkgPackageJob.LoadOrCreatePackageJob(order2);
			var package2 = PackingHelper.CreatePackage(pkgJob2, "P2", 1, "BOX");
			package2.Logs.AddNew(Events.All[eventCode]);
			Factory.Save();

			AssertEquals($"{eventCode} event has been propagated to Order.", 1,
				order2.Logs.Find(Helper.GetLogFilter(eventCode)).Length);
			AssertEquals($"{eventCode} event has been propagated to Pick: Propagated: All Orders",
				1, pick.Logs.Find(Helper.GetLogFilter(eventCode, "Propagated: All Orders")).Length);
		}

		protected PackingTestHelper PackingHelper
		{
			get
			{
				return packingHelper ?? (packingHelper = new PackingTestHelper(Factory));
			}
		}

		PackingTestHelper packingHelper;

		public void TestPropagationTargets_NonPKCEvents()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);
			Factory.Save();

			order.Logs.AddNew(Events.ServiceCompleted);
			Factory.Save();

			AssertEquals("Service Completed event exists on Order.", 1,
				order.Logs.Find(Helper.GetLogFilter(Events.ServiceCompleted.Code)).Length);
			AssertEquals("No Service Completed event on Pick.", 0,
				pick.Logs.Find(Helper.GetLogFilter(Events.ServiceCompleted.Code)).Length);
		}
	}
}
