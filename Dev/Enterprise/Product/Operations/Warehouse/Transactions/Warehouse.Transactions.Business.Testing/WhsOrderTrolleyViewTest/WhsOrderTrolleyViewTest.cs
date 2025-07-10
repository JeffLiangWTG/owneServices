using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	[TestedType(typeof(WhsOrderTrolleyView))]

	class WhsOrderTrolleyViewBusinessObjectTest : WhsBusinessObjectTestCase
	{
		protected override bool IsDeleteSupported() => false;

		[DeveloperOnlyTest]
		public override void TestCalcPropertiesWithDbHitsUseFetchHints()
		{
			Assert("Cannot save the factory for a view.", true);
		}

		public void TestCanDelete()
		{
			AssertEquals("Not allowed to delete dbo.WhsOrderTrolleyView", false, GetNewBusinessObject().CanDelete);
		}

		public override void TestFetchForLoad()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 20m);
			Factory.Save();

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 5m); // assigned to T1
			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part1, 5m); // assigned to T2
			var order3 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O3", data.Part1, 5m); // assigned to T2
			var pick1 = Helper.CreatePickNew(order1, order2);
			var pick2 = Helper.CreatePickNew(order3);

			var trolley1 = Helper.CreateTrolley("T1");
			var trolley2 = Helper.CreateTrolley("T2");
			var trolley3 = Helper.CreateTrolley("T3");

			var package1 = order1.PackageJob.Packages.AddNew();
			var package2 = order1.PackageJob.Packages.AddNew();
			var package3 = order1.PackageJob.Packages.AddNew();
			var package4 = order2.PackageJob.Packages.AddNew();
			var package5 = order2.PackageJob.Packages.AddNew();
			var package6 = order3.PackageJob.Packages.AddNew();

			//load trolley slots
			var trolleyJob1 = Helper.CreateWhsPickTrolleyJob(trolley1.PK, "PIC");

			Helper.CreateWhsPickTrolleySlot(trolleyJob1.PK, package1.PK, 1);
			Helper.CreateWhsPickTrolleySlot(trolleyJob1.PK, package4.PK, 2);

			var trolleyJob2 = Helper.CreateWhsPickTrolleyJob(trolley2.PK, "PIC");
			Helper.CreateWhsPickTrolleySlot(trolleyJob2.PK, package2.PK, 1);
			Helper.CreateWhsPickTrolleySlot(trolleyJob2.PK, package5.PK, 2);

			var trolleyJob3 = Helper.CreateWhsPickTrolleyJob(trolley3.PK, "PIC");
			Helper.CreateWhsPickTrolleySlot(trolleyJob3.PK, package3.PK, 1);
			Helper.CreateWhsPickTrolleySlot(trolleyJob3.PK, package6.PK, 2);

			pick2.FinaliseAllOrders();
			pick2.FinalisePick();
			AssertEquals("Precondition = ensure pick1 is NOT finalised", false, pick1.IsFinalised);
			WhsTestCaseWithFactory.AssertIsFinalisedPrecondition(pick2);
			Factory.Save();

			var factory1 = new BusinessObjectFactory { RefreshEnabled = false };
			factory1.Load<WhsOrderTrolleyView>(new ZQuery());

			var dbHits = new Dictionary<string, int>();
			dbHits.Add(WhsOrderTrolleyViewSchema.Constants.TableName, 1);
			AssertDbHits(dbHits, factory1);
		}
	}
}
