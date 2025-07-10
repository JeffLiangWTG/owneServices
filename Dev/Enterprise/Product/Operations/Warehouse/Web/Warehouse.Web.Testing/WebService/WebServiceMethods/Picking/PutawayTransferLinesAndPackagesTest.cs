using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Packing.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.Web.WebService.Testing
{
	[TestedType(typeof(PutawayTransferLinesAndPackagesToClose))]
	class PutawayTransferLinesAndPackagesTest : TestCaseWithFactory
	{
		public void TestConstructorAndDeconstructor()
		{
			var helper = new WhsTestHelperFunctions(Factory);
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);

			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);

			Factory.Save();

			var order = helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine1 = helper.CreateWhsOrderLine(order, data.Part1, 5m);
			var orderLine2 = helper.CreateWhsOrderLine(order, data.Part1, 5m);
			var pick = helper.CreatePickNew(order);

			Factory.Save();

			var transferLine1 = helper.PickAndMakeInTransitTransfer(orderLine1.PickLines.Single(), ZDateTimeOffset.Now);
			var transferLine2 = helper.PickAndMakeInTransitTransfer(orderLine2.PickLines.Single(), ZDateTimeOffset.Now);

			var package1 = order.PackageJob.Packages.AddNew();
			package1.Pack(orderLine1.ReleaseLines[0], 5m);

			var package2 = order.PackageJob.Packages.AddNew();
			package2.Pack(orderLine2.ReleaseLines[0], 5m);

			Factory.Save();

			var putawayTransferLinesAndPackagesToClose = new PutawayTransferLinesAndPackagesToClose(new[] { transferLine1, transferLine2 }, new[] { package1, package2 });

			AssertContainsExactElementsInAnyOrder("Elements should match arguments provided.", new[] { transferLine1, transferLine2 }, putawayTransferLinesAndPackagesToClose.TransferLinesToPutaway);
			AssertContainsExactElementsInAnyOrder("Elements should match arguments provided.", new[] { package1, package2 }, putawayTransferLinesAndPackagesToClose.PackagesToClose);

			var (lines, packages) = putawayTransferLinesAndPackagesToClose;

			AssertContainsExactElementsInAnyOrder("Deconstructed Elements should match arguments provided.", new[] { transferLine1, transferLine2 }, lines);
			AssertContainsExactElementsInAnyOrder("Deconstructed Elements should match arguments provided.", new[] { package1, package2 }, packages);
		}

		public void TestConstructor_EmptyCollections()
		{
			AssertNoExceptionThrown(() => new PutawayTransferLinesAndPackagesToClose(Array.Empty<WhsTransferLine>(), Array.Empty<PkgPackage>()));
		}

		public void TestConstructor_NullArguments()
		{
			AssertExceptionThrown<ArgumentNullException>(() => new PutawayTransferLinesAndPackagesToClose(null, Array.Empty<PkgPackage>()));
			AssertExceptionThrown<ArgumentNullException>(() => new PutawayTransferLinesAndPackagesToClose(Array.Empty<WhsTransferLine>(), null));
		}
	}
}
