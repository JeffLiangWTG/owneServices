using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	[TestedType(typeof(WhsOrderStatusView))]
	class WhsOrderStatusViewBusinessObjectTest : WhsBusinessObjectTestCase
	{
		protected override bool IsDeleteSupported() => false;

		[DeveloperOnlyTest]
		public override void TestCalcPropertiesWithDbHitsUseFetchHints()
		{
			Assert("Cannot save the factory for a view.", true);
		}

		public override void TestFetchForLoad()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R5", data.Part1, 100m);
			var carrierServicelevel = data.Org1.MiscServ.CarrierServiceLevels.AddNew();
			carrierServicelevel.PL_Code = "RD";
			carrierServicelevel.PL_CarrierServiceLevelDescription = "Road";
			var truck = Helper.CreateEquipment("T001", 1m, Core.Constants.Weight.Kilograms, 1m, Core.Constants.Volume.CubicMetres);
			var load = Helper.CreateWhsLoad(data.Org1, data.Whs1.DefaultOutboundDockDoorLocation, transportUnit: truck);
			Factory.Save();

			var order1 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var order2 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O2");
			var orderLine = Helper.CreateWhsOrderLine(order2, data.Part1, 50m);
			Factory.Save();

			var factory1 = new BusinessObjectFactory { RefreshEnabled = false };
			factory1.Load<WhsOrderStatusView>(new ZQuery());

			var dbHits = new Dictionary<string, int>();
			dbHits.Add(WhsOrderStatusViewSchema.Constants.TableName, 1);
			AssertDbHits(dbHits, factory1);

			Helper.CreateWhsOrderLine(order1, data.Part1, 50m);
			var pick1 = Helper.CreatePickNew(order1);
			var pick2 = Helper.CreatePickNew(order2);
			orderLine.PickLines.Single().WZ_PickedDateTime = ZDateTimeOffset.Now;
			var package = order2.PackageJob.Packages.AddNew();

			var loadPkgPackagePivot = Helper.CreateLoadPkgPackagePivot(package.PK, load);
			loadPkgPackagePivot.WLP_LoadedTime = ZDateTimeOffset.Now;
			loadPkgPackagePivot.WLP_GS_NKLoadingUser = "E";
			Factory.Save();

			pick1.FinaliseAllOrders();
			pick1.FinalisePick();
			AssertIsFinalisedPrecondition(order1);
			AssertIsFinalisedPrecondition(pick1);
			Factory.Save();

			var factory2 = new BusinessObjectFactory { RefreshEnabled = false };
			factory2.Load<WhsOrderStatusView>(new ZQuery());

			AssertDbHits(dbHits, factory2);
		}

		public void TestCanDelete()
		{
			AssertEquals("Not allowed to delete dbo.WhsOrderStatusView", false, GetNewBusinessObject().CanDelete);
		}

		public void TestWOS_OrderStatus_IsLiteralOnly()
		{
			Assert(WhsOrderStatusViewSchema.WOS_OrderStatus.IsLiteralOnly);
		}
	}
}
