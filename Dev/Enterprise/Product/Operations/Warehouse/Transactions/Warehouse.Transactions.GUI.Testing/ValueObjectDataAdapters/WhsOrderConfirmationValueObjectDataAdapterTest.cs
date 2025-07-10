using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.Warehouse.Transactions.DataTransfer;
using Enterprise.ZArchitecture;
using NUnit.Framework;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Warehouse.Transactions.GUI.Testing;

[TestedType(typeof(WhsOrderConfirmationValueObjectDataAdapter))]
sealed class WhsOrderConfirmationValueObjectDataAdapterTest : WhsOrderValueObjectDataAdapterTest
{
	#region Overrides

	#region Export Data

	protected override void PopulateWhsDocketForExport(WhsOrder whsDocket)
	{
		base.PopulateWhsDocketForExport(whsDocket);

		CreateNewPopulatedDocketLine(whsDocket, Product1, 30m, "PQ3", "", 3, 0, "PT1", ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZDate.Empty, ZDate.Empty, ZDate.Empty, ZDate.Empty, ZDateTime.Empty, ZDecimal.Zero, ZDecimal.Zero, ZDecimal.Zero, ZDecimal.Zero, ZDecimal.Zero, ZBool.False, ZBool.False, ZBool.False, ZBool.False, ZBool.False, Factory.New<WhsBondedWarehouseAttribute>(), "Line Coments3", new ZDate(2007, 10, 14), new ZDate(2007, 10, 15), 0m);
		CreateNewPopulatedDocketLine(whsDocket, Product2, 40m, "PQ4", "", 4, 0, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZDate.Empty, ZDate.Empty, ZDate.Empty, ZDate.Empty, ZDateTime.Empty, ZDecimal.Zero, ZDecimal.Zero, ZDecimal.Zero, ZDecimal.Zero, ZDecimal.Zero, ZBool.False, ZBool.False, ZBool.False, ZBool.False, ZBool.False, Factory.New<WhsBondedWarehouseAttribute>(), "Line Coments4", new ZDate(2007, 10, 16), new ZDate(2007, 10, 17), 0m);

		SetClientAttributesType(Client);
		SetProductAttributesUse(Client, Product1.Parent);
		SetProductAttributesUse(Client, Product2.Parent);

		// Hack: turn off pickline balancing trigger for this test
		WhsTestHelperFunctions.SuspendDatabaseProcForTest($"{WhsTestHelperFunctions.WhsCheckTransactionAndPickQtyIsCorrect} (@TransactionLinePKs dbo.TVP_uniqueidentifier READONLY)", "0");
		// Hack: Unfinalise the order, neccessary as the order added unpicked reserved pick lines after the docket was finalised without a Pick and these tests are too tightly coupled to easily change
		var previousFinalisedDate = whsDocket.WD_FinalisedDate;
		whsDocket.WD_DocketStatus = "ENT";
		whsDocket.Lines.ForEach(ol => ol.WE_DocketLineStatus = "");
		whsDocket.WD_FinalisedDate = ZDateTimeOffset.Empty;

		Factory.Save();

		var receive1 = Helper.CreateWhsReceive(Client, Whs, "IN1");
		Helper.CreateWhsReceiveInventoryLine(receive1, Product1.Parent, 5m, new ZDate(2007, 10, 14), new ZDate(2007, 10, 15), "PT1", ZString.Empty, ZString.Empty, ZString.Empty);
		Helper.CreateWhsReceiveInventoryLine(receive1, Product1.Parent, 10m, new ZDate(2007, 10, 14), new ZDate(2007, 10, 15), "PT1", "PT2", ZString.Empty, ZString.Empty);
		Helper.CreateWhsReceiveInventoryLine(receive1, Product2.Parent, 10m, new ZDate(2007, 10, 16), new ZDate(2007, 10, 17), "PT1", ZString.Empty, ZString.Empty, ZString.Empty);
		Helper.CreateWhsReceiveInventoryLine(receive1, Product1.Parent, 15m, new ZDate(2007, 10, 14), new ZDate(2007, 10, 15), "PT2", ZString.Empty, ZString.Empty, ZString.Empty);

		var receive2 = Helper.CreateWhsReceive(Client, Whs, "IN2");
		Helper.CreateWhsReceiveInventoryLine(receive2, Product1.Parent, 5m, new ZDate(2007, 10, 14), new ZDate(2007, 10, 15), "PT1", "PT2", ZString.Empty, ZString.Empty);
		var inventory = Helper.CreateWhsReceiveInventoryLine(receive2, Product1.Parent, 10m, new ZDate(2007, 10, 14), new ZDate(2007, 10, 15), "PT1", "PT2", "PT3", ZString.Empty);
		Helper.SetInventoryCustomAttributes(inventory, "CA1", "CA2", "CA3", "CA4", "CA5", "CA6", 0m, 10000.1m, 23m, -0.01m, 23456m, new ZDateTime(2008, 04, 01), new ZDate(2008, 04, 02), new ZDateTime(2008, 04, 03), new ZDateTime(2008, 04, 04), new ZDateTime(2008, 04, 05), true, true, false, false, true, "BLOB1");
		Helper.CreateWhsReceiveInventoryLine(receive2, Product2.Parent, 10m, new ZDate(2007, 10, 16), new ZDate(2007, 10, 17), "PT2", ZString.Empty, ZString.Empty, ZString.Empty);

		var locations = Whs.Rows.Single(r => r.WR_Name == "WH1").Locations;
		receive1.Inventory[0].WI_WL = locations[0].PK;
		receive1.Inventory[1].WI_WL = locations[1].PK;
		receive1.Inventory[2].WI_WL = locations[2].PK;
		receive1.Inventory[3].WI_WL = locations[3].PK;

		receive2.Inventory[0].WI_WL = locations[4].PK;
		receive2.Inventory[1].WI_WL = locations[5].PK;
		receive2.Inventory[2].WI_WL = locations[6].PK;

		foreach (var location in locations)
		{
			location.WLV_WA_PickingArea = Whs.Areas[0].PK;
		}

		receive1.NotificationManager.Push(new NotificationBufferWithDefaultResponse(true));
		receive1.FinaliseDocket();
		WhsTestCaseWithFactory.AssertIsFinalisedPrecondition(receive1);

		receive2.NotificationManager.Push(new NotificationBufferWithDefaultResponse(true));
		receive2.FinaliseDocket();
		WhsTestCaseWithFactory.AssertIsFinalisedPrecondition(receive2);
		Factory.Save();

		var pick = Helper.CreatePickByAttachingOrders(whsDocket);
		whsDocket.WD_DropMode = whsDocket.Lookups.DropModes[0].Code;

		// 'Re-finalise' the order as the test previously expected
		whsDocket.WD_FinalisedDate = previousFinalisedDate;
		Factory.Save();

		whsDocket.FinaliseDocket();
		AssertEquals("Order should be finalized", true, whsDocket.IsFinalised);

		whsDocket.WD_ShipperCODAmount = 100m;
		whsDocket.WD_CODPayMethod = whsDocket.Lookups.ShipperCODPaymentTypes[0].Code;
		whsDocket.WD_LocalCartInsuranceCost = 200m;
		whsDocket.CalculateTotalsEnabled = false;
		whsDocket.WD_WeightSentUserEntered = 40m;
		whsDocket.WD_TotalWeightUnit = Core.Constants.Weight.Kilograms;
		whsDocket.WD_CubicSent = 50m;
		whsDocket.WD_TotalCubicUnit = Core.Constants.Volume.CubicMetres;
		whsDocket.CalculateTotalsEnabled = true;
	}

	void SetClientAttributesType(OrgHeader client)
	{
		Helper.SetClientAttributeType(client, AttributeNumber.One, false);
		Helper.SetClientAttributeType(client, AttributeNumber.Two, false);
		Helper.SetClientAttributeType(client, AttributeNumber.Three, false);
		Helper.SetClientAttributeType(client, AttributeNumber.ExpiryDate, true);
		Helper.SetClientAttributeType(client, AttributeNumber.PackingDate, true);
	}

	void SetProductAttributesUse(OrgHeader owner, OrgSupplierPart part)
	{
		Helper.SetProductAttributeUse(owner, part, AttributeNumber.One, true);
		Helper.SetProductAttributeUse(owner, part, AttributeNumber.Two, true);
		Helper.SetProductAttributeUse(owner, part, AttributeNumber.Three, true);
		Helper.SetProductAttributeUse(owner, part, AttributeNumber.ExpiryDate, true);
		Helper.SetProductAttributeUse(owner, part, AttributeNumber.PackingDate, true);
	}

	protected override int ExpectedNumberOfEvents => 8;

	protected override void AssertExportedXsdDocket(Xsd.WhsDocket xsdDocket, WhsOrder order)
	{
		base.AssertExportedXsdDocket(xsdDocket, order);
		AssertEquals(100m, xsdDocket.DocketDetail.ShipperCODAmount);
		AssertEquals(order.Lookups.ShipperCODPaymentTypes[0].Code, xsdDocket.DocketDetail.ShipperCODType);
		AssertEquals(200m, xsdDocket.DocketDetail.TransportInsurance);
		AssertEquals(40m, xsdDocket.DocketDetail.Weight.Value);
		AssertEquals(Core.Constants.Weight.Kilograms, xsdDocket.DocketDetail.Weight.DimensionType);
		AssertEquals(50m, xsdDocket.DocketDetail.Cubic.Value);
		AssertEquals(Core.Constants.Volume.CubicMetres, xsdDocket.DocketDetail.Cubic.DimensionType);
	}

	protected override void AssertExportXsdDocketLines(Xsd.WhsDocket xsdDocket)
	{
		AssertEquals(4, xsdDocket.DocketLines.Count);

		#region Line 1

		var xsdDocketLine = GetXsdDocketLineByLineNo(xsdDocket.DocketLines, 1);

		AssertEquals("Description1", xsdDocketLine.Description);
		AssertEquals("Line Coments1", xsdDocketLine.LineComments);
		AssertEquals(new ZShort(1), xsdDocketLine.LineNumber);
		AssertEquals("P1", xsdDocketLine.Product);
		AssertEquals("PQ1", xsdDocketLine.ProductUQ);
		AssertEquals(10m, xsdDocketLine.QuantityActuallyOrdered);
		AssertEquals(10m, xsdDocketLine.QuantityFromClientOrder);
		AssertEquals(new ZShort(1), xsdDocketLine.SubLineNumber);

		AssertEquals("EK1", xsdDocketLine.LineAttributes.BondedEntryKey);
		AssertEquals("CA11", xsdDocketLine.LineAttributes.CustomAttribute1);
		AssertEquals("CA12", xsdDocketLine.LineAttributes.CustomAttribute2);
		AssertEquals("CA13", xsdDocketLine.LineAttributes.CustomAttribute3);
		AssertEquals("CA14", xsdDocketLine.LineAttributes.CustomAttribute4);
		AssertEquals("CA15", xsdDocketLine.LineAttributes.CustomAttribute5);
		AssertEquals("CA16", xsdDocketLine.LineAttributes.CustomAttribute6);

		AssertEquals(new ZDateTime(2008, 01, 01), xsdDocketLine.LineAttributes.CustomDate1);
		AssertEquals(new ZDateTime(2008, 01, 02), xsdDocketLine.LineAttributes.CustomDate2);
		AssertEquals(new ZDateTime(2008, 01, 03), xsdDocketLine.LineAttributes.CustomDate3);
		AssertEquals(new ZDateTime(2008, 01, 04), xsdDocketLine.LineAttributes.CustomDate4);
		AssertEquals(new ZDateTime(2008, 01, 05), xsdDocketLine.LineAttributes.CustomDate5);

		AssertEquals(1.23m, xsdDocketLine.LineAttributes.CustomDecimal1);
		AssertEquals(78m, xsdDocketLine.LineAttributes.CustomDecimal2);
		AssertEquals(0.008m, xsdDocketLine.LineAttributes.CustomDecimal3);
		AssertEquals(123456m, xsdDocketLine.LineAttributes.CustomDecimal4);
		AssertEquals(100m, xsdDocketLine.LineAttributes.CustomDecimal5);

		AssertEquals(false, xsdDocketLine.LineAttributes.CustomFlag1);
		AssertEquals(true, xsdDocketLine.LineAttributes.CustomFlag2);
		AssertEquals(true, xsdDocketLine.LineAttributes.CustomFlag3);
		AssertEquals(false, xsdDocketLine.LineAttributes.CustomFlag4);
		AssertEquals(false, xsdDocketLine.LineAttributes.CustomFlag5);

		AssertEquals(new ZDate(2007, 10, 12), xsdDocketLine.LineAttributes.ExpiryDate);
		AssertEquals(new ZDate(2007, 11, 12), xsdDocketLine.LineAttributes.PackingDate);
		AssertEquals("PA11", xsdDocketLine.LineAttributes.PartAttribute1);
		AssertEquals("PA12", xsdDocketLine.LineAttributes.PartAttribute2);
		AssertEquals("PA13", xsdDocketLine.LineAttributes.PartAttribute3);

		AssertEquals("AddInfo1", xsdDocketLine.CustomsData.AddInfo);
		AssertEquals(10m, xsdDocketLine.CustomsData.BondedWhsQuantity);
		AssertEquals("U1", xsdDocketLine.CustomsData.BondedWhsQuantityUnit);
		AssertEquals(11m, xsdDocketLine.CustomsData.CustomsQuantity);
		AssertEquals("C1", xsdDocketLine.CustomsData.CustomsQuantityUnit);
		AssertEquals("DecRef1", xsdDocketLine.CustomsData.DeclarationReference);
		AssertEquals(new ZDateTime(2007, 10, 10), xsdDocketLine.CustomsData.EntryDate);
		AssertEquals("BEK1", xsdDocketLine.CustomsData.EntryKey);
		AssertEquals(new ZShort(1), xsdDocketLine.CustomsData.EntryLineNumber);
		AssertEquals("US", xsdDocketLine.CustomsData.CountryOfOrigin);
		AssertEquals("USD", xsdDocketLine.CustomsData.TILVCurrency);
		AssertEquals(100m, xsdDocketLine.CustomsData.TILVAmount);
		AssertEquals(110m, xsdDocketLine.CustomsData.ValueForDuty);

		#endregion

		#region Line 2

		xsdDocketLine = GetXsdDocketLineByLineNo(xsdDocket.DocketLines, 2);

		AssertEquals("Description2", xsdDocketLine.Description);
		AssertEquals("Line Coments2", xsdDocketLine.LineComments);
		AssertEquals(new ZShort(2), xsdDocketLine.LineNumber);
		AssertEquals("P2", xsdDocketLine.Product);
		AssertEquals("PQ2", xsdDocketLine.ProductUQ);
		AssertEquals(20m, xsdDocketLine.QuantityActuallyOrdered);
		AssertEquals(20m, xsdDocketLine.QuantityFromClientOrder);
		AssertEquals(new ZShort(2), xsdDocketLine.SubLineNumber);

		AssertEquals("EK2", xsdDocketLine.LineAttributes.BondedEntryKey);
		AssertEquals("CA21", xsdDocketLine.LineAttributes.CustomAttribute1);
		AssertEquals("CA22", xsdDocketLine.LineAttributes.CustomAttribute2);
		AssertEquals("CA23", xsdDocketLine.LineAttributes.CustomAttribute3);
		AssertEquals("CA24", xsdDocketLine.LineAttributes.CustomAttribute4);
		AssertEquals("CA25", xsdDocketLine.LineAttributes.CustomAttribute5);
		AssertEquals("CA26", xsdDocketLine.LineAttributes.CustomAttribute6);

		AssertEquals(new ZDateTime(2008, 02, 01), xsdDocketLine.LineAttributes.CustomDate1);
		AssertEquals(new ZDateTime(2008, 02, 02), xsdDocketLine.LineAttributes.CustomDate2);
		AssertEquals(new ZDateTime(2008, 02, 03), xsdDocketLine.LineAttributes.CustomDate3);
		AssertEquals(new ZDateTime(2008, 02, 04), xsdDocketLine.LineAttributes.CustomDate4);
		AssertEquals(new ZDateTime(2008, 02, 05), xsdDocketLine.LineAttributes.CustomDate5);

		AssertEquals(0.0m, xsdDocketLine.LineAttributes.CustomDecimal1);
		AssertEquals(9000.1m, xsdDocketLine.LineAttributes.CustomDecimal2);
		AssertEquals(-75444.9m, xsdDocketLine.LineAttributes.CustomDecimal3);
		AssertEquals(23m, xsdDocketLine.LineAttributes.CustomDecimal4);
		AssertEquals(900m, xsdDocketLine.LineAttributes.CustomDecimal5);

		AssertEquals(true, xsdDocketLine.LineAttributes.CustomFlag1);
		AssertEquals(true, xsdDocketLine.LineAttributes.CustomFlag2);
		AssertEquals(true, xsdDocketLine.LineAttributes.CustomFlag3);
		AssertEquals(false, xsdDocketLine.LineAttributes.CustomFlag4);
		AssertEquals(true, xsdDocketLine.LineAttributes.CustomFlag5);

		AssertEquals(new ZDate(2007, 10, 15), xsdDocketLine.LineAttributes.ExpiryDate);
		AssertEquals(new ZDate(2007, 11, 15), xsdDocketLine.LineAttributes.PackingDate);
		AssertEquals("PA21", xsdDocketLine.LineAttributes.PartAttribute1);
		AssertEquals("PA22", xsdDocketLine.LineAttributes.PartAttribute2);
		AssertEquals("PA23", xsdDocketLine.LineAttributes.PartAttribute3);

		AssertEquals("AddInfo2", xsdDocketLine.CustomsData.AddInfo);
		AssertEquals(20m, xsdDocketLine.CustomsData.BondedWhsQuantity);
		AssertEquals("U2", xsdDocketLine.CustomsData.BondedWhsQuantityUnit);
		AssertEquals(22m, xsdDocketLine.CustomsData.CustomsQuantity);
		AssertEquals("C2", xsdDocketLine.CustomsData.CustomsQuantityUnit);
		AssertEquals("DecRef2", xsdDocketLine.CustomsData.DeclarationReference);
		AssertEquals(new ZDateTime(2007, 10, 20), xsdDocketLine.CustomsData.EntryDate);
		AssertEquals("BEK2", xsdDocketLine.CustomsData.EntryKey);
		AssertEquals(new ZShort(2), xsdDocketLine.CustomsData.EntryLineNumber);
		AssertEquals("AU", xsdDocketLine.CustomsData.CountryOfOrigin);
		AssertEquals("AUD", xsdDocketLine.CustomsData.TILVCurrency);
		AssertEquals(200m, xsdDocketLine.CustomsData.TILVAmount);
		AssertEquals(220m, xsdDocketLine.CustomsData.ValueForDuty);

		#endregion

		#region Line 3

		xsdDocketLine = GetXsdDocketLineByLineNo(xsdDocket.DocketLines, 3);

		AssertEquals("Description1", xsdDocketLine.Description);
		AssertEquals("Line Coments3", xsdDocketLine.LineComments);
		AssertEquals(new ZShort(3), xsdDocketLine.LineNumber);
		AssertEquals("P1", xsdDocketLine.Product);
		AssertEquals("PQ3", xsdDocketLine.ProductUQ);
		AssertEquals(30m, xsdDocketLine.QuantityActuallyOrdered);
		AssertEquals(30m, xsdDocketLine.QuantityFromClientOrder);
		AssertEquals(ZShort.Zero, xsdDocketLine.SubLineNumber);

		AssertEquals(ZString.Empty, xsdDocketLine.LineAttributes.BondedEntryKey);
		AssertEquals(ZString.Empty, xsdDocketLine.LineAttributes.CustomAttribute1);
		AssertEquals(ZString.Empty, xsdDocketLine.LineAttributes.CustomAttribute2);
		AssertEquals(ZString.Empty, xsdDocketLine.LineAttributes.CustomAttribute3);
		AssertEquals(ZString.Empty, xsdDocketLine.LineAttributes.CustomAttribute4);
		AssertEquals(ZString.Empty, xsdDocketLine.LineAttributes.CustomAttribute5);
		AssertEquals(ZString.Empty, xsdDocketLine.LineAttributes.CustomAttribute6);

		AssertEquals(ZDateTime.Empty, xsdDocketLine.LineAttributes.CustomDate1);
		AssertEquals(ZDateTime.Empty, xsdDocketLine.LineAttributes.CustomDate2);
		AssertEquals(ZDateTime.Empty, xsdDocketLine.LineAttributes.CustomDate3);
		AssertEquals(ZDateTime.Empty, xsdDocketLine.LineAttributes.CustomDate4);
		AssertEquals(ZDateTime.Empty, xsdDocketLine.LineAttributes.CustomDate5);

		AssertEquals(ZDecimal.Zero, xsdDocketLine.LineAttributes.CustomDecimal1);
		AssertEquals(ZDecimal.Zero, xsdDocketLine.LineAttributes.CustomDecimal2);
		AssertEquals(ZDecimal.Zero, xsdDocketLine.LineAttributes.CustomDecimal3);
		AssertEquals(ZDecimal.Zero, xsdDocketLine.LineAttributes.CustomDecimal4);
		AssertEquals(ZDecimal.Zero, xsdDocketLine.LineAttributes.CustomDecimal5);

		AssertEquals(false, xsdDocketLine.LineAttributes.CustomFlag1);
		AssertEquals(false, xsdDocketLine.LineAttributes.CustomFlag2);
		AssertEquals(false, xsdDocketLine.LineAttributes.CustomFlag3);
		AssertEquals(false, xsdDocketLine.LineAttributes.CustomFlag4);
		AssertEquals(false, xsdDocketLine.LineAttributes.CustomFlag5);

		AssertEquals(new ZDate(2007, 10, 14), xsdDocketLine.LineAttributes.ExpiryDate);
		AssertEquals(new ZDate(2007, 10, 15), xsdDocketLine.LineAttributes.PackingDate);
		AssertEquals("PT1", xsdDocketLine.LineAttributes.PartAttribute1);
		AssertEquals(ZString.Empty, xsdDocketLine.LineAttributes.PartAttribute2);
		AssertEquals(ZString.Empty, xsdDocketLine.LineAttributes.PartAttribute3);

		#region Confirmation

		AssertEquals(30m, xsdDocketLine.Confirmation.Quantity);
		AssertEquals(4, xsdDocketLine.Confirmation.Lines.Count);

		var xsdConfirmationLine = GetXsdDocketLineConfirmationLineByAttributes(xsdDocketLine.Confirmation.Lines, "PT1", "PT2", "PT3", 10m);
		AssertConfirmationLine(xsdConfirmationLine, "CA1", "CA2", "CA3", "CA4", "CA5", "CA6", new ZDateTime(2008, 04, 01), new ZDateTime(2008, 04, 02), new ZDateTime(2008, 04, 03), new ZDateTime(2008, 04, 04), new ZDateTime(2008, 04, 05), 0m, 10000.1m, 23m, -0.01m, 23456m, true, true, false, false, true, "PT1", "PT2", "PT3", new ZDate(2007, 10, 15), new ZDate(2007, 10, 14), 10m, "UNT");

		xsdConfirmationLine = GetXsdDocketLineConfirmationLineByAttributes(xsdDocketLine.Confirmation.Lines, "PT1", "PT2", ZString.Empty, 10m);
		AssertConfirmationLine(xsdConfirmationLine, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZDate.Empty, ZDate.Empty, ZDate.Empty, ZDate.Empty, ZDateTime.Empty, ZDecimal.Zero, ZDecimal.Zero, ZDecimal.Zero, ZDecimal.Zero, ZDecimal.Zero, ZBool.False, ZBool.False, ZBool.False, ZBool.False, ZBool.False, "PT1", "PT2", ZString.Empty, new ZDate(2007, 10, 15), new ZDate(2007, 10, 14), 10m, "UNT");

		xsdConfirmationLine = GetXsdDocketLineConfirmationLineByAttributes(xsdDocketLine.Confirmation.Lines, "PT1", "PT2", ZString.Empty, 5m);
		AssertConfirmationLine(xsdConfirmationLine, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZDate.Empty, ZDate.Empty, ZDate.Empty, ZDate.Empty, ZDateTime.Empty, ZDecimal.Zero, ZDecimal.Zero, ZDecimal.Zero, ZDecimal.Zero, ZDecimal.Zero, ZBool.False, ZBool.False, ZBool.False, ZBool.False, ZBool.False, "PT1", "PT2", ZString.Empty, new ZDate(2007, 10, 15), new ZDate(2007, 10, 14), 5m, "UNT");

		xsdConfirmationLine = GetXsdDocketLineConfirmationLineByAttributes(xsdDocketLine.Confirmation.Lines, "PT1", ZString.Empty, ZString.Empty, 5m);
		AssertConfirmationLine(xsdConfirmationLine, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZDate.Empty, ZDate.Empty, ZDate.Empty, ZDate.Empty, ZDateTime.Empty, ZDecimal.Zero, ZDecimal.Zero, ZDecimal.Zero, ZDecimal.Zero, ZDecimal.Zero, ZBool.False, ZBool.False, ZBool.False, ZBool.False, ZBool.False, "PT1", ZString.Empty, ZString.Empty, new ZDate(2007, 10, 15), new ZDate(2007, 10, 14), 5m, "UNT");

		#endregion

		#endregion

		#region Line4

		xsdDocketLine = GetXsdDocketLineByLineNo(xsdDocket.DocketLines, 4);

		AssertEquals("Description2", xsdDocketLine.Description);
		AssertEquals("Line Coments4", xsdDocketLine.LineComments);
		AssertEquals(new ZShort(4), xsdDocketLine.LineNumber);
		AssertEquals("P2", xsdDocketLine.Product);
		AssertEquals("PQ4", xsdDocketLine.ProductUQ);
		AssertEquals(40m, xsdDocketLine.QuantityActuallyOrdered);
		AssertEquals(40m, xsdDocketLine.QuantityFromClientOrder);
		AssertEquals(ZShort.Zero, xsdDocketLine.SubLineNumber);

		AssertEquals(ZString.Empty, xsdDocketLine.LineAttributes.BondedEntryKey);
		AssertEquals(ZString.Empty, xsdDocketLine.LineAttributes.CustomAttribute1);
		AssertEquals(ZString.Empty, xsdDocketLine.LineAttributes.CustomAttribute2);
		AssertEquals(ZString.Empty, xsdDocketLine.LineAttributes.CustomAttribute3);
		AssertEquals(ZString.Empty, xsdDocketLine.LineAttributes.CustomAttribute4);
		AssertEquals(ZString.Empty, xsdDocketLine.LineAttributes.CustomAttribute5);
		AssertEquals(ZString.Empty, xsdDocketLine.LineAttributes.CustomAttribute6);

		AssertEquals(ZDateTime.Empty, xsdDocketLine.LineAttributes.CustomDate1);
		AssertEquals(ZDateTime.Empty, xsdDocketLine.LineAttributes.CustomDate2);
		AssertEquals(ZDateTime.Empty, xsdDocketLine.LineAttributes.CustomDate3);
		AssertEquals(ZDateTime.Empty, xsdDocketLine.LineAttributes.CustomDate4);
		AssertEquals(ZDateTime.Empty, xsdDocketLine.LineAttributes.CustomDate5);

		AssertEquals(ZDecimal.Zero, xsdDocketLine.LineAttributes.CustomDecimal1);
		AssertEquals(ZDecimal.Zero, xsdDocketLine.LineAttributes.CustomDecimal2);
		AssertEquals(ZDecimal.Zero, xsdDocketLine.LineAttributes.CustomDecimal3);
		AssertEquals(ZDecimal.Zero, xsdDocketLine.LineAttributes.CustomDecimal4);
		AssertEquals(ZDecimal.Zero, xsdDocketLine.LineAttributes.CustomDecimal5);

		AssertEquals(false, xsdDocketLine.LineAttributes.CustomFlag1);
		AssertEquals(false, xsdDocketLine.LineAttributes.CustomFlag2);
		AssertEquals(false, xsdDocketLine.LineAttributes.CustomFlag3);
		AssertEquals(false, xsdDocketLine.LineAttributes.CustomFlag4);
		AssertEquals(false, xsdDocketLine.LineAttributes.CustomFlag5);

		AssertEquals(new ZDate(2007, 10, 16), xsdDocketLine.LineAttributes.ExpiryDate);
		AssertEquals(new ZDate(2007, 10, 17), xsdDocketLine.LineAttributes.PackingDate);
		AssertEquals(ZString.Empty, xsdDocketLine.LineAttributes.PartAttribute1);
		AssertEquals(ZString.Empty, xsdDocketLine.LineAttributes.PartAttribute2);
		AssertEquals(ZString.Empty, xsdDocketLine.LineAttributes.PartAttribute3);

		#region Confirmation

		AssertEquals(20m, xsdDocketLine.Confirmation.Quantity);
		AssertEquals(2, xsdDocketLine.Confirmation.Lines.Count);

		xsdConfirmationLine = GetXsdDocketLineConfirmationLineByAttributes(xsdDocketLine.Confirmation.Lines, "PT1", ZString.Empty, ZString.Empty, 10m);
		AssertConfirmationLine(xsdConfirmationLine, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZDate.Empty, ZDate.Empty, ZDate.Empty, ZDate.Empty, ZDateTime.Empty, ZDecimal.Zero, ZDecimal.Zero, ZDecimal.Zero, ZDecimal.Zero, ZDecimal.Zero, ZBool.False, ZBool.False, ZBool.False, ZBool.False, ZBool.False, "PT1", ZString.Empty, ZString.Empty, new ZDate(2007, 10, 17), new ZDate(2007, 10, 16), 10m, "UNT");

		xsdConfirmationLine = GetXsdDocketLineConfirmationLineByAttributes(xsdDocketLine.Confirmation.Lines, "PT2", ZString.Empty, ZString.Empty, 10m);
		AssertConfirmationLine(xsdConfirmationLine, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZDate.Empty, ZDate.Empty, ZDate.Empty, ZDate.Empty, ZDateTime.Empty, ZDecimal.Zero, ZDecimal.Zero, ZDecimal.Zero, ZDecimal.Zero, ZDecimal.Zero, ZBool.False, ZBool.False, ZBool.False, ZBool.False, ZBool.False, "PT2", ZString.Empty, ZString.Empty, new ZDate(2007, 10, 17), new ZDate(2007, 10, 16), 10m, "UNT");

		#endregion

		#endregion
	}

	Xsd.WhsDocketLine GetXsdDocketLineByLineNo(Xsd.WhsDocketLineCollection xsdDocketLines, int lineNo)
	{
		var result = new Xsd.WhsDocketLine();

		foreach (Xsd.WhsDocketLine line in xsdDocketLines)
		{
			if (line.LineNumber == lineNo)
			{
				result = line;
				break;
			}
		}
		return result;
	}

	Xsd.WhsDocketLineConfirmationLine GetXsdDocketLineConfirmationLineByAttributes(Xsd.WhsDocketLineConfirmationLineCollection xsdConfirmationLines, ZString pA1, ZString pA2, ZString pA3, ZDecimal quantity)
	{
		var result = new Xsd.WhsDocketLineConfirmationLine();

		foreach (Xsd.WhsDocketLineConfirmationLine line in xsdConfirmationLines)
		{
			if (line.PartAttribute1 == pA1 && line.PartAttribute2 == pA2 && line.PartAttribute3 == pA3 && line.Quantity == quantity)
			{
				result = line;
				break;
			}
		}
		return result;
	}

	#endregion

	void AssertConfirmationLine(Xsd.WhsDocketLineConfirmationLine line, ZString cA1, ZString cA2, ZString cA3, ZString cA4, ZString cA5, ZString cA6, ZDateTime cDate1, ZDateTime cDate2, ZDateTime cDate3, ZDateTime cDate4, ZDateTime cDate5, ZDecimal cDecimal1, ZDecimal cDecimal2, ZDecimal cDecimal3, ZDecimal cDecimal4, ZDecimal cDecimal5, ZBool cFlag1, ZBool cFlag2, ZBool cFlag3, ZBool cFlag4, ZBool cFlag5, ZString pA1, ZString pA2, ZString pA3, ZDate packingDate, ZDate expiryDate, ZDecimal quantity, ZString uQ)
	{
		AssertEquals(cA1, line.CustomAttribute1);
		AssertEquals(cA2, line.CustomAttribute2);
		AssertEquals(cA3, line.CustomAttribute3);
		AssertEquals(cA4, line.CustomAttribute4);
		AssertEquals(cA5, line.CustomAttribute5);
		AssertEquals(cA6, line.CustomAttribute6);
		AssertEquals(cDate1, line.CustomDate1);
		AssertEquals(cDate2, line.CustomDate2);
		AssertEquals(cDate3, line.CustomDate3);
		AssertEquals(cDate4, line.CustomDate4);
		AssertEquals(cDate5, line.CustomDate5);
		AssertEquals(cDecimal1, line.CustomDecimal1);
		AssertEquals(cDecimal2, line.CustomDecimal2);
		AssertEquals(cDecimal3, line.CustomDecimal3);
		AssertEquals(cDecimal4, line.CustomDecimal4);
		AssertEquals(cDecimal5, line.CustomDecimal5);
		AssertEquals(cFlag1, line.CustomFlag1);
		AssertEquals(cFlag2, line.CustomFlag2);
		AssertEquals(cFlag3, line.CustomFlag3);
		AssertEquals(cFlag4, line.CustomFlag4);
		AssertEquals(cFlag5, line.CustomFlag5);
		AssertEquals(expiryDate, line.ExpiryDate);
		AssertEquals(packingDate, line.PackingDate);
		AssertEquals(pA1, line.PartAttribute1);
		AssertEquals(pA2, line.PartAttribute2);
		AssertEquals(pA3, line.PartAttribute3);
		AssertEquals(quantity, line.Quantity);
		AssertEquals(uQ, line.QuantityUQ);
	}

	protected override WhsDocketValueObjectDataAdapter<WhsOrder> GetDataAdapter()
	{
		return new WhsOrderConfirmationValueObjectDataAdapter();
	}

	protected override Xsd.WhsDocketIdentifierActionType GetExpectedActionType()
	{
		return Xsd.WhsDocketIdentifierActionType.CON;
	}

	protected override Type GetDataAdapterType()
	{
		return typeof(WhsOrderConfirmationValueObjectDataAdapter);
	}

	protected override bool IsImportFromValueObjectSupported => false;

	protected override BusinessObjectAndExpectedOutputFileName GetEmptyBizObjSample()
	{
		var emptyWhsOrderConfirmationPath = resourceRetriever.Value.SaveResourceToFile("Enterprise.Warehouse.Transactions.GUI.Testing.TestFiles.EmptyWhsOrderConfirmation.xml");
		return new BusinessObjectAndExpectedOutputFileName(GetEmptyWhsDocket(), emptyWhsOrderConfirmationPath, ValidationKind.None, "Empty WhsOrder Confirmation");
	}

	protected override BusinessObjectAndExpectedOutputFileName GetFullyPopulatedBizObjSample()
	{
		var populatedWhsOrderConfirmationPath = resourceRetriever.Value.SaveResourceToFile("Enterprise.Warehouse.Transactions.GUI.Testing.TestFiles.PopulatedWhsOrderConfirmation.xml");
		return new BusinessObjectAndExpectedOutputFileName(GetFullyPopulatedWhsDocket(), populatedWhsOrderConfirmationPath, ValidationKind.None, "Populated WhsOrder Confirmation");
	}

	#endregion

	#region TestExportOrderWeightAndCubic

	public void TestExportOrderWeightAndCubic()
	{
		var data = new TestDataSimpleEnvironment(Factory);
		data.Part1.OP_Cubic = 0.5m;
		data.Part1.OP_Weight = 0.1m;

		var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);

		AssertEquals("Total Lines Volume", 5m, order.WD_TotalCubic);
		AssertEquals("Total Lines Weight", 1m, order.WD_TotalWeight);

		order.WD_CubicSent = 3m;
		order.WD_WeightSent = 0.5m;

		var notify = new NotificationBuffer();
		var context = new ValueObjectExportContext(notify);
		var whsValue = Adapter.ExportToValueObject(order, context);
		AssertEquals("Docket weight ", order.WD_WeightSent, whsValue.DocketDetail.Weight.Value);
		AssertEquals("Docket volume ", order.WD_CubicSent, whsValue.DocketDetail.Cubic.Value);

		order.WD_CubicSent = 0m;
		order.WD_WeightSent = 0m;

		whsValue = Adapter.ExportToValueObject(order, context);
		AssertEquals("Docket weight ", order.WD_TotalWeight, whsValue.DocketDetail.Weight.Value);
		AssertEquals("Docket volume ", order.WD_TotalCubic, whsValue.DocketDetail.Cubic.Value);
	}

	#endregion

	#region TestExportOrderConfirmationExportsReleaseCapturedAttributes

	public void TestExportOrderConfirmationExportsReleaseCapturedAttributes()
	{
		var data = new TestDataSimpleEnvironment(Factory);
		Factory.Save();

		Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, PartAttributeTypeList.Codes.VIN);
		Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true, setReleaseCaptured: true);
		Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 3m);

		Factory.Save();

		var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 3m);
		var pick = Helper.CreatePickNew(order);
		var orderLine = order.Lines[0];
		var releaseLine1 = orderLine.ReleaseLines[0];
		releaseLine1.SetupReleaseLine("100", "", "", ZDate.Empty, ZDate.Empty, "");
		releaseLine1.Quantity = 1m;

		var releaseLine2 = orderLine.ReleaseLines.AddNew();
		releaseLine2.SetupReleaseLine("120", "", "", ZDate.Empty, ZDate.Empty, "");
		releaseLine2.Quantity = 1m;

		var releaseLine3 = orderLine.ReleaseLines.AddNew();
		releaseLine3.SetupReleaseLine("140", "", "", ZDate.Empty, ZDate.Empty, "");
		releaseLine3.Quantity = 1m;

		var releaseLine4 = orderLine.ReleaseLines.AddNew();
		releaseLine4.SetupReleaseLine("", "", "", ZDate.Empty, ZDate.Empty, "SN3");
		releaseLine4.Quantity = 1m;
		var notify = new NotificationBuffer();
		var context = new ValueObjectExportContext(notify);
		var exportedDocket = Adapter.ExportToValueObject(order, context);
		AssertEquals(1, exportedDocket.DocketLines.Count);

		var exportedDocketLine = exportedDocket.DocketLines[0];
		AssertEquals(4m, exportedDocketLine.Confirmation.Quantity);
		AssertEquals(4, exportedDocketLine.Confirmation.Lines.Count);

		exportedDocketLine.Confirmation.Lines.Cast<Xsd.WhsDocketLineConfirmationLine>().Single(o => o.PartAttribute1 == "100" && o.Quantity == 1m);
		exportedDocketLine.Confirmation.Lines.Cast<Xsd.WhsDocketLineConfirmationLine>().Single(o => o.PartAttribute1 == "120" && o.Quantity == 1m);
		exportedDocketLine.Confirmation.Lines.Cast<Xsd.WhsDocketLineConfirmationLine>().Single(o => o.PartAttribute1 == "140" && o.Quantity == 1m);
	}

	#endregion

	#region TestExportOrderConfirmation_ExportsOriginalInventoryLineCustomsAttribs

	public void TestExportOrderConfirmation_ExportsOriginalInventoryLineCustomsAttribs()
	{
		var data = new TestDataSimpleEnvironment(Factory);
		SetCustomDocketLineAttributesForFlagsAndDecimals(data.Org1);

		var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
		var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m);
		Helper.SetDocketLineCustomAttributes(receiveLine, "CA1", "CA2", "CA3", "CA4", "CA5", "CA6", 1m, 2m, 3m, 4m, 5m,
			new ZDateTime(2017, 1, 1), new ZDateTime(2017, 1, 2), new ZDateTime(2017, 1, 3), new ZDateTime(2017, 1, 4), new ZDateTime(2017, 1, 5), true, true, true, true, true, "");
		receive.AllocateLocationsWithMock();
		receive.FinaliseDocketWithoutUserConfirmation();
		WhsTestCaseWithFactory.AssertIsFinalisedPrecondition(receive);
		Factory.Save();

		var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
		var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
		Helper.CreatePickNew(order);
		var inTransitLine = Helper.PickAndMakeInTransitTransfer(order.Lines[0].PickLines.Single(), ZDateTimeOffset.Now);
		Helper.SetDocketLineCustomAttributes(inTransitLine, "", "", "", "", "", "", 0m, 0m, 0m, 0m, 0m,
			ZDate.Empty, ZDate.Empty, ZDate.Empty, ZDate.Empty, ZDateTime.Empty, false, false, false, false, false, "");

		var xsdDocket = Adapter.ExportToValueObject(order, new ValueObjectExportContext(Notifications));
		var confirmationLine = xsdDocket.DocketLines[0].Confirmation.Lines[0];
		AssertConfirmationLine(confirmationLine, "CA1", "CA2", "CA3", "CA4", "CA5", "CA6",
			new ZDateTime(2017, 1, 1), new ZDateTime(2017, 1, 2), new ZDateTime(2017, 1, 3), new ZDateTime(2017, 1, 4), new ZDateTime(2017, 1, 5), 1m, 2m, 3m, 4m, 5m, true, true, true, true, true, "", "", "", ZDate.Empty, ZDate.Empty, 10m, "UNT");
	}

	#endregion

	#region TestPickLinesCacheIsClearedBeforeExport

	public void TestPickLinesCacheIsClearedBeforeExport()
	{
		var data = new TestDataSimpleEnvironment(Factory);
		Factory.Save();

		Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "asd", data.Part1, 10);
		var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 3m);
		var orderLine = order.Lines[0];

		Factory.Save();
		AssertEquals(0, orderLine.PickLines.Count);

		var anotherFactory = new BusinessObjectFactory();
		var anotherHelper = new WhsTestHelperFunctions(anotherFactory);
		var orderLoadedFromAnotherFactory = anotherFactory.Load<WhsOrder>(order.PK);
		anotherHelper.CreatePickNew(orderLoadedFromAnotherFactory);
		anotherFactory.Save();
		AssertEquals("Precondition: in other factory we have picklines", 1, orderLoadedFromAnotherFactory.Lines[0].PickLines.Count);

		var notify = new NotificationBuffer();
		var context = new ValueObjectExportContext(notify);
		Adapter.ExportToValueObject(order, context);
		AssertEquals("should be reloaded during export", 1, orderLine.PickLines.Count);
	}

	#endregion
}
