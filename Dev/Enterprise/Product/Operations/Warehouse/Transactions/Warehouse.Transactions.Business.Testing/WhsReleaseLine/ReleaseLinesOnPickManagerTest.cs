using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.Packing.Business;
using Enterprise.Warehouse.Environment.CodeLists;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.DataMapping;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	class ReleaseLinesOnPickManagerTest : WhsTestCaseWithFactory
	{
		#region TestGetUnreleasedQuantityAndBuildCacheIfNeeded

		#region TestGetUnreleasedQuantityAndBuildCacheIfNeeded

		public void TestGetUnreleasedQuantityAndBuildCacheIfNeeded()
		{
			TestGetUnreleasedQuantityAndBuildCacheIfNeeded(ReleaseLinesOnPickManager.GetUnreleasedQuantityAndBuildCacheIfNeeded, assertChecksInputs: true);
		}

		public void TestGetUnreleasedQuantityAndBuildCacheIfNeeded_WhenPassingAttribs()
		{
			TestGetUnreleasedQuantityAndBuildCacheIfNeeded((f, rl, ol) => ReleaseLinesOnPickManager.GetUnreleasedQuantityAndBuildCacheIfNeeded(ol, AttributeParts.New(rl).Key));
		}

		void TestGetUnreleasedQuantityAndBuildCacheIfNeeded(Func<BusinessObjectFactory, WhsReleaseLine, WhsPickableDocketLine, ZDecimal> getUnreleasedQuantity, bool assertChecksInputs = false)
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var org2 = Helper.CreateClient("O2");
			Helper.CreateProductClientRelationShip(org2, data.Part1);

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 20m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part2, 10m);
			Helper.CreateWhsReceiveWithInventory(org2, data.Whs1, "R3", data.Part1, 10m);
			Factory.Save();

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 10m);
			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part1, 5m);
			var order1Line1 = order1.Lines[0];
			var order1Line2 = Helper.CreateWhsOrderLine(order1, data.Part2, 10m);
			var order2Line1 = order2.Lines[0];
			var order2Line2 = Helper.CreateWhsOrderLine(order2, data.Part1, 5m);

			var order3 = Helper.CreateWhsOrderWithOrderLine(org2, data.Whs1, "O3", data.Part1, 10m);
			var order3Line = order3.Lines[0];

			var pick = Helper.CreatePickNew(order1, order2, order3);
			AssertEquals("Precondition", true, pick.GetAllPickLines().Any());
			AssertEquals("Precondition", true, order1Line1.ReleaseLines.Count > 0);
			AssertEquals("Should all have same unreleased quantity.", 0m, getUnreleasedQuantity(Factory, order1Line1.ReleaseLines[0], order1Line1));
			AssertEquals("Should all have same unreleased quantity.", 0m, getUnreleasedQuantity(Factory, order1Line2.ReleaseLines[0], order1Line2));
			AssertEquals("Should all have same unreleased quantity.", 0m, getUnreleasedQuantity(Factory, order2Line1.ReleaseLines[0], order2Line1));
			AssertEquals("Should all have same unreleased quantity.", 0m, getUnreleasedQuantity(Factory, order2Line2.ReleaseLines[0], order2Line2));
			AssertEquals("Should all have same unreleased quantity.", 0m, getUnreleasedQuantity(Factory, order3Line.ReleaseLines[0], order3Line));
			AssertEquals("Should all have same unreleased quantity.", 0m, order1Line1.ReleaseLines[0].UnreleasedQty);
			AssertEquals("Should all have same unreleased quantity.", 0m, order1Line2.ReleaseLines[0].UnreleasedQty);
			AssertEquals("Should all have same unreleased quantity.", 0m, order2Line1.ReleaseLines[0].UnreleasedQty);
			AssertEquals("Should all have same unreleased quantity.", 0m, order2Line2.ReleaseLines[0].UnreleasedQty);
			AssertEquals("Should all have same unreleased quantity.", 0m, order3Line.ReleaseLines[0].UnreleasedQty);

			order1Line1.ReleaseLines[0].Quantity = 0m;
			AssertEquals("Should all have same unreleased quantity.", 10m, getUnreleasedQuantity(Factory, order1Line1.ReleaseLines[0], order1Line1));
			AssertEquals("Should all have same unreleased quantity.", 10m, getUnreleasedQuantity(Factory, order2Line1.ReleaseLines[0], order2Line1));
			AssertEquals("Should all have same unreleased quantity.", 10m, getUnreleasedQuantity(Factory, order2Line2.ReleaseLines[0], order2Line2));
			AssertEquals("Should *not* have same unreleased quantity.", 0m, getUnreleasedQuantity(Factory, order1Line2.ReleaseLines[0], order1Line2));
			AssertEquals("Should *not* have same unreleased quantity.", 0m, getUnreleasedQuantity(Factory, order3Line.ReleaseLines[0], order3Line));
			AssertEquals("Should all have same unreleased quantity.", 10m, order1Line1.ReleaseLines[0].UnreleasedQty);
			AssertEquals("Should all have same unreleased quantity.", 10m, order2Line1.ReleaseLines[0].UnreleasedQty);
			AssertEquals("Should all have same unreleased quantity.", 10m, order2Line2.ReleaseLines[0].UnreleasedQty);
			AssertEquals("Should *not* have same unreleased quantity.", 10m, order1Line1.ReleaseLines[0].UnreleasedQty);
			AssertEquals("Should *not* have same unreleased quantity.", 0m, order3Line.ReleaseLines[0].UnreleasedQty);

			order2Line1.ReleaseLines[0].Quantity = 0m;
			AssertEquals("Should all have same unreleased quantity.", 15m, getUnreleasedQuantity(Factory, order1Line1.ReleaseLines[0], order1Line1));
			AssertEquals("Should all have same unreleased quantity.", 15m, getUnreleasedQuantity(Factory, order2Line1.ReleaseLines[0], order2Line1));
			AssertEquals("Should all have same unreleased quantity.", 15m, getUnreleasedQuantity(Factory, order2Line2.ReleaseLines[0], order2Line2));
			AssertEquals("Should *not* have same unreleased quantity.", 0m, getUnreleasedQuantity(Factory, order3Line.ReleaseLines[0], order3Line));
			AssertEquals("Should all have same unreleased quantity.", 15m, order1Line1.ReleaseLines[0].UnreleasedQty);
			AssertEquals("Should all have same unreleased quantity.", 15m, order2Line1.ReleaseLines[0].UnreleasedQty);
			AssertEquals("Should all have same unreleased quantity.", 15m, order2Line2.ReleaseLines[0].UnreleasedQty);
			AssertEquals("Should *not* have same unreleased quantity.", 0m, order3Line.ReleaseLines[0].UnreleasedQty);

			if (assertChecksInputs)
			{
				AssertExceptionThrown<InvalidOperationException>("Should blowup with dodgy inputs", () => getUnreleasedQuantity(Factory, order1Line1.ReleaseLines[0], order2Line1));
				AssertExceptionThrown<InvalidOperationException>("Should blowup with dodgy inputs", () => getUnreleasedQuantity(Factory, order2Line1.ReleaseLines[0], order1Line1));
				AssertExceptionThrown<InvalidOperationException>("Should blowup with dodgy inputs", () => getUnreleasedQuantity(Factory, order2Line2.ReleaseLines[0], order1Line1));
				AssertExceptionThrown<InvalidOperationException>("Should blowup with dodgy inputs", () => getUnreleasedQuantity(Factory, order2Line2.ReleaseLines[0], order2Line1));
			}
		}

		#endregion

		#region TestGetUnreleasedQuantityAndBuildCacheIfNeeded_AcrossMultiplePicks

		public void TestGetUnreleasedQuantityAndBuildCacheIfNeeded_AcrossMultiplePicks()
		{
			TestGetUnreleasedQuantityAndBuildCacheIfNeeded_AcrossMultiplePicks(ReleaseLinesOnPickManager.GetUnreleasedQuantityAndBuildCacheIfNeeded);
		}

		public void TestGetUnreleasedQuantityAndBuildCacheIfNeeded_AcrossMultiplePicks_WhenPassingAttribs()
		{
			TestGetUnreleasedQuantityAndBuildCacheIfNeeded_AcrossMultiplePicks((f, rl, ol) => ReleaseLinesOnPickManager.GetUnreleasedQuantityAndBuildCacheIfNeeded(ol, AttributeParts.New(rl).Key));
		}

		public void TestGetUnreleasedQuantityAndBuildCacheIfNeeded_AcrossMultiplePicks(Func<BusinessObjectFactory, WhsReleaseLine, WhsPickableDocketLine, ZDecimal> getUnreleasedQuantity)
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 100m);
			Factory.Save();

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part1, 10m);
			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part1, 5m);
			var order1Line = order1.Lines[0];
			var order2Line = order2.Lines[0];

			var pick1 = Helper.CreatePickNew(order1);
			var pick2 = Helper.CreatePickNew(order2);

			AssertEquals("Precondition", true, pick1.GetAllPickLines().Any());
			AssertEquals("Precondition", true, pick2.GetAllPickLines().Any());
			AssertEquals("Should be fully released.", 0m, getUnreleasedQuantity(Factory, order1Line.ReleaseLines[0], order1Line));
			AssertEquals("Should be fully released.", 0m, getUnreleasedQuantity(Factory, order2Line.ReleaseLines[0], order2Line));

			order1Line.ReleaseLines[0].Quantity = 0m;
			AssertEquals("Should be 10m left to allocate", 10m, getUnreleasedQuantity(Factory, order1Line.ReleaseLines[0], order1Line));
			AssertEquals("Should be independent of the other picks release lines", 0m, getUnreleasedQuantity(Factory, order2Line.ReleaseLines[0], order2Line));

			order2Line.ReleaseLines[0].Quantity -= 2m;
			AssertEquals("Should be independent of the other picks release lines", 10m, getUnreleasedQuantity(Factory, order1Line.ReleaseLines[0], order1Line));
			AssertEquals("Should be independent of the other picks release lines", 2m, getUnreleasedQuantity(Factory, order2Line.ReleaseLines[0], order2Line));
			AssertEquals("Should all have same unreleased quantity.", 10m, order1Line.ReleaseLines[0].UnreleasedQty);
			AssertEquals("Should all have same unreleased quantity.", 2m, order2Line.ReleaseLines[0].UnreleasedQty);
		}

		#endregion

		#region TestGetUnreleasedQuantityAndBuildCacheIfNeeded_WithPartAttributes

		#region TestGetUnreleasedQuantityAndBuildCacheIfNeeded_WithPartAttributes_PartAttrib1

		public void TestGetUnreleasedQuantityAndBuildCacheIfNeeded_WithPartAttributes_PartAttrib1()
		{
			TestGetUnreleasedQuantityAndBuildCacheIfNeeded_WithPartAttributes_Core((inv, pa) => inv.WE_PartAttrib1 = pa);
		}

		#endregion

		#region TestGetUnreleasedQuantityAndBuildCacheIfNeeded_WithPartAttributes_PartAttrib2

		public void TestGetUnreleasedQuantityAndBuildCacheIfNeeded_WithPartAttributes_PartAttrib2()
		{
			TestGetUnreleasedQuantityAndBuildCacheIfNeeded_WithPartAttributes_Core((inv, pa) => inv.WE_PartAttrib2 = pa);
		}

		#endregion

		#region TestGetUnreleasedQuantityAndBuildCacheIfNeeded_WithPartAttributes_PartAttrib3

		public void TestGetUnreleasedQuantityAndBuildCacheIfNeeded_WithPartAttributes_PartAttrib3()
		{
			TestGetUnreleasedQuantityAndBuildCacheIfNeeded_WithPartAttributes_Core((inv, pa) => inv.WE_PartAttrib3 = pa);
		}

		#endregion

		#region TestGetUnreleasedQuantityAndBuildCacheIfNeeded_WithPartAttributes_SerialNumber

		public void TestGetUnreleasedQuantityAndBuildCacheIfNeeded_WithPartAttributes_SerialNumber()
		{
			var today = ZDate.Today;
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAllAttributeType(data.Org1, true);
			Helper.SetProductAllAttributeUse(data.Org1, data.Part1, true, useSerialNumber: true);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, today.AddDays(10), today.AddDays(-10), "PA1", "PA2", "PA3", "BEK-1");
			inventory1.WI_SerialNumber = "SN1";
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, today.AddDays(10), today.AddDays(-10), "PA1", "PA2", "PA3", "BEK-2");
			inventory2.WI_SerialNumber = "SN2";
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 2m);
			var orderLine = order.Lines[0];
			var pick = Helper.CreatePickNew(order);
			AssertEquals("Precondition", 2, pick.GetAllPickLines().Count());
			AssertEquals("Precondition", 2, orderLine.ReleaseLines.Count);

			var releaseLine1 = orderLine.ReleaseLines[0];
			var releaseLine2 = orderLine.ReleaseLines[1];
			AssertNotEquals("Precondition", releaseLine1.PartAttribute1 + releaseLine1.PartAttribute2 + releaseLine1.PartAttribute3 + releaseLine1.SerialNumber, releaseLine2.PartAttribute1 + releaseLine2.PartAttribute2 + releaseLine2.PartAttribute3 + releaseLine2.SerialNumber);
			AssertEquals("Precondition.", 0m, ReleaseLinesOnPickManager.GetUnreleasedQuantityAndBuildCacheIfNeeded(Factory, releaseLine1, orderLine));
			AssertEquals("Precondition.", 0m, ReleaseLinesOnPickManager.GetUnreleasedQuantityAndBuildCacheIfNeeded(Factory, releaseLine2, orderLine));
			AssertEquals("Precondition.", 0m, releaseLine1.UnreleasedQty);
			AssertEquals("Precondition.", 0m, releaseLine2.UnreleasedQty);

			releaseLine1.Quantity = 0m;
			AssertEquals("Should *not* have same unreleased quantity.", 1m, ReleaseLinesOnPickManager.GetUnreleasedQuantityAndBuildCacheIfNeeded(Factory, releaseLine1, orderLine));
			AssertEquals("Should *not* have same unreleased quantity.", 0m, ReleaseLinesOnPickManager.GetUnreleasedQuantityAndBuildCacheIfNeeded(Factory, releaseLine2, orderLine));
			AssertEquals("Precondition.", 1m, releaseLine1.UnreleasedQty);
			AssertEquals("Precondition.", 0m, releaseLine2.UnreleasedQty);
		}

		#endregion

		#region TestGetUnreleasedQuantityAndBuildCacheIfNeeded_WithPartAttributes_Core

		void TestGetUnreleasedQuantityAndBuildCacheIfNeeded_WithPartAttributes_Core(Action<WhsDocketLine, string> partAttribSetter)
		{
			var today = ZDate.Today;
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAllAttributeType(data.Org1, true);
			Helper.SetProductAllAttributeUse(data.Org1, data.Part1, true, useSerialNumber: false);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, today.AddDays(10), today.AddDays(-10), "PA1", "PA2", "PA3", "BEK-1");
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, today.AddDays(10), today.AddDays(-10), "PA1", "PA2", "PA3", "BEK-2");
			partAttribSetter(inventory2.InDocketLine, "OTHER");
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 20m);
			var order1Line1 = order1.Lines[0];
			var pick = Helper.CreatePickNew(order1);
			AssertEquals("Precondition", 2, pick.GetAllPickLines().Count());
			AssertEquals("Precondition", 2, order1Line1.ReleaseLines.Count);

			var releaseLine1 = order1Line1.ReleaseLines[0];
			var releaseLine2 = order1Line1.ReleaseLines[1];
			AssertNotEquals("Precondition", releaseLine1.PartAttribute1 + releaseLine1.PartAttribute2 + releaseLine1.PartAttribute3, releaseLine2.PartAttribute1 + releaseLine2.PartAttribute2 + releaseLine2.PartAttribute3);
			AssertEquals("Precondition.", 0m, ReleaseLinesOnPickManager.GetUnreleasedQuantityAndBuildCacheIfNeeded(Factory, releaseLine1, order1Line1));
			AssertEquals("Precondition.", 0m, ReleaseLinesOnPickManager.GetUnreleasedQuantityAndBuildCacheIfNeeded(Factory, releaseLine2, order1Line1));
			AssertEquals("Precondition.", 0m, releaseLine1.UnreleasedQty);
			AssertEquals("Precondition.", 0m, releaseLine2.UnreleasedQty);

			releaseLine1.Quantity = 0m;
			AssertEquals("Should *not* have same unreleased quantity.", 10m, ReleaseLinesOnPickManager.GetUnreleasedQuantityAndBuildCacheIfNeeded(Factory, releaseLine1, order1Line1));
			AssertEquals("Should *not* have same unreleased quantity.", 0m, ReleaseLinesOnPickManager.GetUnreleasedQuantityAndBuildCacheIfNeeded(Factory, releaseLine2, order1Line1));
			AssertEquals("Precondition.", 10m, releaseLine1.UnreleasedQty);
			AssertEquals("Precondition.", 0m, releaseLine2.UnreleasedQty);

			releaseLine2.Quantity -= 2m;
			AssertEquals("Should *not* have same unreleased quantity.", 10m, ReleaseLinesOnPickManager.GetUnreleasedQuantityAndBuildCacheIfNeeded(Factory, releaseLine1, order1Line1));
			AssertEquals("Should *not* have same unreleased quantity.", 2m, ReleaseLinesOnPickManager.GetUnreleasedQuantityAndBuildCacheIfNeeded(Factory, releaseLine2, order1Line1));
			AssertEquals("Precondition.", 10m, releaseLine1.UnreleasedQty);
			AssertEquals("Precondition.", 2m, releaseLine2.UnreleasedQty);
		}

		#endregion

		#region TestGetUnreleasedQuantityAndBuildCacheIfNeeded_WithPartAttributes_SwappedOrder

		public void TestGetUnreleasedQuantityAndBuildCacheIfNeeded_WithPartAttributes_SwappedOrder()
		{
			TestGetUnreleasedQuantityAndBuildCacheIfNeeded_WithPartAttributes_SwappedOrder(ReleaseLinesOnPickManager.GetUnreleasedQuantityAndBuildCacheIfNeeded);
		}

		public void TestGetUnreleasedQuantityAndBuildCacheIfNeeded_WithPartAttributes_SwappedOrder_WhenPassingAttribs()
		{
			TestGetUnreleasedQuantityAndBuildCacheIfNeeded_AcrossMultiplePicks((f, rl, ol) => ReleaseLinesOnPickManager.GetUnreleasedQuantityAndBuildCacheIfNeeded(ol, AttributeParts.New(rl).Key));
		}

		void TestGetUnreleasedQuantityAndBuildCacheIfNeeded_WithPartAttributes_SwappedOrder(Func<BusinessObjectFactory, WhsReleaseLine, WhsPickableDocketLine, ZDecimal> getUnreleasedQuantity)
		{
			var today = ZDate.Today;
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAllAttributeType(data.Org1, true);
			Helper.SetProductAllAttributeUse(data.Org1, data.Part1, true, useSerialNumber: false);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, today.AddDays(10), today.AddDays(-10), "PA1", "PA2", "PA3", "BEK-1");
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, today.AddDays(10), today.AddDays(-10), "PA3", "PA2", "PA1", "BEK-2");
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 20m);
			var order1Line1 = order1.Lines[0];
			var pick = Helper.CreatePickNew(order1);
			AssertEquals("Precondition", 2, pick.GetAllPickLines().Count());
			AssertEquals("Precondition", 2, order1Line1.ReleaseLines.Count);

			var releaseLine1 = order1Line1.ReleaseLines[0];
			var releaseLine2 = order1Line1.ReleaseLines[1];
			AssertEquals("Precondition.", 0m, getUnreleasedQuantity(Factory, releaseLine1, order1Line1));
			AssertEquals("Precondition.", 0m, getUnreleasedQuantity(Factory, releaseLine2, order1Line1));
			AssertEquals("Precondition.", 0m, releaseLine1.UnreleasedQty);
			AssertEquals("Precondition.", 0m, releaseLine2.UnreleasedQty);

			releaseLine1.Quantity = 0m;
			AssertEquals("Should *not* have same unreleased quantity.", 10m, getUnreleasedQuantity(Factory, releaseLine1, order1Line1));
			AssertEquals("Should *not* have same unreleased quantity.", 0m, getUnreleasedQuantity(Factory, releaseLine2, order1Line1));
			AssertEquals("Precondition.", 10m, releaseLine1.UnreleasedQty);
			AssertEquals("Precondition.", 0m, releaseLine2.UnreleasedQty);
		}

		#endregion

		#endregion

		#region TestGetUnreleasedQuantityAndBuildCacheIfNeeded_WithPartAttributes_ReleaseCaptured

		#region TestGetUnreleasedQuantityAndBuildCacheIfNeeded_WithPartAttributes_ReleaseCaptured_PartAttrib1

		public void TestGetUnreleasedQuantityAndBuildCacheIfNeeded_WithPartAttributes_ReleaseCaptured_PartAttrib1()
		{
			TestGetUnreleasedQuantityAndBuildCacheIfNeeded_WithPartAttributes_ReleaseCaptured_Core((rl, pa) => rl.PartAttribute1 = pa);
		}

		#endregion

		#region TestGetUnreleasedQuantityAndBuildCacheIfNeeded_WithPartAttributes_ReleaseCaptured_PartAttrib2

		public void TestGetUnreleasedQuantityAndBuildCacheIfNeeded_WithPartAttributes_ReleaseCaptured_PartAttrib2()
		{
			TestGetUnreleasedQuantityAndBuildCacheIfNeeded_WithPartAttributes_ReleaseCaptured_Core((rl, pa) => rl.PartAttribute2 = pa);
		}

		#endregion

		#region TestGetUnreleasedQuantityAndBuildCacheIfNeeded_WithPartAttributes_ReleaseCaptured_PartAttrib3

		public void TestGetUnreleasedQuantityAndBuildCacheIfNeeded_WithPartAttributes_ReleaseCaptured_PartAttrib3()
		{
			TestGetUnreleasedQuantityAndBuildCacheIfNeeded_WithPartAttributes_ReleaseCaptured_Core((rl, pa) => rl.PartAttribute3 = pa);
		}

		#endregion

		#region TestGetUnreleasedQuantityAndBuildCacheIfNeeded_WithPartAttributes_ReleaseCaptured_SerialNumber

		public void TestGetUnreleasedQuantityAndBuildCacheIfNeeded_WithPartAttributes_ReleaseCaptured_SerialNumber()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAllAttributeType(data.Org1, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true, setReleaseCaptured: true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Two, true, setReleaseCaptured: true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Three, true, setReleaseCaptured: true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true, setReleaseCaptured: true);

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 2m);
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 2m);
			var order1Line1 = order1.Lines[0];
			var pick = Helper.CreatePickNew(order1);
			AssertEquals("Precondition", 1, pick.GetAllPickLines().Count());
			AssertEquals("Precondition", 1, order1Line1.ReleaseLines.Count);

			var releaseLine1 = order1Line1.ReleaseLines[0];
			var releaseLine2 = order1Line1.ReleaseLines.AddNew();
			releaseLine1.Quantity = 1m;
			releaseLine2.Quantity = 1m;

			releaseLine1.PartAttribute1 = "PA1";
			releaseLine1.PartAttribute2 = "PA1";
			releaseLine1.PartAttribute3 = "PA1";
			releaseLine1.SerialNumber = "SN1";
			releaseLine2.PartAttribute1 = "PA1-2";
			releaseLine2.PartAttribute2 = "PA1-2";
			releaseLine2.PartAttribute3 = "PA1-2";
			releaseLine2.SerialNumber = "SN2";

			AssertEquals("Precondition.", 0m, ReleaseLinesOnPickManager.GetUnreleasedQuantityAndBuildCacheIfNeeded(Factory, releaseLine1, order1Line1));
			AssertEquals("Precondition.", 0m, ReleaseLinesOnPickManager.GetUnreleasedQuantityAndBuildCacheIfNeeded(Factory, releaseLine2, order1Line1));
			AssertEquals("Precondition.", 0m, releaseLine1.UnreleasedQty);
			AssertEquals("Precondition.", 0m, releaseLine2.UnreleasedQty);

			releaseLine1.Quantity = 0m;
			AssertEquals("Should share unreleased quantity.", 1m, ReleaseLinesOnPickManager.GetUnreleasedQuantityAndBuildCacheIfNeeded(Factory, releaseLine1, order1Line1));
			AssertEquals("Should share unreleased quantity.", 1m, ReleaseLinesOnPickManager.GetUnreleasedQuantityAndBuildCacheIfNeeded(Factory, releaseLine2, order1Line1));
			AssertEquals("Precondition.", 1m, releaseLine1.UnreleasedQty);
			AssertEquals("Precondition.", 1m, releaseLine2.UnreleasedQty);
		}

		#endregion

		#region TestGetUnreleasedQuantityAndBuildCacheIfNeeded_WithPartAttributes_ReleaseCaptured_Core

		void TestGetUnreleasedQuantityAndBuildCacheIfNeeded_WithPartAttributes_ReleaseCaptured_Core(Action<WhsReleaseLine, string> partAttribSetter)
		{
			var today = ZDate.Today;
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAllAttributeType(data.Org1, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Two, true, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Three, true, true);

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 20m);
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 20m);
			var order1Line1 = order1.Lines[0];
			var pick = Helper.CreatePickNew(order1);
			AssertEquals("Precondition", 1, pick.GetAllPickLines().Count());
			AssertEquals("Precondition", 1, order1Line1.ReleaseLines.Count);

			var releaseLine1 = order1Line1.ReleaseLines[0];
			var releaseLine2 = order1Line1.ReleaseLines.AddNew();
			releaseLine1.Quantity = 10m;
			releaseLine2.Quantity = 10m;

			releaseLine1.PartAttribute1 = "PA1";
			releaseLine1.PartAttribute2 = "PA1";
			releaseLine1.PartAttribute3 = "PA1";
			releaseLine2.PartAttribute1 = "PA1-2";
			releaseLine2.PartAttribute2 = "PA1-2";
			releaseLine2.PartAttribute3 = "PA1-2";
			partAttribSetter(releaseLine2, "OTHER"); // Override the appropriate PartAttribute on releaseLine2

			AssertNotEquals("Precondition", releaseLine1.PartAttribute1 + releaseLine1.PartAttribute2 + releaseLine1.PartAttribute3, releaseLine2.PartAttribute1 + releaseLine2.PartAttribute2 + releaseLine2.PartAttribute3);
			AssertEquals("Precondition.", 0m, ReleaseLinesOnPickManager.GetUnreleasedQuantityAndBuildCacheIfNeeded(Factory, releaseLine1, order1Line1));
			AssertEquals("Precondition.", 0m, ReleaseLinesOnPickManager.GetUnreleasedQuantityAndBuildCacheIfNeeded(Factory, releaseLine2, order1Line1));
			AssertEquals("Precondition.", 0m, releaseLine1.UnreleasedQty);
			AssertEquals("Precondition.", 0m, releaseLine2.UnreleasedQty);

			releaseLine1.Quantity = 0m;
			AssertEquals("Should share unreleased quantity.", 10m, ReleaseLinesOnPickManager.GetUnreleasedQuantityAndBuildCacheIfNeeded(Factory, releaseLine1, order1Line1));
			AssertEquals("Should share unreleased quantity.", 10m, ReleaseLinesOnPickManager.GetUnreleasedQuantityAndBuildCacheIfNeeded(Factory, releaseLine2, order1Line1));
			AssertEquals("Precondition.", 10m, releaseLine1.UnreleasedQty);
			AssertEquals("Precondition.", 10m, releaseLine2.UnreleasedQty);

			releaseLine2.Quantity -= 2m;
			AssertEquals("Should share unreleased quantity.", 12m, ReleaseLinesOnPickManager.GetUnreleasedQuantityAndBuildCacheIfNeeded(Factory, releaseLine1, order1Line1));
			AssertEquals("Should share unreleased quantity.", 12m, ReleaseLinesOnPickManager.GetUnreleasedQuantityAndBuildCacheIfNeeded(Factory, releaseLine2, order1Line1));
			AssertEquals("Precondition.", 12m, releaseLine1.UnreleasedQty);
			AssertEquals("Precondition.", 12m, releaseLine2.UnreleasedQty);
		}

		#endregion

		#endregion

		#endregion

		#region TestReleaseLineCache

		#region TestReleaseLineCache_UsingReleaseCapturedAttributes

		public void TestReleaseLineCache_UsingReleaseCapturedAttributes()
		{
			var today = ZDateTime.Today;
			var data = new TestDataSimpleEnvironment(Factory, 2, 2);
			var locationA_1 = data.Whs1.FindLocation("A-1");
			var locationA_2 = data.Whs1.FindLocation("A-2");
			Helper.SetClientAllAttributeType(data.Org1, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true, true);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, locationA_1, "");
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 20m, locationA_2, "");
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 30m);
			var orderLine = order.Lines[0];
			var pick = Helper.CreatePickNew(order);
			var pickLines = pick.GetAllPickLines();
			var pickLine1 = pickLines.Single(pl => pl.Inventory == inventory1);
			var pickLine2 = pickLines.Single(pl => pl.Inventory == inventory2);
			AssertEquals("Precondition: Order must have 2 pick lines.", 2, pickLines.Count());
			AssertEquals("Precondition: Pick line 1 should have 10 units.", pickLine1.WZ_Units, 10m);
			AssertEquals("Precondition: Pick line 2 should have 20 units.", pickLine2.WZ_Units, 20m);
			AssertEquals("Precondition: Release must have 1 lines.", 1, orderLine.ReleaseLines.Count);

			var releaseLine1 = orderLine.ReleaseLines[0];
			var releaseLine2 = orderLine.ReleaseLines.AddNew();
			releaseLine1.Quantity = 8m;
			releaseLine2.Quantity = 15m;
			releaseLine1.PartAttribute1 = "RED";
			releaseLine2.PartAttribute1 = "";
			AssertEquals("Precondition: Release must have 2 lines.", 2, orderLine.ReleaseLines.Count);

			// Simulate user unchecking pick lines
			AssertNoExceptionThrown("No exception should happen when un-picking order lines.", () => ReleaseLinesOnPickManager.NotifyChange(Factory, pickLine2, -20m));
			AssertNoExceptionThrown("No exception should happen when un-picking order lines.", () => ReleaseLinesOnPickManager.NotifyChange(Factory, pickLine1, -10m));
		}

		public void TestReleaseLineCache_UsingSerialNumber()
		{
			var today = ZDateTime.Today;
			var data = new TestDataSimpleEnvironment(Factory, 2, 2);
			var locationA_1 = data.Whs1.FindLocation("A-1");
			var locationA_2 = data.Whs1.FindLocation("A-2");
			Helper.SetClientAllAttributeType(data.Org1, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, locationA_1, "");
			inventory1.WI_SerialNumber = "SN1";
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, locationA_2, "");
			inventory2.WI_SerialNumber = "SN2";
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 2m);
			var orderLine = order.Lines[0];
			var pick = Helper.CreatePickNew(order);
			var pickLines = pick.GetAllPickLines();
			var pickLine1 = pickLines.Single(pl => pl.Inventory == inventory1);
			var pickLine2 = pickLines.Single(pl => pl.Inventory == inventory2);
			AssertEquals("Precondition: Order must have 2 pick lines.", 2, pickLines.Count());
			AssertEquals("Precondition: Pick line 1 should have 1 unit.", pickLine1.WZ_Units, 1m);
			AssertEquals("Precondition: Pick line 2 should have 1 unit.", pickLine2.WZ_Units, 1m);
			AssertEquals("Precondition: Release must have 1 line.", 2, orderLine.ReleaseLines.Count);

			var releaseLine1 = orderLine.ReleaseLines.Cast<WhsReleaseLine>().Single(r => r.SerialNumber == "SN1");
			var releaseLine2 = orderLine.ReleaseLines.Cast<WhsReleaseLine>().Single(r => r.SerialNumber == "SN2");
			releaseLine1.Quantity = 0m;
			AssertEquals("Precondition: Release must have 2 lines.", 2, orderLine.ReleaseLines.Count);

			// Simulate user unchecking pick lines
			AssertNoExceptionThrown("No exception should happen when un-picking order lines.", () => ReleaseLinesOnPickManager.NotifyChange(Factory, pickLine2, -1m));
			AssertNoExceptionThrown("No exception should happen when un-picking order lines.", () => ReleaseLinesOnPickManager.NotifyChange(Factory, pickLine1, -1m));
		}

		public void TestReleaseLineCache_ImportWizardReadOnlyCheck()
		{
			var today = ZDateTime.Today;
			var data = new TestDataSimpleEnvironment(Factory, 2, 2);
			var locationA_1 = data.Whs1.FindLocation("A-1");
			Helper.SetClientAllAttributeType(data.Org1, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true, false);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Two, true, true);
			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, locationA_1, "");
			inventory1.WI_PartAttrib1 = "RED";
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine1 = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var orderLine2 = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			Helper.CreatePickNew(order);

			orderLine1.ReleaseLines[0].Quantity = 0m;
			var releaseLinesOnOrderLine2 = WhsReleaseLineCollection.GetNewReleaseLinesCollection(orderLine2);
			var wizard = new ImportWizard(new ImportCollectionInfoImpl(releaseLinesOnOrderLine2), null, default);
			wizard.GenerateReadOnlyWarnings();

			// Simulate wizard importing RCA value
			AssertNoExceptionThrown("No exception should happen after wizard generates readonly warnings.", () => ReleaseLinesOnPickManager.NotifyChange(Factory, releaseLinesOnOrderLine2[0], orderLine2, 10m));
		}

		#endregion

		#endregion

		#region TestReleaseLineHasDistinctNonRCAttributes

		#region TestReleaseLineHasDistinctNonRCAttributes_PartAttrib1

		public void TestReleaseLineHasDistinctNonRCAttributes_PartAttrib1()
		{
			TestReleaseLineHasDistinctNonRCAttributes(1, (rl, pa) => rl.PartAttribute1 = pa);
		}

		#endregion

		#region TestReleaseLineHasDistinctNonRCAttributes_PartAttrib2

		public void TestReleaseLineHasDistinctNonRCAttributes_PartAttrib2()
		{
			TestReleaseLineHasDistinctNonRCAttributes(2, (rl, pa) => rl.PartAttribute2 = pa);
		}

		#endregion

		#region TestReleaseLineHasDistinctNonRCAttributes_PartAttrib3

		public void TestReleaseLineHasDistinctNonRCAttributes_PartAttrib3()
		{
			TestReleaseLineHasDistinctNonRCAttributes(3, (rl, pa) => rl.PartAttribute3 = pa);
		}

		#endregion

		#region TestReleaseLineHasDistinctNonRCAttributes_SerialNumber

		public void TestReleaseLineHasDistinctNonRCAttributes_SerialNumber()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAllAttributeType(data.Org1, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true, setReleaseCaptured: true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Two, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Three, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, data.Whs1.DefaultLocation, "", ZDate.Empty, ZDate.Empty, "", "2", "3", "");
			inventory1.WI_SerialNumber = "SN1";

			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, data.Whs1.DefaultLocation, "", ZDate.Empty, ZDate.Empty, "", "2", "3", "");
			inventory2.WI_SerialNumber = "SN2";

			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 2m);
			var orderLine = order.Lines[0];
			var pick = Helper.CreatePickNew(order);
			AssertEquals("Precondition", 2, pick.GetAllPickLines().Count());
			AssertEquals("Precondition", 2, orderLine.ReleaseLines.Count);

			var releaseLine1 = orderLine.ReleaseLines[0];
			var releaseLine2 = orderLine.ReleaseLines[1];
			AssertEquals("Should have distinct non RC Attributes as only the RCAs are the same.", true, ReleaseLinesOnPickManager.ReleaseLineHasDistinctNonRCAttributes(Factory, releaseLine1, orderLine));
			AssertEquals("Should have distinct non RC Attributes as only the RCAs are the same.", true, ReleaseLinesOnPickManager.ReleaseLineHasDistinctNonRCAttributes(Factory, releaseLine2, orderLine));

			// Setup duplicate release line
			var releaseLine3 = orderLine.ReleaseLines.AddNew();
			releaseLine3.PartAttribute2 = releaseLine1.PartAttribute2;
			releaseLine3.PartAttribute3 = releaseLine1.PartAttribute3;
			releaseLine3.SerialNumber = releaseLine1.SerialNumber;
			releaseLine3.Quantity = 1m;

			AssertEquals("Release 3 is a duplicate of Release Line 1, so ReleaseLineHasDistinctNonRCAttributes should return false.", false, ReleaseLinesOnPickManager.ReleaseLineHasDistinctNonRCAttributes(Factory, releaseLine3, orderLine));
			AssertEquals("Release Line 2 still has distinct attributes.", true, ReleaseLinesOnPickManager.ReleaseLineHasDistinctNonRCAttributes(Factory, releaseLine2, orderLine));
		}

		#endregion

		#region TestReleaseLineHasDistinctNonRCAttributes

		void TestReleaseLineHasDistinctNonRCAttributes(int partAttribToMakeRC, Action<WhsReleaseLine, string> partAttribSetter)
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAllAttributeType(data.Org1, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true, partAttribToMakeRC == 1);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Two, true, partAttribToMakeRC == 2);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Three, true, partAttribToMakeRC == 3);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, data.Whs1.DefaultLocation, "", ZDate.Empty, ZDate.Empty,
				partAttribToMakeRC == 1 ? "" : "1", partAttribToMakeRC == 2 ? "" : "2", partAttribToMakeRC == 3 ? "" : "3", "BEK");

			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, data.Whs1.DefaultLocation, "", ZDate.Empty, ZDate.Empty,
				partAttribToMakeRC == 1 ? "" : "3", partAttribToMakeRC == 2 ? "" : "2", partAttribToMakeRC == 3 ? "" : "1", "BEK");

			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 20m);
			var orderLine = order.Lines[0];
			var pick = Helper.CreatePickNew(order);
			AssertEquals("Precondition", 2, pick.GetAllPickLines().Count());
			AssertEquals("Precondition", 2, orderLine.ReleaseLines.Count);

			var releaseLine1 = orderLine.ReleaseLines[0];
			var releaseLine2 = orderLine.ReleaseLines[1];
			releaseLine1.Quantity = 10m;
			releaseLine2.Quantity = 10m;
			partAttribSetter(releaseLine1, "RC1");
			partAttribSetter(releaseLine2, "RC1");
			AssertEquals("Should have distinct non RC Attributes as only the RCAs are the same.", true, ReleaseLinesOnPickManager.ReleaseLineHasDistinctNonRCAttributes(Factory, releaseLine1, orderLine));
			AssertEquals("Should have distinct non RC Attributes as only the RCAs are the same.", true, ReleaseLinesOnPickManager.ReleaseLineHasDistinctNonRCAttributes(Factory, releaseLine2, orderLine));

			releaseLine1.Quantity = 5m;

			// Setup duplicate release line
			var releaseLine3 = orderLine.ReleaseLines.AddNew();
			releaseLine3.PartAttribute1 = releaseLine1.PartAttribute1;
			releaseLine3.PartAttribute2 = releaseLine1.PartAttribute2;
			releaseLine3.PartAttribute3 = releaseLine1.PartAttribute3;
			releaseLine3.Quantity = 5m;

			partAttribSetter(releaseLine3, "OTHER");
			AssertEquals("Release 1 and 3 share the same non RC Attributes, so ReleaseLineHasDistinctNonRCAttributes should return false.", false, ReleaseLinesOnPickManager.ReleaseLineHasDistinctNonRCAttributes(Factory, releaseLine1, orderLine));
			AssertEquals("Release Line 2 still has distinct attributes.", true, ReleaseLinesOnPickManager.ReleaseLineHasDistinctNonRCAttributes(Factory, releaseLine2, orderLine));
		}

		#endregion

		#endregion

		#region TestReconcilePickLines

		#region TestReconcilePickLines

		public void TestReconcilePickLines()
		{
			var now = ZDateTimeOffset.Today;
			var data = new TestDataSimpleEnvironment(Factory);
			var receive1 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "1", data.Part1, 10m);
			receive1.WD_ArrivalDate = now;
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			var orderLine1 = order.Lines[0];
			var orderLine2 = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			order.WD_PickOption = WhsPickOption.Codes.Manual;

			var pick = Helper.CreatePickNew(order);
			pick.IsAlterPick = true;
			var availableInventory = pick.OrderedInventories[0].AvailableInventories[0];
			availableInventory.PickLineQuantity = 10m;

			AssertEquals("Should have added one release line.", 1, orderLine1.ReleaseLines.Count);
			var releaseLine1 = orderLine1.ReleaseLines[0];
			AssertEquals("Should be fully released on this line.", 10m, releaseLine1.Quantity);
			AssertEquals("Should be fully released on this line.", 0m, releaseLine1.UnreleasedQty);
			releaseLine1.Quantity = 5m;

			AssertEquals("Should have added one release line.", 1, orderLine2.ReleaseLines.Count);
			var releaseLine2 = orderLine2.ReleaseLines[0];
			releaseLine2.Quantity = 5m;

			AssertEquals("Precondition", 5m, releaseLine1.Quantity);
			AssertEquals("Precondition", 5m, releaseLine2.Quantity);
			AssertEquals("Precondition", 0m, releaseLine1.UnreleasedQty);
			AssertEquals("Precondition", 0m, releaseLine2.UnreleasedQty);

			AssertEquals("Release Lines unchanged", 5m, releaseLine1.Quantity);
			AssertEquals("Release Lines unchanged", 5m, releaseLine2.Quantity);
			AssertEquals("Release Lines unchanged", 0m, releaseLine1.UnreleasedQty);
			AssertEquals("Release Lines unchanged", 0m, releaseLine2.UnreleasedQty);

			AssertEquals("Each order line has a single pick line", 1, orderLine1.PickLines.Count);
			AssertEquals("Each order line has a single pick line", 1, orderLine2.PickLines.Count);
			AssertEquals("Each pick line has 5 units", 5m, orderLine1.PickLines[0].WZ_Units);
			AssertEquals("Each pick line has 5 units", 5m, orderLine2.PickLines[0].WZ_Units);

			releaseLine1.Quantity = 0m;
			releaseLine2.Quantity = 10m;
			AssertEquals("Precondition", 10m, releaseLine2.Quantity);
			AssertEquals("Precondition", 0m, releaseLine2.UnreleasedQty);
			AssertEquals("Precondition - Should have deleted release line.", 0, orderLine1.ReleaseLines.Count);

			AssertEquals("Order Line 1 has no pick lines", 0, orderLine1.PickLines.Count);
			AssertEquals("Order line 2 has one pick line", 1, orderLine2.PickLines.Count);
			AssertEquals("Order Line 2 has 10 units picked", 10m, orderLine2.PickLines[0].WZ_Units);
			AssertNoExceptionThrown(Factory.Save);

			var newFactory1 = new BusinessObjectFactory { RefreshEnabled = false };
			orderLine1 = newFactory1.Load<WhsOrderLine>(orderLine1.PK);
			orderLine2 = newFactory1.Load<WhsOrderLine>(orderLine2.PK);

			releaseLine2 = orderLine2.ReleaseLines[0];
			releaseLine2.Quantity = 1m;
			releaseLine1 = orderLine1.ReleaseLines[0];
			releaseLine1.Quantity = 5m;
			AssertEquals("Precondition", 5m, releaseLine1.Quantity);
			AssertEquals("Precondition", 1m, releaseLine2.Quantity);
			AssertEquals("Precondition", 4m, releaseLine1.UnreleasedQty);
			AssertEquals("Precondition", 4m, releaseLine2.UnreleasedQty);

			AssertEquals("Order line 1 has one pick line", 1, orderLine1.PickLines.Count);
			AssertEquals("Order Line 2 has one pick lines", 1, orderLine2.PickLines.Count);
			AssertEquals("Order Line 1 has 5 units picked", 5m, orderLine1.PickLines[0].WZ_Units);
			AssertEquals("Order Line 2 has 5 units picked", 5m, orderLine2.PickLines[0].WZ_Units);

			releaseLine1.Quantity = 10m;
			AssertEquals("Order line 1 has one pick line", 1, orderLine1.PickLines.Count);
			AssertEquals("Order Line 2 has one pick lines", 1, orderLine2.PickLines.Count);
			AssertEquals("Order Line 1 has 9 units picked", 9m, orderLine1.PickLines[0].WZ_Units);
			AssertEquals("Order Line 2 has 1 units picked", 1m, orderLine2.PickLines[0].WZ_Units);

			releaseLine2.Quantity = 0m;
			AssertEquals("Precondition.", 0m, releaseLine1.UnreleasedQty);
			AssertEquals("Precondition", 10m, releaseLine1.Quantity);
			AssertEquals("Precondition", 0m, releaseLine1.UnreleasedQty);
			AssertEquals("Precondition - Should have deleted release line.", 0, orderLine2.ReleaseLines.Count);

			AssertEquals("Order Line 2 has no pick lines", 0, orderLine2.PickLines.Count);
			AssertEquals("Order line 1 has one pick line", 1, orderLine1.PickLines.Count);
			AssertEquals("Order Line 1 has 10 units picked", 10m, orderLine1.PickLines[0].WZ_Units);
			AssertNoExceptionThrown(newFactory1.Save);

			var newFactory2 = new BusinessObjectFactory { RefreshEnabled = false };
			orderLine1 = newFactory2.Load<WhsOrderLine>(orderLine1.PK);
			orderLine2 = newFactory2.Load<WhsOrderLine>(orderLine2.PK);

			releaseLine1 = orderLine1.ReleaseLines[0];
			releaseLine1.Quantity = 5m;

			releaseLine2 = orderLine2.ReleaseLines[0];
			releaseLine2.Quantity = 5m;
			AssertEquals("Order line 1 has one pick line", 1, orderLine1.PickLines.Count);
			AssertEquals("Order Line 2 has one pick lines", 1, orderLine2.PickLines.Count);
			AssertEquals("Order Line 1 has 5 units picked", 5m, orderLine1.PickLines[0].WZ_Units);
			AssertEquals("Order Line 2 has 5 units picked", 5m, orderLine2.PickLines[0].WZ_Units);
			AssertNoExceptionThrown(newFactory2.Save);

			var newFactory3 = new BusinessObjectFactory { RefreshEnabled = false };
			orderLine1 = newFactory3.Load<WhsOrderLine>(orderLine1.PK);
			orderLine2 = newFactory3.Load<WhsOrderLine>(orderLine2.PK);

			releaseLine1 = orderLine1.ReleaseLines[0];
			releaseLine1.Quantity = 6m;

			releaseLine2 = orderLine2.ReleaseLines[0];
			releaseLine2.Quantity = 4m;
			AssertEquals("Order line 1 has one pick line", 1, orderLine1.PickLines.Count);
			AssertEquals("Order Line 2 has one pick lines", 1, orderLine2.PickLines.Count);
			AssertEquals("Order Line 1 has 6 units picked", 6m, orderLine1.PickLines[0].WZ_Units);
			AssertEquals("Order Line 2 has 4 units picked", 4m, orderLine2.PickLines[0].WZ_Units);
			AssertNoExceptionThrown(newFactory3.Save);

			var newFactory4 = new BusinessObjectFactory { RefreshEnabled = false };
			orderLine1 = newFactory4.Load<WhsOrderLine>(orderLine1.PK);
			orderLine2 = newFactory4.Load<WhsOrderLine>(orderLine2.PK);

			releaseLine1 = orderLine1.ReleaseLines[0];
			releaseLine1.Quantity = 4m;

			releaseLine2 = orderLine2.ReleaseLines[0];
			releaseLine2.Quantity = 6m;
			AssertEquals("Order line 1 has one pick line", 1, orderLine1.PickLines.Count);
			AssertEquals("Order Line 2 has one pick lines", 1, orderLine2.PickLines.Count);
			AssertEquals("Order Line 1 has 6 units picked", 4m, orderLine1.PickLines[0].WZ_Units);
			AssertEquals("Order Line 2 has 4 units picked", 6m, orderLine2.PickLines[0].WZ_Units);
			AssertNoExceptionThrown(newFactory4.Save);

			var receive2 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "2", data.Part1, 10m);
			receive2.WD_ArrivalDate = now;
			Factory.Save();

			var newFactory5 = new BusinessObjectFactory { RefreshEnabled = false };
			orderLine1 = newFactory5.Load<WhsOrderLine>(orderLine1.PK);
			orderLine2 = newFactory5.Load<WhsOrderLine>(orderLine2.PK);
			pick = newFactory5.Load<WhsPick>(pick.PK);
			pick.IsAlterPick = true;
			availableInventory = pick.OrderedInventories[0].AvailableInventories[0];
			releaseLine1 = orderLine1.ReleaseLines[0];
			releaseLine2 = orderLine2.ReleaseLines[0];

			availableInventory.PickLineQuantity = 20m;
			releaseLine2 = orderLine2.ReleaseLines[0];
			AssertEquals("Both lines should be fully released with 10m units each", 10m, releaseLine1.Quantity);
			AssertEquals("Both lines should be fully released with 10m units each", 10m, releaseLine2.Quantity);
			AssertEquals("Both lines should be fully released with 10m units each", 0m, releaseLine1.UnreleasedQty);
			AssertEquals("Both lines should be fully released with 10m units each", 0m, releaseLine2.UnreleasedQty);

			AssertEquals("Each order line line has 10 units picked", 10m, orderLine1.PickLines.Sum(pl => pl.WZ_Units));
			AssertEquals("Each order line line has 10 units picked", 10m, orderLine2.PickLines.Sum(pl => pl.WZ_Units));
			AssertNoExceptionThrown(newFactory5.Save);
		}

		#endregion

		#region TestReconcilePickLines_WhenStockIsPicked

		public void TestReconcilePickLines_WhenStockIsPicked()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			var orderLine1 = order.Lines[0];
			var orderLine2 = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			order.WD_PickOption = WhsPickOption.Codes.Manual;

			var pick = Helper.CreatePickNew(order);
			pick.IsAlterPick = true;
			var availableInventory = pick.OrderedInventories[0].AvailableInventories[0];
			availableInventory.PickLineQuantity = 10m;

			AssertEquals("Should have added one release line.", 1, orderLine1.ReleaseLines.Count);
			var releaseLine1 = orderLine1.ReleaseLines[0];
			AssertEquals("Should be fully released on this line.", 10m, releaseLine1.Quantity);
			AssertEquals("Should be fully released on this line.", 0m, releaseLine1.UnreleasedQty);

			orderLine1.PickLines.Single().WZ_PickedDateTime = ZDateTimeOffset.Now;
			Factory.Save();

			releaseLine1.Quantity = 5m;
			AssertEquals("Should have added one release line.", 1, orderLine2.ReleaseLines.Count);

			var releaseLine2 = orderLine2.ReleaseLines[0];
			releaseLine2.Quantity = 5m;
			AssertEquals("Precondition", 5m, releaseLine1.Quantity);
			AssertEquals("Precondition", 5m, releaseLine2.Quantity);
			AssertEquals("Precondition", 0m, releaseLine1.UnreleasedQty);
			AssertEquals("Precondition", 0m, releaseLine2.UnreleasedQty);

			AssertEquals("Release Lines unchanged", 5m, releaseLine1.Quantity);
			AssertEquals("Release Lines unchanged", 5m, releaseLine2.Quantity);
			AssertEquals("Release Lines unchanged", 0m, releaseLine1.UnreleasedQty);
			AssertEquals("Release Lines unchanged", 0m, releaseLine2.UnreleasedQty);

			AssertEquals("Each order line has a single pick line", 1, orderLine1.PickLines.Count);
			AssertEquals("Each order line has a single pick line", 1, orderLine2.PickLines.Count);
			AssertEquals("Each pick line has 5 units", 5m, orderLine1.PickLines[0].WZ_Units);
			AssertEquals("Each pick line has 5 units", 5m, orderLine2.PickLines[0].WZ_Units);

			releaseLine1.Quantity = 0m;
			releaseLine2.Quantity = 10m;
			AssertEquals("Precondition", 10m, releaseLine2.Quantity);
			AssertEquals("Precondition", 0m, releaseLine2.UnreleasedQty);
			AssertEquals("Precondition - Should have deleted release line.", 0, orderLine1.ReleaseLines.Count);

			AssertEquals("Order Line 1 has no pick lines", 0, orderLine1.PickLines.Count);
			AssertEquals("Order line 2 has one pick line", 1, orderLine2.PickLines.Count);
			AssertEquals("Order Line 2 has 10 units picked", 10m, orderLine2.PickLines[0].WZ_Units);
			AssertNoExceptionThrown(Factory.Save);

			var newFactory1 = new BusinessObjectFactory { RefreshEnabled = false };
			orderLine1 = newFactory1.Load<WhsOrderLine>(orderLine1.PK);
			orderLine2 = newFactory1.Load<WhsOrderLine>(orderLine2.PK);

			releaseLine2 = orderLine2.ReleaseLines[0];
			releaseLine2.Quantity = 1m;
			releaseLine1 = orderLine1.ReleaseLines[0];
			releaseLine1.Quantity = 5m;
			AssertEquals("Precondition", 5m, releaseLine1.Quantity);
			AssertEquals("Precondition", 1m, releaseLine2.Quantity);
			AssertEquals("Precondition", 4m, releaseLine1.UnreleasedQty);
			AssertEquals("Precondition", 4m, releaseLine2.UnreleasedQty);

			AssertEquals("Order line 1 has one pick line", 1, orderLine1.PickLines.Count);
			AssertEquals("Order Line 2 has one pick lines", 1, orderLine2.PickLines.Count);
			AssertEquals("Order Line 1 has 5 units picked", 5m, orderLine1.PickLines[0].WZ_Units);
			AssertEquals("Order Line 2 has 5 units picked", 5m, orderLine2.PickLines[0].WZ_Units);

			releaseLine1.Quantity = 10m;
			AssertEquals("Order line 1 has one pick line", 1, orderLine1.PickLines.Count);
			AssertEquals("Order Line 2 has one pick lines", 1, orderLine2.PickLines.Count);
			AssertEquals("Order Line 1 has 9 units picked", 9m, orderLine1.PickLines[0].WZ_Units);
			AssertEquals("Order Line 2 has 1 units picked", 1m, orderLine2.PickLines[0].WZ_Units);
			AssertNoExceptionThrown(() => newFactory1.Save());
		}

		#endregion

		#region TestReconcilePickLines_MaintainsAvailInventoryPickLinesWhileMovingPickLines

		public void TestReconcilePickLines_MaintainsAvailInventoryPickLinesWhileMovingPickLines()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAllAttributeType(data.Org1, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 20m, ZDate.Empty, ZDate.Empty, "1", "", "", "");
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocket();
			Factory.Save();

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			var order1Line1 = order1.Lines[0];
			var order1Line2 = Helper.CreateWhsOrderLine(order1, data.Part1, 10m);
			order1Line2.WE_PartAttrib1 = "1";

			var pick = Helper.CreatePickNew(order1);
			pick.OrderedInventories.Cast<WhsPickOrderedInventory>().Single(ordInv => ordInv.PartAttrib1 == "").AvailableInventories[0].PickLineQuantity = 0m;
			AssertEquals("Precondition.", 2, pick.OrderedInventories.Count);

			// Moving pick line
			AssertEquals("Precondition.", 0, order1Line1.ReleaseLines.Count);
			AssertEquals("Precondition.", 1, order1Line2.ReleaseLines.Count);
			AssertEquals("Precondition.", 0, order1Line1.PickLines.Count);
			AssertEquals("Precondition.", 1, order1Line2.PickLines.Count);

			order1Line2.ReleaseLines[0].Quantity = 0m;
			order1Line1.ReleaseLines[0].Quantity = 10m;

			AssertEquals("Should have moved pick lines.", 1, order1Line1.PickLines.Count);
			AssertEquals("Should have moved pick lines.", 0, order1Line2.PickLines.Count);

			AssertContainsExactElementsInAnyOrder("Should have updated available inventory pick lines.",
				order1Line1.PickLines,
				pick.OrderedInventories.Cast<WhsPickOrderedInventory>().Single(ordInv => ordInv.PartAttrib1 == "").AvailableInventories[0].PickLines);

			AssertNoExceptionThrown(Factory.Save);

			// Splitting pick line
			order1Line1.ReleaseLines[0].Quantity = 5m;
			order1Line2.ReleaseLines[0].Quantity = 5m;

			AssertEquals("Should have split pick lines.", 1, order1Line1.PickLines.Count);
			AssertEquals("Should have split pick lines.", 1, order1Line2.PickLines.Count);

			AssertContainsExactElementsInAnyOrder("Should have updated available inventory pick lines.",
				order1Line1.PickLines,
				pick.OrderedInventories.Cast<WhsPickOrderedInventory>().Single(ordInv => ordInv.PartAttrib1 == "").AvailableInventories[0].PickLines);

			AssertContainsExactElementsInAnyOrder("Should have updated available inventory pick lines.",
				order1Line2.PickLines,
				pick.OrderedInventories.Cast<WhsPickOrderedInventory>().Single(ordInv => ordInv.PartAttrib1 == "1").AvailableInventories[0].PickLines);

			AssertNoExceptionThrown(Factory.Save);

			// Merging pick line
			order1Line1.ReleaseLines[0].Quantity++;
			order1Line2.ReleaseLines[0].Quantity--;

			AssertEquals("Should have moved pick lines.", 6m, order1Line1.PickLines[0].WZ_Units);
			AssertEquals("Should have moved pick lines.", 4m, order1Line2.PickLines[0].WZ_Units);

			AssertContainsExactElementsInAnyOrder("Should have updated available inventory pick lines.",
				order1Line1.PickLines,
				pick.OrderedInventories.Cast<WhsPickOrderedInventory>().Single(ordInv => ordInv.PartAttrib1 == "").AvailableInventories[0].PickLines);

			AssertContainsExactElementsInAnyOrder("Should have updated available inventory pick lines.",
				order1Line2.PickLines,
				pick.OrderedInventories.Cast<WhsPickOrderedInventory>().Single(ordInv => ordInv.PartAttrib1 == "1").AvailableInventories[0].PickLines);

			AssertNoExceptionThrown(Factory.Save);
		}

		#endregion

		#region TestReconcilePickLines_PackedPickLinesAreNotMoved

		public void TestReconcilePickLines_PackedPickLinesAreNotMoved()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "1", data.Part1, 10m);
			Factory.Save();

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			var orderLine1 = order1.Lines[0];
			var orderLine2 = order2.Lines[0];
			Helper.CreatePickNew(order1, order2);

			var pickedOrderLine = new[] { orderLine1, orderLine2 }.Single(o => o.PickLines.Count > 0);
			var releaseLine = pickedOrderLine.ReleaseLines[0];
			AssertEquals("Precondition: 10 Units Picked.", 10m, releaseLine.Quantity);

			var package = pickedOrderLine.Order.PackageJob.Packages.AddNew();
			package.Pack(releaseLine, 4m);
			AssertEquals("Precondition: Release Line is packed.", true, releaseLine.IsPacked);

			var unPackedPickLine = pickedOrderLine.PickLines.Single(p => p.IsUnpacked(Factory));
			unPackedPickLine.WZ_GS_NKAssignedTo = "E";
			AssertEquals("Precondition: 6 Units are unpacked.", 6m, unPackedPickLine.WZ_Units);

			var packedPickLine = pickedOrderLine.PickLines.Single(p => !p.IsUnpacked(Factory));
			releaseLine.Quantity = 4m;
			var otherOrderLine = new[] { orderLine1, orderLine2 }.Single(o => o != pickedOrderLine);
			var newReleaseLine = otherOrderLine.ReleaseLines[0];
			AssertEquals("Precondition: Zero Quantity Line exists.", 0m, newReleaseLine.Quantity);

			newReleaseLine.Quantity = 6m;
			AssertEquals(0m, releaseLine.UnreleasedQty);
			AssertEquals(0m, newReleaseLine.UnreleasedQty);
			AssertEquals("PickLine Packing should not have changed.", true, unPackedPickLine.IsUnpacked(Factory));
			AssertEquals("PickLine Packing should not have changed.", false, packedPickLine.IsUnpacked(Factory));
			AssertContainsExactElementsInAnyOrder(new[] { unPackedPickLine }, otherOrderLine.PickLines);
			AssertContainsExactElementsInAnyOrder(new[] { packedPickLine }, pickedOrderLine.PickLines);
		}

		#endregion

		#region TestReconcilePickLines_PackedReleaseLineIsNotRemoved

		public void TestReconcilePickLines_PackedReleaseLineIsNotRemoved()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, true, "COLOUR");
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true, true);
			var receive1 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 100m, data.Whs1.FindLocation("A-1"), "");
			var receive2 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 100m, data.Whs1.FindLocation("A-2"), "");
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 30m, pickOption: WhsPickOption.Codes.Manual);
			var orderLine = order.Lines[0];
			var pick = Helper.CreatePickNew(order);
			pick.IsAlterPick = true;
			var availableInventory1 = pick.OrderedInventories[0].AvailableInventories[0];
			var availableInventory2 = pick.OrderedInventories[0].AvailableInventories[1];
			availableInventory1.PickLineQuantity = 10m;
			availableInventory2.PickLineQuantity = 20m;
			Factory.Save();

			var releaseLine1 = orderLine.ReleaseLines[0];
			AssertEquals("Precondition: 30 Units Picked.", 30m, releaseLine1.Quantity);

			var package = order.PackageJob.Packages.AddNew();
			package.Pack(releaseLine1, 7m);
			AssertEquals("Precondition: Release Line is packed.", true, releaseLine1.IsPacked);

			releaseLine1.Quantity = 8m;
			var releaseLine2 = orderLine.ReleaseLines.AddNew();
			releaseLine2.PartAttribute1 = "RED";
			releaseLine2.Quantity = 15m;
			AssertEquals("Precondition: Order Line Has 2 release lines.", 2, orderLine.ReleaseLines.Count);
			AssertEquals("Precondition: ReleaseLine1 is packed.", true, releaseLine1.IsPacked);
			AssertEquals("Precondition: Quantity of releaseLine1.", 8m, releaseLine1.Quantity);
			AssertEquals("Precondition: UnreleasedQty of releaseLine1.", 7m, releaseLine1.UnreleasedQty);
			AssertEquals("Precondition: Quantity of releaseLine2.", 15m, releaseLine2.Quantity);
			AssertEquals("Precondition: UnreleasedQty of releaseLine2.", 7m, releaseLine2.UnreleasedQty);

			// Simulate the Packing Grid being bound
			var count = order.PackageJob.PackableItemParents.Count;
			// Simulate user unchecking available inventory2
			AssertNoExceptionThrown("No exception should happen when un-picking order lines.", () => { availableInventory2.Allocate = false; });
			AssertEquals("Order Line Has 2 release lines.", 2, orderLine.ReleaseLines.Count);
			AssertEquals("Quantity of releaseLine1.", 8m, releaseLine1.Quantity);
			AssertEquals("UnreleasedQty of releaseLine1.", -13m, releaseLine1.UnreleasedQty);
			AssertEquals("Quantity of releaseLine2.", 15m, releaseLine2.Quantity);
			AssertEquals("UnreleasedQty of releaseLine2.", -13m, releaseLine2.UnreleasedQty);
		}

		#endregion

		#region TestReconcilePickLines_PackedReleaseLineIsNotReduced

		public void TestReconcilePickLines_PackedReleaseLineIsNotReduced()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, true, "COLOUR");
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true, true);
			var receive1 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 100m, data.Whs1.FindLocation("A-1"), "");
			var receive2 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 100m, data.Whs1.FindLocation("A-2"), "");
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 30m, pickOption: WhsPickOption.Codes.Manual);
			var orderLine = order.Lines[0];
			var pick = Helper.CreatePickNew(order);
			pick.IsAlterPick = true;
			var availableInventory1 = pick.OrderedInventories[0].AvailableInventories[0];
			var availableInventory2 = pick.OrderedInventories[0].AvailableInventories[1];
			availableInventory1.PickLineQuantity = 10m;
			availableInventory2.PickLineQuantity = 20m;
			Factory.Save();

			var releaseLine1 = orderLine.ReleaseLines[0];
			AssertEquals("Precondition: 30 Units Picked.", 30m, releaseLine1.Quantity);

			var package = order.PackageJob.Packages.AddNew();
			package.Pack(releaseLine1, 13m);
			AssertEquals("Precondition: Release Line is packed.", true, releaseLine1.IsPacked);
			AssertEquals("Precondition: Quantity of releaseLine1.", 30m, releaseLine1.Quantity);
			AssertEquals("Precondition: UnreleasedQty of releaseLine1.", 0m, releaseLine1.UnreleasedQty);
			AssertEquals("Precondition: PickLineQuantity of availableInventory1.", 10m, availableInventory1.PickLineQuantity);
			AssertEquals("Precondition: PickLineQuantity of availableInventory2.", 20m, availableInventory2.PickLineQuantity);

			// Simulate the Packing Grid being bound
			var count = order.PackageJob.PackableItemParents.Count;
			// Simulate user unchecking available inventory2
			AssertNoExceptionThrown("No exception should happen when un-picking order lines.", () => { availableInventory2.Allocate = false; });
			AssertEquals("Quantity of releaseLine1.", 30m, releaseLine1.Quantity);
			AssertEquals("UnreleasedQty of releaseLine1.", -17m, releaseLine1.UnreleasedQty);
			AssertEquals("PickLineQuantity of availableInventory1.", 10m, availableInventory1.PickLineQuantity);
			AssertEquals("PickLineQuantity of availableInventory2.", 3m, availableInventory2.PickLineQuantity);
		}

		#endregion

		#region TestReconcilePickLines_WithSerialNumber

		public void TestReconcilePickLines_WithSerialNumber()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);
			var receive1 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 1m, data.Whs1.FindLocation("A-1"), "", finalise: false);
			var receive2 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 1m, data.Whs1.FindLocation("A-2"), "", finalise: false);
			receive1.Lines[0].WE_SerialNumber = "SN1";
			receive2.Lines[0].WE_SerialNumber = "SN2";
			receive1.FinaliseDocketWithoutUserConfirmation();
			receive2.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive1);
			AssertIsFinalisedPrecondition(receive2);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 2m, pickOption: WhsPickOption.Codes.Manual);
			var orderLine = order.Lines[0];
			var pick = Helper.CreatePickNew(order);
			pick.IsAlterPick = true;
			var availableInventory1 = pick.OrderedInventories[0].AvailableInventories[0];
			var availableInventory2 = pick.OrderedInventories[0].AvailableInventories[1];
			availableInventory1.PickLineQuantity = 1m;
			availableInventory2.PickLineQuantity = 1m;
			AssertEquals("Should have two Release Lines.", 2, orderLine.ReleaseLines.Count);
			AssertEquals("Should have correct Serial Numbers", 1, orderLine.ReleaseLines.Cast<WhsReleaseLine>().Count(r => r.SerialNumber == "SN1" && r.UnreleasedQty == 0m));
			AssertEquals("Should have correct Serial Numbers", 1, orderLine.ReleaseLines.Cast<WhsReleaseLine>().Count(r => r.SerialNumber == "SN2" && r.UnreleasedQty == 0m));

			availableInventory1.PickLineQuantity = 0m;
			AssertEquals("Should have 1 Release Lines.", 1, orderLine.ReleaseLines.Count);
			AssertEquals("Should have correct Serial Numbers", 0, orderLine.ReleaseLines.Cast<WhsReleaseLine>().Count(r => r.SerialNumber == "SN1"));
			AssertEquals("Should have correct Serial Numbers", 1, orderLine.ReleaseLines.Cast<WhsReleaseLine>().Count(r => r.SerialNumber == "SN2" && r.UnreleasedQty == 0m));
		}

		#endregion

		#region TestReconcilePickLines_ReconcilesWithItself

		public void TestReconcilePickLines_ReconcilesWithItself()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "1", data.Part1, 20m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			var orderLine1 = order.Lines[0];
			var orderLine2 = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			order.WD_PickOption = WhsPickOption.Codes.Manual;

			var pick = Helper.CreatePickNew(order);
			var availableInventory = pick.OrderedInventories[0].AvailableInventories[0];
			availableInventory.PickLineQuantity = 10m;
			AssertEquals("Should have added one release line.", 1, orderLine1.ReleaseLines.Count);

			var releaseLine1 = orderLine1.ReleaseLines[0];
			AssertEquals("Should be fully released on this line.", 10m, releaseLine1.Quantity);
			AssertEquals("Should be fully released on this line.", 0m, releaseLine1.UnreleasedQty);

			releaseLine1.Quantity = 5m;
			AssertEquals("Should have added one release line on orderLine2.", 1, orderLine2.ReleaseLines.Count);

			var releaseLine2 = orderLine2.ReleaseLines[0];
			releaseLine2.Quantity = 5m;
			AssertEquals("Each order line has a single pick line", 1, orderLine1.PickLines.Count);
			AssertEquals("Each order line has a single pick line", 1, orderLine2.PickLines.Count);
			AssertEquals("Precondition", 5m, orderLine1.PickLines[0].WZ_Units);
			AssertEquals("Precondition", 5m, orderLine2.PickLines[0].WZ_Units);

			releaseLine1.Quantity = 1m;
			releaseLine2.Quantity = 1m;
			releaseLine1.Quantity = 5m;

			AssertEquals("Each order line has a single pick line.", 1, orderLine1.PickLines.Count);
			AssertEquals("Each order line has a single pick line.", 1, orderLine2.PickLines.Count);
			AssertEquals("Should have reconciled pick lines with itself, not moved from the other line.", 5m, orderLine1.PickLines[0].WZ_Units);
			AssertEquals("Should have reconciled pick lines with itself, not moved from the other line.", 5m, orderLine2.PickLines[0].WZ_Units);

			releaseLine2.Quantity = 10m;
			releaseLine2.Quantity = 5m;
			AssertEquals("Each order line has a single pick line.", 1, orderLine1.PickLines.Count);
			AssertEquals("Each order line has a single pick line.", 1, orderLine2.PickLines.Count);
			AssertEquals("Should have reconciled pick line with itself.", 5m, orderLine1.PickLines[0].WZ_Units);
			AssertEquals("Should have reconciled pick line with itself.", 5m, orderLine2.PickLines[0].WZ_Units);
			AssertNoExceptionThrown(Factory.Save);
		}

		#endregion

		#region TestReconcilePickLines_ReserveLinesMaintainedDuringMerge

		public void TestReconcilePickLines_ReserveLinesMaintainedDuringMerge()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "1", data.Part1, 20m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 20m);
			var orderLine1 = order.Lines[0];
			var orderLine2 = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var reserveLine = Helper.CreateReservePickLine(orderLine2, receive.Inventory[0], 10m);

			var pick = Helper.CreatePickNew(order);
			var releaseLine1 = orderLine1.ReleaseLines[0];
			var releaseLine2 = orderLine2.ReleaseLines[0];
			AssertEquals("Precondition", false, reserveLine.IsDeleted);
			AssertEquals("Precondition", 10m, reserveLine.WZ_OriginalReservedQty);

			releaseLine2.Quantity = 0m;
			releaseLine1.Quantity = 20m;
			AssertEquals(20m, releaseLine1.Quantity);
			AssertEquals(0m, releaseLine1.UnreleasedQty);
			AssertEquals(true, releaseLine2.IsDeleted);

			AssertEquals("Precondition", 1, orderLine1.PickLines.Count);
			AssertEquals("Precondition", 20m, orderLine1.PickLines[0].WZ_Units);
			AssertEquals("Precondition", 0m, orderLine1.PickLines[0].WZ_OriginalReservedQty);

			AssertEquals("Should still have a reserve line", 0m, orderLine2.PickLines[0].WZ_Units);
			AssertEquals("Should still have a reserve line", 10m, orderLine2.PickLines[0].WZ_OriginalReservedQty);
		}

		#endregion

		#region TestReconcilePickLines_ReserveLinesMaintainedDuringMerge_WhenStockIsPickedInDB

		public void TestReconcilePickLines_ReserveLinesMaintainedDuringMerge_WhenStockIsPickedInDB()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "1", data.Part1, 20m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 20m);
			var orderLine1 = order.Lines[0];
			var orderLine2 = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var reserveLine = Helper.CreateReservePickLine(orderLine2, receive.Inventory[0], 10m);

			Helper.CreatePickNew(order);
			var releaseLine1 = orderLine1.ReleaseLines[0];
			var releaseLine2 = orderLine2.ReleaseLines[0];
			AssertEquals("Precondition", false, reserveLine.IsDeleted);
			AssertEquals("Precondition", 10m, reserveLine.WZ_OriginalReservedQty);

			var now = ZDateTimeOffset.Now;
			reserveLine.WZ_PickedDateTime = now;
			orderLine1.PickLines.Single().WZ_PickedDateTime = now;
			Factory.Save();

			releaseLine2.Quantity = 0m;
			releaseLine1.Quantity = 20m;
			AssertEquals(20m, releaseLine1.Quantity);
			AssertEquals(0m, releaseLine1.UnreleasedQty);
			AssertEquals(true, releaseLine2.IsDeleted);

			AssertEquals("Precondition", 2, orderLine1.PickLines.Count);
			AssertEquals("Precondition", 20m, orderLine1.PickLines.Sum(pl => pl.WZ_Units));
			AssertEquals("Precondition", true, orderLine1.PickLines.All(pl => pl.WZ_OriginalReservedQty == 0m));
			AssertEquals("Reserve line has Original Reserved Qty removed when Picked, so the Order Line will have no Pick Lines.", 0, orderLine2.PickLines.Count);
		}

		#endregion

		#region TestReconcilePickLines_ReserveLinesMaintainedDuringMerge_WhenStockIsPickedInMemory

		public void TestReconcilePickLines_ReserveLinesMaintainedDuringMerge_WhenStockIsPickedInMemory()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "1", data.Part1, 20m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 20m);
			var orderLine1 = order.Lines[0];
			var orderLine2 = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var reserveLine = Helper.CreateReservePickLine(orderLine2, receive.Inventory[0], 10m);

			Helper.CreatePickNew(order);
			var releaseLine1 = orderLine1.ReleaseLines[0];
			var releaseLine2 = orderLine2.ReleaseLines[0];
			AssertEquals("Precondition", false, reserveLine.IsDeleted);
			AssertEquals("Precondition", 10m, reserveLine.WZ_OriginalReservedQty);

			var now = ZDateTimeOffset.Now;
			reserveLine.WZ_PickedDateTime = now;
			Factory.Save();

			orderLine1.PickLines.Single().WZ_PickedDateTime = now;
			releaseLine2.Quantity = 0m;
			releaseLine1.Quantity = 20m;
			AssertEquals(20m, releaseLine1.Quantity);
			AssertEquals(0m, releaseLine1.UnreleasedQty);
			AssertEquals(true, releaseLine2.IsDeleted);

			AssertEquals("Because the PickLine we are moving is Finalised, we don't merge it.", 2, orderLine1.PickLines.Count);
			AssertEquals("Both PickLines should sum to 20 Units.", 20m, orderLine1.PickLines.Sum(pl => pl.WZ_Units));
			AssertEquals("Both PickLines should not have Original Reserved Qty.", true, orderLine1.PickLines.All(pl => pl.WZ_OriginalReservedQty == 0m));
			AssertEquals("Reserve line has Original Reserved Qty removed when Picked, so the Order Line will have no PickLines.", 0, orderLine2.PickLines.Count);
		}

		#endregion

		#region TestReconcilePickLines_ReserveLinesMaintained

		public void TestReconcilePickLines_ReserveLinesMaintained()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 20m);
			var orderLine1 = order.Lines[0];
			var orderLine2 = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var reserveLine = Helper.CreateReservePickLine(orderLine2, receive.Inventory[0], 10m);

			var pick = Helper.CreatePickNew(order);
			var releaseLine2 = orderLine2.ReleaseLines[0];
			AssertEquals("Precondition", false, reserveLine.IsDeleted);
			AssertEquals("Precondition", 10m, reserveLine.WZ_OriginalReservedQty);

			releaseLine2.Quantity = 0m;
			var releaseLine1 = orderLine1.ReleaseLines[0];
			releaseLine1.Quantity = 10m;
			AssertEquals(10m, releaseLine1.Quantity);
			AssertEquals(0m, releaseLine1.UnreleasedQty);
			AssertEquals(true, releaseLine2.IsDeleted);

			AssertEquals("Precondition", 1, orderLine1.PickLines.Count);
			AssertEquals("Precondition", 1, orderLine2.PickLines.Count);
			AssertEquals("Precondition", 10m, orderLine1.PickLines[0].WZ_Units);
			AssertEquals("Precondition", 0m, orderLine1.PickLines[0].WZ_OriginalReservedQty);
			AssertEquals("Precondition", 0m, orderLine2.PickLines[0].WZ_Units);
			AssertEquals("Precondition", 10m, orderLine2.PickLines[0].WZ_OriginalReservedQty);
		}

		#endregion

		#region TestReconcilePickLines_ReserveLinesMaintained_WhenStockIsPicked

		public void TestReconcilePickLines_ReserveLinesMaintained_WhenStockIsPicked()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 20m);
			var orderLine1 = order.Lines[0];
			var orderLine2 = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var reserveLine = Helper.CreateReservePickLine(orderLine2, receive.Inventory[0], 10m);

			Helper.CreatePickNew(order);
			var releaseLine2 = orderLine2.ReleaseLines[0];
			AssertEquals("Precondition", false, reserveLine.IsDeleted);
			AssertEquals("Precondition", 10m, reserveLine.WZ_OriginalReservedQty);

			reserveLine.WZ_PickedDateTime = ZDateTimeOffset.Now;
			AssertEquals("Precondition: Inventory should be reduced.", 0m, receive.Inventory[0].WI_TotalUnits);

			releaseLine2.Quantity = 0m;
			var releaseLine1 = orderLine1.ReleaseLines[0];
			releaseLine1.Quantity = 10m;
			AssertEquals(10m, releaseLine1.Quantity);
			AssertEquals(0m, releaseLine1.UnreleasedQty);
			AssertEquals(true, releaseLine2.IsDeleted);

			AssertEquals(1, orderLine1.PickLines.Count);
			AssertEquals(1, orderLine2.PickLines.Count);
			AssertEquals(10m, orderLine1.PickLines[0].WZ_Units);
			AssertEquals(0m, orderLine1.PickLines[0].WZ_OriginalReservedQty);
			AssertEquals(0m, orderLine2.PickLines[0].WZ_Units);
			AssertEquals(10m, orderLine2.PickLines[0].WZ_OriginalReservedQty);
			AssertEquals("Inventory should not have changed Total Units.", 0m, receive.Inventory[0].WI_TotalUnits);
		}

		#endregion

		#region TestReconcilePickLines_ReserveLinesMaintained_WhenStockIsPickedInDB

		public void TestReconcilePickLines_ReserveLinesMaintained_WhenStockIsPickedInDB()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 20m);
			var orderLine1 = order.Lines[0];
			var orderLine2 = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var reserveLine = Helper.CreateReservePickLine(orderLine2, receive.Inventory[0], 10m);

			Helper.CreatePickNew(order);
			var releaseLine2 = orderLine2.ReleaseLines[0];
			AssertEquals("Precondition", false, reserveLine.IsDeleted);
			AssertEquals("Precondition", 10m, reserveLine.WZ_OriginalReservedQty);

			reserveLine.WZ_PickedDateTime = ZDateTimeOffset.Now;
			Factory.Save();
			AssertEquals("Precondition: Inventory should be reduced.", 0m, receive.Inventory[0].WI_TotalUnits);

			releaseLine2.Quantity = 0m;
			var releaseLine1 = orderLine1.ReleaseLines[0];
			releaseLine1.Quantity = 10m;
			AssertEquals(10m, releaseLine1.Quantity);
			AssertEquals(0m, releaseLine1.UnreleasedQty);
			AssertEquals(true, releaseLine2.IsDeleted);

			AssertEquals(1, orderLine1.PickLines.Count);
			AssertEquals(0, orderLine2.PickLines.Count);
			AssertEquals(10m, orderLine1.PickLines[0].WZ_Units);
			AssertEquals(0m, orderLine1.PickLines[0].WZ_OriginalReservedQty);
			AssertEquals("Inventory should not have changed Total Units.", 0m, receive.Inventory[0].WI_TotalUnits);
		}

		#endregion

		#region TestReconcilePickLines_ReserveLinesZeroQuantityNotMoved

		public void TestReconcilePickLines_ReserveLinesZeroQuantityNotMoved()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive1 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "1", data.Part1, 10m);
			var receive2 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "2", data.Part1, 10m, finalise: false);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			var orderLine1 = order.Lines[0];
			var orderLine2 = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var reserveLine = Helper.CreateReservePickLine(orderLine2, receive2.Inventory[0], 10m);

			var pick = Helper.CreatePickNew(order);
			var releaseLine1 = orderLine1.ReleaseLines[0];
			releaseLine1.Quantity = 9m;

			var releaseLine2 = orderLine2.ReleaseLines[0];
			releaseLine2.Quantity = 1m;
			orderLine2.PickLines.Single(pl => !pl.IsReserveLine).WZ_GS_NKAssignedTo = "A"; // just in case order isnt deterministic (seems to be)

			releaseLine2.Quantity = 0m;
			AssertNoExceptionThrown(() => releaseLine1.Quantity = 10m);
		}

		#endregion

		#region TestReconcilePickLines_MultipleOrders

		public void TestReconcilePickLines_MultipleOrders()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "1", data.Part1, 15m);
			Factory.Save();

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 10m);
			var order1Line1 = order1.Lines[0];
			var order1Line2 = Helper.CreateWhsOrderLine(order1, data.Part1, 10m);
			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part1, 10m);
			var order2Line = order2.Lines[0];

			var pick = Helper.CreatePickNew(order1, order2);
			var availableInventory = pick.OrderedInventories[0].AvailableInventories[0];

			AssertEquals("Should have added one release line.", 1, order1Line1.ReleaseLines.Count);
			AssertEquals("Should have added one release line.", 1, order1Line2.ReleaseLines.Count);
			AssertEquals("Should have *no* release lines.", 0, order2Line.ReleaseLines.Count);

			AssertEquals("Should be fully released.", 10m, order1Line1.ReleaseLines[0].Quantity);
			AssertEquals("Should be fully released.", 5m, order1Line2.ReleaseLines[0].Quantity);
			AssertEquals("Should be fully released.", 0m, order1Line1.ReleaseLines[0].UnreleasedQty);
			AssertEquals("Should be fully released.", 0m, order1Line2.ReleaseLines[0].UnreleasedQty);

			order1Line2.ReleaseLines[0].Quantity = 0m;
			AssertEquals("Should have added one release line.", 1, order2Line.ReleaseLines.Count);
			AssertEquals("Precondition.", 0m, order2Line.ReleaseLines[0].Quantity);
			AssertEquals("Precondition.", 5m, order2Line.ReleaseLines[0].UnreleasedQty);

			order2Line.ReleaseLines[0].Quantity = 5m;
			AssertEquals("Should have a pick line.", 1, order1Line1.PickLines.Count);
			AssertEquals("Should have a pick line.", 1, order2Line.PickLines.Count);
			AssertEquals("Should have *no* pick lines.", 0, order1Line2.PickLines.Count);

			AssertEquals(10m, order1Line1.PickLines[0].WZ_Units);
			AssertEquals(5m, order2Line.PickLines[0].WZ_Units);
			AssertNoExceptionThrown(Factory.Save);

			order2Line.ReleaseLines[0].Quantity = 10m;
			AssertEquals("Precondition.", -5m, order2Line.ReleaseLines[0].UnreleasedQty);
			order1Line1.ReleaseLines[0].Quantity = 5m;
			AssertEquals("Precondition.", 0m, order2Line.ReleaseLines[0].UnreleasedQty);

			order2Line.ReleaseLines[0].Quantity = 0m;
			order1Line1.ReleaseLines[0].Quantity = 10m;
			order1Line2.ReleaseLines[0].Quantity = 5m;

			AssertEquals("Should have added one release line.", 1, order1Line1.ReleaseLines.Count);
			AssertEquals("Should have added one release line.", 1, order1Line2.ReleaseLines.Count);
			AssertEquals("Should have *no* release lines.", 0, order2Line.ReleaseLines.Count);

			AssertEquals("Should be fully released.", 10m, order1Line1.ReleaseLines[0].Quantity);
			AssertEquals("Should be fully released.", 5m, order1Line2.ReleaseLines[0].Quantity);
			AssertEquals("Should be fully released.", 0m, order1Line1.ReleaseLines[0].UnreleasedQty);
			AssertEquals("Should be fully released.", 0m, order1Line2.ReleaseLines[0].UnreleasedQty);
			AssertNoExceptionThrown(Factory.Save);
		}

		#endregion

		#region TestReconcilePickLines_MultipleProducts

		public void TestReconcilePickLines_MultipleProducts()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "1", data.Part1, 10m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "2", data.Part2, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			var orderLine1 = order.Lines[0];
			var orderLine2 = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var orderLine3 = Helper.CreateWhsOrderLine(order, data.Part2, 10m);
			var orderLine4 = Helper.CreateWhsOrderLine(order, data.Part2, 10m);
			var pick = Helper.CreatePickNew(order);

			AssertEquals("Precondition.", 1, orderLine1.ReleaseLines.Count);
			AssertEquals("Precondition.", 0, orderLine2.ReleaseLines.Count);
			AssertEquals("Precondition.", 1, orderLine3.ReleaseLines.Count);
			AssertEquals("Precondition.", 0, orderLine4.ReleaseLines.Count);
			AssertEquals("Precondition", 10m, orderLine1.PickLines[0].WZ_Units);
			AssertEquals("Precondition", 10m, orderLine3.PickLines[0].WZ_Units);

			orderLine1.ReleaseLines[0].Quantity = 0m;
			orderLine3.ReleaseLines[0].Quantity = 0m;
			orderLine4.ReleaseLines[0].Quantity = 10m;
			orderLine2.ReleaseLines[0].Quantity = 10m;

			AssertEquals("Should have moved pick lines.", 0, orderLine1.PickLines.Count);
			AssertEquals("Should have moved pick lines.", 1, orderLine2.PickLines.Count);
			AssertEquals("Should have moved pick lines.", 0, orderLine3.PickLines.Count);
			AssertEquals("Should have moved pick lines.", 1, orderLine4.PickLines.Count);

			AssertEquals("Each pick line has 10 units", 10m, orderLine2.PickLines[0].WZ_Units);
			AssertEquals("Each pick line has 10 units", 10m, orderLine4.PickLines[0].WZ_Units);

			AssertNoExceptionThrown(Factory.Save);
		}

		#endregion

		#region TestReconcilePickLines_MultipleClients

		public void TestReconcilePickLines_MultipleClients()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var org2 = Helper.CreateClient("O2");
			Helper.CreateProductClientRelationShip(org2, data.Part1);

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "1", data.Part1, 10m);
			Helper.CreateWhsReceiveWithInventory(org2, data.Whs1, "2", data.Part1, 10m);
			Factory.Save();

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			var order1Line1 = order1.Lines[0];
			var order1Line2 = Helper.CreateWhsOrderLine(order1, data.Part1, 10m);

			var order2 = Helper.CreateWhsOrderWithOrderLine(org2, data.Whs1, data.Part1, 10m);
			var order2Line1 = order2.Lines[0];
			var order2Line2 = Helper.CreateWhsOrderLine(order2, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order1, order2);

			AssertEquals("Precondition.", 1, order1Line1.ReleaseLines.Count);
			AssertEquals("Precondition.", 0, order1Line2.ReleaseLines.Count);
			AssertEquals("Precondition.", 1, order2Line1.ReleaseLines.Count);
			AssertEquals("Precondition.", 0, order2Line2.ReleaseLines.Count);

			order1Line1.ReleaseLines[0].Quantity = 0m;
			order1Line2.ReleaseLines[0].Quantity = 10m;

			order2Line1.ReleaseLines[0].Quantity = 0m;
			order2Line2.ReleaseLines[0].Quantity = 10m;

			AssertEquals("Should have moved pick lines.", 0, order1Line1.PickLines.Count);
			AssertEquals("Should have moved pick lines.", 1, order1Line2.PickLines.Count);
			AssertEquals("Should have moved pick lines.", 0, order2Line1.PickLines.Count);
			AssertEquals("Should have moved pick lines.", 1, order2Line2.PickLines.Count);
			AssertEquals(true, pick.GetAllPickLines().All(pl => pl.InventoryLine.ClientPK == pl.DocketLine.ClientPK));

			AssertNoExceptionThrown(Factory.Save);
		}

		#endregion

		#region TestReconcilePickLines_PreferencesMovingUnconfirmedPickLines

		public void TestReconcilePickLines_PreferencesMovingUnconfirmedPickLines()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive1 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "1", data.Part1, 5m);
			var receive2 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "2", data.Part1, 5m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 10m);
			var orderLine1 = order.Lines[0];
			var orderLine2 = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);

			AssertEquals("Precondition: Should have added one release line.", 1, orderLine1.ReleaseLines.Count);
			AssertEquals("Precondition: Should have *no* release lines.", 0, orderLine2.ReleaseLines.Count);

			AssertEquals("Precondition: Should be fully released.", 10m, orderLine1.ReleaseLines[0].Quantity);
			AssertEquals("Precondition: Should be fully released.", 0m, orderLine1.ReleaseLines[0].UnreleasedQty);

			AssertEquals("Precondition: Should have 2 pick lines.", 2, orderLine1.PickLines.Count);
			AssertEquals("Precondition: Should have 0 pick lines.", 0, orderLine2.PickLines.Count);

			var pickLine1 = orderLine1.PickLines[0];
			var pickLine2 = orderLine1.PickLines[1];
			pickLine1.WZ_GS_NKAssignedTo = "BRS";
			pickLine1.WZ_PickedDateTime = ZDateTimeOffset.Today;

			AssertEquals("Precondition", "", pickLine2.WZ_GS_NKAssignedTo);
			AssertEquals("Precondition", ZDateTimeOffset.Empty, pickLine2.WZ_PickedDateTime);

			orderLine1.ReleaseLines[0].Quantity -= 5m;
			orderLine2.ReleaseLines[0].Quantity += 5m;

			AssertEquals("Should be 1 pick line on each order line.", 1, orderLine1.PickLines.Count);
			AssertEquals("Should be 1 pick line on each order line.", 1, orderLine2.PickLines.Count);

			AssertEquals("Should have moved the pick line with no assigned to/time.", true, orderLine2.PickLines.Contains(pickLine2));

			orderLine1.ReleaseLines[0].Quantity -= 5m;
			orderLine2.ReleaseLines[0].Quantity += 5m;

			AssertEquals("Should have moved confirmed pick lines if necessary.", 0, orderLine1.PickLines.Count);
			AssertEquals("Should have moved confirmed pick lines if necessary.", 2, orderLine2.PickLines.Count);
			AssertContainsExactElementsInAnyOrder(new[] { pickLine1, pickLine2 }, orderLine2.PickLines);
		}

		public void TestReconcilePickLines_PreferencesMovingUnconfirmedPickLines_InTransit()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive1 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "1", data.Part1, 5m);
			var receive2 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "2", data.Part1, 5m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 10m);
			var orderLine1 = order.Lines[0];
			var orderLine2 = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);

			AssertEquals("Precondition: Should have added one release line.", 1, orderLine1.ReleaseLines.Count);
			AssertEquals("Precondition: Should have *no* release lines.", 0, orderLine2.ReleaseLines.Count);

			AssertEquals("Precondition: Should be fully released.", 10m, orderLine1.ReleaseLines[0].Quantity);
			AssertEquals("Precondition: Should be fully released.", 0m, orderLine1.ReleaseLines[0].UnreleasedQty);

			AssertEquals("Precondition: Should have 2 pick lines.", 2, orderLine1.PickLines.Count);
			AssertEquals("Precondition: Should have 0 pick lines.", 0, orderLine2.PickLines.Count);

			var pickLine1 = orderLine1.PickLines[0];
			var pickLine2 = orderLine1.PickLines[1];
			Helper.PickAndMakeInTransitTransfer(pickLine1, ZDateTimeOffset.Today);
			AssertEquals("Precondition", "", pickLine2.WZ_GS_NKAssignedTo);
			AssertEquals("Precondition", ZDateTimeOffset.Empty, pickLine2.WZ_PickedDateTime);

			orderLine1.ReleaseLines[0].Quantity -= 5m;
			orderLine2.ReleaseLines[0].Quantity += 5m;

			AssertEquals("Should be 1 pick line on each order line.", 1, orderLine1.PickLines.Count);
			AssertEquals("Should be 1 pick line on each order line.", 1, orderLine2.PickLines.Count);

			AssertEquals("Should have moved the pick line with no assigned to/time.", true, orderLine2.PickLines.Contains(pickLine2));

			orderLine1.ReleaseLines[0].Quantity -= 5m;
			orderLine2.ReleaseLines[0].Quantity += 5m;

			AssertEquals("Should have moved confirmed pick lines if necessary.", 0, orderLine1.PickLines.Count);
			AssertEquals("Should have moved confirmed pick lines if necessary.", 2, orderLine2.PickLines.Count);
			AssertContainsExactElementsInAnyOrder(new[] { pickLine1, pickLine2 }, orderLine2.PickLines);
		}

		#endregion

		#region TestReconcilePickLines_OnlyMergesSimilarConfirmedPickLines

		public void TestReconcilePickLines_OnlyMergesSimilarConfirmedPickLines()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive1 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 10m);
			var orderLine1 = order.Lines[0];
			var orderLine2 = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);
			AssertEquals("Precondition: Should have 1 pick lines.", 1, orderLine1.PickLines.Count);
			AssertEquals("Precondition: Should have 0 pick lines.", 0, orderLine2.PickLines.Count);

			var pickLine1 = orderLine1.PickLines[0];
			var pickLine2 = orderLine1.PickLines[0].Split(2m);
			var pickLine3 = orderLine1.PickLines[0].Split(2m);
			var pickLine4 = orderLine1.PickLines[0].Split(2m);
			var pickLine5 = orderLine1.PickLines[0].Split(2m);

			pickLine1.WZ_GS_NKAssignedTo = "BRS";
			pickLine1.WZ_PickedDateTime = ZDateTimeOffset.Today;

			pickLine2.WZ_GS_NKAssignedTo = "BRS";
			pickLine2.WZ_PickedDateTime = ZDateTimeOffset.Today;
			pickLine2.WZ_WE_TransactionLine = orderLine2.PK;
			orderLine1.PickLines.RemoveFromRelationship(pickLine2);
			orderLine2.PickLines.Add(pickLine2);

			pickLine3.WZ_GS_NKAssignedTo = "BRS";
			pickLine3.WZ_PickedDateTime = ZDateTimeOffset.Today.AddDays(-1);

			pickLine4.WZ_GS_NKAssignedTo = "MMC";
			pickLine4.WZ_PickedDateTime = ZDateTimeOffset.Today;

			pickLine5.WZ_WE_TransactionLine = orderLine2.PK;
			orderLine1.PickLines.RemoveFromRelationship(pickLine5);
			orderLine2.PickLines.Add(pickLine5);

			// this test is specifically testing picked time, so we prevent creating In-Transit Transfer Lines here.
			using (OutboundDockDoorHelper.MockOutboundDockDoorCreator())
			{
				Factory.Save();
			}

			AssertEquals("Precondition", 3, orderLine1.PickLines.Count);
			AssertEquals("Precondition", 2, orderLine2.PickLines.Count);

			orderLine1.ReleaseLines[0].Quantity = 0m;
			orderLine2.ReleaseLines[0].Quantity = 10m;
			AssertEquals("Should have only merged similar pick lines.", 0, orderLine1.PickLines.Count);
			AssertEquals("Should have only merged similar pick lines.", 4, orderLine2.PickLines.Count);

			AssertEquals("Should have only merged similar pick lines.", 4m, orderLine2.PickLines.Single(pl => pl.WZ_PickedDateTime == ZDateTimeOffset.Today && pl.WZ_GS_NKAssignedTo == "BRS").WZ_Units);
			AssertEquals("Should have only merged similar pick lines.", 2m, orderLine2.PickLines.Single(pl => pl.WZ_PickedDateTime == ZDateTimeOffset.Empty && pl.WZ_GS_NKAssignedTo == "").WZ_Units);
			AssertEquals("Should have only merged similar pick lines.", 2m, orderLine2.PickLines.Single(pl => pl.WZ_PickedDateTime == ZDateTimeOffset.Today.AddDays(-1) && pl.WZ_GS_NKAssignedTo == "BRS").WZ_Units);
			AssertEquals("Should have only merged similar pick lines.", 2m, orderLine2.PickLines.Single(pl => pl.WZ_PickedDateTime == ZDateTimeOffset.Today && pl.WZ_GS_NKAssignedTo == "MMC").WZ_Units);
		}

		#endregion

		#region TestReconcilePickLines_OnlyMergesSimilarConfirmedPickLines_InTransit

		public void TestReconcilePickLines_OnlyMergesSimilarConfirmedPickLines_InTransit()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 50m);
			var originalInventory = receive.Lines[0];
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine1 = Helper.CreateWhsOrderLine(order, data.Part1, 20m);
			var orderLine2 = Helper.CreateWhsOrderLine(order, data.Part1, 20m);
			var pick = Helper.CreatePickNew(order);

			var otherReceive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 50m);
			var otherInventory = otherReceive.Lines[0];
			Factory.Save();

			var pickLine = orderLine1.PickLines.Single();
			pickLine.WZ_Units = 10m;
			Helper.PickAndMakeInTransitTransfer(pickLine, ZDateTimeOffset.Now);

			var newPickLine = orderLine1.PickLines.Single();
			AssertEquals("Precondition.", originalInventory.PK, newPickLine.WZ_WE_OriginalPickedInventoryLine);

			// Delete all pick lines on orderline 2 so we can create some with our intended datashape
			orderLine2.PickLines.DeleteAll();

			// Correct inventory, no OriginalPickedInventory
			var dodgyPickLine1 = Factory.New<WhsPickLine>();
			dodgyPickLine1.WZ_Units = 1m;
			dodgyPickLine1.WZ_GS_NKAssignedTo = newPickLine.WZ_GS_NKAssignedTo;
			dodgyPickLine1.WZ_WE_TransactionLine = orderLine2.PK;
			dodgyPickLine1.WZ_WE_InventoryLine = newPickLine.WZ_WE_InventoryLine;

			// Correct Inventory, incorrect OriginalPickedInventory
			var dodgyPickLine2 = Factory.New<WhsPickLine>();
			dodgyPickLine2.WZ_Units = 1m;
			dodgyPickLine2.WZ_GS_NKAssignedTo = newPickLine.WZ_GS_NKAssignedTo;
			dodgyPickLine2.WZ_WE_TransactionLine = orderLine2.PK;
			dodgyPickLine2.WZ_WE_InventoryLine = newPickLine.WZ_WE_InventoryLine;
			dodgyPickLine2.WZ_WE_OriginalPickedInventoryLine = otherInventory.PK;

			// Incorrect inventory, correct OriginalPickedInventory 
			var dodgyPickLine3 = Factory.New<WhsPickLine>();
			dodgyPickLine3.WZ_Units = 1m;
			dodgyPickLine3.WZ_GS_NKAssignedTo = newPickLine.WZ_GS_NKAssignedTo;
			dodgyPickLine3.WZ_WE_TransactionLine = orderLine2.PK;
			dodgyPickLine3.WZ_WE_InventoryLine = otherInventory.PK;
			dodgyPickLine3.WZ_WE_OriginalPickedInventoryLine = newPickLine.WZ_WE_OriginalPickedInventoryLine;

			// Correct inventory, correct OriginalPickedInventory 
			var matchingPickLine = Factory.New<WhsPickLine>();
			matchingPickLine.WZ_Units = 1m;
			matchingPickLine.WZ_GS_NKAssignedTo = newPickLine.WZ_GS_NKAssignedTo;
			matchingPickLine.WZ_WE_TransactionLine = orderLine2.PK;
			matchingPickLine.WZ_WE_InventoryLine = newPickLine.WZ_WE_InventoryLine;
			matchingPickLine.WZ_WE_OriginalPickedInventoryLine = newPickLine.WZ_WE_OriginalPickedInventoryLine;

			AssertEquals("Precondition.", 1, orderLine1.PickLines.Count);
			AssertEquals("Precondition.", 4, orderLine2.PickLines.Count);
			AssertEquals("Precondition", 10m, newPickLine.WZ_Units);
			AssertEquals("Precondition", 1m, matchingPickLine.WZ_Units);

			orderLine2.ClearReleaseLines();
			orderLine2.ReleaseLines[0].Quantity = 0m;
			orderLine1.ReleaseLines[0].Quantity += 4m;
			AssertEquals("Should have moved/merged pick lines.", 0, orderLine2.PickLines.Count);
			AssertEquals("Should have moved/merged pick lines.", 4, orderLine1.PickLines.Count);
			AssertEquals("Should have merged the pickline with correct inventory and OriginalPickedInventory.", true, matchingPickLine.IsDeleted);
			AssertEquals("Should have merged the pickline with correct inventory and OriginalPickedInventory.", 11m, newPickLine.WZ_Units);
			AssertEquals("Should *not* have merged other picklines.", false, dodgyPickLine1.IsDeleted);
			AssertEquals("Should *not* have merged other picklines.", false, dodgyPickLine2.IsDeleted);
			AssertEquals("Should *not* have merged other picklines.", false, dodgyPickLine3.IsDeleted);
			AssertEquals("Should *not* have merged other picklines.", 1m, dodgyPickLine1.WZ_Units);
			AssertEquals("Should *not* have merged other picklines.", 1m, dodgyPickLine2.WZ_Units);
			AssertEquals("Should *not* have merged other picklines.", 1m, dodgyPickLine3.WZ_Units);
		}

		#endregion

		#region TestReconcilePickLines_PickByBOM

		public void TestReconcilePickLines_PickByBOM()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var bike = Helper.CreateProduct(data.Org1, "BIKE");
			bike.OP_IsComponentPickedOnSalesOrder = true;

			var wheel = Helper.CreateProduct(data.Org1, "WHEEL");
			var frame = Helper.CreateProduct(data.Org1, "FRAME");
			Helper.CreateProductBOM(bike, wheel, 2m, Constants.PkgUnit.Unit);
			Helper.CreateProductBOM(bike, frame, 1m, Constants.PkgUnit.Unit);
			var location = data.Whs1.FindLocation("A-1");
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveInventoryLine(receive, wheel, 20m, location);
			Helper.CreateWhsReceiveInventoryLine(receive, frame, 10m, location);
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertEquals("Receive should be finalised.", true, receive.IsFinalised);
			Factory.Save();

			var kitOrder = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "KIT", bike, 10m);
			var kitOrderLine1 = kitOrder.Lines[0];
			var kitOrderLine2 = Helper.CreateWhsOrderLine(kitOrder, bike, 10m);

			var pick = Helper.CreatePickNew(kitOrder);
			AssertEquals("Precondition - Picked By BOM", true, kitOrderLine1.IsBOMProductPickedOnSalesOrder);
			AssertEquals("Precondition - Picked By BOM", true, kitOrderLine2.IsBOMProductPickedOnSalesOrder);
			AssertEquals("Precondition - Picked By BOM", true, kitOrderLine1.ChildComponentLines.Count > 0);
			AssertEquals("Precondition - No Lines Picked By BOM Yet", false, kitOrderLine2.ChildComponentLines.Count > 0);

			var wheelOrderLine1 = kitOrderLine1.ChildComponentLines.Single(l => l.WE_OP == wheel.PK);
			var frameOrderLine1 = kitOrderLine1.ChildComponentLines.Single(l => l.WE_OP == frame.PK);
			var createdReceive = Factory.LoadTop1<WhsReceive>(new ZQuery(WhsDocketSchema.WD_WP_ParentPickForReceive, pick.PK));
			WhsReceiveLine createdReceiveLine1;
			WhsBOMInventoryPivot bomLink1 = null;
			WhsBOMInventoryPivot bomLink2 = null;
			WhsPickLine kitPickLine1 = null;
			WhsPickLine kitPickLine2 = null;

			AssertEquals("Precondition - Kit Pick Line created from Children.", 1, kitOrderLine1.PickLines.Count);
			kitPickLine1 = kitOrderLine1.PickLines[0];
			AssertEquals("Precondition - Kit Pick Line created from Children.", 10m, kitPickLine1.WZ_Units);
			AssertEquals("Precondition - No Lines for kitOrderLine2.", 0, kitOrderLine2.PickLines.Count);

			createdReceiveLine1 = createdReceive.Lines[0];
			AssertEquals("Precondition - Receive Line created.", 10m, createdReceiveLine1.WE_TransactionQuantity);
			var bomLinks = createdReceiveLine1.BOMComponentLinks;
			AssertEquals("Precondition - Links created.", 2, bomLinks.Count());
			bomLink1 = bomLinks.Single(l => l.WIP_WE_ComponentLine == wheelOrderLine1.PK);
			bomLink2 = bomLinks.Single(l => l.WIP_WE_ComponentLine == frameOrderLine1.PK);
			AssertEquals("Precondition - Links created.", 20m, bomLink1.WIP_ComponentQuantity);
			AssertEquals("Precondition - Links created.", 10m, bomLink2.WIP_ComponentQuantity);

			AssertEquals("Precondition.", 1, kitOrderLine1.ReleaseLines.Count);
			AssertEquals("Precondition.", 0, kitOrderLine2.ReleaseLines.Count);

			AssertEquals("Precondition.", 10m, kitOrderLine1.ReleaseLines[0].Quantity);
			AssertEquals("Precondition.", 0m, kitOrderLine1.ReleaseLines[0].UnreleasedQty);

			kitOrderLine1.ReleaseLines[0].Quantity = 0m;
			kitOrderLine2.ReleaseLines[0].Quantity = 10m;

			var wheelComponentInvOrderedInv = pick.OrderedInventories.Cast<WhsPickOrderedInventory>().Single(ordInv => ordInv.SupplierPartPK == wheel.PK && ordInv.IsComponentOrderedInventoryOnSalesOrder);
			var frameComponentInvOrderedInv = pick.OrderedInventories.Cast<WhsPickOrderedInventory>().Single(ordInv => ordInv.SupplierPartPK == frame.PK && ordInv.IsComponentOrderedInventoryOnSalesOrder);
			AssertEquals(false, kitOrderLine1.ChildComponentLines.Count > 0);
			AssertEquals(true, kitOrderLine2.ChildComponentLines.Count > 0);
			AssertEquals(1, wheelComponentInvOrderedInv.Owners.Count);
			AssertEquals(1, frameComponentInvOrderedInv.Owners.Count);
			AssertEquals(true, wheelComponentInvOrderedInv.Owners.Contains(kitOrderLine2.ChildComponentLines.First(ccl => ccl.ProductCode == "WHEEL")));
			AssertEquals(true, frameComponentInvOrderedInv.Owners.Contains(kitOrderLine2.ChildComponentLines.First(ccl => ccl.ProductCode == "FRAME")));
			AssertEquals(0, kitOrderLine1.PickLinesForRelease.Count);
			AssertEquals(2, kitOrderLine2.ChildComponentLines.Sum(ccl => ccl.PickLines.Count));
			AssertEquals(20m, kitOrderLine2.ChildComponentLines.SelectMany(ccl => ccl.PickLines).Single(pl => pl.Product.Parent == wheel).WZ_Units);
			AssertEquals(10m, kitOrderLine2.ChildComponentLines.SelectMany(ccl => ccl.PickLines).Single(pl => pl.Product.Parent == frame).WZ_Units);
			AssertNoExceptionThrown(Factory.Save);

			var wheelOrderLine2 = kitOrderLine2.ChildComponentLines.Single(l => l.WE_OP == wheel.PK);
			var frameOrderLine2 = kitOrderLine2.ChildComponentLines.Single(l => l.WE_OP == frame.PK);
			WhsBOMInventoryPivot bomLink3 = null;
			WhsBOMInventoryPivot bomLink4 = null;

			AssertEquals("Kit Pick Line reconciled.", 0, kitOrderLine1.PickLines.Count);
			AssertEquals("Kit Pick Line reconciled.", 1, kitOrderLine2.PickLines.Count);
			kitPickLine2 = kitOrderLine2.PickLines[0];
			AssertEquals("Kit Pick Line reconciled.", 10m, kitPickLine2.WZ_Units);

			createdReceiveLine1 = createdReceive.Lines[0];
			AssertEquals("Receive Line remains the same.", 10m, createdReceiveLine1.WE_TransactionQuantity);
			AssertEquals("Receive Line remains the same.", 10m, createdReceiveLine1.WE_ClientOrderedUnits);
			bomLinks = createdReceiveLine1.BOMComponentLinks;
			AssertEquals("Links reconciled.", 2, bomLinks.Count());
			bomLink3 = bomLinks.Single(l => l.WIP_WE_ComponentLine == wheelOrderLine2.PK);
			bomLink4 = bomLinks.Single(l => l.WIP_WE_ComponentLine == frameOrderLine2.PK);
			AssertEquals("Links reconciled.", 20m, bomLink3.WIP_ComponentQuantity);
			AssertEquals("Links reconciled.", 10m, bomLink4.WIP_ComponentQuantity);
			AssertEquals("We are moving lines directly.", true, bomLink1.PK == bomLink3.PK);
			AssertEquals("We are moving lines directly.", true, bomLink2.PK == bomLink4.PK);

			var newFactory1 = new BusinessObjectFactory { RefreshEnabled = false };
			kitOrderLine1 = newFactory1.Load<WhsOrderLine>(kitOrderLine1.PK);
			kitOrderLine2 = newFactory1.Load<WhsOrderLine>(kitOrderLine2.PK);
			wheelOrderLine2 = kitOrderLine2.ChildComponentLines.Single(l => l.WE_OP == wheel.PK);
			frameOrderLine2 = kitOrderLine2.ChildComponentLines.Single(l => l.WE_OP == frame.PK);
			pick = newFactory1.Load<WhsPick>(pick.PK);

			kitOrderLine2.ReleaseLines[0].Quantity = 9m;
			kitOrderLine1.ReleaseLines[0].Quantity = 10m;
			AssertEquals("Precondition.", -9m, kitOrderLine1.ReleaseLines[0].UnreleasedQty);
			kitOrderLine2.ReleaseLines[0].Quantity = 0m;
			wheelOrderLine1 = kitOrderLine1.ChildComponentLines.Single(l => l.WE_OP == wheel.PK);
			frameOrderLine1 = kitOrderLine1.ChildComponentLines.Single(l => l.WE_OP == frame.PK);
			wheelComponentInvOrderedInv = pick.OrderedInventories.Cast<WhsPickOrderedInventory>().Single(ordInv => ordInv.SupplierPartPK == wheel.PK && ordInv.IsComponentOrderedInventoryOnSalesOrder);
			frameComponentInvOrderedInv = pick.OrderedInventories.Cast<WhsPickOrderedInventory>().Single(ordInv => ordInv.SupplierPartPK == frame.PK && ordInv.IsComponentOrderedInventoryOnSalesOrder);

			AssertEquals("Order line 1 should still be picked by BOM.", true, kitOrderLine1.ChildComponentLines.Count > 0);
			AssertEquals("Order line 2 should *NOT* be picked by BOM.", false, kitOrderLine2.ChildComponentLines.Count > 0);
			AssertEquals("Order line 1 should still be picked by BOM.", 2, kitOrderLine1.ChildComponentLines.Sum(ccl => ccl.PickLines.Count));
			AssertEquals("Order line 2 should *NOT* be picked by BOM.", 0, kitOrderLine2.PickLinesForRelease.Count);
			AssertEquals("Order line 1 should still be picked by BOM.", 20m, kitOrderLine1.ChildComponentLines.SelectMany(ccl => ccl.PickLines).Single(pl => pl.Product.Parent.PK == wheel.PK).WZ_Units);
			AssertEquals("Order line 1 should still be picked by BOM.", 10m, kitOrderLine1.ChildComponentLines.SelectMany(ccl => ccl.PickLines).Single(pl => pl.Product.Parent.PK == frame.PK).WZ_Units);
			AssertEquals("ONLY Order line 1 should be picked by BOM.", 1, wheelComponentInvOrderedInv.Owners.Count);
			AssertEquals("ONLY Order line 1 should be picked by BOM.", 1, frameComponentInvOrderedInv.Owners.Count);
			AssertEquals("Order line 1 should still be picked by BOM.", true, wheelComponentInvOrderedInv.Owners.Contains(kitOrderLine1.ChildComponentLines.First(ccl => ccl.ProductCode == "WHEEL")));
			AssertEquals("Order line 1 should still be picked by BOM.", true, frameComponentInvOrderedInv.Owners.Contains(kitOrderLine1.ChildComponentLines.First(ccl => ccl.ProductCode == "FRAME")));
			AssertNoExceptionThrown(newFactory1.Save);

			createdReceive = newFactory1.Load<WhsReceive>(createdReceive.PK);
			kitOrderLine1 = newFactory1.Load<WhsOrderLine>(kitOrderLine1.PK);
			kitOrderLine2 = newFactory1.Load<WhsOrderLine>(kitOrderLine2.PK);
			wheelOrderLine1 = kitOrderLine1.ChildComponentLines.Single(l => l.WE_OP == wheel.PK);
			frameOrderLine1 = kitOrderLine1.ChildComponentLines.Single(l => l.WE_OP == frame.PK);
			AssertEquals("Child Lines of kitOrderLine2 were deleted by reconcile.", 0, kitOrderLine2.ChildComponentLines.Count);
			AssertEquals("Kit Pick Line reconciled.", 1, kitOrderLine1.PickLines.Count);
			kitPickLine1 = kitOrderLine1.PickLines[0];
			AssertEquals("Kit Pick Line reconciled.", 10m, kitPickLine1.WZ_Units);
			AssertEquals("Kit Pick Line reconciled.", 0, kitOrderLine2.PickLines.Count);

			createdReceiveLine1 = createdReceive.Lines[0];
			AssertEquals("Receive Line remains the same.", 10m, createdReceiveLine1.WE_TransactionQuantity);
			AssertEquals("Receive Line remains the same.", 10m, createdReceiveLine1.WE_ClientOrderedUnits);
			bomLinks = createdReceiveLine1.BOMComponentLinks;
			AssertEquals("Links reconciled.", 2, bomLinks.Count());
			bomLink1 = bomLinks.Single(l => l.WIP_WE_ComponentLine == wheelOrderLine1.PK);
			bomLink2 = bomLinks.Single(l => l.WIP_WE_ComponentLine == frameOrderLine1.PK);
			bomLink3 = bomLinks.SingleOrDefault(l => l.WIP_WE_ComponentLine == wheelOrderLine2.PK);
			bomLink4 = bomLinks.SingleOrDefault(l => l.WIP_WE_ComponentLine == frameOrderLine2.PK);
			AssertEquals("Links reconciled.", 20m, bomLink1.WIP_ComponentQuantity);
			AssertEquals("Links reconciled.", 10m, bomLink2.WIP_ComponentQuantity);
			AssertNull("Links reconciled.", bomLink3);
			AssertNull("Links reconciled.", bomLink4);

			var newFactory2 = new BusinessObjectFactory { RefreshEnabled = false };
			kitOrderLine1 = newFactory2.Load<WhsOrderLine>(kitOrderLine1.PK);
			kitOrderLine2 = newFactory2.Load<WhsOrderLine>(kitOrderLine2.PK);
			pick = newFactory2.Load<WhsPick>(pick.PK);

			kitOrderLine1.ReleaseLines[0].Quantity = 5m;
			kitOrderLine2.ReleaseLines[0].Quantity = 5m;
			AssertEquals("Precondition.", 0m, kitOrderLine1.ReleaseLines[0].UnreleasedQty);
			AssertEquals("Precondition.", 0m, kitOrderLine2.ReleaseLines[0].UnreleasedQty);
			wheelComponentInvOrderedInv = pick.OrderedInventories.Cast<WhsPickOrderedInventory>().Single(ordInv => ordInv.SupplierPartPK == wheel.PK && ordInv.IsComponentOrderedInventoryOnSalesOrder);
			frameComponentInvOrderedInv = pick.OrderedInventories.Cast<WhsPickOrderedInventory>().Single(ordInv => ordInv.SupplierPartPK == frame.PK && ordInv.IsComponentOrderedInventoryOnSalesOrder);

			AssertEquals("Both order lines should be picked by BOM.", true, kitOrderLine1.ChildComponentLines.Count > 0);
			AssertEquals("Both order lines should be picked by BOM.", true, kitOrderLine2.ChildComponentLines.Count > 0);
			AssertEquals("Both order lines should be picked by BOM.", 2, kitOrderLine1.ChildComponentLines.Sum(ccl => ccl.PickLines.Count));
			AssertEquals("Both order lines should be picked by BOM.", 2, kitOrderLine2.ChildComponentLines.Sum(ccl => ccl.PickLines.Count));
			AssertEquals("Both order lines should be picked by BOM.", 10m, kitOrderLine1.ChildComponentLines.SelectMany(ccl => ccl.PickLines).Single(pl => pl.Product.Parent.PK == wheel.PK).WZ_Units);
			AssertEquals("Both order lines should be picked by BOM.", 5m, kitOrderLine1.ChildComponentLines.SelectMany(ccl => ccl.PickLines).Single(pl => pl.Product.Parent.PK == frame.PK).WZ_Units);
			AssertEquals("Both order lines should be picked by BOM.", 10m, kitOrderLine2.ChildComponentLines.SelectMany(ccl => ccl.PickLines).Single(pl => pl.Product.Parent.PK == wheel.PK).WZ_Units);
			AssertEquals("Both order lines should be picked by BOM.", 5m, kitOrderLine2.ChildComponentLines.SelectMany(ccl => ccl.PickLines).Single(pl => pl.Product.Parent.PK == frame.PK).WZ_Units);
			AssertEquals("Both order lines should be picked by BOM.", 2, wheelComponentInvOrderedInv.Owners.Count);
			AssertEquals("Both order lines should be picked by BOM.", 2, frameComponentInvOrderedInv.Owners.Count);
			AssertEquals("Both order lines should be picked by BOM.", true, wheelComponentInvOrderedInv.Owners.Contains(kitOrderLine1.ChildComponentLines.First(ccl => ccl.ProductCode == "WHEEL")));
			AssertEquals("Both order lines should be picked by BOM.", true, wheelComponentInvOrderedInv.Owners.Contains(kitOrderLine2.ChildComponentLines.First(ccl => ccl.ProductCode == "WHEEL")));
			AssertEquals("Both order lines should be picked by BOM.", true, frameComponentInvOrderedInv.Owners.Contains(kitOrderLine1.ChildComponentLines.First(ccl => ccl.ProductCode == "FRAME")));
			AssertEquals("Both order lines should be picked by BOM.", true, frameComponentInvOrderedInv.Owners.Contains(kitOrderLine2.ChildComponentLines.First(ccl => ccl.ProductCode == "FRAME")));
			AssertNoExceptionThrown(newFactory2.Save);

			kitOrderLine1 = newFactory2.Load<WhsOrderLine>(kitOrderLine1.PK);
			kitOrderLine2 = newFactory2.Load<WhsOrderLine>(kitOrderLine2.PK);
			AssertEquals("Kit Pick Line reconciled.", 1, kitOrderLine1.PickLines.Count);
			AssertEquals("Kit Pick Line reconciled.", 1, kitOrderLine2.PickLines.Count);
			kitPickLine1 = kitOrderLine1.PickLines[0];
			kitPickLine2 = kitOrderLine2.PickLines[0];
			AssertEquals("Kit Pick Line reconciled.", 5m, kitPickLine1.WZ_Units);
			AssertEquals("Kit Pick Line reconciled.", 5m, kitPickLine2.WZ_Units);
			wheelOrderLine1 = kitOrderLine1.ChildComponentLines.Single(l => l.WE_OP == wheel.PK);
			frameOrderLine1 = kitOrderLine1.ChildComponentLines.Single(l => l.WE_OP == frame.PK);
			wheelOrderLine2 = kitOrderLine2.ChildComponentLines.Single(l => l.WE_OP == wheel.PK);
			frameOrderLine2 = kitOrderLine2.ChildComponentLines.Single(l => l.WE_OP == frame.PK);

			createdReceive = newFactory2.Load<WhsReceive>(createdReceive.PK);
			AssertEquals("Receive Line was split because of reconcile.", 2, createdReceive.Lines.Count);
			AssertEquals("Receive Line was split because of reconcile.", 10m, createdReceive.Lines.Sum(l => l.WE_TransactionQuantity));
			bomLink1 = newFactory2.LoadTop1<WhsBOMInventoryPivot>(new ZQuery(WhsBOMInventoryPivotSchema.WIP_WE_ComponentLine, wheelOrderLine1.PK));
			bomLink2 = newFactory2.LoadTop1<WhsBOMInventoryPivot>(new ZQuery(WhsBOMInventoryPivotSchema.WIP_WE_ComponentLine, frameOrderLine1.PK));
			bomLink3 = newFactory2.LoadTop1<WhsBOMInventoryPivot>(new ZQuery(WhsBOMInventoryPivotSchema.WIP_WE_ComponentLine, wheelOrderLine2.PK));
			bomLink4 = newFactory2.LoadTop1<WhsBOMInventoryPivot>(new ZQuery(WhsBOMInventoryPivotSchema.WIP_WE_ComponentLine, frameOrderLine2.PK));
			AssertEquals("Links reconciled.", 10m, bomLink1.WIP_ComponentQuantity);
			AssertEquals("Links reconciled.", 5m, bomLink2.WIP_ComponentQuantity);
			AssertEquals("Links reconciled.", 10m, bomLink3.WIP_ComponentQuantity);
			AssertEquals("Links reconciled.", 5m, bomLink4.WIP_ComponentQuantity);
			AssertEquals("Receive Line was split because of reconcile.", 5m, bomLink1.InventoryLine.WE_TransactionQuantity);
			AssertEquals("Receive Line was split because of reconcile.", 5m, bomLink1.InventoryLine.WE_ClientOrderedUnits);
			AssertEquals("Receive Line was split because of reconcile.", 5m, bomLink3.InventoryLine.WE_TransactionQuantity);
			AssertEquals("Receive Line was split because of reconcile.", 5m, bomLink3.InventoryLine.WE_ClientOrderedUnits);

			var newFactory3 = new BusinessObjectFactory { RefreshEnabled = false };
			kitOrderLine1 = newFactory3.Load<WhsOrderLine>(kitOrderLine1.PK);
			kitOrderLine2 = newFactory3.Load<WhsOrderLine>(kitOrderLine2.PK);

			kitOrderLine1.ReleaseLines[0].Quantity = 6m;
			kitOrderLine2.ReleaseLines[0].Quantity = 4m;

			AssertEquals("Should be 6 bikes picked by BOM components on order line 1.", 12m, kitOrderLine1.ChildComponentLines.SelectMany(ccl => ccl.PickLines).Single(pl => pl.Product.Parent.PK == wheel.PK).WZ_Units);
			AssertEquals("Should be 6 bikes picked by BOM components on order line 1.", 6m, kitOrderLine1.ChildComponentLines.SelectMany(ccl => ccl.PickLines).Single(pl => pl.Product.Parent.PK == frame.PK).WZ_Units);
			AssertEquals("Should be 4 bikes picked by BOM components on order line 2.", 8m, kitOrderLine2.ChildComponentLines.SelectMany(ccl => ccl.PickLines).Single(pl => pl.Product.Parent.PK == wheel.PK).WZ_Units);
			AssertEquals("Should be 4 bikes picked by BOM components on order line 2.", 4m, kitOrderLine2.ChildComponentLines.SelectMany(ccl => ccl.PickLines).Single(pl => pl.Product.Parent.PK == frame.PK).WZ_Units);
			AssertNoExceptionThrown(newFactory3.Save);

			kitOrderLine1 = newFactory3.Load<WhsOrderLine>(kitOrderLine1.PK);
			kitOrderLine2 = newFactory3.Load<WhsOrderLine>(kitOrderLine2.PK);
			wheelOrderLine1 = kitOrderLine1.ChildComponentLines.Single(l => l.WE_OP == wheel.PK);
			frameOrderLine1 = kitOrderLine1.ChildComponentLines.Single(l => l.WE_OP == frame.PK);
			wheelOrderLine2 = kitOrderLine2.ChildComponentLines.Single(l => l.WE_OP == wheel.PK);
			frameOrderLine2 = kitOrderLine2.ChildComponentLines.Single(l => l.WE_OP == frame.PK);
			AssertEquals("Kit Pick Line reconciled.", 1, kitOrderLine1.PickLines.Count);
			AssertEquals("Kit Pick Line reconciled.", 1, kitOrderLine2.PickLines.Count);
			kitPickLine1 = kitOrderLine1.PickLines[0];
			kitPickLine2 = kitOrderLine2.PickLines[0];
			AssertEquals("Kit Pick Line reconciled.", 6m, kitPickLine1.WZ_Units);
			AssertEquals("Kit Pick Line reconciled.", 4m, kitPickLine2.WZ_Units);

			createdReceive = newFactory3.Load<WhsReceive>(createdReceive.PK);
			AssertEquals("Receive Line was split because of reconcile.", 2, createdReceive.Lines.Count);
			AssertEquals("Receive Line was split because of reconcile.", 10m, createdReceive.Lines.Sum(l => l.WE_TransactionQuantity));
			bomLink1 = newFactory3.LoadTop1<WhsBOMInventoryPivot>(new ZQuery(WhsBOMInventoryPivotSchema.WIP_WE_ComponentLine, wheelOrderLine1.PK));
			bomLink2 = newFactory3.LoadTop1<WhsBOMInventoryPivot>(new ZQuery(WhsBOMInventoryPivotSchema.WIP_WE_ComponentLine, frameOrderLine1.PK));
			bomLink3 = newFactory3.LoadTop1<WhsBOMInventoryPivot>(new ZQuery(WhsBOMInventoryPivotSchema.WIP_WE_ComponentLine, wheelOrderLine2.PK));
			bomLink4 = newFactory3.LoadTop1<WhsBOMInventoryPivot>(new ZQuery(WhsBOMInventoryPivotSchema.WIP_WE_ComponentLine, frameOrderLine2.PK));
			AssertEquals("Links reconciled.", 12m, bomLink1.WIP_ComponentQuantity);
			AssertEquals("Links reconciled.", 6m, bomLink2.WIP_ComponentQuantity);
			AssertEquals("Links reconciled.", 8m, bomLink3.WIP_ComponentQuantity);
			AssertEquals("Links reconciled.", 4m, bomLink4.WIP_ComponentQuantity);
			AssertEquals("Receive Line was split because of reconcile.", 6m, bomLink1.InventoryLine.WE_TransactionQuantity);
			AssertEquals("Receive Line was split because of reconcile.", 6m, bomLink1.InventoryLine.WE_ClientOrderedUnits);
			AssertEquals("Receive Line was split because of reconcile.", 4m, bomLink3.InventoryLine.WE_TransactionQuantity);
			AssertEquals("Receive Line was split because of reconcile.", 4m, bomLink3.InventoryLine.WE_ClientOrderedUnits);

			var newFactory4 = new BusinessObjectFactory { RefreshEnabled = false };
			kitOrderLine1 = newFactory4.Load<WhsOrderLine>(kitOrderLine1.PK);
			kitOrderLine2 = newFactory4.Load<WhsOrderLine>(kitOrderLine2.PK);

			kitOrderLine1.ReleaseLines[0].Quantity = 4m;
			kitOrderLine2.ReleaseLines[0].Quantity = 6m;

			AssertEquals("Should be 4 bikes picked by BOM components on order line 1.", 8m, kitOrderLine1.ChildComponentLines.SelectMany(ccl => ccl.PickLines).Single(pl => pl.Product.Parent.PK == wheel.PK).WZ_Units);
			AssertEquals("Should be 4 bikes picked by BOM components on order line 1.", 4m, kitOrderLine1.ChildComponentLines.SelectMany(ccl => ccl.PickLines).Single(pl => pl.Product.Parent.PK == frame.PK).WZ_Units);
			AssertEquals("Should be 6 bikes picked by BOM components on order line 2.", 12m, kitOrderLine2.ChildComponentLines.SelectMany(ccl => ccl.PickLines).Single(pl => pl.Product.Parent.PK == wheel.PK).WZ_Units);
			AssertEquals("Should be 6 bikes picked by BOM components on order line 2.", 6m, kitOrderLine2.ChildComponentLines.SelectMany(ccl => ccl.PickLines).Single(pl => pl.Product.Parent.PK == frame.PK).WZ_Units);
			AssertNoExceptionThrown(newFactory4.Save);

			kitOrderLine1 = newFactory4.Load<WhsOrderLine>(kitOrderLine1.PK);
			kitOrderLine2 = newFactory4.Load<WhsOrderLine>(kitOrderLine2.PK);
			wheelOrderLine1 = kitOrderLine1.ChildComponentLines.Single(l => l.WE_OP == wheel.PK);
			frameOrderLine1 = kitOrderLine1.ChildComponentLines.Single(l => l.WE_OP == frame.PK);
			wheelOrderLine2 = kitOrderLine2.ChildComponentLines.Single(l => l.WE_OP == wheel.PK);
			frameOrderLine2 = kitOrderLine2.ChildComponentLines.Single(l => l.WE_OP == frame.PK);
			AssertEquals("Kit Pick Line reconciled.", 1, kitOrderLine1.PickLines.Count);
			AssertEquals("Kit Pick Line reconciled.", 1, kitOrderLine2.PickLines.Count);
			kitPickLine1 = kitOrderLine1.PickLines[0];
			kitPickLine2 = kitOrderLine2.PickLines[0];
			AssertEquals("Kit Pick Line reconciled.", 4m, kitPickLine1.WZ_Units);
			AssertEquals("Kit Pick Line reconciled.", 6m, kitPickLine2.WZ_Units);

			createdReceive = newFactory4.Load<WhsReceive>(createdReceive.PK);
			AssertEquals("Receive Line was split because of reconcile.", 2, createdReceive.Lines.Count);
			AssertEquals("Receive Line was split because of reconcile.", 10m, createdReceive.Lines.Sum(l => l.WE_TransactionQuantity));
			bomLink1 = newFactory4.LoadTop1<WhsBOMInventoryPivot>(new ZQuery(WhsBOMInventoryPivotSchema.WIP_WE_ComponentLine, wheelOrderLine1.PK));
			bomLink2 = newFactory4.LoadTop1<WhsBOMInventoryPivot>(new ZQuery(WhsBOMInventoryPivotSchema.WIP_WE_ComponentLine, frameOrderLine1.PK));
			bomLink3 = newFactory4.LoadTop1<WhsBOMInventoryPivot>(new ZQuery(WhsBOMInventoryPivotSchema.WIP_WE_ComponentLine, wheelOrderLine2.PK));
			bomLink4 = newFactory4.LoadTop1<WhsBOMInventoryPivot>(new ZQuery(WhsBOMInventoryPivotSchema.WIP_WE_ComponentLine, frameOrderLine2.PK));
			AssertEquals("Links reconciled.", 8m, bomLink1.WIP_ComponentQuantity);
			AssertEquals("Links reconciled.", 4m, bomLink2.WIP_ComponentQuantity);
			AssertEquals("Links reconciled.", 12m, bomLink3.WIP_ComponentQuantity);
			AssertEquals("Links reconciled.", 6m, bomLink4.WIP_ComponentQuantity);
			AssertEquals("Receive Line was split because of reconcile.", 4m, bomLink1.InventoryLine.WE_TransactionQuantity);
			AssertEquals("Receive Line was split because of reconcile.", 4m, bomLink1.InventoryLine.WE_ClientOrderedUnits);
			AssertEquals("Receive Line was split because of reconcile.", 6m, bomLink3.InventoryLine.WE_TransactionQuantity);
			AssertEquals("Receive Line was split because of reconcile.", 6m, bomLink3.InventoryLine.WE_ClientOrderedUnits);

			var newFactory5 = new BusinessObjectFactory { RefreshEnabled = false };
			kitOrderLine1 = newFactory5.Load<WhsOrderLine>(kitOrderLine1.PK);
			kitOrderLine2 = newFactory5.Load<WhsOrderLine>(kitOrderLine2.PK);

			kitOrderLine1.ReleaseLines[0].Quantity = 10m;
			kitOrderLine2.ReleaseLines[0].Quantity = 0m;

			AssertEquals("Should be 10 bikes picked by BOM components on order line 1.", 20m, kitOrderLine1.ChildComponentLines.SelectMany(ccl => ccl.PickLines).Single(pl => pl.Product.Parent.PK == wheel.PK).WZ_Units);
			AssertEquals("Should be 10 bikes picked by BOM components on order line 1.", 10m, kitOrderLine1.ChildComponentLines.SelectMany(ccl => ccl.PickLines).Single(pl => pl.Product.Parent.PK == frame.PK).WZ_Units);
			AssertEquals("Should be no bikes picked by BOM components on order line 2.", 0, kitOrderLine2.ChildComponentLines.SelectMany(ccl => ccl.PickLines).Count());
			AssertEquals("Should be no bikes picked by BOM components on order line 2.", 0, kitOrderLine2.ChildComponentLines.SelectMany(ccl => ccl.PickLines).Count());
			AssertNoExceptionThrown(newFactory5.Save);

			kitOrderLine1 = newFactory5.Load<WhsOrderLine>(kitOrderLine1.PK);
			kitOrderLine2 = newFactory5.Load<WhsOrderLine>(kitOrderLine2.PK);
			wheelOrderLine1 = kitOrderLine1.ChildComponentLines.Single(l => l.WE_OP == wheel.PK);
			frameOrderLine1 = kitOrderLine1.ChildComponentLines.Single(l => l.WE_OP == frame.PK);
			AssertEquals("Child Lines of kitOrderLine2 were deleted by reconcile.", 0, kitOrderLine2.ChildComponentLines.Count);
			AssertEquals("Kit Pick Line reconciled.", 1, kitOrderLine1.PickLines.Count);
			AssertEquals("Kit Pick Line reconciled.", 0, kitOrderLine2.PickLines.Count);
			kitPickLine1 = kitOrderLine1.PickLines[0];
			AssertEquals("Kit Pick Line reconciled.", 10m, kitPickLine1.WZ_Units);

			createdReceive = newFactory5.Load<WhsReceive>(createdReceive.PK);
			AssertEquals("Receive Line was merged because of reconcile.", 1, createdReceive.Lines.Count);
			AssertEquals("Receive Line was merged because of reconcile.", 10m, createdReceive.Lines[0].WE_TransactionQuantity);
			AssertEquals("Receive Line was merged because of reconcile.", 10m, createdReceive.Lines[0].WE_ClientOrderedUnits);
			bomLink1 = newFactory5.LoadTop1<WhsBOMInventoryPivot>(new ZQuery(WhsBOMInventoryPivotSchema.WIP_WE_ComponentLine, wheelOrderLine1.PK));
			bomLink2 = newFactory5.LoadTop1<WhsBOMInventoryPivot>(new ZQuery(WhsBOMInventoryPivotSchema.WIP_WE_ComponentLine, frameOrderLine1.PK));
			bomLink3 = newFactory5.LoadTop1<WhsBOMInventoryPivot>(new ZQuery(WhsBOMInventoryPivotSchema.WIP_WE_ComponentLine, wheelOrderLine2.PK));
			bomLink4 = newFactory5.LoadTop1<WhsBOMInventoryPivot>(new ZQuery(WhsBOMInventoryPivotSchema.WIP_WE_ComponentLine, frameOrderLine2.PK));
			AssertEquals("Links reconciled.", 20m, bomLink1.WIP_ComponentQuantity);
			AssertEquals("Links reconciled.", 10m, bomLink2.WIP_ComponentQuantity);
			AssertNull("Links reconciled.", bomLink3);
			AssertNull("Links reconciled.", bomLink4);
		}

		#endregion

		#region TestReconcilePickLines_PickByBOM_WithComponentOnSameOrder

		public void TestReconcilePickLines_PickByBOM_WithComponentOnSameOrder()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var bike = Helper.CreateProduct(data.Org1, "BIKE");
			bike.OP_IsComponentPickedOnSalesOrder = true;

			var wheel = Helper.CreateProduct(data.Org1, "WHEEL");
			var frame = Helper.CreateProduct(data.Org1, "FRAME");
			Helper.CreateProductBOM(bike, wheel, 2m, Constants.PkgUnit.Unit);
			Helper.CreateProductBOM(bike, frame, 1m, Constants.PkgUnit.Unit);
			var location = data.Whs1.FindLocation("A-1");
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveInventoryLine(receive, wheel, 21m, location);
			Helper.CreateWhsReceiveInventoryLine(receive, frame, 11m, location);
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertEquals("Receive should be finalised.", true, receive.IsFinalised);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "KIT", bike, 10m);
			var orderLine1 = order.Lines[0];
			var orderLine2 = Helper.CreateWhsOrderLine(order, bike, 10m);
			var orderLine3 = Helper.CreateWhsOrderLine(order, wheel, 1m);
			var orderLine4 = Helper.CreateWhsOrderLine(order, frame, 1m);

			var pick = Helper.CreatePickNew(order);
			AssertEquals("Precondition - Picked By BOM", true, orderLine1.IsBOMProductPickedOnSalesOrder);
			AssertEquals("Precondition - Picked By BOM", true, orderLine2.IsBOMProductPickedOnSalesOrder);
			AssertEquals("Precondition - *NOT* Picked By BOM", false, orderLine3.IsBOMProductPickedOnSalesOrder);
			AssertEquals("Precondition - *NOT* Picked By BOM", false, orderLine4.IsBOMProductPickedOnSalesOrder);

			AssertEquals("Precondition - Picked By BOM", true, orderLine1.ChildComponentLines.Count > 0);
			AssertEquals("Precondition - No Lines Picked By BOM Yet", false, orderLine2.ChildComponentLines.Count > 0);
			AssertEquals("Precondition - No Lines Picked By BOM Yet", false, orderLine3.ChildComponentLines.Count > 0);
			AssertEquals("Precondition - No Lines Picked By BOM Yet", false, orderLine4.ChildComponentLines.Count > 0);

			AssertEquals("Precondition.", 1, orderLine1.ReleaseLines.Count);
			AssertEquals("Precondition.", 0, orderLine2.ReleaseLines.Count);
			AssertEquals("Precondition.", 1, orderLine3.ReleaseLines.Count);
			AssertEquals("Precondition.", 1, orderLine4.ReleaseLines.Count);

			AssertEquals("Precondition.", 10m, orderLine1.ReleaseLines[0].Quantity);
			AssertEquals("Precondition.", 1m, orderLine3.ReleaseLines[0].Quantity);
			AssertEquals("Precondition.", 1m, orderLine4.ReleaseLines[0].Quantity);

			AssertEquals("Precondition.", 0m, orderLine1.ReleaseLines[0].UnreleasedQty);
			AssertEquals("Precondition.", 0m, orderLine3.ReleaseLines[0].UnreleasedQty);
			AssertEquals("Precondition.", 0m, orderLine4.ReleaseLines[0].UnreleasedQty);

			var wheelOrderLine1 = orderLine1.ChildComponentLines.Single(l => l.WE_OP == wheel.PK);
			var frameOrderLine1 = orderLine1.ChildComponentLines.Single(l => l.WE_OP == frame.PK);
			var createdReceive = Factory.LoadTop1<WhsReceive>(new ZQuery(WhsDocketSchema.WD_WP_ParentPickForReceive, pick.PK));
			WhsReceiveLine createdReceiveLine1;
			WhsBOMInventoryPivot bomLink1 = null;
			WhsBOMInventoryPivot bomLink2 = null;
			WhsPickLine kitPickLine1 = null;
			WhsPickLine kitPickLine2 = null;

			AssertEquals("Precondition - Kit Pick Line created from Children.", 1, orderLine1.PickLines.Count);
			kitPickLine1 = orderLine1.PickLines[0];
			AssertEquals("Precondition - Kit Pick Line created from Children.", 10m, kitPickLine1.WZ_Units);
			AssertEquals("Precondition - No Lines for orderLine2.", 0, orderLine2.PickLines.Count);

			createdReceiveLine1 = createdReceive.Lines[0];
			AssertEquals("Precondition - Receive Line created.", 10m, createdReceiveLine1.WE_TransactionQuantity);
			AssertEquals("Precondition - Receive Line created.", 10m, createdReceiveLine1.WE_ClientOrderedUnits);
			var bomLinks = createdReceiveLine1.BOMComponentLinks;
			AssertEquals("Precondition - Links created.", 2, bomLinks.Count());
			bomLink1 = bomLinks.Single(l => l.WIP_WE_ComponentLine == wheelOrderLine1.PK);
			bomLink2 = bomLinks.Single(l => l.WIP_WE_ComponentLine == frameOrderLine1.PK);
			AssertEquals("Precondition - Links created.", 20m, bomLink1.WIP_ComponentQuantity);
			AssertEquals("Precondition - Links created.", 10m, bomLink2.WIP_ComponentQuantity);

			orderLine1.ReleaseLines[0].Quantity = 0m;
			orderLine2.ReleaseLines[0].Quantity = 10m;

			AssertEquals("Should have moved component lines.", false, orderLine1.ChildComponentLines.Count > 0);
			AssertEquals("Should have moved component lines.", true, orderLine2.ChildComponentLines.Count > 0);
			AssertEquals("Should have moved picklines.", 0, orderLine1.PickLinesForRelease.Count);
			AssertEquals("Should have moved picklines.", 2, orderLine2.ChildComponentLines.Sum(ccl => ccl.PickLines.Count));
			AssertEquals("Should have moved picklines", 20m, orderLine2.ChildComponentLines.SelectMany(ccl => ccl.PickLines).Single(pl => pl.Product.Parent == wheel).WZ_Units);
			AssertEquals("Should have moved picklines", 10m, orderLine2.ChildComponentLines.SelectMany(ccl => ccl.PickLines).Single(pl => pl.Product.Parent == frame).WZ_Units);

			var wheelOrderLine2 = orderLine2.ChildComponentLines.Single(l => l.WE_OP == wheel.PK);
			var frameOrderLine2 = orderLine2.ChildComponentLines.Single(l => l.WE_OP == frame.PK);
			WhsBOMInventoryPivot bomLink3 = null;
			WhsBOMInventoryPivot bomLink4 = null;

			AssertEquals("Kit Pick Line reconciled.", 0, orderLine1.PickLines.Count);
			AssertEquals("Kit Pick Line reconciled.", 1, orderLine2.PickLines.Count);
			kitPickLine2 = orderLine2.PickLines[0];
			AssertEquals("Kit Pick Line reconciled.", 10m, kitPickLine2.WZ_Units);

			createdReceiveLine1 = createdReceive.Lines[0];
			AssertEquals("Receive Line remains the same.", 10m, createdReceiveLine1.WE_TransactionQuantity);
			AssertEquals("Receive Line remains the same.", 10m, createdReceiveLine1.WE_ClientOrderedUnits);
			bomLinks = createdReceiveLine1.BOMComponentLinks;
			AssertEquals("Links reconciled.", 2, bomLinks.Count());
			bomLink3 = bomLinks.Single(l => l.WIP_WE_ComponentLine == wheelOrderLine2.PK);
			bomLink4 = bomLinks.Single(l => l.WIP_WE_ComponentLine == frameOrderLine2.PK);
			AssertEquals("Links reconciled.", 20m, bomLink3.WIP_ComponentQuantity);
			AssertEquals("Links reconciled.", 10m, bomLink4.WIP_ComponentQuantity);
			AssertEquals("We are moving lines directly.", true, bomLink1.PK == bomLink3.PK);
			AssertEquals("We are moving lines directly.", true, bomLink2.PK == bomLink4.PK);

			AssertEquals("Should be unchanged.", 1, orderLine3.PickLines.Count);
			AssertEquals("Should be unchanged.", 1, orderLine4.PickLines.Count);
			AssertEquals("Should be unchanged", 1m, orderLine3.PickLines[0].WZ_Units);
			AssertEquals("Should be unchanged", 1m, orderLine4.PickLines[0].WZ_Units);

			orderLine2.ReleaseLines[0].Quantity = 9m;
			orderLine1.ReleaseLines[0].Quantity = 10m;
			AssertEquals("Precondition.", -9m, orderLine1.ReleaseLines[0].UnreleasedQty);
			orderLine2.ReleaseLines[0].Quantity = 0m;

			AssertEquals("Should have moved component lines.", true, orderLine1.ChildComponentLines.Count > 0);
			AssertEquals("Should have moved component lines.", false, orderLine2.ChildComponentLines.Count > 0);
			AssertEquals("Should have moved picklines.", 2, orderLine1.ChildComponentLines.Sum(ccl => ccl.PickLines.Count));
			AssertEquals("Should have moved picklines.", 0, orderLine2.PickLinesForRelease.Count);
			AssertEquals("Should have moved picklines", 20m, orderLine1.ChildComponentLines.SelectMany(ccl => ccl.PickLines).Single(pl => pl.Product.Parent == wheel).WZ_Units);
			AssertEquals("Should have moved picklines", 10m, orderLine1.ChildComponentLines.SelectMany(ccl => ccl.PickLines).Single(pl => pl.Product.Parent == frame).WZ_Units);

			AssertEquals("Kit Pick Line reconciled.", 1, orderLine1.PickLines.Count);
			kitPickLine1 = orderLine1.PickLines[0];
			AssertEquals("Kit Pick Line reconciled.", 10m, kitPickLine1.WZ_Units);
			AssertEquals("Kit Pick Line reconciled.", 0, orderLine2.PickLines.Count);

			createdReceiveLine1 = createdReceive.Lines[0];
			AssertEquals("Receive Line remains the same.", 10m, createdReceiveLine1.WE_TransactionQuantity);
			AssertEquals("Receive Line remains the same.", 10m, createdReceiveLine1.WE_ClientOrderedUnits);
			bomLinks = createdReceiveLine1.BOMComponentLinks;
			AssertEquals("Links reconciled.", 2, bomLinks.Count());
			wheelOrderLine1 = orderLine1.ChildComponentLines.Single(l => l.WE_OP == wheel.PK);
			frameOrderLine1 = orderLine1.ChildComponentLines.Single(l => l.WE_OP == frame.PK);
			bomLink1 = bomLinks.Single(l => l.WIP_WE_ComponentLine == wheelOrderLine1.PK);
			bomLink2 = bomLinks.Single(l => l.WIP_WE_ComponentLine == frameOrderLine1.PK);
			AssertEquals("Links reconciled.", 20m, bomLink1.WIP_ComponentQuantity);
			AssertEquals("Links reconciled.", 10m, bomLink2.WIP_ComponentQuantity);
			AssertEquals("Links reconciled.", true, bomLink3.IsDeleted);
			AssertEquals("Links reconciled.", true, bomLink4.IsDeleted);

			AssertEquals("Should be unchanged.", 1, orderLine3.PickLines.Count);
			AssertEquals("Should be unchanged.", 1, orderLine4.PickLines.Count);
			AssertEquals("Should be unchanged", 1m, orderLine3.PickLines[0].WZ_Units);
			AssertEquals("Should be unchanged", 1m, orderLine4.PickLines[0].WZ_Units);

			AssertNoExceptionThrown(Factory.Save);
		}

		#endregion

		#region TestReconcilePickLines_PickByBOM_PartiallyPickedByKits

		public void TestReconcilePickLines_PickByBOM_PartiallyPickedByKits()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var bike = Helper.CreateProduct(data.Org1, "BIKE");
			bike.OP_IsComponentPickedOnSalesOrder = true;

			var wheel = Helper.CreateProduct(data.Org1, "WHEEL");
			var frame = Helper.CreateProduct(data.Org1, "FRAME");
			Helper.CreateProductBOM(bike, wheel, 2m, Constants.PkgUnit.Unit);
			Helper.CreateProductBOM(bike, frame, 1m, Constants.PkgUnit.Unit);
			var location = data.Whs1.FindLocation("A-1");
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var bikeInventoryLine = Helper.CreateWhsReceiveInventoryLine(receive, bike, 5m, location);
			Helper.CreateWhsReceiveInventoryLine(receive, wheel, 10m, location);
			Helper.CreateWhsReceiveInventoryLine(receive, frame, 5m, location);
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertEquals("Receive should be finalised.", true, receive.IsFinalised);
			Factory.Save();

			var kitOrder = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "KIT", bike, 10m);
			var kitOrderLine1 = kitOrder.Lines[0];
			var kitOrderLine2 = Helper.CreateWhsOrderLine(kitOrder, bike, 10m);

			var pick = Helper.CreatePickNew(kitOrder);
			AssertEquals("Precondition - Picked By BOM", true, kitOrderLine1.IsBOMProductPickedOnSalesOrder);
			AssertEquals("Precondition - Picked By BOM", true, kitOrderLine2.IsBOMProductPickedOnSalesOrder);
			AssertEquals("Precondition - Picked By BOM", true, kitOrderLine1.ChildComponentLines.Count > 0);
			AssertEquals("Precondition - No Lines Picked By BOM Yet", false, kitOrderLine2.ChildComponentLines.Count > 0);

			AssertEquals("Precondition.", 1, kitOrderLine1.ReleaseLines.Count);
			AssertEquals("Precondition.", 0, kitOrderLine2.ReleaseLines.Count);

			AssertEquals("Precondition.", 10m, kitOrderLine1.ReleaseLines[0].Quantity);
			AssertEquals("Precondition.", 0m, kitOrderLine1.ReleaseLines[0].UnreleasedQty);

			var wheelOrderLine1 = kitOrderLine1.ChildComponentLines.Single(l => l.WE_OP == wheel.PK);
			var frameOrderLine1 = kitOrderLine1.ChildComponentLines.Single(l => l.WE_OP == frame.PK);
			var createdReceive = Factory.LoadTop1<WhsReceive>(new ZQuery(WhsDocketSchema.WD_WP_ParentPickForReceive, pick.PK));
			WhsReceiveLine createdReceiveLine1;
			WhsBOMInventoryPivot bomLink1 = null;
			WhsBOMInventoryPivot bomLink2 = null;
			var kitPickLine0 = kitOrderLine1.PickLines.Single(l => l.InventoryLine.PK == bikeInventoryLine.PK);
			WhsPickLine kitPickLine1 = null;
			WhsPickLine kitPickLine2 = null;

			AssertEquals("Precondition - One more Kit Pick Line created from Children.", 2, kitOrderLine1.PickLines.Count);
			kitPickLine1 = kitOrderLine1.PickLines.Single(l => l.PK != kitPickLine0.PK);
			AssertEquals("Precondition - Kit Pick Line created from Children.", 5m, kitPickLine1.WZ_Units);
			AssertEquals("Precondition - No Lines for orderLine2.", 0, kitOrderLine2.PickLines.Count);

			createdReceiveLine1 = createdReceive.Lines[0];
			AssertEquals("Precondition - Receive Line created.", 5m, createdReceiveLine1.WE_TransactionQuantity);
			AssertEquals("Precondition - Receive Line created.", 5m, createdReceiveLine1.WE_ClientOrderedUnits);
			var bomLinks = createdReceiveLine1.BOMComponentLinks;
			AssertEquals("Precondition - Links created.", 2, bomLinks.Count());
			bomLink1 = bomLinks.Single(l => l.WIP_WE_ComponentLine == wheelOrderLine1.PK);
			bomLink2 = bomLinks.Single(l => l.WIP_WE_ComponentLine == frameOrderLine1.PK);
			AssertEquals("Precondition - Links created.", 10m, bomLink1.WIP_ComponentQuantity);
			AssertEquals("Precondition - Links created.", 5m, bomLink2.WIP_ComponentQuantity);

			kitOrderLine1.ReleaseLines[0].Quantity = 0m;
			kitOrderLine2.ReleaseLines[0].Quantity = 10m;

			AssertEquals("Should have moved component lines", false, kitOrderLine1.ChildComponentLines.Count > 0);
			AssertEquals("Should have moved component lines", true, kitOrderLine2.ChildComponentLines.Count > 0);
			AssertEquals("Should have moved pick lines", 0, kitOrderLine1.PickLinesForRelease.Count);
			AssertEquals("Should have moved pick lines", 2, kitOrderLine2.PickLines.Count);
			AssertEquals("Should have moved pick lines", 2, kitOrderLine2.ChildComponentLines.Sum(ccl => ccl.PickLines.Count));
			AssertEquals("Should have moved pick lines", 10m, kitOrderLine2.ChildComponentLines.SelectMany(ccl => ccl.PickLines).Single(pl => pl.Product.Parent == wheel).WZ_Units);
			AssertEquals("Should have moved pick lines", 5m, kitOrderLine2.ChildComponentLines.SelectMany(ccl => ccl.PickLines).Single(pl => pl.Product.Parent == frame).WZ_Units);
			AssertEquals("Should have moved pick lines", 10m, kitOrderLine2.PickLines.Sum(pl => pl.WZ_Units));

			var wheelOrderLine2 = kitOrderLine2.ChildComponentLines.Single(l => l.WE_OP == wheel.PK);
			var frameOrderLine2 = kitOrderLine2.ChildComponentLines.Single(l => l.WE_OP == frame.PK);
			WhsBOMInventoryPivot bomLink3 = null;
			WhsBOMInventoryPivot bomLink4 = null;

			AssertEquals("Kit Pick Line reconciled.", 0, kitOrderLine1.PickLines.Count);
			AssertEquals("Kit Pick Line reconciled.", 0, kitOrderLine1.ChildComponentLines.Count);
			AssertEquals("Kit Pick Line reconciled.", 2, kitOrderLine2.PickLines.Count);
			AssertEquals("Kit Pick Line reconciled.", kitOrderLine2.PK, kitPickLine0.WZ_WE_TransactionLine);
			kitPickLine2 = kitOrderLine2.PickLines.Single(l => l.PK != kitPickLine0.PK);
			AssertEquals("Kit Pick Line reconciled.", 5m, kitPickLine2.WZ_Units);

			createdReceiveLine1 = createdReceive.Lines[0];
			AssertEquals("Receive Line remains the same.", 5m, createdReceiveLine1.WE_TransactionQuantity);
			AssertEquals("Receive Line remains the same.", 5m, createdReceiveLine1.WE_ClientOrderedUnits);
			bomLinks = createdReceiveLine1.BOMComponentLinks;
			AssertEquals("Links reconciled.", 2, bomLinks.Count());
			bomLink3 = bomLinks.Single(l => l.WIP_WE_ComponentLine == wheelOrderLine2.PK);
			bomLink4 = bomLinks.Single(l => l.WIP_WE_ComponentLine == frameOrderLine2.PK);
			AssertEquals("Links reconciled.", 10m, bomLink3.WIP_ComponentQuantity);
			AssertEquals("Links reconciled.", 5m, bomLink4.WIP_ComponentQuantity);
			AssertEquals("We are moving links directly.", true, bomLink1.PK == bomLink3.PK);
			AssertEquals("We are moving links directly.", true, bomLink2.PK == bomLink4.PK);

			kitOrderLine2.ReleaseLines[0].Quantity = 9m;
			kitOrderLine1.ReleaseLines[0].Quantity = 10m;
			AssertEquals("Precondition.", -9m, kitOrderLine1.ReleaseLines[0].UnreleasedQty);
			kitOrderLine2.ReleaseLines[0].Quantity = 0m;

			AssertEquals("Should have moved component lines", true, kitOrderLine1.ChildComponentLines.Count > 0);
			AssertEquals("Should have moved component lines", false, kitOrderLine2.ChildComponentLines.Count > 0);
			AssertEquals("Should have moved pick lines", 2, kitOrderLine1.PickLines.Count);
			AssertEquals("Should have moved pick lines", 2, kitOrderLine1.ChildComponentLines.Sum(ccl => ccl.PickLines.Count));
			AssertEquals("Should have moved pick lines", 0, kitOrderLine2.PickLinesForRelease.Count);
			AssertEquals("Should have moved pick lines", 0, kitOrderLine2.ChildComponentLines.Count);
			AssertEquals("Should have moved pick lines", 10m, kitOrderLine1.ChildComponentLines.SelectMany(ccl => ccl.PickLines).Single(pl => pl.Product.Parent == wheel).WZ_Units);
			AssertEquals("Should have moved pick lines", 5m, kitOrderLine1.ChildComponentLines.SelectMany(ccl => ccl.PickLines).Single(pl => pl.Product.Parent == frame).WZ_Units);
			AssertEquals("Should have moved pick lines", 10m, kitOrderLine1.PickLines.Sum(pl => pl.WZ_Units));

			AssertEquals("Both lines have 5m units.", true, kitOrderLine1.PickLines.All(l => l.WZ_Units == 5m));
			AssertEquals("Kit Pick Line reconciled.", 0, kitOrderLine2.PickLines.Count);

			createdReceiveLine1 = createdReceive.Lines[0];
			AssertEquals("Receive Line remains the same.", 5m, createdReceiveLine1.WE_TransactionQuantity);
			AssertEquals("Receive Line remains the same.", 5m, createdReceiveLine1.WE_ClientOrderedUnits);
			bomLinks = createdReceiveLine1.BOMComponentLinks;
			AssertEquals("Links reconciled.", 2, bomLinks.Count());
			bomLink1 = bomLinks.Single(l => l.WIP_WE_ComponentLine == wheelOrderLine1.PK);
			bomLink2 = bomLinks.Single(l => l.WIP_WE_ComponentLine == frameOrderLine1.PK);
			AssertEquals("Links reconciled.", 10m, bomLink1.WIP_ComponentQuantity);
			AssertEquals("Links reconciled.", 5m, bomLink2.WIP_ComponentQuantity);
			AssertEquals("We are moving links directly.", true, bomLink1.PK == bomLink3.PK);
			AssertEquals("We are moving links directly.", true, bomLink2.PK == bomLink4.PK);

			AssertNoExceptionThrown(Factory.Save);
		}

		#endregion

		#region TestReconcilePickLines_PickByBOM_SplitAndMergeUnpickedKitReceiveLine

		public void TestReconcilePickLines_PickByBOM_SplitAndMergeUnpickedKitReceiveLine()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var bike = Helper.CreateProduct(data.Org1, "BIKE");
			bike.OP_IsComponentPickedOnSalesOrder = true;

			var wheel = Helper.CreateProduct(data.Org1, "WHEEL");
			var frame = Helper.CreateProduct(data.Org1, "FRAME");
			Helper.CreateProductBOM(bike, wheel, 2m, Constants.PkgUnit.Unit);
			Helper.CreateProductBOM(bike, frame, 1m, Constants.PkgUnit.Unit);
			var location = data.Whs1.FindLocation("A-1");
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveInventoryLine(receive, wheel, 30m, location);
			Helper.CreateWhsReceiveInventoryLine(receive, frame, 15m, location);
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertEquals("Receive should be finalised.", true, receive.IsFinalised);
			Factory.Save();

			var kitOrder = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "KIT1", bike, 10m);
			var kitOrderLine1 = kitOrder.Lines[0];
			var kitOrderLine2 = Helper.CreateWhsOrderLine(kitOrder, bike, 10m);

			var pick = Helper.CreatePickNew(kitOrder);
			AssertEquals("Precondition - Picked By BOM", true, kitOrderLine1.IsBOMProductPickedOnSalesOrder);
			AssertEquals("Precondition - Picked By BOM", true, kitOrderLine2.IsBOMProductPickedOnSalesOrder);
			AssertEquals("Precondition - Picked By BOM", true, kitOrderLine1.ChildComponentLines.Count > 0);
			AssertEquals("Precondition - Picked By BOM", true, kitOrderLine2.ChildComponentLines.Count > 0);

			var wheelOrderLine1 = kitOrderLine1.ChildComponentLines.Single(l => l.WE_OP == wheel.PK);
			var frameOrderLine1 = kitOrderLine1.ChildComponentLines.Single(l => l.WE_OP == frame.PK);
			var wheelOrderLine2 = kitOrderLine2.ChildComponentLines.Single(l => l.WE_OP == wheel.PK);
			var frameOrderLine2 = kitOrderLine2.ChildComponentLines.Single(l => l.WE_OP == frame.PK);
			var createdReceive = Factory.LoadTop1<WhsReceive>(new ZQuery(WhsDocketSchema.WD_WP_ParentPickForReceive, pick.PK));
			var createdReceiveLine1 = createdReceive.Lines.Single(l => l.WE_TransactionQuantity == 10m);
			var createdReceiveLine2 = createdReceive.Lines.Single(l => l.WE_TransactionQuantity == 5m);
			var bomLinks1 = createdReceiveLine1.BOMComponentLinks;
			var bomLinks2 = createdReceiveLine2.BOMComponentLinks;
			AssertEquals(2, bomLinks1.Count());
			AssertEquals(2, bomLinks2.Count());
			var bomLink1 = bomLinks1.Single(l => l.WIP_ComponentQuantity == 20m);
			var bomLink2 = bomLinks1.Single(l => l.WIP_ComponentQuantity == 10m);
			var bomLink3 = bomLinks2.Single(l => l.WIP_ComponentQuantity == 10m);
			var bomLink4 = bomLinks2.Single(l => l.WIP_ComponentQuantity == 5m);

			AssertEquals("Precondition - Kit Pick Line created from Children.", 1, kitOrderLine1.PickLines.Count);
			AssertEquals("Precondition - Kit Pick Line created from Children.", 1, kitOrderLine2.PickLines.Count);
			var kitPickLine1 = kitOrderLine1.PickLines[0];
			var kitPickLine2 = kitOrderLine2.PickLines[0];
			AssertEquals("Precondition - Kit Pick Line created from Children.", 10m, kitPickLine1.WZ_Units);
			AssertEquals("Precondition - Kit Pick Line created from Children.", 5m, kitPickLine2.WZ_Units);
			AssertEquals("Precondition - Receive Line created.", 10m, createdReceiveLine1.WE_TransactionQuantity);
			AssertEquals("Precondition - Receive Line created.", 10m, createdReceiveLine1.WE_ClientOrderedUnits);
			AssertEquals("Precondition - Receive Line created.", 5m, createdReceiveLine2.WE_TransactionQuantity);
			AssertEquals("Precondition - Receive Line created.", 5m, createdReceiveLine2.WE_ClientOrderedUnits);
			AssertEquals("Precondition.", 1, kitOrderLine1.ReleaseLines.Count);
			AssertEquals("Precondition.", 1, kitOrderLine2.ReleaseLines.Count);
			AssertEquals("Precondition.", 10m, kitOrderLine1.ReleaseLines[0].Quantity);
			AssertEquals("Precondition.", 0m, kitOrderLine1.ReleaseLines[0].UnreleasedQty);
			AssertEquals("Precondition.", 5m, kitOrderLine2.ReleaseLines[0].Quantity);
			AssertEquals("Precondition.", 0m, kitOrderLine2.ReleaseLines[0].UnreleasedQty);

			kitOrderLine1.ReleaseLines[0].Quantity = 9m;
			kitOrderLine2.ReleaseLines[0].Quantity = 6m;

			AssertEquals("Quantity updated.", 9m, createdReceiveLine1.WE_TransactionQuantity);
			AssertEquals("Quantity updated.", 9m, createdReceiveLine1.WE_ClientOrderedUnits);
			AssertEquals("Quantity updated.", 6m, createdReceiveLine2.WE_TransactionQuantity);
			AssertEquals("Quantity updated.", 6m, createdReceiveLine2.WE_ClientOrderedUnits);
			kitPickLine1 = kitOrderLine1.PickLines.Single();
			AssertEquals("Quantity updated.", 9m, kitPickLine1.WZ_Units);
			AssertEquals("Quantity updated.", 6m, kitPickLine2.WZ_Units);
			AssertEquals("Links were deleted to recreate.", true, bomLink1.IsDeleted);
			AssertEquals("Links were deleted to recreate.", true, bomLink2.IsDeleted);
			AssertEquals("Links were deleted to recreate.", true, bomLink3.IsDeleted);
			AssertEquals("Links were deleted to recreate.", true, bomLink4.IsDeleted);
			bomLinks1 = createdReceiveLine1.BOMComponentLinks;
			bomLinks2 = createdReceiveLine2.BOMComponentLinks;
			AssertEquals("Links recreated.", 18m, bomLinks1.Single(l => l.WIP_WE_ComponentLine == wheelOrderLine1.PK).WIP_ComponentQuantity);
			AssertEquals("Links recreated.", 9m, bomLinks1.Single(l => l.WIP_WE_ComponentLine == frameOrderLine1.PK).WIP_ComponentQuantity);
			AssertEquals("Links recreated.", 12m, bomLinks2.Single(l => l.WIP_WE_ComponentLine == wheelOrderLine2.PK).WIP_ComponentQuantity);
			AssertEquals("Links recreated.", 6m, bomLinks2.Single(l => l.WIP_WE_ComponentLine == frameOrderLine2.PK).WIP_ComponentQuantity);

			AssertNoExceptionThrown(Factory.Save);
		}

		#endregion

		#region TestReconcilePickLines_PickByBOM_CreateAndDeleteKitReceiveLine

		public void TestReconcilePickLines_PickByBOM_CreateAndDeleteKitReceiveLine()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var bike = Helper.CreateProduct(data.Org1, "BIKE");
			bike.OP_IsComponentPickedOnSalesOrder = true;

			var wheel = Helper.CreateProduct(data.Org1, "WHEEL");
			var frame = Helper.CreateProduct(data.Org1, "FRAME");
			Helper.CreateProductBOM(bike, wheel, 2m, Constants.PkgUnit.Unit);
			Helper.CreateProductBOM(bike, frame, 1m, Constants.PkgUnit.Unit);
			var location = data.Whs1.FindLocation("A-1");
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveInventoryLine(receive, wheel, 20m, location);
			Helper.CreateWhsReceiveInventoryLine(receive, frame, 10m, location);
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertEquals("Receive should be finalised.", true, receive.IsFinalised);
			Factory.Save();

			var kitOrder = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "KIT1", bike, 10m);
			var kitOrderLine1 = kitOrder.Lines[0];
			var kitOrderLine2 = Helper.CreateWhsOrderLine(kitOrder, bike, 10m);

			var pick = Helper.CreatePickNew(kitOrder);
			AssertEquals("Precondition - Picked By BOM", true, kitOrderLine1.IsBOMProductPickedOnSalesOrder);
			AssertEquals("Precondition - Picked By BOM", true, kitOrderLine2.IsBOMProductPickedOnSalesOrder);
			AssertEquals("Precondition - Picked By BOM", true, kitOrderLine1.ChildComponentLines.Count > 0);
			AssertEquals("Precondition - No enough stock", false, kitOrderLine2.ChildComponentLines.Count > 0);

			var wheelOrderLine1 = kitOrderLine1.ChildComponentLines.Single(l => l.WE_OP == wheel.PK);
			var frameOrderLine1 = kitOrderLine1.ChildComponentLines.Single(l => l.WE_OP == frame.PK);
			var createdReceive = Factory.LoadTop1<WhsReceive>(new ZQuery(WhsDocketSchema.WD_WP_ParentPickForReceive, pick.PK));
			var createdReceiveLine1 = createdReceive.Lines.Single();
			var bomLinks1 = createdReceiveLine1.BOMComponentLinks;
			AssertEquals(2, bomLinks1.Count());
			var bomLink1 = bomLinks1.Single(l => l.WIP_ComponentQuantity == 20m);
			var bomLink2 = bomLinks1.Single(l => l.WIP_ComponentQuantity == 10m);

			AssertEquals("Precondition - Kit Pick Line created from Children.", 1, kitOrderLine1.PickLines.Count);
			AssertEquals("Precondition - No Children, no Pick Lines.", 0, kitOrderLine2.PickLines.Count);
			var kitPickLine1 = kitOrderLine1.PickLines[0];
			AssertEquals("Precondition - Kit Pick Line created from Children.", 10m, kitPickLine1.WZ_Units);
			AssertEquals("Precondition - Receive Line created.", 10m, createdReceiveLine1.WE_TransactionQuantity);
			AssertEquals("Precondition - Receive Line created.", 10m, createdReceiveLine1.WE_ClientOrderedUnits);
			AssertEquals("Precondition.", 1, kitOrderLine1.ReleaseLines.Count);
			AssertEquals("Precondition.", 0, kitOrderLine2.ReleaseLines.Count);
			AssertEquals("Precondition.", 10m, kitOrderLine1.ReleaseLines[0].Quantity);

			kitOrderLine1.ReleaseLines[0].Quantity = 9m;
			kitOrderLine2.ReleaseLines[0].Quantity = 1m;

			AssertEquals("Links were deleted to recreate.", true, bomLink1.IsDeleted);
			AssertEquals("Links were deleted to recreate.", true, bomLink2.IsDeleted);

			wheelOrderLine1 = kitOrderLine1.ChildComponentLines.Single(l => l.WE_OP == wheel.PK);
			frameOrderLine1 = kitOrderLine1.ChildComponentLines.Single(l => l.WE_OP == frame.PK);
			var wheelOrderLine2 = kitOrderLine2.ChildComponentLines.Single(l => l.WE_OP == wheel.PK);
			var frameOrderLine2 = kitOrderLine2.ChildComponentLines.Single(l => l.WE_OP == frame.PK);
			createdReceive = Factory.LoadTop1<WhsReceive>(new ZQuery(WhsDocketSchema.WD_WP_ParentPickForReceive, pick.PK));
			createdReceiveLine1 = createdReceive.Lines.Single(l => l.WE_TransactionQuantity == 9m);
			var createdReceiveLine2 = createdReceive.Lines.Single(l => l.WE_TransactionQuantity == 1m);
			bomLinks1 = createdReceiveLine1.BOMComponentLinks;
			var bomLinks2 = createdReceiveLine2.BOMComponentLinks;
			AssertEquals(2, bomLinks1.Count());
			AssertEquals(2, bomLinks2.Count());
			bomLink1 = bomLinks1.Single(l => l.WIP_ComponentQuantity == 18m);
			bomLink2 = bomLinks1.Single(l => l.WIP_ComponentQuantity == 9m);
			var bomLink3 = bomLinks2.Single(l => l.WIP_ComponentQuantity == 2m);
			var bomLink4 = bomLinks2.Single(l => l.WIP_ComponentQuantity == 1m);

			AssertEquals("Kit Pick Line created from Children.", 1, kitOrderLine1.PickLines.Count);
			AssertEquals("Kit Pick Line split from the other Order Line.", 1, kitOrderLine2.PickLines.Count);
			kitPickLine1 = kitOrderLine1.PickLines[0];
			var kitPickLine2 = kitOrderLine2.PickLines[0];
			AssertEquals("Kit Pick Line created from Children.", 9m, kitPickLine1.WZ_Units);
			AssertEquals("Kit Pick Line created from Children.", 1m, kitPickLine2.WZ_Units);
			AssertEquals("Quantity reduced.", 9m, createdReceiveLine1.WE_TransactionQuantity);
			AssertEquals("Quantity reduced.", 9m, createdReceiveLine1.WE_ClientOrderedUnits);
			AssertEquals("New Kit Receive Line created.", false, createdReceiveLine2.IsInDatabase);
			AssertEquals(1m, createdReceiveLine2.WE_TransactionQuantity);
			AssertEquals(1m, createdReceiveLine2.WE_ClientOrderedUnits);
			AssertEquals("Links recreated.", 18m, bomLinks1.Single(l => l.WIP_WE_ComponentLine == wheelOrderLine1.PK).WIP_ComponentQuantity);
			AssertEquals("Links recreated.", 9m, bomLinks1.Single(l => l.WIP_WE_ComponentLine == frameOrderLine1.PK).WIP_ComponentQuantity);
			AssertEquals("Links recreated.", 2m, bomLinks2.Single(l => l.WIP_WE_ComponentLine == wheelOrderLine2.PK).WIP_ComponentQuantity);
			AssertEquals("Links recreated.", 1m, bomLinks2.Single(l => l.WIP_WE_ComponentLine == frameOrderLine2.PK).WIP_ComponentQuantity);
			AssertNoExceptionThrown(Factory.Save);

			kitOrderLine1.ReleaseLines[0].Quantity = 0m;
			kitOrderLine2.ReleaseLines[0].Quantity = 10m;

			AssertEquals("Links were deleted to recreate.", true, bomLink1.IsDeleted);
			AssertEquals("Links were deleted to recreate.", true, bomLink2.IsDeleted);
			AssertEquals("Links were deleted to recreate.", true, bomLink3.IsDeleted);
			AssertEquals("Links were deleted to recreate.", true, bomLink4.IsDeleted);

			AssertEquals("Component Lines deleted through reconcile.", 0, kitOrderLine1.ChildComponentLines.Count);
			wheelOrderLine2 = kitOrderLine2.ChildComponentLines.Single(l => l.WE_OP == wheel.PK);
			frameOrderLine2 = kitOrderLine2.ChildComponentLines.Single(l => l.WE_OP == frame.PK);
			createdReceive = Factory.LoadTop1<WhsReceive>(new ZQuery(WhsDocketSchema.WD_WP_ParentPickForReceive, pick.PK));
			AssertEquals("1 Receive Line was deleted.", 1, createdReceive.Lines.Count);
			AssertEquals("1 Receive Line was deleted.", true, createdReceiveLine1.IsDeleted);
			AssertEquals(10m, createdReceive.Lines.Single().WE_TransactionQuantity);

			AssertEquals("Kit Pick Line was deleted.", 0, kitOrderLine1.PickLines.Count);
			AssertEquals("Kit Pick Line was deleted.", true, kitPickLine1.IsDeleted);
			AssertEquals("Kit Pick Line was merged.", 1, kitOrderLine2.PickLines.Count);
			kitPickLine2 = kitOrderLine2.PickLines[0];
			AssertEquals("Kit Pick Line was merged.", 10m, kitPickLine2.WZ_Units);

			bomLinks2 = createdReceiveLine2.BOMComponentLinks;
			AssertEquals(2, bomLinks2.Count());
			AssertEquals("Links recreated.", 20m, bomLinks2.Single(l => l.WIP_WE_ComponentLine == wheelOrderLine2.PK).WIP_ComponentQuantity);
			AssertEquals("Links recreated.", 10m, bomLinks2.Single(l => l.WIP_WE_ComponentLine == frameOrderLine2.PK).WIP_ComponentQuantity);

			AssertNoExceptionThrown(Factory.Save);
		}

		#endregion

		#region TestReconcilePickLines_PickByBOM_DirectMoveKitReceiveLine

		public void TestReconcilePickLines_PickByBOM_DirectMoveKitReceiveLine()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var bike = Helper.CreateProduct(data.Org1, "BIKE");
			bike.OP_IsComponentPickedOnSalesOrder = true;

			var wheel = Helper.CreateProduct(data.Org1, "WHEEL");
			var frame = Helper.CreateProduct(data.Org1, "FRAME");
			Helper.CreateProductBOM(bike, wheel, 2m, Constants.PkgUnit.Unit);
			Helper.CreateProductBOM(bike, frame, 1m, Constants.PkgUnit.Unit);
			var location = data.Whs1.FindLocation("A-1");
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveInventoryLine(receive, wheel, 20m, location);
			Helper.CreateWhsReceiveInventoryLine(receive, frame, 10m, location);
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertEquals("Receive should be finalised.", true, receive.IsFinalised);
			Factory.Save();

			var kitOrder = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "KIT1", bike, 10m);
			var kitOrderLine1 = kitOrder.Lines[0];
			var kitOrderLine2 = Helper.CreateWhsOrderLine(kitOrder, bike, 10m);

			var pick = Helper.CreatePickNew(kitOrder);
			AssertEquals("Precondition - Picked By BOM", true, kitOrderLine1.IsBOMProductPickedOnSalesOrder);
			AssertEquals("Precondition - Picked By BOM", true, kitOrderLine2.IsBOMProductPickedOnSalesOrder);
			AssertEquals("Precondition - Picked By BOM", true, kitOrderLine1.ChildComponentLines.Count > 0);
			AssertEquals("Precondition - No enough stock", false, kitOrderLine2.ChildComponentLines.Count > 0);

			var wheelOrderLine1 = kitOrderLine1.ChildComponentLines.Single(l => l.WE_OP == wheel.PK);
			var frameOrderLine1 = kitOrderLine1.ChildComponentLines.Single(l => l.WE_OP == frame.PK);
			var createdReceive = Factory.LoadTop1<WhsReceive>(new ZQuery(WhsDocketSchema.WD_WP_ParentPickForReceive, pick.PK));
			var createdReceiveLine1 = createdReceive.Lines.Single();
			var createdReceiveLine1PK = createdReceiveLine1.PK;
			var bomLinks1 = createdReceiveLine1.BOMComponentLinks;
			AssertEquals(2, bomLinks1.Count());
			var bomLink1 = bomLinks1.Single(l => l.WIP_ComponentQuantity == 20m);
			var bomLink2 = bomLinks1.Single(l => l.WIP_ComponentQuantity == 10m);

			AssertEquals("Precondition - Kit Pick Line created from Children.", 1, kitOrderLine1.PickLines.Count);
			AssertEquals("Precondition - No Children, no Pick Lines.", 0, kitOrderLine2.PickLines.Count);
			var kitPickLine1 = kitOrderLine1.PickLines[0];
			var kitPickLine1PK = kitPickLine1.PK;
			AssertEquals("Precondition - Kit Pick Line created from Children.", 10m, kitPickLine1.WZ_Units);
			AssertEquals("Precondition - Receive Line created.", 10m, createdReceiveLine1.WE_TransactionQuantity);
			AssertEquals("Precondition - Receive Line created.", 10m, createdReceiveLine1.WE_ClientOrderedUnits);
			AssertEquals("Precondition.", 1, kitOrderLine1.ReleaseLines.Count);
			AssertEquals("Precondition.", 0, kitOrderLine2.ReleaseLines.Count);
			AssertEquals("Precondition.", 10m, kitOrderLine1.ReleaseLines[0].Quantity);

			kitOrderLine1.ReleaseLines[0].Quantity = 0m;
			kitOrderLine2.ReleaseLines[0].Quantity = 10m;

			AssertEquals("The Component Lines were directly moved to kitOrderLine2.", 0, kitOrderLine1.ChildComponentLines.Count);
			AssertEquals("The Component Lines were directly moved to kitOrderLine2.", kitOrderLine2.PK, wheelOrderLine1.WE_WE_ParentDocketLine);
			AssertEquals("The Component Lines were directly moved to kitOrderLine2.", kitOrderLine2.PK, frameOrderLine1.WE_WE_ParentDocketLine);
			AssertEquals("The Component Lines were directly moved to kitOrderLine2, so as the links.", false, bomLink1.IsDeleted);
			AssertEquals("The Component Lines were directly moved to kitOrderLine2, so as the links.", false, bomLink2.IsDeleted);
			AssertEquals("The Component Lines were directly moved to kitOrderLine2, so as the links.", wheelOrderLine1.PK, bomLink1.WIP_WE_ComponentLine);
			AssertEquals("The Component Lines were directly moved to kitOrderLine2, so as the links.", frameOrderLine1.PK, bomLink2.WIP_WE_ComponentLine);
			AssertEquals("The Component Lines were directly moved to kitOrderLine2, so as the links.", 20m, bomLink1.WIP_ComponentQuantity);
			AssertEquals("The Component Lines were directly moved to kitOrderLine2, so as the links.", 10m, bomLink2.WIP_ComponentQuantity);

			AssertEquals("The Kit Receive Line was directly moved to kitOrderLine2.", 1, createdReceive.Lines.Count);
			AssertEquals("The Kit Receive Line was directly moved to kitOrderLine2.", true, createdReceive.Lines.Single().PK == createdReceiveLine1PK);
			AssertEquals("The Kit Receive Line was directly moved to kitOrderLine2.", true, createdReceive.Lines.Single().WE_TransactionQuantity == 10m);
			AssertEquals("The Kit Receive Line was directly moved to kitOrderLine2.", true, createdReceive.Lines.Single().WE_ClientOrderedUnits == 10m);

			AssertEquals("The Kit Pick Line was directly moved to kitOrderLine2.", 0, kitOrderLine1.PickLines.Count);
			AssertEquals("The Kit Pick Line was directly moved to kitOrderLine2.", 1, kitOrderLine2.PickLines.Count);
			AssertEquals("The Kit Pick Line was directly moved to kitOrderLine2.", true, kitOrderLine2.PickLines.Single().PK == kitPickLine1PK);
			AssertEquals("The Kit Pick Line was directly moved to kitOrderLine2.", true, kitOrderLine2.PickLines.Single().WZ_Units == 10m);

			AssertNoExceptionThrown(Factory.Save);
		}

		#endregion

		#region TestReconcilePickLines_PickByBOM_WillNotThrowExceptionAfterDeletingKitReceiveLine

		public void TestReconcilePickLines_PickByBOM_WillNotThrowExceptionAfterDeletingKitReceiveLine()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var bike = Helper.CreateProduct(data.Org1, "BIKE");
			bike.OP_IsComponentPickedOnSalesOrder = true;

			var wheel = Helper.CreateProduct(data.Org1, "WHEEL");
			var frame = Helper.CreateProduct(data.Org1, "FRAME");
			Helper.CreateProductBOM(bike, wheel, 2m, Constants.PkgUnit.Unit);
			Helper.CreateProductBOM(bike, frame, 1m, Constants.PkgUnit.Unit);
			var location = data.Whs1.FindLocation("A-1");
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveInventoryLine(receive, wheel, 4m, location);
			Helper.CreateWhsReceiveInventoryLine(receive, frame, 2m, location);
			Helper.CreateWhsReceiveInventoryLine(receive, bike, 1m, location);
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertEquals("Receive should be finalised.", true, receive.IsFinalised);
			Factory.Save();

			var kitOrder = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "KIT1", bike, 2m);
			var kitOrderLine1 = kitOrder.Lines[0];
			var kitOrderLine2 = Helper.CreateWhsOrderLine(kitOrder, bike, 2m);

			var pick = Helper.CreatePickNew(kitOrder);
			Factory.Save();
			AssertEquals("Precondition.", 1, kitOrderLine1.ReleaseLines.Count);
			AssertEquals("Precondition.", 1, kitOrderLine2.ReleaseLines.Count);
			AssertEquals("Precondition.", 2m, kitOrderLine1.ReleaseLines[0].Quantity);
			AssertEquals("Precondition.", 1m, kitOrderLine2.ReleaseLines[0].Quantity);

			var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			pick = newFactory.Load<WhsPick>(pick.PK);
			kitOrderLine1 = newFactory.Load<WhsOrderLine>(kitOrderLine1.PK);
			kitOrderLine2 = newFactory.Load<WhsOrderLine>(kitOrderLine2.PK);

			kitOrderLine1.ReleaseLines[0].Quantity = 1m;
			kitOrderLine2.ReleaseLines[0].Quantity = 2m;

			pick.OrderedInventories.Cast<WhsPickOrderedInventory>().ForEach(oi => oi.ClearAvailableInventoriesCache());
			AssertNoExceptionThrown(pick.RunPreSaveValidation);
			AssertNoExceptionThrown(newFactory.Save);

			kitOrderLine1.ReleaseLines[0].Quantity = 2m;
			kitOrderLine2.ReleaseLines[0].Quantity = 1m;

			pick.OrderedInventories.Cast<WhsPickOrderedInventory>().ForEach(oi => oi.ClearAvailableInventoriesCache());
			AssertNoExceptionThrown(pick.RunPreSaveValidation);
			AssertNoExceptionThrown(newFactory.Save);
		}

		#endregion

		#region TestReconcilePickLines_PickByBOM_KitsPickedThroughPutawayComponents

		public void TestReconcilePickLines_PickByBOM_KitsPickedThroughPutawayComponents()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var dockDoorLocationType = Helper.CreateLocationType("XYZ", "Test", false, 0, LocationClasses.Codes.DDL);
			Helper.Factory.Save();
			var bike = Helper.CreateProduct(data.Org1, "BIKE");
			bike.OP_IsComponentPickedOnSalesOrder = true;

			var wheel = Helper.CreateProduct(data.Org1, "WHEEL");
			var frame = Helper.CreateProduct(data.Org1, "FRAME");
			Helper.CreateProductBOM(bike, wheel, 2m, "UNT");
			Helper.CreateProductBOM(bike, frame, 1m, "UNT");
			var location = data.Whs1.FindLocation("A-1");
			var dockdoorLocation = data.Whs1.FindLocation("A-2");
			dockdoorLocation.WLV_WLT_LocationType = dockDoorLocationType.PK;
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveInventoryLine(receive, wheel, 20m, location);
			Helper.CreateWhsReceiveInventoryLine(receive, frame, 10m, location);
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertEquals("Receive should be finalised.", true, receive.IsFinalised);
			Helper.Factory.Save();

			var kitOrder = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "KIT1", bike, 10m);
			var kitOrderLine1 = kitOrder.Lines[0];
			var kitOrderLine2 = Helper.CreateWhsOrderLine(kitOrder, bike, 10m);

			var pick = Helper.CreatePickNew(kitOrder);
			AssertEquals("Precondition - Picked By BOM", true, kitOrderLine1.IsBOMProductPickedOnSalesOrder);
			AssertEquals("Precondition - Picked By BOM", true, kitOrderLine2.IsBOMProductPickedOnSalesOrder);
			AssertEquals("Precondition - Picked By BOM", true, kitOrderLine1.ChildComponentLines.Count > 0);
			AssertEquals("Precondition - No enough stock", false, kitOrderLine2.ChildComponentLines.Count > 0);

			var wheelOrderLine1 = kitOrderLine1.ChildComponentLines.Single(l => l.WE_OP == wheel.PK);
			var frameOrderLine1 = kitOrderLine1.ChildComponentLines.Single(l => l.WE_OP == frame.PK);
			var wheelPickLine = wheelOrderLine1.PickLines[0];
			var framePickLine = frameOrderLine1.PickLines[0];
			var createdReceive = Helper.Factory.LoadTop1<WhsReceive>(new ZQuery(WhsDocketSchema.WD_WP_ParentPickForReceive, pick.PK));
			var createdReceiveLine = createdReceive.Lines.Single();

			wheelPickLine.WZ_PickedDateTime = ZDateTimeOffset.Now;
			framePickLine.WZ_PickedDateTime = ZDateTimeOffset.Now;
			Helper.Factory.Save();

			// simulate RF Putaway to Dock Door
			var currentUser = GlbStaff.CurrentUser.GS_Code;
			var inventoryLineQuery = new ZQuery(WhsDocketLineSchema.PK, pick.GetAllPickLines().Where(pl => !pl.WZ_WE_OriginalPickedInventoryLine.IsEmpty).Select(pl => pl.WZ_WE_InventoryLine));
			var inTransitLines = Factory.Load<WhsTransferLine>(inventoryLineQuery).Where(tl => tl.WE_GS_NKPutawayBy.EqualsIgnoringCase(currentUser) && tl.WE_CurrentInventoryStatus == InventoryStatus.Codes.InTransit);
			var (transferLinesToIgnoreWhenSettingLocation, _) = WhsPickByBOMHelper.CreatePickByBOMTransferLinesIfNecessary(Factory, dockdoorLocation.PK, inTransitLines.ToArray(), currentUser, isClosingPackagesWhenPutToDockDoor: true);

			WhsPackingConsolidationService.PutawayTransferLines(dockdoorLocation, inTransitLines, transferLinesToIgnoreWhenSettingLocation);
			Factory.Save();

			kitOrderLine1.ReleaseLines[0].Quantity = 1m;
			kitOrderLine2.ReleaseLines[0].Quantity = 9m;

			AssertEquals("Component Lines reconciled.", 2, kitOrderLine1.ChildComponentLines.Count);
			AssertEquals("Component Lines reconciled.", 2, kitOrderLine2.ChildComponentLines.Count);
			var wheelOrderLine2 = kitOrderLine2.ChildComponentLines.Single(l => l.WE_OP == wheel.PK);
			var frameOrderLine2 = kitOrderLine2.ChildComponentLines.Single(l => l.WE_OP == frame.PK);

			AssertEquals("Kit Pick Line was split.", 1, kitOrderLine1.PickLines.Count);
			AssertEquals("Kit Pick Line was split.", 1, kitOrderLine2.PickLines.Count);
			AssertEquals("Kit Pick Line was split.", 1m, kitOrderLine1.PickLines.Single().WZ_Units);
			AssertEquals("Kit Pick Line was split.", 9m, kitOrderLine2.PickLines.Single().WZ_Units);
			Factory.Save();

			var outboundTransferLineWheel = wheelOrderLine1.PickLines.Single().InventoryLine;
			var outboundTransferLineFrame = frameOrderLine1.PickLines.Single().InventoryLine;
			var outboundPickLineWheel = outboundTransferLineWheel.PickLines.Single();
			var outboundPickLineFrame = outboundTransferLineFrame.PickLines.Single();
			var originalDataWheel = GetPickLineData(outboundPickLineWheel);
			var originalDataFrame = GetPickLineData(outboundPickLineFrame);
			AssertEquals("Precondition: Original Order Line is set.", wheelOrderLine1.PK, outboundPickLineWheel.WZ_WE_OriginalOrderLine);
			AssertEquals("Precondition: Original Order Line is set.", frameOrderLine1.PK, outboundPickLineFrame.WZ_WE_OriginalOrderLine);

			kitOrderLine1.ReleaseLines[0].Quantity = 0m;
			kitOrderLine2.ReleaseLines[0].Quantity = 10m;

			AssertEquals("The Kit Receive Line was not changed because it was picked.", 1, createdReceive.Lines.Count);
			AssertEquals(10m, createdReceiveLine.WE_TransactionQuantity);
			AssertEquals(10m, createdReceiveLine.WE_ClientOrderedUnits);

			AssertEquals("Component Lines were deleted.", 0, kitOrderLine1.ChildComponentLines.Count);
			AssertEquals("Component Lines exists.", 2, kitOrderLine2.ChildComponentLines.Count);
			AssertEquals("Kit Pick Line was reconciled.", 0, kitOrderLine1.PickLines.Count);
			AssertEquals("Kit Pick Line was reconciled.", 1, kitOrderLine2.PickLines.Count);
			AssertEquals("Kit Pick Line was reconciled.", 10m, kitOrderLine2.PickLines.Single().WZ_Units);

			AssertEquals("Should delete Original Outbound PickLine so it can be recreated.", true, outboundPickLineWheel.IsDeleted);
			AssertEquals("Should delete Original Outbound PickLine so it can be recreated.", true, outboundPickLineFrame.IsDeleted);

			var newOutboundPickLineWheel = outboundTransferLineWheel.PickLines.Single();
			var newOutboundPickLineFrame = outboundTransferLineFrame.PickLines.Single();
			AssertEquals("Outbound PickLine should be recreated.", false, newOutboundPickLineWheel.IsInDatabase);
			AssertEquals("Outbound PickLine should be recreated.", false, newOutboundPickLineFrame.IsInDatabase);
			AssertEquals("Outbound PickLine should point to other Child Component Line.", wheelOrderLine2.PK, newOutboundPickLineWheel.WZ_WE_OriginalOrderLine);
			AssertEquals("Outbound PickLine should point to other Child Component Line.", frameOrderLine2.PK, newOutboundPickLineFrame.WZ_WE_OriginalOrderLine);
			AssertEquals("New PickLine should have previous data cloned.", originalDataWheel, GetPickLineData(newOutboundPickLineWheel));
			AssertEquals("New PickLine should have previous data cloned.", originalDataFrame, GetPickLineData(newOutboundPickLineFrame));

			AssertNoExceptionThrown(Factory.Save);

			string GetPickLineData(WhsPickLine pickLine)
			{
				return string.Join("|", [pickLine.WZ_WE_InventoryLine.ToString(), pickLine.WZ_Units, pickLine.WZ_GS_NKAssignedTo, ZDateTimeOffset.TruncateMilliseconds(pickLine.WZ_PickedDateTime).ToISO8601String()]);
			}
		}

		#endregion

		#region TestReconcilePickLines_PartAttributes

		#region TestReconcilePickLines_PartAttributes_ExpiryDate

		public void TestReconcilePickLines_PartAttributes_ExpiryDate()
		{
			TestReconcilePickLines_PartAttributes(AttributeNumber.ExpiryDate, WhsReleaseLine.Schema.ExpiryDate);
		}

		#endregion

		#region TestReconcilePickLines_PartAttributes_PackingDate

		public void TestReconcilePickLines_PartAttributes_PackingDate()
		{
			TestReconcilePickLines_PartAttributes(AttributeNumber.PackingDate, WhsReleaseLine.Schema.PackingDate);
		}

		#endregion

		#region TestReconcilePickLines_PartAttributes_PartAttribute1

		public void TestReconcilePickLines_PartAttributes_PartAttribute1()
		{
			TestReconcilePickLines_PartAttributes(AttributeNumber.One, WhsReleaseLine.Schema.PartAttribute1);
		}

		#endregion

		#region TestReconcilePickLines_PartAttributes_PartAttribute2

		public void TestReconcilePickLines_PartAttributes_PartAttribute2()
		{
			TestReconcilePickLines_PartAttributes(AttributeNumber.Two, WhsReleaseLine.Schema.PartAttribute2);
		}

		#endregion

		#region TestReconcilePickLines_PartAttributes_PartAttribute3

		public void TestReconcilePickLines_PartAttributes_PartAttribute3()
		{
			TestReconcilePickLines_PartAttributes(AttributeNumber.Three, WhsReleaseLine.Schema.PartAttribute3);
		}

		#endregion

		#region TestReconcilePickLines_PartAttributes_SerialNumber

		public void TestReconcilePickLines_PartAttributes_SerialNumber()
		{
			var a = new ZString("A");
			var b = new ZString("B");
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAllAttributeType(data.Org1, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m);
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m);
			inventory1.WI_SerialNumber = a;
			inventory2.WI_SerialNumber = b;

			receive.AllocateLocationsWithMock();
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 1m);
			var orderLine1 = order.Lines[0];
			var orderLine2 = Helper.CreateWhsOrderLine(order, data.Part1, 1m);

			var pick = Helper.CreatePickNew(order);
			AssertEquals("Precondition", 2, pick.GetAllPickLines().Count());
			AssertEquals("Precondition", 1, orderLine1.ReleaseLines.Count);
			AssertEquals("Precondition", 1, orderLine2.ReleaseLines.Count);
			AssertEquals("Precondition", true, orderLine1.PickLines[0].WZ_WE_InventoryLine == inventory1.WI_WE_InDocketLine);
			AssertEquals("Precondition", true, orderLine2.PickLines[0].WZ_WE_InventoryLine == inventory2.WI_WE_InDocketLine);

			orderLine1.ReleaseLines[0].Quantity = 0m;
			orderLine2.ReleaseLines[0].Quantity = 0m;

			orderLine1.ReleaseLines[1].Quantity = 1m;
			orderLine2.ReleaseLines[0].Quantity = 1m;
			AssertEquals("Precondition", 1, orderLine1.ReleaseLines.Count);
			AssertEquals("Precondition", 1, orderLine2.ReleaseLines.Count);
			AssertEquals("Should have moved pick lines.", true, orderLine1.PickLines[0].WZ_WE_InventoryLine == inventory2.WI_WE_InDocketLine);
			AssertEquals("Should have moved pick lines.", true, orderLine2.PickLines[0].WZ_WE_InventoryLine == inventory1.WI_WE_InDocketLine);
			AssertNoExceptionThrown(Factory.Save);
		}

		#endregion

		#region TestReconcilePickLines_PartAttributes

		void TestReconcilePickLines_PartAttributes(AttributeNumber attributeType, string attributeColumn)
		{
			var today = ZDateTime.Today;
			var isDate = attributeType == AttributeNumber.ExpiryDate || attributeType == AttributeNumber.PackingDate;
			var a = isDate ? today.AddDays(5) : (IZType)new ZString("A");
			var b = isDate ? today.AddDays(10) : (IZType)new ZString("B");

			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAllAttributeType(data.Org1, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, attributeType, true);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m);
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m);

			var columnFixedForPartAttribVsPartAttribute = attributeColumn.Replace("ute", "");
			inventory1.InDocketLine["WE_" + columnFixedForPartAttribVsPartAttribute] = a;
			inventory2.InDocketLine["WE_" + columnFixedForPartAttribVsPartAttribute] = b;
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 10m);
			var orderLine1 = order.Lines[0];
			var orderLine2 = Helper.CreateWhsOrderLine(order, data.Part1, 10m);

			var pick = Helper.CreatePickNew(order);
			AssertEquals("Precondition", 2, pick.GetAllPickLines().Count());
			AssertEquals("Precondition", 1, orderLine1.ReleaseLines.Count);
			AssertEquals("Precondition", 1, orderLine2.ReleaseLines.Count);
			AssertEquals("Precondition", true, orderLine1.PickLines[0].WZ_WE_InventoryLine == inventory1.WI_WE_InDocketLine);
			AssertEquals("Precondition", true, orderLine2.PickLines[0].WZ_WE_InventoryLine == inventory2.WI_WE_InDocketLine);

			orderLine1.ReleaseLines[0].Quantity = 0m;
			orderLine2.ReleaseLines[0].Quantity = 0m;

			orderLine1.ReleaseLines[1].Quantity = 10m;
			orderLine2.ReleaseLines[0].Quantity = 10m;

			AssertEquals("Precondition", 1, orderLine1.ReleaseLines.Count);
			AssertEquals("Precondition", 1, orderLine2.ReleaseLines.Count);
			AssertEquals("Should have moved pick lines.", true, orderLine1.PickLines[0].WZ_WE_InventoryLine == inventory2.WI_WE_InDocketLine);
			AssertEquals("Should have moved pick lines.", true, orderLine2.PickLines[0].WZ_WE_InventoryLine == inventory1.WI_WE_InDocketLine);
			AssertNoExceptionThrown(Factory.Save);

			orderLine1.ReleaseLines[0].Quantity = 9m;
			orderLine2.ReleaseLines[0].Quantity = 9m;
			orderLine1.ReleaseLines[1].Quantity = 10m;
			orderLine2.ReleaseLines[1].Quantity = 10m;
			orderLine1.ReleaseLines[0].Quantity = 0m;
			orderLine2.ReleaseLines[0].Quantity = 0m;

			AssertEquals("Precondition", 1, orderLine1.ReleaseLines.Count);
			AssertEquals("Precondition", 1, orderLine2.ReleaseLines.Count);
			AssertEquals("Should have moved ", true, orderLine1.PickLines[0].WZ_WE_InventoryLine == inventory1.WI_WE_InDocketLine);
			AssertEquals("Should have moved ", true, orderLine2.PickLines[0].WZ_WE_InventoryLine == inventory2.WI_WE_InDocketLine);
			AssertNoExceptionThrown(Factory.Save);
		}

		#endregion

		#endregion

		#region TestReconcilePickLines_PartAttributes_Ordered

		#region TestReconcilePickLines_PartAttributes_Ordered_ExpiryDate

		public void TestReconcilePickLines_PartAttributes_Ordered_ExpiryDate()
		{
			TestReconcilePickLines_PartAttributes_Ordered(AttributeNumber.ExpiryDate, WhsReleaseLine.Schema.ExpiryDate);
		}

		#endregion

		#region TestReconcilePickLines_PartAttributes_Ordered_PackingDate

		public void TestReconcilePickLines_PartAttributes_Ordered_PackingDate()
		{
			TestReconcilePickLines_PartAttributes_Ordered(AttributeNumber.PackingDate, WhsReleaseLine.Schema.PackingDate);
		}

		#endregion

		#region TestReconcilePickLines_PartAttributes_Ordered_PartAttribute1

		public void TestReconcilePickLines_PartAttributes_Ordered_PartAttribute1()
		{
			TestReconcilePickLines_PartAttributes_Ordered(AttributeNumber.One, WhsReleaseLine.Schema.PartAttribute1);
		}

		#endregion

		#region TestReconcilePickLines_PartAttributes_Ordered_PartAttribute2

		public void TestReconcilePickLines_PartAttributes_Ordered_PartAttribute2()
		{
			TestReconcilePickLines_PartAttributes_Ordered(AttributeNumber.Two, WhsReleaseLine.Schema.PartAttribute2);
		}

		#endregion

		#region TestReconcilePickLines_PartAttributes_Ordered_PartAttribute3

		public void TestReconcilePickLines_PartAttributes_Ordered_PartAttribute3()
		{
			TestReconcilePickLines_PartAttributes_Ordered(AttributeNumber.Three, WhsReleaseLine.Schema.PartAttribute3);
		}

		#endregion

		#region TestReconcilePickLines_PartAttributes_Ordered_SerialNumber

		public void TestReconcilePickLines_PartAttributes_Ordered_SerialNumber()
		{
			var a = new ZString("A");
			var b = new ZString("B");

			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAllAttributeType(data.Org1, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m);
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m);
			inventory1.InDocketLine.WE_SerialNumber = a;
			inventory2.InDocketLine.WE_SerialNumber = b;
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 1m);
			var orderLine1 = order.Lines[0];
			var orderLine2 = Helper.CreateWhsOrderLine(order, data.Part1, 1m);
			var orderLine3 = Helper.CreateWhsOrderLine(order, data.Part1, 1m);
			var orderLine4 = Helper.CreateWhsOrderLine(order, data.Part1, 1m);

			orderLine1.WE_SerialNumber = a;
			orderLine2.WE_SerialNumber = a;
			orderLine3.WE_SerialNumber = b;
			orderLine4.WE_SerialNumber = b;

			var pick = Helper.CreatePickNew(order);
			AssertEquals("Precondition", 2, pick.GetAllPickLines().Count());
			AssertEquals("Precondition", 1, orderLine1.ReleaseLines.Count);
			AssertEquals("Precondition", 0, orderLine2.ReleaseLines.Count);
			AssertEquals("Precondition", 1, orderLine3.ReleaseLines.Count);
			AssertEquals("Precondition", 0, orderLine4.ReleaseLines.Count);
			AssertEquals("Precondition", true, orderLine1.PickLines[0].WZ_WE_InventoryLine == inventory1.WI_WE_InDocketLine);
			AssertEquals("Precondition", true, orderLine3.PickLines[0].WZ_WE_InventoryLine == inventory2.WI_WE_InDocketLine);

			orderLine1.ReleaseLines[0].Quantity = 0m;
			orderLine3.ReleaseLines[0].Quantity = 0m;

			orderLine2.ReleaseLines[0].Quantity = 1m;
			orderLine4.ReleaseLines[0].Quantity = 1m;

			AssertEquals("Precondition", 0, orderLine1.ReleaseLines.Count);
			AssertEquals("Precondition", 1, orderLine2.ReleaseLines.Count);
			AssertEquals("Precondition", 0, orderLine3.ReleaseLines.Count);
			AssertEquals("Precondition", 1, orderLine4.ReleaseLines.Count);
			AssertEquals("Should have moved pick lines", 1, orderLine2.PickLines.Count);
			AssertEquals("Should have moved pick lines", 1, orderLine4.PickLines.Count);
			AssertEquals("Should have moved pick lines", true, orderLine2.PickLines[0].WZ_WE_InventoryLine == inventory1.WI_WE_InDocketLine);
			AssertEquals("Should have moved pick lines", true, orderLine4.PickLines[0].WZ_WE_InventoryLine == inventory2.WI_WE_InDocketLine);

			orderLine2.ReleaseLines[0].Quantity = 0m;
			orderLine4.ReleaseLines[0].Quantity = 0m;
			orderLine1.ReleaseLines[0].Quantity = 1m;
			orderLine3.ReleaseLines[0].Quantity = 1m;

			AssertEquals("Precondition", 1, orderLine1.ReleaseLines.Count);
			AssertEquals("Precondition", 0, orderLine2.ReleaseLines.Count);
			AssertEquals("Precondition", 1, orderLine3.ReleaseLines.Count);
			AssertEquals("Precondition", 0, orderLine4.ReleaseLines.Count);
			AssertEquals("Should have moved pick lines", 1, orderLine1.PickLines.Count);
			AssertEquals("Should have moved pick lines", 1, orderLine3.PickLines.Count);
			AssertEquals("Should have moved pick lines", true, orderLine1.PickLines[0].WZ_WE_InventoryLine == inventory1.WI_WE_InDocketLine);
			AssertEquals("Should have moved pick lines", true, orderLine3.PickLines[0].WZ_WE_InventoryLine == inventory2.WI_WE_InDocketLine);

			AssertNoExceptionThrown(Factory.Save);
		}

		#endregion

		#region TestReconcilePickLines_PartAttributes_Ordered

		void TestReconcilePickLines_PartAttributes_Ordered(AttributeNumber attributeType, string attributeColumn)
		{
			var today = ZDateTime.Today;
			var isDate = attributeType == AttributeNumber.ExpiryDate || attributeType == AttributeNumber.PackingDate;
			var a = isDate ? today.AddDays(5) : (IZType)new ZString("A");
			var b = isDate ? today.AddDays(10) : (IZType)new ZString("B");

			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAllAttributeType(data.Org1, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, attributeType, true);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m);
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m);

			var columnFixedForPartAttribVsPartAttribute = attributeColumn.Replace("ute", "");
			inventory1.InDocketLine["WE_" + columnFixedForPartAttribVsPartAttribute] = a;
			inventory2.InDocketLine["WE_" + columnFixedForPartAttribVsPartAttribute] = b;
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 10m);
			var orderLine1 = order.Lines[0];
			var orderLine2 = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var orderLine3 = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var orderLine4 = Helper.CreateWhsOrderLine(order, data.Part1, 10m);

			orderLine1["WE_" + columnFixedForPartAttribVsPartAttribute] = a;
			orderLine2["WE_" + columnFixedForPartAttribVsPartAttribute] = a;
			orderLine3["WE_" + columnFixedForPartAttribVsPartAttribute] = b;
			orderLine4["WE_" + columnFixedForPartAttribVsPartAttribute] = b;

			var pick = Helper.CreatePickNew(order);
			AssertEquals("Precondition", 2, pick.GetAllPickLines().Count());
			AssertEquals("Precondition", 1, orderLine1.ReleaseLines.Count);
			AssertEquals("Precondition", 0, orderLine2.ReleaseLines.Count);
			AssertEquals("Precondition", 1, orderLine3.ReleaseLines.Count);
			AssertEquals("Precondition", 0, orderLine4.ReleaseLines.Count);
			AssertEquals("Precondition", true, orderLine1.PickLines[0].WZ_WE_InventoryLine == inventory1.WI_WE_InDocketLine);
			AssertEquals("Precondition", true, orderLine3.PickLines[0].WZ_WE_InventoryLine == inventory2.WI_WE_InDocketLine);

			orderLine1.ReleaseLines[0].Quantity = 0m;
			orderLine3.ReleaseLines[0].Quantity = 0m;

			orderLine2.ReleaseLines[0].Quantity = 10m;
			orderLine4.ReleaseLines[0].Quantity = 10m;

			AssertEquals("Precondition", 0, orderLine1.ReleaseLines.Count);
			AssertEquals("Precondition", 1, orderLine2.ReleaseLines.Count);
			AssertEquals("Precondition", 0, orderLine3.ReleaseLines.Count);
			AssertEquals("Precondition", 1, orderLine4.ReleaseLines.Count);
			AssertEquals("Should have moved pick lines", 1, orderLine2.PickLines.Count);
			AssertEquals("Should have moved pick lines", 1, orderLine4.PickLines.Count);
			AssertEquals("Should have moved pick lines", true, orderLine2.PickLines[0].WZ_WE_InventoryLine == inventory1.WI_WE_InDocketLine);
			AssertEquals("Should have moved pick lines", true, orderLine4.PickLines[0].WZ_WE_InventoryLine == inventory2.WI_WE_InDocketLine);

			orderLine2.ReleaseLines[0].Quantity = 9m;
			orderLine4.ReleaseLines[0].Quantity = 9m;
			orderLine1.ReleaseLines[0].Quantity = 10m;
			orderLine3.ReleaseLines[0].Quantity = 10m;
			orderLine2.ReleaseLines[0].Quantity = 0m;
			orderLine4.ReleaseLines[0].Quantity = 0m;

			AssertEquals("Precondition", 1, orderLine1.ReleaseLines.Count);
			AssertEquals("Precondition", 0, orderLine2.ReleaseLines.Count);
			AssertEquals("Precondition", 1, orderLine3.ReleaseLines.Count);
			AssertEquals("Precondition", 0, orderLine4.ReleaseLines.Count);
			AssertEquals("Should have moved pick lines", 1, orderLine1.PickLines.Count);
			AssertEquals("Should have moved pick lines", 1, orderLine3.PickLines.Count);
			AssertEquals("Should have moved pick lines", true, orderLine1.PickLines[0].WZ_WE_InventoryLine == inventory1.WI_WE_InDocketLine);
			AssertEquals("Should have moved pick lines", true, orderLine3.PickLines[0].WZ_WE_InventoryLine == inventory2.WI_WE_InDocketLine);

			AssertNoExceptionThrown(Factory.Save);
		}

		#endregion

		#endregion

		#region TestReconcilePickLines_PartAttributes_ReleaseCaptured

		#region TestReconcilePickLines_PartAttributes_ReleaseCaptured_PartAttribute1

		public void TestReconcilePickLines_PartAttributes_ReleaseCaptured_PartAttribute1()
		{
			TestReconcilePickLines_PartAttributes_ReleaseCaptured(AttributeNumber.One, WhsReleaseLine.Schema.PartAttribute1);
		}

		#endregion

		#region TestReconcilePickLines_PartAttributes_ReleaseCaptured_PartAttribute2

		public void TestReconcilePickLines_PartAttributes_ReleaseCaptured_PartAttribute2()
		{
			TestReconcilePickLines_PartAttributes_ReleaseCaptured(AttributeNumber.Two, WhsReleaseLine.Schema.PartAttribute2);
		}

		#endregion

		#region TestReconcilePickLines_PartAttributes_ReleaseCaptured_PartAttribute3

		public void TestReconcilePickLines_PartAttributes_ReleaseCaptured_PartAttribute3()
		{
			TestReconcilePickLines_PartAttributes_ReleaseCaptured(AttributeNumber.Three, WhsReleaseLine.Schema.PartAttribute3);
		}

		#endregion

		#region TestReconcilePickLines_PartAttributes_ReleaseCaptured_SerialNumber

		public void TestReconcilePickLines_PartAttributes_ReleaseCaptured_SerialNumber()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAllAttributeType(data.Org1, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true, setReleaseCaptured: true);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m);
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 10m);
			var orderLine1 = order.Lines[0];
			var orderLine2 = Helper.CreateWhsOrderLine(order, data.Part1, 10m);

			var pick = Helper.CreatePickNew(order);
			AssertEquals("Precondition", 1, pick.GetAllPickLines().Count());
			AssertEquals("Precondition", 1, orderLine1.ReleaseLines.Count);
			AssertEquals("Precondition", 0, orderLine2.ReleaseLines.Count);
			AssertEquals("Precondition", 1, orderLine1.PickLines.Count);
			AssertEquals("Precondition", 0, orderLine2.PickLines.Count);
			AssertEquals("Precondition", 10m, orderLine1.PickLines[0].WZ_Units);

			orderLine1.ReleaseLines[0].SerialNumber = "SN1";

			WhsPickLine getRCAWithAttrib(WhsPickLineCollection pl, ZString rca) => pl.Single(ca => ca.WZ_ReleaseCapturedSerialNumber == rca);
			AssertEquals("Precondition", 1m, orderLine1.PickLines.Single(ca => ca.WZ_ReleaseCapturedSerialNumber == "SN1").WZ_Units);
			AssertEquals("SN1", orderLine2.ReleaseLines[0].SerialNumber);

			orderLine2.ReleaseLines[0].Quantity = 5m;
			orderLine2.ReleaseLines[0].SerialNumber = "SN2";

			AssertEquals("Should have split pick lines", 2, orderLine1.PickLines.Count);
			AssertEquals("Should have split pick lines", 2, orderLine2.PickLines.Count);
			AssertEquals("Should have split pick lines", 5m, orderLine1.PickLines.Sum(p => p.WZ_Units));
			AssertEquals("Should have split pick lines", 5m, orderLine2.PickLines.Sum(p => p.WZ_Units));
			AssertEquals("Should have correct RCAs", 1m, getRCAWithAttrib(orderLine1.PickLines, "SN1").WZ_Units);
			AssertEquals("Should have correct RCAs", 1m, getRCAWithAttrib(orderLine2.PickLines, "SN2").WZ_Units);
		}

		#endregion

		#region TestReconcilePickLines_PartAttributes_ReleaseCaptured

		void TestReconcilePickLines_PartAttributes_ReleaseCaptured(AttributeNumber attributeType, string attributeColumn)
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAllAttributeType(data.Org1, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, attributeType, true, true);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m);
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 10m);
			var orderLine1 = order.Lines[0];
			var orderLine2 = Helper.CreateWhsOrderLine(order, data.Part1, 10m);

			var pick = Helper.CreatePickNew(order);
			AssertEquals("Precondition", 1, pick.GetAllPickLines().Count());
			AssertEquals("Precondition", 1, orderLine1.ReleaseLines.Count);
			AssertEquals("Precondition", 0, orderLine2.ReleaseLines.Count);
			AssertEquals("Precondition", 1, orderLine1.PickLines.Count);
			AssertEquals("Precondition", 0, orderLine2.PickLines.Count);
			AssertEquals("Precondition", 10m, orderLine1.PickLines[0].WZ_Units);

			orderLine1.ReleaseLines[0][attributeColumn] = "RED";

			var columnFixedForPartAttribVsPartAttribute = attributeColumn.Replace("ute", "");
			Func<WhsPickLineCollection, ZString, WhsPickLine> getRCAWithAttrib = (pl, rca) => pl.Single(ca => (ZString)ca["WZ_ReleaseCaptured" + columnFixedForPartAttribVsPartAttribute] == rca);
			AssertEquals("Precondition", 10m, getRCAWithAttrib(orderLine1.PickLines, "RED").WZ_Units);

			orderLine1.ReleaseLines[0].Quantity = 5m;
			orderLine2.ReleaseLines[0].Quantity = 5m;
			orderLine2.ReleaseLines[0][attributeColumn] = "BLUE";

			AssertEquals("Precondition", 1, orderLine1.ReleaseLines.Count);
			AssertEquals("Precondition", 1, orderLine2.ReleaseLines.Count);
			AssertEquals("Should have split pick lines", 1, orderLine1.PickLines.Count);
			AssertEquals("Should have split pick lines", 1, orderLine2.PickLines.Count);
			AssertEquals("Should have split pick lines", 5m, orderLine1.PickLines[0].WZ_Units);
			AssertEquals("Should have split pick lines", 5m, orderLine2.PickLines[0].WZ_Units);
			AssertEquals("Should have correct RCAs", 5m, getRCAWithAttrib(orderLine1.PickLines, "RED").WZ_Units);
			AssertEquals("Should have correct RCAs", 5m, getRCAWithAttrib(orderLine2.PickLines, "BLUE").WZ_Units);
			AssertNoErrors(orderLine1.ReleaseLines[0]);
			AssertNoErrors(orderLine2.ReleaseLines[0]);
			AssertNoExceptionThrown(Factory.Save);

			orderLine2.ReleaseLines[0].Quantity = 10m;
			orderLine1.ReleaseLines[0].Delete();

			AssertEquals("Precondition", 0, orderLine1.ReleaseLines.Count);
			AssertEquals("Precondition", 1, orderLine2.ReleaseLines.Count);
			AssertEquals("Should have moved pick lines", 0, orderLine1.PickLines.Count);
			AssertEquals("Should have moved pick lines", 1, orderLine2.PickLines.Count);
			AssertEquals("Should have moved pick lines", 10m, orderLine2.PickLines[0].WZ_Units);
			AssertEquals("Should have correct RCAs", 10m, getRCAWithAttrib(orderLine2.PickLines, "BLUE").WZ_Units);
			orderLine2.ReleaseLines[0].RunPreSaveValidation();
			AssertNoErrors(orderLine2.ReleaseLines[0]);
			AssertNoExceptionThrown(Factory.Save);

			orderLine2.ReleaseLines[0].Quantity = 8m;
			orderLine1.ReleaseLines[0].Quantity = 2m;
			orderLine1.ReleaseLines[0][attributeColumn] = "GREEN";

			AssertEquals("Precondition", 1, orderLine1.ReleaseLines.Count);
			AssertEquals("Precondition", 1, orderLine2.ReleaseLines.Count);
			AssertEquals("Should have moved pick lines", 1, orderLine1.PickLines.Count);
			AssertEquals("Should have moved pick lines", 1, orderLine2.PickLines.Count);
			AssertEquals("Should have moved pick lines", 8m, orderLine2.PickLines[0].WZ_Units);
			AssertEquals("Should have moved pick lines", 2m, orderLine1.PickLines[0].WZ_Units);
			AssertEquals("Should have correct RCAs", 8m, getRCAWithAttrib(orderLine2.PickLines, "BLUE").WZ_Units);
			AssertEquals("Should have correct RCAs", 2m, getRCAWithAttrib(orderLine1.PickLines, "GREEN").WZ_Units);
			orderLine1.ReleaseLines[0].RunPreSaveValidation();
			orderLine2.ReleaseLines[0].RunPreSaveValidation();
			AssertNoErrors(orderLine1.ReleaseLines[0]);
			AssertNoErrors(orderLine2.ReleaseLines[0]);
			AssertNoExceptionThrown(Factory.Save);

			orderLine1.ReleaseLines[0].Delete();
			orderLine2.ReleaseLines[0].Quantity = 10m;

			AssertEquals("Precondition", 1, orderLine2.ReleaseLines.Count);
			orderLine2.ReleaseLines[0].Quantity = 2m;

			AssertEquals("Precondition", 1, orderLine1.ReleaseLines.Count);
			orderLine1.ReleaseLines[0].Quantity = 2m;
			orderLine1.ReleaseLines[0][attributeColumn] = "RED";

			orderLine2.ReleaseLines.AddNew().Quantity = 2m;
			AssertEquals("Precondition", 2, orderLine2.ReleaseLines.Count);
			orderLine2.ReleaseLines[1][attributeColumn] = "GREEN";

			orderLine1.ReleaseLines.AddNew().Quantity = 2m;
			AssertEquals("Precondition", 2, orderLine1.ReleaseLines.Count);
			orderLine1.ReleaseLines[1][attributeColumn] = "YELLOW";

			orderLine2.ReleaseLines.AddNew().Quantity = 2m;
			AssertEquals("Precondition", 3, orderLine2.ReleaseLines.Count);
			orderLine2.ReleaseLines[2][attributeColumn] = "ORANGE";

			AssertEquals("Precondition", 2, orderLine1.ReleaseLines.Count);
			AssertEquals("Precondition", 3, orderLine2.ReleaseLines.Count);
			AssertEquals("Should have moved pick lines", 2, orderLine1.PickLines.Count);
			AssertEquals("Should have moved pick lines", 3, orderLine2.PickLines.Count);
			AssertEquals("Should have correct RCAs", 2m, getRCAWithAttrib(orderLine1.PickLines, "RED").WZ_Units);
			AssertEquals("Should have correct RCAs", 2m, getRCAWithAttrib(orderLine1.PickLines, "YELLOW").WZ_Units);
			AssertEquals("Should have correct RCAs", 2m, getRCAWithAttrib(orderLine2.PickLines, "BLUE").WZ_Units);
			AssertEquals("Should have correct RCAs", 2m, getRCAWithAttrib(orderLine2.PickLines, "GREEN").WZ_Units);
			AssertEquals("Should have correct RCAs", 2m, getRCAWithAttrib(orderLine2.PickLines, "ORANGE").WZ_Units);
			AssertNoErrors(orderLine1.ReleaseLines[0]);
			AssertNoErrors(orderLine1.ReleaseLines[1]);
			AssertNoErrors(orderLine2.ReleaseLines[0]);
			AssertNoErrors(orderLine2.ReleaseLines[1]);
			AssertNoErrors(orderLine2.ReleaseLines[2]);
			AssertNoExceptionThrown(Factory.Save);
		}

		public void TestReconcilePickLines_PartAttributes_ReleaseCaptured_ReconcileMultipleLines()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAllAttributeType(data.Org1, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Two, true, true);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m);
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 10m);
			var orderLine1 = order.Lines[0];
			var orderLine2 = Helper.CreateWhsOrderLine(order, data.Part1, 10m);

			var pick = Helper.CreatePickNew(order);
			AssertEquals("Precondition", 1, pick.GetAllPickLines().Count());
			AssertEquals("Precondition", 1, orderLine1.ReleaseLines.Count);
			AssertEquals("Precondition", 0, orderLine2.ReleaseLines.Count);
			AssertEquals("Precondition", 1, orderLine1.PickLines.Count);
			AssertEquals("Precondition", 0, orderLine2.PickLines.Count);
			AssertEquals("Precondition", 10m, orderLine1.PickLines[0].WZ_Units);

			var releaseLine1 = orderLine1.ReleaseLines[0];
			releaseLine1.Quantity = 8m;
			releaseLine1.PartAttribute1 = "RED";
			releaseLine1.PartAttribute2 = "BIG";

			var releaseLine2 = orderLine2.ReleaseLines[0];
			releaseLine2.Quantity = 1m;
			var releaseLine3 = orderLine2.ReleaseLines.AddNew();
			releaseLine3.Quantity = 1m;
			releaseLine3.PartAttribute1 = "BLUE";
			releaseLine3.PartAttribute2 = "SMALL";

			AssertEquals("Precondition", 1, orderLine1.ReleaseLines.Count);
			AssertEquals("Precondition", 2, orderLine2.ReleaseLines.Count);
			AssertEquals("Precondition", 1, orderLine1.PickLines.Count);
			AssertEquals("Precondition", 2, orderLine2.PickLines.Count);
			AssertEquals("Precondition", 8m, orderLine1.PickLines[0].WZ_Units);
			AssertEquals("Precondition", 1m, orderLine2.PickLines[0].WZ_Units);
			AssertEquals("Precondition", 1m, orderLine2.PickLines[1].WZ_Units);
			AssertEquals("Should have correct RCAs", "RED", orderLine1.PickLines[0].WZ_ReleaseCapturedPartAttrib1);
			AssertEquals("Should have correct RCAs", "BIG", orderLine1.PickLines[0].WZ_ReleaseCapturedPartAttrib2);
			AssertEquals("Should have correct RCAs", "BLUE", orderLine2.PickLines.Single(pl => pl.WZ_ReleaseCapturedPartAttrib2 == "SMALL").WZ_ReleaseCapturedPartAttrib1);
			AssertNoExceptionThrown(Factory.Save);

			releaseLine2.Quantity = 6m;
			releaseLine3.Quantity = 4m;
			releaseLine1.Quantity = 7m;

			AssertEquals("1 ReleaseLine for orderLine1", 1, orderLine1.ReleaseLines.Count);
			AssertEquals("2 ReleaseLines for orderLine2", 2, orderLine2.ReleaseLines.Count);
			AssertEquals("1 Pick Line remaining on orderLine1", 1, orderLine1.PickLines.Count);
			AssertEquals("1 Pick Line on orderLine2, another line is updated to have the Release Captured Attrits and then merged.", 1, orderLine2.PickLines.Count);
			AssertEquals("7 units remaining", 7m, orderLine1.PickLines[0].WZ_Units);
			AssertEquals("1 unit reconciled", 3m, orderLine2.PickLines[0].WZ_Units);
			AssertEquals("Should have correct RCAs", "RED", orderLine1.PickLines[0].WZ_ReleaseCapturedPartAttrib1);
			AssertEquals("Should have correct RCAs", "BIG", orderLine1.PickLines[0].WZ_ReleaseCapturedPartAttrib2);
			AssertEquals("Should have correct RCAs", "BLUE", orderLine2.PickLines.Single(pl => pl.WZ_ReleaseCapturedPartAttrib2 == "SMALL").WZ_ReleaseCapturedPartAttrib1);
			Assert("Not a valid state to save.", releaseLine3.QuantityInfo.HasError("The Release Captured Quantity for this Product cannot be greater than the Quantity Allocated for this Order Line."));

			orderLine1.ReleaseLines[0].Delete();

			AssertEquals("Release Line deleted.", 0, orderLine1.ReleaseLines.Count);
			AssertEquals("2 ReleaseLines for orderLine2", 2, orderLine2.ReleaseLines.Count);
			AssertEquals("No Pick Line remaining on orderLine1", 0, orderLine1.PickLines.Count);
			AssertEquals("2 Pick Lines on orderLine2", 2, orderLine2.PickLines.Count);
			var pickLine1 = orderLine2.PickLines.Single(l => l.WZ_Units == 6m);
			var pickLine2 = orderLine2.PickLines.Single(l => l.WZ_Units == 4m);
			AssertEquals("Should have correct RCAs", "", pickLine1.WZ_ReleaseCapturedPartAttrib1);
			AssertEquals("Should have correct RCAs", "", pickLine1.WZ_ReleaseCapturedPartAttrib2);
			AssertEquals("Should have correct RCAs", "BLUE", pickLine2.WZ_ReleaseCapturedPartAttrib1);
			AssertEquals("Should have correct RCAs", "SMALL", pickLine2.WZ_ReleaseCapturedPartAttrib2);
			AssertNoExceptionThrown(Factory.Save);
		}

		public void TestReconcilePickLines_PartAttributes_ReleaseCaptured_ReconcileMultipleLines_MoveFromCorrectPickLines_1()
		{
			TestReconcilePickLines_PartAttributes_ReleaseCaptured_ReconcileMultipleLines_MoveFromCorrectPickLinesCore(releaseLineToDelete: 1);
		}

		public void TestReconcilePickLines_PartAttributes_ReleaseCaptured_ReconcileMultipleLines_MoveFromCorrectPickLines_2()
		{
			TestReconcilePickLines_PartAttributes_ReleaseCaptured_ReconcileMultipleLines_MoveFromCorrectPickLinesCore(releaseLineToDelete: 2);
		}

		public void TestReconcilePickLines_PartAttributes_ReleaseCaptured_ReconcileMultipleLines_MoveFromCorrectPickLines_3()
		{
			TestReconcilePickLines_PartAttributes_ReleaseCaptured_ReconcileMultipleLines_MoveFromCorrectPickLinesCore(releaseLineToDelete: 3);
		}

		void TestReconcilePickLines_PartAttributes_ReleaseCaptured_ReconcileMultipleLines_MoveFromCorrectPickLinesCore(int releaseLineToDelete)
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAllAttributeType(data.Org1, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true, true);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m);
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 10m);
			var orderLine1 = order.Lines[0];
			var orderLine2 = Helper.CreateWhsOrderLine(order, data.Part1, 10m);

			var pick = Helper.CreatePickNew(order);
			AssertEquals("Precondition", 1, pick.GetAllPickLines().Count());
			AssertEquals("Precondition", 1, orderLine1.ReleaseLines.Count);
			AssertEquals("Precondition", 0, orderLine2.ReleaseLines.Count);
			AssertEquals("Precondition", 1, orderLine1.PickLines.Count);
			AssertEquals("Precondition", 0, orderLine2.PickLines.Count);
			AssertEquals("Precondition", 10m, orderLine1.PickLines[0].WZ_Units);

			var releaseLine0 = orderLine1.ReleaseLines[0];
			releaseLine0.Quantity = 7m;
			releaseLine0.PartAttribute1 = "RED";

			var releaseLine1 = orderLine2.ReleaseLines[0];
			releaseLine1.Quantity = 1m;
			releaseLine1.PartAttribute1 = "GREEN";
			var releaseLine2 = orderLine2.ReleaseLines.AddNew();
			releaseLine2.Quantity = 1m;
			releaseLine2.PartAttribute1 = "BLUE";
			var releaseLine3 = orderLine2.ReleaseLines.AddNew();
			releaseLine3.Quantity = 1m;
			releaseLine3.PartAttribute1 = "";

			AssertEquals("Precondition", 1, orderLine1.ReleaseLines.Count);
			AssertEquals("Precondition", 3, orderLine2.ReleaseLines.Count);
			AssertEquals("Precondition", 1, orderLine1.PickLines.Count);
			AssertEquals("Precondition", 3, orderLine2.PickLines.Count);
			AssertEquals("Precondition", 7m, orderLine1.PickLines[0].WZ_Units);
			AssertEquals("Precondition", 1m, orderLine2.PickLines[0].WZ_Units);
			AssertEquals("Precondition", 1m, orderLine2.PickLines[1].WZ_Units);
			AssertEquals("Precondition", 1m, orderLine2.PickLines[2].WZ_Units);
			AssertEquals("Should have correct RCAs", "RED", orderLine1.PickLines[0].WZ_ReleaseCapturedPartAttrib1);
			AssertEquals("Should have correct RCAs", 1m, orderLine2.PickLines.Single(pl => pl.WZ_ReleaseCapturedPartAttrib1 == "GREEN").WZ_Units);
			AssertEquals("Should have correct RCAs", 1m, orderLine2.PickLines.Single(pl => pl.WZ_ReleaseCapturedPartAttrib1 == "BLUE").WZ_Units);
			AssertEquals("Should have correct RCAs", 1m, orderLine2.PickLines.Single(pl => pl.WZ_ReleaseCapturedPartAttrib1 == "").WZ_Units);
			AssertNoExceptionThrown(Factory.Save);

			releaseLine0.Quantity = 9m;

			if (releaseLineToDelete == 1)
			{
				releaseLine1.Delete();
			}
			else if (releaseLineToDelete == 2)
			{
				releaseLine2.Delete();
			}
			else
			{
				releaseLine3.Delete();
			}

			AssertEquals("1 ReleaseLine for orderLine1", 1, orderLine1.ReleaseLines.Count);
			AssertEquals("2 ReleaseLines for orderLine2", 2, orderLine2.ReleaseLines.Count);
			AssertEquals("1 Pick Line on orderLine1", 1, orderLine1.PickLines.Count);
			AssertEquals("2 Pick Line on orderLine2", 2, orderLine2.PickLines.Count);
			AssertEquals("8 units, 1 reconciled from orderLine2", 8m, orderLine1.PickLines[0].WZ_Units);
			AssertEquals("1 unit remaining", 1m, orderLine2.PickLines[0].WZ_Units);
			AssertEquals("1 unit remaining", 1m, orderLine2.PickLines[1].WZ_Units);
			AssertEquals("Should have correct RCAs", "RED", orderLine1.PickLines[0].WZ_ReleaseCapturedPartAttrib1);
			if (releaseLineToDelete == 1)
			{
				AssertEquals("Should have correct RCAs", 1m, orderLine2.PickLines.Single(pl => pl.WZ_ReleaseCapturedPartAttrib1 == "BLUE").WZ_Units);
				AssertEquals("Should have correct RCAs", 1m, orderLine2.PickLines.Single(pl => pl.WZ_ReleaseCapturedPartAttrib1 == "").WZ_Units);
			}
			else if (releaseLineToDelete == 2)
			{
				AssertEquals("Should have correct RCAs", 1m, orderLine2.PickLines.Single(pl => pl.WZ_ReleaseCapturedPartAttrib1 == "GREEN").WZ_Units);
				AssertEquals("Should have correct RCAs", 1m, orderLine2.PickLines.Single(pl => pl.WZ_ReleaseCapturedPartAttrib1 == "").WZ_Units);
			}
			else
			{
				AssertEquals("Should have correct RCAs", 1m, orderLine2.PickLines.Single(pl => pl.WZ_ReleaseCapturedPartAttrib1 == "BLUE").WZ_Units);
				AssertEquals("Should have correct RCAs", 1m, orderLine2.PickLines.Single(pl => pl.WZ_ReleaseCapturedPartAttrib1 == "GREEN").WZ_Units);
			}

			AssertNoExceptionThrown(Factory.Save);
		}

		#endregion

		#endregion

		#region TestReconcilePickLines_KeepsAvailableInventoryPickLineSplitUpToDate

		public void TestReconcilePickLines_KeepsAvailableInventoryPickLineSplitUpToDate()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "1", data.Part1, 20m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			var orderLine1 = order.Lines[0];
			var orderLine2 = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			order.WD_PickOption = WhsPickOption.Codes.Manual;

			var pick = Helper.CreatePickNew(order);
			var availableInventory = pick.OrderedInventories[0].AvailableInventories[0];
			availableInventory.PickLineQuantity = 10m;
			AssertEquals("Should have added one release line.", 1, orderLine1.ReleaseLines.Count);
			AssertEquals("Should have one Picked Details Line.", 1, availableInventory.AvailableInventoriesSplitByPickedDetails.Count);
			AssertContainsExactElementsInAnyOrder(availableInventory.PickLines, availableInventory.AvailableInventoriesSplitByPickedDetails[0].PickLinesForPickingDetails);

			var releaseLine1 = orderLine1.ReleaseLines[0];
			AssertEquals("Precondition: Should be fully released on this line.", 10m, releaseLine1.Quantity);
			AssertEquals("Precondition: Should be fully released on this line.", 0m, releaseLine1.UnreleasedQty);

			releaseLine1.Quantity = 5m;
			AssertEquals("Should have added one release line on orderLine2.", 1, orderLine2.ReleaseLines.Count);

			var releaseLine2 = orderLine2.ReleaseLines[0];
			// testing split
			releaseLine2.Quantity = 5m;
			AssertEquals("Each order line has a single pick line", 1, orderLine1.PickLines.Count);
			AssertEquals("Each order line has a single pick line", 1, orderLine2.PickLines.Count);
			AssertEquals("Precondition", 5m, orderLine1.PickLines[0].WZ_Units);
			AssertEquals("Precondition", 5m, orderLine2.PickLines[0].WZ_Units);
			AssertContainsExactElementsInAnyOrder(new[] { orderLine1.PickLines.Single(), orderLine2.PickLines.Single() }, availableInventory.PickLines);
			AssertEquals("Should have one Picked Details Line.", 1, availableInventory.AvailableInventoriesSplitByPickedDetails.Count);
			AssertContainsExactElementsInAnyOrder(availableInventory.PickLines, availableInventory.AvailableInventoriesSplitByPickedDetails[0].PickLinesForPickingDetails);

			// testing merge
			releaseLine1.Quantity = 0m;
			releaseLine2.Quantity = 10m;
			AssertEquals("Should have no PickLines", 0, orderLine1.PickLines.Count);
			AssertEquals("Each order line has a single pick line", 1, orderLine2.PickLines.Count);
			AssertEquals("Precondition", 10m, orderLine2.PickLines[0].WZ_Units);
			AssertContainsExactElementsInAnyOrder(orderLine2.PickLines, availableInventory.PickLines);
			AssertEquals("Should have one Picked Details Line.", 1, availableInventory.AvailableInventoriesSplitByPickedDetails.Count);
			AssertContainsExactElementsInAnyOrder(availableInventory.PickLines, availableInventory.AvailableInventoriesSplitByPickedDetails[0].PickLinesForPickingDetails);

			// testing move
			releaseLine2.Quantity = 0m;
			var newReleaseLine1 = orderLine1.ReleaseLines[0];
			newReleaseLine1.Quantity = 10m;
			AssertEquals("Each order line has a single pick line", 1, orderLine1.PickLines.Count);
			AssertEquals("Should have no PickLines", 0, orderLine2.PickLines.Count);
			AssertEquals("Precondition", 10m, orderLine1.PickLines[0].WZ_Units);
			AssertContainsExactElementsInAnyOrder(orderLine1.PickLines, availableInventory.PickLines);
			AssertEquals("Should have one Picked Details Line.", 1, availableInventory.AvailableInventoriesSplitByPickedDetails.Count);
			AssertContainsExactElementsInAnyOrder(availableInventory.PickLines, availableInventory.AvailableInventoriesSplitByPickedDetails[0].PickLinesForPickingDetails);
			AssertNoExceptionThrown(Factory.Save);
		}

		#endregion

		#region TestReconcilePickLines_MultipleOrderedInventory_KeepsAvailableInventoryPickLineSplitUpToDateWhenMovingWholePickLine

		public void TestReconcilePickLines_MultipleOrderedInventory_KeepsAvailableInventoryPickLineSplitUpToDateWhenMovingWholePickLine()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAllAttributeType(data.Org1, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, ZDate.Empty, ZDate.Empty, "1", "", "", "");
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			var orderLine1 = order.Lines[0];
			var orderLine2 = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			orderLine2.WE_PartAttrib1 = "1";

			var pick = Helper.CreatePickNew(order);
			AssertEquals("Precondition.", 2, pick.OrderedInventories.Count);

			// Moving pick line
			AssertEquals("Precondition.", 0, orderLine1.ReleaseLines.Count);
			AssertEquals("Precondition.", 1, orderLine2.ReleaseLines.Count);
			AssertEquals("Precondition.", 0, orderLine1.PickLines.Count);
			AssertEquals("Precondition.", 1, orderLine2.PickLines.Count);

			var availableInventory1 = pick.OrderedInventories.Cast<WhsPickOrderedInventory>().Single(o => o.PartAttrib1 == "").AvailableInventories[0];
			var availableInventory2 = pick.OrderedInventories.Cast<WhsPickOrderedInventory>().Single(o => o.PartAttrib1 == "1").AvailableInventories[0];
			AssertEquals("Should have *no* Picked Details Line.", 0, availableInventory1.AvailableInventoriesSplitByPickedDetails.Count);
			AssertEquals("Should have one Picked Details Line.", 1, availableInventory2.AvailableInventoriesSplitByPickedDetails.Count);
			AssertEquals("Available Inventory should have correct PickLine Quantity.", 0m, availableInventory1.PickLineQuantity);
			AssertEquals("Available Inventory should have correct PickLine Quantity.", 10m, availableInventory2.PickLineQuantity);
			AssertContainsExactElementsInAnyOrder(availableInventory2.PickLines, availableInventory2.AvailableInventoriesSplitByPickedDetails[0].PickLinesForPickingDetails);

			orderLine2.ReleaseLines[0].Quantity = 0m;
			orderLine1.ReleaseLines[0].Quantity = 10m;
			AssertEquals("Should have moved pick lines.", 1, orderLine1.PickLines.Count);
			AssertEquals("Should have moved pick lines.", 0, orderLine2.PickLines.Count);
			AssertEquals("Should have one Picked Details Line.", 1, availableInventory1.AvailableInventoriesSplitByPickedDetails.Count);
			AssertEquals("Should have *no* Picked Details Line.", 0, availableInventory2.AvailableInventoriesSplitByPickedDetails.Count);
			AssertEquals("Available Inventory should have correct PickLine Quantity.", 10m, availableInventory1.PickLineQuantity);
			AssertEquals("Available Inventory should have correct PickLine Quantity.", 0m, availableInventory2.PickLineQuantity);
			AssertContainsExactElementsInAnyOrder(availableInventory1.PickLines, availableInventory1.AvailableInventoriesSplitByPickedDetails[0].PickLinesForPickingDetails);
			AssertNoExceptionThrown(Factory.Save);
		}

		#endregion

		#region TestReconcilePickLines_MultipleOrderedInventory_KeepsAvailableInventoryPickLineSplitUpToDateWhenMergingPickLine

		public void TestReconcilePickLines_MultipleOrderedInventory_KeepsAvailableInventoryPickLineSplitUpToDateWhenMergingPickLine()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAllAttributeType(data.Org1, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, ZDate.Empty, ZDate.Empty, "1", "", "", "");
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			var orderLine1 = order.Lines[0];
			var orderLine2 = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			orderLine2.WE_PartAttrib1 = "1";

			var pick = Helper.CreatePickNew(order);
			AssertEquals("Precondition.", 2, pick.OrderedInventories.Count);

			// Moving pick line
			AssertEquals("Precondition.", 0, orderLine1.ReleaseLines.Count);
			AssertEquals("Precondition.", 1, orderLine2.ReleaseLines.Count);
			AssertEquals("Precondition.", 0, orderLine1.PickLines.Count);
			AssertEquals("Precondition.", 1, orderLine2.PickLines.Count);

			var availableInventory1 = pick.OrderedInventories.Cast<WhsPickOrderedInventory>().Single(o => o.PartAttrib1 == "").AvailableInventories[0];
			var availableInventory2 = pick.OrderedInventories.Cast<WhsPickOrderedInventory>().Single(o => o.PartAttrib1 == "1").AvailableInventories[0];
			AssertEquals("Should have *no* Picked Details Line.", 0, availableInventory1.AvailableInventoriesSplitByPickedDetails.Count);
			AssertEquals("Should have one Picked Details Line.", 1, availableInventory2.AvailableInventoriesSplitByPickedDetails.Count);
			AssertEquals("Available Inventory should have correct PickLine Quantity.", 0m, availableInventory1.PickLineQuantity);
			AssertEquals("Available Inventory should have correct PickLine Quantity.", 10m, availableInventory2.PickLineQuantity);
			AssertEquals("Available Inventory Split should have correct StockUnit Quantity.", 10m, availableInventory2.AvailableInventoriesSplitByPickedDetails[0].StockUnitQuantity);
			AssertContainsExactElementsInAnyOrder(availableInventory2.PickLines, availableInventory2.AvailableInventoriesSplitByPickedDetails[0].PickLinesForPickingDetails);

			orderLine2.ReleaseLines[0].Quantity = 8m;
			orderLine1.ReleaseLines[0].Quantity = 2m;
			AssertEquals("Should have one Picked Details Line.", 1, availableInventory1.AvailableInventoriesSplitByPickedDetails.Count);
			AssertEquals("Should have one Picked Details Line.", 1, availableInventory2.AvailableInventoriesSplitByPickedDetails.Count);
			AssertEquals("Available Inventory should have correct PickLine Quantity.", 2m, availableInventory1.PickLineQuantity);
			AssertEquals("Available Inventory should have correct PickLine Quantity.", 8m, availableInventory2.PickLineQuantity);
			AssertContainsExactElementsInAnyOrder(availableInventory1.PickLines, availableInventory1.AvailableInventoriesSplitByPickedDetails[0].PickLinesForPickingDetails);
			AssertContainsExactElementsInAnyOrder(availableInventory2.PickLines, availableInventory2.AvailableInventoriesSplitByPickedDetails[0].PickLinesForPickingDetails);

			// force merging of pickline
			orderLine2.ReleaseLines[0].Quantity = 7m;
			orderLine1.ReleaseLines[0].Quantity = 3m;
			AssertEquals("Should have split pick lines.", 1, orderLine1.PickLines.Count);
			AssertEquals("Should have split pick lines.", 1, orderLine2.PickLines.Count);
			AssertEquals("Should have one Picked Details Line.", 1, availableInventory1.AvailableInventoriesSplitByPickedDetails.Count);
			AssertEquals("Should have one Picked Details Line.", 1, availableInventory2.AvailableInventoriesSplitByPickedDetails.Count);
			AssertEquals("Available Inventory should have correct PickLine Quantity.", 3m, availableInventory1.PickLineQuantity);
			AssertEquals("Available Inventory should have correct PickLine Quantity.", 7m, availableInventory2.PickLineQuantity);
			AssertEquals("Available Inventory Split should have correct StockUnit Quantity.", 3m, availableInventory1.AvailableInventoriesSplitByPickedDetails[0].StockUnitQuantity);
			AssertEquals("Available Inventory Split should have correct StockUnit Quantity.", 7m, availableInventory2.AvailableInventoriesSplitByPickedDetails[0].StockUnitQuantity);
			AssertContainsExactElementsInAnyOrder(availableInventory1.PickLines, availableInventory1.AvailableInventoriesSplitByPickedDetails[0].PickLinesForPickingDetails);
			AssertContainsExactElementsInAnyOrder(availableInventory2.PickLines, availableInventory2.AvailableInventoriesSplitByPickedDetails[0].PickLinesForPickingDetails);
		}

		#endregion

		#region TestReconcilePickLines_MultipleOrderedInventory_KeepsAvailableInventoryPickLineSplitUpToDateWhenSplittingPickLine

		public void TestReconcilePickLines_MultipleOrderedInventory_KeepsAvailableInventoryPickLineSplitUpToDateWhenSplittingPickLine()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAllAttributeType(data.Org1, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, ZDate.Empty, ZDate.Empty, "1", "", "", "");
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			var orderLine1 = order.Lines[0];
			var orderLine2 = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			orderLine2.WE_PartAttrib1 = "1";

			var pick = Helper.CreatePickNew(order);
			AssertEquals("Precondition.", 2, pick.OrderedInventories.Count);
			AssertEquals("Precondition.", 0, orderLine1.ReleaseLines.Count);
			AssertEquals("Precondition.", 1, orderLine2.ReleaseLines.Count);
			AssertEquals("Precondition.", 0, orderLine1.PickLines.Count);
			AssertEquals("Precondition.", 1, orderLine2.PickLines.Count);

			var availableInventory1 = pick.OrderedInventories.Cast<WhsPickOrderedInventory>().Single(o => o.PartAttrib1 == "").AvailableInventories[0];
			var availableInventory2 = pick.OrderedInventories.Cast<WhsPickOrderedInventory>().Single(o => o.PartAttrib1 == "1").AvailableInventories[0];
			AssertEquals("Should have *no* Picked Details Line.", 0, availableInventory1.AvailableInventoriesSplitByPickedDetails.Count);
			AssertEquals("Should have one Picked Details Line.", 1, availableInventory2.AvailableInventoriesSplitByPickedDetails.Count);
			AssertEquals("Available Inventory should have correct PickLine Quantity.", 0m, availableInventory1.PickLineQuantity);
			AssertEquals("Available Inventory should have correct PickLine Quantity.", 10m, availableInventory2.PickLineQuantity);
			AssertEquals("Available Inventory Split should have correct StockUnit Quantity.", 10m, availableInventory2.AvailableInventoriesSplitByPickedDetails[0].StockUnitQuantity);
			AssertContainsExactElementsInAnyOrder(availableInventory2.PickLines, availableInventory2.AvailableInventoriesSplitByPickedDetails[0].PickLinesForPickingDetails);

			orderLine2.ReleaseLines[0].Quantity = 8m;
			orderLine1.ReleaseLines[0].Quantity = 2m;
			AssertEquals("Should have split pick lines.", 1, orderLine1.PickLines.Count);
			AssertEquals("Should have split pick lines.", 1, orderLine2.PickLines.Count);
			AssertEquals("Should have one Picked Details Line.", 1, availableInventory1.AvailableInventoriesSplitByPickedDetails.Count);
			AssertEquals("Should have one Picked Details Line.", 1, availableInventory2.AvailableInventoriesSplitByPickedDetails.Count);
			AssertEquals("Available Inventory should have correct PickLine Quantity.", 2m, availableInventory1.PickLineQuantity);
			AssertEquals("Available Inventory should have correct PickLine Quantity.", 8m, availableInventory2.PickLineQuantity);
			AssertEquals("Available Inventory Split should have correct StockUnit Quantity.", 2m, availableInventory1.AvailableInventoriesSplitByPickedDetails[0].StockUnitQuantity);
			AssertEquals("Available Inventory Split should have correct StockUnit Quantity.", 8m, availableInventory2.AvailableInventoriesSplitByPickedDetails[0].StockUnitQuantity);
			AssertContainsExactElementsInAnyOrder(availableInventory1.PickLines, availableInventory1.AvailableInventoriesSplitByPickedDetails[0].PickLinesForPickingDetails);
			AssertContainsExactElementsInAnyOrder(availableInventory2.PickLines, availableInventory2.AvailableInventoriesSplitByPickedDetails[0].PickLinesForPickingDetails);
			AssertNoExceptionThrown(Factory.Save);
		}

		#endregion

		#region TestReconcilePickLines_OrderLineDeletedAndRecreated

		public void TestReconcilePickLines_OrderLineDeletedAndRecreated()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAllAttributeType(data.Org1, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, ZDate.Empty, ZDate.Empty, "1", "", "", "");
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 6m);
			var orderLine1 = order.Lines[0];
			var orderLine2 = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			orderLine2.WE_PartAttrib1 = "1";
			var pick = Helper.CreatePickNew(order);
			Factory.Save();

			AssertNotNull(pick.OrderedInventories.Cast<WhsPickOrderedInventory>().FirstOrDefault(orderedInventory => orderedInventory.QuantityOrdered == 6));
			AssertNotNull(pick.OrderedInventories.Cast<WhsPickOrderedInventory>().FirstOrDefault(orderedInventory => orderedInventory.QuantityOrdered == 10));
			orderLine2.ReleaseLines[0].Quantity = 8m;

			var newFactory = new BusinessObjectFactory();
			var orderInNewFactory = newFactory.Load<WhsOrder>(order.PK);
			var orderLine1InNewFactory = orderInNewFactory.Lines.Single(line => line.WE_TransactionQuantity == 6m);
			orderLine1InNewFactory.WE_TransactionQuantity = 0m;
			var orderLine3InNewFactory = orderInNewFactory.Lines.AddNew();
			orderLine3InNewFactory.WE_TransactionQuantity = 4m;
			orderLine3InNewFactory.WE_OP = data.Part1.PK;
			orderInNewFactory.IsSavedFromOrderForm = true;
			orderInNewFactory.RunPreSaveValidation();
			newFactory.Save();

			var orderLine3InOrigFactory = Factory.Load<WhsOrderLine>(orderLine3InNewFactory.PK);
			AssertEquals("New order line has release lines in the original factory.", true, orderLine3InOrigFactory.ReleaseLines.Count > 0);
			AssertNoExceptionThrown(() => orderLine3InOrigFactory.ReleaseLines[0].Quantity = 2m);
		}

		#endregion

		#region TestReconcilePickLines_NewOrderLineAdded

		public void TestReconcilePickLines_NewOrderLineAdded()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAllAttributeType(data.Org1, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, ZDate.Empty, ZDate.Empty, "1", "", "", "");
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 6m);
			var orderLine1 = order.Lines[0];
			var orderLine2 = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			orderLine2.WE_PartAttrib1 = "1";
			var pick = Helper.CreatePickNew(order);
			Factory.Save();

			AssertNotNull(pick.OrderedInventories.Cast<WhsPickOrderedInventory>().FirstOrDefault(orderedInventory => orderedInventory.QuantityOrdered == 6));
			AssertNotNull(pick.OrderedInventories.Cast<WhsPickOrderedInventory>().FirstOrDefault(orderedInventory => orderedInventory.QuantityOrdered == 10));
			orderLine2.ReleaseLines[0].Quantity = 8m;

			var newFactory = new BusinessObjectFactory();
			var orderInNewFactory = newFactory.Load<WhsOrder>(order.PK);
			var orderLine3InNewFactory = orderInNewFactory.Lines.AddNew();
			orderLine3InNewFactory.WE_TransactionQuantity = 4m;
			orderLine3InNewFactory.WE_OP = data.Part1.PK;
			orderInNewFactory.IsSavedFromOrderForm = true;
			orderInNewFactory.RunPreSaveValidation();
			newFactory.Save();

			var orderLine3InOrigFactory = Factory.Load<WhsOrderLine>(orderLine3InNewFactory.PK);
			AssertEquals("New order line has release lines in the original factory.", true, orderLine3InOrigFactory.ReleaseLines.Count > 0);
			AssertNoExceptionThrown(() => orderLine3InOrigFactory.ReleaseLines[0].Quantity = 2m);
		}

		#endregion

		#endregion

		#region TestPickLinesUnreleasedByAttributes_DoesNotRemoveIncorrectEntries

		public void TestPickLinesUnreleasedByAttributes_DoesNotRemoveIncorrectEntries()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, true);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true, setReleaseCaptured: true);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, ZDate.Empty, ZDate.Empty, "ATTRIBUTE1", "", "", "");
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 3m);
			Helper.CreatePickNew(order);

			var releaseLines = order.Lines[0].ReleaseLines;
			var releaseLine1 = releaseLines[0];
			releaseLine1.SerialNumber = "SN1";
			AssertEquals("Quantity should be reduced.", 1m, releaseLine1.Quantity);

			var releaseLine2 = (WhsReleaseLine)((IBindingList)releaseLines).AddNew();
			AssertEquals("Precondition: Non-RC Attribs should be copied", "ATTRIBUTE1", releaseLine2.PartAttribute1);

			releaseLine2.SerialNumber = "SN2";
			((ICancelAddNew)releaseLines).EndNew(1);
			AssertEquals("Quantity should be reduced.", 1m, releaseLine2.Quantity);

			var releaseLine3 = (WhsReleaseLine)((IBindingList)releaseLines).AddNew();
			AssertEquals("Precondition: Non-RC Attribs should be copied", "ATTRIBUTE1", releaseLine3.PartAttribute1);

			releaseLine3.SerialNumber = "SN3";
			((ICancelAddNew)releaseLines).EndNew(2);
			AssertEquals("Quantity should stay at 1.", 1m, releaseLine3.Quantity);

			var invalidReleaseLine = (WhsReleaseLine)((IBindingList)releaseLines).AddNew();
			invalidReleaseLine.SerialNumber = "SN4";
			((ICancelAddNew)releaseLines).EndNew(3);
			AssertEquals("Quantity should be 0.", 0m, invalidReleaseLine.Quantity);
			AssertEquals("Non-RC Attribs should not be copied.", "", invalidReleaseLine.PartAttribute1);

			invalidReleaseLine.Delete();
			AssertNoExceptionThrown(() => releaseLine3.Delete());
		}

		#endregion

		#region TestPickLinesUnreleasedByAttributes_MissingKeyException_ShowCorrectInfo

		public void TestPickLinesUnreleasedByAttributes_MissingKeyException_ShowCorrectInfo()
		{
			var today = ZDateTime.Today;
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, PartAttributeTypeList.Codes.VIN);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Two, PartAttributeTypeList.Codes.Mandatory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Three, PartAttributeTypeList.Codes.Mandatory);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true, setReleaseCaptured: true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Two, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Three, true);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var inventoryLocation = data.Whs1.FindLocation("A-1-1");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 50m, inventoryLocation, ZDate.Empty, ZDate.Empty, "", "HARLEY", "BIKE", "");
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocketWithoutUserConfirmation();
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 20m);
			var orderLine = order.Lines[0];
			orderLine.WE_PartAttrib2 = "HARLEY";
			orderLine.WE_PartAttrib3 = "BIKE";
			var pick = Helper.CreatePickNew(order);
			Factory.Save();

			orderLine.ReleaseLines[0].PartAttribute1 = "RED";
			orderLine.ReleaseLines[0].Quantity = 1m;
			orderLine.ReleaseLines.AddNew().Quantity = 1m;
			orderLine.ReleaseLines.AddNew().Quantity = 1m;
			orderLine.ReleaseLines.AddNew().Quantity = 1m;
			Factory.Save();

			ErrorReporter.Clear();
			AssertNoExceptionThrown(() => orderLine.ReleaseLines[0].Delete());
			AssertExceptionThrown<KeyNotFoundException>(() => orderLine.ReleaseLines[1].Delete());
			var reportMessage = $@"The given key was not present in the dictionary.
Current State Information:
Key: [Pick PK: {pick.PK} | Product PK: {data.Part1.PK} | Client PK: {data.Org1.PK}]
Dictionary:
.

Order Number: {order.WD_DocketID}
OrderLinePK: {orderLine.PK}
Part Attribute 1: 
Part Attribute 2: 
Part Attribute 3: 
Serial Number: 
Packing Date: 
Expiry Date: 
Difference: -1
AttributesKey: |||||

ReleaseLine.IsDeleted: False
ReleaseLinesByProductAndClient contains Key: True
Does returned ReleaseLinesByProductAndClient value contain ReleaseLine: True";
			AssertEquals("Correct message reported.", reportMessage, ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
		}

		#endregion

		#region TestNotifyChange_NoPick

		public void TestNotifyChange_NoPick_ReleaseLinesNotBuilt()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Org1.MiscServ.OM_WhsIsRecalculateOrderPricing = true;
			Factory.Save();

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine1 = Helper.CreateWhsOrderLine(order, data.Part1, 5m);
			orderLine1.WE_UnitPriceAfterDiscount = 1m;
			var reservePickLine = Helper.CreateReservePickLine(orderLine1, receive.Inventory[0], 5m);
			AssertEquals("Precondition", 5m, orderLine1.WE_ExtendedLinePrice);

			AssertNoExceptionThrown(() => ReleaseLinesOnPickManager.NotifyChange(Factory, reservePickLine, -5m));
			AssertEquals(5m, orderLine1.WE_ExtendedLinePrice);
		}

		#endregion

		#region TestNotifyChange_NoNullReferenceExceptionWhenPickIsDeletedFromOrder

		public void TestNotifyChange_NoNullReferenceExceptionWhenPickIsDeletedFromOrder()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 2);
			var locationA_1 = data.Whs1.FindLocation("A-1");
			Helper.SetClientAllAttributeType(data.Org1, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true, true);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, locationA_1, "");
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 30m);
			var orderLine = order.Lines[0];
			var pick = Helper.CreatePickNew(order);
			var pickLines = pick.GetAllPickLines();
			var pickLine1 = pickLines.Single(pl => pl.Inventory == inventory1);

			_ = orderLine.ReleaseLines;

			order.WD_WP = ZGuid.Empty;
			ReleaseLinesOnPickManager.RegisterReleaseLines(Factory, orderLine.ReleaseLines, (WhsPickableDocketLine)pickLine1.DocketLine);

			AssertNoExceptionThrown("No NullReferenceException should happen.", () => ReleaseLinesOnPickManager.NotifyChange(Factory, pickLine1, -10m));
		}

		#endregion
	}
}
