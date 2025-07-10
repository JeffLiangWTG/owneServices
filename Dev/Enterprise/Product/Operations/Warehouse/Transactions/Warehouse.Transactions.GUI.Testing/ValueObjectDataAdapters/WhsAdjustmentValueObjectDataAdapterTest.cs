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

[TestedType(typeof(WhsAdjustmentValueObjectDataAdapter))]
sealed class WhsAdjustmentValueObjectDataAdapterTest : WhsDocketValueObjectDataAdapterTest<WhsAdjustment>
{
	protected override ZString GetExpectedExportedDocketStatus()
	{
		return DocketStatus.Codes.Finalised;
	}

	protected override void PopulateWhsDocketForExport(WhsAdjustment whsDocket)
	{
		base.PopulateWhsDocketForExport(whsDocket);
		whsDocket.WD_FinalisedDate = new ZDateTimeOffset(2007, 07, 06);
		whsDocket.WD_DocketStatus = DocketStatus.Codes.Finalised;
	}

	protected override void AssertExportedXsdDocket(Xsd.WhsDocket xsdDocket, WhsAdjustment adjustment)
	{
		base.AssertExportedXsdDocket(xsdDocket, adjustment);
		AssertEquals(DocketTypes.Codes.WhsAdjustment, xsdDocket.Identifier.DocketType);
		var xsdAdjustment = (Xsd.WhsCustomerAdjustmentDetail)xsdDocket.DocketDetail.Item;
		AssertEquals(new ZDateTime(2007, 07, 06), xsdAdjustment.AdjustmentDate);
	}

	protected override void AssertExportXsdDocketLines(Xsd.WhsDocket xsdDocket)
	{
		AssertEquals(2, xsdDocket.DocketLines.Count);

		AssertEquals("Description1", xsdDocket.DocketLines[0].Description);
		AssertEquals("Line Coments1", xsdDocket.DocketLines[0].LineComments);
		AssertEquals(new ZShort(1), xsdDocket.DocketLines[0].LineNumber);
		AssertEquals("P1", xsdDocket.DocketLines[0].Product);
		AssertEquals("UNT", xsdDocket.DocketLines[0].ProductUQ);
		AssertEquals(10m, xsdDocket.DocketLines[0].QuantityActuallyOrdered);
		AssertEquals(10m, xsdDocket.DocketLines[0].QuantityFromClientOrder);
		AssertEquals(new ZShort(1), xsdDocket.DocketLines[0].SubLineNumber);

		AssertEquals("EK1", xsdDocket.DocketLines[0].LineAttributes.BondedEntryKey);
		AssertEquals("CA11", xsdDocket.DocketLines[0].LineAttributes.CustomAttribute1);
		AssertEquals("CA12", xsdDocket.DocketLines[0].LineAttributes.CustomAttribute2);
		AssertEquals("CA13", xsdDocket.DocketLines[0].LineAttributes.CustomAttribute3);
		AssertEquals(new ZDate(2007, 10, 12), xsdDocket.DocketLines[0].LineAttributes.ExpiryDate);
		AssertEquals(new ZDate(2007, 11, 12), xsdDocket.DocketLines[0].LineAttributes.PackingDate);
		AssertEquals("PA11", xsdDocket.DocketLines[0].LineAttributes.PartAttribute1);
		AssertEquals("PA12", xsdDocket.DocketLines[0].LineAttributes.PartAttribute2);
		AssertEquals("PA13", xsdDocket.DocketLines[0].LineAttributes.PartAttribute3);

		AssertEquals("AddInfo1", xsdDocket.DocketLines[0].CustomsData.AddInfo);
		AssertEquals(10m, xsdDocket.DocketLines[0].CustomsData.BondedWhsQuantity);
		AssertEquals("U1", xsdDocket.DocketLines[0].CustomsData.BondedWhsQuantityUnit);
		AssertEquals(11m, xsdDocket.DocketLines[0].CustomsData.CustomsQuantity);
		AssertEquals("C1", xsdDocket.DocketLines[0].CustomsData.CustomsQuantityUnit);
		AssertEquals("DecRef1", xsdDocket.DocketLines[0].CustomsData.DeclarationReference);
		AssertEquals(new ZDateTime(2007, 10, 10), xsdDocket.DocketLines[0].CustomsData.EntryDate);
		AssertEquals("BEK1", xsdDocket.DocketLines[0].CustomsData.EntryKey);
		AssertEquals(new ZShort(1), xsdDocket.DocketLines[0].CustomsData.EntryLineNumber);
		AssertEquals("US", xsdDocket.DocketLines[0].CustomsData.CountryOfOrigin);
		AssertEquals("USD", xsdDocket.DocketLines[0].CustomsData.TILVCurrency);
		AssertEquals(100m, xsdDocket.DocketLines[0].CustomsData.TILVAmount);
		AssertEquals(110m, xsdDocket.DocketLines[0].CustomsData.ValueForDuty);

		AssertEquals("Description2", xsdDocket.DocketLines[1].Description);
		AssertEquals("Line Coments2", xsdDocket.DocketLines[1].LineComments);
		AssertEquals(new ZShort(2), xsdDocket.DocketLines[1].LineNumber);
		AssertEquals("P2", xsdDocket.DocketLines[1].Product);
		AssertEquals("UNT", xsdDocket.DocketLines[1].ProductUQ);
		AssertEquals(20m, xsdDocket.DocketLines[1].QuantityActuallyOrdered);
		AssertEquals(20m, xsdDocket.DocketLines[1].QuantityFromClientOrder);
		AssertEquals(new ZShort(2), xsdDocket.DocketLines[1].SubLineNumber);

		AssertEquals("EK2", xsdDocket.DocketLines[1].LineAttributes.BondedEntryKey);
		AssertEquals("CA21", xsdDocket.DocketLines[1].LineAttributes.CustomAttribute1);
		AssertEquals("CA22", xsdDocket.DocketLines[1].LineAttributes.CustomAttribute2);
		AssertEquals("CA23", xsdDocket.DocketLines[1].LineAttributes.CustomAttribute3);
		AssertEquals(new ZDate(2007, 10, 15), xsdDocket.DocketLines[1].LineAttributes.ExpiryDate);
		AssertEquals(new ZDate(2007, 11, 15), xsdDocket.DocketLines[1].LineAttributes.PackingDate);
		AssertEquals("PA21", xsdDocket.DocketLines[1].LineAttributes.PartAttribute1);
		AssertEquals("PA22", xsdDocket.DocketLines[1].LineAttributes.PartAttribute2);
		AssertEquals("PA23", xsdDocket.DocketLines[1].LineAttributes.PartAttribute3);

		AssertEquals("AddInfo2", xsdDocket.DocketLines[1].CustomsData.AddInfo);
		AssertEquals(20m, xsdDocket.DocketLines[1].CustomsData.BondedWhsQuantity);
		AssertEquals("U2", xsdDocket.DocketLines[1].CustomsData.BondedWhsQuantityUnit);
		AssertEquals(22m, xsdDocket.DocketLines[1].CustomsData.CustomsQuantity);
		AssertEquals("C2", xsdDocket.DocketLines[1].CustomsData.CustomsQuantityUnit);
		AssertEquals("DecRef2", xsdDocket.DocketLines[1].CustomsData.DeclarationReference);
		AssertEquals(new ZDateTime(2007, 10, 20), xsdDocket.DocketLines[1].CustomsData.EntryDate);
		AssertEquals("BEK2", xsdDocket.DocketLines[1].CustomsData.EntryKey);
		AssertEquals(new ZShort(2), xsdDocket.DocketLines[1].CustomsData.EntryLineNumber);
		AssertEquals("AU", xsdDocket.DocketLines[1].CustomsData.CountryOfOrigin);
		AssertEquals("AUD", xsdDocket.DocketLines[1].CustomsData.TILVCurrency);
		AssertEquals(200m, xsdDocket.DocketLines[1].CustomsData.TILVAmount);
		AssertEquals(220m, xsdDocket.DocketLines[1].CustomsData.ValueForDuty);

		AssertEquals(0, xsdDocket.DocketLines[1].CrossDockLines.Count);
	}

	protected override void AssertExportedXsdDocketTotals(Xsd.WhsDocket xsdDocket, WhsAdjustment whsDocket)
	{
	}

	protected override WhsAdjustment GetNewPopulatedDocket(WhsWarehouse whs, OrgHeader client, ZString reference)
	{
		return Helper.CreateWhsAdjustment(client, whs, reference);
	}

	protected override ZString GetXSDDocketTypeCode()
	{
		return DocketTypes.Codes.WhsAdjustment;
	}

	protected override Type GetExpectedDocketErrorHandlerType()
	{
		throw new NotImplementedException();
	}

	protected override ZString GetExternalReference()
	{
		return "ADJUSTMENT1";
	}

	protected override ZString GetExpectedMessage()
	{
		throw new NotImplementedException();
	}

	protected override Type GetDataAdapterType()
	{
		return typeof(WhsAdjustmentValueObjectDataAdapter);
	}

	protected override ValueObjectDataAdapter<WhsAdjustment, Xsd.WhsDocket> GetNewBizObjXmlDataAdapter()
	{
		var result = (WhsAdjustmentValueObjectDataAdapter)GetDataAdapter();
		result.FileName = "AdjustmentFile";
		return result;
	}

	protected override WhsAdjustment GetEmptyWhsDocket()
	{
		var adjustment = Factory.New<WhsAdjustment>();
		adjustment.WD_DocketStatus = DocketStatus.Codes.Cancelled;
		return adjustment;
	}

	protected override WhsAdjustment GetFullyPopulatedWhsDocket()
	{
		var whs = Helper.CreateWarehouse("Warehouse");
		whs.WW_WarehouseCode = "WHS";

		var savingFactory = new BusinessObjectFactory();
		var client = Helper.CreateOrLoadClientInSeperateFactory("WHSCLIENT", "WHSClient for test", savingFactory);
		SetCustomDocketAttributesForFlagsAndDecimals(client);
		savingFactory.Save();

		var adjustment = Helper.CreateWhsAdjustment(client, whs);
		adjustment.WD_ExternalReference = "ADJUSTMENT1";
		adjustment.WD_FinalisedDate = new ZDateTimeOffset(2007, 08, 16);
		adjustment.WD_DocketStatus = DocketStatus.Codes.Cancelled;
		adjustment.WD_CustomAttrib1 = "Attrib1";
		adjustment.WD_CustomAttrib2 = "Attrib2";
		adjustment.WD_CustomAttrib3 = "Attrib3";
		adjustment.WD_CustomAttrib4 = "Attrib4";
		adjustment.WD_CustomAttrib5 = "Attrib5";
		adjustment.WD_CustomDate1 = new ZDateTime(2008, 01, 01);
		adjustment.WD_CustomDate2 = new ZDateTime(2008, 12, 31);
		adjustment.WD_CustomDecimal1 = .009m;
		adjustment.WD_CustomDecimal2 = 1000m;
		adjustment.WD_CustomDecimal3 = 123.457m;
		adjustment.WD_CustomDecimal4 = 67m;
		adjustment.WD_CustomDecimal5 = 5.23m;
		adjustment.WD_CustomFlag1 = true;
		adjustment.WD_CustomFlag2 = false;
		adjustment.WD_CustomFlag3 = true;
		adjustment.WD_CustomFlag4 = false;
		adjustment.WD_CustomFlag5 = true;

		PopulateCustomValues(adjustment);

		return adjustment;
	}

	protected override Xsd.WhsDockets GetNewXsdDockets()
	{
		var xsdDockets = base.GetNewXsdDockets();
		var xsdDocket = xsdDockets.WhsDocket[0];
		xsdDocket.DocketDetail.Item = new Xsd.WhsCustomerAdjustmentDetail();
		var xsdLine = xsdDocket.DocketLines[0];
		return xsdDockets;
	}

	protected override WhsDocketValueObjectDataAdapter<WhsAdjustment> GetDataAdapter()
	{
		return new WhsAdjustmentValueObjectDataAdapter();
	}

	protected override WhsDocketCollection GetCollection()
	{
		return new WhsAdjustmentCollection(Factory, new AdhocCollectionRelationship(typeof(WhsAdjustment)));
	}

	protected override void PopulateXsdDocketAdditonalDetail(Xsd.WhsDocket xsdDocket)
	{
	}

	protected override void PopulateXsdDocketLineAdditionalDetail(Xsd.WhsDocketLine xsdDocketLine)
	{
	}

	protected override void AssertWhsDocketOtherDetail(WhsAdjustment whsDocket)
	{
		throw new NotImplementedException();
	}

	protected override void AssertWhsDocketLineOtherDetail(WhsDocketLine whsDocketLine)
	{
		throw new NotImplementedException();
	}

	protected override bool IsImportFromValueObjectSupported => false;

	protected override BusinessObjectAndExpectedOutputFileName GetEmptyBizObjSample()
	{
		var emptyWhsAdjustmentPath = resourceRetriever.Value.SaveResourceToFile("Enterprise.Warehouse.Transactions.GUI.Testing.TestFiles.EmptyWhsAdjustment.xml");
		return new BusinessObjectAndExpectedOutputFileName(GetEmptyWhsDocket(), emptyWhsAdjustmentPath, ValidationKind.None, "Empty WhsAdjustment");
	}

	protected override BusinessObjectAndExpectedOutputFileName GetFullyPopulatedBizObjSample()
	{
		var populatedWhsAdjustmentPath = resourceRetriever.Value.SaveResourceToFile("Enterprise.Warehouse.Transactions.GUI.Testing.TestFiles.PopulatedWhsAdjustment.xml");
		return new BusinessObjectAndExpectedOutputFileName(GetFullyPopulatedWhsDocket(), populatedWhsAdjustmentPath, ValidationKind.None, "Populated WhsAdjustment");
	}
}
