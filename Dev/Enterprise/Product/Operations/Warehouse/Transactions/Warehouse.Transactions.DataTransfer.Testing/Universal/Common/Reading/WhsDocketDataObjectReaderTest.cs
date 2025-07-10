using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.UniversalDataBuss.Management.Testing;
using Enterprise.UniversalDataBuss.ServiceTasks;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Integration.CodeLists;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.ServiceTasks.CW.Test;
using Env = Enterprise.ZArchitecture.Environment;
using RegistrationNumber = Enterprise.UniversalDataBuss.DataObjects.Universal.RegistrationNumber;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Warehouse.Transactions.DataTransfer.Universal.Testing
{
	abstract class WhsDocketDataObjectReaderTest<TDocket, TDocketLine, TReader> : WhsUniversalTestCase
		where TDocket : WhsDocket
		where TDocketLine : WhsDocketLine
		where TReader : WhsDocketDataObjectReader<TDocket, TDocketLine>, IDocketType
	{
		// matching

		#region TestMatchingOnClientAndExternalReference

		public void TestMatchingOnClientAndExternalReference()
		{
			AssertMatchingOnClientAndExternalReference();
		}

		protected void AssertMatchingOnClientAndExternalReference()
		{
			Data.ShipmentDataObject.Order.Warehouse = null;
			Data.ShipmentDataObject.Order.OrderNumber = "123";
			Data.ShipmentDataObject.Order.OrderNumberSplit = 7;

			var client = Data.CreateClientOrgCRAHOLSYDInDB();
			var warehouse = Helper.CreateWarehouse("MEL");

			var docketToMatch = GetNewDocket(client, warehouse, "123");
			docketToMatch.WD_ExternalReferenceSplit = 7;

			var docketWithDifferentClient = GetNewDocket(Helper.CreateClient(), warehouse, "123");
			docketWithDifferentClient.WD_ExternalReferenceSplit = 7;

			// adjustment does not support split, differentiate via ext ref, otherwise we can differentiate via split
			var externalRef = DocketSupportsSplitNo ? "123" : "456";
			var docketToNotMatch = GetNewDocket(client, warehouse, externalRef);
			docketToNotMatch.WD_ExternalReferenceSplit = 0;

			Factory.SaveForTesting();

			// some dockets will generate their own ext ref on save if not in DB.
			docketToMatch.WD_ExternalReference = "123";
			Factory.SaveForTesting();

			var loadedDocket = GetNewReader(Data.ShipmentDataObject, Logger).ReadIntoBusinessObject();
			AssertEquals(docketToMatch, loadedDocket);
			AssertEquals(false, Logger.HasErrors);
		}

		protected virtual bool DocketSupportsSplitNo
		{
			get { return true; }
		}

		#endregion

		#region TestRejectImportIfCancelledDocketMatched

		public void TestRejectImportIfCancelledDocketMatched()
		{
			var docket = GetNewDocket(Data.Orgs.CRAHOLSYD, Data.GetOrCreateWarehouseInDB(), "123");
			docket.WD_DocketStatus = DocketStatus.Codes.Cancelled;
			Factory.SaveForTesting();
			AssertEquals("Precondition - Docket should be Cancelled.", true, docket.IsCancelled);

			Data.ShipmentDataObject.DataContext.AddDataTarget(DataContext, null);
			Data.ShipmentDataObject.Order.OrderNumber = "123";
			var importResults = GetImportResultsViaDataContextManager(ShipmentDataObject);
			var failureResult = importResults.Single(r => r.DataContextType == DataContext);
			var expectedFailureMsg = string.Format("Warehouse {0} could not be updated because it is Canceled.", GetDocketType() + " " + docket.WD_DocketID);
			AssertEquals(false, failureResult.WasSuccessful);
			AssertContains(expectedFailureMsg, failureResult.ToString());
		}

		public void TestDataContextIsAttachedToXmlSessionTracker()
		{
			Data.ShipmentDataObject.DataContext.AddDataTarget(DataContext, null);
			Data.ShipmentDataObject.Order.OrderNumber = "123";

			var importFactory = new UniversalObjectFactory(new BusinessObjectFactory() { NameForDebugging = "Universal Message Processing" });
			var message = GetQueuedUniversalShipmentMessage(ShipmentDataObject);
			Factory.SaveForTesting();

			var serviceTask = new UMIServiceTask() { ServiceLogger = new TestServiceLogger() };
			serviceTask.RunTask();
			AssertNotContains("Exception", serviceTask.ServiceLogger.ToString());
		}

		protected IEnumerable<IImportResult> GetImportResultsViaDataContextManager(UniversalShipment dataObject)
		{
			var message = GetQueuedUniversalShipmentMessage(dataObject);
			var manager = new UniversalMessageProcessingManager(new ServiceTaskLogForTesting());
			IXmlSessionTracker tracker;
			using (ObjectFactory.Substitute<IAllocationEngineManager>(new AllocateFIFOLegacyMock()))
			{
				tracker = manager.Process(message);
			}
			return tracker.ImportResults;
		}

		#endregion

		// create / update job

		#region TestLineCollectionContentIsPartial

		public void TestOrderLineCollectionContentIsPartial_AddNewLineToExistingDocket()
		{
			var data = new TestDataSimpleEnvironment(Factory.BOFactory, saveFactory_doNotUseForNewTests: false);
			var docket = CreateDocketWithLine(data.Org1, data.Whs1, data.Part1, 10m);
			var shipmentDataObject = GetNewDocketDataObject(docket);
			var reader = GetNewReader(shipmentDataObject, Logger);
			var matchedDocket = reader.ReadIntoBusinessObject();
			AssertEquals(docket, matchedDocket);

			shipmentDataObject.Order.OrderLineCollection.Content = CollectionContent.Partial;
			var orderLineDataObject2 = GetSecondOrderLineDataObjectForPartialCollectionImport(shipmentDataObject.Order);
			orderLineDataObject2.OrderedQty = 10m;

			reader = GetNewReader(shipmentDataObject, Logger);
			var matchedDocket2 = reader.ReadIntoBusinessObject();
			AssertEquals("Should find 2 docket lines (1 pre-existing, 1 newly added).", 2, matchedDocket2.Lines.Count);

			var line1 = matchedDocket2.Lines.Single(l => l.WE_LineNo == 1);
			AssertEquals("Pre-existing line should stay.", 10m, line1.WE_TransactionQuantity);

			var line2 = matchedDocket2.Lines.Single(l => l.WE_LineNo == 2);
			AssertEquals("Added a new order line.", 10m, line2.WE_TransactionQuantity);
		}

		public void TestOrderLineCollectionContentIsPartial_AddNewLineAndUpdateToExistingDocket()
		{
			var data = new TestDataSimpleEnvironment(Factory.BOFactory, saveFactory_doNotUseForNewTests: false);
			var docket = CreateDocketWithLine(data.Org1, data.Whs1, data.Part1, 10m);
			var shipmentDataObject = GetNewDocketDataObject(docket);

			var reader = GetNewReader(shipmentDataObject, Logger);
			var matchedDocket = reader.ReadIntoBusinessObject();
			AssertEquals("Docket loaded should be as same as original docket.", docket, matchedDocket);
			AssertEquals("Docket should have 1 docket line.", 1, matchedDocket.Lines.Count);

			var docketLine1 = matchedDocket.Lines.Single();
			AssertEquals("Pre-condition: WE_LineNo", (ZShort)1, docketLine1.WE_LineNo);
			AssertEquals("Pre-condition: WE_TransactionQuantity", 10m, docketLine1.WE_TransactionQuantity);
			SetupAndAssertDocketLinePrecondition(docketLine1);

			// existing line, update a field
			shipmentDataObject.Order.OrderLineCollection.Content = CollectionContent.Partial;
			var docketLineDataObject = shipmentDataObject.Order.OrderLineCollection.Single();
			UpdateExistingDocketLineDataObject(docketLineDataObject);

			// new line
			var newOrderLine = GetSecondOrderLineDataObjectForPartialCollectionImport(shipmentDataObject.Order, isNewLine: true);
			newOrderLine.OrderedQty = 5m;
			newOrderLine.Product = docketLineDataObject.Product;
			shipmentDataObject.Order.OrderLineCollection.Add(newOrderLine);

			reader = GetNewReader(shipmentDataObject, Logger);
			var matchedDocket2 = reader.ReadIntoBusinessObject();
			AssertEquals("Should find 2 order lines (1 pre-existing, 1 newly added).", 2, matchedDocket2.Lines.Count);

			var line1 = matchedDocket2.Lines.Single(l => l.WE_LineNo == 1);
			AssertEquals("WE_TransactionQuantity stays same.", 10m, line1.WE_TransactionQuantity);
			AssertExistingDocketLineAfterImport(line1);

			var line2 = matchedDocket2.Lines.Single(l => l.WE_LineNo == 2);
			AssertEquals("Added a new order line.", 5m, line2.WE_TransactionQuantity);
		}

		protected virtual OrderLine GetSecondOrderLineDataObjectForPartialCollectionImport(Order orderDataObject, bool isNewLine = false)
		{
			var orderLineDataObject = isNewLine ? new OrderLine() : orderDataObject.OrderLineCollection.Single();
			orderLineDataObject.LineNumber = 2;
			return orderLineDataObject;
		}

		protected abstract void SetupAndAssertDocketLinePrecondition(WhsDocketLine docketLine);

		protected abstract void UpdateExistingDocketLineDataObject(OrderLine docketLineDataObject);

		protected abstract void AssertExistingDocketLineAfterImport(WhsDocketLine docketLine);

		protected abstract TDocket CreateDocketWithLine(OrgHeader client, WhsWarehouse warehouse, OrgSupplierPart product, ZDecimal units);
		protected abstract UniversalShipment GetNewDocketDataObject(TDocket docket);

		#endregion

		#region TestWhsDocketsOfDifferentTypesAreNotPickedUpByMatchingForTheTypeOfDocketWeWantToMatch

		public void TestWhsDocketsOfDifferentTypesAreNotPickedUpByMatchingForTheTypeOfDocketWeWantToMatch()
		{
			Data.CreateClientOrgCRAHOLSYDInDB();
			Data.GetOrCreateWarehouseInDB();

			var whsDocketOfDifferentType = GetNewDocketOfDifferentType();
			AssertNotEquals("Docket to test against should be of a different type", typeof(TDocket), whsDocketOfDifferentType.GetType());

			whsDocketOfDifferentType.WD_OH_Client = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, "CRAHOLSYD")).PK;
			whsDocketOfDifferentType.WD_ExternalReference = "ORDERME";

			Factory.SaveForTesting();

			ShipmentDataObject.Order.OrderNumber = "ORDERME";

			var reader = GetNewReader(ShipmentDataObject, Logger);
			var whsDocketBO = reader.ReadIntoBusinessObject();

			AssertNotNull(whsDocketBO);

			CombineAssertions(delegate
			{
				AssertNotEquals("Created a new docket", whsDocketOfDifferentType.PK, whsDocketBO.PK);
				AssertMultilineASCIIEquals("Logger.Logs", string.Format(@"
Information - Matching 'ConsignorDocumentaryAddress':- Matched to 'CRAHOLSYD' by code, address '' (only address).
Information - No matching {0} found, creating new {0}.
Information - Populating {0}...
Information - Matching 'ConsignorDocumentaryAddress':- Matched to 'CRAHOLSYD' by code, address '' (only address).
Information - Added Warehouse {1} from UniversalShipment.
".Trim(), typeof(TDocket).Name, GetDocketType()), Logger.Logs);
			});
		}

		protected abstract WhsDocket GetNewDocketOfDifferentType();

		#endregion

		#region TestThrowsExceptionIfDocketDoesNotHaveEnoughInformation

		public void TestThrowsExceptionIfDocketDoesNotHaveEnoughInformation()
		{
			Data.CreateEmptyShipmentDataObject();
			var client = Data.CreateClientOrgCRAHOLSYDInDB();
			var warehouse = Data.GetOrCreateWarehouseInDB();

			var reader = GetNewReader(ShipmentDataObject, Logger, true);
			AssertExceptionThrown("Can't read in the Docket without a valid Warehouse and Client Address.", typeof(DataObjectReadFailureException),
				string.Format("Cannot Import {0}\r\nNo Client Address was provided.\r\nNo Warehouse was provided.", GetDocketType()), () => reader.ReadIntoBusinessObject());

			ShipmentDataObject.Order = new Order { Warehouse = new UniversalDataBuss.DataObjects.Universal.Warehouse { Code = "WHS" } };
			reader = GetNewReader(ShipmentDataObject, Logger, true);
			AssertExceptionThrown("Can't read in the Docket without a valid Client Address.", typeof(DataObjectReadFailureException),
				string.Format("Cannot Import {0}\r\nNo Client Address was provided.", GetDocketType()), () => reader.ReadIntoBusinessObject());

			var randomAddress = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance)
			{
				AddressShortCode = "LO",
				AddressType = "ConsignorDocumentaryAddress",
				Address1 = "322 Local Street",
				Address2 = "Loco",
				City = "Loca",
				CompanyName = "Local Company",
				Country = new Country { Code = "LOC", Name = "Locola" },
				Email = "Local@local.com",
				Fax = "11110000",
				GovRegNum = "101",
				GovRegNumType = new RegistrationNumberType { Code = "OCL", Description = "Obssessed Crazy Local" },
				Mobile = "0411001100",
				Phone = "0211001100",
				Port = new UNLOCO { Code = "AUSYD", Name = "Sydney" },
				Postcode = "1010",
				State = "LCO",
				OrganizationCode = "LOCALCOMP",
				ScreeningStatus = new CodeDescriptionPair { Code = "LUC", Description = "Lucid" },
				UniversalNettingCode = "LOLOC",
				UniversalOfficeCode = "LILAC",
			};
			randomAddress.SetRegistrationNumberCollection(() => new List<RegistrationNumber> { new RegistrationNumber { Type = new RegistrationNumberType { Code = "GST", Description = "GST" }, Value = "LOLOLO" },
				new RegistrationNumber { CountryOfIssue = new Country { Code = "NZ", Name = "New Zealand" }, Type = new RegistrationNumberType { Code = "LST", Description = "LST" }, Value = "OLOLOL" } });

			ShipmentDataObject.SetOrganizationAddressCollection(() => new List<OrganizationAddress>() { randomAddress });
			reader = GetNewReader(ShipmentDataObject, Logger, true);
			AssertExceptionThrown("Can't read in the Docket without a valid Client Address.", typeof(DataObjectReadFailureException),
string.Format(@"
Cannot Import {0}
Unable to match Client Address, please make sure the supplied Client Address is valid. Details were:
Address1: 322 Local Street
Address2: Loco
AddressShortCode: LO
City: Loca
CompanyName: Local Company
Country: LOC - Locola
Email: Local@local.com
Fax: 11110000
GovRegNum: 101
GovRegNumType: OCL - Obssessed Crazy Local
Mobile: 0411001100
OrganizationCode: LOCALCOMP
Phone: 0211001100
Port: AUSYD - Sydney
Postcode: 1010
State: LCO
UniversalNettingCode: LOLOC
UniversalOfficeCode: LILAC
RegistrationNumber 1:
Type: GST - GST
Value: LOLOLO
RegistrationNumber 2:
CountryOfIssue: NZ - New Zealand
Type: LST - LST
Value: OLOLOL
			".Trim(), GetDocketType()), () => reader.ReadIntoBusinessObject());

			ShipmentDataObject.SetOrganizationAddressCollection(() => new List<OrganizationAddress>() { GetNewAddressData_CRAHOLSYD(nameof(DocAddressType.ConsignorDocumentaryAddress)) });
			reader = GetNewReader(ShipmentDataObject, Logger, true);
			AssertNoExceptionThrown("Docket reads in fine", () => reader.ReadIntoBusinessObject());

			ShipmentDataObject.Order.Warehouse = null;
			reader = GetNewReader(ShipmentDataObject, Logger, true);

			if (IsExceptionThrownWhenNoOrInvalidWarehouseSpecifiedButClientIsSpecifiedAndValidWarehouseIsInDB)
			{
				AssertExceptionThrown("Can't read in the Docket without a valid Warehouse.", typeof(DataObjectReadFailureException),
					string.Format("Cannot Import {0}\r\nNo Warehouse was provided.", GetDocketType()), () => reader.ReadIntoBusinessObject());
			}
			else
			{
				AssertNoExceptionThrown("Docket reads in fine", () => reader.ReadIntoBusinessObject());
			}

			var whsDocketToLoad = GetNewDocket(client, warehouse);
			whsDocketToLoad.WD_ExternalReference = "ORDERME";

			Factory.SaveForTesting();
			whsDocketToLoad.WD_ExternalReference = "ORDERME";
			Factory.SaveForTesting();

			ShipmentDataObject.Order.OrderNumber = "ORDERME";
			reader = GetNewReader(ShipmentDataObject, Logger);
			AssertNoExceptionThrown("Docket reads in fine", () => reader.ReadIntoBusinessObject());

			ShipmentDataObject.Order.Warehouse = new UniversalDataBuss.DataObjects.Universal.Warehouse { Code = "WHS" };
			reader = GetNewReader(ShipmentDataObject, Logger);
			AssertNoExceptionThrown("Docket reads in fine", () => reader.ReadIntoBusinessObject());

			ShipmentDataObject.Order.Warehouse = null;
			ShipmentDataObject.SetOrganizationAddressCollection(() => new List<OrganizationAddress>() { GetNewAddressData_CRAHOLSYD(nameof(DocAddressType.ConsignorDocumentaryAddress)) });
			reader = GetNewReader(ShipmentDataObject, Logger);
			AssertNoExceptionThrown("Docket reads in fine", () => reader.ReadIntoBusinessObject());

			ShipmentDataObject.Order.Warehouse = new UniversalDataBuss.DataObjects.Universal.Warehouse { Code = "WWW" };
			reader = GetNewReader(ShipmentDataObject, Logger);

			if (IsExceptionThrownWhenNoOrInvalidWarehouseSpecifiedButClientIsSpecifiedAndValidWarehouseIsInDB)
			{
				AssertExceptionThrown("Can't read in the Docket without a valid Warehouse.", typeof(DataObjectReadFailureException),
					string.Format("Cannot Import {0}\r\nUnable to match Warehouse: WWW.", GetDocketType()), () => reader.ReadIntoBusinessObject());
			}
			else
			{
				AssertNoExceptionThrown("Docket reads in fine", () => reader.ReadIntoBusinessObject());
			}
		}

		protected virtual bool IsExceptionThrownWhenNoOrInvalidWarehouseSpecifiedButClientIsSpecifiedAndValidWarehouseIsInDB
		{
			get { return true; }
		}

		#endregion

		#region TestWorkflowCustomFieldsOnDocketAreImported

		protected abstract string GetProcessType();

		public void TestWorkflowCustomFieldsOnDocketAreImported()
		{
			Data.CreateClientOrgCRAHOLSYDInDB();
			Data.GetOrCreateWarehouseInDB();

			#region Setup Template

			var processTaskTemplate = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			processTaskTemplate.P0_ProcessType = GetProcessType();
			processTaskTemplate.P0_IsActive = true;

			var genCustomColumnString = Factory.New<GenCustomColumnDefinition>();
			genCustomColumnString.XC_Name = "Textual context";
			genCustomColumnString.XC_Type = "STR";

			processTaskTemplate.GenCustomColumnDefinitions.Add(genCustomColumnString);

			var genCustomColumnDate = Factory.New<GenCustomColumnDefinition>();
			genCustomColumnDate.XC_Name = "First Date";
			genCustomColumnDate.XC_Type = "DAT";

			processTaskTemplate.GenCustomColumnDefinitions.Add(genCustomColumnDate);

			var genCustomColumnDecimal = Factory.New<GenCustomColumnDefinition>();
			genCustomColumnDecimal.XC_Name = "Deci Deca";
			genCustomColumnDecimal.XC_Type = "DEC";

			processTaskTemplate.GenCustomColumnDefinitions.Add(genCustomColumnDecimal);

			var genCustomColumnBool = Factory.New<GenCustomColumnDefinition>();
			genCustomColumnBool.XC_Name = "Flagger";
			genCustomColumnBool.XC_Type = "BOO";

			processTaskTemplate.GenCustomColumnDefinitions.Add(genCustomColumnBool);

			var genCustomColumnBool2 = Factory.New<GenCustomColumnDefinition>();
			genCustomColumnBool2.XC_Name = "Flag This!";
			genCustomColumnBool2.XC_Type = "BOO";

			processTaskTemplate.GenCustomColumnDefinitions.Add(genCustomColumnBool2);

			var genCustomColumnInt = Factory.New<GenCustomColumnDefinition>();
			genCustomColumnInt.XC_Name = "Integer Mate";
			genCustomColumnInt.XC_Type = "INT";

			processTaskTemplate.GenCustomColumnDefinitions.Add(genCustomColumnInt);

			var genCustomColumnInt2 = Factory.New<GenCustomColumnDefinition>();
			genCustomColumnInt2.XC_Name = "Integraler";
			genCustomColumnInt2.XC_Type = "INT";

			processTaskTemplate.GenCustomColumnDefinitions.Add(genCustomColumnInt2);

			Factory.SaveForTesting();

			#endregion

			ShipmentDataObject.SetCustomizedFieldCollection(() => new List<CustomizedField>());
			ShipmentDataObject.CustomizedFieldCollection.Add(CustomizedField.New("Textual context", new ZString("HELLO")));
			ShipmentDataObject.CustomizedFieldCollection.Add(CustomizedField.New("Customs are customary", new ZString("GOODBYE")));
			ShipmentDataObject.CustomizedFieldCollection.Add(CustomizedField.New("First Date", new ZDateTime(2011, 1, 1)));
			ShipmentDataObject.CustomizedFieldCollection.Add(CustomizedField.New("Last Date", new ZDateTime(2011, 1, 2)));
			ShipmentDataObject.CustomizedFieldCollection.Add(CustomizedField.New("Deci Deca", new ZDecimal(0.3)));
			ShipmentDataObject.CustomizedFieldCollection.Add(CustomizedField.New("+ 1 point zero", new ZDecimal(1.3)));
			ShipmentDataObject.CustomizedFieldCollection.Add(CustomizedField.New("Flagger", ZBool.True));
			var incorrectCustomField = new CustomizedField { Key = "Flag This!", Value = new ZString("I am NOT a BOOLEAN!"), DataType = DataType.Boolean };
			ShipmentDataObject.CustomizedFieldCollection.Add(incorrectCustomField);
			ShipmentDataObject.CustomizedFieldCollection.Add(CustomizedField.New("Integer Mate", new ZInt(42)));
			ShipmentDataObject.CustomizedFieldCollection.Add(CustomizedField.New("Integraler", ZInt.Zero));
			ShipmentDataObject.CustomizedFieldCollection.Add(CustomizedField.New("Bogus Custom Field", new ZString("I am BOGUS")));

			var reader = GetNewReader(ShipmentDataObject, Logger);
			var whsDocketBO = reader.ReadIntoBusinessObject();

			CombineAssertions(() =>
			{
				var customFields = whsDocketBO.GetUserDefinedValues();

				if (SupportsWorkflowCustomFieldImport)
				{
					var customFieldsString = customFields.Select(f => f.PropertyName + " - " + f.Value).ToList();
					Assert("Custom Field 1 not found", customFieldsString.Contains("Deci Deca - 0.3"));
					Assert("Custom Field 2 not found", customFieldsString.Contains("First Date - 01-Jan-11 00:00:00"));
					Assert("Custom Field 3 not found", customFieldsString.Contains("Flagger - Y"));
					Assert("Custom Field 4 not found", customFieldsString.Contains("Integer Mate - 42"));
					Assert("Custom Field 5 not found", customFieldsString.Contains("Textual context - HELLO"));

					AssertMultilineASCIIEquals("Logger.Logs", string.Format(@"
Information - Matching 'ConsignorDocumentaryAddress':- Matched to 'CRAHOLSYD' by code, address '' (only address).
Information - No matching {0} found, creating new {0}.
Information - Populating {0}...
Information - Matching 'ConsignorDocumentaryAddress':- Matched to 'CRAHOLSYD' by code, address '' (only address).
Warning - Custom Fields - Invalid value [I am NOT a BOOLEAN!]. Value must be a valid Boolean (true or false).
Information - Added Warehouse {1} from UniversalShipment.
".Trim(), typeof(TDocket).Name, GetDocketType()), Logger.Logs);
				}
				else
				{
					AssertEquals("Should not have imported custom fields.", 0, customFields.Count());
				}
			});
		}

		protected virtual bool SupportsWorkflowCustomFieldImport => true;

		#endregion

		#region TestOrganisationLevelCustomFieldsAreImported

		public void TestOrganisationLevelCustomFieldsAreImported()
		{
			if (SupportsWorkflowCustomFieldImport)
			{
				Data.GetOrCreateWarehouseInDB();
				var client = Data.CreateClientOrgCRAHOLSYDInDB();
				Data.SetupCustomLabels(client);
				Data.AddCustomFieldsToDataObject(ShipmentDataObject);

				var reader = GetNewReader(ShipmentDataObject, Logger);
				var docket = reader.ReadIntoBusinessObject();

				AssertNotNull(docket);
				CombineAssertions(delegate
				{
					AssertCustomFieldsImported(new WhsDocket.CustomLabelsProvider(docket), docket);
				});
			}
			else
			{
				Assert(true);
			}
		}

		#endregion

		#region TestImportingOfClient

		public void TestImportingOfClient()
		{
			Data.CreateClientOrgCRAHOLSYDInDB();
			Data.GetOrCreateWarehouseInDB();

			var reader = GetNewReader(ShipmentDataObject, Logger);
			var docket = reader.ReadIntoBusinessObject();
			AssertAddressContentMatches_CRAHOLSYD(docket.Client.MainAddress);
		}

		#endregion

		#region TestImportingOfClient_PreventChangingIfThereAreLines

		public void TestImportingOfClient_PreventChangingIfThereAreLines()
		{
			Data.CreateClientOrgCRAHOLSYDInDB();
			var otherClient = Helper.CreateClient("CCL", "CoolClient");

			var docket = GetNewDocket(otherClient, Data.GetOrCreateWarehouseInDB(), "123");
			var line = docket.Lines.AddNew();
			line.FillWithValidTestData();
			Factory.SaveForTesting();

			Data.ShipmentDataObject.DataContext.AddDataTarget(DataContext, docket.WD_DocketID);
			Data.ShipmentDataObject.Order.OrderNumber = "123";
			Data.ShipmentDataObject.Order.SetOrderLineCollection(() => GetDummyOrderLineList(otherClient));

			var reader1 = GetNewReader(Data.ShipmentDataObject, Logger);
			var docketBO = reader1.ReadIntoBusinessObject();
			AssertEquals("Should have updated the docket.", docketBO, docket);
			AssertNotNull(docketBO);

			AssertEquals("Should not have updated client.", otherClient.PK, docket.WD_OH_Client);
			AssertNoExceptionThrown(() => Factory.SaveForTesting());
		}

		protected virtual DataObjectList<OrderLine> GetDummyOrderLineList(OrgHeader client)
		{
			return new DataObjectList<OrderLine>();
		}

		#endregion

		#region TestImportOfWarehouseFromWarehouseAddress

		public void TestImportOfWarehouseFromWarehouseAddress()
		{
			var client = Data.CreateClientOrgCRAHOLSYDInDB();
			var warehouse = Data.GetOrCreateWarehouseInDB();

			var clientParams = WhsClientParams.GetClientParams(client);
			clientParams.ClientParametersByWarehouse.AddNew(); // Hack for order to fail with having no warehouse
			ShipmentDataObject.Order.Warehouse = null; // Clear warehouse code

			var invalidWarehouseAddress = new OrganizationAddress { AddressType = nameof(DocAddressType.Warehouse), Address1 = "WILLNOTMATCH" };
			ShipmentDataObject.OrganizationAddressCollection.Add(invalidWarehouseAddress);

			var reader1 = GetNewReader(ShipmentDataObject, Logger);
			AssertExceptionThrown("Warehouse Address not linked to real Warehouse should be unable to Import.",
				typeof(DataObjectReadFailureException), string.Format("Cannot Import {0}\r\nNo Warehouse was provided.", GetDocketType()), () => reader1.ReadIntoBusinessObject());

			Logger.ClearLogs();
			ShipmentDataObject.OrganizationAddressCollection.Remove(invalidWarehouseAddress);
			ShipmentDataObject.OrganizationAddressCollection.Add(Data.Orgs.WarehouseAddressDataObject_INTHEMSYD);
			var reader2 = GetNewReader(ShipmentDataObject, Logger);
			var docket = reader2.ReadIntoBusinessObject();
			AssertNotNull(docket);
			AssertEquals(warehouse.PK, docket.Warehouse.PK);
			AssertEquals(false, Logger.HasErrors);
			AssertEquals("Warehouse is not a valid WhsDocket AddressType and should not be added to the Docket's Address collection.", false, Logger.HasWarnings);
		}

		#endregion

		#region TestImportOfWarehouseDoesNotConsiderTransitWarehouses

		public void TestImportOfWarehouseDoesNotConsiderTransitWarehouses()
		{
			var client = Data.CreateClientOrgCRAHOLSYDInDB();
			var warehouse = Data.GetOrCreateWarehouseInDB();
			warehouse.WW_WarehouseType = WarehouseTypes.Codes.Transit;
			Factory.SaveForTesting();

			var reader1 = GetNewReader(Data.ShipmentDataObject, Logger);
			AssertExceptionThrown("Should not be able to match a Transit Warehouse using Code.",
				typeof(DataObjectReadFailureException), string.Format("Cannot Import {0}\r\nUnable to match Warehouse: WHS - Coolhouse.", GetDocketType()), () => reader1.ReadIntoBusinessObject());

			Data.ShipmentDataObject.Order.Warehouse = null;
			Data.ShipmentDataObject.OrganizationAddressCollection.Add(Data.Orgs.WarehouseAddressDataObject_INTHEMSYD);
			var reader2 = GetNewReader(Data.ShipmentDataObject, Logger);
			AssertExceptionThrown("Should not be able to match a Transit Warehouse using Address.",
				typeof(DataObjectReadFailureException), string.Format("Cannot Import {0}\r\nNo Warehouse was provided.", GetDocketType()), () => reader2.ReadIntoBusinessObject());
		}

		#endregion

		#region TestImportOfWarehouse_WhenDocketHasLines

		public void TestImportOfWarehouse_WhenDocketHasLines()
		{
			TestImportOfWarehouse_WhenDocketHasLinesCore();
		}

		protected virtual void TestImportOfWarehouse_WhenDocketHasLinesCore()
		{
			var otherWhs = Helper.CreateWarehouse("WSS", "CoolShack");
			var docket = GetNewDocket(Data.Orgs.CRAHOLSYD, otherWhs, "123");
			var line = docket.Lines.AddNew();
			line.FillWithValidTestData();
			Factory.SaveForTesting();

			Data.GetOrCreateWarehouseInDB();
			Factory.SaveForTesting();

			Data.ShipmentDataObject.DataContext.AddDataTarget(DataContext, null);
			Data.ShipmentDataObject.Order.OrderNumber = "123";
			Data.ShipmentDataObject.Order.SetOrderLineCollection(() => GetDummyOrderLineList(Data.Orgs.CRAHOLSYD));

			var reader1 = GetNewReader(Data.ShipmentDataObject, Logger);
			var docketBO = reader1.ReadIntoBusinessObject();
			AssertNotNull(docketBO);
			AssertEquals("Should have updated the docket.", docketBO, docket);

			AssertEquals("Should not have updated warehouse.", "WSS", docket.Warehouse.WW_WarehouseCode);
			AssertNoExceptionThrown(() => Factory.SaveForTesting());
		}

		#endregion

		#region TestGetRelatedWarehouse_WithActiveAndInactiveWarehouses

		public void TestGetRelatedWarehouse_WithActiveAndInactiveWarehouses_ShouldIgnoreInactive()
		{
			// We will test only for orders and receives
			var client = Data.CreateClientOrgCRAHOLSYDInDB();
			var whs1 = Data.GetOrCreateWarehouseInDB();
			var whsAddress = whs1.WarehouseAddress;
			whsAddress.Header.OH_IsWarehouseClient = true;

			// Set inactive Warehouse with same address and branch as Whs1
			var whs2 = Helper.CreateWarehouse("000", "000", whs1.WarehouseAddress, whs1.RelatedCompanyBranch);
			whs2.WW_IsActive = false;
			whs2.WW_WarehouseType = "FTZ";

			var whs3 = Helper.CreateWarehouse("ZZZ", "ZZZ", whs1.WarehouseAddress, whs1.RelatedCompanyBranch);
			whs3.WW_IsActive = false;
			whs3.WW_WarehouseType = "FTZ";

			// Set CYD Warehouse with same address and branch as Whs1
			var whs4 = Helper.CreateWarehouse("AAA", "AAA", whs1.WarehouseAddress, whs1.RelatedCompanyBranch);
			whs4.WW_WarehouseType = "CYD";

			Factory.SaveForTesting();
			AssertEquals("Warehouse 2 must be inactive.", false, whs2.WW_IsActive);
			AssertEquals("Warehouse 3 must be inactive.", false, whs3.WW_IsActive);

			// Add Warehouse address to the order.
			ShipmentDataObject.OrganizationAddressCollection.Add(Data.Orgs.WarehouseAddressDataObject_INTHEMSYD);

			var order = ShipmentDataObject.Order;
			order.Warehouse = null;
			order.ClientReference = "";
			order.OrderNumber = "ORDERME";
			order.TotalUnits = 10m;
			order.UnitsSent = 10m;

			var reader = GetNewReader(ShipmentDataObject, Logger);
			var whsOrderBO = reader.ReadIntoBusinessObject();

			CombineAssertions(() =>
			{
				AssertNotNull("Order could not be read into business object", whsOrderBO);
				AssertEquals("Code: Expected to use active Warehouse but used the in-active Whs.", whs1.WW_WarehouseCode, whsOrderBO.Warehouse.WW_WarehouseCode);
				AssertEquals("PK: Expected to use active Warehouse but used the in-active Whs.", whs1.PK, whsOrderBO.WD_WW_Whs);
			});
		}

		#endregion

		#region TestGetRelatedWarehouse_WithCode_CorrectWarehouseType

		public void TestGetRelatedWarehouse_WithCode_CorrectWarehouseType()
		{
			Data.CreateClientOrgCRAHOLSYDInDB();
			var cydWarehouse = Data.GetOrCreateWarehouseInDB();
			cydWarehouse.WW_WarehouseType = "CYD";

			var productWarehouse = Helper.CreateWarehouse("AAA", "000", cydWarehouse.WarehouseAddress, cydWarehouse.RelatedCompanyBranch);

			// Add Warehouse address to the order.
			ShipmentDataObject.OrganizationAddressCollection.Add(Data.Orgs.WarehouseAddressDataObject_INTHEMSYD);

			var order = ShipmentDataObject.Order;
			order.Warehouse = new UniversalDataBuss.DataObjects.Universal.Warehouse { Code = "AAA", Name = "AAA" };
			order.ClientReference = "";
			order.OrderNumber = "ORDERME";
			order.TotalUnits = 10m;
			order.UnitsSent = 10m;

			var reader = GetNewReader(ShipmentDataObject, Logger);
			var whsOrderBO = reader.ReadIntoBusinessObject();

			AssertEquals("Expected to use product warehouse.", productWarehouse.PK, whsOrderBO.WD_WW_Whs);
		}

		#endregion

		#region TestGetRelatedWarehouse_MatchWithAddress

		public void TestGetRelatedWarehouse_MatchWithAddress()
		{
			Data.CreateClientOrgCRAHOLSYDInDB();
			var cydWarehouse = Data.GetOrCreateWarehouseInDB();
			cydWarehouse.WW_WarehouseType = "CYD";

			var productWarehouse = Helper.CreateWarehouse("AAA", "000", cydWarehouse.WarehouseAddress, cydWarehouse.RelatedCompanyBranch);

			// Add Warehouse address to the order.
			ShipmentDataObject.OrganizationAddressCollection.Add(Data.Orgs.WarehouseAddressDataObject_INTHEMSYD);

			var order = ShipmentDataObject.Order;
			order.Warehouse = null;
			order.ClientReference = "";
			order.OrderNumber = "ORDERME";
			order.TotalUnits = 10m;
			order.UnitsSent = 10m;

			var reader = GetNewReader(ShipmentDataObject, Logger);
			var whsOrderBO = reader.ReadIntoBusinessObject();

			AssertEquals("Expected to use product warehouse.", productWarehouse.PK, whsOrderBO.WD_WW_Whs);
		}

		#endregion

		#region TestRejectImportDueToInvalidCharacters

		public void TestRejectImportDueToInvalidCharacters_ExternalReference()
		{
			Data.CreateClientOrgCRAHOLSYDInDB();
			Data.GetOrCreateWarehouseInDB();

			var order = ShipmentDataObject.Order;
			order.OrderNumber = "你好";

			var reader = GetNewReader(ShipmentDataObject, Logger);

			var expectedErrorString = @"Cannot perform import due to invalid characters in field: OrderNumber.";
			AssertExceptionThrown(typeof(DataObjectReadFailureException), expectedErrorString, () => reader.ReadIntoBusinessObject());
		}

		public void TestRejectImportDueToInvalidCharacters_ClientReference()
		{
			Data.CreateClientOrgCRAHOLSYDInDB();
			Data.GetOrCreateWarehouseInDB();

			var order = ShipmentDataObject.Order;
			order.ClientReference = "你好";

			var reader = GetNewReader(ShipmentDataObject, Logger);

			if (SupportsClientReferenceImport)
			{
				var expectedErrorString = @"Cannot perform import due to invalid characters in field: ClientReference.";
				AssertExceptionThrown(typeof(DataObjectReadFailureException), expectedErrorString, () => reader.ReadIntoBusinessObject());
			}
			else
			{
				AssertNoExceptionThrown(() => reader.ReadIntoBusinessObject());
			}
		}

		protected virtual bool SupportsClientReferenceImport => false;

		public void TestRejectImportDueToInvalidCharacters_TransportReference()
		{
			Data.CreateClientOrgCRAHOLSYDInDB();
			Data.GetOrCreateWarehouseInDB();

			var order = ShipmentDataObject.Order;
			order.TransportReference = "你好";

			var reader = GetNewReader(ShipmentDataObject, Logger);

			if (SupportsTransportReferenceImport)
			{
				var expectedErrorString = @"Cannot perform import due to invalid characters in field: TransportReference.";
				AssertExceptionThrown(typeof(DataObjectReadFailureException), expectedErrorString, () => reader.ReadIntoBusinessObject());
			}
			else
			{
				AssertNoExceptionThrown(() => reader.ReadIntoBusinessObject());
			}
		}

		protected virtual bool SupportsTransportReferenceImport => false;

		#endregion

		#region TestWithNotes

		public void TestWithNotes()
		{
			Data.CreateClientOrgCRAHOLSYDInDB();
			Data.GetOrCreateWarehouseInDB();

			var noteDataObject = SetupNote();
			noteDataObject.IsCustomDescription = false;

			ShipmentDataObject.Order.OrderNumber = "ORDERME";
			ShipmentDataObject.SetNoteCollection(() => new DataObjectList<Note>());
			ShipmentDataObject.NoteCollection.Add(noteDataObject);

			var reader = GetNewReader(ShipmentDataObject, Logger);
			var whsDocketBO = reader.ReadIntoBusinessObject();

			AssertNotNull(whsDocketBO);

			var note = whsDocketBO.Notes.FindByDescription("DOG FLOGGER!!");
			AssertEquals("Note Collection contains 'DOG FLOGGER!!' note.", 1, note.Length);

			CombineAssertions(() =>
			{
				AssertNoteContents(note[0]);
				AssertEquals("noteBO.ST_IsCustomDescription", true, note[0].ST_IsCustomDescription);
				AssertMultilineASCIIEquals("Logger.Logs", string.Format(@"
Information - Matching 'ConsignorDocumentaryAddress':- Matched to 'CRAHOLSYD' by code, address '' (only address).
Information - No matching {0} found, creating new {0}.
Information - Populating {0}...
Information - Matching 'ConsignorDocumentaryAddress':- Matched to 'CRAHOLSYD' by code, address '' (only address).
Information - No matching StmNote found, creating new StmNote.
Information - Populating StmNote...
Warning - Description(value: DOG FLOGGER!!) is a custom note. IsCustomDescription(value: False/empty) was ignored.
Information - Added Warehouse {1} from UniversalShipment.
				".Trim(), typeof(TDocket).Name, GetDocketType()), Logger.Logs);
			});
		}

		#endregion

		#region TestAdditionalReferenceNumbers

		public void TestAdditionalReferenceNumbers()
		{
			Data.CreateClientOrgCRAHOLSYDInDB();
			Data.GetOrCreateWarehouseInDB();

			ShipmentDataObject.Order.OrderNumber = "ORDERME";
			ShipmentDataObject.SetAdditionalReferenceCollection(() => new DataObjectList<AdditionalReference>());

			var additionalReferenceNumberDataObject = new AdditionalReference { ReferenceNumber = "112233", Type = new EntryType { Code = "TRF", Description = "HELLO" } };
			ShipmentDataObject.AdditionalReferenceCollection.Add(additionalReferenceNumberDataObject);

			var reader = GetNewReader(ShipmentDataObject, Logger);
			var whsDocketBO = reader.ReadIntoBusinessObject();

			AssertNotNull(whsDocketBO);

			if (SupportsAdditionalReferences)
			{
				CombineAssertions(() =>
				{
					AssertEquals("whsDocketBO.References.Count", 1, whsDocketBO.References.Count);

					var additionalReferenceNumberBO = whsDocketBO.References[0];
					AssertEquals("additionalReferenceNumberBO.WX_Reference", "112233", additionalReferenceNumberBO.WX_Reference);
					AssertEquals("additionalReferenceNumberBO.WX_RefType", "TRF", additionalReferenceNumberBO.WX_RefType);

					AssertMultilineASCIIEquals("Logger.Logs", string.Format(@"
Information - Matching 'ConsignorDocumentaryAddress':- Matched to 'CRAHOLSYD' by code, address '' (only address).
Information - No matching {0} found, creating new {0}.
Information - Populating {0}...
Information - Matching 'ConsignorDocumentaryAddress':- Matched to 'CRAHOLSYD' by code, address '' (only address).
Information - No matching WhsDocketReference found, creating new WhsDocketReference.
Information - Populating WhsDocketReference...
Information - Added Warehouse {1} from UniversalShipment.
".Trim(), typeof(TDocket).Name, GetDocketType()), Logger.Logs);
				});
			}
			else
			{
				CombineAssertions(() =>
				{
					AssertEquals("whsDocketBO.References.Count", 0, whsDocketBO.References.Count);

					AssertMultilineASCIIEquals("Logger.Logs", string.Format(@"
Information - Matching 'ConsignorDocumentaryAddress':- Matched to 'CRAHOLSYD' by code, address '' (only address).
Information - No matching {0} found, creating new {0}.
Information - Populating {0}...
Information - Matching 'ConsignorDocumentaryAddress':- Matched to 'CRAHOLSYD' by code, address '' (only address).
Information - Added Warehouse {1} from UniversalShipment.
".Trim(), typeof(TDocket).Name, GetDocketType()), Logger.Logs);
				});
			}
		}

		public void TestAdditionalReferenceNumbers_TPCType()
		{
			Data.CreateClientOrgCRAHOLSYDInDB();
			Data.GetOrCreateWarehouseInDB();

			ShipmentDataObject.Order.OrderNumber = "ORDERME";

			var additionalReferences = new DataObjectList<AdditionalReference>();
			var otherReference = new AdditionalReference
			{
				ReferenceNumber = "555666",
				Type = new EntryType { Code = WarehouseAdditionalReferenceTypes.Codes.OrderTypeCode, Description = WarehouseAdditionalReferenceTypes.Descriptions.OrderTypeCode }
			};
			additionalReferences.Add(otherReference);
			ShipmentDataObject.SetAdditionalReferenceCollection(() => additionalReferences);

			var duplicatedTPCReference = new AdditionalReference
			{
				ReferenceNumber = "777888",
				Type = new EntryType { Code = WarehouseAdditionalReferenceTypes.Codes.ThirdPartyCarrierAccountNumber, Description = WarehouseAdditionalReferenceTypes.Descriptions.ThirdPartyCarrierAccountNumber }
			};

			ShipmentDataObject.AdditionalReferenceCollection.Add(duplicatedTPCReference);

			var reader = GetNewReader(ShipmentDataObject, Logger);
			var whsDocketBO = reader.ReadIntoBusinessObject();

			AssertNotNull(whsDocketBO);
			if (SupportsAdditionalReferences)
			{
				AssertEquals("whsDocketBO.References.Count", 2, whsDocketBO.References.Count);

				var additionalReferenceNumberBO1 = whsDocketBO.References[0];
				AssertEquals("additionalReferenceNumberBO.WX_Reference", "555666", additionalReferenceNumberBO1.WX_Reference);
				AssertEquals("additionalReferenceNumberBO.WX_RefType", WarehouseAdditionalReferenceTypes.Codes.OrderTypeCode, additionalReferenceNumberBO1.WX_RefType);

				var additionalReferenceNumberBO2 = whsDocketBO.References[1];
				AssertEquals("additionalReferenceNumberBO.WX_Reference", "777888", additionalReferenceNumberBO2.WX_Reference);
				AssertEquals("additionalReferenceNumberBO.WX_RefType", WarehouseAdditionalReferenceTypes.Codes.ThirdPartyCarrierAccountNumber, additionalReferenceNumberBO2.WX_RefType);

				AssertMultilineASCIIEquals("Logger.Logs", string.Format(@"
Information - Matching 'ConsignorDocumentaryAddress':- Matched to 'CRAHOLSYD' by code, address '' (only address).
Information - No matching {0} found, creating new {0}.
Information - Populating {0}...
Information - Matching 'ConsignorDocumentaryAddress':- Matched to 'CRAHOLSYD' by code, address '' (only address).
Information - No matching WhsDocketReference found, creating new WhsDocketReference.
Information - Populating WhsDocketReference...
Information - No matching WhsDocketReference found, creating new WhsDocketReference.
Information - Populating WhsDocketReference...
Information - Added Warehouse {1} from UniversalShipment.
".Trim(), typeof(TDocket).Name, GetDocketType()), Logger.Logs);
			}
		}

		public void TestAdditionalReferenceNumbers_DuplicatedTPCTypes()
		{
			Data.CreateClientOrgCRAHOLSYDInDB();
			Data.GetOrCreateWarehouseInDB();

			ShipmentDataObject.Order.OrderNumber = "ORDERME";

			var additionalReferences = new DataObjectList<AdditionalReference>();
			var tpcReference = new AdditionalReference
			{
				ReferenceNumber = "555666",
				Type = new EntryType { Code = WarehouseAdditionalReferenceTypes.Codes.ThirdPartyCarrierAccountNumber, Description = WarehouseAdditionalReferenceTypes.Descriptions.ThirdPartyCarrierAccountNumber }
			};
			additionalReferences.Add(tpcReference);
			ShipmentDataObject.SetAdditionalReferenceCollection(() => additionalReferences);

			var duplicatedTPCReference = new AdditionalReference
			{
				ReferenceNumber = "777888",
				Type = new EntryType { Code = WarehouseAdditionalReferenceTypes.Codes.ThirdPartyCarrierAccountNumber, Description = WarehouseAdditionalReferenceTypes.Descriptions.ThirdPartyCarrierAccountNumber }
			};

			ShipmentDataObject.AdditionalReferenceCollection.Add(duplicatedTPCReference);

			AssertEquals(2, ShipmentDataObject.AdditionalReferenceCollection.Count(ar => ar.Type.Code.Value == WarehouseAdditionalReferenceTypes.Codes.ThirdPartyCarrierAccountNumber));

			var reader = GetNewReader(ShipmentDataObject, Logger);

			if (SupportsAdditionalReferences)
			{
				// Assert Error Message
				AssertExceptionThrown(typeof(DataObjectReadFailureException), string.Format("Unable to import duplicate TPC Reference: {0}.", duplicatedTPCReference.ReferenceNumber), () => reader.ReadIntoBusinessObject());
			}
		}

		protected virtual bool SupportsAdditionalReferences => true;

		#endregion

		#region TestImportSetsDataImporting

		public void TestImportSetsDataImporting()
		{
			Data.CreateClientOrgCRAHOLSYDInDB();
			Data.GetOrCreateWarehouseInDB();

			var reader = new WhsDocketDataObjectReaderForTest(ShipmentDataObject, Logger, Factory);

			bool beforePopulateBusinessObjectCompletesFired = false;
			reader.BeforePopulateBusinessObjectCompletes += (sender, e) =>
			{
				AssertEquals(true, ((ISupportDataImporting)e.Docket).IsImportingData);
				beforePopulateBusinessObjectCompletesFired = true;
			};

			ISupportDataImporting importedDocket = reader.ReadIntoBusinessObject();
			AssertEquals(true, beforePopulateBusinessObjectCompletesFired);
			AssertEquals(false, importedDocket.IsImportingData);
		}

		#region TestImportFailsIfNotWarehouseClient

		public void TestImportFailsIfNotWarehouseClient()
		{
			var client = Data.CreateClientOrgCRAHOLSYDInDB();
			client.OH_IsWarehouseClient = false;
			Data.GetOrCreateWarehouseInDB();

			var reader = GetNewReader(ShipmentDataObject, Logger);

			AssertExceptionThrown(typeof(DataObjectReadFailureException),
				string.Format("Cannot Import {0}\r\nThe matched Organization {1} must be marked as a Warehouse Client.", GetDocketType(), client.OH_FullName), () => reader.ReadIntoBusinessObject());
		}

		#endregion

		#region class WhsDocketDataObjectReaderForTest

		class WhsDocketDataObjectReaderForTest : WhsDocketDataObjectReader<TDocket, TDocketLine>
		{
			public WhsDocketDataObjectReaderForTest(UniversalShipment docketDataObject, IXmlImportLogger logger, UniversalObjectFactory factory)
				: base(docketDataObject, logger, factory)
			{
			}

			protected override void PopulateBusinessObjectCore(TDocket docket)
			{
				if (AddRowErrorAction != null)
				{
					AddRowErrorAction(docket);
				}
				base.PopulateBusinessObjectCore(docket);
				OnBeforePopulateBusinessObjectCompletes(this, docket);
			}

			public Action<TDocket> AddRowErrorAction;

			public event EventHandler<PopulateBusinessObjectEventArgs> BeforePopulateBusinessObjectCompletes;

			void OnBeforePopulateBusinessObjectCompletes(object sender, TDocket docket)
			{
				if (BeforePopulateBusinessObjectCompletes != null)
				{
					BeforePopulateBusinessObjectCompletes(this, new PopulateBusinessObjectEventArgs(docket));
				}
			}

			#region Abstract Implementation

			protected override string DocketTypeCode
			{
				get { return "TST"; }
			}

			protected override void AddAdditionalFilter(UniversalShipment dataObject, ZQuery query)
			{
			}

			protected override string DocketType
			{
				get { return "TestDocket"; }
			}

			public override DataContextType DataContextType
			{
				get { return DataContextType.WarehouseAdjustment; }
			}

			protected override IMatchingBusinessEntityFinder<TDocket> GetCombinedReferenceMatcher()
			{
				return null;
			}

			#endregion

			protected override bool ShouldDeleteUnmatchedDocketLines(TDocket docket, IEnumerable<OrderLine> lines)
			{
				return false;
			}

			protected override DataObjectReader<OrderLine, TDocketLine> GetNewLineReader(TDocket docket, OrderLine orderLineDataObject, IEnumerable<TDocketLine> matchedLines)
			{
				return null; // will prob blow up.
			}

			public void SetImportStrategyForTest(WhsImportStrategy importStrategy)
			{
				whsImportStrategy = importStrategy;
			}

			WhsImportStrategy whsImportStrategy;
			protected override WhsImportStrategy GetNewImportStrategy()
			{
				return whsImportStrategy ?? base.GetNewImportStrategy();
			}

			public bool PopulateBizOWasCalled { get; private set; }
			protected override void PopulateBizO(TDocket docket)
			{
				PopulateBizOWasCalled = true;
				base.PopulateBizO(docket);
			}
		}

		class WhsImportStrategyForTest : WhsDocketDataObjectReaderForTest.WhsImportStrategy
		{
			public WhsImportStrategyForTest(WhsDocketDataObjectReaderForTest reader) : base(reader)
			{
			}

			public bool ShouldPopulateBizOCoreReturnValueForTest { get; set; }
			protected override bool ShouldPopulateBizOCore(UniversalShipment dataObject, TDocket docket)
			{
				return ShouldPopulateBizOCoreReturnValueForTest;
			}
		}

		class PopulateBusinessObjectEventArgs : EventArgs
		{
			public PopulateBusinessObjectEventArgs(TDocket docket)
			{
				Docket = docket;
			}

			public readonly TDocket Docket;
		}

		#endregion

		#region Test ShouldPopulateBizO on Import Strategy

		public void TestShouldNotPopulateBizO_IfStrategySaysItShouldNot()
		{
			Data.CreateClientOrgCRAHOLSYDInDB();
			Data.GetOrCreateWarehouseInDB();

			var reader = new WhsDocketDataObjectReaderForTest(ShipmentDataObject, Logger, Factory);
			var importStrategy = new WhsImportStrategyForTest(reader);
			importStrategy.ShouldPopulateBizOCoreReturnValueForTest = false;
			reader.SetImportStrategyForTest(importStrategy);

			reader.ReadIntoBusinessObject();

			AssertEquals("When import strategy dictates not to populate bizO -> populateBizO method should not be called.", false, reader.PopulateBizOWasCalled);
		}

		public void TestShouldPopulateBizO_IfStrategySaysItShould()
		{
			Data.CreateClientOrgCRAHOLSYDInDB();
			Data.GetOrCreateWarehouseInDB();

			var reader = new WhsDocketDataObjectReaderForTest(ShipmentDataObject, Logger, Factory);
			var importStrategy = new WhsImportStrategyForTest(reader);
			importStrategy.ShouldPopulateBizOCoreReturnValueForTest = true;
			reader.SetImportStrategyForTest(importStrategy);

			reader.ReadIntoBusinessObject();

			AssertEquals("When import strategy dictates to populate bizO -> populateBizO method should be called.", true, reader.PopulateBizOWasCalled);
		}

		#endregion

		#endregion

		#region TestWhsImportStrategy_LoadDocketFromCustomsLinesDocketNumbers

		public void TestWhsImportStrategy_LoadDocketFromCustomsLinesDocketNumbers()
		{
			TestWhsImportStrategy_LoadDocketFromCustomsLinesDocketNumbersCore();
		}

		protected virtual void TestWhsImportStrategy_LoadDocketFromCustomsLinesDocketNumbersCore()
		{
			var reader = new WhsDocketDataObjectReaderForTest(ShipmentDataObject, Logger, Factory);
			var importStrategy = new WhsImportStrategyForTest(reader);

			AssertNull("LoadDocketFromCustomsLinesDocketNumbers", importStrategy.LoadDocketFromCustomsLinesDocketNumbers);
		}

		#endregion

		#region TestLogRowErrors

		public void TestLogRowErrors()
		{
			Data.CreateClientOrgCRAHOLSYDInDB();
			Data.GetOrCreateWarehouseInDB();

			var reader = new WhsDocketDataObjectReaderForTest(ShipmentDataObject, Logger, Factory);
			reader.AddRowErrorAction = bo => bo.AddRowError("Test Row Error");
			reader.ReadIntoBusinessObject();

			AssertEquals("Should log Row Error", true, Logger.HasErrors);
			AssertEquals("Should log Row Error", "Test Row Error", Logger.GetErrors());
		}

		#endregion

		#region IDocketTypeMembers

		public void TestIDocketType()
		{
			AssertEquals(GetDocketType(), GetNewReader(Data.ShipmentDataObject, Logger).DocketType);
		}

		#endregion

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();

			CanUserEditOrganisationCodeOriginalValue = (bool)Env.RawDataRegistry.Instance.CanUserEditOrganisationCode.Value;
			Env.RawDataRegistry.Instance.CanUserEditOrganisationCode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
		}

		protected override void TearDown()
		{
			base.TearDown();
			Env.RawDataRegistry.Instance.CanUserEditOrganisationCode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, CanUserEditOrganisationCodeOriginalValue);
		}

		protected UniversalShipment ShipmentDataObject
		{
			get { return Data.ShipmentDataObject; }
		}

		protected abstract TReader GetNewReader(UniversalShipment shipmentDataObject, IXmlImportLogger logger, bool useCleanFactory = false);
		protected abstract string GetDocketType();
		protected abstract TDocket GetNewDocket(OrgHeader client, WhsWarehouse warehouse, string externalReference = "");
		protected abstract DataContextType DataContext { get; }

		static Note SetupNote()
		{
			var noteDataObject = new Note();

			noteDataObject.Description = "DOG FLOGGER!!";
			noteDataObject.Visibility = new CodeDescriptionPair() { Code = nameof(StmNoteVisibility.PUB), Description = "Public" };
			noteDataObject.NoteContext = new NoteContext() { Code = "BEB", Description = "Baby Eats Banana" };

			return noteDataObject;
		}

		static void AssertNoteContents(StmNote noteBO)
		{
			AssertEquals("noteBO.ST_Description", "DOG FLOGGER!!", noteBO.ST_Description);
			AssertEquals("noteBO.ST_NoteContext", "BEB", noteBO.ST_NoteContext);
			AssertEquals("noteBO.ST_NoteType", "PUB", noteBO.ST_NoteType);
		}

		protected override TestDataForUniversal GetNewTestData()
		{
			return new TestDataForUniversal(Factory, Logger, DataContext);
		}

		bool CanUserEditOrganisationCodeOriginalValue;

		#endregion
	}
}
