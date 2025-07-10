using System;
using System.IO;
using System.Linq;
using System.Text;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Registry.Business;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.DataTransfer.Xml;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.Warehouse.DataTransfer.CodeLists;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.Warehouse.Transactions.DataTransfer;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Warehouse.Transactions.GUI.Testing;

[TestedType(typeof(WhsOrderValueObjectDataAdapter))]
class WhsOrderValueObjectDataAdapterTest : WhsDocketValueObjectDataAdapterTest<WhsOrder>
{
	#region Test Export

	#region TestExportOrderUnits

	public void TestExportOrderUnits()
	{
		var data = new TestDataSimpleEnvironment(Factory);
		var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 5m);
		order.WD_UnitsSent = 2;

		AssertEquals("Total Units from lines", 5m, order.WD_TotalUnitsFromLines);

		var notify = new NotificationBuffer();
		var context = new ValueObjectExportContext(notify);
		var orderValueObject = Adapter.ExportToValueObject(order, context);
		AssertEquals("Docket Detail Units", order.WD_UnitsSent, orderValueObject.DocketDetail.Units);

		order.WD_UnitsSent = 0m;
		orderValueObject = Adapter.ExportToValueObject(order, context);
		AssertEquals("Docket Detail Units", order.WD_TotalUnitsFromLines, orderValueObject.DocketDetail.Units);
	}

	#endregion

	#region TestExportOrderRequiredDate

	public void TestExportOrderRequiredDate()
	{
		var now = ZDateTimeOffset.Now;
		var data = new TestDataSimpleEnvironment(Factory);
		var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 5m);
		order.WD_RequiredDate = now.AddDays(-1);

		var notify = new NotificationBuffer();
		var context = new ValueObjectExportContext(notify);
		var orderValueObject = Adapter.ExportToValueObject(order, context);
		var orderDetail = (Xsd.WhsCustomerOrderDetail)orderValueObject.DocketDetail.Item;
		AssertEquals(DateTimeKind.Unspecified, orderDetail.DateRequired.ToDateTime().Kind);
	}

	#endregion

	#endregion

	#region Test Import

	#region TestImportDecimals_OutOfRange

	public void TestImportDecimals_OutOfRange()
	{
		if (IsImportFromValueObjectSupported)
		{
			var bigOutOfSqlRangeDecimal = Decimal.MaxValue;
			var expectedValue = 0M;

			var collection = GetCollection();
			var xsdDockets = new Xsd.WhsDockets();
			var xsdDocket = xsdDockets.WhsDocket.AddNew();
			PopulateXsdDocket(xsdDocket, Whs, Client, "REF1");

			xsdDocket.DocketDetail.ShipperCODAmount = bigOutOfSqlRangeDecimal;
			xsdDocket.DocketDetail.TransportInsurance = bigOutOfSqlRangeDecimal;
			xsdDocket.DocketDetail.Units = bigOutOfSqlRangeDecimal;
			xsdDocket.DocketDetail.Weight.Value = bigOutOfSqlRangeDecimal;
			xsdDocket.DocketDetail.Cubic.Value = bigOutOfSqlRangeDecimal;

			var context = new ValueObjectImportContext(Factory, Notifications);
			Adapter.ImportFromValueObject(collection, xsdDockets, context);

			AssertContains("Value overflow error", Notifications.AsString);

			AssertEquals("Collection should have 1 imported dockets", 1, collection.Count);
			AssertEquals("WD_ShipperCODAmount", expectedValue, collection[0].WD_ShipperCODAmount);
			AssertEquals("WD_LocalCartInsuranceCost", expectedValue, collection[0].WD_LocalCartInsuranceCost);
			AssertEquals("WD_TotalUnits", expectedValue, collection[0].WD_TotalUnits);
			AssertEquals("WD_TotalWeight", expectedValue, collection[0].WD_TotalWeight);
			AssertEquals("WD_WeightSent", expectedValue, collection[0].WD_WeightSent);
			AssertEquals("WD_TotalCubic", expectedValue, collection[0].WD_TotalCubic);
			AssertEquals("WD_CubicSent", expectedValue, collection[0].WD_CubicSent);
		}
		else
		{
			Assert("Not required", true);
		}
	}

	#endregion

	#region TestImportOrderEmptyGoodsBillTo

	public void TestImportOrderEmptyGoodsBillTo()
	{
		if (IsImportFromValueObjectSupported)
		{
			var collection = GetCollection();
			var xsdDockets = new Xsd.WhsDockets();
			var xsdDocket = xsdDockets.WhsDocket.AddNew();
			PopulateXsdDocket(xsdDocket, Whs, Client, "REF1");
			xsdDocket.DocketDetail.Item = new Xsd.WhsCustomerOrderDetail();
			var orderDetail = (Xsd.WhsCustomerOrderDetail)xsdDocket.DocketDetail.Item;
			Factory.Save();

			var context = new ValueObjectImportContext(Factory, Notifications);
			Adapter.ImportFromValueObject(collection, xsdDockets, context);

			AssertEquals("Collection should have 1 imported dockets", 1, collection.Count);
			var importedOrder = (WhsOrder)collection[0];
			AssertEquals(false, importedOrder.GoodsBillToDocAddress.E2_AddressOverride);
		}
		else
		{
			Assert("Not required", true);
		}
	}

	#endregion

	#region TestImportOrderWithGoodsBillToOverride

	public void TestImportOrderWithGoodsBillToOverride()
	{
		if (IsImportFromValueObjectSupported)
		{
			var collection = GetCollection();
			var xsdDockets = new Xsd.WhsDockets();
			var xsdDocket = xsdDockets.WhsDocket.AddNew();
			PopulateXsdDocket(xsdDocket, Whs, Client, "REF1");
			xsdDocket.DocketDetail.Item = new Xsd.WhsCustomerOrderDetail();
			var orderDetail = (Xsd.WhsCustomerOrderDetail)xsdDocket.DocketDetail.Item;
			orderDetail.GoodsBilledTo.CompanyName = "Bill To Override Name";
			Factory.Save();

			var context = new ValueObjectImportContext(Factory, Notifications);
			Adapter.ImportFromValueObject(collection, xsdDockets, context);

			AssertEquals("Collection should have 1 imported dockets", 1, collection.Count);
			var importedOrder = (WhsOrder)collection[0];
			AssertEquals("Bill To Override Name", importedOrder.GoodsBillToDocAddress.E2_CompanyName);
		}
		else
		{
			Assert("Not required", true);
		}
	}

	#endregion

	#region TestImportOrderWithUnknownConsigneeAddsProperUnmatchedNote

	public void TestImportOrderWithUnknownConsigneeAddsProperUnmatchedNote()
	{
		if (IsImportFromValueObjectSupported)
		{
			var unmatchedOrgRegistryItem = OrganisationsDataRegistry.Instance.UseUnmatchedOrganisationForMatching.Value;
			unmatchedOrgRegistryItem.IsEnabled = true;
			OrganisationsDataRegistry.Instance.UseUnmatchedOrganisationForMatching.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, unmatchedOrgRegistryItem);

			var client = Helper.CreateClient("CLINAMSYD", "CLIENT NAME");
			client.OH_RL_NKClosestPort = "AUSYD";

			var clientAddress = client.Addresses[0];
			clientAddress.OA_Address1 = "Client Address";
			clientAddress.OA_City = "KIEV";

			Factory.Save();

			var collection = GetCollection();
			var xsdDockets = new Xsd.WhsDockets();
			var xsdDocket = xsdDockets.WhsDocket.AddNew();
			PopulateXsdDocket(xsdDocket, Whs, client, "REF1");

			xsdDocket.Identifier.Client = GetNewXsdOrganisation(client.OH_Code, client.OH_FullName, clientAddress.OA_Address1, clientAddress.OA_City);

			xsdDocket.DocketDetail.Item = new Xsd.WhsCustomerOrderDetail();
			var orderDetail = (Xsd.WhsCustomerOrderDetail)xsdDocket.DocketDetail.Item;
			orderDetail.Consignee = GetNewXsdDocAddress("CNE", "CNE NAME", "Consignee Address", "Sydney", Xsd.DocAddressAddressType.CEA);

			var context = new ValueObjectImportContext(Factory, Notifications);
			Adapter.ImportFromValueObject(collection, xsdDockets, context);

			AssertEquals("Collection should have 1 imported dockets", 1, collection.Count);
			var importedOrder = (WhsOrder)collection[0];
			AssertEquals("Should load correct Client", client, importedOrder.Client);
			AssertEquals("Should select correct address", clientAddress.OA_Address1, importedOrder.Client.MainAddress.OA_Address1);
			AssertEquals("Should use Unmatched Org", unmatchedOrgRegistryItem.Organisation, importedOrder.ConsigneePK);

			var unmatchedOrgNotes = importedOrder.Notes.FindByDescription(PredefinedNoteTypes.Instance.UnmatchedOrgDetails.Description);
			AssertEquals("unmatchedOrgNotes.Length", 1, unmatchedOrgNotes.Length);
			var unmatchedOrgNote = unmatchedOrgNotes[0];
			AssertMultilineASCIIEquals("Unmatched Orgs Note Content", @" 
Organisation Type: Consignee
Owner Code: CNE
EDI Code: 
Organisation Name: CNE NAME
Address Line 1: Consignee Address
Address Line 2: 
City: Sydney
Post Code: 
State or Province: 
Country: 
Doc Address Type: CEA
 ".TrimStart(), unmatchedOrgNote.ST_NoteText);
		}
		else
		{
			Assert("Not required", true);
		}
	}

	#endregion

	#region TestImportOrderWithSameClientAndConsigneeButDifferentAddresses

	public void TestImportOrderWithSameClientAndConsigneeButDifferentAddresses()
	{
		if (IsImportFromValueObjectSupported)
		{
			var client = Helper.CreateClient("CLINAMSYD", "CLIENT NAME");
			client.OH_RL_NKClosestPort = "AUSYD";

			var clientAddress = client.Addresses[0];
			clientAddress.OA_Address1 = "Client Address";
			clientAddress.OA_City = "KIEV";

			var consigneeAddress = client.Addresses.AddNew();
			consigneeAddress.OA_Address1 = "Consignee Address";
			consigneeAddress.OA_City = "SYDNEY";

			Factory.Save();

			var collection = GetCollection();
			var xsdDockets = new Xsd.WhsDockets();
			var xsdDocket = xsdDockets.WhsDocket.AddNew();
			PopulateXsdDocket(xsdDocket, Whs, client, "REF1");

			xsdDocket.Identifier.Client = GetNewXsdOrganisation(client.OH_Code, client.OH_FullName, clientAddress.OA_Address1, clientAddress.OA_City);

			xsdDocket.DocketDetail.Item = new Xsd.WhsCustomerOrderDetail();
			var orderDetail = (Xsd.WhsCustomerOrderDetail)xsdDocket.DocketDetail.Item;
			orderDetail.Consignee = GetNewXsdDocAddress(client.OH_Code, client.OH_FullName, consigneeAddress.OA_Address1, consigneeAddress.OA_City, Xsd.DocAddressAddressType.CEA);

			var context = new ValueObjectImportContext(Factory, Notifications);
			Adapter.ImportFromValueObject(collection, xsdDockets, context);

			AssertEquals("Collection should have 1 imported dockets", 1, collection.Count);
			var importedOrder = (WhsOrder)collection[0];
			AssertEquals("Should load correct Client", client, importedOrder.Client);
			AssertEquals("Should select correct address", clientAddress.OA_Address1, importedOrder.Client.MainAddress.OA_Address1);
			AssertEquals("Should load correct Consignee", client, importedOrder.Consignee);
			AssertEquals("Should select correct address", consigneeAddress.OA_Address1, importedOrder.ConsigneeDocAddress.Address.OA_Address1);
		}
		else
		{
			Assert("Not required", true);
		}
	}

	#endregion

	#region TestImportOrderWithWhitespaceIssues

	public void TestImportOrderWithWhitespaceIssues()
	{
		Helper.CreateWarehouse("TestWarehouse", "WHS", "RowName");
		Helper.CreateProduct(Helper.CreateClient("WHSCLIENT", "WHSClient for test"), "PRD1");
		Factory.Save();

		var notify = new NotificationBuffer();
		var adapter = new WhsOrderValueObjectDataAdapter();
		var serialiser = new XmlValueObjectSerializer(typeof(Xsd.WhsDocket));
		var importedOrders = new WhsOrderCollection(Factory);
		var testInput = resourceRetriever.Value.GetString("Enterprise.Warehouse.Transactions.GUI.Testing.TestFiles.PopulatedWhsOrderWhitespaceIssues.xml");

		using (var inputStream = new MemoryStream(Encoding.UTF8.GetBytes(testInput)))
		{
			serialiser.ImportXmlData(inputStream, adapter, importedOrders, null, notify);
		}

		AssertEquals("Precondition - Correct number of orders imported.", 1, importedOrders.Count);
		var docket = importedOrders[0];
		var hasCustomerReferenceError = docket.Logs.GetAllLogs().Any(log => ((StmALog)log).SL_Reference.Contains("Error (Customer Reference)"));
		AssertEquals("No Customer reference log errors.", false, hasCustomerReferenceError);
	}

	#endregion

	#region TestWeightVolSetFromImport

	public void TestWeightVolSetFromImport()
	{
		if (IsImportFromValueObjectSupported)
		{
			SetupData();
			var collection = GetCollection();
			var xsdDockets = new Xsd.WhsDockets();
			PopulateXsdDockets(xsdDockets);
			var context = new ValueObjectImportContext(Factory, Notifications);
			WhsOrder order;

			foreach (AttributeNumber attribNo in Enum.GetValues(typeof(AttributeNumber)))
			{
				Helper.SetClientAttributeType(Client, attribNo, true);
				Helper.SetProductAttributeUse(Client, Part, attribNo, true);
				Factory.Save();
			}

			var xsdDocket = xsdDockets.WhsDocket[0];
			xsdDocket.DocketDetail.Units = 0;
			xsdDocket.DocketDetail.Weight.Value = 0;
			xsdDocket.DocketDetail.Cubic.Value = 0;

			Adapter.ImportFromValueObject(collection, xsdDockets, context);
			AssertEquals("Collection should have a new element", 1, collection.Count);
			order = (WhsOrder)collection[0];
			AssertEquals(false, order.WD_WeightVolSetFromImport);

			order.WD_WeightVolSetFromImport = ZBool.True;
			Adapter.ImportFromValueObject(collection, xsdDockets, context);
			AssertEquals("Collection should have a new element", 1, collection.Count);
			order = (WhsOrder)collection[0];
			AssertEquals(false, order.WD_WeightVolSetFromImport);

			xsdDocket.DocketDetail.Units = 10;
			xsdDocket.DocketDetail.Weight.Value = 0;
			xsdDocket.DocketDetail.Cubic.Value = 0;

			Adapter.ImportFromValueObject(collection, xsdDockets, context);
			AssertEquals("Collection should have a new element", 1, collection.Count);
			order = (WhsOrder)collection[0];
			AssertEquals(false, order.WD_WeightVolSetFromImport);

			order.WD_WeightVolSetFromImport = ZBool.False;
			xsdDocket.DocketDetail.Units = 0;
			xsdDocket.DocketDetail.Weight.Value = 20;
			xsdDocket.DocketDetail.Cubic.Value = 0;

			Adapter.ImportFromValueObject(collection, xsdDockets, context);
			AssertEquals("Collection should have a new element", 1, collection.Count);
			order = (WhsOrder)collection[0];
			AssertEquals(true, order.WD_WeightVolSetFromImport);

			order.WD_WeightVolSetFromImport = ZBool.False;
			xsdDocket.DocketDetail.Units = 0;
			xsdDocket.DocketDetail.Weight.Value = 0;
			xsdDocket.DocketDetail.Cubic.Value = 30;

			Adapter.ImportFromValueObject(collection, xsdDockets, context);
			AssertEquals("Collection should have a new element", 1, collection.Count);
			order = (WhsOrder)collection[0];
			AssertEquals(true, order.WD_WeightVolSetFromImport);

			order.WD_WeightVolSetFromImport = ZBool.False;
			xsdDocket.DocketDetail.Units = 10;
			xsdDocket.DocketDetail.Weight.Value = 20;
			xsdDocket.DocketDetail.Cubic.Value = 30;

			Adapter.ImportFromValueObject(collection, xsdDockets, context);
			AssertEquals("Collection should have a new element", 1, collection.Count);
			order = (WhsOrder)collection[0];
			AssertEquals(true, order.WD_WeightVolSetFromImport);

			order.WD_WeightVolSetFromImport = ZBool.False;
			xsdDocket.DocketDetail.Units = 0;
			xsdDocket.DocketDetail.Weight.Value = 0;
			xsdDocket.DocketDetail.Cubic.Value = 0;

			Adapter.ImportFromValueObject(collection, xsdDockets, context);
			AssertEquals("Collection should have a new element", 1, collection.Count);
			order = (WhsOrder)collection[0];
			AssertEquals(false, order.WD_WeightVolSetFromImport);
		}
		else
		{
			Assert(true);
		}
	}

	#endregion

	#region TestImportOrderCreatesJob

	public void TestImportOrderCreatesJob()
	{
		if (IsImportFromValueObjectSupported)
		{
			AccountingConfigurationRegistry.Instance.AddJobInvoicingRecordAtSavingOrEditingOfOperationsJob.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			SetupData();
			var collection = GetCollection();
			var xsdDockets = new Xsd.WhsDockets();
			PopulateXsdDockets(xsdDockets);
			Helper.SetClientAllAttributeType(Client, true);
			Helper.SetProductAllAttributeUse(Client, Part, true);

			var xsdDocket = xsdDockets.WhsDocket[0];
			xsdDocket.DocketDetail.Units = 0;
			xsdDocket.DocketDetail.Weight.Value = 0;
			xsdDocket.DocketDetail.Cubic.Value = 0;

			var context = new ValueObjectImportContext(Factory, Notifications);
			Adapter.ImportFromValueObject(collection, xsdDockets, context);
			var order = (WhsOrder)collection[0];
			AssertNotNull("Order should have Job Header created on Import.", order.JobHeader);
			order.JobHeader.Dispose(); // Disposes Mutex
		}
		else
		{
			Assert(true);
		}
	}
	#endregion

	#region TestImportConsignee

	public void TestImportConsignee_DefaultFromBuyLink()
	{
		if (IsImportFromValueObjectSupported)
		{
			var consignee = Helper.CreateClient("CSNCLIENT", "Consignee client");
			consignee.Addresses.MainAddress.OA_Address1 = "Consignee DANCING Address1";
			consignee.Addresses.MainAddress.OA_Address2 = "Two DANCING Address2";
			consignee.OH_RL_NKClosestPort = "AUSYD";
			consignee.OH_IsConsignee = true;
			Client.BuyerLinks.AddNew(consignee);
			Factory.Save();

			var xsdDocket = new Xsd.WhsDocket();
			PopulateXsdDocket(xsdDocket, Whs, Client, "REF1");

			var context = new ValueObjectImportContext(Factory, Notifications);
			var order = Factory.New<WhsOrder>();
			Adapter.ImportFromValueObject(order, xsdDocket, context);

			AssertEquals("Correct default Address1 on order.", "Consignee DANCING Address1", order.ConsigneeDocAddress.Address1);
			AssertEquals("Correct default Address2 on order.", "Two DANCING Address2", order.ConsigneeDocAddress.Address2);
		}
		else
		{
			Assert("Not required", true);
		}
	}

	public void TestImportConsignee_ProvidedByXML_NotDefaultedFromBuyLink()
	{
		if (IsImportFromValueObjectSupported)
		{
			var consignee = Helper.CreateClient("CSNCLIENT", "Consignee client");
			consignee.Addresses.MainAddress.OA_Address1 = "consignee DANCING Address1";
			consignee.Addresses.MainAddress.OA_Address2 = "Two DANCING Address2";
			consignee.OH_RL_NKClosestPort = "AUSYD";
			consignee.OH_IsConsignee = true;
			Client.BuyerLinks.AddNew(consignee);
			Factory.Save();

			var xsdDocket = new Xsd.WhsDocket();
			PopulateXsdDocket(xsdDocket, Whs, Client, "REF1");

			xsdDocket.DocketDetail.Item = new Xsd.WhsCustomerOrderDetail();
			var orderDetail = (Xsd.WhsCustomerOrderDetail)xsdDocket.DocketDetail.Item;
			orderDetail.Consignee.AddressLine1 = "xsdConsigneeAddress1";

			var context = new ValueObjectImportContext(Factory, Notifications);
			var order = Factory.New<WhsOrder>();
			Adapter.ImportFromValueObject(order, xsdDocket, context);

			AssertEquals("Correct xml provided Address1 on order.", "xsdConsigneeAddress1", order.ConsigneeDocAddress.Address1);
			AssertEquals("Correct xml provided empty Address2 on order.", "", order.ConsigneeDocAddress.Address2);
		}
		else
		{
			Assert("Not required", true);
		}
	}

	#endregion

	#endregion

	#region TestWeightAndCubicUnits

	public void TestWeightAndCubicUnits()
	{
		if (IsImportFromValueObjectSupported)
		{
			SetupData();
			var collection = GetCollection();
			var xsdDockets = new Xsd.WhsDockets();
			PopulateXsdDockets(xsdDockets);
			xsdDockets.WhsDocket[0].Identifier.Reference = "TEST1";
			var existingOrder = Helper.CreateWhsOrder(Client, Whs, "TEST1", Helper.Notify);
			var context = new ValueObjectImportContext(Factory, Notifications);
			WhsOrder order;

			foreach (AttributeNumber attribNo in Enum.GetValues(typeof(AttributeNumber)))
			{
				Helper.SetClientAttributeType(Client, attribNo, true);
				Helper.SetProductAttributeUse(Client, Part, attribNo, true);
				Factory.Save();
			}

			var xsdDocket = xsdDockets.WhsDocket[0];
			xsdDocket.DocketDetail.Weight.IsSpecified = false;
			xsdDocket.DocketDetail.Cubic.IsSpecified = false;
			existingOrder.WD_TotalCubicUnit = "M3";
			existingOrder.WD_TotalWeightUnit = "KG";
			Adapter.ImportFromValueObject(collection, xsdDockets, context);
			AssertEquals("Collection should have a new element", 1, collection.Count);
			order = (WhsOrder)collection[0];
			AssertEquals(existingOrder, order);
			AssertEquals("M3", order.WD_TotalCubicUnit);
			AssertEquals("KG", order.WD_TotalWeightUnit);

			xsdDocket.DocketDetail.Weight.IsSpecified = true;
			xsdDocket.DocketDetail.Cubic.IsSpecified = true;
			xsdDocket.DocketDetail.Weight.DimensionType = "";
			xsdDocket.DocketDetail.Cubic.DimensionType = "";
			existingOrder.WD_TotalCubicUnit = "M3";
			existingOrder.WD_TotalWeightUnit = "KG";
			Adapter.ImportFromValueObject(collection, xsdDockets, context);
			AssertEquals("Collection should have a new element", 1, collection.Count);
			order = (WhsOrder)collection[0];
			AssertEquals(existingOrder, order);
			AssertEquals("M3", order.WD_TotalCubicUnit);
			AssertEquals("KG", order.WD_TotalWeightUnit);

			xsdDocket.DocketDetail.Weight.IsSpecified = true;
			xsdDocket.DocketDetail.Cubic.IsSpecified = true;
			xsdDocket.DocketDetail.Weight.DimensionType = "LB";
			xsdDocket.DocketDetail.Cubic.DimensionType = "C3";
			existingOrder.WD_TotalCubicUnit = "M3";
			existingOrder.WD_TotalWeightUnit = "KG";
			Adapter.ImportFromValueObject(collection, xsdDockets, context);
			AssertEquals("Collection should have a new element", 1, collection.Count);
			order = (WhsOrder)collection[0];
			AssertEquals(existingOrder, order);
			AssertEquals("C3", order.WD_TotalCubicUnit);
			AssertEquals("LB", order.WD_TotalWeightUnit);

			xsdDocket.DocketDetail.Weight.IsSpecified = true;
			xsdDocket.DocketDetail.Cubic.IsSpecified = true;
			xsdDocket.DocketDetail.Weight.DimensionType = "LB";
			xsdDocket.DocketDetail.Cubic.DimensionType = "C3";
			existingOrder.WD_TotalCubicUnit = "";
			existingOrder.WD_TotalWeightUnit = "";
			Adapter.ImportFromValueObject(collection, xsdDockets, context);
			AssertEquals("Collection should have a new element", 1, collection.Count);
			order = (WhsOrder)collection[0];
			AssertEquals(existingOrder, order);
			AssertEquals("C3", order.WD_TotalCubicUnit);
			AssertEquals("LB", order.WD_TotalWeightUnit);

			xsdDocket.DocketDetail.Weight.IsSpecified = true;
			xsdDocket.DocketDetail.Cubic.IsSpecified = true;
			xsdDocket.DocketDetail.Weight.DimensionType = "";
			xsdDocket.DocketDetail.Cubic.DimensionType = "";
			existingOrder.WD_TotalCubicUnit = "";
			existingOrder.WD_TotalWeightUnit = "";
			Adapter.ImportFromValueObject(collection, xsdDockets, context);
			AssertEquals("Collection should have a new element", 1, collection.Count);
			order = (WhsOrder)collection[0];
			AssertEquals(existingOrder, order);
			AssertEquals(Env.Registry.PackageVolumeUnit, order.WD_TotalCubicUnit);
			AssertEquals(Env.Registry.PackageWeightUnit, order.WD_TotalWeightUnit);
		}
		else
		{
			Assert(true);
		}
	}

	#endregion

	#region Overrides

	#region Cross Dock

	protected override WhsPickLine AddNewCrossDockLinkForExport(WhsDocketLine line, ZString crossDockRef, ZShort lineNo, ZShort subLineNo, ZDecimal qty)
	{
		base.AddNewCrossDockLinkForExport(line, crossDockRef, lineNo, subLineNo, qty);
		var receive = Helper.CreateWhsReceive(line.Docket.Client, line.Docket.Warehouse, crossDockRef);
		var inventory = Helper.CreateWhsReceiveInventoryLine(receive, line.SupplierPart, qty);
		inventory.WI_WE_OriginalInDocketLineForRating = inventory.WI_WE_InDocketLine;
		inventory.WI_LineNo = lineNo;
		inventory.WI_SubLineNo = subLineNo;
		inventory.InDocketLine.WE_CurrentInventoryStatus = inventory.WI_InventoryStatus;
		return Helper.CreateReservePickLine((WhsPickableDocketLine)line, inventory, qty);
	}

	protected override void SetupCrossDockDataForImport(WhsWarehouse warehouse, OrgHeader client, OrgSupplierPart part)
	{
		base.SetupCrossDockDataForImport(warehouse, client, part);

		var receive1 = Helper.CreateWhsReceive(client, warehouse);
		receive1.WD_ExternalReference = "CD1";
		var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive1, part, 10m, 1, 0);
		Helper.CreateWhsReceiveInventoryLine(receive1, part, 20m, 1, 1);

		var receive2 = Helper.CreateWhsReceive(client, warehouse);
		receive2.WD_ExternalReference = "CD2";
		var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive2, part, 30m, 1, 0);

		var receive3 = Helper.CreateWhsReceive(client, warehouse);
		receive3.WD_ExternalReference = "CD3";
		var part1 = Helper.CreateProduct(client, "Part1");
		Helper.CreateWhsReceiveInventoryLine(receive3, part1, 30m, 1, 0);

		var client2 = Helper.CreateClient("CL2");
		Helper.CreateProductClientRelationShip(client2, part);
		var receive4 = Helper.CreateWhsReceive(client2, warehouse);
		receive4.WD_ExternalReference = "CD4";
		Helper.CreateWhsReceiveInventoryLine(receive4, part, 30m, 1, 0);

		var warehouse2 = Helper.CreateWarehouse("WH2");
		var receive5 = Helper.CreateWhsReceive(client, warehouse2);
		receive5.WD_ExternalReference = "CD5";
		Helper.CreateWhsReceiveInventoryLine(receive5, part, 30m, 1, 0);

		var order = Helper.CreateWhsOrder(client, warehouse);
		order.WD_ExternalReference = "REF";
		order.ConsigneePK = Helper.CreateClient("CN1").PK;
		var orderLine1 = Helper.CreateWhsOrderLine(order, part, 10m, 10, 1);
		Factory.Save();

		Helper.CreateReservePickLine(orderLine1, inventory1, 1m);
		Helper.CreateReservePickLine(orderLine1, inventory2, 9m);
	}

	protected override void AssertImportedCrossDockData(WhsOrder docket)
	{
		base.AssertImportedCrossDockData(docket);

		docket.Lines.ApplySort(WhsDocketLine.Schema.WE_LineNo, System.ComponentModel.ListSortDirection.Ascending);
		var line = docket.Lines[0];
		AssertEquals((short)1, line.WE_LineNo);
		AssertEquals((short)0, line.WE_SubLineNo);
		AssertEquals(0, line.ReservedPickLines.Count);

		line = docket.Lines[1];
		AssertEquals((short)2, line.WE_LineNo);
		AssertEquals((short)1, line.WE_SubLineNo);
		AssertEquals(1, line.ReservedPickLines.Count);
		var inventory = line.ReservedPickLines[0].Inventory;
		AssertEquals("CD1", inventory.InDocketLine.Docket.WD_ExternalReference);
		AssertEquals((short)1, inventory.WI_LineNo);
		AssertEquals((short)0, inventory.WI_SubLineNo);
		AssertEquals(5m, line.ReservedPickLines[0].WZ_OriginalReservedQty);
		AssertEquals(5m, line.ReservedPickLines[0].ReservedQuantity);

		line = docket.Lines[2];
		AssertEquals((short)3, line.WE_LineNo);
		AssertEquals((short)2, line.WE_SubLineNo);
		AssertEquals(1, line.ReservedPickLines.Count);
		inventory = line.ReservedPickLines[0].Inventory;
		AssertEquals("CD2", inventory.InDocketLine.Docket.WD_ExternalReference);
		AssertEquals((short)1, inventory.WI_LineNo);
		AssertEquals((short)0, inventory.WI_SubLineNo);
		AssertEquals(10m, line.ReservedPickLines[0].WZ_OriginalReservedQty);
		AssertEquals(10m, line.ReservedPickLines[0].ReservedQuantity);

		line = docket.Lines[3];
		AssertEquals((short)4, line.WE_LineNo);
		AssertEquals((short)0, line.WE_SubLineNo);
		AssertEquals(0, line.ReservedPickLines.Count);

		line = docket.Lines[4];
		AssertEquals((short)10, line.WE_LineNo);
		AssertEquals((short)1, line.WE_SubLineNo);
		AssertEquals(1, line.ReservedPickLines.Count);
		inventory = line.ReservedPickLines[0].Inventory;
		AssertEquals("CD1", inventory.InDocketLine.Docket.WD_ExternalReference);
		AssertEquals((short)1, inventory.WI_LineNo);
		AssertEquals((short)0, inventory.WI_SubLineNo);
		AssertEquals(1m, line.ReservedPickLines[0].WZ_OriginalReservedQty);
		AssertEquals(5m, line.ReservedPickLines[0].ReservedQuantity);
	}

	#endregion

	protected override ZString GetExpectedExportedDocketStatus() => DocketStatus.Codes.AttachedToPick;

	protected override void PopulateWhsDocketTotalsForExport(WhsOrder whsDocket)
	{
		base.PopulateWhsDocketTotalsForExport(whsDocket);
		whsDocket.CalculateTotalsEnabled = false;
		whsDocket.WD_TotalUnits = 30;
		whsDocket.WD_UnitsSent = 10;
		whsDocket.WD_PalletsSent = 20;
		whsDocket.CalculateTotalsEnabled = true;
	}

	protected override void PopulateWhsDocketForExport(WhsOrder whsDocket)
	{
		base.PopulateWhsDocketForExport(whsDocket);
		whsDocket.ConsigneeNameOrPK = Consignee.PK.ToString();
		whsDocket.ConsigneeAddressPK = Consignee.MainAddress.PK;
		whsDocket.GoodsBillToPK = GoodsBillTo.PK;
		whsDocket.GoodsBillToAddressPK = GoodsBillTo.MainAddress.PK;
		whsDocket.WD_RequiredDate = new ZDateTimeOffset(2007, 07, 06);
		whsDocket.WD_DocketSubType = OrderType.Codes.Customs;

		whsDocket.WD_DocketStatus = DocketStatus.Codes.AttachedToPick;

		var docketLine = whsDocket.Lines[0];
		docketLine.WE_UnitDiscountAmount = 150m;
		docketLine.WE_UnitDiscountPercent = 50m;
		docketLine.WE_UnitPriceAfterDiscount = 150m;
		docketLine.WE_RecommendedUnitPrice = 200m;
		docketLine.WE_ExtendedLinePrice = 300m;

		docketLine = whsDocket.Lines[1];
		docketLine.WE_UnitDiscountAmount = 30m;
		docketLine.WE_UnitDiscountPercent = 10m;
		docketLine.WE_UnitPriceAfterDiscount = 270m;
		docketLine.WE_RecommendedUnitPrice = 300m;
		docketLine.WE_ExtendedLinePrice = 400m;
	}

	protected override void AssertExportedXsdDocket(Xsd.WhsDocket xsdDocket, WhsOrder order)
	{
		base.AssertExportedXsdDocket(xsdDocket, order);
		AssertEquals(DocketTypes.Codes.WhsOrder, xsdDocket.Identifier.DocketType);

		var xsdOrder = (Xsd.WhsCustomerOrderDetail)xsdDocket.DocketDetail.Item;

		AssertEquals("cns", xsdOrder.Consignee.AddressReference.Organisation.EDICode);
		AssertEquals("cns", xsdOrder.Consignee.AddressReference.Organisation.OwnerCode);
		AssertEquals("consignee", xsdOrder.Consignee.AddressReference.Organisation.OrganisationDetails.Name);
		AssertEquals("consignee Address1", xsdOrder.Consignee.AddressReference.Organisation.OrganisationDetails.Addresses[0].AddressLine1);
		AssertEquals("consignee Address2", xsdOrder.Consignee.AddressReference.Organisation.OrganisationDetails.Addresses[0].AddressLine2);
		AssertEquals("consignee City", xsdOrder.Consignee.AddressReference.Organisation.OrganisationDetails.Addresses[0].CityOrSuburb);

		AssertEquals("gbt", xsdOrder.GoodsBilledTo.AddressReference.Organisation.EDICode);
		AssertEquals("gbt", xsdOrder.GoodsBilledTo.AddressReference.Organisation.OwnerCode);
		AssertEquals("goodsBillTo", xsdOrder.GoodsBilledTo.AddressReference.Organisation.OrganisationDetails.Name);
		AssertEquals("goodsBillTo Address1", xsdOrder.GoodsBilledTo.AddressReference.Organisation.OrganisationDetails.Addresses[0].AddressLine1);
		AssertEquals("goodsBillTo Address2", xsdOrder.GoodsBilledTo.AddressReference.Organisation.OrganisationDetails.Addresses[0].AddressLine2);
		AssertEquals("goodsBillTo City", xsdOrder.GoodsBilledTo.AddressReference.Organisation.OrganisationDetails.Addresses[0].CityOrSuburb);

		AssertEquals(new ZDateTime(2007, 07, 06), xsdOrder.DateRequired);

		AssertEquals(OrderType.Codes.Customs, xsdOrder.OrderType);
	}

	protected override void AssertExportXsdDocketLines(Xsd.WhsDocket xsdDocket)
	{
		base.AssertExportXsdDocketLines(xsdDocket);
		var orderLine = (Xsd.WhsCustomerOrderLineDetail)xsdDocket.DocketLines[0].Item;
		AssertEquals(150m, orderLine.Pricing.UnitDiscountAmount);
		AssertEquals(50m, orderLine.Pricing.UnitDiscount);
		AssertEquals(150m, orderLine.Pricing.UnitPriceAfterDiscount);
		AssertEquals(200m, orderLine.Pricing.RecommendedUnitPrice);
		AssertEquals(300m, orderLine.Pricing.ExtendedPrice);
		AssertEquals(10m, orderLine.ProjectedShortfallQuantity);

		orderLine = (Xsd.WhsCustomerOrderLineDetail)xsdDocket.DocketLines[1].Item;
		AssertEquals(30m, orderLine.Pricing.UnitDiscountAmount);
		AssertEquals(10m, orderLine.Pricing.UnitDiscount);
		AssertEquals(270m, orderLine.Pricing.UnitPriceAfterDiscount);
		AssertEquals(300m, orderLine.Pricing.RecommendedUnitPrice);
		AssertEquals(400m, orderLine.Pricing.ExtendedPrice);
		AssertEquals(20m, orderLine.ProjectedShortfallQuantity);
	}

	protected override WhsOrder GetNewPopulatedDocket(WhsWarehouse whs, OrgHeader client, ZString reference)
	{
		return Helper.CreateWhsOrder(client, whs, reference);
	}

	protected override ZString GetXSDDocketTypeCode()
	{
		return DocketTypes.Codes.WhsOrder;
	}

	protected override Type GetExpectedDocketErrorHandlerType()
	{
		return typeof(WhsOrderErrorHandler);
	}

	protected override ZString GetExternalReference()
	{
		return "ORDER1";
	}

	protected override ZString GetExpectedMessage()
	{
		return "Found Warehouse Order W00000001: ORDER1 of WHSFORSYD";
	}

	protected override Type GetDataAdapterType()
	{
		return typeof(WhsOrderValueObjectDataAdapter);
	}

	protected override ValueObjectDataAdapter<WhsOrder, Xsd.WhsDocket> GetNewBizObjXmlDataAdapter()
	{
		var result = (WhsOrderValueObjectDataAdapter)GetDataAdapter();
		result.FileName = "OrderFile";
		return result;
	}

	protected override WhsOrder GetEmptyWhsDocket()
	{
		var order = Factory.New<WhsOrder>();
		order.WD_DocketStatus = DocketStatus.Codes.Cancelled;
		return order;
	}

	protected override WhsOrder GetFullyPopulatedWhsDocket()
	{
		var whs = Helper.CreateWarehouse("Warehouse");
		whs.WW_WarehouseCode = "WHS";

		var savingFactory = new BusinessObjectFactory();
		var client = Helper.CreateOrLoadClientInSeperateFactory("WHSCLIENT", "WHSClient for test", savingFactory);
		SetCustomDocketAttributesForFlagsAndDecimals(client);
		savingFactory.Save();

		var order = Helper.CreateWhsOrder(client, whs);
		order.WD_ExternalReference = "ORDER1";
		order.WD_RequiredDate = new ZDateTimeOffset(2007, 08, 16);
		order.WD_DocketStatus = DocketStatus.Codes.Cancelled;
		order.WD_CustomAttrib1 = "Attrib1";
		order.WD_CustomAttrib2 = "Attrib2";
		order.WD_CustomAttrib3 = "Attrib3";
		order.WD_CustomAttrib4 = "Attrib4";
		order.WD_CustomAttrib5 = "Attrib5";
		order.WD_CustomDate1 = new ZDateTime(2008, 01, 01);
		order.WD_CustomDate2 = new ZDateTime(2008, 12, 31);
		order.WD_CustomDecimal1 = .009m;
		order.WD_CustomDecimal2 = 1000m;
		order.WD_CustomDecimal3 = 123.457m;
		order.WD_CustomDecimal4 = 67m;
		order.WD_CustomDecimal5 = 5.23m;
		order.WD_CustomFlag1 = true;
		order.WD_CustomFlag2 = false;
		order.WD_CustomFlag3 = true;
		order.WD_CustomFlag4 = false;
		order.WD_CustomFlag5 = true;

		PopulateCustomValues(order);
		return order;
	}

	protected override Type GetExpectedDocketDataFormatterType()
	{
		return typeof(WhsOrderDataFormatter);
	}

	protected override Xsd.WhsDockets GetNewXsdDockets()
	{
		var xsdDockets = base.GetNewXsdDockets();
		var xsdDocket = xsdDockets.WhsDocket[0];
		xsdDocket.DocketDetail.Item = new Xsd.WhsCustomerOrderDetail();
		var xsdLine = xsdDocket.DocketLines[0];
		xsdLine.Item = new Xsd.WhsCustomerOrderLineDetail();
		return xsdDockets;
	}

	protected override WhsDocketValueObjectDataAdapter<WhsOrder> GetDataAdapter()
	{
		return new WhsOrderValueObjectDataAdapter();
	}

	protected override WhsDocketCollection GetCollection()
	{
		return new WhsOrderCollection(Factory, new AdhocCollectionRelationship(typeof(WhsOrder)));
	}

	protected override void PopulateXsdDocketAdditonalDetail(Xsd.WhsDocket xsdDocket)
	{
		xsdDocket.DocketDetail.Item = new Xsd.WhsCustomerOrderDetail();
		var customerOrderDetail = (Xsd.WhsCustomerOrderDetail)xsdDocket.DocketDetail.Item;
		customerOrderDetail.OrderType = "ORD";
		customerOrderDetail.DateRequired = new ZDateTime(2006, 10, 19);
		customerOrderDetail.TotalBillToInvoiceAmount = 100.0m;
		PopulateDocAddress(customerOrderDetail.Consignee, Xsd.DocAddressAddressType.CEA, "Consignee Org", "Consignee Address", "AUSYD");
		PopulateDocAddress(customerOrderDetail.GoodsBilledTo, Xsd.DocAddressAddressType.GBA, "BillTo Org", "BillTo Address", "AUSYD");

		xsdDocket.DocketDetail.ShipperCODAmount = 10.10m;
		xsdDocket.DocketDetail.ShipperCODType = "ABC";
		xsdDocket.DocketDetail.TransportInsurance = 20.20m;
		xsdDocket.DocketDetail.Weight.Value = 100m;
		xsdDocket.DocketDetail.Weight.DimensionType = "CD";
		xsdDocket.DocketDetail.Cubic.Value = 200m;
		xsdDocket.DocketDetail.Cubic.DimensionType = "DE";
	}

	protected override void PopulateXsdDocketLineAdditionalDetail(Xsd.WhsDocketLine xsdDocketLine)
	{
		xsdDocketLine.Item = new Xsd.WhsCustomerOrderLineDetail();
		var customerOrderLineDetail = (Xsd.WhsCustomerOrderLineDetail)xsdDocketLine.Item;
		customerOrderLineDetail.ConsigneeOrBuyerProductCode = "BUYERCODE";
		customerOrderLineDetail.ConsigneeOrBuyerProductDescription = "BUYERDESC";
		customerOrderLineDetail.Pricing.RecommendedUnitPrice = 100.0m;
		customerOrderLineDetail.Pricing.UnitDiscount = 10.0m;
		customerOrderLineDetail.Pricing.UnitDiscountAmount = 20.0m;
		customerOrderLineDetail.Pricing.UnitPriceAfterDiscount = 80.0m;
		customerOrderLineDetail.Pricing.ExtendedPrice = 200.0m;
	}

	protected override void AssertWhsDocketOtherDetail(WhsOrder whsDocket)
	{
		AssertEquals(0m, whsDocket.WD_UnitsSent);
		AssertEquals(11m, whsDocket.WD_TotalUnits);
		AssertEquals(new ZShort(13), whsDocket.WD_PalletsSent);

		AssertEquals("ORD", whsDocket.WD_DocketSubType);
		AssertEquals(new ZDateTimeOffset(2006, 10, 19), whsDocket.WD_RequiredDate);
		AssertNotNull(whsDocket.Consignee);
		AssertEquals("Consignee Org", whsDocket.Consignee.OH_FullName);
		AssertNotNull(whsDocket.GoodsBillToDocAddress.Organisation);
		AssertEquals("BillTo Org", whsDocket.GoodsBillToDocAddress.Organisation.OH_FullName);

		AssertEquals(10.10m, whsDocket.WD_ShipperCODAmount);
		AssertEquals("ABC", whsDocket.WD_CODPayMethod);
		AssertEquals(20.20m, whsDocket.WD_LocalCartInsuranceCost);
		AssertEquals(100m, whsDocket.WD_TotalWeight);
		AssertEquals(100m, whsDocket.WD_WeightSent);
		AssertEquals("CD", whsDocket.WD_TotalWeightUnit);
		AssertEquals(200m, whsDocket.WD_TotalCubic);
		AssertEquals(200m, whsDocket.WD_CubicSent);
		AssertEquals("DE", whsDocket.WD_TotalCubicUnit);
	}

	protected override void AssertWhsDocketLineOtherDetail(WhsDocketLine whsDocketLine)
	{
		AssertEquals(100.0m, whsDocketLine.WE_RecommendedUnitPrice);
		AssertEquals(10.0m, whsDocketLine.WE_UnitDiscountPercent);
		AssertEquals(20.0m, whsDocketLine.WE_UnitDiscountAmount);
		AssertEquals(80.0m, whsDocketLine.WE_UnitPriceAfterDiscount);
		AssertEquals(200.0m, whsDocketLine.WE_ExtendedLinePrice);
	}

	#region Import Address Overriden Organisations

	protected override void SetOverridenAddresses(Xsd.WhsDocket xsdDocket)
	{
		var customerOrderDetail = (Xsd.WhsCustomerOrderDetail)xsdDocket.DocketDetail.Item;
		customerOrderDetail.Consignee.AddressReference.IsSpecified = false;
		customerOrderDetail.GoodsBilledTo.AddressReference.IsSpecified = false;
	}

	protected override void AssertOverridenAddressesWereImportedCorrectly(WhsOrder whsOrder)
	{
		AssertNotNull(whsOrder.ConsigneeDocAddress.Organisation);
		AssertEquals("Consignee Org", whsOrder.ConsigneeDocAddress.E2_CompanyName);

		AssertNotNull(whsOrder.GoodsBillToDocAddress.Organisation);
		AssertEquals("BillTo Org", whsOrder.GoodsBillToDocAddress.E2_CompanyName);
	}

	#endregion

	protected override void PopulateCustomAddress(Xsd.WhsDocketDocketDetail docketDetail)
	{
		base.PopulateCustomAddress(docketDetail);

		var customerOrderDetail = (Xsd.WhsCustomerOrderDetail)docketDetail.Item;
		customerOrderDetail.Consignee.AddressReference.IsSpecified = false;
		customerOrderDetail.GoodsBilledTo.AddressReference.IsSpecified = false;

		customerOrderDetail.Consignee.AddressLine1 = "OrderConsigneeAddress1";
		customerOrderDetail.GoodsBilledTo.AddressLine1 = "OrderGoodsBilledToAddress1";
	}

	protected override void AssertCustomAddress(WhsOrder order)
	{
		base.AssertCustomAddress(order);
		AssertEquals("OrderConsigneeAddress1", order.ConsigneeDocAddress.Address1);
		AssertEquals("DocketConsigneeAddress2", order.ConsigneeDocAddress.Address2);
		AssertEquals("OrderGoodsBilledToAddress1", order.GoodsBillToDocAddress.Address1);
		AssertEquals("", order.GoodsBillToDocAddress.Address2);
	}

	protected override BusinessObjectAndExpectedOutputFileName GetEmptyBizObjSample()
	{
		var emptyWhsOrderPath = resourceRetriever.Value.SaveResourceToFile("Enterprise.Warehouse.Transactions.GUI.Testing.TestFiles.EmptyWhsOrder.xml");
		return new BusinessObjectAndExpectedOutputFileName(GetEmptyWhsDocket(), emptyWhsOrderPath, ValidationKind.None, "Empty WhsOrder");
	}

	protected override BusinessObjectAndExpectedOutputFileName GetFullyPopulatedBizObjSample()
	{
		var populatedWhsOrderPath = resourceRetriever.Value.SaveResourceToFile("Enterprise.Warehouse.Transactions.GUI.Testing.TestFiles.PopulatedWhsOrder.xml");
		return new BusinessObjectAndExpectedOutputFileName(GetFullyPopulatedWhsDocket(), populatedWhsOrderPath, ValidationKind.None, "Populated WhsOrder");
	}

	#endregion

	#region Implementation

	protected Xsd.DocAddress GetNewXsdDocAddress(ZString organisationCode, ZString organisationName, ZString address1, ZString city, Xsd.DocAddressAddressType addressType)
	{
		var result = new Xsd.DocAddress();
		result.AddressReference.AddressSequenceRef = 1;
		result.AddressReference.Organisation = GetNewXsdOrganisation(organisationCode, organisationName, address1, city);
		result.AddressReference.IsSpecified = true;
		result.AddressLine1 = address1;
		result.AddressType = addressType;
		result.CityOrSuburb = city;
		result.IsSpecified = true;
		return result;
	}

	protected Xsd.Organisation GetNewXsdOrganisation(ZString code, ZString name, ZString address1, ZString city)
	{
		var result = new Xsd.Organisation();
		result.IsSpecified = true;
		result.OwnerCode = code;

		result.OrganisationDetails = new Xsd.OrganisationDetail();
		result.OrganisationDetails.Name = name;
		result.OrganisationDetails.IsSpecified = true;

		var xsdOrgAddress = result.OrganisationDetails.Addresses.AddNew();
		xsdOrgAddress.CompanyName = name;
		xsdOrgAddress.AddressLine1 = address1;
		xsdOrgAddress.CityOrSuburb = city;
		xsdOrgAddress.IsSpecified = true;
		return result;
	}

	protected override void SetUp()
	{
		base.SetUp();
		SetUpConsignee();
		SetUpGoodsBillTo();
		AccountingConfigurationRegistry.Instance.AddJobInvoicingRecordAtSavingOrEditingOfOperationsJob.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
	}

	protected virtual void SetUpGoodsBillTo()
	{
		GoodsBillTo = Helper.CreateClient();
		GoodsBillTo.OH_Code = "gbt";
		GoodsBillTo.OH_FullName = "goodsBillTo";
		GoodsBillTo.MainAddress.OA_Address1 = "goodsBillTo Address1";
		GoodsBillTo.MainAddress.OA_Address2 = "goodsBillTo Address2";
		GoodsBillTo.MainAddress.OA_City = "goodsBillTo City";
	}

	protected virtual void SetUpConsignee()
	{
		Consignee = Helper.CreateClient();
		Consignee.OH_Code = "cns";
		Consignee.OH_FullName = "consignee";
		Consignee.MainAddress.OA_Address1 = "consignee Address1";
		Consignee.MainAddress.OA_Address2 = "consignee Address2";
		Consignee.MainAddress.OA_City = "consignee City";
	}

	protected OrgHeader Consignee;
	protected OrgHeader GoodsBillTo;

	#endregion
}
