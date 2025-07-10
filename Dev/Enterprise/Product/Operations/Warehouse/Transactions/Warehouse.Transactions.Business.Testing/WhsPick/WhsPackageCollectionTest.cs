using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	[TestedType(typeof(WhsPackageCollection))]
	class WhsPackageCollectionTest : ActiveBusinessObjectCollectionTestCase<WhsPackageCollection>
	{
		public void TestWhsPackageCollection()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			data.Whs1.WW_IsPickByUOMEnabled = true;
			var locA1 = data.Whs1.FindLocation("A-1");
			var locA2 = data.Whs1.FindLocation("A-2");

			var helper = new WhsTestHelperFunctions(Factory);
			helper.CreateProductUnit(data.Part1, "CAS", 5);
			helper.CreateProductUnit(data.Part1, "PLT", 20);

			Factory.LoadFromUniqueKey<RefPackType>(RefPackTypeSchema.F3_Code, new ZString("CAS")).F3_UOMType = UOMPackTypesList.Codes.Case;
			Factory.LoadFromUniqueKey<RefPackType>(RefPackTypeSchema.F3_Code, new ZString("PLT")).F3_UOMType = UOMPackTypesList.Codes.Pallet;

			var receivePalletLoc1 = helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 20, locA1, "PLT1");
			var receiveCaseLoc2 = helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 5, locA2, "CAS1");
			Factory.Save();

			var order = helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine1 = helper.CreateWhsOrderLine(order, data.Part1, 20);
			var orderLine2 = helper.CreateWhsOrderLine(order, data.Part1, 5);
			var pick = helper.CreatePickNew(order);
			pick.WP_PickCasesByLabel = true;
			pick.WP_PickPalletsByLabel = true;
			Factory.Save();

			var query = new ZQuery(PkgPackageSchema.KP_KP_ParentPackage, null);
			query.AddToFilter(PkgPackageSchema.KP_KJ_ParentPackageJob, new ZGuid[] { order.PackageJob.PK });
			var collection = new WhsPackageCollection(Factory, query);
			AssertEquals("Precondition", 0, collection.Count);
			pick.AllocatePackageLabels();

			AssertEquals("There should be 2 packages: 1 for pallet and 1 for case.", 2, pick.OuterPackages.Count);
			var packageCAS = pick.OuterPackages.Single(package => package.PackType.F3_Code.Equals("CAS"));
			var packagePLT = pick.OuterPackages.Single(package => package.PackType.F3_Code.Equals("PLT"));

			AssertEquals("This is active collection should update the collection.", 2, collection.Count);
		}

		public void TestGetFetchStrategy()
		{
			var collection = new WhsPackageCollection(Factory, ZQuery.NoResultQuery); // query is not effect on FetchStrategy
			AssertType<WhsPackageCollectionFetchStrategy>(((IBusinessObjectCollection)collection).FetchStrategy);
		}

		protected override WhsPackageCollection GetCollectionToTest() => new WhsPackageCollection(Factory, null);
	}
}
