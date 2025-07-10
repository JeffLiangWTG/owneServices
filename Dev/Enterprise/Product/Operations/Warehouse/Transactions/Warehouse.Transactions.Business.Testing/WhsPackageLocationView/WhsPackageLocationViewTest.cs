using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Packing.Business;
using Enterprise.Warehouse.Environment.CodeLists;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	[TestedType(typeof(WhsPackageLocationView))]
	class WhsPackageLocationViewBusinessObjectTest : WhsBusinessObjectTestCase
	{
		#region TestWarehouse

		public void TestWarehouse()
		{
			var whs1 = Helper.CreateWarehouse("WHS1", "A", 2, 1);
			var whs2 = Helper.CreateWarehouse("WHS2", "B", 2, 1);

			var packageLocation = Factory.New<WhsPackageLocationView>();

			packageLocation.WPK_WW_Whs = whs1.PK;
			AssertEquals(whs1, packageLocation.Warehouse);

			packageLocation.WPK_WW_Whs = whs2.PK;
			AssertEquals(whs2, packageLocation.Warehouse);
		}

		#endregion

		#region TestLocation

		public void TestLocation()
		{
			var whs1 = Helper.CreateWarehouse("WHS1", "A", 2, 1);
			var whs2 = Helper.CreateWarehouse("WHS2", "B", 2, 1);

			var warehouse1Location = whs1.Rows[0].Locations[0];
			var warehouse2Location = whs2.Rows[0].Locations[0];

			var packageLocation = Factory.New<WhsPackageLocationView>();

			packageLocation.WPK_WL_Location = warehouse1Location.PK;
			AssertEquals(warehouse1Location, packageLocation.Location);

			packageLocation.WPK_WL_Location = warehouse2Location.PK;
			AssertEquals(warehouse2Location, packageLocation.Location);
		}

		#endregion

		#region TestPackage

		public void TestPackage()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R5", data.Part1, 100m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			Factory.Save();

			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 50m);
			var pick = Helper.CreatePickNew(order);
			orderLine.PickLines.Single().WZ_PickedDateTime = ZDateTimeOffset.Now;
			var pkg1 = order.PackageJob.Packages.AddNew();
			var pkg2 = order.PackageJob.Packages.AddNew();
			Factory.Save();

			var packageLocation = Factory.New<WhsPackageLocationView>();

			packageLocation.WPK_KP_Package = pkg1.PK;
			AssertEquals(pkg1, packageLocation.Package);

			packageLocation.WPK_KP_Package = pkg2.PK;
			AssertEquals(pkg2, packageLocation.Package);
		}

		#endregion

		#region Test Overrides

		public override void TestFetchForLoad()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);

			var packingConsolidationLocationType = Helper.CreateLocationType("CON", "Packing", false, 0, LocationClasses.Codes.CON);
			var packingLocation = Helper.CreateRowAndGenerateLocations(data.Whs1, "PS1", 1, 1).Locations[0];
			packingLocation.WLV_WLT_LocationType = packingConsolidationLocationType.PK;

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 20m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			Helper.CreatePickNew(order);

			var pickLine = order.Lines[0].PickLines.Single();
			var transferLine = Helper.PickAndMakeInTransitTransfer(pickLine, ZDateTimeOffset.Now);
			transferLine.WE_WL = packingLocation.PK;

			transferLine.FinaliseDocketLine();
			Factory.Save();

			var packageJob = PkgPackageJob.LoadOrCreatePackageJob(order);

			var package = PackingHelper.CreatePackage(packageJob, "PKG", 1, PkgUnit.Box);
			package.Pack(order.Lines[0].ReleaseLines[0], 10m);

			Factory.Save();

			var factory2 = new BusinessObjectFactory { RefreshEnabled = false };
			factory2.Load<WhsPackageLocationView>(new ZQuery());

			var dbHits = new Dictionary<string, int>();
			dbHits.Add(WhsPackageLocationViewSchema.Constants.TableName, 1);
			AssertDbHits(dbHits, factory2);
		}

		protected override bool IsDeleteSupported() => false;

		[DeveloperOnlyTest]
		public override void TestCalcPropertiesWithDbHitsUseFetchHints()
		{
			Assert("Cannot save the factory for a view.", true);
		}

		public void TestCanDelete()
		{
			AssertEquals("Not allowed to delete dbo.WhsPackageLocationView", false, GetNewBusinessObject().CanDelete);
		}

		#endregion

	}
}
