using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.Packing.Business;
using Enterprise.Packing.Business.Testing;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Environment.CodeLists;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.Warehouse.Transactions.PickByLabel;
using Enterprise.Warehouse.Transactions.TrolleyPicking.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Warehouse.Web.WebService.Business.Testing
{
	[TestedType(typeof(PickByLabelInfo))]
	public class PickByLabelInfoTestCase : WhsPickJobInfoTestCase<PickByLabelInfo>
	{
		#region TestConstructor

		public void TestConstructor()
		{
			var packingHelper = new PackingTestHelper(Factory);
			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WW_IsPickByUOMEnabled = true;

			var receive1 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 5m);
			var receive2 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 5m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);
			AssertEquals("Precondition.", 2, pick.GetAllPickLines().Count());

			var package1 = order.PackageJob.Packages.AddNew("PLT", "PACKAGE-1");
			var picklines = pick.GetAllPickLines().ToArray();
			packingHelper.CreatePackageDivot(package1, picklines[0]);
			packingHelper.CreatePackageDivot(package1, picklines[1]); // this wont be passed into ctor so will not be included
			Factory.Save();

			var pickByLabelJob = WhsPickByLabelHelper.GetOrCreatePickByLabelJob(Factory, data.Whs1.PK, "AAA", data.Whs1.WW_DefaultOutboundDockDoor);
			var pickByLabelInfo1 = new PickByLabelInfo(data.Whs1, pickByLabelJob, package1, new[] { picklines[0] }, false);
			AssertEquals("Should set PK.", package1.PK, pickByLabelInfo1.PK);
			AssertEquals("Should set Reference.", "PACKAGE-1", pickByLabelInfo1.Reference);
			AssertEquals("Should set IsPickByBiggestPackTypeEnabled.", true, pickByLabelInfo1.IsPickByBiggestPackTypeEnabled);
			AssertEquals("Should set IsPickByUOMTypeEnabled.", true, pickByLabelInfo1.IsPickByUOMTypeEnabled);
			AssertEquals("Should set IsMultiOrder.", false, pickByLabelInfo1.IsMultiOrder);
			AssertEquals("Should set Orders.", order.PK, pickByLabelInfo1.Orders.Single().PK);
			AssertEquals("Should set Lines.", picklines[0].PK, pickByLabelInfo1.Lines.Single().PKs.Single());
			AssertEquals("Should set HasStartedPicking to false.", false, pickByLabelInfo1.HasStartedPicking);

			AssertEquals("Should have added unit conversions.", data.Part1.PK, pickByLabelInfo1.UnitConversionsPerProduct.Single().ProductPK);
			AssertEquals("Should have added unit conversions.", 2, pickByLabelInfo1.UnitConversionsPerProduct.Single().Conversions.Count);
			AssertNotNull(pickByLabelInfo1.UnitConversionsPerProduct.Single().Conversions.Single(c => c.PackType == Constants.PkgUnit.Unit && c.Qty == 1m));
			AssertNotNull(pickByLabelInfo1.UnitConversionsPerProduct.Single().Conversions.Single(c => c.PackType == Constants.PkgUnit.Carton && c.Qty == 12m));

			AssertEquals("Should Populate Products.", "P1", pickByLabelInfo1.ProductInfos.Single().Code);
			AssertEquals("Should populate PartAttributeInfos.", 1, pickByLabelInfo1.ProductPartAttributesInfos.Count);

			// Change Pick by UOM and Pick by Biggest Pack Type
			data.Whs1.WW_IsPickByUOMEnabled = false;
			Factory.Save();

			var branchPK = pick.Warehouse.WW_GB_RelatedCompanyBranch.ToGuid();
			using (WarehouseDataRegistry.Instance.PickByBiggestType.SetTemporaryValue(Guid.Empty, branchPK, Guid.Empty, temporaryValue: false))
			{
				var factory2 = new BusinessObjectFactory { RefreshEnabled = false }; // Must create new Factory as PickByUOM is cached
				var pickByLabelInfo2 = new PickByLabelInfo(factory2.Load<WhsWarehouse>(data.Whs1.PK), pickByLabelJob, factory2.Load<PkgPackage>(package1.PK), new[] { factory2.Load<WhsPickLine>(pick.GetAllPickLines().First().PK) }, false);
				AssertEquals("Should set IsPickByBiggestPackTypeEnabled.", false, pickByLabelInfo2.IsPickByBiggestPackTypeEnabled);
				AssertEquals("Should set IsPickByUOMTypeEnabled.", true, pickByLabelInfo2.IsPickByUOMTypeEnabled);

				var factory3 = new BusinessObjectFactory { RefreshEnabled = false }; // Must create new Factory as PickByUOM is cached
				factory3.Load<WhsPick>(pick.PK).GetAllPickLines().ForEach(pl => pl.WZ_F3_NKAllocatedPackType = ""); // property falls back to this
				var pickByLabelInfo3 = new PickByLabelInfo(factory3.Load<WhsWarehouse>(data.Whs1.PK), pickByLabelJob, factory3.Load<PkgPackage>(package1.PK), new[] { factory3.Load<WhsPickLine>(pick.GetAllPickLines().First().PK) }, false);
				AssertEquals("Should set IsPickByUOMTypeEnabled.", false, pickByLabelInfo3.IsPickByUOMTypeEnabled);
			}
		}

		#endregion

		#region TestIsUsingDirectedPackingConsolidation

		public void TestIsUsingDirectedPackingConsolidation_UsingPackingConsolidation()
		{
			TestIsUsingDirectedPackingConsolidationCore(true);
		}

		public void TestIsUsingDirectedPackingConsolidation_NotUsingPackingConsolidation()
		{
			TestIsUsingDirectedPackingConsolidationCore(false);
		}

		void TestIsUsingDirectedPackingConsolidationCore(bool isUsingDirectedPackingConsolidation)
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var packingHelper = new PackingTestHelper(Factory);
			data.Whs1.WW_IsPickByUOMEnabled = true;

			var receive1 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 5m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 5m);
			order.WD_UseDirectedPackingConsolidation = isUsingDirectedPackingConsolidation;
			var pick = Helper.CreatePickNew(order);

			var package = order.PackageJob.Packages.AddNew("PLT", "PACKAGE-1");
			var pickLines = pick.GetAllPickLines().ToArray();
			packingHelper.CreatePackageDivot(package, pickLines[0]);
			Factory.Save();

			var pickByLabelJob = WhsPickByLabelHelper.GetOrCreatePickByLabelJob(Factory, data.Whs1.PK, "AAA", data.Whs1.WW_DefaultOutboundDockDoor);
			var pickInfo = new PickByLabelInfo(data.Whs1, pickByLabelJob, package, pickLines, false);
			AssertEquals("Should only be using directed packing consolidation if order on pick is.",
				isUsingDirectedPackingConsolidation,
				pickInfo.IsUsingDirectedPackingConsolidation);
		}

		public void TestIsUsingDirectedPackingConsolidation_OrderMustSetFlag()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var packingHelper = new PackingTestHelper(Factory);
			data.Whs1.WW_IsPickByUOMEnabled = true;

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 5m);
			order1.WD_UseDirectedPackingConsolidation = false;

			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part1, 5m);
			order2.WD_UseDirectedPackingConsolidation = true;
			var pick = Helper.CreatePickNew(order1, order2);
			var pickLine = order1.Lines[0].PickLines.Single();

			var package1 = order1.PackageJob.Packages.AddNew("PLT", "PACKAGE-1");
			package1.Pack(order1.Lines[0].ReleaseLines[0], 5m);

			var package2 = order2.PackageJob.Packages.AddNew("PLT", "PACKAGE-2");
			package2.Pack(order2.Lines[0].ReleaseLines[0], 5m);

			Factory.Save();

			var pickByLabelJob = WhsPickByLabelHelper.GetOrCreatePickByLabelJob(Factory, data.Whs1.PK, "AAA", data.Whs1.WW_DefaultOutboundDockDoor);
			var pickInfo = new PickByLabelInfo(data.Whs1, pickByLabelJob, package1, new[] { pickLine }, false);
			AssertEquals("Should only be using directed packing consolidation if order on package is, not based on other orders on pick.",
				false,
				pickInfo.IsUsingDirectedPackingConsolidation);
		}

		protected override void TestIsUsingDirectedPackingConsolidation_PickWithLooseInventoryCore()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var packingHelper = new PackingTestHelper(Factory);

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 5m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part2, 5m);

			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderline1 = Helper.CreateWhsOrderLine(order, data.Part1, 5m);
			var orderline2 = Helper.CreateWhsOrderLine(order, data.Part2, 5m);
			order.WD_UseDirectedPackingConsolidation = true;

			var pick = Helper.CreatePickNew(order);
			var pickLine = orderline1.PickLines.Single();

			var package = order.PackageJob.Packages.AddNew();
			package.Pack(orderline1.ReleaseLines[0], 5m);

			Factory.Save();

			var pickByLabelJob = WhsPickByLabelHelper.GetOrCreatePickByLabelJob(Factory, data.Whs1.PK, "AAA", data.Whs1.WW_DefaultOutboundDockDoor);
			var pickInfo = new PickByLabelInfo(data.Whs1, pickByLabelJob, package, new[] { pickLine }, false);
			AssertEquals("Should use directed packing consolidation if order on pick has loose inventory.",
				false,
				pickInfo.IsUsingDirectedPackingConsolidation);
		}

		protected override void TestIsUsingDirectedPackingConsolidation_PickWithLooseInventoryAlreadyPutawayCore()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var packingHelper = new PackingTestHelper(Factory);

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 5m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part2, 5m);

			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderline1 = Helper.CreateWhsOrderLine(order, data.Part1, 5m);
			var orderline2 = Helper.CreateWhsOrderLine(order, data.Part2, 5m);
			order.WD_UseDirectedPackingConsolidation = true;

			var pick = Helper.CreatePickNew(order);
			var pickLine = orderline1.PickLines.Single();

			var package = order.PackageJob.Packages.AddNew();
			package.Pack(orderline1.ReleaseLines[0], 5m);

			Factory.Save();

			var pickLine2 = orderline2.PickLines.Single();
			var transferLine = Helper.PickAndMakeInTransitTransfer(pickLine2, ZDateTimeOffset.Now);
			transferLine.FinaliseDocketLine();

			Factory.Save();

			var pickByLabelJob = WhsPickByLabelHelper.GetOrCreatePickByLabelJob(Factory, data.Whs1.PK, "AAA", data.Whs1.WW_DefaultOutboundDockDoor);
			var pickInfo = new PickByLabelInfo(data.Whs1, pickByLabelJob, package, new[] { pickLine }, false);
			AssertEquals("Should use directed packing consolidation if order on pick has loose inventory.",
				false,
				pickInfo.IsUsingDirectedPackingConsolidation);
		}

		public void TestIsUsingDirectedPackingConsolidation_WithSomePartsAlreadyPutaway()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 5m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part2, 5m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderline1 = Helper.CreateWhsOrderLine(order, data.Part1, 5m);
			var orderline2 = Helper.CreateWhsOrderLine(order, data.Part2, 5m);
			order.WD_UseDirectedPackingConsolidation = true;

			Helper.CreatePickNew(order);
			var pickLine = orderline1.PickLines.Single();

			var package1 = order.PackageJob.Packages.AddNew();
			package1.Pack(orderline1.ReleaseLines[0], 5m);

			var package2 = order.PackageJob.Packages.AddNew();
			package2.Pack(orderline2.ReleaseLines[0], 5m);
			Factory.Save();

			var pickLine2 = orderline2.PickLines.Single();
			var transferLine = Helper.PickAndMakeInTransitTransfer(pickLine2, ZDateTimeOffset.Now);
			transferLine.FinaliseDocketLine();
			Factory.Save();

			var pickByLabelJob = WhsPickByLabelHelper.GetOrCreatePickByLabelJob(Factory, data.Whs1.PK, "AAA", data.Whs1.WW_DefaultOutboundDockDoor);
			var pickInfo = new PickByLabelInfo(data.Whs1, pickByLabelJob, package1, new[] { pickLine }, false);
			AssertEquals("Should not be using directed packing consolidation if part of the job is already putaway.",
				false,
				pickInfo.IsUsingDirectedPackingConsolidation);
		}

		public void TestIsUsingDirectedPackingConsolidation_WithSiblingPickByLabelJob_WithAssignedPutawayLocation()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 15m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part2, 5m);
			Factory.Save();

			var order1 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderline1 = Helper.CreateWhsOrderLine(order1, data.Part1, 5m);
			var orderline2 = Helper.CreateWhsOrderLine(order1, data.Part2, 5m);

			var order2 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O2");
			var orderLine3 = Helper.CreateWhsOrderLine(order2, data.Part1, 10m);
			order1.WD_UseDirectedPackingConsolidation = true;

			Helper.CreatePickNew(order1, order2);
			var pickLine = orderline1.PickLines.Single();

			var package1 = order1.PackageJob.Packages.AddNew();
			package1.Pack(orderline1.ReleaseLines[0], 5m);

			var package2 = order1.PackageJob.Packages.AddNew();
			package2.Pack(orderline2.ReleaseLines[0], 5m);

			var package3 = order2.PackageJob.Packages.AddNew();
			package3.Pack(orderLine3.ReleaseLines[0], 10m);
			Factory.Save();

			var pickByLabelJob1 = WhsPickByLabelHelper.GetOrCreatePickByLabelJob(Factory, data.Whs1.PK, "AAA", data.Whs1.WW_DefaultOutboundDockDoor);
			WhsPickByLabelHelper.AddPackageToListOfPickByLabelForUser(Helper.Factory, data.Whs1.PK, data.Whs1.WW_DefaultOutboundDockDoor, "AAA", package1.PK);

			var pickByLabelJob2 = WhsPickByLabelHelper.GetOrCreatePickByLabelJob(Helper.Factory, data.Whs1.PK, "BBB", data.Whs1.WW_DefaultOutboundDockDoor);
			WhsPickByLabelHelper.AddPackageToListOfPickByLabelForUser(Helper.Factory, data.Whs1.PK, data.Whs1.WW_DefaultOutboundDockDoor, "BBB", package2.PK);
			WhsPickByLabelHelper.AddPackageToListOfPickByLabelForUser(Helper.Factory, data.Whs1.PK, data.Whs1.WW_DefaultOutboundDockDoor, "BBB", package3.PK);
			pickByLabelJob2.WTK_WL_PutawayLocation = data.Whs1.WW_DefaultOutboundDockDoor;
			Factory.Save();

			var pickInfo = new PickByLabelInfo(data.Whs1, pickByLabelJob1, package1, new[] { pickLine }, false);
			AssertEquals("Should not be using directed packing consolidation if part of the job is already putaway.",
				false,
				pickInfo.IsUsingDirectedPackingConsolidation);
		}

		#endregion

		#region TestConstructor_ScannedRCASerialNumbers

		public void TestConstructor_ScannedRCASerialNumbers()
		{
			var packingHelper = new PackingTestHelper(Factory);
			var data = new TestDataSimpleEnvironment(Factory);

			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true, setReleaseCaptured: true);

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10);
			Factory.Save();

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 2m);
			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part1, 2m);
			Helper.CreatePickNew(order1);
			Helper.CreatePickNew(order2);
			Factory.Save();

			var originalReleaseLine1 = order1.Lines[0].ReleaseLines[0];
			var releaseLine1 = order1.Lines[0].ReleaseLines.AddNew("", "", "", "S1", ZDate.Empty, ZDate.Empty, 1m);
			var releaseLine2 = order1.Lines[0].ReleaseLines.AddNew("", "", "", "S2", ZDate.Empty, ZDate.Empty, 1m);
			originalReleaseLine1.Delete(); // this release line is no longer valid as the Order Line is for 2 units

			var originalReleaseLine2 = order2.Lines[0].ReleaseLines[0];
			var releaseLine3 = order2.Lines[0].ReleaseLines.AddNew("", "", "", "S3", ZDate.Empty, ZDate.Empty, 1m);
			var releaseLine4 = order2.Lines[0].ReleaseLines.AddNew("", "", "", "S4", ZDate.Empty, ZDate.Empty, 1m);
			originalReleaseLine2.Delete(); // this release line is no longer valid as the Order Line is for 2 units

			var package1 = order1.PackageJob.Packages.AddNew("PLT", "PACKAGE-1");
			package1.Pack(releaseLine1, 1);
			package1.Pack(releaseLine2, 1);
			var package2 = order2.PackageJob.Packages.AddNew("PLT", "PACKAGE-2");
			package2.Pack(releaseLine3, 1);
			var package3 = order2.PackageJob.Packages.AddNew("PLT", "PACKAGE-3");
			package3.Pack(releaseLine4, 1);
			Factory.Save();

			var pickByLabelJob = WhsPickByLabelHelper.GetOrCreatePickByLabelJob(Factory, data.Whs1.PK, "AAA", data.Whs1.WW_DefaultOutboundDockDoor);
			WhsPickByLabelHelper.GetOrCreatePickByLabelLabel(pickByLabelJob, package1.PK);
			WhsPickByLabelHelper.GetOrCreatePickByLabelLabel(pickByLabelJob, package2.PK);
			var pickByLabelInfo = new PickByLabelInfo(data.Whs1, pickByLabelJob, package1);

			var scannedRCASerialNumbersForPart1 = pickByLabelInfo.ScannedRCASerialNumbersPerProduct.Single(s => s.ProductPK == data.Part1.PK.ToGuid());
			AssertContainsExactElementsInAnyOrder(new[] { "S1", "S2", "S3", "S4" }, scannedRCASerialNumbersForPart1.ScannedRCASerialNumbers);
		}

		#endregion

		#region TestPickPK

		protected override void TestPickPKsCore()
		{
			AssertContainsExactElementsInAnyOrder(Array.Empty<Guid>(), GetNewPickInfo().PickPKs);

			var data = new TestDataSimpleEnvironment(Factory);
			var packingHelper = new PackingTestHelper(Factory);
			data.Whs1.WW_IsPickByUOMEnabled = true;

			var receive1 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 5m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 5m);
			var pick = Helper.CreatePickNew(order);

			var package = order.PackageJob.Packages.AddNew("PLT", "PACKAGE-1");
			var pickLines = pick.GetAllPickLines().ToArray();
			packingHelper.CreatePackageDivot(package, pickLines[0]);
			Factory.Save();

			var pickByLabelJob = WhsPickByLabelHelper.GetOrCreatePickByLabelJob(Factory, data.Whs1.PK, "AAA", data.Whs1.WW_DefaultOutboundDockDoor);
			var pickInfo2 = new PickByLabelInfo(data.Whs1, pickByLabelJob, package, pickLines, false);
			AssertContainsExactElementsInAnyOrder("PickPK should be set to the Pick's PK.", new[] { pick.PK }, pickInfo2.PickPKs);
		}

		#endregion

		#region TestIsPutawayOnly

		protected override void TestIsPutawayOnlyCore()
		{
			var pickJobInfo1 = GetNewPickInfo();
			AssertEquals("IsPutawayOnly", false, pickJobInfo1.IsPutawayOnly);

			var data = new TestDataSimpleEnvironment(Factory);
			var packingHelper = new PackingTestHelper(Factory);
			data.Whs1.WW_IsPickByUOMEnabled = true;

			var receive1 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 5m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 5m);
			var pick = Helper.CreatePickNew(order);

			var package = order.PackageJob.Packages.AddNew("PLT", "PACKAGE-1");
			var pickLines = pick.GetAllPickLines().ToArray();
			packingHelper.CreatePackageDivot(package, pickLines[0]);
			Factory.Save();

			var pickByLabelJob = WhsPickByLabelHelper.GetOrCreatePickByLabelJob(Factory, data.Whs1.PK, "AAA", data.Whs1.WW_DefaultOutboundDockDoor);
			var pickJobInfo2 = new PickByLabelInfo(data.Whs1, pickByLabelJob, package, pick.GetAllPickLines(), false);
			AssertEquals("IsPutawayOnly", false, pickJobInfo1.IsPutawayOnly);

			var pickJobInfo3 = new PickByLabelInfo(data.Whs1, pickByLabelJob, package);
			AssertEquals("IsPutawayOnly", true, pickJobInfo3.IsPutawayOnly);
			AssertEquals("HasStartedPicking", true, pickJobInfo3.HasStartedPicking);
		}

		#endregion

		#region TestDockDoorLocation

		protected override void TestDockDoorLocationCore()
		{
			var pickInfo1 = GetNewPickInfo();
			AssertEquals("DockDoorLocation", string.Empty, pickInfo1.DockDoorLocation);

			var data = new TestDataSimpleEnvironment(Factory);
			var packingHelper = new PackingTestHelper(Factory);
			data.Whs1.WW_IsPickByUOMEnabled = true;

			var receive1 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 5m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 5m);
			var pick = Helper.CreatePickNew(order);

			var package = order.PackageJob.Packages.AddNew("PLT", "PACKAGE-1");
			var pickLines = pick.GetAllPickLines().ToArray();
			packingHelper.CreatePackageDivot(package, pickLines[0]);
			Factory.Save();

			pick.WP_WL_DockDoor = ZGuid.Empty;
			var pickByLabelJob = WhsPickByLabelHelper.GetOrCreatePickByLabelJob(Factory, data.Whs1.PK, "AAA", data.Whs1.WW_DefaultOutboundDockDoor);
			var pickInfo2 = new PickByLabelInfo(data.Whs1, pickByLabelJob, package, pickLines, false);
			AssertEquals("DockDoorLocation", string.Empty, pickInfo2.DockDoorLocation);

			pick.WP_WL_DockDoor = data.Whs1.WW_DefaultOutboundDockDoor;
			AssertEquals("Precondition", data.Whs1.WW_DefaultOutboundDockDoor, pick.WP_WL_DockDoor);

			var pickJobInfo3 = new PickByLabelInfo(data.Whs1, pickByLabelJob, package, pickLines, false);
			AssertEquals("DockDoorLocation", pick.DockDoorLocation.WLV_LocationString, pickJobInfo3.DockDoorLocation);

			pick.WP_WL_DockDoor = data.Whs1.DefaultLocation.PK;
			AssertNotEquals("Precondition", data.Whs1.WW_DefaultOutboundDockDoor, pick.WP_WL_DockDoor);

			var pickJobInfo4 = new PickByLabelInfo(data.Whs1, pickByLabelJob, package, pickLines, false);
			AssertEquals("DockDoorLocation", pick.DockDoorLocation.WLV_LocationString, pickJobInfo4.DockDoorLocation);
		}

		protected override void TestDockDoorLocation_FixedWidthLocationCore()
		{
			var pickInfo1 = GetNewPickInfo();
			AssertEquals("DockDoorLocation", string.Empty, pickInfo1.DockDoorLocation);

			var data = new TestDataSimpleEnvironment(Factory);
			var packingHelper = new PackingTestHelper(Factory);
			var warehouse = Helper.CreateFixedWidthLocationWarehouse("ZZ", 2, 2, 2);
			Helper.CreateRowAndGenerateLocations(warehouse, "Z", 4, 3, 2);
			warehouse.WW_IsPickByUOMEnabled = true;
			Factory.Save();

			var dockdoorLocationType = Factory.LoadTop1<WhsLocationType>(new ZQuery(WhsLocationTypeSchema.WLT_LocationClass, "DDL"));
			var location1 = warehouse.FindLocation("Z030201");
			location1.WLV_WLT_LocationType = dockdoorLocationType.PK;
			location1.WLV_LocationStatus = "NOR";
			var location2 = warehouse.FindLocation("Z040302");
			location2.WLV_WLT_LocationType = dockdoorLocationType.PK;
			location2.WLV_LocationStatus = "NOR";
			Factory.Save();

			warehouse.WW_DefaultInboundDockDoor = location1.PK;
			warehouse.WW_DefaultOutboundDockDoor = location1.PK;
			Factory.Save();

			Helper.CreateWhsReceiveWithInventory(data.Org1, warehouse, "R1", data.Part1, 5m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, warehouse, "O1", data.Part1, 5m);
			var pick = Helper.CreatePickNew(order);

			var package = order.PackageJob.Packages.AddNew("PLT", "PACKAGE-1");
			var pickLines = pick.GetAllPickLines().ToArray();
			packingHelper.CreatePackageDivot(package, pickLines[0]);
			Factory.Save();

			pick.WP_WL_DockDoor = ZGuid.Empty;
			var pickByLabelJob = WhsPickByLabelHelper.GetOrCreatePickByLabelJob(Factory, warehouse.PK, "AAA", warehouse.WW_DefaultOutboundDockDoor);
			var pickInfo2 = new PickByLabelInfo(data.Whs1, pickByLabelJob, package, pickLines, false);
			AssertEquals("DockDoorLocation", string.Empty, pickInfo2.DockDoorLocation);

			pick.WP_WL_DockDoor = warehouse.WW_DefaultOutboundDockDoor;
			AssertEquals("Precondition", warehouse.WW_DefaultOutboundDockDoor, pick.WP_WL_DockDoor);

			var pickJobInfo3 = new PickByLabelInfo(data.Whs1, pickByLabelJob, package, pickLines, false);
			AssertEquals("DockDoorLocation", "Z030201", pickJobInfo3.DockDoorLocation);
			AssertEquals("DockDoorLocation_UserFriendly", "Z-03-02-01", pickJobInfo3.DockDoorLocation_UserFriendly);

			pick.WP_WL_DockDoor = location2.PK;
			AssertNotEquals("Precondition", warehouse.WW_DefaultOutboundDockDoor, pick.WP_WL_DockDoor);

			var pickJobInfo4 = new PickByLabelInfo(data.Whs1, pickByLabelJob, package, pickLines, false);
			AssertEquals("DockDoorLocation", "Z040302", pickJobInfo4.DockDoorLocation);
			AssertEquals("DockDoorLocation_UserFriendly", "Z-04-03-02", pickJobInfo4.DockDoorLocation_UserFriendly);
		}

		#endregion

		#region TestHasStartedPicking

		public void TestHasStartedPicking()
		{
			var packingHelper = new PackingTestHelper(Factory);
			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WW_IsPickByUOMEnabled = true;

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 6m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 4m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);
			AssertEquals("Precondition.", 2, pick.GetAllPickLines().Count());

			var package1 = order.PackageJob.Packages.AddNew("PLT", "PACKAGE-1");
			var picklines = pick.GetAllPickLines().ToArray();
			var pickLine1 = picklines.Single(pickLine => pickLine.WZ_Units == 6m);
			var pickLine2 = picklines.Single(pickLine => pickLine.WZ_Units == 4m);
			packingHelper.CreatePackageDivot(package1, picklines[0]);
			packingHelper.CreatePackageDivot(package1, picklines[1]);
			Factory.Save();

			var pickByLabelJob = WhsPickByLabelHelper.GetOrCreatePickByLabelJob(Factory, data.Whs1.PK, "AAA", data.Whs1.WW_DefaultOutboundDockDoor);
			var pickByLabelInfo = new PickByLabelInfo(data.Whs1, pickByLabelJob, package1, new[] { pickLine1 }, true);
			AssertEquals("HasStartedPicking is true.", true, pickByLabelInfo.HasStartedPicking);
		}

		#endregion

		#region TestIsUsingCarrierLabelIntegration

		public void TestIsUsingCarrierLabelIntegration()
		{
			var pickByLabelInfo = new PickByLabelInfo();
			AssertEquals("IsUsingCarrierLabelIntegration is false.", false, pickByLabelInfo.IsUsingCarrierLabelIntegration);
			pickByLabelInfo.IsUsingCarrierLabelIntegration = true;
			AssertEquals("IsUsingCarrierLabelIntegration is true.", true, pickByLabelInfo.IsUsingCarrierLabelIntegration);
		}

		#endregion

		#region TestIsPackingStationAllowed

		protected override void TestIsPackingStationAllowedCore(bool hasPackingStationLocation)
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var packingHelper = new PackingTestHelper(Factory);
			data.Whs1.WW_IsPickByUOMEnabled = true;

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 5m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 5m);
			var pick = Helper.CreatePickNew(order);

			var package = order.PackageJob.Packages.AddNew("PLT", "PACKAGE-1");
			var pickLines = pick.GetAllPickLines().ToArray();
			packingHelper.CreatePackageDivot(package, pickLines[0]);
			Factory.Save();

			if (hasPackingStationLocation)
			{
				var packingStationLocationType = Helper.CreateLocationType("XYZ", "Test", false, 0, LocationClasses.Codes.PST);
				var newRow = Helper.CreateRowAndGenerateLocations(data.Whs1, "P");
				var packingLocation = newRow.Locations[0];
				packingLocation.WLV_WLT_LocationType = packingStationLocationType.PK;
				Factory.Save();
			}

			var pickByLabelJob = WhsPickByLabelHelper.GetOrCreatePickByLabelJob(Factory, data.Whs1.PK, "AAA", data.Whs1.WW_DefaultOutboundDockDoor);
			var pickInfo2 = new PickByLabelInfo(data.Whs1, pickByLabelJob, package, pickLines, false);
			AssertEquals("HasPackingStation", hasPackingStationLocation, pickInfo2.IsPackingStationAllowed);
		}

		protected override void TestIsPackingStationAllowed_PackingStationIsInvalidLocationStatusCore(string locationStatus)
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var packingHelper = new PackingTestHelper(Factory);
			data.Whs1.WW_IsPickByUOMEnabled = true;

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 5m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 5m);
			var pick = Helper.CreatePickNew(order);

			var package = order.PackageJob.Packages.AddNew("PLT", "PACKAGE-1");
			var pickLines = pick.GetAllPickLines().ToArray();
			packingHelper.CreatePackageDivot(package, pickLines[0]);
			Factory.Save();

			var packingStationLocationType = Helper.CreateLocationType("XYZ", "Test", false, 0, LocationClasses.Codes.PST);
			var newRow = Helper.CreateRowAndGenerateLocations(data.Whs1, "P");
			var packingLocation = newRow.Locations[0];
			packingLocation.WLV_WLT_LocationType = packingStationLocationType.PK;

			packingLocation.WLV_LocationStatus = locationStatus;
			Factory.Save();

			var pickByLabelJob = WhsPickByLabelHelper.GetOrCreatePickByLabelJob(Factory, data.Whs1.PK, "AAA", data.Whs1.WW_DefaultOutboundDockDoor);
			var pickInfo2 = new PickByLabelInfo(data.Whs1, pickByLabelJob, package, pickLines, false);
			AssertEquals("HasPackingStation", false, pickInfo2.IsPackingStationAllowed);
		}

		protected override void TestIsPackingStationAllowed_PickWithAssignedPackingStationCore()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var packingHelper = new PackingTestHelper(Factory);
			data.Whs1.WW_IsPickByUOMEnabled = true;

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 5m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 5m);
			var pick = Helper.CreatePickNew(order);

			var packingStationLocationType = Helper.CreateLocationType("XYZ", "Test", false, 0, LocationClasses.Codes.PST);
			var newRow = Helper.CreateRowAndGenerateLocations(data.Whs1, "P");
			var packingLocation = newRow.Locations[0];
			packingLocation.WLV_WLT_LocationType = packingStationLocationType.PK;
			pick.WP_WL_PackingStation = packingLocation.PK;

			var package = order.PackageJob.Packages.AddNew("PLT", "PACKAGE-1");
			var pickLines = pick.GetAllPickLines().ToArray();
			packingHelper.CreatePackageDivot(package, pickLines[0]);

			Factory.Save();

			var pickByLabelJob = WhsPickByLabelHelper.GetOrCreatePickByLabelJob(Factory, data.Whs1.PK, "AAA", data.Whs1.WW_DefaultOutboundDockDoor);
			var pickInfo = new PickByLabelInfo(data.Whs1, pickByLabelJob, package, pickLines, false);
			AssertEquals("HasPackingStation", true, pickInfo.IsPackingStationAllowed);
		}

		protected override void TestIsPackingStationAllowed_PickByBOMCore()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateProductBOM(data.Part1, data.Part2);
			data.Part1.OP_IsComponentPickedOnSalesOrder = true;
			var packingHelper = new PackingTestHelper(Factory);
			data.Whs1.WW_IsPickByUOMEnabled = true;

			var packingStationLocationType = Helper.CreateLocationType("XYZ", "Test", false, 0, LocationClasses.Codes.PST);
			var newRow = Helper.CreateRowAndGenerateLocations(data.Whs1, "P");
			var packingLocation = newRow.Locations[0];
			packingLocation.WLV_WLT_LocationType = packingStationLocationType.PK;

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part2, 5m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 5m);
			var pick = Helper.CreatePickNew(order);
			var componentOrderLine = order.Lines.Single(l => l.WE_OP == data.Part2.PK);

			var package = order.PackageJob.Packages.AddNew("PLT", "PACKAGE-1");
			var pickLines = componentOrderLine.PickLines.ToArray();
			packingHelper.CreatePackageDivot(package, pickLines[0]);
			Factory.Save();

			var pickByLabelJob = WhsPickByLabelHelper.GetOrCreatePickByLabelJob(Factory, data.Whs1.PK, "AAA", data.Whs1.WW_DefaultOutboundDockDoor);
			var pickInfo = new PickByLabelInfo(data.Whs1, pickByLabelJob, package, pickLines, false);
			AssertEquals("Pick by BOM can't use Pick by Label in the first place, so the code doesn't check it at all.", true, pickInfo.IsPackingStationAllowed);
		}

		protected override void TestIsPackingStationAllowed_DBHitsCore()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var packingHelper = new PackingTestHelper(Factory);

			var packingStationLocationType = Helper.CreateLocationType("XYZ", "Test", false, 0, LocationClasses.Codes.PST);
			var newRow = Helper.CreateRowAndGenerateLocations(data.Whs1, "P");
			var packingLocation = newRow.Locations[0];
			packingLocation.WLV_WLT_LocationType = packingStationLocationType.PK;

			var count = 10;
			var products = new List<OrgSupplierPart>();

			for (var i = 0; i < count; i++)
			{
				var mainProduct = Helper.CreateProduct(data.Org1, $"P1{i}");

				Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, $"R{i}", mainProduct, 10m);

				products.Add(mainProduct);
			}
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			for (var i = 0; i < count; i++)
			{
				Helper.CreateWhsOrderLine(order, products[i], 20m);
			}

			var pick = Helper.CreatePickNew(order);
			var package = order.PackageJob.Packages.AddNew("PLT", "PACKAGE-1");

			var pickLines = pick.GetAllPickLines().ToArray();
			foreach (var line in pickLines)
			{
				packingHelper.CreatePackageDivot(package, line);
			}
			Factory.Save();

			var pickByLabelJob = WhsPickByLabelHelper.GetOrCreatePickByLabelJob(Factory, data.Whs1.PK, "AAA", data.Whs1.WW_DefaultOutboundDockDoor);
			Factory.Save();

			var expectedDBHits = new Dictionary<string, int>()
			{
				{ OrgHeaderSchema.Constants.TableName, 1 },
				{ OrgMiscServSchema.Constants.TableName, 1 },
				{ OrgSupplierPartSchema.Constants.TableName, 1 },
				{ PkgPackageSchema.Constants.TableName, 1 },
				{ PkgPackageHeaderSchema.Constants.TableName, 1 },
				{ PkgPackageJobSchema.Constants.TableName, 1 },
				{ WhsDocketSchema.Constants.TableName, 4 },
				{ WhsDocketLineSchema.Constants.TableName, 1 },
				{ WhsPickSchema.Constants.TableName, 1 },
				{ WhsPickByLabelJobSchema.Constants.TableName, 1 },
				{ WhsPickLineSchema.Constants.TableName, 1 },
				{ WhsWarehouseSchema.Constants.TableName, 1 }
			};

			var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var whsInNewFactory = newFactory.Load<WhsWarehouse>(data.Whs1.PK);
			var jobInNewFactory = newFactory.Load<WhsPickByLabelJob>(pickByLabelJob.PK);
			var pkgInNewFactory = newFactory.Load<PkgPackage>(package.PK);

			using (AssertDbHitsWithUsefulQueryInformation(expectedDBHits, newFactory, ignoredNotSpecifiedUnlessGreaterThan5Hits: true))
			{
				var pickInfo = new PickByLabelInfo(whsInNewFactory, jobInNewFactory, pkgInNewFactory, pickLines, false);
				AssertEquals("IsPackingStationAllowed on a Pick by Label job is always true.", true, pickInfo.IsPackingStationAllowed);
			}
		}

		#endregion

		#region TestPSTLocationPK

		protected override void TestPackingStationPKCore()
		{
			var pickInfo1 = GetNewPickInfo();
			AssertEquals("No Packing Station for Default", Guid.Empty, pickInfo1.PackingStationPK);

			var data = new TestDataSimpleEnvironment(Factory);
			var packingHelper = new PackingTestHelper(Factory);
			data.Whs1.WW_IsPickByUOMEnabled = true;

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 5m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 5m);
			var pick = Helper.CreatePickNew(order);

			var package = order.PackageJob.Packages.AddNew("PLT", "PACKAGE-1");
			var pickLines = pick.GetAllPickLines().ToArray();
			packingHelper.CreatePackageDivot(package, pickLines[0]);
			Factory.Save();

			var packingStationLocationType = Helper.CreateLocationType("XYZ", "Test", false, 0, LocationClasses.Codes.PST);
			var newRow = Helper.CreateRowAndGenerateLocations(data.Whs1, "P");
			var packingLocation = newRow.Locations[0];
			packingLocation.WLV_WLT_LocationType = packingStationLocationType.PK;
			pick.WP_WL_PackingStation = packingLocation.PK;
			Factory.Save();

			var pickByLabelJob = WhsPickByLabelHelper.GetOrCreatePickByLabelJob(Factory, data.Whs1.PK, "AAA", data.Whs1.WW_DefaultOutboundDockDoor);
			var pickInfo2 = new PickByLabelInfo(data.Whs1, pickByLabelJob, package, pickLines, false);
			AssertEquals("Packing Station PK is Correct", packingLocation.PK.ToGuid(), pickInfo2.PackingStationPK);
		}

		#endregion

		#region TestAssignedPutawayLocationCore

		protected override void TestAssignedPutawayLocation_PackingStationCore()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var packingHelper = new PackingTestHelper(Factory);
			data.Whs1.WW_IsPickByUOMEnabled = true;

			var packingStationLocationType = Helper.CreateLocationType("XYZ", "Test", false, 0, LocationClasses.Codes.PST);
			var newRow = Helper.CreateRowAndGenerateLocations(data.Whs1, "P");
			var packingLocation = newRow.Locations[0];
			packingLocation.WLV_WLT_LocationType = packingStationLocationType.PK;

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part2, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			Helper.CreateWhsOrderLine(order, data.Part2, 10m);
			order.WD_UseDirectedPackingConsolidation = true;
			var pick = Helper.CreatePickNew(order);

			var pickLine1 = order.Lines[0].PickLines.Single();
			var transferLine1 = Helper.PickAndMakeInTransitTransfer(pickLine1, ZDateTimeOffset.Now);
			transferLine1.WE_WL = packingLocation.PK;
			transferLine1.FinaliseDocketLine();

			var pickLine2 = order.Lines[1].PickLines.Single();
			var transferLine2 = Helper.PickAndMakeInTransitTransfer(pickLine2, ZDateTimeOffset.Now);

			var package1 = order.PackageJob.Packages.AddNew("PLT", "PACKAGE-1");
			packingHelper.CreatePackageDivot(package1, pickLine1);

			var package2 = order.PackageJob.Packages.AddNew("PLT", "PACKAGE-2");
			packingHelper.CreatePackageDivot(package2, pickLine2);

			pick.WP_WL_PackingStation = packingLocation.PK;
			Factory.Save();

			var pickByLabelJob = WhsPickByLabelHelper.GetOrCreatePickByLabelJob(Helper.Factory, data.Whs1.PK, "AAA", data.Whs1.WW_DefaultOutboundDockDoor);
			WhsPickByLabelHelper.AddPackageToListOfPickByLabelForUser(Helper.Factory, data.Whs1.PK, data.Whs1.WW_DefaultOutboundDockDoor, "AAA", package2.PK);
			Factory.Save();

			var pickJobInfo = new PickByLabelInfo(data.Whs1, pickByLabelJob, package2, new[] { pickLine2 }, false);
			AssertEquals("Putaway Station Location String is Correct", packingLocation.WLV_LocationString, pickJobInfo.AssignedPutawayLocation);
			AssertEquals("Putaway Station User Friendly Location String is Correct", packingLocation.WLV_LocationString_UserFriendly, pickJobInfo.AssignedPutawayLocation_UserFriendly);
			AssertEquals("Putaway Station Location Class String is Correct", packingLocation.WLV_LocationClass, pickJobInfo.AssignedPutawayLocationClass);
		}

		protected override void TestAssignedPutawayLocation_DockDoorCore()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);

			var dockdoorLocation = data.Whs1.DefaultOutboundDockDoorLocation;
			var packingHelper = new PackingTestHelper(Factory);
			data.Whs1.WW_IsPickByUOMEnabled = true;

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part2, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			Helper.CreateWhsOrderLine(order, data.Part2, 10m);
			order.WD_UseDirectedPackingConsolidation = true;
			var pick = Helper.CreatePickNew(order);

			var pickLine1 = order.Lines[0].PickLines.Single();
			var transferLine1 = Helper.PickAndMakeInTransitTransfer(pickLine1, ZDateTimeOffset.Now);
			transferLine1.WE_WL = dockdoorLocation.PK;
			transferLine1.FinaliseDocketLine();

			var pickLine2 = order.Lines[1].PickLines.Single();
			var transferLine2 = Helper.PickAndMakeInTransitTransfer(pickLine2, ZDateTimeOffset.Now);

			var package1 = order.PackageJob.Packages.AddNew("PLT", "PACKAGE-1");
			package1.Pack(order.Lines[0].ReleaseLines[0], 10m);

			var package2 = order.PackageJob.Packages.AddNew("PLT", "PACKAGE-2");
			package2.Pack(order.Lines[1].ReleaseLines[0], 10m);
			Factory.Save();

			var pickByLabelJob = WhsPickByLabelHelper.GetOrCreatePickByLabelJob(Helper.Factory, data.Whs1.PK, "AAA", data.Whs1.WW_DefaultOutboundDockDoor);
			WhsPickByLabelHelper.AddPackageToListOfPickByLabelForUser(Helper.Factory, data.Whs1.PK, data.Whs1.WW_DefaultOutboundDockDoor, "AAA", package2.PK);
			Factory.Save();

			var pickJobInfo = new PickByLabelInfo(data.Whs1, pickByLabelJob, package2, new[] { pickLine2 }, false);
			AssertEquals("Putaway Station Location String is Correct", dockdoorLocation.WLV_LocationString, pickJobInfo.AssignedPutawayLocation);
			AssertEquals("Putaway Station User Friendly Location String is Correct", dockdoorLocation.WLV_LocationString_UserFriendly, pickJobInfo.AssignedPutawayLocation_UserFriendly);
			AssertEquals("Putaway Station Location Class String is Correct", dockdoorLocation.WLV_LocationClass, pickJobInfo.AssignedPutawayLocationClass);
		}

		protected override void TestAssignedPutawayLocation_ConsolidationLocationCore()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var packingHelper = new PackingTestHelper(Factory);
			data.Whs1.WW_IsPickByUOMEnabled = true;

			var consolidationLocationType = Helper.CreateLocationType("XYZ", "Test", false, 0, LocationClasses.Codes.CON);
			var newRow = Helper.CreateRowAndGenerateLocations(data.Whs1, "P");
			var packingLocation = newRow.Locations[0];
			packingLocation.WLV_WLT_LocationType = consolidationLocationType.PK;

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part2, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			Helper.CreateWhsOrderLine(order, data.Part2, 10m);
			order.WD_UseDirectedPackingConsolidation = true;
			var pick = Helper.CreatePickNew(order);

			var pickLine1 = order.Lines[0].PickLines.Single();
			var transferLine1 = Helper.PickAndMakeInTransitTransfer(pickLine1, ZDateTimeOffset.Now);
			transferLine1.WE_WL = packingLocation.PK;
			transferLine1.FinaliseDocketLine();

			var pickLine2 = order.Lines[1].PickLines.Single();
			var transferLine2 = Helper.PickAndMakeInTransitTransfer(pickLine2, ZDateTimeOffset.Now);

			var package1 = order.PackageJob.Packages.AddNew("PLT", "PACKAGE-1");
			packingHelper.CreatePackageDivot(package1, pickLine1);

			var package2 = order.PackageJob.Packages.AddNew("PLT", "PACKAGE-2");
			packingHelper.CreatePackageDivot(package2, pickLine2);

			Factory.Save();

			var pickByLabelJob = WhsPickByLabelHelper.GetOrCreatePickByLabelJob(Factory, data.Whs1.PK, "AAA", data.Whs1.WW_DefaultOutboundDockDoor);
			var pickJobInfo = new PickByLabelInfo(data.Whs1, pickByLabelJob, package2, new[] { pickLine2 }, false);
			AssertEquals("Putaway Location String is Correct", string.Empty, pickJobInfo.AssignedPutawayLocation);
			AssertEquals("Putaway User Friendly Location String is Correct", string.Empty, pickJobInfo.AssignedPutawayLocation_UserFriendly);
			AssertEquals("Putaway Location Class String is Correct", string.Empty, pickJobInfo.AssignedPutawayLocationClass);
		}

		protected override void TestAssignedPutawayLocation_NothingPutawayYetCore()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);

			var packingHelper = new PackingTestHelper(Factory);
			data.Whs1.WW_IsPickByUOMEnabled = true;

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			order.WD_UseDirectedPackingConsolidation = true;
			var pick = Helper.CreatePickNew(order);

			var pickLine = order.Lines[0].PickLines.Single();
			var transferLine1 = Helper.PickAndMakeInTransitTransfer(pickLine, ZDateTimeOffset.Now);

			var package = order.PackageJob.Packages.AddNew("PLT", "PACKAGE-1");
			packingHelper.CreatePackageDivot(package, pickLine);
			Factory.Save();

			var pickByLabelJob = WhsPickByLabelHelper.GetOrCreatePickByLabelJob(Factory, data.Whs1.PK, "AAA", data.Whs1.WW_DefaultOutboundDockDoor);
			var pickJobInfo = new PickByLabelInfo(data.Whs1, pickByLabelJob, package, new[] { pickLine }, false);
			AssertEquals("Putaway Station Location String is Correct", string.Empty, pickJobInfo.AssignedPutawayLocation);
			AssertEquals("Putaway Station User Friendly Location String is Correct", string.Empty, pickJobInfo.AssignedPutawayLocation_UserFriendly);
			AssertEquals("Putaway Station Location Class String is Correct", string.Empty, pickJobInfo.AssignedPutawayLocationClass);
		}

		public void TestAssignedPutawayLocationClass_NothingPutawayYet_PartOfPickOnToteTrolleyPickingJob_PickByTote()
		{
			TestAssignedPutawayLocationClass_NothingPutawayYet_PartOfPickOnToteTrolleyPickingJobCore(isPickByTote: true);
		}

		public void TestAssignedPutawayLocationClass_NothingPutawayYet_PartOfPickOnToteTrolleyPickingJob_PickByCarton()
		{
			TestAssignedPutawayLocationClass_NothingPutawayYet_PartOfPickOnToteTrolleyPickingJobCore(isPickByTote: false);
		}

		void TestAssignedPutawayLocationClass_NothingPutawayYet_PartOfPickOnToteTrolleyPickingJobCore(bool isPickByTote)
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var packingStationLocationType = Helper.CreateLocationType("XYZ", "Test", false, 0, LocationClasses.Codes.PST);
			var newRow = Helper.CreateRowAndGenerateLocations(data.Whs1, "P");
			var packingLocation = newRow.Locations[0];
			packingLocation.WLV_WLT_LocationType = packingStationLocationType.PK;

			var packingHelper = new PackingTestHelper(Factory);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 15m);
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocket();
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine1 = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var orderLine2 = Helper.CreateWhsOrderLine(order, data.Part2, 15m);
			Helper.CreatePickNew(order);
			var pickLine1 = orderLine1.PickLines.Single();
			var pickLine2 = orderLine2.PickLines.Single();
			pickLine2.WZ_PickedDateTime = ZDateTimeOffset.Today;
			Helper.Factory.Save();

			var pkgJob = PkgPackageJob.LoadOrCreatePackageJob(order);
			var pkg_OnTrolley = packingHelper.CreatePackage(pkgJob, "PKG1", 1, PkgUnit.Box);
			pkg_OnTrolley.SetIsTote(isPickByTote);
			packingHelper.CreatePackageDivot(pkg_OnTrolley, pickLine1);

			var trolley = Helper.CreateTrolley("T001");
			var trolleyJob = Helper.CreateWhsPickTrolleyJob(trolley);
			Helper.CreateWhsPickTrolleySlot(trolleyJob, pkg_OnTrolley, 1);

			var package2 = packingHelper.CreatePackage(pkgJob, "PKG2", 1, PkgUnit.Box);
			packingHelper.CreatePackageDivot(package2, pickLine2);
			Helper.Factory.Save();

			var pickByLabelJob = WhsPickByLabelHelper.GetOrCreatePickByLabelJob(Helper.Factory, data.Whs1.PK, "AAA", data.Whs1.WW_DefaultOutboundDockDoor);
			WhsPickByLabelHelper.AddPackageToListOfPickByLabelForUser(Helper.Factory, data.Whs1.PK, data.Whs1.WW_DefaultOutboundDockDoor, "AAA", package2.PK);
			Factory.Save();

			var pickJobInfo = new PickByLabelInfo(data.Whs1, pickByLabelJob, package2, new[] { pickLine2 }, false);
			AssertEquals("Putaway Station Location String is Correct", string.Empty, pickJobInfo.AssignedPutawayLocation);
			AssertEquals("Putaway Station User Friendly Location String is Correct", string.Empty, pickJobInfo.AssignedPutawayLocation_UserFriendly);
			AssertEquals("Putaway Station Location Class String is Correct", isPickByTote ? LocationClasses.Codes.PST : string.Empty, pickJobInfo.AssignedPutawayLocationClass);
		}

		public void TestAssignedPutawayLocationClass_NothingPutawayYet_PartOfPickOnToteTrolleyPickingJob_NoPackingStations()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var packingHelper = new PackingTestHelper(Factory);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 15m);
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocket();
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine1 = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var orderLine2 = Helper.CreateWhsOrderLine(order, data.Part2, 15m);
			Helper.CreatePickNew(order);
			var pickLine1 = orderLine1.PickLines.Single();
			var pickLine2 = orderLine2.PickLines.Single();
			pickLine2.WZ_PickedDateTime = ZDateTimeOffset.Today;
			Helper.Factory.Save();

			var pkgJob = PkgPackageJob.LoadOrCreatePackageJob(order);
			var pkg_OnTrolley = packingHelper.CreatePackage(pkgJob, "PKG1", 1, PkgUnit.Box);
			pkg_OnTrolley.SetIsTote(true);
			packingHelper.CreatePackageDivot(pkg_OnTrolley, pickLine1);

			var trolley = Helper.CreateTrolley("T001");
			var trolleyJob = Helper.CreateWhsPickTrolleyJob(trolley);
			Helper.CreateWhsPickTrolleySlot(trolleyJob, pkg_OnTrolley, 1);

			var package2 = packingHelper.CreatePackage(pkgJob, "PKG2", 1, PkgUnit.Box);
			packingHelper.CreatePackageDivot(package2, pickLine2);
			Helper.Factory.Save();

			var pickByLabelJob = WhsPickByLabelHelper.GetOrCreatePickByLabelJob(Factory, data.Whs1.PK, "AAA", data.Whs1.WW_DefaultOutboundDockDoor);
			var pickJobInfo = new PickByLabelInfo(data.Whs1, pickByLabelJob, package2, new[] { pickLine2 }, false);
			AssertEquals("Putaway Station Location String is Correct", string.Empty, pickJobInfo.AssignedPutawayLocation);
			AssertEquals("Putaway Station User Friendly Location String is Correct", string.Empty, pickJobInfo.AssignedPutawayLocation_UserFriendly);
			AssertEquals("Putaway Station Location Class String is Correct", string.Empty, pickJobInfo.AssignedPutawayLocationClass);
		}

		public void TestAssignedPutawayLocationClass_PickByLabelJobWithPutawayLocation()
		{
			var packingHelper = new PackingTestHelper(Helper.Factory);
			var data = new TestDataSimpleEnvironment(Helper.Factory);

			var packingStationType = Helper.CreateLocationType("PAK", "Packing", false, 0, LocationClasses.Codes.PST);
			var packingStationLocation = Helper.CreateRowAndGenerateLocations(data.Whs1, "PackingStation", 1, 1).Locations[0];
			packingStationLocation.WLV_WLT_LocationType = packingStationType.PK;
			Helper.Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 50m);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 50m);
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocket();
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			Helper.CreatePickNew(order);

			var pickLine = orderLine.PickLines.Single();
			pickLine.WZ_PickedDateTime = ZDateTimeOffset.Today;
			Helper.Factory.Save();

			var pkgJob = PkgPackageJob.LoadOrCreatePackageJob(order);
			var package = packingHelper.CreatePackage(pkgJob, "PKG1", 1, PkgUnit.Box);
			packingHelper.CreatePackageDivot(package, pickLine);

			var pickByLabelJob = WhsPickByLabelHelper.GetOrCreatePickByLabelJob(Helper.Factory, data.Whs1.PK, "AAA", data.Whs1.WW_DefaultOutboundDockDoor);
			WhsPickByLabelHelper.AddPackageToListOfPickByLabelForUser(Helper.Factory, data.Whs1.PK, data.Whs1.WW_DefaultOutboundDockDoor, "AAA", package.PK);
			pickByLabelJob.WTK_WL_PutawayLocation = packingStationLocation.PK;
			Helper.Factory.Save();

			AssertEquals(packingStationLocation.PK, pickByLabelJob.WTK_WL_PutawayLocation);

			var pickJobInfo = new PickByLabelInfo(data.Whs1, pickByLabelJob, package, new[] { pickLine }, false);
			AssertEquals("Putaway Station Location String is Correct", "PackingStation", pickJobInfo.AssignedPutawayLocation);
			AssertEquals("Putaway Station User Friendly Location String is Correct", "PackingStation", pickJobInfo.AssignedPutawayLocation_UserFriendly);
			AssertEquals("Putaway Station Location Class String is Correct", LocationClasses.Codes.PST, pickJobInfo.AssignedPutawayLocationClass);
		}

		public void TestAssignedPutawayLocationClass_WithSiblingPickByLabelJobsWithPutawayLocation()
		{
			var packingHelper = new PackingTestHelper(Helper.Factory);
			var data = new TestDataSimpleEnvironment(Helper.Factory);

			var packingStationType = Helper.CreateLocationType("PAK", "Packing", false, 0, LocationClasses.Codes.PST);
			var packingStationLocation = Helper.CreateRowAndGenerateLocations(data.Whs1, "PackingStation", 1, 1).Locations[0];
			packingStationLocation.WLV_WLT_LocationType = packingStationType.PK;
			Helper.Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 50m);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 50m);
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocket();
			Helper.Factory.Save();

			var order1 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine1 = Helper.CreateWhsOrderLine(order1, data.Part1, 10m);
			var orderLine2 = Helper.CreateWhsOrderLine(order1, data.Part2, 15m);
			Helper.CreatePickNew(order1);

			var order2 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O2");
			var orderLine3 = Helper.CreateWhsOrderLine(order2, data.Part1, 10m);
			var orderLine4 = Helper.CreateWhsOrderLine(order2, data.Part2, 15m);
			Helper.CreatePickNew(order2);

			var order3 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O3");
			var orderLine5 = Helper.CreateWhsOrderLine(order3, data.Part1, 10m);
			Helper.CreatePickNew(order3);

			var pickLine1 = orderLine1.PickLines.Single();
			pickLine1.WZ_PickedDateTime = ZDateTimeOffset.Today;
			var pickLine2 = orderLine2.PickLines.Single();
			pickLine2.WZ_PickedDateTime = ZDateTimeOffset.Today;
			var pickLine3 = orderLine3.PickLines.Single();
			pickLine3.WZ_PickedDateTime = ZDateTimeOffset.Today;
			var pickLine4 = orderLine4.PickLines.Single();
			pickLine4.WZ_PickedDateTime = ZDateTimeOffset.Today;
			var pickLine5 = orderLine5.PickLines.Single();
			pickLine5.WZ_PickedDateTime = ZDateTimeOffset.Today;
			Helper.Factory.Save();

			var pkgJob1 = PkgPackageJob.LoadOrCreatePackageJob(order1);
			var package1 = packingHelper.CreatePackage(pkgJob1, "PKG1", 1, PkgUnit.Box);
			packingHelper.CreatePackageDivot(package1, pickLine1);

			var pickByLabelJob1 = WhsPickByLabelHelper.GetOrCreatePickByLabelJob(Helper.Factory, data.Whs1.PK, "AAA", data.Whs1.WW_DefaultOutboundDockDoor);
			WhsPickByLabelHelper.AddPackageToListOfPickByLabelForUser(Helper.Factory, data.Whs1.PK, data.Whs1.WW_DefaultOutboundDockDoor, "AAA", package1.PK);
			pickByLabelJob1.WTK_WL_PutawayLocation = packingStationLocation.PK;
			Helper.Factory.Save();

			var package2 = packingHelper.CreatePackage(pkgJob1, "PKG2", 1, PkgUnit.Box);
			packingHelper.CreatePackageDivot(package2, pickLine2);

			var pkgJob2 = PkgPackageJob.LoadOrCreatePackageJob(order2);
			var package3 = packingHelper.CreatePackage(pkgJob2, "PKG3", 1, PkgUnit.Box);
			packingHelper.CreatePackageDivot(package3, pickLine3);

			var pickByLabelJob2 = WhsPickByLabelHelper.GetOrCreatePickByLabelJob(Helper.Factory, data.Whs1.PK, "BBB", data.Whs1.WW_DefaultOutboundDockDoor);
			WhsPickByLabelHelper.AddPackageToListOfPickByLabelForUser(Helper.Factory, data.Whs1.PK, data.Whs1.WW_DefaultOutboundDockDoor, "BBB", package2.PK);
			WhsPickByLabelHelper.AddPackageToListOfPickByLabelForUser(Helper.Factory, data.Whs1.PK, data.Whs1.WW_DefaultOutboundDockDoor, "BBB", package3.PK);

			var package4 = packingHelper.CreatePackage(pkgJob2, "PKG4", 1, PkgUnit.Box);
			packingHelper.CreatePackageDivot(package4, pickLine4);

			var pkgJob3 = PkgPackageJob.LoadOrCreatePackageJob(order3);
			var package5 = packingHelper.CreatePackage(pkgJob3, "PKG5", 1, PkgUnit.Box);
			packingHelper.CreatePackageDivot(package5, pickLine5);

			var pickByLabelJob3 = WhsPickByLabelHelper.GetOrCreatePickByLabelJob(Helper.Factory, data.Whs1.PK, "CCC", data.Whs1.WW_DefaultOutboundDockDoor);
			WhsPickByLabelHelper.AddPackageToListOfPickByLabelForUser(Helper.Factory, data.Whs1.PK, data.Whs1.WW_DefaultOutboundDockDoor, "CCC", package4.PK);
			WhsPickByLabelHelper.AddPackageToListOfPickByLabelForUser(Helper.Factory, data.Whs1.PK, data.Whs1.WW_DefaultOutboundDockDoor, "CCC", package5.PK);
			Helper.Factory.Save();

			AssertEquals(packingStationLocation.PK, pickByLabelJob1.WTK_WL_PutawayLocation);
			AssertEquals(ZGuid.Empty, pickByLabelJob2.WTK_WL_PutawayLocation);
			AssertEquals(ZGuid.Empty, pickByLabelJob3.WTK_WL_PutawayLocation);

			var pickJobInfo = new PickByLabelInfo(data.Whs1, pickByLabelJob3, package5, new[] { pickLine5 }, false);
			AssertEquals("Putaway Station Location String is Correct", "PackingStation", pickJobInfo.AssignedPutawayLocation);
			AssertEquals("Putaway Station User Friendly Location String is Correct", "PackingStation", pickJobInfo.AssignedPutawayLocation_UserFriendly);
			AssertEquals("Putaway Station Location Class String is Correct", LocationClasses.Codes.PST, pickJobInfo.AssignedPutawayLocationClass);
		}

		#endregion

		#region TestAllowPickDockDoorLocationOverride

		protected override void TestAllowPickDockDoorLocationOverrideCore()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var packingHelper = new PackingTestHelper(Factory);
			data.Whs1.WW_IsPickByUOMEnabled = true;

			var pickPackParam = Helper.CreatePickPackParameter(data.Org1, data.Whs1);
			pickPackParam.WPP_AllowPickDockDoorLocationOverride = false;

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 5m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 5m);
			var pick = Helper.CreatePickNew(order);

			var package = order.PackageJob.Packages.AddNew("PLT", "PACKAGE-1");
			var pickLines = pick.GetAllPickLines().ToArray();
			packingHelper.CreatePackageDivot(package, pickLines[0]);
			Factory.Save();

			var pickByLabelJob = WhsPickByLabelHelper.GetOrCreatePickByLabelJob(Factory, data.Whs1.PK, "AAA", data.Whs1.WW_DefaultOutboundDockDoor);
			WhsPickByLabelHelper.GetOrCreatePickByLabelLabel(pickByLabelJob, package.PK);
			var pickInfo1 = new PickByLabelInfo(data.Whs1, pickByLabelJob, package, pickLines, false);
			AssertEquals("AllowPickDockDoorLocationOverride should be false", false, pickInfo1.AllowPickDockDoorLocationOverride);

			pickPackParam.WPP_AllowPickDockDoorLocationOverride = true;
			Factory.Save();

			var pickInfo2 = new PickByLabelInfo(data.Whs1, pickByLabelJob, package, pickLines, false);
			AssertEquals("AllowPickDockDoorLocationOverride should be true", true, pickInfo2.AllowPickDockDoorLocationOverride);
		}

		protected override void TestAllowPickDockDoorLocationOverride_LinkedPicksCore()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var packingHelper = new PackingTestHelper(Factory);

			var pickPackParam1 = Helper.CreatePickPackParameter(data.Org1, data.Whs1);
			pickPackParam1.WPP_AllowPickDockDoorLocationOverride = false;

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 20m);

			var client2 = Helper.CreateClient("CL2");
			Helper.CreateProductClientRelationShip(client2, data.Part2);

			var pickPackParam2 = Helper.CreatePickPackParameter(client2, data.Whs1);
			pickPackParam2.WPP_AllowPickDockDoorLocationOverride = false;

			Helper.CreateWhsReceiveWithInventory(client2, data.Whs1, "R2", data.Part2, 20m);
			Factory.Save();

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 20m);
			var pick1 = Helper.CreatePickNew(order1);

			var order2 = Helper.CreateWhsOrderWithOrderLine(client2, data.Whs1, data.Part2, 20m);
			var pick2 = Helper.CreatePickNew(order2);

			var package1 = order1.PackageJob.Packages.AddNew();
			package1.KP_PackageID = "P01";
			package1.Pack(order1.Lines[0].ReleaseLines[0], 20m);

			var package2 = order2.PackageJob.Packages.AddNew();
			package2.KP_PackageID = "P02";
			package2.Pack(order2.Lines[0].ReleaseLines[0], 20m);

			var pickByLabelJob = WhsPickByLabelHelper.GetOrCreatePickByLabelJob(Factory, data.Whs1.PK, "AAA", data.Whs1.WW_DefaultOutboundDockDoor);
			WhsPickByLabelHelper.GetOrCreatePickByLabelLabel(pickByLabelJob, package1.PK);
			WhsPickByLabelHelper.GetOrCreatePickByLabelLabel(pickByLabelJob, package2.PK);

			var dda = Helper.CreateWhsDockDoorAssignment(data.Whs1.DefaultOutboundDockDoorLocation, pick1, pick2);
			Factory.Save();

			var pick1Lines = pick1.GetAllPickLines();
			var pick2Lines = pick2.GetAllPickLines();

			var pickJobInfo11 = new PickByLabelInfo(data.Whs1, pickByLabelJob, package1, pick1Lines, false);
			AssertEquals("AllowPickDockDoorLocationOverride pick1", false, pickJobInfo11.AllowPickDockDoorLocationOverride);

			var pickJobInfo21 = new PickByLabelInfo(data.Whs1, pickByLabelJob, package2, pick2Lines, false);
			AssertEquals("AllowPickDockDoorLocationOverride pick2", false, pickJobInfo21.AllowPickDockDoorLocationOverride);

			// Enable 1 param
			pickPackParam1.WPP_AllowPickDockDoorLocationOverride = true;
			Factory.Save();

			var pickJobInfo12 = new PickByLabelInfo(data.Whs1, pickByLabelJob, package1, pick1Lines, false);
			AssertEquals("AllowPickDockDoorLocationOverride pick1", false, pickJobInfo12.AllowPickDockDoorLocationOverride);

			var pickJobInfo22 = new PickByLabelInfo(data.Whs1, pickByLabelJob, package2, pick2Lines, false);
			AssertEquals("AllowPickDockDoorLocationOverride pick2", false, pickJobInfo22.AllowPickDockDoorLocationOverride);

			// Enable both params
			pickPackParam2.WPP_AllowPickDockDoorLocationOverride = true;
			Factory.Save();

			var pickJobInfo13 = new PickByLabelInfo(data.Whs1, pickByLabelJob, package1, pick1Lines, false);
			AssertEquals("AllowPickDockDoorLocationOverride pick1", true, pickJobInfo13.AllowPickDockDoorLocationOverride);

			var pickJobInfo23 = new PickByLabelInfo(data.Whs1, pickByLabelJob, package2, pick2Lines, false);
			AssertEquals("AllowPickDockDoorLocationOverride pick2", true, pickJobInfo23.AllowPickDockDoorLocationOverride);

			dda.WDA_FirstPutawayToDockDoorUtc = ZDateTime.UtcNow;
			Factory.Save();

			var pickJobInfo14 = new PickByLabelInfo(data.Whs1, pickByLabelJob, package1, pick1Lines, false);
			AssertEquals("AllowPickDockDoorLocationOverride pick1, false if DDA WDA_FirstPutawayToDockDoorUtc set",
				false, pickJobInfo14.AllowPickDockDoorLocationOverride);

			var pickJobInfo24 = new PickByLabelInfo(data.Whs1, pickByLabelJob, package2, pick2Lines, false);
			AssertEquals("AllowPickDockDoorLocationOverride pick2, false if DDA WDA_FirstPutawayToDockDoorUtc set",
				false, pickJobInfo24.AllowPickDockDoorLocationOverride);
		}

		#endregion

		#region Implementation

		protected override PickByLabelInfo GetNewPickInfo()
		{
			return new PickByLabelInfo();
		}

		#endregion
	}
}
