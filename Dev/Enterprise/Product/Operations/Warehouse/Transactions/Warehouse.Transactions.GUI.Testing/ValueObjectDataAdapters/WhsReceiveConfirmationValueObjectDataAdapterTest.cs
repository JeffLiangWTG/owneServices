using System;
using CargoWise.Types;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.DataTransfer;
using NUnit.Framework;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Warehouse.Transactions.GUI.Testing;

[TestedType(typeof(WhsReceiveConfirmationValueObjectDataAdapter))]
sealed class WhsReceiveConfirmationValueObjectDataAdapterTest : WhsReceiveValueObjectDataAdapterTest
{
	protected override WhsDocketLine CreateNewPopulatedDocketLine(WhsReceive docket, WhsProduct product, ZDecimal quantity, ZString uQ, ZString entryKey, ZShort lineNo, ZShort lineSubNo,
		ZString pA1, ZString pA2, ZString pA3, ZString sn, ZString cA1, ZString cA2, ZString cA3, ZString cA4, ZString cA5, ZString cA6, ZDateTime cDate1, ZDateTime cDate2, ZDateTime cDate3, ZDateTime cDate4, ZDateTime cDate5,
		ZDecimal cDecimal1, ZDecimal cDecimal2, ZDecimal cDecimal3, ZDecimal cDecimal4, ZDecimal cDecimal5, ZBool cFlag1, ZBool cFlag2, ZBool cFlag3, ZBool cFlag4, ZBool cFlag5,
		WhsBondedWarehouseAttribute bWA, ZString comments, ZDate expireDate, ZDate packingDate, ZDecimal confirmedQty)
	{
		var line = base.CreateNewPopulatedDocketLine(docket, product, quantity, uQ, entryKey, lineNo, lineSubNo, pA1, pA2, pA3, sn, cA1, cA2, cA3, cA4, cA5, cA6,
			cDate1, cDate2, cDate3, cDate4, cDate5, cDecimal1, cDecimal2, cDecimal3, cDecimal4, cDecimal5, cFlag1, cFlag2, cFlag3, cFlag4, cFlag5,
			bWA, comments, expireDate, packingDate, confirmedQty);

		line.WE_ClientOrderedUnits = quantity;
		line.WE_TransactionQuantity = confirmedQty;
		return line;
	}

	protected override void AssertExportXsdDocketLines(Xsd.WhsDocket xsdDocket)
	{
		AssertEquals(2, xsdDocket.DocketLines.Count);

		#region Line 1

		AssertEquals("Description1", xsdDocket.DocketLines[0].Description);
		AssertEquals("Line Coments1", xsdDocket.DocketLines[0].LineComments);
		AssertEquals(new ZShort(1), xsdDocket.DocketLines[0].LineNumber);
		AssertEquals("P1", xsdDocket.DocketLines[0].Product);
		AssertEquals("UNT", xsdDocket.DocketLines[0].ProductUQ);
		AssertEquals(10m, xsdDocket.DocketLines[0].QuantityActuallyOrdered);
		AssertEquals(10m, xsdDocket.DocketLines[0].QuantityFromClientOrder);
		AssertEquals(new ZShort(1), xsdDocket.DocketLines[0].SubLineNumber);

		AssertEquals("EK1", xsdDocket.DocketLines[0].LineAttributes.BondedEntryKey);
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

		#region Confirmation

		AssertEquals(5m, xsdDocket.DocketLines[0].Confirmation.Quantity);
		AssertEquals(0, xsdDocket.DocketLines[0].Confirmation.Lines.Count);

		#endregion

		#endregion

		#region Line 2

		AssertEquals("Description2", xsdDocket.DocketLines[1].Description);
		AssertEquals("Line Coments2", xsdDocket.DocketLines[1].LineComments);
		AssertEquals(new ZShort(2), xsdDocket.DocketLines[1].LineNumber);
		AssertEquals("P2", xsdDocket.DocketLines[1].Product);
		AssertEquals("UNT", xsdDocket.DocketLines[1].ProductUQ);
		AssertEquals(20m, xsdDocket.DocketLines[1].QuantityActuallyOrdered);
		AssertEquals(20m, xsdDocket.DocketLines[1].QuantityFromClientOrder);
		AssertEquals(new ZShort(2), xsdDocket.DocketLines[1].SubLineNumber);

		AssertEquals("EK2", xsdDocket.DocketLines[1].LineAttributes.BondedEntryKey);
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

		#region Confirmation

		AssertEquals(10m, xsdDocket.DocketLines[1].Confirmation.Quantity);
		AssertEquals(0, xsdDocket.DocketLines[1].Confirmation.Lines.Count);

		#endregion

		#endregion

	}

	protected override WhsDocketValueObjectDataAdapter<WhsReceive> GetDataAdapter()
	{
		return new WhsReceiveConfirmationValueObjectDataAdapter();
	}

	protected override Xsd.WhsDocketIdentifierActionType GetExpectedActionType()
	{
		return Xsd.WhsDocketIdentifierActionType.CON;
	}

	protected override Type GetDataAdapterType()
	{
		return typeof(WhsReceiveConfirmationValueObjectDataAdapter);
	}

	protected override bool IsImportFromValueObjectSupported => false;

	protected override BusinessObjectAndExpectedOutputFileName GetEmptyBizObjSample()
	{
		var emptyWhsReceiveConfirmationPath = resourceRetriever.Value.SaveResourceToFile("Enterprise.Warehouse.Transactions.GUI.Testing.TestFiles.EmptyWhsReceiveConfirmation.xml");
		return new BusinessObjectAndExpectedOutputFileName(GetEmptyWhsDocket(), emptyWhsReceiveConfirmationPath, ValidationKind.None, "Empty WhsOrder Confirmation");
	}

	protected override BusinessObjectAndExpectedOutputFileName GetFullyPopulatedBizObjSample()
	{
		var populatedWhsReceiveConfirmationPath = resourceRetriever.Value.SaveResourceToFile("Enterprise.Warehouse.Transactions.GUI.Testing.TestFiles.PopulatedWhsReceiveConfirmation.xml");
		return new BusinessObjectAndExpectedOutputFileName(GetFullyPopulatedWhsDocket(), populatedWhsReceiveConfirmationPath, ValidationKind.None, "Populated WhsOrder Confirmation");
	}
}
