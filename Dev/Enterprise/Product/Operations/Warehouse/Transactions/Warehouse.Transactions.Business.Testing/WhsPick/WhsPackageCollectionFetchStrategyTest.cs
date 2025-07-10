using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Packing.Business;
using Enterprise.Packing.Business.Testing;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	class WhsPackageCollectionFetchStrategyTest : TestCaseWithFactory
	{
		public void TestWhsPackageCollectionFetchStrategy_PackageStatus()
		{
			var columns = new[] { new TableColumn(PkgPackageSchema.Constants.TableName, PkgPackage.Schema.PackageStatus) };
			TestWhsPackageCollectionFetchStrategyCore(columns, expectAddFetchHint: true);
		}

		public void TestWhsPackageCollectionFetchStrategy_NotPackageStatus()
		{
			var columns = new[] { new TableColumn(PkgPackageSchema.Constants.TableName, PkgPackage.Schema.LabelPrinted) };
			TestWhsPackageCollectionFetchStrategyCore(columns, expectAddFetchHint: false);
		}

		public void TestWhsPackageCollectionFetchStrategy_IncludePackageStatus()
		{
			var columns = new[] {
				new TableColumn(PkgPackageSchema.Constants.TableName, PkgPackage.Schema.PackageStatus),
				new TableColumn(PkgPackageSchema.Constants.TableName, PkgPackage.Schema.LabelPrinted)
			};
			TestWhsPackageCollectionFetchStrategyCore(columns, expectAddFetchHint: true);
		}

		void TestWhsPackageCollectionFetchStrategyCore(TableColumn[] columns, bool expectAddFetchHint)
		{
			var data = new TestDataForInventory(Factory);
			data.CreateSimpleInventory(true);
			var helper = new WhsTestHelperFunctions(Factory);
			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 1);
			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 1);
			Factory.Save();

			var order1 = helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 1m);
			var order2 = helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part1, 1m);
			var pick = helper.CreatePickNew(order1, order2);
			Factory.Save();

			var pickLines = pick.GetAllPickLines().ToArray();
			var packingHelper = new PackingTestHelper(Factory);
			var package1 = packingHelper.CreatePackage(order1.PackageJob, 1, "UNT");
			var pickLine1 = pickLines[0];
			packingHelper.CreatePackageDivot(package1, pickLine1);
			var load1 = Factory.NewWithValidTestData<WhsLoad>();
			load1.WLO_StartTime = DateTimeOffset.Now;
			load1.WLO_TransportationUnitNumber = "ABC456";
			var loadPkgPackagePivot1 = helper.CreateLoadPkgPackagePivot(package1.PK, load1);

			var package2 = packingHelper.CreatePackage(order2.PackageJob, 1, "UNT");
			var pickLine2 = pickLines[1];
			packingHelper.CreatePackageDivot(package2, pickLine2);
			var load2 = Factory.NewWithValidTestData<WhsLoad>();
			load2.WLO_StartTime = DateTimeOffset.Now;
			load2.WLO_TransportationUnitNumber = "ABC456";
			helper.CreateLoadPkgPackagePivot(package2.PK, load2);
			Factory.Save();

			var viewFactory = new BusinessObjectFactory();
			var packageInNewFactory = viewFactory.Load<PkgPackage>(package1.PK);
			var query = new ZQuery(PkgPackageSchema.KP_KP_ParentPackage, null);
			query.AddToFilter(PkgPackageSchema.KP_KJ_ParentPackageJob, new[] { order1.PackageJob.PK });
			var collection = new WhsPackageCollection(viewFactory, query);
			var packageCollectionFetchStrategy = new WhsPackageCollectionFetchStrategy(collection);
			AssertEquals("Precondotion", 0, viewFactory.ActiveFetchHintsForTable(WhsLoadPkgPackagePivotSchema.Constants.TableName));

			packageCollectionFetchStrategy.FetchForView(pick.OuterPackages.ToArray(), columns);

			AssertEquals("Should add fetch hint only when accessing PackageStatus column.", expectAddFetchHint ? 2 : 0, viewFactory.ActiveFetchHintsForTable(WhsLoadPkgPackagePivotSchema.Constants.TableName));
			AssertEquals("Precondition", 0, viewFactory.Load<WhsLoadPkgPackagePivot>(new ZQuery() { FetchOnlyFromLocalCache = true }).Length);

			var pkgPackageAdditionalColumns = new CustomPropertyContainer<PkgPackage>();
			pkgPackageAdditionalColumns.AddCustomProperty(PkgPackage.Schema.PackageStatus, "Package Status", typeof(ZString), (pkg) => new PackagePackingHelper(pkg).GetPackageLabelStatus());
			pkgPackageAdditionalColumns[PkgPackage.Schema.PackageStatus].GetValue(packageInNewFactory);

			AssertEquals("Should Load All WhsLoadPkgPackagePivots if fetch hints added.", expectAddFetchHint ? 2 : 1, viewFactory.Load<WhsLoadPkgPackagePivot>(new ZQuery() { FetchOnlyFromLocalCache = true }).Length);
		}

		public void TestWhsPackageCollectionFetchStrategy_PickArea_UsesTVP()
		{
			using (var settings = TestEntityFrameworkSettings.Get())
			{
				settings.TVPRule = new TVPRule("1");

				var data = new TestDataForInventory(Factory);
				data.CreateSimpleInventory(true);
				var helper = new WhsTestHelperFunctions(Factory);
				helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 1);
				helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 1);
				Factory.Save();

				var order1 = helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 1m);
				var order2 = helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part1, 1m);
				var pick = helper.CreatePickNew(order1, order2);
				Factory.Save();

				var pickLines = pick.GetAllPickLines().ToArray();
				var packingHelper = new PackingTestHelper(Factory);
				var package1 = packingHelper.CreatePackage(order1.PackageJob, 1, "UNT");
				var pickLine1 = pickLines[0];
				packingHelper.CreatePackageDivot(package1, pickLine1);

				var package2 = packingHelper.CreatePackage(order2.PackageJob, 1, "UNT");
				var pickLine2 = pickLines[1];
				packingHelper.CreatePackageDivot(package2, pickLine2);
				Factory.Save();

				var viewFactory = new BusinessObjectFactory();
				var packageInNewFactory = viewFactory.Load<PkgPackage>(package1.PK);
				var query = new ZQuery(PkgPackageSchema.KP_KP_ParentPackage, null);
				query.AddToFilter(PkgPackageSchema.KP_KJ_ParentPackageJob, new[] { order1.PackageJob.PK });
				var collection = new WhsPackageCollection(viewFactory, query);

				var packageCollectionFetchStrategy = new WhsPackageCollectionFetchStrategy(collection);

				using (TestConnection.TrackExecutedCommands())
				{
					packageCollectionFetchStrategy.FetchForView(pick.OuterPackages.ToArray(), new[] { new TableColumn(PkgPackageSchema.Constants.TableName, WhsLocation.Schema.PickArea) });
					viewFactory.ExecuteAllFetchHints();

					var areaQueryCommand = TestConnection.ExecutedCommands.SingleOrDefault(c => c.Contains("WHERE WA_PK IN"));
					AssertContains("Area fetch hint query should use TVP.", "KI_KP_Package in (SELECT Value FROM", areaQueryCommand);
				}
			}
		}
	}
}
