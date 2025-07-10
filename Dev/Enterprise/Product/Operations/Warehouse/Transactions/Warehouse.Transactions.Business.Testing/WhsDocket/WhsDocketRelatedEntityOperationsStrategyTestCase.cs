using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	class WhsDocketRelatedEntityOperationsStrategyTestCase : WhsTestCaseWithFactory
	{
		#region TestConstructor

		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>(() => GetNewStrategy(null));
		}

		#endregion

		#region TestEventLogExists

		public void TestEventLogExists()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			var strategy = GetNewStrategy(receive);
			AssertEquals(false, strategy.EventLogExists(Events.WarehouseJobEnteredCode));

			receive.Logs.AddNew(Events.WarehouseJobEntered, "TEST");
			AssertEquals(true, strategy.EventLogExists(Events.WarehouseJobEnteredCode));
			Factory.Save();

			// test that works with both records in memory and in DB only.
			var otherFactory = new BusinessObjectFactory();
			var receiveInOtherFactory = otherFactory.Load<WhsReceive>(receive.PK);
			var strategyInAnotherFactory = GetNewStrategy(receiveInOtherFactory);
			AssertEquals(true, strategyInAnotherFactory.EventLogExists(Events.WarehouseJobEnteredCode));
		}

		public void TestEventLogExists_IgnoresCancelledLogs()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			var strategy = GetNewStrategy(receive);
			AssertEquals(false, strategy.EventLogExists(Events.WarehouseJobEnteredCode));

			var log = receive.Logs.AddNew(Events.WarehouseJobEntered, "TEST");
			AssertEquals(true, strategy.EventLogExists(Events.WarehouseJobEnteredCode));

			log.Cancel();
			AssertEquals(true, log.SL_IsCancelled);
			AssertEquals(false, strategy.EventLogExists(Events.WarehouseJobEnteredCode));
		}

		public void TestEventLogExists_IgnoresEstimateLogs()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			var strategy = GetNewStrategy(receive);
			AssertEquals(false, strategy.EventLogExists(Events.WarehouseJobEnteredCode));

			var log = receive.Logs.AddNew(Events.WarehouseJobEntered, "TEST");
			AssertEquals(true, strategy.EventLogExists(Events.WarehouseJobEnteredCode));

			using (log.LockForUpdatingKeyFieldsForTesting())
			{
				log.SL_IsEstimate = true;
			}
			AssertEquals(false, strategy.EventLogExists(Events.WarehouseJobEnteredCode));
		}

		#endregion

		#region TestGetCurrentMaxLineNo

		public void TestGetCurrentMaxLineNo()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			ZShort maxLineNo = 0;

			var strategy = GetNewStrategy(receive);
			AssertEquals((ZShort)0, strategy.GetCurrentMaxLineNo(maxLineNo));
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m);
			AssertEquals((ZShort)2, strategy.GetCurrentMaxLineNo(maxLineNo));

			Factory.Save();

			// test that works with both records in memory and in DB only.
			var otherFactory = new BusinessObjectFactory();
			var receiveInOtherFactory = otherFactory.Load<WhsReceive>(receive.PK);
			var strategyInAnotherFactory = GetNewStrategy(receiveInOtherFactory);
			AssertEquals((ZShort)2, strategyInAnotherFactory.GetCurrentMaxLineNo(maxLineNo));
		}

		#endregion

		#region Implementation

		protected virtual WhsDocketRelatedEntityOperationsStrategy GetNewStrategy(WhsDocket docket)
		{
			return new WhsDocketRelatedEntityOperationsStrategy(docket);
		}

		#endregion
	}
}
