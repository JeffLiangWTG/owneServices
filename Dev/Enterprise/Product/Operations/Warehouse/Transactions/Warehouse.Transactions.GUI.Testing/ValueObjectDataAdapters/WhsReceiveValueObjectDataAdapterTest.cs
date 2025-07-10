using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.DataTransfer.CodeLists;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.Warehouse.Transactions.DataTransfer;
using NUnit.Framework;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Warehouse.Transactions.GUI.Testing;

[TestedType(typeof(WhsReceiveValueObjectDataAdapter))]
class WhsReceiveValueObjectDataAdapterTest : WhsDocketValueObjectDataAdapterTest<WhsReceive>
{
	protected override void SetupCrossDockDataForImport(WhsWarehouse warehouse, OrgHeader client, OrgSupplierPart part)
	{
		base.SetupCrossDockDataForImport(warehouse, client, part);

		var order1 = Helper.CreateWhsOrder(client, warehouse);
		order1.WD_ExternalReference = "CD1";
		var orderLine1 = Helper.CreateWhsOrderLine(order1, part, 10m, 1, 0);
		Helper.CreateWhsOrderLine(order1, part, 20m, 1, 1);

		var order2 = Helper.CreateWhsOrder(client, warehouse);
		order2.WD_ExternalReference = "CD2";
		var orderLine2 = Helper.CreateWhsOrderLine(order2, part, 30m, 1, 0);

		var order3 = Helper.CreateWhsOrder(client, warehouse);
		order3.WD_ExternalReference = "CD3";
		var part1 = Helper.CreateProduct(client, "Part1");
		Helper.CreateWhsOrderLine(order3, part1, 30m, 1, 0);

		var client2 = Helper.CreateClient("CL2");
		Helper.CreateProductClientRelationShip(client2, part);
		var order4 = Helper.CreateWhsOrder(client2, warehouse);
		order4.WD_ExternalReference = "CD4";
		Helper.CreateWhsOrderLine(order4, part, 30m, 1, 0);

		var warehouse2 = Helper.CreateWarehouse("WH2");
		var order5 = Helper.CreateWhsOrder(client, warehouse2);
		order5.WD_ExternalReference = "CD5";
		Helper.CreateWhsOrderLine(order5, part, 30m, 1, 0);

		var receive = Helper.CreateWhsReceive(client, warehouse);
		receive.WD_ExternalReference = "REF";
		var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive, part, 10m, 10, 1);
		Factory.Save();

		Helper.CreateReservePickLine(orderLine1, inventory1, 1m);
		Helper.CreateReservePickLine(orderLine2, inventory1, 9m);
	}

	protected override void AssertImportedCrossDockData(WhsReceive docket)
	{
		base.AssertImportedCrossDockData(docket);

		docket.Lines.ApplySort(WhsDocketLine.Schema.WE_LineNo, System.ComponentModel.ListSortDirection.Ascending);
		var line = docket.Lines[0];
		AssertEquals((short)1, line.WE_LineNo);
		AssertEquals((short)0, line.WE_SubLineNo);
		AssertEquals(1, line.Inventory.Count);
		AssertEquals(0, line.Inventory[0].ReservedPickLines.Count);

		line = docket.Lines[1];
		AssertEquals((short)2, line.WE_LineNo);
		AssertEquals((short)1, line.WE_SubLineNo);
		AssertEquals(1, line.Inventory.Count);
		AssertEquals(1, line.Inventory[0].ReservedPickLines.Count);

		var orderLine = line.Inventory[0].ReservedPickLines[0].DocketLine;
		AssertEquals("CD1", orderLine.Docket.WD_ExternalReference);
		AssertEquals((short)1, orderLine.WE_LineNo);
		AssertEquals((short)0, orderLine.WE_SubLineNo);
		AssertEquals(5m, line.Inventory[0].ReservedPickLines[0].WZ_OriginalReservedQty);
		AssertEquals(5m, line.Inventory[0].ReservedPickLines[0].ReservedQuantity);

		line = docket.Lines[2];
		AssertEquals((short)3, line.WE_LineNo);
		AssertEquals((short)2, line.WE_SubLineNo);
		AssertEquals(1, line.Inventory.Count);
		AssertEquals(1, line.Inventory[0].ReservedPickLines.Count);

		orderLine = line.Inventory[0].ReservedPickLines[0].DocketLine;
		AssertEquals("CD2", orderLine.Docket.WD_ExternalReference);
		AssertEquals((short)1, orderLine.WE_LineNo);
		AssertEquals((short)0, orderLine.WE_SubLineNo);
		AssertEquals(10m, line.Inventory[0].ReservedPickLines[0].WZ_OriginalReservedQty);
		AssertEquals(10m, line.Inventory[0].ReservedPickLines[0].ReservedQuantity);

		line = docket.Lines[3];
		AssertEquals((short)4, line.WE_LineNo);
		AssertEquals((short)0, line.WE_SubLineNo);
		AssertEquals(1, line.Inventory.Count);
		AssertEquals(0, line.Inventory[0].ReservedPickLines.Count);

		line = docket.Lines[4];
		AssertEquals((short)10, line.WE_LineNo);
		AssertEquals((short)1, line.WE_SubLineNo);
		AssertEquals(1, line.Inventory.Count);
		AssertEquals(2, line.Inventory[0].ReservedPickLines.Count);

		orderLine = line.Inventory[0].ReservedPickLines[0].DocketLine;
		AssertEquals("CD1", orderLine.Docket.WD_ExternalReference);
		AssertEquals((short)1, orderLine.WE_LineNo);
		AssertEquals((short)0, orderLine.WE_SubLineNo);
		AssertEquals(1m, line.Inventory[0].ReservedPickLines[0].WZ_OriginalReservedQty);
		AssertEquals(5m, line.Inventory[0].ReservedPickLines[0].ReservedQuantity);

		orderLine = line.Inventory[0].ReservedPickLines[1].DocketLine;
		AssertEquals("CD2", orderLine.Docket.WD_ExternalReference);
		AssertEquals((short)1, orderLine.WE_LineNo);
		AssertEquals((short)0, orderLine.WE_SubLineNo);
		AssertEquals(9m, line.Inventory[0].ReservedPickLines[1].WZ_OriginalReservedQty);
		AssertEquals(9m, line.Inventory[0].ReservedPickLines[1].ReservedQuantity);
	}

	protected override WhsPickLine AddNewCrossDockLinkForExport(WhsDocketLine line, ZString crossDockRef, ZShort lineNo, ZShort subLineNo, ZDecimal qty)
	{
		base.AddNewCrossDockLinkForExport(line, crossDockRef, lineNo, subLineNo, qty);
		var order = Helper.CreateWhsOrder(line.Docket.Client, line.Docket.Warehouse, crossDockRef);
		var orderLine = Helper.CreateWhsOrderLine(order, line.SupplierPart, qty, lineNo, subLineNo);
		if (line.Inventory.Count == 0)
		{
			line.Inventory.AddNew();
		}

		return Helper.CreateReservePickLine(orderLine, line.Inventory[0], qty);
	}

	protected override ZString GetExpectedExportedDocketStatus()
	{
		return DocketStatus.Codes.Putaway;
	}

	protected override void PopulateWhsDocketTotalsForExport(WhsReceive whsDocket)
	{
		base.PopulateWhsDocketTotalsForExport(whsDocket);
		whsDocket.CalculateTotalsEnabled = false;
		whsDocket.WD_TotalUnits = 10;
		whsDocket.WD_TotalPallets = 20;
		whsDocket.CalculateTotalsEnabled = true;
	}

	protected override void PopulateWhsDocketForExport(WhsReceive whsDocket)
	{
		base.PopulateWhsDocketForExport(whsDocket);
		whsDocket.WD_ArrivalDate = new ZDateTimeOffset(2007, 07, 01);
		whsDocket.WD_BookingDate = new ZDateTimeOffset(2007, 07, 02);
		whsDocket.WD_ETA = new ZDateTimeOffset(2007, 07, 03);
		whsDocket.WD_ETD = new ZDateTimeOffset(2007, 07, 04);

		whsDocket.WD_DocketStatus = DocketStatus.Codes.Putaway;

		whsDocket.SupplierDocAddress.OrganisationPK = Supplier.PK;
		whsDocket.SupplierDocAddress.E2_OA_Address = Supplier.MainAddress.PK;
	}

	protected override void AssertExportedXsdDocket(Xsd.WhsDocket xsdDocket, WhsReceive receive)
	{
		base.AssertExportedXsdDocket(xsdDocket, receive);
		AssertEquals(DocketTypes.Codes.WhsASN, xsdDocket.Identifier.DocketType);

		var xsdInwards = (Xsd.WhsCustomerInwardsDetail)xsdDocket.DocketDetail.Item;
		AssertEquals(new ZDateTime(2007, 07, 01), xsdInwards.ArrivalDate);
		AssertEquals(new ZDateTime(2007, 07, 02), xsdInwards.BookingDate);
		AssertEquals(new ZDateTime(2007, 07, 03), xsdInwards.ETA);
		AssertEquals(new ZDateTime(2007, 07, 04), xsdInwards.ETD);

		AssertEquals("spl", xsdInwards.Supplier.AddressReference.Organisation.EDICode);
		AssertEquals("spl", xsdInwards.Supplier.AddressReference.Organisation.OwnerCode);
		AssertEquals("supplier", xsdInwards.Supplier.AddressReference.Organisation.OrganisationDetails.Name);
		AssertEquals("supplier Address1", xsdInwards.Supplier.AddressReference.Organisation.OrganisationDetails.Addresses[0].AddressLine1);
		AssertEquals("supplier Address2", xsdInwards.Supplier.AddressReference.Organisation.OrganisationDetails.Addresses[0].AddressLine2);
		AssertEquals("supplier City", xsdInwards.Supplier.AddressReference.Organisation.OrganisationDetails.Addresses[0].CityOrSuburb);
	}

	protected override WhsReceive GetNewPopulatedDocket(WhsWarehouse whs, OrgHeader client, ZString reference)
	{
		return Helper.CreateWhsReceive(client, whs, reference);
	}

	protected override ZString GetXSDDocketTypeCode()
	{
		return DocketTypes.Codes.WhsASN;
	}

	protected override WhsDocketLine AddNewPopulatedLine(WhsReceive docket, ZShort lineNo, ZShort subLineNo, OrgSupplierPart product, ZDecimal units)
	{
		var inventory = Helper.CreateWhsReceiveInventoryLine(docket, Part, units, lineNo, subLineNo);
		inventory.InDocketLine.ClearInventoryCache();
		return inventory.InDocketLine;
	}

	protected override Type GetExpectedDocketDataFormatterType()
	{
		return typeof(WhsReceiveDataFormatter);
	}

	protected override Type GetExpectedDocketErrorHandlerType()
	{
		return typeof(WhsReceiveErrorHandler);
	}

	protected override ZString GetExternalReference()
	{
		return "INWARDS1";
	}

	protected override ZString GetExpectedMessage()
	{
		return "Found Warehouse Receipt W00000001: INWARDS1 of WHSFORSYD";
	}

	protected override Type GetDataAdapterType()
	{
		return typeof(WhsReceiveValueObjectDataAdapter);
	}

	protected override ValueObjectDataAdapter<WhsReceive, Xsd.WhsDocket> GetNewBizObjXmlDataAdapter()
	{
		var result = (WhsReceiveValueObjectDataAdapter)GetDataAdapter();
		result.FileName = "InwardsFile";
		return result;
	}

	protected override WhsReceive GetEmptyWhsDocket()
	{
		var receive = Factory.New<WhsReceive>();
		receive.WD_ArrivalDate = ZDateTimeOffset.Empty;
		receive.WD_BookingDate = ZDateTimeOffset.Empty;
		receive.WD_DocketStatus = DocketStatus.Codes.Cancelled;
		return receive;
	}

	protected override WhsReceive GetFullyPopulatedWhsDocket()
	{
		var whs = Helper.CreateWarehouse("Warehouse");
		whs.WW_WarehouseCode = "WHS";

		var savingFactory = new BusinessObjectFactory();
		var client = Helper.CreateOrLoadClientInSeperateFactory("WHSCLIENT", "WHSClient for test", savingFactory);
		SetCustomDocketAttributesForFlagsAndDecimals(client);
		savingFactory.Save();

		var receive = Helper.CreateWhsReceive(client, whs);
		receive.WD_ExternalReference = "INWARDS1";
		receive.WD_BookingDate = new ZDateTimeOffset(2007, 08, 15);
		receive.WD_ArrivalDate = new ZDateTimeOffset(2007, 08, 14);
		receive.WD_DocketStatus = DocketStatus.Codes.Cancelled;
		receive.WD_CustomAttrib1 = "Attrib1";
		receive.WD_CustomAttrib2 = "Attrib2";
		receive.WD_CustomAttrib3 = "Attrib3";
		receive.WD_CustomAttrib4 = "Attrib4";
		receive.WD_CustomAttrib5 = "Attrib5";
		receive.WD_CustomDate1 = new ZDateTime(2008, 01, 01);
		receive.WD_CustomDate2 = new ZDateTime(2008, 12, 31);
		receive.WD_CustomDecimal1 = .009m;
		receive.WD_CustomDecimal2 = 1000m;
		receive.WD_CustomDecimal3 = 123.457m;
		receive.WD_CustomDecimal4 = 67m;
		receive.WD_CustomDecimal5 = 5.23m;
		receive.WD_CustomFlag1 = true;
		receive.WD_CustomFlag2 = false;
		receive.WD_CustomFlag3 = true;
		receive.WD_CustomFlag4 = false;
		receive.WD_CustomFlag5 = true;

		PopulateCustomValues(receive);
		return receive;
	}

	protected override void AssertLineInfoQuantity(WhsDocketLine line, ZDecimal units)
	{
		AssertEquals("Line Packs don't match", units, line.WE_ClientOrderedUnits);

		var receiveLine = (WhsReceiveLine)line;
		AssertEquals(1, receiveLine.Inventory.Count);
		AssertEquals(units, receiveLine.Inventory[0].WI_ExpectedReceiptQuantity);
	}

	protected override void AssertQuantity(WhsDocketLine line)
	{
		AssertEquals(200m, line.WE_ClientOrderedUnits);

		var receiveLine = (WhsReceiveLine)line;
		AssertEquals(1, receiveLine.Inventory.Count);
		AssertEquals(200m, receiveLine.Inventory[0].WI_ExpectedReceiptQuantity);
	}

	protected override Xsd.WhsDockets GetNewXsdDockets()
	{
		var xsdDockets = base.GetNewXsdDockets();
		var xsdDocket = xsdDockets.WhsDocket[0];
		xsdDocket.DocketDetail.Item = new Xsd.WhsCustomerInwardsDetail();
		var xsdLine = xsdDocket.DocketLines[0];
		//XsdLine.Item = new Xsd.WhsCustomerOrderLineDetail();
		return xsdDockets;
	}

	protected override void SetUp()
	{
		base.SetUp();
		SetUpSupplier();
	}

	protected virtual void SetUpSupplier()
	{
		Supplier = Helper.CreateClient();
		Supplier.OH_Code = "spl";
		Supplier.OH_FullName = "supplier";
		Supplier.MainAddress.OA_Address1 = "supplier Address1";
		Supplier.MainAddress.OA_Address2 = "supplier Address2";
		Supplier.MainAddress.OA_City = "supplier City";
	}

	protected OrgHeader Supplier;

	protected override WhsDocketValueObjectDataAdapter<WhsReceive> GetDataAdapter()
	{
		return new WhsReceiveValueObjectDataAdapter();
	}

	protected override WhsDocketCollection GetCollection()
	{
		return new WhsReceiveCollection(Factory, new AdhocCollectionRelationship(typeof(WhsReceive)));
	}

	protected override void PopulateXsdDocketAdditonalDetail(Xsd.WhsDocket xsdDocket)
	{
		xsdDocket.DocketDetail.Item = new Xsd.WhsCustomerInwardsDetail();
		var customerReceiveDetail = (Xsd.WhsCustomerInwardsDetail)xsdDocket.DocketDetail.Item;
		PopulateDocAddress(customerReceiveDetail.Supplier, Xsd.DocAddressAddressType.SUD, "Supplier Org", "Supplier Address", "AUSYD");
	}

	protected override void PopulateXsdDocketLineAdditionalDetail(Xsd.WhsDocketLine xsdDocketLine)
	{
	}

	protected override void AssertWhsDocketOtherDetail(WhsReceive whsDocket)
	{
		AssertEquals(11m, whsDocket.WD_TotalUnits);
		AssertEquals(new ZShort(13), whsDocket.WD_TotalPallets);

		AssertNotNull(whsDocket.SupplierDocAddress.Organisation);
		AssertEquals("Supplier Org", whsDocket.SupplierDocAddress.Organisation.OH_FullName);
	}

	protected override void AssertWhsDocketLineOtherDetail(WhsDocketLine whsDocketLine)
	{
	}

	protected override BusinessObjectAndExpectedOutputFileName GetEmptyBizObjSample()
	{
		var emptyWhsReceivePath = resourceRetriever.Value.SaveResourceToFile("Enterprise.Warehouse.Transactions.GUI.Testing.TestFiles.EmptyWhsReceive.xml");
		return new BusinessObjectAndExpectedOutputFileName(GetEmptyWhsDocket(), emptyWhsReceivePath, ValidationKind.None, "Empty WhsReceive");
	}

	protected override BusinessObjectAndExpectedOutputFileName GetFullyPopulatedBizObjSample()
	{
		var populatedWhsReceivePath = resourceRetriever.Value.SaveResourceToFile("Enterprise.Warehouse.Transactions.GUI.Testing.TestFiles.PopulatedWhsReceive.xml");
		return new BusinessObjectAndExpectedOutputFileName(GetFullyPopulatedWhsDocket(), populatedWhsReceivePath, ValidationKind.None, "Populated WhsReceive");
	}

	protected override void SetOverridenAddresses(Xsd.WhsDocket xsdDocket)
	{
		var customerReceiveDetail = (Xsd.WhsCustomerInwardsDetail)xsdDocket.DocketDetail.Item;
		customerReceiveDetail.Supplier.AddressReference.IsSpecified = false;
	}

	protected override void AssertOverridenAddressesWereImportedCorrectly(WhsReceive whsReceive)
	{
		AssertNotNull(whsReceive.SupplierDocAddress.Organisation);
		AssertEquals("Supplier Org", whsReceive.SupplierDocAddress.E2_CompanyName);
	}

	protected override void PopulateCustomAddress(Xsd.WhsDocketDocketDetail docketDetail)
	{
		base.PopulateCustomAddress(docketDetail);
		var customerReceiveDetail = (Xsd.WhsCustomerInwardsDetail)docketDetail.Item;
		customerReceiveDetail.Supplier.AddressReference.IsSpecified = false;
		customerReceiveDetail.Supplier.AddressLine1 = "ReceiveSupplierAddress1";
	}

	protected override void AssertCustomAddress(WhsReceive receive)
	{
		base.AssertCustomAddress(receive);

		AssertEquals("ReceiveSupplierAddress1", receive.SupplierDocAddress.Address1);
		AssertEquals("DocketSupplierAddress2", receive.SupplierDocAddress.Address2);
	}
}
