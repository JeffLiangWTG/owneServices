using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.DataTransfer.Xml;
using Enterprise.DataTransfer.Xml.Testing;
using Enterprise.DocumentScanning.Business.Test;
using Enterprise.DocumentScanning.Business.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.Warehouse.Transactions.DataTransfer;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Warehouse.Transactions.GUI.Testing;

abstract class WhsDocketValueObjectDataAdapterTest<TDocket> : ValueObjectDataAdapterTest<TDocket, Xsd.WhsDocket>
	where TDocket : WhsDocket
{
	#region Test Error Handler

	public void TestDocketLinesErrorHandler()
	{
		if (IsImportFromValueObjectSupported)
		{
			AssertNotNull(Adapter.DocketErrorHandler);
			AssertEquals(GetExpectedDocketErrorHandlerType(), Adapter.DocketErrorHandler.GetType());

			var collection = GetCollection();
			var xsdDockets = new Xsd.WhsDockets();

			foreach (AttributeNumber attribNo in Enum.GetValues(typeof(AttributeNumber)))
			{
				Helper.SetClientAttributeType(Client, attribNo, false);
				Helper.SetProductAttributeUse(Client, Part, attribNo, false);
			}
			Factory.Save();

			PopulateXsdDockets(xsdDockets);

			var context = new ValueObjectImportContext(Factory, Notifications);
			Adapter.ImportFromValueObject(collection, xsdDockets, context);

			AssertEquals("Collection should have a new element", 1, collection.Count);
			var importedDocket = (TDocket)collection[0];
			AssertEquals("Invalid docket status", DocketStatus.Codes.Error, importedDocket.WD_DocketStatus);

			GetDocketLogEvent(importedDocket, Events.DataImport.Description, "Line 1.0 Error (Part Attr1): ATTR1");
			GetDocketLogEvent(importedDocket, Events.DataImport.Description, "Line 1.0 Error (Part Attr2): ATTR2");
			GetDocketLogEvent(importedDocket, Events.DataImport.Description, "Line 1.0 Error (Part Attr3): ATTR3");
			GetDocketLogEvent(importedDocket, Events.DataImport.Description, "Line 1.0 Error (Expiry Date): 17-Oct-06");
			GetDocketLogEvent(importedDocket, Events.DataImport.Description, "Line 1.0 Error (Packing Date): 16-Oct-06");
		}
		else
		{
			Assert("Not Required", true);
		}
	}

	public void TestDocketErrorHandler()
	{
		if (IsImportFromValueObjectSupported)
		{
			AssertNotNull(Adapter.DocketErrorHandler);
			AssertEquals(GetExpectedDocketErrorHandlerType(), Adapter.DocketErrorHandler.GetType());

			var collection = GetCollection();
			var xsdDockets = new Xsd.WhsDockets();
			var xsdDocket = xsdDockets.WhsDocket.AddNew();

			PopulateOrg(xsdDocket.Identifier.Client, Client.OH_FullName, Client.MainAddress.OA_Address1, Client.UNLOCO.RL_Code);

			xsdDocket.Identifier.Reference = "Ref1";
			xsdDocket.DocketDetail.WarehouseCode = "WHS";

			var xsdLine = xsdDocket.DocketLines.AddNew();
			xsdLine.LineNumber = 1;
			xsdLine.SubLineNumber = 1;
			xsdLine.Product = "PR1";

			xsdLine = xsdDocket.DocketLines.AddNew();
			xsdLine.LineNumber = 1;
			xsdLine.SubLineNumber = 2;
			xsdLine.Product = "PR2";

			var context = new ValueObjectImportContext(Factory, Notifications);
			Adapter.ImportFromValueObject(collection, xsdDockets, context);

			AssertEquals("Collection should have a new element", 1, collection.Count);
			var importedDocket = (TDocket)collection[0];
			AssertEquals("Invalid docket status", DocketStatus.Codes.Error, importedDocket.WD_DocketStatus);

			GetDocketLogEvent(importedDocket, Events.DataImport.Description, "Line 1.1 Error (Product): PR1");
			GetDocketLogEvent(importedDocket, Events.DataImport.Description, "Line 1.2 Error (Product): PR2");
		}
		else
		{
			Assert("Not Required", true);
		}
	}

	protected StmALogCollection GetDocketLogEvents(TDocket docket, ZString eventDescription, ZString eventReference)
	{
		var logs = new StmALogCollection(Factory);
		foreach (var log in docket.Logs.LogsNotInDB)
		{
			if (log.SL_EventDescription == eventDescription && log.SL_Reference == eventReference)
			{
				logs.Add(log);
			}
		}
		return logs;
	}

	protected StmALog GetDocketLogEvent(TDocket docket, ZString eventDescription, ZString eventReference)
	{
		var logs = GetDocketLogEvents(docket, eventDescription, eventReference);
		AssertEquals("Should find only one log with description '" + eventDescription + "' and reference '" + eventReference + "'", 1, logs.Count);
		AssertNotNull("Log should be created for event '" + eventDescription + "' and reference '" + eventReference + "'", logs[0]);
		return logs[0];
	}

	#endregion

	#region Test Data Formatter

	public void TestDocketDataFormatter()
	{
		AssertNotNull(Adapter.DocketDataFormatter);
		AssertEquals(GetExpectedDocketDataFormatterType(), Adapter.DocketDataFormatter.GetType());
	}

	public void TestDocketDataFormatterBeingUsed()
	{
		if (IsImportFromValueObjectSupported)
		{
			var collection = GetCollection();
			var xsdDockets = GetNewXsdDockets();
			var xsdDocket = xsdDockets.WhsDocket[0];
			PopulateXsdDocket(xsdDocket, Whs, Client, "REF1");
			var context = new ValueObjectImportContext(Factory, Notifications);

			AssertEquals("Precondition: TESTWasFormatXsdDocketDataCalled is false ", false, Adapter.DocketDataFormatter.TESTWasFormatXsdDocketDataCalled);
			AssertEquals("Precondition: TESTWasFormatXsdDocketLineDataCalled is false", false, Adapter.DocketDataFormatter.TESTWasFormatXsdDocketLineDataCalled);

			Adapter.ImportFromValueObject(collection, xsdDockets, context);
			AssertEquals(true, Adapter.DocketDataFormatter.TESTWasFormatXsdDocketDataCalled);
			AssertEquals(true, Adapter.DocketDataFormatter.TESTWasFormatXsdDocketDataCalled);
		}
		else
		{
			Assert("Not Required", true);
		}
	}

	#endregion

	#region Test Data Import

	#region Test Cross Dock

	public void TestImportLineCrossDock()
	{
		if (IsImportFromValueObjectSupported)
		{
			var collection = GetCollection();
			var xsdDockets = new Xsd.WhsDockets();
			var xsdDocket = xsdDockets.WhsDocket.AddNew();

			var warehouse = Helper.CreateWarehouse("WHS");
			warehouse.WW_WarehouseCode = "WH1";

			var client = Helper.CreateClient("CLT");
			client.Addresses.MainAddress.OA_Address1 = "ClientAddress";
			client.OH_RL_NKClosestPort = "AUSYD";
			var part = Helper.CreateProduct(client, "PART");
			var partUnit = part.PartUnits.AddNew();
			partUnit.OF_ParentPackType = "PLT";
			partUnit.OF_PackType = "UNT";
			partUnit.OF_QuantityInParent = 10;
			part.OP_StockKeepingUnit = "PLT";

			SetupCrossDockDataForImport(warehouse, client, part);

			Factory.Save();

			var context = new ValueObjectImportContext(Factory, Notifications);

			PopulateXsdDocket(xsdDocket, Whs, client, "REF");
			xsdDocket.Identifier.ActionType = Xsd.WhsDocketIdentifierActionType.NTF;
			var xsdLine = xsdDocket.DocketLines.AddNew();
			PopulateXsdDocketLine(xsdLine, "PART", 1, 0, 10m);

			xsdLine = xsdDocket.DocketLines.AddNew();
			PopulateXsdDocketLine(xsdLine, "PART", 2, 1, 15m);
			AddCrossDockDataToXsdLine(xsdLine, "CD1", 1, 0, 5m, "PLT");

			xsdLine = xsdDocket.DocketLines.AddNew();
			PopulateXsdDocketLine(xsdLine, "PART", 3, 2, 15m);
			AddCrossDockDataToXsdLine(xsdLine, "CD1", 1, 1, 10m, "CAS");
			AddCrossDockDataToXsdLine(xsdLine, "CD2", 1, 0, 100m, "UNT");

			xsdLine = xsdDocket.DocketLines.AddNew();
			PopulateXsdDocketLine(xsdLine, "PART", 4, 0, 20m);
			AddCrossDockDataToXsdLine(xsdLine, "CD3", 1, 0, 5m, "PLT");
			AddCrossDockDataToXsdLine(xsdLine, "CD4", 1, 0, 5m, "PLT");
			AddCrossDockDataToXsdLine(xsdLine, "CD5", 1, 0, 5m, "PLT");

			xsdLine = xsdDocket.DocketLines.AddNew();
			PopulateXsdDocketLine(xsdLine, "PART", 10, 1, 5m);
			AddCrossDockDataToXsdLine(xsdLine, "CD1", 1, 0, 5m, "PLT");

			Notifications.Clear();
			Adapter.ImportFromValueObject(collection, xsdDockets, context);
			AssertEquals("Collection should have a new element", 1, collection.Count);

			var importedDocket = (TDocket)collection[0];
			AssertEquals("Should have 5 lines", 5, importedDocket.Lines.Count);
			AssertImportedCrossDockData(importedDocket);
		}
		else
		{
			Assert("Not Required", true);
		}
	}

	protected virtual void AddCrossDockDataToXsdLine(Xsd.WhsDocketLine line, ZString reference, ZShort lineNo, ZShort subLineNo, ZDecimal units, ZString unitsUQ)
	{
		var crossDocketLine = line.CrossDockLines.AddNew();
		crossDocketLine.Reference = reference;
		crossDocketLine.LineNumber = lineNo;
		crossDocketLine.SubLineNumber = subLineNo;
		crossDocketLine.AllocationQty = units;
		crossDocketLine.AllocationQtyUQ = unitsUQ;
	}

	protected virtual void SetupCrossDockDataForImport(WhsWarehouse warehouse, OrgHeader client, OrgSupplierPart part)
	{
	}

	protected virtual void AssertImportedCrossDockData(TDocket docket)
	{
	}

	#endregion

	#region TestImportLineSelectsCorrectProduct

	public void TestImportLineSelectsCorrectProduct()
	{
		if (IsImportFromValueObjectSupported)
		{
			var collection = GetCollection();
			var xsdDockets = new Xsd.WhsDockets();
			var xsdDocket = xsdDockets.WhsDocket.AddNew();

			var client1 = Helper.CreateClient("CLT1");
			client1.Addresses.MainAddress.OA_Address1 = "ClientAddress1";
			client1.OH_RL_NKClosestPort = "AUSYD";

			var client2 = Helper.CreateClient("CLT2");
			client2.Addresses.MainAddress.OA_Address1 = "ClientAddress2";
			client2.OH_RL_NKClosestPort = "AUSYD";

			var part1 = Helper.CreateProduct(client1, "PART");
			Helper.CreateProductClientRelationShip(client1, part1);

			var part2 = Helper.CreateProduct(client2, "PART");
			Helper.CreateProductClientRelationShip(client2, part2);

			Factory.Save();

			var context = new ValueObjectImportContext(Factory, Notifications);

			PopulateXsdDocket(xsdDocket, Whs, client1, "REF1");
			PopulateXsdDocketLine(xsdDocket.DocketLines.AddNew(), "PART", 1, 0, 100m);
			xsdDocket.Identifier.ActionType = Xsd.WhsDocketIdentifierActionType.NTF;
			Notifications.Clear();
			Adapter.ImportFromValueObject(collection, xsdDockets, context);
			AssertEquals("Collection should have a new element", 1, collection.Count);
			var importedDocket = (TDocket)collection[0];
			AssertEquals("Should have 1 line", 1, importedDocket.Lines.Count);
			AssertNotNull("Should have imported Product", importedDocket.Lines[0].Product);
			AssertNotNull("Should have imported Product", importedDocket.Lines[0].Product.Parent);
			AssertEquals("Should import Part1", part1, importedDocket.Lines[0].Product.Parent);

			collection = GetCollection();
			xsdDockets = new Xsd.WhsDockets();
			xsdDocket = xsdDockets.WhsDocket.AddNew();
			PopulateXsdDocket(xsdDocket, Whs, client2, "REF1");
			PopulateXsdDocketLine(xsdDocket.DocketLines.AddNew(), "PART", 1, 0, 100m);
			xsdDocket.Identifier.ActionType = Xsd.WhsDocketIdentifierActionType.NTF;
			Notifications.Clear();
			Adapter.ImportFromValueObject(collection, xsdDockets, context);
			AssertEquals("Collection should have a new element", 1, collection.Count);
			importedDocket = (TDocket)collection[0];
			AssertEquals("Should have 1 line", 1, importedDocket.Lines.Count);
			AssertNotNull("Should have imported Product", importedDocket.Lines[0].Product);
			AssertNotNull("Should have imported Product", importedDocket.Lines[0].Product.Parent);
			AssertEquals("Should import Part2", part2, importedDocket.Lines[0].Product.Parent);
		}
		else
		{
			Assert("Not Required", true);
		}
	}

	#endregion

	#region TestImportOfNonExistingDocketWithLines

	public void TestImportOfNonExistingDocketWithLines()
	{
		if (IsImportFromValueObjectSupported)
		{
			var part1 = Helper.CreateProduct(Client, "PR1");
			var part2 = Helper.CreateProduct(Client, "PR2");

			var collection = GetCollection();
			var xsdDockets = new Xsd.WhsDockets();
			var xsdDocket = xsdDockets.WhsDocket.AddNew();
			PopulateXsdDocket(xsdDocket, Whs, Client, "REF1");

			var xsdLine10 = xsdDocket.DocketLines.AddNew();
			PopulateXsdDocketLine(xsdLine10, part1.OP_PartNum, 1, 0, 100m);

			var xsdLine11 = xsdDocket.DocketLines.AddNew();
			PopulateXsdDocketLine(xsdLine11, part1.OP_PartNum, 1, 1, 50m);

			var xsdLine20 = xsdDocket.DocketLines.AddNew();
			PopulateXsdDocketLine(xsdLine20, part2.OP_PartNum, 2, 0, 200m);

			Factory.Save();

			var context = new ValueObjectImportContext(Factory, Notifications);
			Adapter.ImportFromValueObject(collection, xsdDockets, context);

			AssertEquals("Collection should have a new element", 1, collection.Count);
			var importedDocket = (TDocket)collection[0];
			AssertEquals("REF1", importedDocket.WD_ExternalReference);

			AssertEquals("Docket should have 3 lines", 3, importedDocket.Lines.Count);
			AssertLineInfo(importedDocket, 1, 0, 100m);
			AssertLineInfo(importedDocket, 1, 1, 50m);
			AssertLineInfo(importedDocket, 2, 0, 200m);
		}
		else
		{
			Assert("Not required", true);
		}
	}

	#endregion

	#region TestImportOfNonExistingDocketNoLines

	public void TestImportOfNonExistingDocketNoLines()
	{
		if (IsImportFromValueObjectSupported)
		{
			var part1 = Helper.CreateProduct(Client, "PR1");
			var part2 = Helper.CreateProduct(Client, "PR2");

			var collection = GetCollection();
			var xsdDockets = new Xsd.WhsDockets();
			var xsdDocket = xsdDockets.WhsDocket.AddNew();
			PopulateXsdDocket(xsdDocket, Whs, Client, "REF1");

			Factory.Save();

			var context = new ValueObjectImportContext(Factory, Notifications);
			Adapter.ImportFromValueObject(collection, xsdDockets, context);

			AssertEquals("Collection should have 1 imported dockets", 1, collection.Count);
			var importedDocket = (TDocket)collection[0];
			AssertEquals("REF1", importedDocket.WD_ExternalReference);

			AssertEquals("Docket should have status 'Cancelled'", true, importedDocket.IsCancelled);
			AssertEquals("Docket should have 0 lines", 0, importedDocket.Lines.Count);
		}
		else
		{
			Assert("Not required", true);
		}
	}

	#endregion

	#region TestFindExistingBusinessObject_SameRefButAnotherDocketTypeInXML

	public void TestFindExistingBusinessObject_SameRefButAnotherDocketTypeInXML()
	{
		Factory.Save(); // client needs to be saved to db

		var docket = GetNewPopulatedDocket(Whs, Client, "Ref1");
		var xsdDocketTypeCode = GetXSDDocketTypeCode();
		var xsdOtherDocketTypeCode = new Warehouse.DataTransfer.CodeLists.DocketTypes().ToArray().First(c => c.Code != xsdDocketTypeCode).Code;

		var xsdDockets = new Xsd.WhsDockets();
		var xsdDocket = xsdDockets.WhsDocket.AddNew();
		PopulateXsdDocket(xsdDocket, Whs, Client, "Ref1");
		var context = new ValueObjectImportContext(Factory, Notifications);

		xsdDocket.Identifier.DocketType = docket.WD_DocketType;
		var docketFound = Adapter.FindExistingBusinessObject(xsdDocket, context);
		AssertEquals("Should find Docket for xsd of our Type.", docket, docketFound);

		xsdDocket.Identifier.DocketType = "";
		docketFound = Adapter.FindExistingBusinessObject(xsdDocket, context);
		AssertEquals("Should still find Docket for xsd of our Type.", docket, docketFound);

		xsdDocket.Identifier.DocketType = xsdDocketTypeCode;
		docketFound = Adapter.FindExistingBusinessObject(xsdDocket, context);
		AssertEquals("Should find Docket for xsd of our Type.", docket, docketFound);

		xsdDocket.Identifier.DocketType = xsdOtherDocketTypeCode;
		docketFound = Adapter.FindExistingBusinessObject(xsdDocket, context);
		AssertNull("Should not load a Docket of another Type.", docketFound);
	}

	public void TestFindExistingBusinessObject_SameRefButAnotherType_EmptyDocketTypeInXML()
	{
		Factory.Save(); // client needs to be saved to db
		var docket = Helper.CreateWhsWorkOrder(Client, Whs, "Ref1");

		var xsdDockets = new Xsd.WhsDockets();
		var xsdDocket = xsdDockets.WhsDocket.AddNew();
		PopulateXsdDocket(xsdDocket, Whs, Client, "Ref1");
		xsdDocket.Identifier.DocketType = "";

		var context = new ValueObjectImportContext(Factory, Notifications);
		var docketFound = Adapter.FindExistingBusinessObject(xsdDocket, context);
		AssertNull("Should not load a Docket of another Type.", docketFound);
	}

	#endregion

	#region TestCreateOrUpdateFromValueObject_ShouldCreateOrUpdateBusinessObject

	public override void TestCreateOrUpdateFromValueObject_ShouldCreateOrUpdateBusinessObject()
	{
		var docket = GetNewPopulatedDocket(Whs, Client, "Ref1");
		var xsdDocketTypeCode = GetXSDDocketTypeCode();
		var xsdOtherDocketTypeCode = new Warehouse.DataTransfer.CodeLists.DocketTypes().ToArray().First(c => c.Code != xsdDocketTypeCode).Code;

		var xsdDockets = new Xsd.WhsDockets();
		var xsdDocket = xsdDockets.WhsDocket.AddNew();
		PopulateXsdDocket(xsdDocket, Whs, Client, "Ref1");
		xsdDocket.Identifier.DocketType = xsdOtherDocketTypeCode;

		var collection = GetCollection();
		var context = new ValueObjectImportContext(Factory, Notifications);
		Adapter.ImportFromValueObject(collection, xsdDockets, context);

		AssertEquals("Precondition: No reocrds must be imported.", 0, collection.Count);
		var expectedErrorMsg = string.Format("Docket Type '{0}' is not valid for this import.", xsdOtherDocketTypeCode);
		AssertNotNull("Notifications should contain an ImportingDataError with the expected error message.", Notifications.GetEventsByType(ErrorType.ImportingDataError).Single(err => err.Message.Contains(expectedErrorMsg)));
	}

	#endregion

	#region TestImportOfExistingDocketWithLines

	public void TestImportOfExistingDocketWithLines()
	{
		if (IsImportFromValueObjectSupported)
		{
			var part1 = Helper.CreateProduct(Client, "PR1");
			var part2 = Helper.CreateProduct(Client, "PR2");

			var docket = GetNewPopulatedDocket(Whs, Client, "REF1");

			AddNewPopulatedLine(docket, 1, 0, part1, 90m);
			AddNewPopulatedLine(docket, 2, 1, part1, 40m);
			AddNewPopulatedLine(docket, 3, 0, part2, 0m);

			var collection = GetCollection();
			var xsdDockets = new Xsd.WhsDockets();
			var xsdDocket = xsdDockets.WhsDocket.AddNew();
			PopulateXsdDocket(xsdDocket, Whs, Client, "Ref1");

			var xsdLine10 = xsdDocket.DocketLines.AddNew();
			PopulateXsdDocketLine(xsdLine10, part1.OP_PartNum, 1, 0, 100m);

			var xsdLine11 = xsdDocket.DocketLines.AddNew();
			PopulateXsdDocketLine(xsdLine11, part1.OP_PartNum, 1, 2, 50m);

			var xsdLine20 = xsdDocket.DocketLines.AddNew();
			PopulateXsdDocketLine(xsdLine20, part2.OP_PartNum, 2, 0, 200m);

			var xsdLine30 = xsdDocket.DocketLines.AddNew();
			PopulateXsdDocketLine(xsdLine30, part2.OP_PartNum, 3, 0, 0m);

			Factory.Save();

			var context = new ValueObjectImportContext(Factory, Notifications);
			Adapter.ImportFromValueObject(collection, xsdDockets, context);

			AssertEquals("Collection should have a new element", 1, collection.Count);
			var updatedDocket = (TDocket)collection[0];
			AssertEquals("Wrong docket was updated", docket.WD_ExternalReference, updatedDocket.WD_ExternalReference);

			AssertEquals("Docket should have 5 lines", 5, updatedDocket.Lines.Count);
			AssertLineInfo(updatedDocket, 1, 0, 100m);
			AssertLineInfo(updatedDocket, 2, 1, 0m);
			AssertLineInfo(updatedDocket, 1, 2, 50m);
			AssertLineInfo(updatedDocket, 2, 0, 200m);
			AssertLineInfo(updatedDocket, 3, 0, 0m);
		}
		else
		{
			Assert("Not required", true);
		}
	}

	#endregion

	#region TestImportOfExistingDocketDoesNotMatchInactiveProducts

	public void TestImportOfExistingDocketDoesNotMatchInactiveProducts()
	{
		if (IsImportFromValueObjectSupported)
		{
			var docket = GetNewPopulatedDocket(Whs, Client, "REF1");
			var part = Helper.CreateProduct(Client, "Part1");
			var inactivePart = Helper.CreateProduct(Client, "Part2");
			inactivePart.OP_IsActive = false;

			AddNewPopulatedLine(docket, 1, 0, inactivePart, 90m);
			AddNewPopulatedLine(docket, 2, 0, part, 40m);

			var collection = GetCollection();
			var xsdDockets = new Xsd.WhsDockets();
			var xsdDocket = xsdDockets.WhsDocket.AddNew();
			PopulateXsdDocket(xsdDocket, Whs, Client, "Ref1");

			var xsdLineForInactive = xsdDocket.DocketLines.AddNew();
			PopulateXsdDocketLine(xsdLineForInactive, inactivePart.OP_PartNum, 1, 0, 100m);

			var xsdLineForActive = xsdDocket.DocketLines.AddNew();
			PopulateXsdDocketLine(xsdLineForActive, part.OP_PartNum, 2, 0, 200m);

			Factory.Save();

			var context = new ValueObjectImportContext(Factory, Notifications);
			Adapter.ImportFromValueObject(collection, xsdDockets, context);

			AssertEquals("Collection should have a new element", 1, collection.Count);

			var updatedDocket = (TDocket)collection[0];
			AssertEquals("Wrong docket was updated", docket.WD_ExternalReference, updatedDocket.WD_ExternalReference);
			AssertEquals("Docket should have 2 lines", 2, updatedDocket.Lines.Count);
			AssertLineInfo(updatedDocket, 1, 0, 100m);
			AssertLineInfo(updatedDocket, 2, 0, 200m);
			AssertEquals("Import had Error", true, context.NotificationsHasErrors);
			AssertMultilineASCIIEquals("Should not have matched InActive Product.", string.Format(@"
Successfully matched organization with code 'WHSFORSYD', Mapping Organization: EDICUS, Using: Similarity Matcher, Found match: True
Error: ERROR MESSAGE: (Product: PART2/Client: WHSClient for test could not be found. {0} no: REF1)
Successfully created new product 'PART1' for client 'WHSFORSYD'
{0} updated
				".Trim(), updatedDocket.HumanReadableName), Notifications.AsString);
		}
		else
		{
			Assert("Not required", true);
		}
	}

	#endregion

	#region DeleteOrIgnore

	protected enum DeleteOrIgnore
	{
		Delete, Ignore
	}

	#endregion

	#region TestImportOfExistingDocketWithLinesToDelete

	public void TestImportOfExistingDocketWithLinesToDelete()
	{
		if (IsImportFromValueObjectSupported)
		{
			ImportExistingDocketWithLinesToDeleteOrIgnore(DeleteOrIgnore.Delete);
		}
		else
		{
			Assert("Not required", true);
		}
	}

	#endregion

	#region TestImportOfExistingDocketWithLinesToIgnore

	public void TestImportOfExistingDocketWithLinesToIgnore()
	{
		if (IsImportFromValueObjectSupported)
		{
			ImportExistingDocketWithLinesToDeleteOrIgnore(DeleteOrIgnore.Ignore);
		}
		else
		{
			Assert("Not required", true);
		}
	}

	#endregion

	#region ImportExistingDocketWithLinesToDeleteOrIgnore

	protected void ImportExistingDocketWithLinesToDeleteOrIgnore(DeleteOrIgnore deleteOrIgnore)
	{
		var part1 = Helper.CreateProduct(Client, "PR1");
		var docket = GetNewPopulatedDocket(Whs, Client, "REF1");
		docket.Client.MiscServ.OM_WhsEDIChangeMessageCancelOrderLinesNotIncludedInMessage = (deleteOrIgnore == DeleteOrIgnore.Delete);

		AddNewPopulatedLine(docket, 1, 1, part1, 40m);
		AddNewPopulatedLine(docket, 2, 1, part1, 20m);

		var collection = GetCollection();
		var xsdDockets = new Xsd.WhsDockets();
		var xsdDocket = xsdDockets.WhsDocket.AddNew();
		PopulateXsdDocket(xsdDocket, Whs, Client, "REF1");

		var xsdLine1 = xsdDocket.DocketLines.AddNew();
		PopulateXsdDocketLine(xsdLine1, part1.OP_PartNum, 1, 1, 40m);

		Factory.Save();

		var context = new ValueObjectImportContext(Factory, Notifications);
		Adapter.ImportFromValueObject(collection, xsdDockets, context);

		AssertEquals("Collection should have a new element", 1, collection.Count);
		var updatedDocket = (TDocket)collection[0];

		AssertEquals("Docket should have 2 lines", 2, updatedDocket.Lines.Count);
		AssertLineInfo(updatedDocket, 1, 1, 40m);
		AssertLineInfo(updatedDocket, 2, 1, deleteOrIgnore == DeleteOrIgnore.Delete ? 0m : 20m);
	}

	#endregion

	#region TestImportOfExistingNonUpdatableDocketWithLines

	public void TestImportOfExistingNonUpdatableDocketWithLines()
	{
		if (IsImportFromValueObjectSupported)
		{
			var part1 = Helper.CreateProduct(Client, "PR1");
			var part2 = Helper.CreateProduct(Client, "PR2");

			var docket = GetNewPopulatedDocket(Whs, Client, "REF1");

			AddNewPopulatedLine(docket, 1, 0, part1, 90m);
			AddNewPopulatedLine(docket, 1, 1, part1, 40m);
			AddNewPopulatedLine(docket, 2, 0, part2, 0m);

			var collection = GetCollection();
			var xsdDockets = new Xsd.WhsDockets();
			var xsdDocket = xsdDockets.WhsDocket.AddNew();
			PopulateXsdDocket(xsdDocket, Whs, Client, "Ref1");

			var xsdLine10 = xsdDocket.DocketLines.AddNew();
			PopulateXsdDocketLine(xsdLine10, part1.OP_PartNum, 1, 0, 100m);

			var xsdLine11 = xsdDocket.DocketLines.AddNew();
			PopulateXsdDocketLine(xsdLine11, part1.OP_PartNum, 1, 2, 50m);

			var xsdLine20 = xsdDocket.DocketLines.AddNew();
			PopulateXsdDocketLine(xsdLine20, part2.OP_PartNum, 2, 0, 200m);

			var xsdLine30 = xsdDocket.DocketLines.AddNew();
			PopulateXsdDocketLine(xsdLine30, part2.OP_PartNum, 3, 0, 0m);

			Factory.Save();
			ValueObjectImportContext context;
			var nonUpdatableStatuses = GetNonUpdatableStatuses();

			foreach (CodeDescriptionPair nonUpdatableStatus in nonUpdatableStatuses)
			{
				docket.WD_DocketStatus = nonUpdatableStatus.Code;

				collection = GetCollection();
				Notifications.Clear();
				context = new ValueObjectImportContext(Factory, Notifications);
				Adapter.ImportFromValueObject(collection, xsdDockets, context);

				AssertEquals("Collection should have a new element", 1, collection.Count);
				var nonUpdatableDocket = (TDocket)collection[0];
				AssertEquals("Wrong docket was selected for update", docket.WD_ExternalReference, nonUpdatableDocket.WD_ExternalReference);
				AssertEquals("Docket status was modified", nonUpdatableStatus.Code, nonUpdatableDocket.WD_DocketStatus);

				// Ensure we have an importing data error with the correct message
				var expectedErrorMsg = GetExpectedBizObjReadableName(nonUpdatableDocket) + ": " + nonUpdatableDocket.WD_ExternalReference + " with Status " + nonUpdatableDocket.WD_DocketStatusDescription + " can not be modified.";
				var errorMsg = Notifications.GetEventsByType(ErrorType.DataErrorPreventSave).FirstOrDefault(err => err.Message.Contains(expectedErrorMsg));
				AssertNotNull("Notifications should contain an ImportingDataError with the expected error message.", errorMsg);

				AssertEquals("Docket should have original 3 lines", 3, nonUpdatableDocket.Lines.Count);
				AssertLineInfo(nonUpdatableDocket, 1, 0, 90m);
				AssertLineInfo(nonUpdatableDocket, 1, 1, 40m);
				AssertLineInfo(nonUpdatableDocket, 2, 0, 0m);
			}
		}
		else
		{
			Assert("Not required", true);
		}
	}

	#endregion

	#region TestImportOfExistingDocketNoLines

	public void TestImportOfExistingDocketNoLines()
	{
		if (IsImportFromValueObjectSupported)
		{
			var part1 = Helper.CreateProduct(Client, "PR1");
			var part2 = Helper.CreateProduct(Client, "PR2");

			var docket = GetNewPopulatedDocket(Whs, Client, "REF1");

			AddNewPopulatedLine(docket, 1, 0, part1, 90m);
			AddNewPopulatedLine(docket, 2, 1, part1, 40m);
			AddNewPopulatedLine(docket, 3, 0, part2, 0m);

			var collection = GetCollection();
			var xsdDockets = new Xsd.WhsDockets();
			var xsdDocket = xsdDockets.WhsDocket.AddNew();
			PopulateXsdDocket(xsdDocket, Whs, Client, "REF1");

			Factory.Save();

			var context = new ValueObjectImportContext(Factory, Notifications);
			Adapter.ImportFromValueObject(collection, xsdDockets, context);

			AssertEquals("Collection should have a new element", 1, collection.Count);
			var updatedDocket = (TDocket)collection[0];
			AssertEquals("Wrong docket was updated", docket.WD_ExternalReference, updatedDocket.WD_ExternalReference);

			AssertEquals("Docket should have status 'Cancelled'", true, updatedDocket.IsCancelled);
			AssertEquals("Docket should still have 3 lines", 3, updatedDocket.Lines.Count);

			AssertLineInfo(updatedDocket, 1, 0, 0m);
			AssertLineInfo(updatedDocket, 2, 1, 0m);
			AssertLineInfo(updatedDocket, 3, 0, 0m);
		}
		else
		{
			Assert("Not required", true);
		}
	}

	#endregion

	#region TestImportOfLineProducts

	#region TestImportOfLine_NotAllowCreateNewProduct

	public void TestImportOfLine_NotAllowCreateNewProduct()
	{
		if (IsImportFromValueObjectSupported)
		{
			var allowCreateNewProduct = SystemDataRegistry.Instance.CreateMissingWarehouseProduct.Value;          //default value should be false, i.e. not allow to create new products;
			var collection = ImportTestDocketLineWithNonExistingProduct(allowCreateNewProduct);

			var updatedDocket = (TDocket)collection[0];
			var updatedDocketLine = updatedDocket.Lines[0];
			var invalidPart = updatedDocketLine.SupplierPart;
			var invalidPartRelation = invalidPart.RelatedOrganisations[0];

			AssertEquals("Import should have created 1 Docket.", 1, collection.Count);
			AssertEquals(1, updatedDocket.Lines.Count);
			AssertLineInfo(updatedDocket, 1, 0, 200m);
			AssertEquals(ProductType.Codes.Invalid, updatedDocketLine.SupplierPart.OP_PartNum);
			AssertEquals(ProductType.Descriptions.Invalid, updatedDocketLine.SupplierPart.OP_Desc);
			AssertEquals("UNT", updatedDocketLine.SupplierPart.OP_StockKeepingUnit);
			AssertEquals(1, invalidPart.RelatedOrganisations.Count);
			AssertEquals(Client, invalidPartRelation.Organisation);
			AssertEquals(OrgPartRelation.RelationshipTypes.Owner, invalidPartRelation.OU_Relationship);
		}
		else
		{
			Assert("Not required", true);
		}
	}

	#endregion

	#region TestImportOfLine_AllowCreateNewProduct

	public void TestImportOfLine_AllowCreateNewProduct()
	{
		if (IsImportFromValueObjectSupported)
		{
			var collection = ImportTestDocketLineWithNonExistingProduct(true);
			var updatedDocket = (TDocket)collection[0];
			var updatedDocketLine = updatedDocket.Lines[0];
			var newPart = updatedDocket.Lines[0].SupplierPart;
			var newPartRelation = newPart.RelatedOrganisations[0];

			AssertEquals("Import should have created 1 Docket.", 1, collection.Count);
			AssertEquals(1, updatedDocket.Lines.Count);
			AssertLineInfo(updatedDocket, 1, 0, 200m);
			AssertEquals("TESTPRODUCT", updatedDocketLine.SupplierPart.OP_PartNum);
			AssertEquals("TEST PRODUCT", updatedDocketLine.SupplierPart.OP_Desc);
			AssertEquals("BAG", updatedDocketLine.SupplierPart.OP_StockKeepingUnit);
			AssertEquals(1, newPart.RelatedOrganisations.Count);
			AssertEquals(Client, newPartRelation.Organisation);
			AssertEquals(OrgPartRelation.RelationshipTypes.Owner, newPartRelation.OU_Relationship);
		}
		else
		{
			Assert("Not required", true);
		}
	}

	#endregion

	#region TestImportOfLine_WithExistingProductDifferentClient

	public void TestImportOfLine_WithExistingProductDifferentClient()
	{
		if (IsImportFromValueObjectSupported)
		{
			var client2 = Helper.CreateClient("CLT2", "Client 2");
			client2.Addresses.MainAddress.OA_Address1 = "ClientAddress2";
			client2.OH_RL_NKClosestPort = "AUSYD";

			var part1 = Helper.CreateProduct(client2, "TESTPRODUCT");
			part1.OP_Desc = "EXISTING PRODUCT";

			AssertEquals("Only 1 'TESTPRODUCT' record before importing", 1, FindCollectionByProductCode("TESTPRODUCT").Length);

			var collection = ImportTestDocketLineWithNonExistingProduct(true);
			var updatedDocket = (TDocket)collection[0];
			var updatedDocketLine = updatedDocket.Lines[0];
			var newPart = updatedDocket.Lines[0].SupplierPart;
			var newPartRelation = newPart.RelatedOrganisations[0];

			AssertEquals("Import should have created 1 Docket.", 1, collection.Count);
			AssertEquals(1, updatedDocket.Lines.Count);
			AssertLineInfo(updatedDocket, 1, 0, 200m);
			AssertEquals("TESTPRODUCT", updatedDocketLine.SupplierPart.OP_PartNum);
			AssertEquals("TEST PRODUCT", updatedDocketLine.SupplierPart.OP_Desc);
			AssertEquals("BAG", updatedDocketLine.SupplierPart.OP_StockKeepingUnit);

			AssertEquals("2 'TESTPRODUCT' records after importing", 2, FindCollectionByProductCode("TESTPRODUCT").Length);
			AssertEquals(part1.OP_PartNum, newPart.OP_PartNum);
			AssertEquals("TEST PRODUCT", newPart.OP_Desc);
			AssertEquals("BAG", newPart.OP_StockKeepingUnit);
			AssertEquals(Client, newPartRelation.Organisation);
			AssertEquals("EXISTING PRODUCT", part1.OP_Desc);
			AssertEquals("UNT", part1.OP_StockKeepingUnit);
			AssertEquals(client2, part1.RelatedOrganisations[0].Organisation);
			AssertEquals(part1.RelatedOrganisations[0].OU_Relationship, newPartRelation.OU_Relationship);
		}
		else
		{
			Assert("Not required", true);
		}
	}

	#endregion

	#region ImportTestDocketLineWithNonExistingProduct

	WhsDocketCollection ImportTestDocketLineWithNonExistingProduct(bool allowCreateNewProduct)
	{
		SystemDataRegistry.Instance.CreateMissingWarehouseProduct.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, allowCreateNewProduct);

		var collection = GetCollection();
		var xsdDockets = new Xsd.WhsDockets();
		var xsdDocket = xsdDockets.WhsDocket.AddNew();
		PopulateXsdDocket(xsdDocket, Whs, Client, "Ref1");

		var xsdLine1 = xsdDocket.DocketLines.AddNew();
		xsdLine1.Description = "TEST PRODUCT";
		PopulateXsdDocketLine(xsdLine1, "TESTPRODUCT", 1, 0, 200m);
		xsdLine1.ProductUQ = "BAG"; // should be after PopulateXsdDocketLine(...)

		Factory.Save();

		var context = new ValueObjectImportContext(Factory, Notifications);
		Adapter.ImportFromValueObject(collection, xsdDockets, context);

		return collection;
	}

	#endregion

	#region TestImportOfPackageType

	#region TestImportOfPackageType_WithValidCode

	public void TestImportOfPackageType_WithValidCode()
	{
		if (IsImportFromValueObjectSupported)
		{
			var collection = GetCollection();
			var xsdDockets = GetPopulatedXsdDocketsForPackageUnitImport("PLT");

			// a code map is set up so we can be sure its not used in the case of a valid package type
			CreateOrgCodeMapping(GlbCompany.CurrentCompany.OrgProxy.PK, Constants.OrgPatternMatchOverrideRelationships.PackageType, "PLT", "BAG");

			Adapter.ImportFromValueObject(collection, xsdDockets, Context);
			AssertEquals("Package type in import file is a standard one and should be used", "PLT", collection[0].Lines[0].WE_F3_NKPackType);
		}
		else
		{
			Assert("Not required", true);
		}
	}

	#endregion

	#region TestImportOfPackageType_WithInvalidCodeAndNoCodeMaps

	public void TestImportOfPackageType_WithInvalidCodeAndNoCodeMaps()
	{
		if (IsImportFromValueObjectSupported)
		{
			var collection = GetCollection();
			var xsdDockets = GetPopulatedXsdDocketsForPackageUnitImport("XXX");

			Adapter.ImportFromValueObject(collection, xsdDockets, Context);
			var expectedWarningMessage = $"Pack Type (XXX) is not valid. When you edit this record, the pack type field will display a warning. To avoid this warning you can either use Code Mapping to map this value to the appropriate {Core.Constants.ProductName} package type or you can add this value to the reference files (Reference Files -> Package Types)";
			AssertContains("Invalid Package Warning not found", expectedWarningMessage, Notifications.AsString);
			AssertEquals("Invalid package type is used as no code mapping found", "XXX", collection[0].Lines[0].WE_F3_NKPackType);
		}
		else
		{
			Assert("Not required", true);
		}
	}

	#endregion

	#region TestImportOfPackageType_WithInvalidCodeAndCodeMappingOrgSetInXMLInterchange

	public void TestImportOfPackageType_WithInvalidCodeAndCodeMappingOrgSetInXMLInterchange()
	{
		if (IsImportFromValueObjectSupported)
		{
			var collection = GetCollection();
			var xsdDockets = GetPopulatedXsdDocketsForPackageUnitImport("XXX");

			CreateOrgCodeMapping(Client.PK, Constants.OrgPatternMatchOverrideRelationships.PackageType, "XXX", "PLT");
			CreateOrgCodeMapping(GlbCompany.CurrentCompany.OrgProxy.PK, Constants.OrgPatternMatchOverrideRelationships.PackageType, "XXX", "UNT");

			var interchange = new Xsd.XmlInterchange();
			var xsdOrg = new Xsd.Organisation();
			xsdOrg.EDICode = Client.OH_Code;
			interchange.InterchangeInfo.EDIOrganisation = xsdOrg;
			var context = new ValueObjectImportContext(Factory, interchange, Notifications);

			Adapter.ImportFromValueObject(collection, xsdDockets, context);
			AssertEquals("Code mapping from client should be used as org in Interchange takes precedence over Org Proxy", "PLT", collection[0].Lines[0].WE_F3_NKPackType);
		}
		else
		{
			Assert("Not required", true);
		}
	}

	#endregion

	#region TestImportOfPackageType_WithInvalidCodeAndCodeMappingOnOrgProxy

	public void TestImportOfPackageType_WithInvalidCodeAndCodeMappingOnOrgProxy()
	{
		if (IsImportFromValueObjectSupported)
		{
			var collection = GetCollection();
			var xsdDockets = GetPopulatedXsdDocketsForPackageUnitImport("ZZZ");

			CreateOrgCodeMapping(GlbCompany.CurrentCompany.OrgProxy.PK, Constants.OrgPatternMatchOverrideRelationships.PackageType, "ZZZ", "UNT");

			Adapter.ImportFromValueObject(collection, xsdDockets, Context);
			AssertEquals("Code mapping on Org Proxy should be used", "UNT", collection[0].Lines[0].WE_F3_NKPackType);
		}
		else
		{
			Assert("Not required", true);
		}
	}

	#endregion

	#region TestImportOfPackageType_WithInvalidCodeAndMappedCodeIsTooLong

	public void TestImportOfPackageType_WithInvalidCodeAndMappedCodeIsTooLong()
	{
		if (IsImportFromValueObjectSupported)
		{
			var collection = GetCollection();
			var xsdDockets = GetPopulatedXsdDocketsForPackageUnitImport("ZZZ");
			CreateOrgCodeMapping(GlbCompany.CurrentCompany.OrgProxy.PK, Constants.OrgPatternMatchOverrideRelationships.PackageType, "ZZZ", "TOO-LONG");

			Adapter.ImportFromValueObject(collection, xsdDockets, Context);
			string expectedWarningMessage =
				"Pack Type (TOO-LONG) has exceeded the maximum length allowed by the system. When you edit this record, " +
				$"the package type field will display a warning. Please use Code Mapping to map this value to the appropriate {Core.Constants.ProductName} package type.";
			Assert(string.Format("Invalid Package Warning - [{0}] - not found. Error messages found were:\r\n\r\n{1}", expectedWarningMessage, Notifications.AsString),
				Notifications.AsString.Contains(expectedWarningMessage));
			AssertEquals("TOO", collection[0].Lines[0].WE_F3_NKPackType);
		}
		else
		{
			Assert("Not required", true);
		}
	}

	#endregion

	Xsd.WhsDockets GetPopulatedXsdDocketsForPackageUnitImport(string productUQ)
	{
		var result = new Xsd.WhsDockets();
		var xsdDocket = result.WhsDocket.AddNew();
		PopulateXsdDocket(xsdDocket, Whs, Client, "Ref1");

		var xsdLine1 = xsdDocket.DocketLines.AddNew();
		PopulateXsdDocketLine(xsdLine1, "P1", 1, 0, 200m);
		xsdLine1.ProductUQ = productUQ;

		return result;
	}

	void CreateOrgCodeMapping(ZGuid orgPK, string relationshipType, string foreignCode, string localCode)
	{
		var orgProxyCodeMap = Factory.New<OrgPatternMatchOverride>();
		orgProxyCodeMap.OO_OH = orgPK;
		orgProxyCodeMap.OO_Relationship = relationshipType;
		orgProxyCodeMap.OO_ForeignCode = foreignCode;
		orgProxyCodeMap.OO_LocalCode = localCode;
	}

	#endregion

	#region FindCollectionByProductCode

	OrgSupplierPart[] FindCollectionByProductCode(ZString productPartNum)
	{
		var productQuery = new ZQuery();
		productQuery.AddToFilter(OrgSupplierPartSchema.OP_PartNum, productPartNum);

		var productCollection = Factory.Load<OrgSupplierPart>(productQuery);
		return productCollection;
	}

	#endregion

	#endregion

	#region GetNonUpdatableStatuses

	protected virtual CodeDescriptionPairList GetNonUpdatableStatuses()
	{
		var allStatuses = new DocketStatus();
		var nonUpdatableStatuses = new DocketStatus();
		foreach (CodeDescriptionPair status in allStatuses)
		{
			if (status.Code == DocketStatus.Codes.Entered ||
				status.Code == DocketStatus.Codes.New ||
				status.Code == DocketStatus.Codes.Error ||
				status.Code == DocketStatus.Codes.AttachedToPick ||
				status.Code == DocketStatus.Codes.Held)
			{
				nonUpdatableStatuses.Remove(status);
			}
		}
		return nonUpdatableStatuses;
	}

	#endregion

	#region GetExpectedBizObjReadableName

	protected virtual ZString GetExpectedBizObjReadableName(TDocket bizObj)
	{
		return bizObj.HumanReadableName;
	}

	#endregion

	#region AssertLineInfo

	protected virtual void AssertLineInfo(TDocket docket, ZShort lineNo, ZShort subLineNo, ZDecimal units)
	{
		WhsDocketLine lineFound = null;
		foreach (var line in docket.Lines)
		{
			if (line.WE_LineNo == lineNo && line.WE_SubLineNo == subLineNo)
			{
				lineFound = line;
				break;
			}
		}
		AssertNotNull(string.Format("Line was not found: LineNo: {0}, SubLineNo: {1}", lineNo, subLineNo), lineFound);
		AssertLineInfoQuantity(lineFound, units);
	}

	#endregion

	#region AssertLineInfoQuantity

	protected virtual void AssertLineInfoQuantity(WhsDocketLine line, ZDecimal units)
	{
		AssertEquals("Line Packs don't match", units, line.WE_PackQuantity);
	}

	#endregion

	#region AddNewPopulatedLine

	protected virtual WhsDocketLine AddNewPopulatedLine(TDocket docket, ZShort lineNo, ZShort subLineNo, OrgSupplierPart part, ZDecimal units)
	{
		var line = docket.Lines.AddNew();
		line.WE_OP = part.PK;
		line.WE_LineNo = lineNo;
		line.WE_SubLineNo = subLineNo;
		line.WE_TransactionQuantity = units;
		return line;
	}

	#endregion

	#region PopulateXsdDocket

	protected virtual void PopulateXsdDocket(Xsd.WhsDocket xsdDocket, WhsWarehouse warehouse, OrgHeader client, ZString reference)
	{
		xsdDocket.DocketDetail.WarehouseCode = warehouse.WW_WarehouseCode;
		PopulateOrg(xsdDocket.Identifier.Client, client.OH_FullName, client.MainAddress.OA_Address1, client.UNLOCO.RL_Code);
		xsdDocket.Identifier.Reference = reference;
	}

	#endregion

	#region PopulateXsdDocketLine

	protected virtual void PopulateXsdDocketLine(Xsd.WhsDocketLine xsdLine, ZString productCode, ZShort lineNo, ZShort subLineNo, ZDecimal units)
	{
		xsdLine.Product = productCode;
		xsdLine.ProductUQ = "UNT";
		xsdLine.LineNumber = lineNo;
		xsdLine.SubLineNumber = subLineNo;
		xsdLine.QuantityActuallyOrdered = units;
	}

	#endregion

	protected abstract TDocket GetNewPopulatedDocket(WhsWarehouse whs, OrgHeader client, ZString reference);

	protected abstract ZString GetXSDDocketTypeCode();

	#region TestImportLineAttributesWhenAttributesNotSet

	public void TestImportLineAttributesWhenAttributesNotSet()
	{
		if (IsImportFromValueObjectSupported)
		{
			SetupData();
			var collection = GetCollection();
			var xsdDockets = new Xsd.WhsDockets();
			PopulateXsdDockets(xsdDockets);
			var context = new ValueObjectImportContext(Factory, Notifications);
			TDocket whsDocket;

			foreach (AttributeNumber attribNo in Enum.GetValues(typeof(AttributeNumber)))
			{
				Helper.SetProductAttributeUse(Client, Part, attribNo, false);
				Factory.Save();
			}

			Adapter.ImportFromValueObject(collection, xsdDockets, context);
			AssertEquals("Collection should have a new element", 1, collection.Count);
			whsDocket = (TDocket)collection[0];
			AssertEquals("Imported Docket should have 1 line", 1, whsDocket.Lines.Count);
			AssertEquals("incorrect WE_ExpiryDate", ZDateTime.Empty, whsDocket.Lines[0].WE_ExpiryDate);
			AssertEquals("incorrect WE_PackingDate", ZDateTime.Empty, whsDocket.Lines[0].WE_PackingDate);
			AssertEquals("incorrect WE_PartAttrib1", ZString.Empty, whsDocket.Lines[0].WE_PartAttrib1);
			AssertEquals("incorrect WE_PartAttrib2", ZString.Empty, whsDocket.Lines[0].WE_PartAttrib2);
			AssertEquals("incorrect WE_PartAttrib3", ZString.Empty, whsDocket.Lines[0].WE_PartAttrib3);
			AssertEquals("incorrect WE_SerialNumber", ZString.Empty, whsDocket.Lines[0].WE_SerialNumber);
		}
		else
		{
			Assert("Not Required", true);
		}
	}

	#endregion

	#region TestImportLineAttributesWhenAttributesNonMandatory

	public void TestImportLineAttributesWhenAttributesNonMandatory()
	{
		if (IsImportFromValueObjectSupported)
		{
			SetupData();
			var collection = GetCollection();
			var xsdDockets = new Xsd.WhsDockets();
			PopulateXsdDockets(xsdDockets);
			var context = new ValueObjectImportContext(Factory, Notifications);

			foreach (AttributeNumber attribNo in Enum.GetValues(typeof(AttributeNumber)))
			{
				Helper.SetClientAttributeType(Client, attribNo, false);
				Helper.SetProductAttributeUse(Client, Part, attribNo, true);
				Factory.Save();
			}

			Adapter.ImportFromValueObject(collection, xsdDockets, context);

			AssertEquals("Collection should have a new element", 1, collection.Count);
			TDocket whsDocket = (TDocket)collection[0];
			AssertEquals("Imported Docket should have 1 line", 1, whsDocket.Lines.Count);
			AssertEquals("incorrect WE_ExpiryDate", ZDateTime.Empty, whsDocket.Lines[0].WE_ExpiryDate);
			AssertEquals("incorrect WE_PackingDate", ZDateTime.Empty, whsDocket.Lines[0].WE_PackingDate);
			AssertEquals("incorrect WE_PartAttrib1", "ATTR1", whsDocket.Lines[0].WE_PartAttrib1);
			AssertEquals("incorrect WE_PartAttrib2", "ATTR2", whsDocket.Lines[0].WE_PartAttrib2);
			AssertEquals("incorrect WE_PartAttrib3", "ATTR3", whsDocket.Lines[0].WE_PartAttrib3);
		}
		else
		{
			Assert("Not Required", true);
		}
	}

	#endregion

	#region TestImportLineAttributesWhenAttributesMandatory

	public void TestImportLineAttributesWhenAttributesMandatory()
	{
		if (IsImportFromValueObjectSupported)
		{
			SetupData();
			var collection = GetCollection();
			var xsdDockets = new Xsd.WhsDockets();
			PopulateXsdDockets(xsdDockets);
			var context = new ValueObjectImportContext(Factory, Notifications);

			foreach (AttributeNumber attribNo in Enum.GetValues(typeof(AttributeNumber)))
			{
				Helper.SetClientAttributeType(Client, attribNo, true);
				Helper.SetProductAttributeUse(Client, Part, attribNo, true);
				Factory.Save();
			}

			Adapter.ImportFromValueObject(collection, xsdDockets, context);

			AssertEquals("Collection should have a new element", 1, collection.Count);
			TDocket whsDocket = (TDocket)collection[0];
			AssertEquals("Imported Docket should have 1 line", 1, whsDocket.Lines.Count);
			AssertEquals("incorrect WE_ExpiryDate", new ZDateTime(2006, 10, 17), whsDocket.Lines[0].WE_ExpiryDate);
			AssertEquals("incorrect WE_PackingDate", new ZDateTime(2006, 10, 16), whsDocket.Lines[0].WE_PackingDate);
			AssertEquals("incorrect WE_PartAttrib1", "ATTR1", whsDocket.Lines[0].WE_PartAttrib1);
			AssertEquals("incorrect WE_PartAttrib2", "ATTR2", whsDocket.Lines[0].WE_PartAttrib2);
			AssertEquals("incorrect WE_PartAttrib3", "ATTR3", whsDocket.Lines[0].WE_PartAttrib3);
		}
		else
		{
			Assert("Not Required", true);
		}
	}

	#endregion

	#region TestImportWithBOTHClientPartRelation

	public void TestImportWithBOTHClientPartRelation()
	{
		if (IsImportFromValueObjectSupported)
		{
			SetupData(OrgPartRelation.RelationshipTypes.Both);

			// turn on attributes
			Helper.EnableWarehouseForBond(Whs, true);

			foreach (AttributeNumber attribNo in Enum.GetValues(typeof(AttributeNumber)))
			{
				Helper.SetClientAttributeType(Client, attribNo, true);
				Helper.SetProductAttributeUse(Client, Part, attribNo, true);
			}
			Factory.Save();

			var collection = GetCollection();
			var xsdDockets = new Xsd.WhsDockets();
			PopulateXsdDockets(xsdDockets);

			var context = new ValueObjectImportContext(Factory, Notifications);
			Adapter.ImportFromValueObject(collection, xsdDockets, context);

			AssertEquals("Collection should have a new element", 1, collection.Count);
			var whsDocket = (TDocket)collection[0];

			AssertEquals(GetExternalReference(), whsDocket.WD_ExternalReference);
			AssertEquals(Client.PK, whsDocket.WD_OH_Client);

			AssertEquals(1, whsDocket.Lines.Count);
			var whsDocketLine = whsDocket.Lines[0];
			AssertNotNull(whsDocketLine.SupplierPart);
			AssertEquals("PRODUCT", whsDocketLine.SupplierPart.OP_PartNum);
			AssertNotNull(whsDocketLine.SupplierPart.RelatedOrganisations.FindByOrganisationPKAndRelationship(Client.PK, OrgPartRelation.RelationshipTypes.Both));
			AssertQuantity(whsDocketLine);
			AssertEquals("COMMENTS", whsDocketLine.WE_LineComment);
			AssertEquals("BONDEDKEY", whsDocketLine.WE_BondedEntryKey);
			AssertEquals(new ZDateTime(2006, 10, 17), whsDocketLine.WE_ExpiryDate);
			AssertEquals(new ZDateTime(2006, 10, 16), whsDocketLine.WE_PackingDate);
			AssertEquals("ATTR1", whsDocketLine.WE_PartAttrib1);
			AssertEquals("ATTR2", whsDocketLine.WE_PartAttrib2);
			AssertEquals("ATTR3", whsDocketLine.WE_PartAttrib3);
		}
		else
		{
			Assert("Not required", true);
		}
	}

	#endregion

	#region TestImport

	public void TestImport()
	{
		if (IsImportFromValueObjectSupported)
		{
			SetupData();

			// turn on attributes
			Helper.EnableWarehouseForBond(Whs, true);

			foreach (AttributeNumber attribNo in Enum.GetValues(typeof(AttributeNumber)))
			{
				Helper.SetClientAttributeType(Client, attribNo, true);
				Helper.SetProductAttributeUse(Client, Part, attribNo, true);
			}
			Factory.Save();

			var collection = GetCollection();
			var xsdDockets = new Xsd.WhsDockets();
			PopulateXsdDockets(xsdDockets);

			var context = new ValueObjectImportContext(Factory, Notifications);
			Adapter.ImportFromValueObject(collection, xsdDockets, context);

			AssertEquals("Collection should have a new element", 1, collection.Count);
			var whsDocket = (TDocket)collection[0];

			AssertEquals(GetExternalReference(), whsDocket.WD_ExternalReference);
			AssertEquals(Client.PK, whsDocket.WD_OH_Client);
			AssertEquals(Whs.PK, whsDocket.WD_WW_Whs);

			AssertEquals("CustomAttrib1", whsDocket.WD_CustomAttrib1);
			AssertEquals("CustomAttrib2", whsDocket.WD_CustomAttrib2);
			AssertEquals("CustomAttrib3", whsDocket.WD_CustomAttrib3);
			AssertEquals("CustomAttrib4", whsDocket.WD_CustomAttrib4);
			AssertEquals("CustomAttrib5", whsDocket.WD_CustomAttrib5);

			AssertEquals(new ZDateTime(2008, 01, 02), whsDocket.WD_CustomDate1);
			AssertEquals(new ZDateTime(2008, 12, 31), whsDocket.WD_CustomDate2);

			AssertEquals(0m, whsDocket.WD_CustomDecimal1);
			AssertEquals(12.123m, whsDocket.WD_CustomDecimal2);
			AssertEquals(10000m, whsDocket.WD_CustomDecimal3);
			AssertEquals(233m, whsDocket.WD_CustomDecimal4);
			AssertEquals(0.009m, whsDocket.WD_CustomDecimal5);

			AssertEquals(false, whsDocket.WD_CustomFlag1);
			AssertEquals(false, whsDocket.WD_CustomFlag2);
			AssertEquals(true, whsDocket.WD_CustomFlag3);
			AssertEquals(false, whsDocket.WD_CustomFlag4);
			AssertEquals(true, whsDocket.WD_CustomFlag5);

			AssertEquals("Should have 2 references", 2, whsDocket.References.Count);
			AssertEquals("BOL", whsDocket.References[0].WX_RefType);
			AssertEquals("TEST1", whsDocket.References[0].WX_Reference);

			AssertEquals("HBL", whsDocket.References[1].WX_RefType);
			AssertEquals("TEST2", whsDocket.References[1].WX_Reference);

			AssertEquals("Should have 2 containers", 2, whsDocket.Containers.Count);
			AssertEquals("C1", whsDocket.Containers[0].WC_ContainerNum);
			AssertEquals(ZBool.True, whsDocket.Containers[0].WC_IsChargeable);
			AssertEquals(ZBool.True, whsDocket.Containers[0].WC_IsPalletised);
			AssertEquals(10, whsDocket.Containers[0].WC_ItemCount);
			AssertEquals(100, whsDocket.Containers[0].WC_PalletCount);
			AssertNotNull(whsDocket.Containers[0].Container);
			AssertEquals("TST10FT", whsDocket.Containers[0].Container.RC_Code);
			AssertEquals("S1", whsDocket.Containers[0].WC_SealNum);

			AssertEquals("C2", whsDocket.Containers[1].WC_ContainerNum);
			AssertEquals(ZBool.False, whsDocket.Containers[1].WC_IsChargeable);
			AssertEquals(ZBool.False, whsDocket.Containers[1].WC_IsPalletised);
			AssertEquals(20, whsDocket.Containers[1].WC_ItemCount);
			AssertEquals(200, whsDocket.Containers[1].WC_PalletCount);
			AssertNull(whsDocket.Containers[1].Container);
			AssertEquals("S2", whsDocket.Containers[1].WC_SealNum);

			if (whsDocket is IJobWithTransportCompany job)
			{
				AssertNotNull(job.TransportCoDocAddress.Organisation);
				AssertEquals("ALL Transport AU", job.TransportCoDocAddress.Organisation.OH_FullName);
			}

			AssertNotNull(whsDocket.TransportBillToDocAddress.Organisation);
			AssertEquals("Transport Billed To", whsDocket.TransportBillToDocAddress.Organisation.OH_FullName);

			AssertEquals("TRREF", whsDocket.WD_TransportReference);
			AssertEquals("D2D", whsDocket.WD_PL_NKCarrierServiceLevel);
			AssertEquals("TSL", whsDocket.WD_RS_NKServiceLevel);

			AssertContainsNote(whsDocket.Notes, PredefinedNoteTypes.Instance.HandlingInstructions);
			AssertContainsNote(whsDocket.Notes, PredefinedNoteTypes.Instance.DangerousGoodsAdditionalHandlingInformation);
			AssertContainsNote(whsDocket.Notes, PredefinedNoteTypes.Instance.DeliveryInstructionsNote);

			AssertEquals("BCD", whsDocket.WD_F3_NKTotalPackType);
			AssertEquals(12, whsDocket.WD_PackagesSent);

			AssertWhsDocketOtherDetail(whsDocket);

			AssertEquals(1, whsDocket.Lines.Count);
			var whsDocketLine = whsDocket.Lines[0];
			AssertNotNull(whsDocketLine.SupplierPart);
			AssertEquals("PRODUCT", whsDocketLine.SupplierPart.OP_PartNum);
			AssertNotNull(whsDocketLine.SupplierPart.RelatedOrganisations.FindByOrganisationPKAndRelationship(Client.PK, OrgPartRelation.RelationshipTypes.Owner));
			AssertQuantity(whsDocketLine);
			AssertEquals("COMMENTS", whsDocketLine.WE_LineComment);
			AssertEquals("BONDEDKEY", whsDocketLine.WE_BondedEntryKey);
			AssertEquals(new ZDateTime(2006, 10, 17), whsDocketLine.WE_ExpiryDate);
			AssertEquals(new ZDateTime(2006, 10, 16), whsDocketLine.WE_PackingDate);
			AssertEquals("ATTR1", whsDocketLine.WE_PartAttrib1);
			AssertEquals("ATTR2", whsDocketLine.WE_PartAttrib2);
			AssertEquals("ATTR3", whsDocketLine.WE_PartAttrib3);
			AssertEquals("CATTR1", whsDocketLine.WE_CustomAttrib1);
			AssertEquals("CATTR2", whsDocketLine.WE_CustomAttrib2);
			AssertEquals("CATTR3", whsDocketLine.WE_CustomAttrib3);
			AssertEquals("CATTR4", whsDocketLine.WE_CustomAttrib4);
			AssertEquals("CATTR5", whsDocketLine.WE_CustomAttrib5);
			AssertEquals("CATTR6", whsDocketLine.WE_CustomAttrib6);
			AssertEquals(new ZDateTime(2008, 06, 01), whsDocketLine.WE_CustomDate1);
			AssertEquals(new ZDateTime(2008, 06, 02), whsDocketLine.WE_CustomDate2);
			AssertEquals(new ZDateTime(2008, 06, 03), whsDocketLine.WE_CustomDate3);
			AssertEquals(new ZDateTime(2008, 06, 04), whsDocketLine.WE_CustomDate4);
			AssertEquals(new ZDateTime(2008, 06, 05), whsDocketLine.WE_CustomDate5);
			AssertEquals(190.8m, whsDocketLine.WE_CustomDecimal1);
			AssertEquals(23m, whsDocketLine.WE_CustomDecimal2);
			AssertEquals(0m, whsDocketLine.WE_CustomDecimal3);
			AssertEquals(1000009.008m, whsDocketLine.WE_CustomDecimal4);
			AssertEquals(-8765.01m, whsDocketLine.WE_CustomDecimal5);
			AssertEquals(false, whsDocketLine.WE_CustomFlag1);
			AssertEquals(false, whsDocketLine.WE_CustomFlag2);
			AssertEquals(true, whsDocketLine.WE_CustomFlag3);
			AssertEquals(true, whsDocketLine.WE_CustomFlag4);
			AssertEquals(false, whsDocketLine.WE_CustomFlag5);
			AssertWhsDocketLineCustomsData(whsDocketLine);
			AssertWhsDocketLineOtherDetail(whsDocketLine);
		}
		else
		{
			Assert("Not required", true);
		}
	}

	#endregion

	#region Import Address Overriden Organisations

	public void TestImportAddressOverridenOrganisations()
	{
		if (IsImportFromValueObjectSupported)
		{
			SetupData();

			// turn on attributes
			Helper.EnableWarehouseForBond(Whs, true);

			foreach (AttributeNumber attribNo in Enum.GetValues(typeof(AttributeNumber)))
			{
				Helper.SetClientAttributeType(Client, attribNo, true);
				Helper.SetProductAttributeUse(Client, Part, attribNo, true);
			}

			Factory.Save();

			var xsdDockets = new Xsd.WhsDockets();
			PopulateXsdDockets(xsdDockets);

			var xsdDocket = xsdDockets.WhsDocket[0];
			xsdDocket.DocketDetail.TransportCompany.AddressReference.IsSpecified = false;
			xsdDocket.DocketDetail.TransportBilledTo.AddressReference.IsSpecified = false;
			SetOverridenAddresses(xsdDocket);

			var context = new ValueObjectImportContext(Factory, Notifications);
			var collection = GetCollection();
			Adapter.ImportFromValueObject(collection, xsdDockets, context);

			AssertEquals("Collection should have a new element", 1, collection.Count);

			var whsDocket = (TDocket)collection[0];
			if (whsDocket is IJobWithTransportCompany job)
			{
				AssertNotNull(job.TransportCoDocAddress.Organisation);
				AssertEquals("ALL Transport AU", job.TransportCoDocAddress.E2_CompanyName);
			}

			AssertNotNull(whsDocket.TransportBillToDocAddress.Organisation);
			AssertEquals("Transport Billed To", whsDocket.TransportBillToDocAddress.E2_CompanyName);
			AssertOverridenAddressesWereImportedCorrectly(whsDocket);
		}
		else
		{
			Assert("Not required", true);
		}
	}

	protected virtual void SetOverridenAddresses(Xsd.WhsDocket xsdDocket)
	{
	}

	protected virtual void AssertOverridenAddressesWereImportedCorrectly(TDocket whsDocket)
	{
	}

	#endregion

	#region PopulateValueObjectWithLongStrings

	protected override void PopulateValueObjectWithLongStrings(IValueObject value, ValueObjectPropertyNavigator navigator)
	{
		base.PopulateValueObjectWithLongStrings(value, navigator);
		if (value is Xsd.WhsDocket)
		{
			Xsd.WhsDocket docket = (Xsd.WhsDocket)value;
			Whs.WW_WarehouseCode = "WHS";
			docket.DocketDetail.WarehouseCode = Whs.WW_WarehouseCode;
		}
	}

	#endregion

	#region TestImportCustomValues

	public void TestImportCustomValues()
	{
		if (IsImportFromValueObjectSupported)
		{
			var docket = Factory.New<TDocket>();

			var docketValue = new Xsd.WhsDocket();

			docketValue.DocketDetail.CustomValues.Add(new Xsd.CustomValue { Name = "Bool1", Type = "Boolean", Value = "Y" });
			docketValue.DocketDetail.CustomValues.Add(new Xsd.CustomValue { Name = "Date1", Type = "DateTime", Value = "2011-02-01T00:00:00" });
			docketValue.DocketDetail.CustomValues.Add(new Xsd.CustomValue { Name = "Decimal1", Type = "Decimal", Value = "3.21" });
			docketValue.DocketDetail.CustomValues.Add(new Xsd.CustomValue { Name = "Int1", Type = "Integer", Value = "123" });
			docketValue.DocketDetail.CustomValues.Add(new Xsd.CustomValue { Name = "String1", Type = "String", Value = "test string" });

			var adapter = GetDataAdapter();
			adapter.ImportFromValueObject(docket, docketValue, new ValueObjectImportContext(Factory, new NotificationBuffer()));

			AssertEquals("Custom bool is correct.", new ZBool(true), docket.GetUserDefinedValue<ZBool>("Bool1"));
			AssertEquals("Custom datetime is correct.", new ZDateTime(2011, 02, 01), docket.GetUserDefinedValue<ZDateTime>("Date1"));
			AssertEquals("Custom decimal is correct.", new ZDecimal(3.21m), docket.GetUserDefinedValue<ZDecimal>("Decimal1"));
			AssertEquals("Custom integer is correct.", new ZInt(123), docket.GetUserDefinedValue<ZInt>("Int1"));
			AssertEquals("Custom string is correct.", new ZString("test string"), docket.GetUserDefinedValue<ZString>("String1"));
		}
		else
		{
			Assert("Not required", true);
		}
	}

	#endregion

	#region TestImportAddress

	public void TestImportAddress()
	{
		if (IsImportFromValueObjectSupported)
		{
			var docket = Factory.New<TDocket>();
			var transportCompanyAddress = docket.DocAddresses.AddNew();
			transportCompanyAddress.DocAddressType = DocAddressType.TransportCompanyDocumentaryAddress;
			transportCompanyAddress.E2_AddressOverride = true;
			transportCompanyAddress.Address1 = "DocketTransportAddress1";
			transportCompanyAddress.Address2 = "DocketTransportAddress2";

			var transportBillToAddressCompanyAddress = docket.DocAddresses.AddNew();
			transportBillToAddressCompanyAddress.DocAddressType = DocAddressType.TransportBillToAddress;
			transportBillToAddressCompanyAddress.E2_OA_Address = TransportCo.MainAddress.PK;

			var consigneeCompanyAddress = docket.DocAddresses.AddNew();
			consigneeCompanyAddress.DocAddressType = DocAddressType.ConsigneeAddress;
			consigneeCompanyAddress.E2_AddressOverride = true;
			consigneeCompanyAddress.Address1 = "DocketConsigneeAddress1";
			consigneeCompanyAddress.Address2 = "DocketConsigneeAddress2";

			var goodsBillToCompanyAddress = docket.DocAddresses.AddNew();
			goodsBillToCompanyAddress.DocAddressType = DocAddressType.GoodsBillToAddress;
			goodsBillToCompanyAddress.E2_OA_Address = TransportCo.MainAddress.PK;

			var supplierCompanyAddress = docket.DocAddresses.AddNew();
			supplierCompanyAddress.DocAddressType = DocAddressType.SupplierDocumentaryAddress;
			supplierCompanyAddress.E2_AddressOverride = true;
			supplierCompanyAddress.Address1 = "DocketSupplierAddress1";
			supplierCompanyAddress.Address2 = "DocketSupplierAddress2";

			var xsdDockets = new Xsd.WhsDockets();
			PopulateXsdDockets(xsdDockets);
			var xsdDocket = xsdDockets.WhsDocket[0];
			xsdDocket.DocketDetail.TransportCompany.AddressReference.IsSpecified = false;
			xsdDocket.DocketDetail.TransportBilledTo.AddressReference.IsSpecified = false;
			PopulateCustomAddress(xsdDocket.DocketDetail);

			var adapter = GetDataAdapter();
			adapter.ImportFromValueObject(docket, xsdDocket, new ValueObjectImportContext(Factory, new NotificationBuffer()));

			var transportCompanyDocAddress = docket.DocAddresses.FindOrCreateWithDocAddressType(DocAddressType.TransportCompanyDocumentaryAddress);
			AssertEquals("TransportCoAddress", transportCompanyDocAddress.Address1);
			AssertEquals("DocketTransportAddress2", transportCompanyDocAddress.Address2);

			AssertEquals("TransportBillToAddress", docket.TransportBillToDocAddress.Address1);
			AssertEquals("", docket.TransportBillToDocAddress.Address2);

			AssertCustomAddress(docket);
		}
		else
		{
			Assert("Not required", true);
		}
	}

	protected virtual void PopulateCustomAddress(Xsd.WhsDocketDocketDetail docketDetail)
	{
	}

	protected virtual void AssertCustomAddress(TDocket docket)
	{
	}

	#endregion

	#endregion

	#region Test Data Export

	#region TestExportData

	[TestDate(2007, 1, 1)]
	public void TestExportData()
	{
		SetupData();
		var whsDocket = GetNewPopulatedDocket(Whs, Client, "1234");
		SetCustomDocketAttributesForFlagsAndDecimals(Client);
		SetCustomDocketLineAttributesForFlagsAndDecimals(Client);
		PopulateWhsDocketForExport(whsDocket);
		PopulateWhsDocketTotalsForExport(whsDocket);
		var xsdDocket = Adapter.ExportToValueObject(whsDocket, new ValueObjectExportContext(Notifications));
		AssertExportedXsdDocket(xsdDocket, whsDocket);
	}

	#endregion

	#region SetUpStorageMainAndStorageDocs

	TDocket SetUpStorageMainAndStorageDocs()
	{
		using (var resourceRetrieverDocumentScanning = new EmbeddedResourceRetriever(typeof(StorageDocsBaseTest).Assembly))
		{
			var documentFactoryProvider = ObjectFactory.Get<IDocumentFactoryProvider>();
			var documentFactory = documentFactoryProvider.GetFactory(Factory);
			var storageDocsFactory = documentFactory.GetFactory(1);

			var docket = (WhsDocket)((BusinessObjectFactory)documentFactory).NewWithValidTestData(typeof(TDocket));
			var storageMain = (BusinessObject)((BusinessObjectFactory)documentFactory).New<IStorageMain>();
			var storageDocs = (BusinessObject)storageDocsFactory.New<IStorageDocs>();
			var storageFile = (BusinessObject)storageDocsFactory.New<IStorageFile>();

			storageMain[StorageMainSchema.SM_ParentFK.Name] = docket.PK;
			storageMain[StorageMainSchema.SM_DB.Name] = 1;
			storageDocs[StorageDocsSchema.SC_SM.Name] = storageMain.PK;
			storageFile[StorageDocsSchema.SC_SM.Name] = storageMain.PK;

			storageDocs[StorageDocsSchema.SC_DataType.Name] = "TIF";
			storageFile[StorageDocsSchema.SC_DataType.Name] = "PDF";
			var year = ZDateTime.Now.Year;
			var documentDate = new ZDateTime(year, 10, 11);
			storageDocs[StorageDocsSchema.SC_Date.Name] = documentDate;
			storageDocs[StorageDocsSchema.SC_DocType.Name] = "MBL";
			storageFile[StorageDocsSchema.SC_Date.Name] = documentDate;
			storageFile[StorageDocsSchema.SC_DocType.Name] = "QUO";
			storageDocs[StorageDocsSchema.SC_ImageData.Name] = resourceRetrieverDocumentScanning.GetBytes("Enterprise.DocumentScanning.Business.Test.TestDocs.small.tif");
			storageDocs[StorageDocsSchema.SC_Desc.Name] = "Testing eDoc1";
			storageDocs[StorageDocsSchema.SC_IsPublished.Name] = true;
			storageFile[StorageDocsSchema.SC_ImageData.Name] = resourceRetrieverDocumentScanning.GetBytes("Enterprise.DocumentScanning.Business.Test.TestDocs.Sample.PDF");
			storageFile[StorageDocsSchema.SC_Desc.Name] = "Testing eDoc2";
			storageFile[StorageDocsSchema.SC_IsPublished.Name] = true;

			documentFactory.Save();

			return (TDocket)docket;
		}
	}

	#endregion

	#region TestExportStorageDocs_IncludeeDocsIsFalse

	public void TestExportStorageDocs_IncludeeDocsIsFalse()
	{
		var whsDocket = SetUpStorageMainAndStorageDocs();

		if (whsDocket is WhsOrder)
		{
			SystemDataRegistry.Instance.IncludeWhsOrdereDocs.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
		}
		else if (whsDocket is WhsReceive)
		{
			SystemDataRegistry.Instance.IncludeWhsReceipteDocs.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
		}
		else if (whsDocket is WhsAdjustment)
		{
			SystemDataRegistry.Instance.IncludeWhsAdjustmenteDocs.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
		}

		var result = Adapter.ExportToValueObject(whsDocket, new ValueObjectExportContext(new NotificationBuffer()));
		AssertEquals("Documents should NOT be specified", false, result.Documents.IsSpecified);
	}

	#endregion

	#region TestExportStorageDocs_IncludeeDocsIsTrue

	public void TestExportStorageDocs_IncludeeDocsIsTrue()
	{
		var whsDocket = SetUpStorageMainAndStorageDocs();

		if (whsDocket is WhsOrder)
		{
			SystemDataRegistry.Instance.IncludeWhsOrdereDocs.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
		}
		else if (whsDocket is WhsReceive)
		{
			SystemDataRegistry.Instance.IncludeWhsReceipteDocs.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
		}
		else if (whsDocket is WhsAdjustment)
		{
			SystemDataRegistry.Instance.IncludeWhsAdjustmenteDocs.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
		}

		var result = Adapter.ExportToValueObject(whsDocket, new ValueObjectExportContext(new NotificationBuffer()));

		var year = ZDateTime.Now.Year;

		AssertEquals("Documents count should be 2", 2, result.Documents.Count);
		AssertEquals("DataType", "TIF", result.Documents[0].DataType);
		AssertEquals("Date", new ZDateTime(year, 10, 11), result.Documents[0].Date);
		AssertEquals("Document Type", "MBL", result.Documents[0].DocumentType);
		AssertEquals("Data", true, result.Documents[0].Data.Length > 1000);
		AssertEquals("Description", "Testing eDoc1", result.Documents[0].Description);

		AssertEquals("DataType", "PDF", result.Documents[1].DataType);
		AssertEquals("Date", new ZDateTime(year, 10, 11), result.Documents[1].Date);
		AssertEquals("Document Type", "QUO", result.Documents[1].DocumentType);
		AssertEquals("Data", true, result.Documents[1].Data.Length > 1000);
		AssertEquals("Description", "Testing eDoc2", result.Documents[1].Description);
	}

	#endregion

	#region TestExportBilling

	public void TestExportBilling()
	{
		var docket = GetNewPopulatedDocket(Whs, Client, "1234");

		SystemDataRegistry.Instance.IncludeBillingInfoInWarehouseXML.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

		var xsdDocketNoBilling1 = Adapter.ExportToValueObject(docket, new ValueObjectExportContext(Notifications));
		Assert("Billing data should not export as Billing Export turned off in Registry", !xsdDocketNoBilling1.Billing.IsSpecified);

		SystemDataRegistry.Instance.IncludeBillingInfoInWarehouseXML.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

		var xsdDocketNoBilling2 = Adapter.ExportToValueObject(docket, new ValueObjectExportContext(Notifications));
		Assert("Billing data should not export as no Job Header is attached", !xsdDocketNoBilling2.Billing.IsSpecified);

		var header = Factory.NewJobForTesting<JobHeader>();
		header.JH_ParentID = docket.PK;
		var charge = Factory.NewWithValidTestData<JobCharge>();
		charge.JR_JH = header.PK;
		charge.JR_InvoiceType = AgencyInvoiceTypesList.Codes.ForeignCollect;

		var xsdDocketWithBilling = Adapter.ExportToValueObject(docket, new ValueObjectExportContext(Notifications));
		Assert("Billing data should export", xsdDocketWithBilling.Billing.IsSpecified);
	}

	#endregion

	#region TestExportCustomValues

	public void TestExportCustomValues()
	{
		// Create a receive job and add custom values to it
		var docket = Factory.New<TDocket>();
		var docketValue = new Xsd.WhsDocket();

		// Note, the collection will return these alpha sorted....
		docket.SetUserDefinedValue("Bool1", new ZBool(true));
		docket.SetUserDefinedValue("Date1", new ZDateTime(2011, 02, 01));
		docket.SetUserDefinedValue("Decimal1", new ZDecimal(3.21m));
		docket.SetUserDefinedValue("Int1", new ZInt(123));
		docket.SetUserDefinedValue("String1", new ZString("test string"));

		// Get overridden adapter and export the value object
		var adapter = GetDataAdapter();
		adapter.ExportToValueObject(docket, docketValue, new ValueObjectExportContext(new NotificationBuffer()));

		// Check the custom values are correctly set
		AssertEquals("Correct number of custom values set.", 5, docketValue.DocketDetail.CustomValues.Count);

		AssertEquals("Bool name correctly set.", "Bool1", docketValue.DocketDetail.CustomValues[0].Name);
		AssertEquals("Bool type correctly set.", "Boolean", docketValue.DocketDetail.CustomValues[0].Type);
		AssertEquals("Bool value correctly set.", "Y", docketValue.DocketDetail.CustomValues[0].Value);

		AssertEquals("Date name correctly set.", "Date1", docketValue.DocketDetail.CustomValues[1].Name);
		AssertEquals("Date type correctly set.", "DateTime", docketValue.DocketDetail.CustomValues[1].Type);
		AssertEquals("Date value correctly set.", "2011-02-01T00:00:00", docketValue.DocketDetail.CustomValues[1].Value);

		AssertEquals("Decimal name correctly set.", "Decimal1", docketValue.DocketDetail.CustomValues[2].Name);
		AssertEquals("Decimal type correctly set.", "Decimal", docketValue.DocketDetail.CustomValues[2].Type);
		AssertEquals("Decimal value correctly set.", "3.21", docketValue.DocketDetail.CustomValues[2].Value);

		AssertEquals("Integer name correctly set.", "Int1", docketValue.DocketDetail.CustomValues[3].Name);
		AssertEquals("Integer type correctly set.", "Integer", docketValue.DocketDetail.CustomValues[3].Type);
		AssertEquals("Integer value correctly set.", "123", docketValue.DocketDetail.CustomValues[3].Value);

		AssertEquals("String name correctly set.", "String1", docketValue.DocketDetail.CustomValues[4].Name);
		AssertEquals("String type correctly set.", "String", docketValue.DocketDetail.CustomValues[4].Type);
		AssertEquals("String value correctly set.", "test string", docketValue.DocketDetail.CustomValues[4].Value);
	}

	#endregion

	#endregion

	#region Implementation

	#region CreateNewRefContainer

	protected virtual RefContainer CreateNewRefContainer(ZString code)
	{
		var @ref = Factory.New<RefContainer>();
		@ref.RC_Code = code;
		@ref.RC_IsActive = ZBool.True;
		return @ref;
	}

	#endregion

	#region AssertQuantity

	protected virtual void AssertQuantity(WhsDocketLine line)
	{
		AssertEquals(100m, line.WE_PackQuantity);
		AssertEquals(200m, line.WE_TransactionQuantity);
	}

	#endregion

	#region PopulateWhsDocketForExport

	protected virtual void PopulateWhsDocketForExport(TDocket whsDocket)
	{
		whsDocket.WD_DocketID = "docket123456";
		whsDocket.WD_CustomerReference = "custref";
		whsDocket.WD_DropMode = "dm";
		whsDocket.WD_FinalisedDate = new ZDateTimeOffset(2007, 07, 05);

		if (whsDocket is IJobWithTransportCompany job)
		{
			job.TransportCoDocAddress.OrganisationPK = TransportCo.PK;
			job.TransportCoDocAddress.E2_OA_Address = TransportCo.MainAddress.PK;
		}

		whsDocket.WD_TransportReference = "trsref";

		var serviceLevel = TransportCo.MiscServ.CarrierServiceLevels.AddNew();
		serviceLevel.PL_Code = "TST";
		whsDocket.WD_PL_NKCarrierServiceLevel = "CSL";
		whsDocket.WD_RS_NKServiceLevel = "TSL";

		whsDocket.WD_CustomAttrib1 = "CustomAttrib1";
		whsDocket.WD_CustomAttrib2 = "CustomAttrib2";
		whsDocket.WD_CustomAttrib3 = "CustomAttrib3";
		whsDocket.WD_CustomAttrib4 = "CustomAttrib4";
		whsDocket.WD_CustomAttrib5 = "CustomAttrib5";

		whsDocket.WD_CustomDate1 = new ZDateTime(2008, 09, 16);
		whsDocket.WD_CustomDate2 = new ZDateTime(2008, 01, 31);

		whsDocket.WD_CustomFlag1 = true;
		whsDocket.WD_CustomFlag2 = false;
		whsDocket.WD_CustomFlag3 = true;
		whsDocket.WD_CustomFlag4 = true;
		whsDocket.WD_CustomFlag5 = false;

		whsDocket.WD_CustomDecimal1 = 10.123m;
		whsDocket.WD_CustomDecimal2 = 0.0m;
		whsDocket.WD_CustomDecimal3 = 178.9m;
		whsDocket.WD_CustomDecimal4 = 32m;
		whsDocket.WD_CustomDecimal5 = 1000m;

		var reference1 = whsDocket.References.AddNew();
		reference1.WX_RefType = reference1.Lookups.ReferenceTypes[0].Code;
		reference1.WX_Reference = "TEST1";

		var reference2 = whsDocket.References.AddNew();
		reference2.WX_RefType = reference2.Lookups.ReferenceTypes[1].Code;
		reference2.WX_Reference = "TEST2";

		var container1 = whsDocket.Containers.AddNew();
		container1.WC_IsChargeable = ZBool.True;
		container1.WC_ContainerNum = "C1";
		container1.WC_IsPalletised = ZBool.True;
		container1.WC_ItemCount = 10;
		container1.WC_PalletCount = 100;
		container1.WC_RC = CreateNewRefContainer("TST10FT").PK;
		container1.WC_SealNum = "S1";

		var container2 = whsDocket.Containers.AddNew();
		container2.WC_IsChargeable = ZBool.False;
		container2.WC_ContainerNum = "C2";
		container2.WC_IsPalletised = ZBool.False;
		container2.WC_ItemCount = 20;
		container2.WC_PalletCount = 200;
		container2.WC_RC = ZGuid.Empty;
		container2.WC_SealNum = "S2";

		whsDocket.Notes.AddNew(false, PredefinedNoteTypes.Instance.HandlingInstructions.Description, "This is my first note!");

		whsDocket.Logs.AddNew(Events.Arrival);
		whsDocket.Logs.AddNew(Events.Arrival);
		whsDocket.Logs.AddNew(Events.Arrival);

		var bWA1 = Factory.New<WhsBondedWarehouseAttribute>();
		bWA1.WB_AddInfo = "AddInfo1";
		bWA1.WB_BondedWhsQty = 10m;
		bWA1.WB_BondedWhsUnitOfQty = "U1";
		bWA1.WB_CustomsQty = 11m;
		bWA1.WB_CustomsUnitOfQty = "C1";
		bWA1.WB_DeclarationReference = "DecRef1";
		bWA1.WB_EntryDate = new ZDateTime(2007, 10, 10);
		bWA1.WB_EntryKey = "BEK1";
		bWA1.WB_EntryLineNo = 1;
		bWA1.WB_RN_NKCountryOfOrigin = "US";
		bWA1.WB_RX_NKTILVCurrency = "USD";
		bWA1.WB_TILV = 100m;
		bWA1.WB_ValueForDuty = 110m;
		var line = CreateNewPopulatedDocketLine(whsDocket, Product1, 10m, "PQ1", "EK1", 1, 1, "PA11", "PA12", "PA13", "SN1", "CA11", "CA12", "CA13", "CA14", "CA15", "CA16", new ZDateTime(2008, 01, 01), new ZDateTime(2008, 01, 02), new ZDateTime(2008, 01, 03), new ZDateTime(2008, 01, 04), new ZDateTime(2008, 01, 05), 1.23m, 78m, 0.008m, 123456m, 100m, false, true, true, false, false, bWA1, "Line Coments1", new ZDate(2007, 10, 12), new ZDate(2007, 11, 12), 5m);
		AddNewCrossDockLinkForExport(line, "CD1", 1, 0, 10m);
		AddNewCrossDockLinkForExport(line, "CD2", 2, 1, 20m);

		var bWA2 = Factory.New<WhsBondedWarehouseAttribute>();
		bWA2.WB_AddInfo = "AddInfo2";
		bWA2.WB_BondedWhsQty = 20m;
		bWA2.WB_BondedWhsUnitOfQty = "U2";
		bWA2.WB_CustomsQty = 22m;
		bWA2.WB_CustomsUnitOfQty = "C2";
		bWA2.WB_DeclarationReference = "DecRef2";
		bWA2.WB_EntryDate = new ZDateTime(2007, 10, 20);
		bWA2.WB_EntryKey = "BEK2";
		bWA2.WB_EntryLineNo = 2;
		bWA2.WB_RN_NKCountryOfOrigin = "AU";
		bWA2.WB_RX_NKTILVCurrency = "AUD";
		bWA2.WB_TILV = 200m;
		bWA2.WB_ValueForDuty = 220m;
		CreateNewPopulatedDocketLine(whsDocket, Product2, 20m, "PQ2", "EK2", 2, 2, "PA21", "PA22", "PA23", "SN2", "CA21", "CA22", "CA23", "CA24", "CA25", "CA26", new ZDateTime(2008, 02, 01), new ZDateTime(2008, 02, 02), new ZDateTime(2008, 02, 03), new ZDateTime(2008, 02, 04), new ZDateTime(2008, 02, 05), 0.0m, 9000.1m, -75444.9m, 23m, 900m, true, true, true, false, true, bWA2, "Line Coments2", new ZDate(2007, 10, 15), new ZDate(2007, 11, 15), 10m);
	}

	#endregion

	#region AddNewCrossDockLinkForExport

	protected virtual WhsPickLine AddNewCrossDockLinkForExport(WhsDocketLine line, ZString crossDockRef, ZShort lineNo, ZShort subLineNo, ZDecimal qty)
	{
		return null;
	}

	#endregion

	#region PopulateWhsDocketTotalsForExport

	protected virtual void PopulateWhsDocketTotalsForExport(TDocket whsDocket)
	{
		whsDocket.CalculateTotalsEnabled = false;
		whsDocket.WD_PackagesSent = 30;
		whsDocket.WD_F3_NKTotalPackType = "BCD";
		whsDocket.CalculateTotalsEnabled = true;
	}

	#endregion

	#region CreateNewPopulatedDocketLine

	protected virtual WhsDocketLine CreateNewPopulatedDocketLine(TDocket docket, WhsProduct product, ZDecimal quantity, ZString uQ, ZString entryKey, ZShort lineNo, ZShort lineSubNo, ZString pA1, ZString pA2, ZString pA3, ZString sn, ZString cA1, ZString cA2, ZString cA3, ZString cA4, ZString cA5, ZString cA6, ZDateTime cDate1, ZDateTime cDate2, ZDateTime cDate3, ZDateTime cDate4, ZDateTime cDate5, ZDecimal cDecimal1, ZDecimal cDecimal2, ZDecimal cDecimal3, ZDecimal cDecimal4, ZDecimal cDecimal5, ZBool cFlag1, ZBool cFlag2, ZBool cFlag3, ZBool cFlag4, ZBool cFlag5, WhsBondedWarehouseAttribute bWA, ZString comments, ZDate expireDate, ZDate packingDate, ZDecimal confirmedQty)
	{
		var line = docket.Lines.AddNew();
		line.WE_WD = docket.PK;
		line.WE_OP = product.Parent.PK;
		line.WE_BondedEntryKey = entryKey;
		line.WE_PackQuantity = quantity;
		line.WE_F3_NKPackType = uQ;
		line.WE_LineNo = lineNo;
		line.WE_SubLineNo = lineSubNo;
		line.WE_PartAttrib1 = pA1;
		line.WE_PartAttrib2 = pA2;
		line.WE_PartAttrib3 = pA3;
		line.WE_SerialNumber = sn;
		line.WE_CustomAttrib1 = cA1;
		line.WE_CustomAttrib2 = cA2;
		line.WE_CustomAttrib3 = cA3;
		line.WE_CustomAttrib4 = cA4;
		line.WE_CustomAttrib5 = cA5;
		line.WE_CustomAttrib6 = cA6;
		line.WE_CustomDate1 = cDate1;
		line.WE_CustomDate2 = cDate2;
		line.WE_CustomDate3 = cDate3;
		line.WE_CustomDate4 = cDate4;
		line.WE_CustomDate5 = cDate5;
		line.WE_CustomDecimal1 = cDecimal1;
		line.WE_CustomDecimal2 = cDecimal2;
		line.WE_CustomDecimal3 = cDecimal3;
		line.WE_CustomDecimal4 = cDecimal4;
		line.WE_CustomDecimal5 = cDecimal5;
		line.WE_CustomFlag1 = cFlag1;
		line.WE_CustomFlag2 = cFlag2;
		line.WE_CustomFlag3 = cFlag3;
		line.WE_CustomFlag4 = cFlag4;
		line.WE_CustomFlag5 = cFlag5;
		line.WE_LineComment = comments;
		line.WE_ExpiryDate = expireDate;
		line.WE_PackingDate = packingDate;
		line.WE_FinalisedDate = docket.WD_FinalisedDate;
		line.WE_DocketLineStatus = docket.WD_DocketStatus == DocketStatus.Codes.Finalised ? DocketLineStatus.Codes.Finalised : "";
		bWA.WB_ParentID = line.PK;
		bWA.WB_ParentTableCode = line.TablePrefix;
		AssertEquals(bWA, line.CustomsData);
		return line;
	}

	#endregion

	#region AssertExportXsdDocketLines

	protected virtual void AssertExportXsdDocketLines(Xsd.WhsDocket xsdDocket)
	{
		AssertEquals(2, xsdDocket.DocketLines.Count);

		AssertEquals("Description1", xsdDocket.DocketLines[0].Description);
		AssertEquals("Line Coments1", xsdDocket.DocketLines[0].LineComments);
		AssertEquals(new ZShort(1), xsdDocket.DocketLines[0].LineNumber);
		AssertEquals("P1", xsdDocket.DocketLines[0].Product);
		AssertEquals("PQ1", xsdDocket.DocketLines[0].ProductUQ);
		AssertEquals(10m, xsdDocket.DocketLines[0].QuantityActuallyOrdered);
		AssertEquals(10m, xsdDocket.DocketLines[0].QuantityFromClientOrder);
		AssertEquals(new ZShort(1), xsdDocket.DocketLines[0].SubLineNumber);

		AssertEquals("EK1", xsdDocket.DocketLines[0].LineAttributes.BondedEntryKey);
		AssertEquals("CA11", xsdDocket.DocketLines[0].LineAttributes.CustomAttribute1);
		AssertEquals("CA12", xsdDocket.DocketLines[0].LineAttributes.CustomAttribute2);
		AssertEquals("CA13", xsdDocket.DocketLines[0].LineAttributes.CustomAttribute3);
		AssertEquals("CA14", xsdDocket.DocketLines[0].LineAttributes.CustomAttribute4);
		AssertEquals("CA15", xsdDocket.DocketLines[0].LineAttributes.CustomAttribute5);

		AssertEquals(new ZDateTime(2008, 01, 01), xsdDocket.DocketLines[0].LineAttributes.CustomDate1);
		AssertEquals(new ZDateTime(2008, 01, 02), xsdDocket.DocketLines[0].LineAttributes.CustomDate2);
		AssertEquals(new ZDateTime(2008, 01, 03), xsdDocket.DocketLines[0].LineAttributes.CustomDate3);
		AssertEquals(new ZDateTime(2008, 01, 04), xsdDocket.DocketLines[0].LineAttributes.CustomDate4);
		AssertEquals(new ZDateTime(2008, 01, 05), xsdDocket.DocketLines[0].LineAttributes.CustomDate5);

		AssertEquals(1.23m, xsdDocket.DocketLines[0].LineAttributes.CustomDecimal1);
		AssertEquals(78m, xsdDocket.DocketLines[0].LineAttributes.CustomDecimal2);
		AssertEquals(0.008m, xsdDocket.DocketLines[0].LineAttributes.CustomDecimal3);
		AssertEquals(123456m, xsdDocket.DocketLines[0].LineAttributes.CustomDecimal4);
		AssertEquals(100m, xsdDocket.DocketLines[0].LineAttributes.CustomDecimal5);

		AssertEquals(false, xsdDocket.DocketLines[0].LineAttributes.CustomFlag1);
		AssertEquals(true, xsdDocket.DocketLines[0].LineAttributes.CustomFlag2);
		AssertEquals(true, xsdDocket.DocketLines[0].LineAttributes.CustomFlag3);
		AssertEquals(false, xsdDocket.DocketLines[0].LineAttributes.CustomFlag4);
		AssertEquals(false, xsdDocket.DocketLines[0].LineAttributes.CustomFlag5);

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

		AssertEquals(2, xsdDocket.DocketLines[0].CrossDockLines.Count);
		AssertEquals("CD1", xsdDocket.DocketLines[0].CrossDockLines[0].Reference);
		AssertEquals((short)1, xsdDocket.DocketLines[0].CrossDockLines[0].LineNumber);
		AssertEquals((short)0, xsdDocket.DocketLines[0].CrossDockLines[0].SubLineNumber);
		AssertEquals(10m, xsdDocket.DocketLines[0].CrossDockLines[0].AllocationQty);
		AssertEquals("UNT", xsdDocket.DocketLines[0].CrossDockLines[0].AllocationQtyUQ);
		AssertEquals("CD2", xsdDocket.DocketLines[0].CrossDockLines[1].Reference);
		AssertEquals((short)2, xsdDocket.DocketLines[0].CrossDockLines[1].LineNumber);
		AssertEquals((short)1, xsdDocket.DocketLines[0].CrossDockLines[1].SubLineNumber);
		AssertEquals(20m, xsdDocket.DocketLines[0].CrossDockLines[1].AllocationQty);
		AssertEquals("UNT", xsdDocket.DocketLines[0].CrossDockLines[1].AllocationQtyUQ);

		AssertEquals("Description2", xsdDocket.DocketLines[1].Description);
		AssertEquals("Line Coments2", xsdDocket.DocketLines[1].LineComments);
		AssertEquals(new ZShort(2), xsdDocket.DocketLines[1].LineNumber);
		AssertEquals("P2", xsdDocket.DocketLines[1].Product);
		AssertEquals("PQ2", xsdDocket.DocketLines[1].ProductUQ);
		AssertEquals(20m, xsdDocket.DocketLines[1].QuantityActuallyOrdered);
		AssertEquals(20m, xsdDocket.DocketLines[1].QuantityFromClientOrder);
		AssertEquals(new ZShort(2), xsdDocket.DocketLines[1].SubLineNumber);

		AssertEquals("EK2", xsdDocket.DocketLines[1].LineAttributes.BondedEntryKey);
		AssertEquals("CA21", xsdDocket.DocketLines[1].LineAttributes.CustomAttribute1);
		AssertEquals("CA22", xsdDocket.DocketLines[1].LineAttributes.CustomAttribute2);
		AssertEquals("CA23", xsdDocket.DocketLines[1].LineAttributes.CustomAttribute3);
		AssertEquals("CA24", xsdDocket.DocketLines[1].LineAttributes.CustomAttribute4);
		AssertEquals("CA25", xsdDocket.DocketLines[1].LineAttributes.CustomAttribute5);

		AssertEquals(new ZDateTime(2008, 02, 01), xsdDocket.DocketLines[1].LineAttributes.CustomDate1);
		AssertEquals(new ZDateTime(2008, 02, 02), xsdDocket.DocketLines[1].LineAttributes.CustomDate2);
		AssertEquals(new ZDateTime(2008, 02, 03), xsdDocket.DocketLines[1].LineAttributes.CustomDate3);
		AssertEquals(new ZDateTime(2008, 02, 04), xsdDocket.DocketLines[1].LineAttributes.CustomDate4);
		AssertEquals(new ZDateTime(2008, 02, 05), xsdDocket.DocketLines[1].LineAttributes.CustomDate5);

		AssertEquals(0.0m, xsdDocket.DocketLines[1].LineAttributes.CustomDecimal1);
		AssertEquals(9000.1m, xsdDocket.DocketLines[1].LineAttributes.CustomDecimal2);
		AssertEquals(-75444.9m, xsdDocket.DocketLines[1].LineAttributes.CustomDecimal3);
		AssertEquals(23m, xsdDocket.DocketLines[1].LineAttributes.CustomDecimal4);
		AssertEquals(900m, xsdDocket.DocketLines[1].LineAttributes.CustomDecimal5);

		AssertEquals(true, xsdDocket.DocketLines[1].LineAttributes.CustomFlag1);
		AssertEquals(true, xsdDocket.DocketLines[1].LineAttributes.CustomFlag2);
		AssertEquals(true, xsdDocket.DocketLines[1].LineAttributes.CustomFlag3);
		AssertEquals(false, xsdDocket.DocketLines[1].LineAttributes.CustomFlag4);
		AssertEquals(true, xsdDocket.DocketLines[1].LineAttributes.CustomFlag5);

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

	#endregion

	protected abstract ZString GetExpectedExportedDocketStatus();

	#region AssertExportedXsdDocket

	protected virtual void AssertExportedXsdDocket(Xsd.WhsDocket xsdDocket, TDocket whsDocket)
	{
		AssertEquals("WHSFORSYD", xsdDocket.Identifier.Client.EDICode);
		AssertEquals("WHSClient for test", xsdDocket.Identifier.Client.OrganisationDetails.Name);
		AssertEquals("ClientAddress", xsdDocket.Identifier.Client.OrganisationDetails.Addresses[0].AddressLine1);
		AssertEquals("AUSYD", xsdDocket.Identifier.Client.OrganisationDetails.Addresses[0].Location.Value);

		AssertEquals(GetExpectedExportedDocketStatus(), xsdDocket.DocketDetail.Status);

		AssertEquals("CustomAttrib1", xsdDocket.DocketDetail.CustomAttributes.CustomAttrib1);
		AssertEquals("CustomAttrib2", xsdDocket.DocketDetail.CustomAttributes.CustomAttrib2);
		AssertEquals("CustomAttrib3", xsdDocket.DocketDetail.CustomAttributes.CustomAttrib3);
		AssertEquals("CustomAttrib4", xsdDocket.DocketDetail.CustomAttributes.CustomAttrib4);
		AssertEquals("CustomAttrib5", xsdDocket.DocketDetail.CustomAttributes.CustomAttrib5);

		AssertEquals(new ZDateTime(2008, 09, 16), xsdDocket.DocketDetail.CustomAttributes.CustomDate1);
		AssertEquals(new ZDateTime(2008, 01, 31), xsdDocket.DocketDetail.CustomAttributes.CustomDate2);

		AssertEquals(true, xsdDocket.DocketDetail.CustomAttributes.CustomFlag1);
		AssertEquals(false, xsdDocket.DocketDetail.CustomAttributes.CustomFlag2);
		AssertEquals(true, xsdDocket.DocketDetail.CustomAttributes.CustomFlag3);
		AssertEquals(true, xsdDocket.DocketDetail.CustomAttributes.CustomFlag4);
		AssertEquals(false, xsdDocket.DocketDetail.CustomAttributes.CustomFlag5);

		AssertEquals(10.123m, xsdDocket.DocketDetail.CustomAttributes.CustomDecimal1);
		AssertEquals(true, xsdDocket.DocketDetail.CustomAttributes.CustomDecimal1Specified);
		AssertEquals(0.0m, xsdDocket.DocketDetail.CustomAttributes.CustomDecimal2);
		AssertEquals(true, xsdDocket.DocketDetail.CustomAttributes.CustomDecimal2Specified);
		AssertEquals(178.9m, xsdDocket.DocketDetail.CustomAttributes.CustomDecimal3);
		AssertEquals(true, xsdDocket.DocketDetail.CustomAttributes.CustomDecimal3Specified);
		AssertEquals(32m, xsdDocket.DocketDetail.CustomAttributes.CustomDecimal4);
		AssertEquals(true, xsdDocket.DocketDetail.CustomAttributes.CustomDecimal4Specified);
		AssertEquals(1000m, xsdDocket.DocketDetail.CustomAttributes.CustomDecimal5);
		AssertEquals(true, xsdDocket.DocketDetail.CustomAttributes.CustomDecimal5Specified);

		AssertEquals("Should have 2 exported references", 2, xsdDocket.DocketDetail.References.Count);
		AssertEquals(whsDocket.References[0].Lookups.ReferenceTypes[0].Code, xsdDocket.DocketDetail.References[0].Type);
		AssertEquals("TEST1", xsdDocket.DocketDetail.References[0].Value);

		AssertEquals(whsDocket.References[1].Lookups.ReferenceTypes[1].Code, xsdDocket.DocketDetail.References[1].Type);
		AssertEquals("TEST2", xsdDocket.DocketDetail.References[1].Value);

		AssertEquals("Should have 2 exported containers", 2, xsdDocket.DocketDetail.Containers.Count);
		AssertEquals("C1", xsdDocket.DocketDetail.Containers[0].ContainerNo);
		AssertEquals(true, xsdDocket.DocketDetail.Containers[0].Chargeable);
		AssertEquals(10, xsdDocket.DocketDetail.Containers[0].Packages);
		AssertEquals(true, xsdDocket.DocketDetail.Containers[0].Palletised);
		AssertEquals(100, xsdDocket.DocketDetail.Containers[0].Pallets);
		AssertEquals("S1", xsdDocket.DocketDetail.Containers[0].SealNo);
		AssertEquals("TST10FT", xsdDocket.DocketDetail.Containers[0].Type);

		AssertEquals("C2", xsdDocket.DocketDetail.Containers[1].ContainerNo);
		AssertEquals(false, xsdDocket.DocketDetail.Containers[1].Chargeable);
		AssertEquals(20, xsdDocket.DocketDetail.Containers[1].Packages);
		AssertEquals(false, xsdDocket.DocketDetail.Containers[1].Palletised);
		AssertEquals(200, xsdDocket.DocketDetail.Containers[1].Pallets);
		AssertEquals("S2", xsdDocket.DocketDetail.Containers[1].SealNo);
		AssertEquals(ZString.Empty, xsdDocket.DocketDetail.Containers[1].Type);

		AssertEquals("WHS", xsdDocket.DocketDetail.WarehouseCode);

		AssertEquals("1234", xsdDocket.Identifier.Reference);
		AssertEquals("docket123456", xsdDocket.Identifier.DocketID);
		AssertEquals("custref", xsdDocket.DocketDetail.CustomerReference);
		AssertEquals("CSL", xsdDocket.DocketDetail.TransportServiceLevel);
		AssertEquals("TSL", xsdDocket.DocketDetail.ServiceLevel);
		AssertEquals("trsref", xsdDocket.DocketDetail.TransportReference);

		var organisationInXml = xsdDocket.DocketDetail.TransportCompany.AddressReference.Organisation;
		if (whsDocket is IJobWithTransportCompany)
		{
			AssertEquals("trc", organisationInXml.EDICode);
			AssertEquals("trc", organisationInXml.OwnerCode);
			AssertEquals("transport", organisationInXml.OrganisationDetails.Name);
			AssertEquals("transport Address1", organisationInXml.OrganisationDetails.Addresses[0].AddressLine1);
			AssertEquals("transport Address2", organisationInXml.OrganisationDetails.Addresses[0].AddressLine2);
			AssertEquals("transport City", organisationInXml.OrganisationDetails.Addresses[0].CityOrSuburb);
		}
		else
		{
			AssertEquals("", organisationInXml.EDICode);
			AssertEquals("Should have no Transport Address.", 0, organisationInXml.OrganisationDetails.Addresses.Count);
		}

		AssertEquals("Action Type is incorrect", GetExpectedActionType(), xsdDocket.Identifier.ActionType);

		AssertExportedXsdDocketTotals(xsdDocket, whsDocket);

		AssertEquals(1, xsdDocket.Notes.Count);
		AssertEquals("This is my first note!", xsdDocket.Notes[0].NoteData);
		AssertExportXsdDocketLines(xsdDocket);

		AssertEquals(ExpectedNumberOfEvents, xsdDocket.Events.Event.Count);
	}

	// for different setups this number may differ
	protected virtual int ExpectedNumberOfEvents => 3;

	#endregion

	#region AssertExportedXsdDocketTotals

	protected virtual void AssertExportedXsdDocketTotals(Xsd.WhsDocket xsdDocket, TDocket whsDocket)
	{
		AssertEquals(10m, xsdDocket.DocketDetail.Units);
		AssertEquals(true, xsdDocket.DocketDetail.UnitsSpecified);
		AssertEquals(20, xsdDocket.DocketDetail.Pallets);
		AssertEquals(true, xsdDocket.DocketDetail.PalletsSpecified);
		AssertEquals(30, (ZInt)xsdDocket.DocketDetail.Packages.Value);
		AssertEquals("BCD", xsdDocket.DocketDetail.Packages.DimensionType);
	}

	#endregion

	#region GetExpectedActionType

	protected virtual Xsd.WhsDocketIdentifierActionType GetExpectedActionType()
	{
		return Xsd.WhsDocketIdentifierActionType.NTF;
	}

	#endregion

	#region AssertWhsDocketLineCustomsData

	protected virtual void AssertWhsDocketLineCustomsData(WhsDocketLine whsDocketLine)
	{
		AssertEquals("TEST ADDINFO", whsDocketLine.CustomsData.WB_AddInfo);
		AssertEquals(10m, whsDocketLine.CustomsData.WB_BondedWhsQty);
		AssertEquals("PL", whsDocketLine.CustomsData.WB_BondedWhsUnitOfQty);
		AssertEquals("US", whsDocketLine.CustomsData.CountryOfOrigin.Code);
		AssertEquals(20m, whsDocketLine.CustomsData.WB_CustomsQty);
		AssertEquals("CT", whsDocketLine.CustomsData.WB_CustomsUnitOfQty);
		AssertEquals(new ZDateTime(2007, 07, 11), whsDocketLine.CustomsData.WB_EntryDate);
		AssertEquals("ENTRY KEY", whsDocketLine.CustomsData.WB_EntryKey);
		AssertEquals(new ZShort(2), whsDocketLine.CustomsData.WB_EntryLineNo);
		AssertEquals(30m, whsDocketLine.CustomsData.WB_TILV);
		AssertEquals("USD", whsDocketLine.CustomsData.WB_RX_NKTILVCurrency);
		AssertEquals(40m, whsDocketLine.CustomsData.WB_ValueForDuty);
		AssertEquals("DECLARATION REF", whsDocketLine.CustomsData.WB_DeclarationReference);
	}

	#endregion

	#region TestImportOfAnExistingObject

	public void TestImportOfAnExistingObject()
	{
		if (IsImportFromValueObjectSupported)
		{
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			SetupData();

			var whsDocket = Factory.New<TDocket>();
			whsDocket.WD_ExternalReference = GetExternalReference();
			whsDocket.WD_OH_Client = Client.PK;
			whsDocket.WD_WW_Whs = Whs.PK;

			whsDocket.References.AddNew();
			whsDocket.References.AddNew();

			whsDocket.References[0].WX_Reference = "ORIGINAL1";
			whsDocket.References[0].WX_RefType = "BOL";

			whsDocket.References[1].WX_Reference = "ORIGINAL2";
			whsDocket.References[1].WX_RefType = "DRV";

			var container1 = whsDocket.Containers.AddNew();
			container1.WC_IsChargeable = ZBool.False;
			container1.WC_ContainerNum = "C1";
			container1.WC_IsPalletised = ZBool.False;
			container1.WC_ItemCount = 5;
			container1.WC_PalletCount = 50;
			container1.WC_RC = CreateNewRefContainer("TST5FT").PK;
			container1.WC_SealNum = "SOLD";

			var container2 = whsDocket.Containers.AddNew();
			container2.WC_IsChargeable = ZBool.False;
			container2.WC_ContainerNum = "C3";
			container2.WC_IsPalletised = ZBool.False;
			container2.WC_ItemCount = 30;
			container2.WC_PalletCount = 300;
			container2.WC_RC = ZGuid.Empty;
			container2.WC_SealNum = "S3";

			Factory.Save();

			var collection = GetCollection();
			var xsdDockets = new Xsd.WhsDockets();
			PopulateXsdDockets(xsdDockets);

			var context = new ValueObjectImportContext(Factory, Notifications);
			Adapter.ImportFromValueObject(collection, xsdDockets, context);

			AssertEquals(true, whsDocket.HasChanges);
			AssertEquals("Message shown", true, ((QueryUserYesNoYesAllNoAllEventArgs)Notifications.LastUserResponse).Message.Contains(GetExpectedMessage()));

			AssertEquals("Should have 3 references", 3, whsDocket.References.Count);
			AssertEquals("BOL", whsDocket.References[0].WX_RefType);
			AssertEquals("TEST1", whsDocket.References[0].WX_Reference);

			AssertEquals("DRV", whsDocket.References[1].WX_RefType);
			AssertEquals("ORIGINAL2", whsDocket.References[1].WX_Reference);

			AssertEquals("HBL", whsDocket.References[2].WX_RefType);
			AssertEquals("TEST2", whsDocket.References[2].WX_Reference);

			AssertEquals("Should have 2 containers", 2, whsDocket.Containers.Count);
			AssertEquals("C1", whsDocket.Containers[0].WC_ContainerNum);
			AssertEquals(ZBool.True, whsDocket.Containers[0].WC_IsChargeable);
			AssertEquals(ZBool.True, whsDocket.Containers[0].WC_IsPalletised);
			AssertEquals(10, whsDocket.Containers[0].WC_ItemCount);
			AssertEquals(100, whsDocket.Containers[0].WC_PalletCount);
			AssertEquals("S1", whsDocket.Containers[0].WC_SealNum);
			AssertNotNull(whsDocket.Containers[0].Container);
			AssertEquals("TST10FT", whsDocket.Containers[0].Container.RC_Code);

			AssertEquals("C2", whsDocket.Containers[1].WC_ContainerNum);
			AssertEquals(ZBool.False, whsDocket.Containers[1].WC_IsChargeable);
			AssertEquals(ZBool.False, whsDocket.Containers[1].WC_IsPalletised);
			AssertEquals(20, whsDocket.Containers[1].WC_ItemCount);
			AssertEquals(200, whsDocket.Containers[1].WC_PalletCount);
			AssertEquals("S2", whsDocket.Containers[1].WC_SealNum);
			AssertNull(whsDocket.Containers[1].Container);
		}
		else
		{
			Assert("Not required", true);
		}
	}

	#endregion

	#region SetupData

	protected void SetupData(ZString partRelation)
	{
		Whs.WW_WarehouseCode = "WHS";
		Part.RelatedOrganisations.RemoveAndDeleteAll();
		Helper.CreateProductClientRelationShip(Client, Part, partRelation);
		Factory.Save();
	}

	protected void SetupData()
	{
		SetupData(OrgPartRelation.RelationshipTypes.Owner);
	}

	#endregion

	protected abstract ZString GetExpectedMessage();

	protected abstract ZString GetExternalReference();

	#region AssertContainsNote

	protected void AssertContainsNote(Notes notes, PredefinedNoteType noteType)
	{
		var result = false;

		foreach (StmNote note in notes.GetAllNotes())
		{
			if (note.ST_Description == noteType.Description)
			{
				result = true;
				break;
			}
		}
		AssertEquals("Should contain " + noteType.Description + "note", true, result);
	}

	#endregion

	protected override void SetUp()
	{
		base.SetUp();

		var testDbHelper = new DocManagerDBHelperTestClass();
		if (!testDbHelper.DatabaseExists(1))
		{
			testDbHelper.CreateDatabase(1);
		}

		SetUpTransportCo();
		Product1 = WhsProduct.GetWhsProduct(Helper.CreateProduct(Client, "P1"));
		Product2 = WhsProduct.GetWhsProduct(Helper.CreateProduct(Client, "P2"));
		Product1.Parent.OP_Desc = "Description1";
		Product2.Parent.OP_Desc = "Description2";
		resourceRetriever = new Lazy<EmbeddedResourceRetriever>(() => new EmbeddedResourceRetriever());
	}

	protected Lazy<EmbeddedResourceRetriever> resourceRetriever;

	protected override void TearDown()
	{
		base.TearDown();
		if (resourceRetriever.IsValueCreated)
		{
			resourceRetriever.Value.Dispose();
		}
	}

	#region SetUpTransportCo

	protected virtual void SetUpTransportCo()
	{
		TransportCo = Helper.CreateClient();
		TransportCo.OH_Code = "trc";
		TransportCo.OH_FullName = "transport";
		TransportCo.MainAddress.OA_Address1 = "transport Address1";
		TransportCo.MainAddress.OA_Address2 = "transport Address2";
		TransportCo.MainAddress.OA_City = "transport City";
	}

	#endregion

	#region SetCustomDocketAttrbute

	protected void SetCustomDocketAttrbute(OrgHeader client, string attributeName)
	{
		var orgLabel = client.CustomLabels.AddNew();
		orgLabel.OT_OH = client.PK;
		orgLabel.OT_FieldName = attributeName;
	}

	#endregion

	#region SetCustomDocketAttributesForFlagsAndDecimals

	protected void SetCustomDocketAttributesForFlagsAndDecimals(OrgHeader client)
	{
		SetCustomDocketAttrbute(client, Core.Constants.CustomLabels.WhsDocket.CustomDecimal1);
		SetCustomDocketAttrbute(client, Core.Constants.CustomLabels.WhsDocket.CustomDecimal2);
		SetCustomDocketAttrbute(client, Core.Constants.CustomLabels.WhsDocket.CustomDecimal3);
		SetCustomDocketAttrbute(client, Core.Constants.CustomLabels.WhsDocket.CustomDecimal4);
		SetCustomDocketAttrbute(client, Core.Constants.CustomLabels.WhsDocket.CustomDecimal5);
		SetCustomDocketAttrbute(client, Core.Constants.CustomLabels.WhsDocket.CustomFlag1);
		SetCustomDocketAttrbute(client, Core.Constants.CustomLabels.WhsDocket.CustomFlag2);
		SetCustomDocketAttrbute(client, Core.Constants.CustomLabels.WhsDocket.CustomFlag3);
		SetCustomDocketAttrbute(client, Core.Constants.CustomLabels.WhsDocket.CustomFlag4);
		SetCustomDocketAttrbute(client, Core.Constants.CustomLabels.WhsDocket.CustomFlag5);
	}

	#endregion

	#region SetCustomDocketLineAttributesForFlagsAndDecimals

	protected void SetCustomDocketLineAttributesForFlagsAndDecimals(OrgHeader client)
	{
		SetCustomDocketAttrbute(client, Core.Constants.CustomLabels.WhsDocketLine.CustomDecimal1);
		SetCustomDocketAttrbute(client, Core.Constants.CustomLabels.WhsDocketLine.CustomDecimal2);
		SetCustomDocketAttrbute(client, Core.Constants.CustomLabels.WhsDocketLine.CustomDecimal3);
		SetCustomDocketAttrbute(client, Core.Constants.CustomLabels.WhsDocketLine.CustomDecimal4);
		SetCustomDocketAttrbute(client, Core.Constants.CustomLabels.WhsDocketLine.CustomDecimal5);
		SetCustomDocketAttrbute(client, Core.Constants.CustomLabels.WhsDocketLine.CustomFlag1);
		SetCustomDocketAttrbute(client, Core.Constants.CustomLabels.WhsDocketLine.CustomFlag2);
		SetCustomDocketAttrbute(client, Core.Constants.CustomLabels.WhsDocketLine.CustomFlag3);
		SetCustomDocketAttrbute(client, Core.Constants.CustomLabels.WhsDocketLine.CustomFlag4);
		SetCustomDocketAttrbute(client, Core.Constants.CustomLabels.WhsDocketLine.CustomFlag5);
	}

	#endregion

	protected OrgHeader TransportCo;
	protected WhsProduct Product1;
	protected WhsProduct Product2;

	#region GetNewXsdDockets

	protected virtual Xsd.WhsDockets GetNewXsdDockets()
	{
		var xsdDockets = new Xsd.WhsDockets();
		var xsdDocket = xsdDockets.WhsDocket.AddNew();
		xsdDocket.DocketLines.AddNew();
		return xsdDockets;
	}

	#endregion

	protected abstract Type GetExpectedDocketErrorHandlerType();

	#region GetExpectedDocketDataFormatterType

	protected virtual Type GetExpectedDocketDataFormatterType()
	{
		return typeof(WhsDocketDataFormatter);
	}

	#endregion

	#region PopulateXsdDockets

	protected void PopulateXsdDockets(Xsd.WhsDockets xsdDockets)
	{
		var xsdDocket = xsdDockets.WhsDocket.AddNew();
		xsdDocket.Identifier.Reference = GetExternalReference();
		PopulateOrg(xsdDocket.Identifier.Client, "WHSClient for test", "ClientAddress", "AUSYD");
		xsdDocket.DocketDetail.WarehouseCode = "WHS";
		xsdDocket.DocketDetail.CustomerReference = "2006";
		PopulateDocAddress(xsdDocket.DocketDetail.TransportCompany, Xsd.DocAddressAddressType.TRA, "ALL Transport AU", "TransportCoAddress", "AUSYD");
		PopulateDocAddress(xsdDocket.DocketDetail.TransportBilledTo, Xsd.DocAddressAddressType.TBT, "Transport Billed To", "TransportBillToAddress", "AUSYD");
		xsdDocket.DocketDetail.TransportReference = "TRREF";
		xsdDocket.DocketDetail.TransportServiceLevel = "D2D";
		xsdDocket.DocketDetail.ServiceLevel = "TSL";

		xsdDocket.DocketDetail.CustomAttributes.CustomAttrib1 = "CustomAttrib1";
		xsdDocket.DocketDetail.CustomAttributes.CustomAttrib2 = "CustomAttrib2";
		xsdDocket.DocketDetail.CustomAttributes.CustomAttrib3 = "CustomAttrib3";
		xsdDocket.DocketDetail.CustomAttributes.CustomAttrib4 = "CustomAttrib4";
		xsdDocket.DocketDetail.CustomAttributes.CustomAttrib5 = "CustomAttrib5";

		xsdDocket.DocketDetail.CustomAttributes.CustomDate1 = new ZDateTime(2008, 01, 02);
		xsdDocket.DocketDetail.CustomAttributes.CustomDate2 = new ZDateTime(2008, 12, 31);

		xsdDocket.DocketDetail.CustomAttributes.CustomDecimal1 = 0m;
		xsdDocket.DocketDetail.CustomAttributes.CustomDecimal1Specified = true;
		xsdDocket.DocketDetail.CustomAttributes.CustomDecimal2 = 12.123m;
		xsdDocket.DocketDetail.CustomAttributes.CustomDecimal2Specified = true;
		xsdDocket.DocketDetail.CustomAttributes.CustomDecimal3 = 10000m;
		xsdDocket.DocketDetail.CustomAttributes.CustomDecimal3Specified = true;
		xsdDocket.DocketDetail.CustomAttributes.CustomDecimal4 = 233m;
		xsdDocket.DocketDetail.CustomAttributes.CustomDecimal4Specified = true;
		xsdDocket.DocketDetail.CustomAttributes.CustomDecimal5 = 0.009m;
		xsdDocket.DocketDetail.CustomAttributes.CustomDecimal5Specified = true;

		xsdDocket.DocketDetail.CustomAttributes.CustomFlag1 = false;
		xsdDocket.DocketDetail.CustomAttributes.CustomFlag1Specified = true;
		xsdDocket.DocketDetail.CustomAttributes.CustomFlag2 = false;
		xsdDocket.DocketDetail.CustomAttributes.CustomFlag2Specified = true;
		xsdDocket.DocketDetail.CustomAttributes.CustomFlag3 = true;
		xsdDocket.DocketDetail.CustomAttributes.CustomFlag3Specified = true;
		xsdDocket.DocketDetail.CustomAttributes.CustomFlag4 = false;
		xsdDocket.DocketDetail.CustomAttributes.CustomFlag4Specified = true;
		xsdDocket.DocketDetail.CustomAttributes.CustomFlag5 = true;
		xsdDocket.DocketDetail.CustomAttributes.CustomFlag5Specified = true;

		var reference = xsdDocket.DocketDetail.References.AddNew();
		reference.Type = "BOL";
		reference.Value = "TEST1";

		reference = xsdDocket.DocketDetail.References.AddNew();
		reference.Type = "HBL";
		reference.Value = "TEST2";

		var container = xsdDocket.DocketDetail.Containers.AddNew();
		container.Chargeable = true;
		container.ContainerNo = "C1";
		container.Packages = 10;
		container.Palletised = true;
		container.Pallets = 100;
		container.SealNo = "S1";
		CreateNewRefContainer("TST10FT");
		container.Type = "TST10FT";

		container = xsdDocket.DocketDetail.Containers.AddNew();
		container.Chargeable = false;
		container.ContainerNo = "C2";
		container.Packages = 20;
		container.Palletised = false;
		container.Pallets = 200;
		container.SealNo = "S2";
		container.Type = ZString.Empty;

		PopulateNotes(xsdDocket.Notes, Xsd.NotesNoteNoteType.HandlingInstructions, "Handling Instructions");
		PopulateNotes(xsdDocket.Notes, Xsd.NotesNoteNoteType.DangerousGoodsAdditionalHandlingInformation, "DG goods");
		PopulateNotes(xsdDocket.Notes, Xsd.NotesNoteNoteType.DeliveryInstructionsNote, "Delivery Instructions");

		xsdDocket.DocketDetail.Units = 11;
		xsdDocket.DocketDetail.Packages.Value = 12;
		xsdDocket.DocketDetail.Packages.DimensionType = "BCD";
		xsdDocket.DocketDetail.Pallets = 13;

		PopulateXsdDocketAdditonalDetail(xsdDocket);

		var xsdDocketLine = xsdDocket.DocketLines.AddNew();
		xsdDocketLine.Product = "Product";
		xsdDocketLine.Description = "Desc";
		xsdDocketLine.QuantityActuallyOrdered = 100.0m;
		xsdDocketLine.QuantityActuallyOrderedSpecified = true;
		xsdDocketLine.QuantityFromClientOrder = 100.0m;
		xsdDocketLine.ProductUQ = "BOX";
		xsdDocketLine.LineAttributes.BondedEntryKey = "BondedKEY";
		xsdDocketLine.LineAttributes.ExpiryDate = new ZDate(2006, 10, 17);
		xsdDocketLine.LineAttributes.PackingDate = new ZDate(2006, 10, 16);
		xsdDocketLine.LineAttributes.PartAttribute1 = "ATTR1";
		xsdDocketLine.LineAttributes.PartAttribute2 = "ATTR2";
		xsdDocketLine.LineAttributes.PartAttribute3 = "ATTR3";

		xsdDocketLine.LineAttributes.CustomAttribute1 = "CAttr1";
		xsdDocketLine.LineAttributes.CustomAttribute2 = "CAttr2";
		xsdDocketLine.LineAttributes.CustomAttribute3 = "CAttr3";
		xsdDocketLine.LineAttributes.CustomAttribute4 = "CAttr4";
		xsdDocketLine.LineAttributes.CustomAttribute5 = "CAttr5";
		xsdDocketLine.LineAttributes.CustomAttribute6 = "CAttr6";

		xsdDocketLine.LineAttributes.CustomDate1 = new ZDateTime(2008, 06, 01);
		xsdDocketLine.LineAttributes.CustomDate2 = new ZDateTime(2008, 06, 02);
		xsdDocketLine.LineAttributes.CustomDate3 = new ZDateTime(2008, 06, 03);
		xsdDocketLine.LineAttributes.CustomDate4 = new ZDateTime(2008, 06, 04);
		xsdDocketLine.LineAttributes.CustomDate5 = new ZDateTime(2008, 06, 05);

		xsdDocketLine.LineAttributes.CustomDecimal1 = 190.8m;
		xsdDocketLine.LineAttributes.CustomDecimal1Specified = true;
		xsdDocketLine.LineAttributes.CustomDecimal2 = 23m;
		xsdDocketLine.LineAttributes.CustomDecimal2Specified = true;
		xsdDocketLine.LineAttributes.CustomDecimal3 = 0m;
		xsdDocketLine.LineAttributes.CustomDecimal3Specified = true;
		xsdDocketLine.LineAttributes.CustomDecimal4 = 1000009.008m;
		xsdDocketLine.LineAttributes.CustomDecimal4Specified = true;
		xsdDocketLine.LineAttributes.CustomDecimal5 = -8765.01m;
		xsdDocketLine.LineAttributes.CustomDecimal5Specified = true;

		xsdDocketLine.LineAttributes.CustomFlag1 = false;
		xsdDocketLine.LineAttributes.CustomFlag1Specified = true;
		xsdDocketLine.LineAttributes.CustomFlag2 = false;
		xsdDocketLine.LineAttributes.CustomFlag2Specified = true;
		xsdDocketLine.LineAttributes.CustomFlag3 = true;
		xsdDocketLine.LineAttributes.CustomFlag3Specified = true;
		xsdDocketLine.LineAttributes.CustomFlag4 = true;
		xsdDocketLine.LineAttributes.CustomFlag4Specified = true;
		xsdDocketLine.LineAttributes.CustomFlag5 = false;
		xsdDocketLine.LineAttributes.CustomFlag5Specified = true;

		xsdDocketLine.LineComments = "Comments";
		xsdDocketLine.LineNumber = 1;

		PopulateXsdDocketLineCustomsData(xsdDocketLine);
		PopulateXsdDocketLineAdditionalDetail(xsdDocketLine);
	}

	#endregion

	#region PopulateXsdDocketLineCustomsData

	void PopulateXsdDocketLineCustomsData(Xsd.WhsDocketLine xsdDocketLine)
	{
		xsdDocketLine.CustomsData.AddInfo = "TEST ADDINFO";
		xsdDocketLine.CustomsData.BondedWhsQuantity = 10m;
		xsdDocketLine.CustomsData.BondedWhsQuantityUnit = "PL";
		xsdDocketLine.CustomsData.CountryOfOrigin = "US";
		xsdDocketLine.CustomsData.CustomsQuantity = 20m;
		xsdDocketLine.CustomsData.CustomsQuantityUnit = "CT";
		xsdDocketLine.CustomsData.EntryDate = new ZDate(2007, 07, 11);
		xsdDocketLine.CustomsData.EntryKey = "ENTRY KEY";
		xsdDocketLine.CustomsData.EntryLineNumber = 2;
		xsdDocketLine.CustomsData.TILVAmount = 30m;
		xsdDocketLine.CustomsData.TILVCurrency = "USD";
		xsdDocketLine.CustomsData.ValueForDuty = 40m;
		xsdDocketLine.CustomsData.DeclarationReference = "DECLARATION REF";
	}

	#endregion

	#region PopulateNotes

	void PopulateNotes(Xsd.NotesNoteCollection notes, Xsd.NotesNoteNoteType noteType, string noteData)
	{
		var note = notes.AddNew();
		note.NoteType = noteType;
		note.NoteData = noteData;
	}

	#endregion

	#region PopulateDocAddress

	protected void PopulateDocAddress(Xsd.DocAddress xsdOrgAddress, Xsd.DocAddressAddressType addressType, ZString name, ZString addressLine1, ZString uNLOCO)
	{
		xsdOrgAddress.AddressReference.AddressSequenceRef = 1;
		PopulateOrg(xsdOrgAddress.AddressReference.Organisation, name, addressLine1, uNLOCO);
		xsdOrgAddress.AddressReference.IsSpecified = true;
		xsdOrgAddress.AddressLine1 = addressLine1;
		xsdOrgAddress.AddressType = addressType;
		xsdOrgAddress.CompanyName = name;
		xsdOrgAddress.CountryCode = uNLOCO.Left(2);
	}

	#endregion

	#region PopulateOrg

	protected void PopulateOrg(Xsd.Organisation xsdOrg, ZString name, ZString addressLine1, ZString uNLOCO)
	{
		xsdOrg.IsSpecified = true;
		xsdOrg.OrganisationDetails = new Xsd.OrganisationDetail();
		xsdOrg.OrganisationDetails.Name = name;
		var address = xsdOrg.OrganisationDetails.Addresses.AddNew();
		address.AddressLine1 = addressLine1;
		xsdOrg.OrganisationDetails.Location = Xsd.UNLOCO.FromPortCode(Factory, uNLOCO);
	}

	#endregion

	#region PopulateCustomValues
	protected void PopulateCustomValues(WhsDocket docket)
	{
		docket.SetUserDefinedValue("TestBoolean", new ZBool(true));
		docket.SetUserDefinedValue("TestDateTime", new ZDateTime(2001, 02, 03));
		docket.SetUserDefinedValue("TestDecimal", new ZDecimal(1.23m));
		docket.SetUserDefinedValue("TestInteger", new ZInt(456));
		docket.SetUserDefinedValue("TestString", new ZString("String"));
	}
	#endregion

	#region SetUniqueReference

	protected override void SetUniqueReference(IValueObject value, ZString reference)
	{
		base.SetUniqueReference(value, reference);
		var docket = value as Xsd.WhsDocket;
		docket.Identifier.Reference = reference;
	}

	#endregion

	#region IsExportToValueObjectSupported

	protected override bool IsExportToValueObjectSupported => true;

	#endregion

	#region ExpectedRootCollectionElementName

	protected override string ExpectedRootCollectionElementName => "WhsDockets";

	#endregion

	#region ExpectedRootElementName

	protected override string ExpectedRootElementName => "WhsDocket";

	#endregion

	#region GetMiscSampleBusinessObjects

	protected override BusinessObjectAndExpectedOutputFileName[] GetMiscSampleBusinessObjects() => Array.Empty<BusinessObjectAndExpectedOutputFileName>();

	#endregion

	#region GetPopulatedBizObjWithEmptyFieldsSample

	protected override BusinessObjectAndExpectedOutputFileName GetPopulatedBizObjWithEmptyFieldsSample()
	{
		return GetEmptyBizObjSample();
	}

	#endregion

	#region XmlNodesToExcludeFromCoverageTest

	protected override string[] XmlNodesToExcludeFromCoverageTest
	{
		get
		{
			return new string[]
			{
				//tested in other ValueObjectDataAdapters
				"Identifier/Client",
				"DocketDetail/TransportCompany",
				"Notes/NoteCreatedDateTime",
				"Notes/NoteData",
				"Notes/CustomNoteTypeName",
				"Identifier/Client",
				"DocketLines",
				"DocketDetail/TransportBilledTo",
				"DocketDetail/TransportServiceLevel",
				"DocketDetail/ServiceLevel",
				"DocketDetail/TransportReference",
				"DocketDetail/CustomerReference",
				"DocketDetail/References",
				"DocketDetail/Containers/SealNo",
				"DocketDetail/Containers/ContainerNo",
				"DocketDetail/Containers/Type",
				"DocketDetail/CubicUQ",
				"DocketDetail/Cubic",
				"DocketDetail/PackagesUQ",
				"DocketDetail/Pallets",
				"DocketDetail/ShipperCODAmount",
				"DocketDetail/ShipperCODType",
				"DocketDetail/TransportInsurance",
				"DocketDetail/WeightUQ",
				"DocketDetail/Weight",
				"DocketDetail/Units",
				"DocketDetail/Packages",
				"DocketDetail/Containers/Palletised",
				"DocketDetail/Containers/Chargeable",
				"DocketDetail/Containers/Packages",
				"DocketDetail/Containers/Pallets",
				"Identifier/Client",
				// docket id is export only
				"Identifier/DocketID",
				"Events",
				"Billing",
				"Documents/DataType",
				"Documents/DocumentType",
				"Documents/Description",
				"Documents/FileName",
				"Documents/Date",
				"Documents/IsSystemGenerated",
				"Documents/IsPublished",
				"Documents/SaveVersions",
			};
		}
	}

	#endregion

	#region WhsTestHelperFunctions

	protected WhsTestHelperFunctions Helper => fHelper ?? (fHelper = new WhsTestHelperFunctions(Factory));
	WhsTestHelperFunctions fHelper;

	#endregion

	#region Client

	protected OrgHeader Client
	{
		get
		{
			if (fClient == null)
			{
				fClient = Helper.CreateClient("WHSCLIENT", "WHSClient for test");
				fClient.Addresses.MainAddress.OA_Address1 = "ClientAddress";
				fClient.OH_RL_NKClosestPort = "AUSYD";
			}
			return fClient;
		}
	}
	OrgHeader fClient;

	#endregion

	#region Part

	protected OrgSupplierPart Part
	{
		get
		{
			if (fProduct == null)
			{
				fProduct = Helper.CreateProduct(Client, "Product");
				Helper.CreateProductUnit(fProduct, "BOX", 2m);
			}
			return fProduct;
		}
	}
	OrgSupplierPart fProduct;

	#endregion

	#region Whs
	protected WhsWarehouse Whs
	{
		get
		{
			if (fWhs == null)
			{
				fWhs = Helper.CreateWarehouse("WAREHOUSE", "WH1", 10, 10);
				fWhs.WW_WarehouseCode = "WH1";
			}
			return fWhs;
		}
	}
	WhsWarehouse fWhs;

	#endregion

	#region Context

	ValueObjectImportContext Context => context ?? (context = new ValueObjectImportContext(Factory, Notifications));
	ValueObjectImportContext context;

	#endregion

	#region Notifications

	protected GuiNotificationBuffer Notifications => notifications ?? (notifications = new GuiNotificationBuffer());
	GuiNotificationBuffer notifications;

	protected class GuiNotificationBuffer : NotificationBuffer
	{
		public IQueryUserEventArgs LastUserResponse;

		protected override void QueryUser(IQueryUserEventArgs e)
		{
			base.QueryUser(e);
			LastUserResponse = e;
		}
	}

	#endregion

	#region Adapter

	protected WhsDocketValueObjectDataAdapter<TDocket> Adapter => adapter ?? (adapter = GetDataAdapter());
	WhsDocketValueObjectDataAdapter<TDocket> adapter;

	#endregion

	protected abstract TDocket GetEmptyWhsDocket();
	protected abstract TDocket GetFullyPopulatedWhsDocket();
	protected abstract WhsDocketCollection GetCollection();
	protected abstract WhsDocketValueObjectDataAdapter<TDocket> GetDataAdapter();
	protected abstract void PopulateXsdDocketAdditonalDetail(Xsd.WhsDocket xsdDocket);
	protected abstract void PopulateXsdDocketLineAdditionalDetail(Xsd.WhsDocketLine xsdDocketLine);
	protected abstract void AssertWhsDocketOtherDetail(TDocket whsDocket);
	protected abstract void AssertWhsDocketLineOtherDetail(WhsDocketLine whsDocketLine);

	#endregion
}
