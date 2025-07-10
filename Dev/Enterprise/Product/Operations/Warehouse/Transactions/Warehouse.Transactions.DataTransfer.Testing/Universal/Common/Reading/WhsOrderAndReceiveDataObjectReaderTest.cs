using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CargoWise.Application;
using CargoWise.Application.Testing;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.MasterFiles.DataTransfer.Universal.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.UniversalDataBuss.DataObjects.Writing;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.UniversalDataBuss.Management.Testing;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Environment.CodeLists;
using Enterprise.Warehouse.Integration.CodeLists;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Warehouse.Transactions.DataTransfer.Universal.Testing.WhsBondedChangeOfInventoryDataObjectReaderTest;
using Currency = Enterprise.UniversalDataBuss.DataObjects.Universal.Currency;
using UniversalCodeDescriptionPair = Enterprise.UniversalDataBuss.DataObjects.Universal.CodeDescriptionPair;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Warehouse.Transactions.DataTransfer.Universal.Testing
{
	[GuiTest]
	abstract class WhsOrderAndReceiveDataObjectReaderTest<TDocket, TDocketLine, TReader> : WhsDocketDataObjectReaderTest<TDocket, TDocketLine, TReader>
		where TDocket : WhsDocket, IJobWithTransportCompany
		where TDocketLine : WhsDocketLine
		where TReader : WhsOrderAndReceiveDataObjectReader<TDocket, TDocketLine>
	{
		#region TestLoadingDocket

		#region TestWhsDocketsOfDifferentTypesAreNotPickedUpByContextMatchingForTheTypeOfDocketWeWantToMatch

		public void TestWhsDocketsOfDifferentTypesAreNotPickedUpByContextMatchingForTheTypeOfDocketWeWantToMatch()
		{
			Data.CreateClientOrgCRAHOLSYDInDB();
			Data.GetOrCreateWarehouseInDB();

			var whsDocketOfDifferentType = GetNewDocketOfDifferentType();
			AssertNotEquals("Docket to test against should be of a different type", typeof(TDocket), whsDocketOfDifferentType.GetType());

			whsDocketOfDifferentType.WD_OH_Client = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, "CRAHOLSYD")).PK;
			whsDocketOfDifferentType.WD_CustomerReference = "CUSTOMER";

			Factory.SaveForTesting();

			ShipmentDataObject.Order.ClientReference = "CUSTOMER";

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

		#endregion

		#region TestLoadsLatestDocketWhenHasEqualMatches

		public void TestLoadsLatestDocketWhenHasEqualMatches()
		{
			var client = Data.CreateClientOrgCRAHOLSYDInDB();
			var warehouse = Data.GetOrCreateWarehouseInDB();

			var dummyDocket = GetNewDocket(client, warehouse, "1");
			dummyDocket.WD_CustomerReference = "CUSTOMER";
			dummyDocket.WD_TransportReference = "TRANS";
			dummyDocket.WD_TotalUnits = 10m;
			dummyDocket.WD_TotalOrderValue = 11m;
			dummyDocket.WD_RX_NKTotalOrderCurrency = "AUD";

			Factory.SaveForTesting();

			var docketBOToLoad = GetNewDocket(client, warehouse, "2");
			docketBOToLoad.WD_CustomerReference = "CUSTOMER";
			docketBOToLoad.WD_TransportReference = "TRANS";
			docketBOToLoad.WD_TotalUnits = 10m;
			docketBOToLoad.WD_TotalOrderValue = 11m;
			docketBOToLoad.WD_RX_NKTotalOrderCurrency = "AUD";
			docketBOToLoad.WD_SystemCreateTimeUtc = dummyDocket.WD_SystemCreateTimeUtc.AddDays(1);

			Factory.SaveForTesting();

			ShipmentDataObject.Order.ClientReference = "CUSTOMER";
			ShipmentDataObject.Order.TransportReference = "TRANS";
			ShipmentDataObject.Order.TotalUnits = 11m;
			ShipmentDataObject.GoodsValue = 12m;
			ShipmentDataObject.GoodsValueCurrency = new Currency() { Code = "USD" };

			var reader = GetNewReader(ShipmentDataObject, Logger);
			var whsDocketBO = reader.ReadIntoBusinessObject();

			AssertNotNull(whsDocketBO);

			CombineAssertions(delegate
			{
				AssertEquals("whsDocketBO.WD_TotalOrderValue", 12m, whsDocketBO.WD_TotalOrderValue);
				AssertEquals("whsDocketBO.TotalOrderCurr", "USD", whsDocketBO.WD_RX_NKTotalOrderCurrency);
				AssertEquals("whsDocketBO.WD_TotalUnits", 11m, whsDocketBO.WD_TotalUnits);
				AssertEquals("loaded same docket", docketBOToLoad.PK, whsDocketBO.PK);
				AssertMultilineASCIIEquals("Logger.Logs", string.Format(@"
Information - Matching 'ConsignorDocumentaryAddress':- Matched to 'CRAHOLSYD' by code, address '' (only address).
Information - Successfully loaded matching {0}.
Information - Populating {0}...
Information - Matching 'ConsignorDocumentaryAddress':- Matched to 'CRAHOLSYD' by code, address '' (only address).
Information - Updated Warehouse {1} {2} from UniversalShipment.
".Trim(), typeof(TDocket).Name, GetDocketType(), docketBOToLoad.WD_DocketID), Logger.Logs);
			});
		}

		#endregion

		#region TestLoadingDocketWithMostMatches

		public void TestLoadingDocketWithMostMatches()
		{
			var client = Data.CreateClientOrgCRAHOLSYDInDB();
			var warehouse = Data.GetOrCreateWarehouseInDB();

			var docketBOToLoad = GetNewDocket(client, warehouse);
			docketBOToLoad.WD_ExternalReference = "ORDERME1";
			docketBOToLoad.WD_CustomerReference = "CUSTOMER";
			docketBOToLoad.WD_TransportReference = "TRANS";
			docketBOToLoad.WD_TotalUnits = 10m;

			Factory.SaveForTesting();

			var dummyDocket = GetNewDocket(client, warehouse);
			dummyDocket.WD_ExternalReference = "ORDERME2";
			dummyDocket.WD_CustomerReference = "CUSTOMER";
			dummyDocket.WD_TotalUnits = 10m;
			dummyDocket.WD_SystemCreateTimeUtc = docketBOToLoad.WD_SystemCreateTimeUtc.AddDays(1);

			Factory.SaveForTesting();

			ShipmentDataObject.Order.ClientReference = "CUSTOMER";
			ShipmentDataObject.Order.TransportReference = "TRANS";
			ShipmentDataObject.Order.TotalUnits = 11m;

			var reader = GetNewReader(ShipmentDataObject, Logger);
			var whsDocketBO = reader.ReadIntoBusinessObject();

			AssertNotNull(whsDocketBO);

			CombineAssertions(delegate
			{
				AssertEquals("whsDocketBO.WD_TotalUnits", 11m, whsDocketBO.WD_TotalUnits);
				AssertEquals("loaded same docket", docketBOToLoad.PK, whsDocketBO.PK);
				AssertMultilineASCIIEquals("Logger.Logs", string.Format(@"
Information - Matching 'ConsignorDocumentaryAddress':- Matched to 'CRAHOLSYD' by code, address '' (only address).
Information - Successfully loaded matching {0}.
Information - Populating {0}...
Information - Matching 'ConsignorDocumentaryAddress':- Matched to 'CRAHOLSYD' by code, address '' (only address).
Information - Updated Warehouse {1} {2} from UniversalShipment.
".Trim(), typeof(TDocket).Name, GetDocketType(), docketBOToLoad.WD_DocketID), Logger.Logs);
			});
		}

		#endregion

		#region TestMatchExistingDocketThroughOrderNumber

		#region TestDoesNotMatchExistingDocketWithOrderNumberOnly

		public void TestDoesNotMatchExistingDocketWithOrderNumberOnly()
		{
			Data.CreateClientOrgCRAHOLSYDInDB();
			var warehouse = Data.GetOrCreateWarehouseInDB();

			var nonMatchingDocketBO = GetNewDocket(new WhsTestHelperFunctions(Factory.BOFactory).CreateClient(), warehouse);
			nonMatchingDocketBO.WD_DocketID = "W00000002";
			nonMatchingDocketBO.WD_ExternalReference = "ORDERME";
			nonMatchingDocketBO.WD_ExternalReferenceSplit = new ZByte(2);
			nonMatchingDocketBO.WD_TotalUnits = 10m;

			Factory.SaveForTesting();

			ShipmentDataObject.Order.OrderNumber = "ORDERME";
			ShipmentDataObject.Order.OrderNumberSplit = new ZByte(2);
			ShipmentDataObject.Order.TotalUnits = 11m;

			var reader = GetNewReader(ShipmentDataObject, Logger);
			var whsDocketBO = reader.ReadIntoBusinessObject();

			AssertNotNull(whsDocketBO);

			CombineAssertions(delegate
			{
				AssertEquals("whsDocketBO.WD_TotalUnits", 11m, whsDocketBO.WD_TotalUnits);
				AssertNotEquals("Does not load docket", nonMatchingDocketBO.PK, whsDocketBO.PK);
				AssertMultilineASCIIEquals("Logger.Logs", string.Format(@"
Information - Matching 'ConsignorDocumentaryAddress':- Matched to 'CRAHOLSYD' by code, address '' (only address).
Information - No matching {0} found, creating new {0}.
Information - Populating {0}...
Information - Matching 'ConsignorDocumentaryAddress':- Matched to 'CRAHOLSYD' by code, address '' (only address).
Information - Added Warehouse {1} from UniversalShipment.
".Trim(), typeof(TDocket).Name, GetDocketType()), Logger.Logs);
			});
		}

		#endregion

		#region TestMatchingExistingDocketThroughClientAddressPlusOrderNumberPlusZeroSplitNumber

		public void TestMatchingExistingDocketThroughClientAddressPlusOrderNumberPlusZeroSplitNumber()
		{
			var client = Data.CreateClientOrgCRAHOLSYDInDB();
			var warehouse = Data.GetOrCreateWarehouseInDB();

			var docketBOToLoad = GetNewDocket(client, warehouse);
			docketBOToLoad.WD_ExternalReference = "ORDERME";
			docketBOToLoad.WD_TotalUnits = 10m;

			Factory.SaveForTesting();

			var dummyDocket = GetNewDocket(new WhsTestHelperFunctions(Factory.BOFactory).CreateClient(), warehouse);
			dummyDocket.WD_ExternalReference = "ORDERME";
			dummyDocket.WD_CustomerReference = "CUSTOMER";
			dummyDocket.WD_SystemCreateTimeUtc = docketBOToLoad.WD_SystemCreateTimeUtc.AddDays(1);

			Factory.SaveForTesting();

			ShipmentDataObject.Order.OrderNumber = "ORDERME";
			ShipmentDataObject.Order.ClientReference = "CUSTOMER";
			ShipmentDataObject.Order.TotalUnits = 11m;

			var reader = GetNewReader(ShipmentDataObject, Logger);
			var whsDocketBO = reader.ReadIntoBusinessObject();

			AssertNotNull(whsDocketBO);

			CombineAssertions(delegate
			{
				AssertEquals("whsDocketBO.WD_TotalUnits", 11m, whsDocketBO.WD_TotalUnits);
				AssertEquals("loaded same docket", docketBOToLoad.PK, whsDocketBO.PK);
				AssertMultilineASCIIEquals("Logger.Logs", string.Format(@"
Information - Matching 'ConsignorDocumentaryAddress':- Matched to 'CRAHOLSYD' by code, address '' (only address).
Information - Successfully loaded matching {0}.
Information - Populating {0}...
Information - Matching 'ConsignorDocumentaryAddress':- Matched to 'CRAHOLSYD' by code, address '' (only address).
Information - Updated Warehouse {1} {2} from UniversalShipment.
".Trim(), typeof(TDocket).Name, GetDocketType(), docketBOToLoad.WD_DocketID), Logger.Logs);
			});
		}

		#endregion

		#region TestMatchingExistingDocketThroughClientAddressPlusOrderNumberPlusSplitNumber

		public void TestMatchingExistingDocketThroughClientAddressPlusOrderNumberPlusSplitNumber()
		{
			var client = Data.CreateClientOrgCRAHOLSYDInDB();
			var warehouse = Data.GetOrCreateWarehouseInDB();

			var docketBOToLoad = GetNewDocket(client, warehouse);
			docketBOToLoad.WD_ExternalReference = "ORDERME";
			docketBOToLoad.WD_ExternalReferenceSplit = new ZByte(2);
			docketBOToLoad.WD_TotalUnits = 10m;

			Factory.SaveForTesting();

			var dummyDocket = GetNewDocket(new WhsTestHelperFunctions(Factory.BOFactory).CreateClient(), warehouse);
			dummyDocket.WD_ExternalReference = "ORDERME";
			dummyDocket.WD_ExternalReferenceSplit = new ZByte(2);
			dummyDocket.WD_CustomerReference = "CUSTOMER";
			dummyDocket.WD_SystemCreateTimeUtc = docketBOToLoad.WD_SystemCreateTimeUtc.AddDays(1);

			Factory.SaveForTesting();

			ShipmentDataObject.Order.OrderNumber = "ORDERME";
			ShipmentDataObject.Order.OrderNumberSplit = new ZByte(2);
			ShipmentDataObject.Order.ClientReference = "CUSTOMER";
			ShipmentDataObject.Order.TotalUnits = 11m;

			var reader = GetNewReader(ShipmentDataObject, Logger);
			var whsDocketBO = reader.ReadIntoBusinessObject();

			AssertNotNull(whsDocketBO);

			CombineAssertions(delegate
			{
				AssertEquals("whsDocketBO.WD_TotalUnits", 11m, whsDocketBO.WD_TotalUnits);
				AssertEquals("loaded same docket", docketBOToLoad.PK, whsDocketBO.PK);
				AssertMultilineASCIIEquals("Logger.Logs", string.Format(@"
Information - Matching 'ConsignorDocumentaryAddress':- Matched to 'CRAHOLSYD' by code, address '' (only address).
Information - Successfully loaded matching {0}.
Information - Populating {0}...
Information - Matching 'ConsignorDocumentaryAddress':- Matched to 'CRAHOLSYD' by code, address '' (only address).
Information - Updated Warehouse {1} {2} from UniversalShipment.
".Trim(), typeof(TDocket).Name, GetDocketType(), docketBOToLoad.WD_DocketID), Logger.Logs);
			});
		}

		#endregion

		#endregion

		#region TestMatchExistingDocketThroughCustomerReference

		#region TestOnlyMatchesDocketThroughCustomerReferenceIfOrderNumberIsNotProvided

		public void TestOnlyMatchesDocketThroughCustomerReferenceIfOrderNumberIsNotProvided()
		{
			var client = Data.CreateClientOrgCRAHOLSYDInDB();
			var warehouse = Data.GetOrCreateWarehouseInDB();
			var nonMatchingDocket = GetNewDocket(client, warehouse, "123");
			nonMatchingDocket.WD_CustomerReference = "CUSTOMER";
			Factory.SaveForTesting();

			ShipmentDataObject.Order.ClientReference = "CUSTOMER";
			ShipmentDataObject.Order.OrderNumber = "456";

			var reader = GetNewReader(ShipmentDataObject, Logger);
			var docket = reader.ReadIntoBusinessObject();

			AssertNotNull(docket);
			AssertNotEquals("Should not update existing docket", nonMatchingDocket.PK, docket.PK);
			AssertEquals("456", docket.WD_ExternalReference);
			AssertEquals("CUSTOMER", docket.WD_CustomerReference);
			AssertEquals(false, Logger.HasErrors);
			AssertEquals(false, Logger.HasWarnings);
		}

		#endregion

		#region TestDoesNotMatchExistingDocketWithOnlyCustomerReference

		public void TestDoesNotMatchExistingDocketWithOnlyCustomerReference()
		{
			Data.CreateClientOrgCRAHOLSYDInDB();
			var warehouse = Data.GetOrCreateWarehouseInDB();

			var nonMatchingDocketBO = GetNewDocket(Helper.CreateClient(), warehouse);
			nonMatchingDocketBO.WD_DocketID = "W00000002";
			nonMatchingDocketBO.WD_CustomerReference = "CUSTOMER";
			nonMatchingDocketBO.WD_TotalUnits = 10m;

			Factory.SaveForTesting();

			ShipmentDataObject.Order.ClientReference = "CUSTOMER";
			ShipmentDataObject.Order.TotalUnits = 11m;

			var reader = GetNewReader(ShipmentDataObject, Logger);
			var whsDocketBO = reader.ReadIntoBusinessObject();

			AssertNotNull(whsDocketBO);

			CombineAssertions(delegate
			{
				AssertEquals("whsDocketBO.WD_TotalUnits", 11m, whsDocketBO.WD_TotalUnits);
				AssertNotEquals("Does not load docket", nonMatchingDocketBO.PK, whsDocketBO.PK);
				AssertMultilineASCIIEquals("Logger.Logs", string.Format(@"
Information - Matching 'ConsignorDocumentaryAddress':- Matched to 'CRAHOLSYD' by code, address '' (only address).
Information - No matching {0} found, creating new {0}.
Information - Populating {0}...
Information - Matching 'ConsignorDocumentaryAddress':- Matched to 'CRAHOLSYD' by code, address '' (only address).
Information - Added Warehouse {1} from UniversalShipment.
				".Trim(), typeof(TDocket).Name, GetDocketType()), Logger.Logs);
			});
		}

		#endregion

		#region TestMatchingExistingDocketThroughClientAddressPlusCustomerReference

		public void TestMatchingExistingDocketThroughClientAddressPlusCustomerReference()
		{
			var client = Data.CreateClientOrgCRAHOLSYDInDB();
			var warehouse = Data.GetOrCreateWarehouseInDB();

			var docketBOToLoad = GetNewDocket(client, warehouse);
			docketBOToLoad.WD_CustomerReference = "CUSTOMER";
			docketBOToLoad.WD_TotalUnits = 10m;

			Factory.SaveForTesting();

			ShipmentDataObject.Order.ClientReference = "CUSTOMER";
			ShipmentDataObject.Order.TotalUnits = 11m;

			var reader = GetNewReader(ShipmentDataObject, Logger);
			var whsDocketBO = reader.ReadIntoBusinessObject();

			AssertNotNull(whsDocketBO);

			CombineAssertions(delegate
			{
				AssertEquals("whsDocketBO.WD_TotalUnits", 11m, whsDocketBO.WD_TotalUnits);
				AssertEquals("loaded same docket", docketBOToLoad.PK, whsDocketBO.PK);
				AssertMultilineASCIIEquals("Logger.Logs", string.Format(@"
Information - Matching 'ConsignorDocumentaryAddress':- Matched to 'CRAHOLSYD' by code, address '' (only address).
Information - Successfully loaded matching {0}.
Information - Populating {0}...
Information - Matching 'ConsignorDocumentaryAddress':- Matched to 'CRAHOLSYD' by code, address '' (only address).
Information - Updated Warehouse {1} {2} from UniversalShipment.
".Trim(), typeof(TDocket).Name, GetDocketType(), docketBOToLoad.WD_DocketID), Logger.Logs);
			});
		}

		#endregion

		#region TestMatchingExternalReferenceConsiderSplitNo

		public void TestMatchingExternalReferenceConsiderSplitNo()
		{
			var client = Data.CreateClientOrgCRAHOLSYDInDB();
			var warehouse = Data.GetOrCreateWarehouseInDB();

			ShipmentDataObject.Order.OrderNumber = "ExternalRef";
			ShipmentDataObject.Order.OrderNumberSplit = 0;
			ShipmentDataObject.Order.TotalUnits = 5m;

			// create
			var reader = GetNewReader(ShipmentDataObject, Logger);
			var whsDocket0 = reader.ReadIntoBusinessObject();
			AssertNotNull(whsDocket0);
			CombineAssertions(delegate
			{
				AssertEquals("whsDocket0.WD_ExternalReferenceSplit", (byte)0, whsDocket0.WD_ExternalReferenceSplit);
				AssertEquals("whsDocket0.WD_TotalUnits", 5m, whsDocket0.WD_TotalUnits);
			});

			// update
			ShipmentDataObject.Order.TotalUnits = 15m;
			reader = GetNewReader(ShipmentDataObject, Logger);
			var whsDocket0Updated = reader.ReadIntoBusinessObject();
			AssertNotNull(whsDocket0Updated);
			CombineAssertions(delegate
			{
				AssertEquals("whsDocket0_updated.WD_ExternalReferenceSplit", (byte)0, whsDocket0Updated.WD_ExternalReferenceSplit);
				AssertEquals("whsDocket0_updated.WD_TotalUnits", 15m, whsDocket0Updated.WD_TotalUnits);
				AssertEquals("Should find whsDocket0 and updated.", whsDocket0.PK, whsDocket0Updated.PK);
			});

			// create new with split 1
			ShipmentDataObject.Order.OrderNumberSplit = 1;
			reader = GetNewReader(ShipmentDataObject, Logger);
			var whsDocket1 = reader.ReadIntoBusinessObject();
			AssertNotNull(whsDocket1);
			CombineAssertions(delegate
			{
				AssertEquals("whsDocket1.WD_ExternalReferenceSplit", (byte)1, whsDocket1.WD_ExternalReferenceSplit);
				AssertEquals("whsDocket1.WD_TotalUnits", 15m, whsDocket1.WD_TotalUnits);
				AssertNotEquals("Should not find existing docket with split no 0 and create a new.", whsDocket0.PK, whsDocket1.PK);
			});
		}

		#endregion

		#endregion

		#endregion

		#region TestImportingOfChargeLines

		[TestDate(2011, 12, 12)]
		public void TestImportingOfChargeLines()
		{
			using (Factory.BOFactory.AddDisposableService())
			{
				Data.CreateClientOrgCRAHOLSYDInDB();
				Data.GetOrCreateWarehouseInDB();

				ShipmentDataObject.DataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
				ShipmentDataObject.DataContext.CodesMappedToTarget = true; // Reguired to import JobCosting
				ShipmentDataObject.JobCosting = new JobCosting(DefaultDataObjectWriterStrategy.TestInstance);
				ShipmentDataObject.JobCosting.Branch = new Branch();
				ShipmentDataObject.JobCosting.Branch.Code = "SYD";
				ShipmentDataObject.JobCosting.SetChargeLineCollection(() => new List<ChargeLine>());
				var chargeLine1 = GetChargeLine("SYD", "FRT", "001", new ZDateTime(2011, 10, 04), null, new ZDateTime(2011, 10, 04), 100.00m, 100.00m, "AUD", null,
					"AALSHI", "ABIGAS", "FIS", "Charge Description 1", 1, null, "FIN", 100.00m, 100.00m, "AUD", null);
				ShipmentDataObject.JobCosting.ChargeLineCollection.Add(chargeLine1);
				var chargeLine2 = GetChargeLine("SYD", "BAF", "001", new ZDateTime(2011, 10, 04), null, new ZDateTime(2011, 10, 04), 200.00m, 200.00m, "AUD", null,
					"AALSHI", "ABIGAS", "FIS", "Charge Description 1", 1, null, "FIN", 250.00m, 250.00m, "AUD", null);
				ShipmentDataObject.JobCosting.ChargeLineCollection.Add(chargeLine2);
				chargeLine1.ImportMetaData = new ImportMetaData(DefaultDataObjectWriterStrategy.TestInstance);
				chargeLine1.ImportMetaData.Instruction = InstructionType.Insert;
				chargeLine2.ImportMetaData = new ImportMetaData(DefaultDataObjectWriterStrategy.TestInstance);
				chargeLine2.ImportMetaData.Instruction = InstructionType.Insert;

				var jobs = Factory.Load<JobHeader>(new ZQuery(JobHeaderSchema.JH_JobNum, "W00000001").AddToFilter(JobHeaderSchema.JH_GC, GlbCompany.CurrentCompany.PK));
				AssertEquals("jobs.Length", 0, jobs.Length);

				var charges = Factory.Load<JobCharge>(new ZQuery());
				AssertEquals("charges.Length", 0, charges.Length);

				var reader = GetNewReader(ShipmentDataObject, Logger);
				var whsDocketBO = reader.ReadIntoBusinessObject();
				whsDocketBO.WD_DocketID = "W00000001";
				Factory.SaveAtEndOfImport(Logger);

				AssertNotNull(whsDocketBO);

				CombineAssertions(delegate
				{
					AssertMultilineASCIIEquals("Logger.Logs", string.Format(@"
Information - Matching 'ConsignorDocumentaryAddress':- Matched to 'CRAHOLSYD' by code, address '' (only address).
Information - No matching {0} found, creating new {0}.
Information - Populating {0}...
Information - Matching 'ConsignorDocumentaryAddress':- Matched to 'CRAHOLSYD' by code, address '' (only address).
Warning - Whilst importing Charge Line: Job Number= Charge Code=FRT Creditor=AALSHI, Debtor=ABIGAS Cost OS Amount=100.00 Sell OS Amount=100.00
Warning - Override Comment: You have not entered an Override Comment.
Warning - Description: Charge description was changed from default. This description will appear on AR Invoice without translation.
Warning - Debtor: You have selected a Debtor that is neither your Local Client or your Overseas Agent.
Whilst invoicing any party is valid, this should be confirmed.

Warning - Whilst importing Charge Line: Job Number= Charge Code=BAF Creditor=AALSHI, Debtor=ABIGAS Cost OS Amount=200.00 Sell OS Amount=250.00
Warning - Override Comment: You have not entered an Override Comment.
Warning - Description: Charge description was changed from default. This description will appear on AR Invoice without translation.
Warning - Debtor: You have selected a Debtor that is neither your Local Client or your Overseas Agent.
Whilst invoicing any party is valid, this should be confirmed.
Warning - Revenue Override Comment: You have not entered a Revenue Override Comment.

Information - Added Warehouse {1} from UniversalShipment.
Information - Successfully saved Warehouse {1} W00000001.
".Trim(), typeof(TDocket).Name, GetDocketType()), Logger.Logs);
				});

				jobs = Factory.Load<JobHeader>(new ZQuery(JobHeaderSchema.JH_JobNum, "W00000001").AddToFilter(JobHeaderSchema.JH_GC, GlbCompany.CurrentCompany.PK));
				AssertEquals("jobs.Length", 1, jobs.Length);

				charges = Factory.Load<JobCharge>(new ZQuery(JobChargeSchema.JR_JH, jobs[0].PK));
				AssertEquals("charges.Length", 2, charges.Length);

				JobCharge fRTCharge;
				JobCharge bAFCharge;

				if (charges[0].ChargeCode.AC_Code == "FRT")
				{
					fRTCharge = charges[0];
					bAFCharge = charges[1];
				}
				else
				{
					fRTCharge = charges[1];
					bAFCharge = charges[0];
				}

				AssertCharge(fRTCharge, chargeLine1);
				AssertCharge(bAFCharge, chargeLine2);
			}
		}

		#endregion

		#region TestImport_CannotCancelWarehouseBondedJob

		public void TestImport_CannotCancelWarehouseBondedJob()
		{
			var client = Data.CreateClientOrgCRAHOLSYDInDB();
			var whs = Data.GetOrCreateWarehouseInDB();

			Helper.CreateArea(whs, "BOND", AreaTypes.Codes.Bonded);
			var docketBOToLoad = GetNewDocket(client, whs, "2");
			docketBOToLoad.WD_ExternalReference = "ORDER123";
			docketBOToLoad.WD_DocketStatus = DocketStatus.Codes.Entered;
			docketBOToLoad.WD_DocketSubType = OrderType.Codes.Customs;
			docketBOToLoad.WD_DocketID = "W00000002";
			docketBOToLoad.WD_OH_Client = client.PK;
			docketBOToLoad.WD_TransportReference = "TASD6563";
			Factory.SaveForTesting();

			AssertEquals("Precondition- docket is bonded", true, docketBOToLoad.IsCustomsTransaction);
			ShipmentDataObject.Order.Status = new UniversalCodeDescriptionPair { Code = "CAN", Description = "Cancelled" };
			ShipmentDataObject.Order.OrderNumber = "ORDER123";
			ShipmentDataObject.Order.TransportReference = "ABCD0987";

			var reader = GetNewReader(ShipmentDataObject, Logger);
			var whsDocketBO = reader.ReadIntoBusinessObject();

			AssertNotNull(whsDocketBO);

			CombineAssertions(delegate
			{
				AssertEquals("whsOrderBO.WD_DocketStatus", DocketStatus.Codes.Entered, whsDocketBO.WD_DocketStatus);
				AssertEquals("whsOrderBO.WD_TransportReference", "ABCD0987", whsDocketBO.WD_TransportReference);
				AssertMultilineASCIIEquals("Logger.Logs", string.Format(@"
Information - Matching 'ConsignorDocumentaryAddress':- Matched to 'CRAHOLSYD' by code, address '' (only address).
Information - Successfully loaded matching {0}.
Information - Populating {0}...
Information - Matching 'ConsignorDocumentaryAddress':- Matched to 'CRAHOLSYD' by code, address '' (only address).
Information - Updated Warehouse {1} {2} from UniversalShipment.
".Trim(), typeof(TDocket).Name, GetDocketType(), docketBOToLoad.WD_DocketID), Logger.Logs);
			});
		}

		#endregion

		#region TestImport_CanCancelWarehouseNonBondedJob

		#region TestImport_CanCancelWarehouseNonBondedJob_EnteredStatus

		public void TestImport_CanCancelWarehouseNonBondedJob_EnteredStatus()
		{
			AssertImport_CanCancelWarehouseNonBondedJob(DocketStatus.Codes.Entered);
		}

		#endregion

		#region TestImport_CanCancelWarehouseNonBondedJob_ErrorStatus

		public void TestImport_CanCancelWarehouseNonBondedJob_ErrorStatus()
		{
			AssertImport_CanCancelWarehouseNonBondedJob(DocketStatus.Codes.Error);
		}

		#endregion

		#region TestImport_CanCancelWarehouseNonBondedJob_HeldStatus

		public void TestImport_CanCancelWarehouseNonBondedJob_HeldStatus()
		{
			AssertImport_CanCancelWarehouseNonBondedJob(DocketStatus.Codes.Held);
		}

		#endregion

		void AssertImport_CanCancelWarehouseNonBondedJob(ZString statusCode)
		{
			var docketBOToLoad = CreateDocketWithStatusCode(statusCode);
			Factory.SaveForTesting();

			ShipmentDataObject.Order.Status = new UniversalCodeDescriptionPair { Code = "CAN", Description = "Cancelled" };
			ShipmentDataObject.Order.OrderNumber = "ORDER123";
			ShipmentDataObject.Order.TransportReference = "ERTG7892";

			var reader = GetNewReader(ShipmentDataObject, Logger);
			var whsDocketBO = reader.ReadIntoBusinessObject();

			CombineAssertions(delegate
			{
				AssertEquals("Docket job is cancelled.", true, docketBOToLoad.IsCancelled);
				AssertEquals(true, docketBOToLoad.IsCancelled);
				AssertEquals("whsOrderBO.WD_DocketStatus", DocketStatus.Codes.Cancelled, whsDocketBO.WD_DocketStatus);
				AssertEquals("whsOrderBO.WD_TransportReference", "TRANS123", whsDocketBO.WD_TransportReference);
				AssertMultilineASCIIEquals("Logger.Logs", string.Format(@"
Information - Matching 'ConsignorDocumentaryAddress':- Matched to 'CRAHOLSYD' by code, address '' (only address).
Information - Successfully loaded matching {0}.
Information - Populating {0}...
Information - The warehouse {0} - {2} has been canceled and no other updates were made.
Information - Updated Warehouse {1} {2} from UniversalShipment.
".Trim(), typeof(TDocket).Name, GetDocketType(), docketBOToLoad.WD_DocketID), Logger.Logs);
			});
		}

		#endregion

		#region TestImport_CannotCancelWarehouseNonBondedJob

		#region TestImport_CannotCancelWarehouseNonBondedJob_CancelledStatus

		public void TestImport_CannotCancelWarehouseNonBondedJob_CancelledStatus()
		{
			var docketBOToLoad = CreateDocketWithStatusCode(DocketStatus.Codes.Cancelled);
			Factory.SaveForTesting();

			ShipmentDataObject.Order.Status = new UniversalCodeDescriptionPair { Code = "CAN", Description = "Cancelled" };
			ShipmentDataObject.Order.OrderNumber = "ORDER123";

			var reader = GetNewReader(ShipmentDataObject, Logger);
			var whsDocketBO = reader.ReadIntoBusinessObject();

			AssertNotNull(whsDocketBO);

			CombineAssertions(delegate
			{
				AssertEquals($@"Cannot populate {typeof(TDocket).Name} because:
Warehouse {GetDocketType()} {docketBOToLoad.WD_DocketID} could not be updated because it is Canceled.", Logger.GetErrors());
				AssertEquals(true, Logger.HasErrors);
				AssertEquals(false, Logger.HasWarnings);
			});
		}

		protected void AssertTestImport_CannotCancelWarehouseNonBondedJob_SetCancelStatusFail(WhsDocket docket, string expectedExceptionError)
		{
			ShipmentDataObject.Order.Status = new UniversalCodeDescriptionPair { Code = "CAN", Description = "Cancelled" };
			ShipmentDataObject.Order.OrderNumber = "ORDER123";
			ShipmentDataObject.Order.TransportReference = "TRANS456";

			var reader = GetNewReader(ShipmentDataObject, Logger);

			CombineAssertions(() =>
			{
				var ex = AssertExceptionThrown<DataObjectReadFailureException>(() => reader.ReadIntoBusinessObject());
				AssertEquals(expectedExceptionError, ex.Message);

				AssertNotContains("docket should not be update while docket fail to cancel", string.Format(@"Updated Warehouse {0} {1} from UniversalShipment", GetDocketType(), docket.WD_DocketID), Logger.Logs);
				AssertEquals("docket should not be update while docket fail to cancel", "TRANS123", docket.WD_TransportReference);
			});
		}

		#endregion

		#region TestImport_CannotCancelWarehouseNonBondedJob_DocketDoesNotExist

		public void TestImport_CannotCancelWarehouseNonBondedJob_DocketDoesNotExist()
		{
			Data.CreateClientOrgCRAHOLSYDInDB();
			Data.GetOrCreateWarehouseInDB();

			ShipmentDataObject.Order.Status = new UniversalCodeDescriptionPair { Code = "CAN", Description = "Canceled" };
			ShipmentDataObject.Order.OrderNumber = "ORDERME";

			var reader = GetNewReader(ShipmentDataObject, Logger);
			var docket = reader.ReadIntoBusinessObject();
			AssertContains("Cannot cancel the warehouse job as there is no existing job number 'ORDERME'.", Logger.GetErrors());
		}

		#endregion

		#region TestImport_CannotCancelWarehouseNonBondedJob_FinalisedStatus

		public void TestImport_CannotCancelWarehouseNonBondedJob_FinalisedStatus()
		{
			var docketBOToLoad = CreateDocketWithStatusCode(DocketStatus.Codes.Entered);
			var line = docketBOToLoad.Lines.AddNew();
			line.WE_OP = Data.Product.PK;
			line.WE_TransactionQuantity = 1;

			if (line.WE_DocketLineType == "WOR" || line.WE_DocketLineType == "ORD")
			{
				line.WE_WL = ZGuid.Empty;
			}
			else
			{
				line.WE_WL = docketBOToLoad.Warehouse.DefaultLocation.PK;
			}

			docketBOToLoad.Warehouse.WW_AutoPrintPackingSlip = false;
			FinaliseDocket(docketBOToLoad);
			Factory.SaveForTesting();

			AssertEquals("Precondition: docket is finalised", true, docketBOToLoad.IsFinalised);
			AssertImport_CannotCancelWarehouseNonBondedJob(docketBOToLoad, DocketStatus.Codes.Finalised);
		}

		#endregion

		#region TestImport_CannotCancelWarehouseNonBondedJob_PutawayStatus

		public void TestImport_CannotCancelWarehouseNonBondedJob_PutawayStatus()
		{
			var docketBOToLoad = CreateDocketWithStatusCode(DocketStatus.Codes.Putaway);
			Factory.SaveForTesting();

			AssertImport_CannotCancelWarehouseNonBondedJob(docketBOToLoad, DocketStatus.Codes.Putaway);
		}

		#endregion

		void AssertImport_CannotCancelWarehouseNonBondedJob(TDocket docketBOToLoad, ZString status)
		{
			ShipmentDataObject.Order.Status = new UniversalCodeDescriptionPair { Code = "CAN", Description = "Cancelled" };
			ShipmentDataObject.Order.OrderNumber = "ORDER123";
			ShipmentDataObject.Order.TransportReference = "ERTG7892";

			var reader = GetNewReader(ShipmentDataObject, Logger);
			var whsDocketBO = reader.ReadIntoBusinessObject();

			AssertNotNull(whsDocketBO);
			CombineAssertions(delegate
			{
				AssertEquals(false, docketBOToLoad.IsCancelled);
				if (status == DocketStatus.Codes.Finalised)
				{
					AssertEquals("whsOrderBO.IsFinalised", true, whsDocketBO.IsFinalised);
				}
				else
				{
					AssertEquals("whsOrderBO.WD_DocketStatus", status, whsDocketBO.WD_DocketStatus);
				}
				AssertEquals("whsOrderBO.WD_TransportReference", "TRANS123", whsDocketBO.WD_TransportReference);
				AssertMultilineASCIIEquals("Logger.Logs", string.Format(@"
Information - Matching 'ConsignorDocumentaryAddress':- Matched to 'CRAHOLSYD' by code, address '' (only address).
Information - Successfully loaded matching {0}.
Error - Cannot populate {0} because:
You can only cancel dockets with Entered (Saved) status
".Trim(), typeof(TDocket).Name), Logger.Logs);
			});
		}

		#endregion

		#region TestLinkDocketWithParent

		protected void AssertLinkDocketWithParent(BusinessObject parent, DataContextType dataContextType)
		{
			var dummy = Factory.New<DummyBusinessObject>();
			dummy.Z0_Description = "D1234"; // this is the job number field in universal.
			Factory.SaveForTesting();
			Data.CreateClientOrgCRAHOLSYDInDB();
			Data.GetOrCreateWarehouseInDB();

			ShipmentDataObject.OrganizationAddressCollection.Add(Data.Orgs.ConsigneeAddressDataObject_CRAHOLSYD);
			Logger.TopLevelDataContext.AddDataSource(DataContextType.DummyBusinessObject, "D1234");
			Logger.TopLevelDataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
			Logger.OutboundSessionTracker = new DataWritingManager(new DummyActionInfo()); // internal import

			var docket1 = GetNewReader(Data.ShipmentDataObject, Logger).ReadIntoBusinessObject();
			AssertEquals("Should not have created any link between the docket and unrelated entities.", 0, docket1.GetRelatedParents().Count());

			Logger.TopLevelDataContext.AddDataSource(dataContextType, DataContextKeyForLinking);
			Factory.SaveForTesting();

			var docket2 = GetNewReader(Data.ShipmentDataObject, Logger).ReadIntoBusinessObject();
			AssertContainsExactElementsInAnyOrder([parent.PK], docket2.GetRelatedParents().Select(p => p.PK));

			Factory.SaveForTesting();

			var otherFactory = new UniversalObjectFactory();
			var pivots = otherFactory.Load<WhsDocketJobPivot>(new ZQuery(WhsDocketJobPivotSchema.WV_WD_Docket, docket2.PK));

			AssertContainsExactElementsInAnyOrder([parent.PK], pivots.Select(p => p.WV_ParentId));
			AssertContainsExactElementsInAnyOrder([parent.TablePrefix], pivots.Select(p => p.WV_ParentTableCode));
		}

		protected virtual ZString DataContextKeyForLinking
		{
			get { return "J0001001"; }
		}

		#endregion

		#region TestMatchViaJobNumberButDoNotProvideExternalReference_ShouldKeepExistingReference

		public void TestMatchViaJobNumberButDoNotProvideExternalReference_ShouldKeepExistingReference()
		{
			Data.ShipmentDataObject.Order = null; // no external reference provided

			var client = Data.CreateClientOrgCRAHOLSYDInDB();
			var warehouse = Helper.CreateWarehouse("MEL");
			var docketToMatch = GetNewDocket(client, warehouse, "123");
			docketToMatch.IsUniqueExternalReferenceCreatedOnSave = false;
			Factory.SaveForTesting();

			Data.ShipmentDataObject.DataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
			Data.ShipmentDataObject.DataContext.AddDataTarget(DataContext, docketToMatch.WD_DocketID);
			var results = GetImportResultsViaDataContextManager(Data.ShipmentDataObject);
			var matchedDocket = (TDocket)results.Single(r => r.DataContextType == DataContext).GetBizOForTesting(Data.ShipmentDataObject, new BusinessObjectFactory());
			AssertEquals(docketToMatch.PK, matchedDocket.PK);
			AssertEquals("Should not clear out the current Reference.", "123", matchedDocket.WD_ExternalReference);
		}

		#endregion

		#region TestOrderLines_SumOfLineWeightsExceedsMaxWeight

		public void TestOrderLines_SumOfLineWeightsExceedsMaxWeight()
		{
			Data.CreateClientOrgCRAHOLSYDInDB();
			Data.GetOrCreateWarehouseInDB();

			var product = Helper.CreateProduct(Data.Orgs.CRAHOLSYD, "BOWLHAT");
			product.OP_Weight = 50000;

			Factory.SaveForTesting();

			var orderLines = new DataObjectList<OrderLine>();
			for (var i = 1; i <= 10; i++)
			{
				var orderLineDataObject = new OrderLine();
				orderLineDataObject.Product = new Product { Code = "BOWLHAT", Description = "Bowler Hat" };
				orderLineDataObject.OrderedQty = 28.2m;
				orderLineDataObject.LineNumber = i;
				orderLines.Add(orderLineDataObject);
			}

			ShipmentDataObject.Order.SetOrderLineCollection(() => orderLines);
			ShipmentDataObject.Order.TotalLineWeight = 50m;

			var reader = GetNewReader(ShipmentDataObject, Logger);
			var whsDocketBO = reader.ReadIntoBusinessObject();

			AssertNotNull(whsDocketBO);
			AssertEquals("whsOrderBO.Lines.Count", 10, whsDocketBO.Lines.Count);
			Factory.SaveForTesting(); // Should not blowup
			AssertEquals(whsDocketBO.WD_TotalWeight, 50m);
		}

		#endregion

		#region TestOrderLines_SumOfLineVolumesExceedsMaxVolume

		public void TestOrderLines_SumOfLineVolumesExceedsMaxVolume()
		{
			Data.CreateClientOrgCRAHOLSYDInDB();
			Data.GetOrCreateWarehouseInDB();

			var product = Helper.CreateProduct(Data.Orgs.CRAHOLSYD, "BOWLHAT");
			product.OP_Cubic = 50000;

			Factory.SaveForTesting();

			var orderLines = new DataObjectList<OrderLine>();
			for (var i = 1; i <= 10; i++)
			{
				var orderLineDataObject = new OrderLine();
				orderLineDataObject.Product = new Product { Code = "BOWLHAT", Description = "Bowler Hat" };
				orderLineDataObject.OrderedQty = 28.2m;
				orderLineDataObject.LineNumber = i;
				orderLines.Add(orderLineDataObject);
			}

			ShipmentDataObject.Order.SetOrderLineCollection(() => orderLines);
			ShipmentDataObject.Order.TotalLineVolume = 50m;
			var reader = GetNewReader(ShipmentDataObject, Logger);
			var whsDocketBO = reader.ReadIntoBusinessObject();

			AssertNotNull(whsDocketBO);
			AssertEquals("whsOrderBO.Lines.Count", 10, whsDocketBO.Lines.Count);
			Factory.SaveForTesting(); // Should not blowup
			AssertEquals(whsDocketBO.WD_TotalCubic, 50m);
		}

		#endregion

		// Customs Matching tests

		#region Test_CustomsSource_SetsIsInwardProcessing

		public void Test_CustomsSource_SetsIsInwardProcessing_DefaultToBondedWarehouse()
		{
			Data.SetupProductsAndShipmentDataObjectForCustomsImportInDB();

			var reader = GetNewReader(Data.ShipmentDataObject, Logger);

			using (ObjectFactory.Substitute<IAllocationEngineManager>(new AllocateFIFOLegacyMock()))
			using (ObjectFactory.Substitute("WarehouseRegimeTypeProviders", new Hashtable()))
			{
				var newDocket = reader.ReadIntoBusinessObject();
				AssertEquals(false, Logger.HasErrors);
				AssertEquals(false, Logger.HasWarnings);
				AssertEquals("Should default to CustomsRegime.BondedWarehouse.", false, newDocket.WD_IsInwardsProcessingJob);
			}
		}

		public void Test_CustomsSource_SetsIsInwardProcessing_BondedWarehouse()
			=> Test_CustomsSource_SetsIsInwardProcessingCore(isInwardProcessing: false);

		public void Test_CustomsSource_SetsIsInwardProcessing_InwardsProcessing()
			=> Test_CustomsSource_SetsIsInwardProcessingCore(isInwardProcessing: true);

		void Test_CustomsSource_SetsIsInwardProcessingCore(bool isInwardProcessing)
		{
			Data.SetupProductsAndShipmentDataObjectForCustomsImportInDB(isWarehouseCreatedAsVirtual: true, createInventoryAsInwardProcessing: isInwardProcessing);

			var warehouseRegimeTypeProviderMock = WarehouseCustomsDetailsProvidersMocks.CreateWarehouseRegimeTypeProviderMock(isInwardProcessing ? CustomsRegime.InwardProcessing : CustomsRegime.BondedWarehouse);
			var warehouseRegimeTypeProvider = new Hashtable { { "Shared", new TestObjectHandle(warehouseRegimeTypeProviderMock) } };
			using (ObjectFactory.Substitute("WarehouseRegimeTypeProviders", warehouseRegimeTypeProvider))
			{
				var reader = GetNewReader(Data.ShipmentDataObject, Logger);
				var newDocket = reader.ReadIntoBusinessObject();
				AssertEquals(false, Logger.HasErrors);
				AssertEquals(false, Logger.HasWarnings);

				AssertEquals("Should set Docket to appropriate regime type.", isInwardProcessing, newDocket.WD_IsInwardsProcessingJob);
			}
		}

		public void Test_NonCustomsSource_IgnoresRegimeType()
		{
			Data.CreateClientOrgCRAHOLSYDInDB();
			Data.GetOrCreateWarehouseInDB();
			Factory.SaveForTesting();

			using (new WarehouseCustomsDetailsProvidersMocks(outOfRegimeType: CustomsRegime.InwardProcessing))
			{
				var reader = GetNewReader(ShipmentDataObject, Logger);
				var newDocket = reader.ReadIntoBusinessObject();
				AssertEquals(false, Logger.HasErrors);
				AssertEquals(false, Logger.HasWarnings);

				AssertEquals("Should not set Docket to Inward Processing Job.", false, newDocket.WD_IsInwardsProcessingJob);
			}
		}

		#endregion

		#region Test_CustomsSource_MatchingCancelledJobInRealWhs_CreatesNewJobWithIncreasedSplitNo

		public void Test_CustomsSource_MatchingCancelledJobInRealWhs_CreatesNewJobWithIncreasedSplitNo()
		{
			Data.SetupProductsAndShipmentDataObjectForCustomsImportInDB();
			var docket = GetNewDocket(Data.Orgs.CRAHOLSYD, Data.GetOrCreateWarehouseInDB(), "B123");
			docket.WD_CustomsParentReference = "B123-EDIDATEDI";
			docket.WD_DocketStatus = DocketStatus.Codes.Cancelled;
			Factory.SaveForTesting();
			AssertEquals("Precondition - Docket should be Cancelled.", true, docket.IsCancelled);

			var reader1 = GetNewReader(Data.ShipmentDataObject, Logger);
			var newDocket = GetDocketWithAllocateMock(reader1);
			AssertNotEquals(docket.PK, newDocket.PK);
			AssertEquals(Data.Orgs.CRAHOLSYD.PK, newDocket.WD_OH_Client);
			AssertEquals("B123", newDocket.WD_ExternalReference);
			AssertEquals("B123-EDIDATEDI", newDocket.WD_CustomsParentReference);
			AssertEquals(new ZByte(1), newDocket.WD_ExternalReferenceSplit);
			AssertEquals(false, Logger.HasErrors);
			AssertEquals(false, Logger.HasWarnings);

			var reader2 = GetNewReader(Data.ShipmentDataObject, Logger);
			var rematchedDocket = GetDocketWithAllocateMock(reader2);
			AssertEquals(rematchedDocket.PK, newDocket.PK);
			AssertEquals(Data.Orgs.CRAHOLSYD.PK, rematchedDocket.WD_OH_Client);
			AssertEquals("B123", rematchedDocket.WD_ExternalReference);
			AssertEquals("B123-EDIDATEDI", rematchedDocket.WD_CustomsParentReference);
			AssertEquals(new ZByte(1), rematchedDocket.WD_ExternalReferenceSplit);
			AssertEquals(false, Logger.HasErrors);
			AssertEquals(false, Logger.HasWarnings);
			Factory.SaveForTesting();
		}

		public void Test_CustomsSource_MatchingCancelledJobInRealWhs_CreatesNewJobWithIncreasedSplitNo_PreviousDocketHasMaxSplitNo()
		{
			Data.SetupProductsAndShipmentDataObjectForCustomsImportInDB();
			var docket = GetNewDocket(Data.Orgs.CRAHOLSYD, Data.GetOrCreateWarehouseInDB(), "B123");
			docket.WD_CustomsParentReference = "B123-EDIDATEDI";
			docket.WD_DocketStatus = DocketStatus.Codes.Cancelled;
			docket.WD_ExternalReferenceSplit = byte.MaxValue;
			Factory.SaveForTesting();
			AssertEquals("Precondition - Docket should be Cancelled.", true, docket.IsCancelled);

			var reader = GetNewReader(Data.ShipmentDataObject, Logger);
			AssertExceptionThrown(
				typeof(DataObjectReadFailureException),
				string.Format("Cannot Import {0}\r\nUnable to generate a new {0} as the previous {0} has reached the maximum allowed splits.", GetDocketType()),
				() => GetNewReader(ShipmentDataObject, Logger, useCleanFactory: true).ReadIntoBusinessObject());
		}

		#endregion

		#region Test_CustomsSource_MatchingOnExternalReference

		public void Test_CustomsSource_MatchingOnExternalReference()
		{
			Data.SetupProductsAndShipmentDataObjectForCustomsImportInDB();

			var reader = GetNewReader(Data.ShipmentDataObject, Logger);
			var docket1 = GetDocketWithAllocateMock(reader);
			AssertNotNull(docket1);
			docket1.WD_ExternalReference = "B187";
			docket1.WD_CustomsParentReference = "B187-EDIDATEDI";

			Factory.SaveForTesting();
			var docket2 = GetDocketWithAllocateMock(reader);
			AssertNotEquals(docket1.PK, docket2.PK);

			Factory.SaveForTesting();
			// remove the client and ensure we still match on the BJob number.
			AssertEquals("Precondition", "B123", docket2.WD_ExternalReference);
			AssertEquals("Precondition", "B123-EDIDATEDI", docket2.WD_CustomsParentReference);
			Data.ShipmentDataObject.OrganizationAddressCollection.Clear();
			var docket3 = GetDocketWithAllocateMock(GetNewReader(Data.ShipmentDataObject, Logger));
			AssertEquals(docket2.PK, docket3.PK);
		}

		#endregion

		#region Test_CustomsSource_MatchingFinalisedJobInRealWhs_RejectsImport

		public void Test_CustomsSource_MatchingFinalisedJobInRealWhs_RejectsImport()
		{
			Data.SetupProductsAndShipmentDataObjectForCustomsImportInDB();

			// create an existing order originating from Customs universal
			var warehouse = Data.GetOrCreateWarehouseInDB();
			var docket = GetDocketWithLineForCustoms(Data.Orgs.CRAHOLSYD, warehouse, "B123", "B123-EDIDATEDI", Data.Product, 10m, "EntryNumber123", 2, "DummyOutward-1", 102, finalise: true);
			Factory.SaveForTesting();
			AssertIsFinalisedPrecondition(docket);

			var importResults = GetImportResultsViaDataContextManager(ShipmentDataObject);
			var failureResult = importResults.Single(r => r.DataContextType == DataContext);
			AssertEquals(false, failureResult.WasSuccessful);
			AssertContains(ErrorMessageForAttemptingAmendingFinalisedDocketInRealWarehouse, failureResult.ToString());
		}

		#endregion

		#region TestIUSWarehouseCustomsLineDetails

		public void TestIUSWarehouseCustomsLineDetails_Consumption()
		{
			TestAndAssertIUSWarehouseCustomsLineDetailsCore(zoneStatusCode: "P", fromOtherFTZ: true, outwardType: OutwardType.Consumption, expectedZoneStatusCode: "P", expectedIsFromOtherFTZ: true, expectedOutwardType: "CNN");
		}

		public void TestIUSWarehouseCustomsLineDetails_Exports()
		{
			TestAndAssertIUSWarehouseCustomsLineDetailsCore(zoneStatusCode: "D", fromOtherFTZ: false, outwardType: OutwardType.Exports, expectedZoneStatusCode: "D", expectedIsFromOtherFTZ: false, expectedOutwardType: "EXS");
		}

		public void TestIUSWarehouseCustomsLineDetails_ToOtherFTZ()
		{
			TestAndAssertIUSWarehouseCustomsLineDetailsCore(zoneStatusCode: "Z", fromOtherFTZ: true, outwardType: OutwardType.ToOtherFTZ, expectedZoneStatusCode: "Z", expectedIsFromOtherFTZ: true, expectedOutwardType: "TOF");
		}

		public void TestIUSWarehouseCustomsLineDetails_NULL()
		{
			TestAndAssertIUSWarehouseCustomsLineDetailsCore(zoneStatusCode: null, fromOtherFTZ: null, outwardType: null, expectedZoneStatusCode: "", expectedIsFromOtherFTZ: false, expectedOutwardType: ExpectedDefaultOutwardType);
		}

		void TestAndAssertIUSWarehouseCustomsLineDetailsCore(ZString? zoneStatusCode, ZBool? fromOtherFTZ, OutwardType? outwardType, string expectedZoneStatusCode, bool expectedIsFromOtherFTZ, string expectedOutwardType)
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedStates))
			{
				Data.SetupProductsAndShipmentDataObjectForCustomsImportInDB(isFTZWarehouse: true);

				var invoiceLine = WarehouseCustomsDetailsProvidersMocks.CreateInvoiceLine();
				var lineDetails = WarehouseCustomsDetailsProvidersMocks.CreateWarehouseCustomsLineDetailsMockUS(invoiceLine, zoneStatusCode: zoneStatusCode, isFromOtherFTZ: fromOtherFTZ, outwardType: outwardType);

				using (new WarehouseCustomsDetailsProvidersMocks(new[] { lineDetails }))
				{
					var data = new TestDataForUniversal(Factory, Logger, DataContextType.InBond);
					var reader = GetNewReader(ShipmentDataObject, Logger);
					var docket = GetDocketWithAllocateMock(reader);

					var customsData = docket.Lines[0].CustomsData;

					AssertEquals("WB_ZoneStatus: It expected to **not** read for order, It will be populated after create pick.", PopulatesCustomsInfoFromUXML ? "" : expectedZoneStatusCode, customsData.WB_ZoneStatus);
					AssertEquals("WB_IsFromAnotherFTZWhs: It expected to **not** read for order, It will be populated after create pick.", !PopulatesCustomsInfoFromUXML && expectedIsFromOtherFTZ, customsData.WB_IsFromAnotherFTZWhs);
					AssertEquals("WB_OutwardType: It expected always set value.", expectedOutwardType, customsData.WB_OutwardType);
				}
			}
		}

		#endregion

		#region Test_CustomsSource_MatchingFinalisedJobInRealWhs_DoesNotRejectImportIfForChangeOfOwnership

		public void Test_CustomsSource_MatchingFinalisedJobInRealWhs_DoesNotRejectImportIfForChangeOfOwnership()
		{
			using (new WarehouseCustomsDetailsProvidersMocks())
			{
				var data = new TestDataForUniversal(Factory, Logger, DataContextType.WarehouseBondedChangeOfInventory);
				data.SetupProductsAndShipmentDataObjectForCustomsImportInDB(recipientRoles: new[] { new RecipientRoleDetail { Type = RecipientRoleType.BCO } });

				// create an existing order originating from Customs universal
				var warehouse = data.GetOrCreateWarehouseInDB();
				var docket = GetDocketWithLineForCustoms(GetOwnerForChangeOfOwnershipTests(data), warehouse, "B123", "B123-EDIDATEDI", GetProductForChangeOfOwnershipTests(data), 10m, "EntryNumber123", 2, "DummyOutward-1", 102, finalise: true);
				AssertIsFinalisedPrecondition(docket);
				Factory.SaveForTesting();

				var importResults = GetImportResultsViaDataContextManager(data.ShipmentDataObject);
				var importResult = importResults.Single(r => r.DataContextType == DataContextType.WarehouseBondedChangeOfInventory);
				AssertEquals(true, importResult.WasSuccessful);
			}
		}

		protected virtual OrgHeader GetOwnerForChangeOfOwnershipTests(TestDataForUniversal data)
		{
			return data.Orgs.CRAHOLSYD;
		}

		protected virtual OrgSupplierPart GetProductForChangeOfOwnershipTests(TestDataForUniversal data)
		{
			return data.Product;
		}

		#endregion

		#region TestMarkAsImportingForChangeOfInventory

		public void TestIsImportingForChangeOfInventory_Default()
		{
			var data = new TestDataForUniversal(Factory, Logger, DataContext);
			data.SetupProductsAndShipmentDataObjectForCustomsImportInDB();
			var reader = GetNewReader(data.ShipmentDataObject, Logger);
			var docket = GetDocketWithAllocateMock(reader);
			AssertEquals(false, docket.IsImportingForChangeOfInventory);
		}

		#endregion

		// Customs Import tests

		#region Test_CustomsSource_ImportOfCommercialInvoiceLines

		public void Test_CustomsSource_ImportOfCommercialInvoiceLines()
		{
			Test_CustomsSource_ImportOfCommercialInvoiceLinesCore(false);
		}

		public void Test_CustomsSource_ImportOfCommercialInvoiceLines_WithSerialNumber()
		{
			Test_CustomsSource_ImportOfCommercialInvoiceLinesCore(true);
		}

		void Test_CustomsSource_ImportOfCommercialInvoiceLinesCore(bool enableSerialNumber)
		{
			Data.SetupProductsAndShipmentDataObjectForCustomsImportInDB(isSerialNumberTest: enableSerialNumber);

			var reader = GetNewReader(ShipmentDataObject, Logger);
			var docket = GetDocketWithAllocateMock(reader);
			Factory.SaveAtEndOfImport(Logger);
			AssertEquals(1, docket.Lines.Count);

			CombineAssertions(delegate
			{
				AssertLineFromCommercialInvoiceLine((TDocketLine)docket.Lines[0], enableSerialNumber);
				AssertEquals(false, Logger.HasErrors);
			});
		}

		protected virtual bool IsExWarehouse => false;

		protected override TestDataForUniversal GetNewTestData()
		{
			var result = base.GetNewTestData();
			if (IsExWarehouse)
			{
				result.ShipmentDataObject.MessageType = new UniversalCodeDescriptionPair() { Code = "EXW" };
			}
			return result;
		}

		void AssertLineFromCommercialInvoiceLine(TDocketLine line, bool isSerialNumberTest)
		{
			var infos = GetLineInfosForCommercialInvoiceLineTest(line);

			int i = 0;
			var productCodeInfo = infos[i++];
			var unitsInfo = infos[i++];
			var unitsUQInfo = infos[i++];

			var customsAddInfoInfo = infos[i++];
			var customsQtyInfo = infos[i++];
			var customsQtyUQInfo = infos[i++];
			var customsEntryKeyInfo = infos[i++];
			var customsEntryLineNoInfo = infos[i++];
			var customsCountryOfOriginInfo = infos[i++];
			var customsTILVInfo = infos[i++];
			var customsValueForDutyInfo = infos[i++];

			var linePriceInfo = infos[i++];
			var lineNoInfo = infos[i++];
			var partAttrib1Info = infos[i++];
			var partAttrib2Info = infos[i++];
			var partAttrib3Info = infos[i++];
			var serialNumberInfo = infos[i++];
			var bondedKeyInfo = infos[i++];

			var customsWBDeclarationReferenceInfo = infos[i++];

			var customsSecondQuantityInfo = infos[i++];
			var customsSecondUnitQtyInfo = infos[i++];
			var tariffInfo = infos[i++];
			var primaryPreferenceInfo = infos[i++];

			var customsThirdQuantityInfo = infos[i++];
			var customsThirdUnitQtyInfo = infos[i++];
			// ManufacturerAddress need to be tested after customs team implement it
			var customsZoneStatusInfo = infos[i++];
			var customsIsFromAnotherFTZWhsInfo = infos[i++];
			var customsOutwardTypeInfo = infos[i++];
			var customsDeadLineInfo = infos[i++];
			var inwardStyleInfo = infos[i++];
			var inwardProcedureInfo = infos[i++];

			AssertEquals("Product Code", "P1", productCodeInfo.Value);
			AssertEquals("Bonded Qty", isSerialNumberTest ? 1m : 5m, unitsInfo.Value);
			AssertEquals("Bonded Qty UQ", "BOX", unitsUQInfo.Value);

			AssertEquals("Customs AddInfo", IsExWarehouse ? ZString.Empty : ExpectedCustomsAddInfo, customsAddInfoInfo.Value);
			AssertEquals("Customs Qty", PopulatesCustomsInfoFromUXML ? 0m : 6m, customsQtyInfo.Value);
			AssertEquals("Customs Qty UQ", PopulatesCustomsInfoFromUXML ? "" : "PCE", customsQtyUQInfo.Value);
			AssertEquals("Customs Entry Key", ExpectedCustomsEntryKey, customsEntryKeyInfo.Value);
			AssertEquals("Customs Entry Line No.", ExpectedCustomsEntryLineNo, customsEntryLineNoInfo.Value);
			AssertEquals("Customs Country of Origin", PopulatesCustomsInfoFromUXML ? "" : "IT", customsCountryOfOriginInfo.Value);
			AssertEquals("Customs TILV", PopulatesCustomsInfoFromUXML ? 0m : 777m, customsTILVInfo.Value);
			AssertEquals("Customs Value for Duty", PopulatesCustomsInfoFromUXML ? 0m : 888m, customsValueForDutyInfo.Value);

			if (linePriceInfo != null)
			{
				AssertEquals("Line Price", 10.3m, linePriceInfo.Value);
			}

			AssertEquals("Order/Comm Inv Line No", new ZShort(3), lineNoInfo.Value);
			AssertEquals("Attrib1", "Red", partAttrib1Info.Value);
			AssertEquals("Attrib2", "Medium", partAttrib2Info.Value);
			AssertEquals("Attrib3", "S1234", partAttrib3Info.Value);
			AssertEquals("SerialNumber", isSerialNumberTest ? "SNN" : "", serialNumberInfo.Value);
			AssertEquals("Bonded Entry Key", "ENTRYNUMBER123-2", bondedKeyInfo.Value);

			AssertEquals("B123", customsWBDeclarationReferenceInfo.Value);

			AssertEquals("Customs Second Qty", PopulatesCustomsInfoFromUXML ? 0m : 12m, customsSecondQuantityInfo.Value);
			AssertEquals("Customs Second Qty UQ", PopulatesCustomsInfoFromUXML ? "" : "BOX", customsSecondUnitQtyInfo.Value);
			AssertEquals("Customs Tariff", PopulatesCustomsInfoFromUXML ? "" : "T2", tariffInfo.Value);
			AssertEquals("Customs Primary Preference", PopulatesCustomsInfoFromUXML ? "" : "PP", primaryPreferenceInfo.Value);

			AssertEquals("Customs Third Qty", PopulatesCustomsInfoFromUXML ? 0m : 12m, customsSecondQuantityInfo.Value);
			AssertEquals("Customs Third Qty UQ", PopulatesCustomsInfoFromUXML ? "" : "BOX", customsSecondUnitQtyInfo.Value);

			AssertEquals("Customs Zone Status _ is *NOT* impliment IUSWarehouseCustomsLineDetails", "", customsZoneStatusInfo.Value);
			AssertEquals("Customs Is From Another FTZ _ is *NOT* impliment IUSWarehouseCustomsLineDetails", false, customsIsFromAnotherFTZWhsInfo.Value);
			AssertEquals("Customs Outward Type _ is *NOT* impliment IUSWarehouseCustomsLineDetails", ExpectedDefaultOutwardType, customsOutwardTypeInfo.Value);

			AssertEquals(ZDateTime.Empty, customsDeadLineInfo.Value);
			AssertEquals("", inwardStyleInfo.Value);
			AssertEquals("", inwardProcedureInfo.Value);
		}

		protected abstract ZBool PopulatesCustomsInfoFromUXML { get; }
		protected virtual ZString ExpectedCustomsEntryKey => "EntryNumber123";
		protected virtual ZShort ExpectedCustomsEntryLineNo => 2;
		protected virtual ZString ExpectedCustomsAddInfo => "TILV4Warehouse=999*Moo=50";
		protected virtual ZString ExpectedDefaultOutwardType => "";

		protected virtual ZPropertyInfo[] GetLineInfosForCommercialInvoiceLineTest(TDocketLine line)
		{
			return new ZPropertyInfo[]
			{
				line.SupplierPart.OP_PartNumInfo,
				line.WE_TransactionQuantityInfo,
				line.WE_F3_NKPackTypeInfo,

				line.CustomsData.WB_AddInfoInfo,
				line.CustomsData.WB_CustomsQtyInfo,
				line.CustomsData.WB_CustomsUnitOfQtyInfo,
				line.CustomsData.WB_EntryKeyInfo,
				line.CustomsData.WB_EntryLineNoInfo,
				line.CustomsData.WB_RN_NKCountryOfOriginInfo,
				line.CustomsData.WB_TILVInfo,
				line.CustomsData.WB_ValueForDutyInfo,

				line.WE_ExtendedLinePriceInfo,
				line.WE_LineNoInfo,
				line.WE_PartAttrib1Info,
				line.WE_PartAttrib2Info,
				line.WE_PartAttrib3Info,
				line.WE_SerialNumberInfo,
				line.WE_BondedEntryKeyInfo,

				line.CustomsData.WB_DeclarationReferenceInfo,

				line.CustomsData.WB_CustomsSecondQuantityInfo,
				line.CustomsData.WB_CustomsSecondUnitQtyInfo,
				line.CustomsData.WB_TariffInfo,
				line.CustomsData.WB_PrimaryPreferenceInfo,

				line.CustomsData.WB_CustomsThirdQuantityInfo,
				line.CustomsData.WB_CustomsThirdUnitQtyInfo,
				// ManufacturerAddress need to be tested after customs team implement it
				line.CustomsData.WB_ZoneStatusInfo,
				line.CustomsData.WB_IsFromAnotherFTZWhsInfo,
				line.CustomsData.WB_OutwardTypeInfo,
				line.CustomsData.WB_CustomsDeadlineInfo,
				line.CustomsData.WB_InwardStyleInfo,
				line.CustomsData.WB_InwardProcedureInfo,
			};
		}

		#endregion

		#region Test_CustomsSource_DocAddressToIgnore

		public void Test_CustomsSource_DocAddressToIgnore()
		{
			Data.SetupProductsAndShipmentDataObjectForCustomsImportInDB();

			var reader = GetNewReader(ShipmentDataObject, Logger, useCleanFactory: true);
			var docket = GetDocketWithAllocateMock(reader);
			AssertEquals(false, Logger.HasErrors);
			AssertEquals("Importer and CustomsWarehouse are not valid WhsDocket AddressTypes and should not be added to the Docket's Address collection.", false, Logger.HasWarnings);
		}

		#endregion

		#region Test_CustomsSource_ImportOfClient

		public void Test_CustomsSource_ImportOfClient()
		{
			Data.SetupProductsAndShipmentDataObjectForCustomsImportInDB(addImporterToXML: false);

			// no importer (client)
			var reader1 = GetNewReader(ShipmentDataObject, Logger, useCleanFactory: true);
			AssertExceptionThrown(typeof(DataObjectReadFailureException),
				string.Format("Cannot Import {0}\r\nNo Client Address was provided.", GetDocketType()), () => reader1.ReadIntoBusinessObject());

			// importer address does not match
			var reader2 = GetNewReader(ShipmentDataObject, Logger, useCleanFactory: true);
			var orgAddressDataObject = GetNewAddressData_INTHEMSYD(DocAddressType.ImporterDocumentaryAddress);
			ShipmentDataObject.OrganizationAddressCollection.Add(orgAddressDataObject);
			AssertExceptionThrown(typeof(DataObjectReadFailureException), () => reader2.ReadIntoBusinessObject());

			// import matches
			ShipmentDataObject.OrganizationAddressCollection.Remove(orgAddressDataObject);
			ShipmentDataObject.OrganizationAddressCollection.Add(Data.Orgs.ImporterAddressDataObject_CRAHOLSYD);
			var orgAddress = new OrganisationDataObjectReader(orgAddressDataObject, Logger, Factory).GetMatchedOrNewForTesting();

			Factory.SaveForTesting();
			Logger.ClearLogs(); // cleanup

			var reader3 = GetNewReader(ShipmentDataObject, Logger, useCleanFactory: true);
			var docket = GetDocketWithAllocateMock(reader3);
			AssertAddressContentMatches_CRAHOLSYD(docket.Client.MainAddress);
			AssertEquals(false, Logger.HasErrors);
		}

		public void Test_CustomsSource_ImportOfClient_WarehouseClientTakesPrecedenceOverImporteDocumentary()
		{
			Data.SetupProductsAndShipmentDataObjectForCustomsImportInDB(addImporterToXML: false);

			// add WUFSHIJNB Product and Inventory
			Data.ProductWUFSHIJNB.OP_PartNum = "P1"; // ensure product for WUFSHIJNB is valid (P1 is in the UXML)
			var receive = Helper.CreateWhsReceive(Data.Orgs.WUFSHIJNB, Data.GetOrCreateWarehouseInDB(), "R2");
			Helper.CreateWhsReceiveInventoryLine(receive, Data.ProductWUFSHIJNB, 100m, ZDate.Empty, ZDate.Empty, "Red", "Medium", "S1234", "EntryNumber123-2");
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocketWithoutUserConfirmation();
			Factory.SaveForTesting();
			AssertIsFinalisedPrecondition(receive);

			var warehouseClientDO = GetNewAddressData_CRAHOLSYD(AddressTypes.WarehouseClient);
			var importerDocumentaryAddressDO = GetNewAddressData_WUFSHIJNB(DocAddressType.ImporterDocumentaryAddress);

			ShipmentDataObject.OrganizationAddressCollection.Add(warehouseClientDO);
			ShipmentDataObject.OrganizationAddressCollection.Add(importerDocumentaryAddressDO);

			var docket = GetDocketWithAllocateMock(GetNewReader(ShipmentDataObject, Logger, useCleanFactory: true));
			AssertAddressContentMatches_CRAHOLSYD(docket.Client.MainAddress);
			AssertEquals(false, Logger.HasErrors);

			Logger.ClearLogs(); // cleanup
			ShipmentDataObject.OrganizationAddressCollection.Remove(warehouseClientDO);
			var docket2 = GetDocketWithAllocateMock(GetNewReader(ShipmentDataObject, Logger, useCleanFactory: true));
			AssertAddressContentMatches_WUFSHIJNB(docket2.Client.MainAddress);
			AssertEquals(false, Logger.HasErrors);
		}

		#endregion

		#region Test_CustomsSource_ImportOfWarehouse

		public void Test_CustomsSource_ImportOfWarehouse()
		{
			Data.SetupProductsAndShipmentDataObjectForCustomsImportInDB();

			// warehouse exists matching address, should find it
			var reader = GetNewReader(ShipmentDataObject, Logger);
			var docket = GetDocketWithAllocateMock(reader);
			AssertAddressContentMatches_INTHEMSYD(docket.Warehouse.WarehouseAddress);
			AssertEquals(false, Logger.HasErrors);
		}

		public void Test_CustomsSource_ImportOfWarehouse_DoesNotMatchTransitWarehouses()
		{
			Data.SetupProductsAndShipmentDataObjectForCustomsImportInDB(createInventory: false);
			Data.GetOrCreateWarehouseInDB().WW_WarehouseType = WarehouseTypes.Codes.Transit;
			Factory.SaveForTesting();

			bool actionWasInvoked = false;
			var reader = GetNewReader(Data.ShipmentDataObject, Logger);
			Func<TDocket> attemptImport = () =>
			{
				actionWasInvoked = true;
				return GetDocketWithAllocateMock(reader);
			};

			AssertTransitWarehousesAreNotMatchedOnImport(attemptImport);
			AssertEquals("Subclasses should invoke import action to Test behaviour.", true, actionWasInvoked);
		}

		protected abstract void AssertTransitWarehousesAreNotMatchedOnImport(Func<TDocket> attemptImport);

		public void Test_CustomsSource_ImportOfWarehouse_RejectsImportIfWhsAddressIsBad()
		{
			Data.SetupProductsAndShipmentDataObjectForCustomsImportInDB(addWarehouseToXML: false);

			// set a *bad* address on the import file
			var orgAddressDataObject = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance);
			orgAddressDataObject.AddressType = nameof(DocAddressType.CustomsWarehouseAddress);
			orgAddressDataObject.Address1 = "oink";
			ShipmentDataObject.OrganizationAddressCollection.Add(orgAddressDataObject);

			// address is bad, should reject import
			AssertExceptionThrown("Can't read in the Docket without a valid Warehouse Address.", typeof(DataObjectReadFailureException),
			string.Format(@"
Cannot Import {0}
Unable to match Warehouse Address, please make sure the supplied Warehouse Address is valid. Details were:
Address1: oink".Trim(), GetDocketType()), () => GetNewReader(ShipmentDataObject, Logger, useCleanFactory: true).ReadIntoBusinessObject());
		}

		#endregion

		#region Test_CustomsSource_ImportOfWarehouse_RejectImportIfValidWarehouseAddressButNoWarehouseInDB

		public void Test_CustomsSource_ImportOfWarehouse_RejectImportIfValidAddressButNoWarehouseInDB()
		{
			Data.SetupProductsAndShipmentDataObjectForCustomsImportInDB(createWarehouse: false, isWarehouseCreatedAsVirtual: true);

			AssertExceptionThrown("Can't read in the Docket without a valid Warehouse Address.", typeof(DataObjectReadFailureException),
			string.Format(@"
Cannot Import {0}
Unable to match Warehouse for Organization: In The Moment Address: Unit 12, Level 3.
".Trim(), GetDocketType()), () => GetNewReader(ShipmentDataObject, Logger, useCleanFactory: true).ReadIntoBusinessObject());
		}

		#endregion

		#region Test_CustomsSource_ImportOfZeroQtyLinesAreIgnored

		public void Test_CustomsSource_ImportOfZeroQtyLinesAreIgnored()
		{
			Test_CustomsSource_ImportOfZeroQtyLinesAreIgnoredCore(false);
		}

		public void Test_CustomsSource_ImportOfZeroQtyLinesAreIgnored_WithSerialNumber()
		{
			Test_CustomsSource_ImportOfZeroQtyLinesAreIgnoredCore(true);
		}

		void Test_CustomsSource_ImportOfZeroQtyLinesAreIgnoredCore(bool enableSerialNumber)
		{
			Data.SetupProductsAndShipmentDataObjectForCustomsImportInDB(isSerialNumberTest: enableSerialNumber);
			Data.AddCommercialInvoiceLineWithRelatedEntryLine(0, 7, Data.Product); // add a second 'order line' with 0 qty

			var reader = GetNewReader(ShipmentDataObject, Logger);
			var docket = GetDocketWithAllocateMock(reader);
			AssertEquals(1, docket.Lines.Count); // line two have been ignored

			CombineAssertions(delegate
			{
				AssertLineFromCommercialInvoiceLine((TDocketLine)docket.Lines[0], enableSerialNumber);
				AssertEquals(false, Logger.HasErrors);
			});
		}

		#endregion

		#region Test_CustomsSource_ImportIntoRealWhs / Test_CustomsSource_ImportIntoVirtualWhsFinalisesDocket

		[TestDate(2013, 1, 1, 7, 7, 30)]
		public void Test_CustomsSource_ImportIntoRealWhs_ValidationRelaxFromCustoms()
		{
			Assert_CustomsSource_ImportFinalisesDocketIfVirtualWhs(useVirtualWhs: false, validationRelaxed: true);
		}

		[TestDate(2013, 1, 1, 7, 7, 30)]
		public void Test_CustomsSource_ImportIntoRealWhs_FinaliseNotAllowed()
		{
			Assert_CustomsSource_ImportFinalisesDocketIfVirtualWhs(useVirtualWhs: false, validationRelaxed: false);
		}

		[TestDate(2013, 1, 1, 7, 7, 30)]
		public void Test_CustomsSource_ImportIntoVirtualWhsFinalisesDocket()
		{
			Assert_CustomsSource_ImportFinalisesDocketIfVirtualWhs(useVirtualWhs: true, validationRelaxed: false);
		}

		void Assert_CustomsSource_ImportFinalisesDocketIfVirtualWhs(bool useVirtualWhs, bool validationRelaxed)
		{
			TDocket docket;
			Data.SetupProductsAndShipmentDataObjectForCustomsImportInDB(isWarehouseCreatedAsVirtual: useVirtualWhs);

			var putawayEngineMock = PutawayEngineManagerForReceiveMockHandler.GetMockPutawayEngineManagerForReceive();

			using (WarehouseDataRegistry.Instance.AllowOrderToBeFinalisedWithoutCustomsClearance.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, validationRelaxed))
			using (ObjectFactory.Substitute(putawayEngineMock.Object))
			using (ObjectFactory.Substitute<IAllocationEngineManager>(new AllocateFIFOLegacyMock()))
			{
				// warehouse exists matching address, should find it
				var reader = GetNewReader(ShipmentDataObject, Logger);
				docket = reader.ReadIntoBusinessObject();
			}

			AssertNotNull("docket", docket);
			AssertEquals("Precondition", false, Logger.HasErrors);
			AssertEquals("CUS", docket.WD_DocketSubType);
			AssertEquals("B123", docket.WD_ExternalReference);
			AssertEquals("B123-EDIDATEDI", docket.WD_CustomsParentReference);
			AssertEquals(useVirtualWhs, docket.IsFinalised);

			Assert_CustomsSource_ImportFinalisesDocketIfVirtualWhsCore(docket, useVirtualWhs, validationRelaxed);
			AssertEquals(false, Logger.HasErrors);
		}

		protected abstract void Assert_CustomsSource_ImportFinalisesDocketIfVirtualWhsCore(TDocket docket, bool useVirtualWhs, bool finalisedAllowed);

		#endregion

		#region Test_CustomsSource_ImportIntoVirtualWhsFinalisesDocket_RejectsImportIfUnableToFinalise

		public void Test_CustomsSource_ImportIntoVirtualWhsFinalisesDocket_RejectsImportIfUnableToFinalise()
		{
			Test_CustomsSource_ImportIntoVirtualWhsFinalisesDocket_RejectsImportIfUnableToFinaliseCore();
		}

		protected abstract void Test_CustomsSource_ImportIntoVirtualWhsFinalisesDocket_RejectsImportIfUnableToFinaliseCore();

		#endregion

		#region Test_CustomsSource_ShouldNotMatchWithSupplierInDocketLine

		public void Test_CustomsSource_ShouldNotMatchWithSupplierInDocketLine()
		{
			Data.SetupProductsAndShipmentDataObjectForCustomsImportInDB();

			var client = Data.Orgs.CRAHOLSYD;
			var warehouse = Data.GetOrCreateWarehouseInDB();
			var supplier1 = Helper.CreateClient("Supplier1");
			var supplier2 = Helper.CreateClient("Supplier2");
			var product1 = Data.Product;
			var product2 = Data.CreateProduct(Data.Product.OP_PartNum); // duplicate code
			product1.RelatedOrganisations.AddSupplier(supplier1);
			product2.RelatedOrganisations.AddSupplier(supplier2);

			Helper.CreateWhsReceiveWithInventory(client, warehouse, "R2", product2, 100m, "EntryNumber123-3");

			using (DuplicateProductTriggerSuspenderForTest.Suspend())
			{
				Factory.SaveForTesting();
			}

			var supplier1AddressDO = new OrganizationDataObjectWriter(new DataWritingManager(new DummyActionInfo()), AddressTypes.Supplier).GetDataObject(supplier1.MainAddress);
			var supplier2AddressDO = new OrganizationDataObjectWriter(new DataWritingManager(new DummyActionInfo()), AddressTypes.Supplier).GetDataObject(supplier2.MainAddress);
			var line1 = Data.ShipmentDataObject.CommercialInfo.CommercialInvoiceCollection[0].CommercialInvoiceLineCollection[0];
			var line2 = Data.AddCommercialInvoiceLineWithRelatedEntryLine(10m, Data.CustomsCommInvLineNoOnOrder + 1, product2);
			line1.OrganizationAddressCollection = new List<OrganizationAddress> { supplier2AddressDO };
			line2.OrganizationAddressCollection = new List<OrganizationAddress> { supplier1AddressDO };

			var reader = GetNewReader(Data.ShipmentDataObject, Logger);
			AssertExceptionThrown("Cannot import DocketLine with mulitple product match.", typeof(DataObjectReadFailureException),
				$"Cannot Import {GetDocketType()} Line 1.\r\nMultiple Product matched: {product1.OP_PartNum} for Client {client.OH_Code}.", () => reader.ReadIntoBusinessObject());
		}

		#endregion

		#region Test_CustomsSource_RejectsImportIfBranchIsEmptyOrDoesNotExistInDB

		public void Test_CustomsSource_RejectsImportIfBranchIsEmptyOrDoesNotExistInDB()
		{
			Data.SetupProductsAndShipmentDataObjectForCustomsImportInDB();
			Data.ShipmentDataObject.Branch = null;

			var reader1 = GetNewReader(Data.ShipmentDataObject, Logger, useCleanFactory: true);
			AssertExceptionThrown(typeof(DataObjectReadFailureException),
				"Cannot Import Customs Job B123 as no Branch was provided.", () => GetDocketWithAllocateMock(reader1));

			Data.ShipmentDataObject.Branch = new Branch { Code = "ZZZ" };
			var reader2 = GetNewReader(Data.ShipmentDataObject, Logger, useCleanFactory: true);
			AssertExceptionThrown(typeof(DataObjectReadFailureException),
				"Cannot Import Customs Job B123. Unable to match Branch ZZZ.", () => GetDocketWithAllocateMock(reader2));

			Logger.ClearLogs();
			Data.ShipmentDataObject.Branch = Branch.New(GlbBranch.CurrentBranch);
			var reader3 = GetNewReader(Data.ShipmentDataObject, Logger, useCleanFactory: true);
			AssertNoExceptionThrown(() => GetDocketWithAllocateMock(reader3));
			AssertEquals(false, Logger.HasErrors);
			AssertEquals(false, Logger.HasWarnings);
		}

		#endregion

		#region Test_CustomsSource_RejectsImportIfWarehouseCountryIsDifferentToCountryOnUniversalShipment

		public void Test_CustomsSource_RejectsImportIfWarehouseCountryIsDifferentToCountryOnUniversalShipment()
		{
			Data.SetupProductsAndShipmentDataObjectForCustomsImportInDB();

			// Setup valid branch
			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_RN_NKCountryCode = "US";
			var branch = company.Branches.AddNew();
			branch.FillWithValidTestData();
			branch.GB_Code = "AKL";
			branch.GB_BranchName = "Auckland";
			branch.GB_RL_NKHomePort = "NZAKL";

			Factory.SaveForTesting();

			Data.ShipmentDataObject.Branch = Branch.New(branch);
			var reader1 = GetNewReader(Data.ShipmentDataObject, Logger, useCleanFactory: true);
			AssertExceptionThrown(typeof(DataObjectReadFailureException),
				"Cannot Import Customs Job B123 as its Branch Country (US) does not match the Warehouse Country (AU).", () => GetDocketWithAllocateMock(reader1));

			Logger.ClearLogs();
			company.GC_RN_NKCountryCode = "AU";
			Factory.SaveForTesting();

			var reader2 = GetNewReader(Data.ShipmentDataObject, Logger, useCleanFactory: true);
			AssertNoExceptionThrown(() => GetDocketWithAllocateMock(reader2));
			AssertEquals(false, Logger.HasErrors);
			AssertEquals(false, Logger.HasWarnings);
		}

		#endregion

		#region Test_CustomsSource_WithInactiveWarehouse_UseOnlyActive

		public void Test_CustomsSource_WithInactiveWarehouse_UseOnlyActive()
		{
			Data.SetupProductsAndShipmentDataObjectForCustomsImportInDB();
			var client = Data.CreateClientOrgCRAHOLSYDInDB();
			var whs1 = Data.GetOrCreateWarehouseInDB();

			whs1.WW_IsActive = false;
			Factory.SaveForTesting();

			var reader = GetNewReader(Data.ShipmentDataObject, Logger);
			AssertExceptionThrown(typeof(DataObjectReadFailureException), string.Format("Cannot Import {0}\r\nUnable to match Warehouse for Organization", GetDocketType()), () => reader.ReadIntoBusinessObject(), assertStartsWith: true);
		}

		#endregion

		// Customs Import tests - Amendments

		#region AssertNewDocketAndCancellingOutOfPreviousDocketFromCustomsAmendment

		protected TDocket AssertNewDocketAndCancellingOutOfPreviousDocketFromCustomsAmendment(UniversalShipment dataObject, OrgSupplierPart expectedProduct, int originalCustomsQty, int amendedCustomsQty, byte expectedDocketSplitNo)
		{
			var importResults = GetImportResultsViaDataContextManager(dataObject);

			// should replace the original job with whatever is on the amendment. original job remains.
			var docket = (TDocket)importResults.Single().GetBizOForTesting(dataObject, new BusinessObjectFactory());
			AssertEquals("B123", docket.WD_ExternalReference);
			AssertEquals("B123-EDIDATEDI", docket.WD_CustomsParentReference);
			AssertEquals(expectedDocketSplitNo, docket.WD_ExternalReferenceSplit);
			AssertEquals(true, docket.IsFinalised);

			var lineForAmendedStock = docket.Lines.Single();
			var newStockAmount = GetExpectedDocketNewLineUnits(amendedCustomsQty);
			CombineAssertions(() =>
			{
				AssertEquals("Replacement Docket Line Units.", newStockAmount, lineForAmendedStock.WE_TransactionQuantity);
				AssertEquals("Replacement Docket Line Product.", expectedProduct.PK, lineForAmendedStock.WE_OP);
				AssertEquals("Replacement Docket Line WE_BondedEntryKey.", "ENTRYNUMBER123-2", lineForAmendedStock.WE_BondedEntryKey);
				AssertEquals("Replacement Docket Line CustomsData.WB_EntryKey.", ExpectedCustomsEntryKey, lineForAmendedStock.CustomsData.WB_EntryKey);
				AssertEquals("Replacement Docket Line CustomsData.WB_EntryLineNo.", ExpectedCustomsEntryLineNo, lineForAmendedStock.CustomsData.WB_EntryLineNo);
			});

			if (expectedDocketSplitNo > 0)
			{
				// should cancel out everything on the original job
				AssertPreviousJobInVirtualWhsWasCancelledOut(expectedProduct, originalCustomsQty, amendedCustomsQty, expectedDocketSplitNo);
			}

			return docket;
		}

		protected void AssertInventoryWasAmendedBackToOriginalQuantities(ZString originalReceiptReference, ZDecimal originalReceiveQty, ZDecimal qtyToAmendInventoryOnReceiveBackTo)
		{
			var query = new ZQuery();
			query.AddToFilter(WhsInventoryViewSchema.WI_BondedEntryKey, "ENTRYNUMBER123-2");

			var inventory = new BusinessObjectFactory().Load<WhsInventoryView>(query).Single();
			AssertEquals(originalReceiveQty, inventory.WI_InDocketLineUnits);
			AssertEquals(qtyToAmendInventoryOnReceiveBackTo, inventory.WI_TotalUnits);
			AssertEquals(originalReceiptReference, inventory.InDocketLine.ReceiptReference);
		}

		protected void AssertOrderWasCancelledOutInVirtualWhs(WhsOrder order)
		{
			AssertEquals(true, order.IsCancelled);
			AssertEquals(false, order.IsFinalised);
			AssertNull(order.Pick);
			AssertEquals(0, order.Lines.Cast<WhsOrderLine>().SelectMany(l => l.PickLines).Count());
		}

		protected abstract ZDecimal GetExpectedDocketNewLineUnits(int customsAmendedAmount);
		protected abstract void AssertPreviousJobInVirtualWhsWasCancelledOut(OrgSupplierPart expectedProduct, int originalCustomsQty, int amendedCustomsQty, byte expectedDocketSplitNo);
		protected abstract void AssertDocketWasCancelledOut(TDocket docketThatWasCancelledOut, ZDecimal amendedValue);

		#endregion

		#region Test_CustomsSource_ImportOfFinalisedJobInRealWhsRejectsTheImport

		public void Test_CustomsSource_ImportOfFinalisedJobInRealWhsRejectsTheImport()
		{
			Data.SetupProductsAndShipmentDataObjectForCustomsImportInDB(isWarehouseCreatedAsVirtual: false);

			// create an existing job originating from Customs universal
			var warehouse = Data.GetOrCreateWarehouseInDB(isWarehouseCreatedAsVirtual: false);

			var docket = GetDocketWithLineForCustoms(Data.Orgs.CRAHOLSYD, warehouse, "B123", "B123-EDIDATEDI", Data.Product, 10m, "EntryNumber123", 2, "DummyOutward-1", 102, finalise: true);
			Factory.SaveForTesting();
			AssertIsFinalisedPrecondition(docket);

			// amend qty from 10 to 7
			Data.ShipmentDataObject.CommercialInfo.CommercialInvoiceCollection[0].CommercialInvoiceLineCollection[0].BondedWarehouseQuantity = 7;
			var importResult = GetImportResultsViaDataContextManager(Data.ShipmentDataObject).Single();
			AssertEquals(false, importResult.WasSuccessful);
			AssertEquals(ErrorMessageForAttemptingAmendingFinalisedDocketInRealWarehouse, importResult.Logs.Where(log => log.Type == LogType.Error).ToStringContents(e => e.Message));

			var docketInOtherFactory = new BusinessObjectFactory().Load<TDocket>(docket.PK);
			AssertEquals("Original quantity should not be changed by this amendment because it is a finalised docket in real warehouse.", 10m, docketInOtherFactory.Lines[0].WE_TransactionQuantity);
		}

		protected abstract ZString ErrorMessageForAttemptingAmendingFinalisedDocketInRealWarehouse { get; }

		#endregion

		#region Test_CustomsSource_ImportOfNonFinalisedJobInRealWhsUpdatesTheJob

		public void Test_CustomsSource_ImportOfNonFinalisedJobInRealWhsUpdatesTheJob()
		{
			Data.SetupProductsAndShipmentDataObjectForCustomsImportInDB(isWarehouseCreatedAsVirtual: false);

			// create an existing job originating from Customs universal
			var importResult1 = GetImportResultsViaDataContextManager(Data.ShipmentDataObject).Single();
			AssertEquals(true, importResult1.WasSuccessful);

			var docket = Factory.LoadTop1<WhsDocket>(new ZQuery(WhsDocketSchema.WD_DocketID, importResult1.DataContextKey));
			AssertEquals("Precondition - Docket should NOT be Finalised.", false, docket.IsFinalised);

			// changing the qty from 10 to 7
			Data.ShipmentDataObject.CommercialInfo.CommercialInvoiceCollection[0].CommercialInvoiceLineCollection[0].BondedWarehouseQuantity = 7;
			var importResult2 = GetImportResultsViaDataContextManager(Data.ShipmentDataObject).Single();
			AssertEquals(true, importResult2.WasSuccessful);

			var docketInOtherFactory = new BusinessObjectFactory().Load<TDocket>(docket.PK);
			AssertEquals("Original quantity should be changed by this amendment because it is NOT finalised docket in real warehouse.", 7m, docketInOtherFactory.Lines[0].WE_TransactionQuantity);
		}

		#endregion

		#region Test_CustomsSource_ImportOfFinalisedJobInVirtualWhsCreatesAdmendmentEvenWhenFirstJobWasCancelled

		public void Test_CustomsSource_ImportOfFinalisedJobInVirtualWhsCreatesAdmendmentEvenWhenFirstJobWasCancelled()
		{
			Data.SetupProductsAndShipmentDataObjectForCustomsImportInDB(isWarehouseCreatedAsVirtual: true);

			// create an existing order originating from Customs universal
			var warehouse = Data.GetOrCreateWarehouseInDB(isWarehouseCreatedAsVirtual: true);
			var docket = GetDocketWithLineForCustoms(Data.Orgs.CRAHOLSYD, warehouse, "B123", "B123-EDIDATEDI", Data.Product, 10m, "EntryNumber123", 2, "DummyOutward-1", 102, finalise: true);
			docket.Lines[0].WE_SerialNumber = "";
			Factory.SaveForTesting();
			AssertIsFinalisedPrecondition(docket);

			var putawayEngineMock = PutawayEngineManagerForReceiveMockHandler.GetMockPutawayEngineManagerForReceive();

			using (ObjectFactory.Substitute(putawayEngineMock.Object))
			{
				// send customs cancel event
				var eventDataObject = Data.GetEventDataObject(Events.CancelTheWarehouseJob);
				ImportEventViaDataContextManager(eventDataObject);
				Factory.SaveForTesting();

				// ensure cancel success
				AssertDocketWasCancelledOut(new BusinessObjectFactory().Load<TDocket>(docket.PK), 0m);

				// amend qty from 10 to 7 (effectively replacing the previous cancel event)
				Data.ShipmentDataObject.CommercialInfo.CommercialInvoiceCollection[0].CommercialInvoiceLineCollection[0].BondedWarehouseQuantity = 7;
				var newDocket = AssertNewDocketAndCancellingOutOfPreviousDocketFromCustomsAmendment(Data.ShipmentDataObject, Data.Product, 10, 7, 1);

				var docketThatWasCancelledOut = new BusinessObjectFactory().Load<TDocket>(docket.PK);
				AssertEquals("Should not cancel out a cancelled out docket.", 1, docketThatWasCancelledOut.Logs.Find(l => l.SL_SE_NKEvent == Events.CancelledCode).Count());
				AssertNotEquals(docketThatWasCancelledOut.PK, newDocket.PK);
				AssertDocketCancelledByCustomsEventIsNotCancelledAgainWhenAmending(docketThatWasCancelledOut);
			}
		}

		protected abstract void AssertDocketCancelledByCustomsEventIsNotCancelledAgainWhenAmending(TDocket docketThatWasCancelledOut);

		#endregion

		// Customs Import tests - Events

		#region Test_CustomsSource_OnUniversalEventAdded_RealWhs_CancelsDocketWhenCancelEventIsAdded

		public void Test_CustomsSource_OnUniversalEventAdded_RealWhs_CancelsDocketWhenCancelEventIsAdded()
		{
			Data.SetupProductsAndShipmentDataObjectForCustomsImportInDB();
			var warehouse = Data.GetOrCreateWarehouseInDB();
			var docket = GetDocketWithLineForCustoms(Data.Orgs.CRAHOLSYD, warehouse, "B123", "B123-EDIDATEDI", Data.Product, 10m, "EntryNumber123", 2, "DummyOutward-1", 102);
			SetupDocketToTestCancel(docket);
			Factory.SaveForTesting();

			var eventDataObject = Data.GetEventDataObject(Events.WarehouseJobCanNowBeFinalised);
			ImportEventViaDataContextManager(eventDataObject);
			Factory.SaveForTesting();
			AssertNotNull(docket.Logs.Find(l => l.SL_SE_NKEvent == Events.WarehouseJobCanNowBeFinalisedCode).SingleOrDefault());
			AssertEquals(false, docket.IsCancelled);

			eventDataObject.EventType = Events.CancelTheWarehouseJobCode;
			ImportEventViaDataContextManager(eventDataObject);
			Factory.SaveForTesting();

			// cancelling the job as a result of a customs cancel event occurs in a separate factory, so load from db.
			var docketInOtherFactory = new BusinessObjectFactory().Load<TDocket>(docket.PK);
			AssertNotNull(docketInOtherFactory.Logs.Find(l => l.SL_SE_NKEvent == Events.CancelTheWarehouseJobCode).SingleOrDefault());
			AssertNotNull(docketInOtherFactory.Logs.Find(l => l.SL_SE_NKEvent == Events.CancelledCode).SingleOrDefault());
			AssertEquals("Cancel Event should have cancelled Docket.", true, docketInOtherFactory.IsCancelled);
			TestDocketIsCancelledInRealWhs(docketInOtherFactory);
		}

		protected virtual void SetupDocketToTestCancel(TDocket docket)
		{
		}

		protected abstract void TestDocketIsCancelledInRealWhs(TDocket docket);

		protected void ImportEventViaDataContextManager(UniversalEvent eventDataObject)
		{
			var message = GetQueuedUniversalEventMessage(eventDataObject);
			var manager = new UniversalMessageProcessingManager(new ServiceTaskLogForTesting());
			manager.Process(message);
		}

		#endregion

		#region Test_CustomsSource_OnUniversalEventAdded_VirtualWhs_AmendsDocket

		public void Test_CustomsSource_OnUniversalEventAdded_VirtualWhs_AmendsDocket()
		{
			Data.SetupProductsAndShipmentDataObjectForCustomsImportInDB(isWarehouseCreatedAsVirtual: true);
			var warehouse = Data.GetOrCreateWarehouseInDB(isWarehouseCreatedAsVirtual: true);
			var docket = GetDocketWithLineForCustoms(Data.Orgs.CRAHOLSYD, warehouse, "B123", "B123-EDIDATEDI", Data.Product, 10m, "EntryNumber123", 2, "DummyOutward-1", 102, finalise: true);
			docket.Lines[0].WE_SerialNumber = "";
			Factory.SaveForTesting();

			var eventDataObject = Data.GetEventDataObject(Events.WarehouseJobCanNowBeFinalised);
			ImportEventViaDataContextManager(eventDataObject);
			Factory.SaveForTesting();
			// even though Virtual Jobs don't do anything with the Hold or Accept Event, we need to accept
			// the Event onto the Job, otherwise Universal will complain that no Module took the Event.
			AssertNotNull(docket.Logs.Find(l => l.SL_SE_NKEvent == Events.WarehouseJobCanNowBeFinalisedCode).SingleOrDefault());

			eventDataObject.EventType = Events.CancelTheWarehouseJobCode;
			ImportEventViaDataContextManager(eventDataObject);
			Factory.SaveForTesting();

			// cancelling the job as a result of a customs cancel event occurs in a separate factory, so load from db.
			var docketInOtherFactory = new BusinessObjectFactory().Load<TDocket>(docket.PK);
			AssertNotNull(docketInOtherFactory.Logs.Find(l => l.SL_SE_NKEvent == Events.CancelTheWarehouseJobCode).SingleOrDefault());
			AssertNotNull(docketInOtherFactory.Logs.Find(l => l.SL_SE_NKEvent == Events.CancelledCode).SingleOrDefault());
			AssertEquals("Cancel Event should have cancelled Docket.", true, docketInOtherFactory.IsCancelled);
			AssertDocketWasCancelledOut(docketInOtherFactory, 0m);
		}

		#endregion

		//

		#region TestAdditionalServices

		public void TestAdditionalServices()
		{
			Data.CreateClientOrgCRAHOLSYDInDB();
			Data.GetOrCreateWarehouseInDB();

			new OrganisationDataObjectReader(GetNewAddressData_INTHEMSYD(DocAddressType.Contractor), Logger, Factory).GetMatchedOrNewForTesting();

			Factory.SaveForTesting();

			ShipmentDataObject.Order.OrderNumber = "ORDERME";
			ShipmentDataObject.LocalProcessing = new LocalProcessing(DefaultDataObjectWriterStrategy.TestInstance);
			ShipmentDataObject.LocalProcessing.SetAdditionalServiceCollection(() => new DataObjectList<AdditionalService>() { AdditionalServiceDataObjectReaderTest.SetupAdditionalService() });
			Logger.ClearLogs();

			var reader = GetNewReader(ShipmentDataObject, Logger);
			var whsDocketBO = reader.ReadIntoBusinessObject();

			AssertNotNull(whsDocketBO);
			AssertEquals("whsDocketBO.Services.Count", 1, whsDocketBO.Services.Count);

			CombineAssertions(delegate
			{
				var additionalServiceBO = whsDocketBO.Services[0];
				AdditionalServiceDataObjectReaderTest.AssertContents(additionalServiceBO);

				AssertMultilineASCIIEquals("Logger.Logs", string.Format(@"
Information - Matching 'ConsignorDocumentaryAddress':- Matched to 'CRAHOLSYD' by code, address '' (only address).
Information - No matching {0} found, creating new {0}.
Information - Populating {0}...
Information - Matching 'ConsignorDocumentaryAddress':- Matched to 'CRAHOLSYD' by code, address '' (only address).
Information - No matching JobService found, creating new JobService.
Information - Populating JobService...
Information - Matching 'LocalClient':- Matched to 'CRAHOLSYD' by code, address '' (only address).
Information - Matching 'Contractor':- Matched to 'INTHEMSYD' by code, address '' (only address).
Information - Added Warehouse {1} from UniversalShipment.
".Trim(), typeof(TDocket).Name, GetDocketType()), Logger.Logs);
			});
		}

		#endregion

		#region TestWithOrgAddresses

		public void TestWithOrgAddresses()
		{
			Data.CreateClientOrgCRAHOLSYDInDB();
			Data.GetOrCreateWarehouseInDB();

			var newOrgAddressDataObjectTransport = GetNewAddressData_WUFSHIJNB(DocAddressType.TransportCompanyDocumentaryAddress);
			var unknownOrgAddressDataObjectSTP = GetNewAddressData_WUFSHIJNB(DocAddressType.ShipToParty);
			ShipmentDataObject.OrganizationAddressCollection.Add(newOrgAddressDataObjectTransport);
			ShipmentDataObject.OrganizationAddressCollection.Add(unknownOrgAddressDataObjectSTP);

			var reader = GetNewReader(ShipmentDataObject, Logger);
			var docket = reader.ReadIntoBusinessObject();

			AssertNotNull(docket);

			var jobDocAddressBOTransport = docket.DocAddresses.FindByDocAddressType(DocAddressType.TransportCompanyDocumentaryAddress);
			var jobDocAddressBOSTP = docket.DocAddresses.FindByDocAddressType(DocAddressType.ShipToParty);

			AssertEquals("whsDocketBO.DocAddresses.Count", 1, docket.DocAddresses.Cast<JobDocAddress>().Count(j => !j.IsEmpty));

			CombineAssertions(delegate
			{
				AssertJobDocAddressContentMatches_WUFSHIJNB(jobDocAddressBOTransport);
				AssertNull(jobDocAddressBOSTP);
				AssertEquals("jobDocAddressBOTransport.E2_AddressType", "TRA", jobDocAddressBOTransport.E2_AddressType);
				AssertEquals("jobDocAddressBOTransport.E2_AddressOverride", true, jobDocAddressBOTransport.E2_AddressOverride);
				AssertEquals("jobDocAddressBOTransport.E2_OA_Address", MiscOrgAddressPK, jobDocAddressBOTransport.E2_OA_Address);
				AssertMultilineASCIIEquals("Logger.Logs", string.Format(@"
Information - Matching 'ConsignorDocumentaryAddress':- Matched to 'CRAHOLSYD' by code, address '' (only address).
Information - No matching {0} found, creating new {0}.
Information - Populating {0}...
Information - Matching 'ConsignorDocumentaryAddress':- Matched to 'CRAHOLSYD' by code, address '' (only address).
Warning - Matching 'TransportCompanyDocumentaryAddress':- No match found for '[Org. Code: WUFSHIJNB; Company Name: WUFU SHIPPING LINE; Address 1: Level 2, Building G; Address 2: 34 Dock Lane; City: Johannesburg]'.
Warning - Matching 'ShipToParty':- No match found for '[Org. Code: WUFSHIJNB; Company Name: WUFU SHIPPING LINE; Address 1: Level 2, Building G; Address 2: 34 Dock Lane; City: Johannesburg]'.
Warning - Unknown Address Type [ShipToParty] found. Job Document Address not imported.
Information - Added Warehouse {1} from UniversalShipment.
".Trim(), typeof(TDocket).Name, GetDocketType()), Logger.Logs);
			});
		}

		#endregion

		#region TestWithLocalClient

		public void TestWithLocalClient()
		{
			Data.CreateClientOrgCRAHOLSYDInDB();
			Data.GetOrCreateWarehouseInDB();

			new OrganisationDataObjectReader(GetLocalClientAddress(), Logger, Factory).GetMatchedOrNewForTesting();
			Logger.ClearLogs();

			Factory.SaveForTesting();

			ShipmentDataObject.OrganizationAddressCollection.Add(GetLocalClientAddress());

			var reader = GetNewReader(ShipmentDataObject, Logger);
			var whsDocketBO = reader.ReadIntoBusinessObject();
			Factory.SaveAtEndOfImport(Logger);

			AssertNotNull(whsDocketBO);
			CombineAssertions(delegate
			{
				AssertLocalClientAddress(whsDocketBO.JobHeader.LocalChargesAddr);
				AssertMultilineASCIIEquals("Logger.Logs", string.Format(@"
Information - Matching 'ConsignorDocumentaryAddress':- Matched to 'CRAHOLSYD' by code, address '' (only address).
Information - No matching {0} found, creating new {0}.
Information - Populating {0}...
Information - Matching 'ConsignorDocumentaryAddress':- Matched to 'CRAHOLSYD' by code, address '' (only address).
Information - Matching 'LocalClient':- Matched to 'CRAHOLSYD' by code, address '' (only address).
Information - Added Warehouse {1} from UniversalShipment.
Information - Successfully saved Warehouse {1} W00000001.
".Trim(), typeof(TDocket).Name, GetDocketType()), Logger.Logs);
			});
		}

		public void TestWithLocalClient_WithLockedMutex()
		{
			var client = Data.CreateClientOrgCRAHOLSYDInDB();
			var warehouse = Data.GetOrCreateWarehouseInDB();
			new OrganisationDataObjectReader(GetLocalClientAddress(), Logger, Factory).GetMatchedOrNewForTesting();
			Logger.ClearLogs();

			var docketBOToLoad = GetNewDocket(client, warehouse);
			Factory.SaveAtEndOfImport(Logger);
			AssertNotNull(docketBOToLoad);
			AssertNull("Precondition.", docketBOToLoad.JobHeader);

			using (var mutex = JobHeader.GetMutex_ForTestOnly(docketBOToLoad.PK))
			{
				Assert("Precondition", mutex.Lock());

				var dataObject2 = GetNewDocketDataObject(docketBOToLoad);
				dataObject2.OrganizationAddressCollection.Add(GetLocalClientAddress());

				var reader2 = GetNewReader(dataObject2, Logger);
				var ex = AssertExceptionThrown<DataObjectReadFailureException>(() => reader2.ReadIntoBusinessObject());
				AssertEquals(@"Unable to set Local Client due to the following error:
You have created the job W00000001 on another form, but haven't saved it yet.
Please close or save other forms that use job W00000001 to continue.", ex.Message);
			}
		}

		#endregion

		#region TestWithContainers

		public void TestWithContainers()
		{
			Data.CreateClientOrgCRAHOLSYDInDB();
			Data.GetOrCreateWarehouseInDB();

			var containerDataObject = WhsDocketContainerDataObjectReaderTest.SetupContainer();

			ShipmentDataObject.SetContainerCollection(() => new DataObjectList<Container>());
			ShipmentDataObject.ContainerCollection.Add(containerDataObject);

			var reader = GetNewReader(ShipmentDataObject, Logger);
			var whsDocketBO = reader.ReadIntoBusinessObject();

			AssertNotNull(whsDocketBO);
			AssertEquals("whsDocketBO.Containers.Count", 1, whsDocketBO.Containers.Count);

			CombineAssertions(delegate
			{
				var container = whsDocketBO.Containers[0];
				WhsDocketContainerDataObjectReaderTest.AssertContents(container);
				AssertMultilineASCIIEquals("Logger.Logs", string.Format(@"
Information - Matching 'ConsignorDocumentaryAddress':- Matched to 'CRAHOLSYD' by code, address '' (only address).
Information - No matching {0} found, creating new {0}.
Information - Populating {0}...
Information - Matching 'ConsignorDocumentaryAddress':- Matched to 'CRAHOLSYD' by code, address '' (only address).
Information - No matching WhsDocketContainer found, creating new WhsDocketContainer.
Information - Populating WhsDocketContainer...
Information - Successfully loaded matching Container Type.
Information - Added Warehouse {1} from UniversalShipment.
".Trim(), typeof(TDocket).Name, GetDocketType()), Logger.Logs);
			});
		}

		public void TestWithContainers_CompleteCollection()
		{
			var client = Data.CreateClientOrgCRAHOLSYDInDB();
			var warehouse = Data.GetOrCreateWarehouseInDB();

			var docketBOToLoad = GetNewDocket(client, warehouse);
			docketBOToLoad.WD_ExternalReference = "ORDERME1";
			docketBOToLoad.WD_CustomerReference = "CUSTOMER";
			docketBOToLoad.WD_TransportReference = "TRANS";
			docketBOToLoad.WD_TotalUnits = 10m;

			var containerBOToLoad = Factory.New<WhsDocketContainer>();
			containerBOToLoad.WC_ContainerNum = "CNT123456";
			containerBOToLoad.WC_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;
			containerBOToLoad.WC_SealNum = "R11111111";
			var containerBOToLoad1 = Factory.New<WhsDocketContainer>();
			containerBOToLoad1.WC_ContainerNum = "CNT654321";
			containerBOToLoad1.WC_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;
			containerBOToLoad1.WC_SealNum = "R2222222";

			docketBOToLoad.Containers.Add(containerBOToLoad);
			docketBOToLoad.Containers.Add(containerBOToLoad1);
			Factory.SaveForTesting();

			var containerDataObject = new Container(DefaultDataObjectWriterStrategy.TestInstance);
			containerDataObject.ContainerNumber = "CNT123456";
			containerDataObject.Seal = "s1111";
			containerDataObject.ContainerType = new ContainerType { Code = "20GP", Description = "Twenty foot general purpose", ISOCode = "22G0" };

			var containerDataObject1 = new Container(DefaultDataObjectWriterStrategy.TestInstance);
			containerDataObject1.ContainerNumber = "CNT666666";
			containerDataObject1.Seal = "s2222";
			containerDataObject1.ContainerType = new ContainerType { Code = "20GP", Description = "Twenty foot general purpose", ISOCode = "22G0" };

			ShipmentDataObject.SetContainerCollection(() => new DataObjectList<Container>());
			ShipmentDataObject.ContainerCollection.Content = CollectionContent.Complete;
			ShipmentDataObject.ContainerCollection.Add(containerDataObject);
			ShipmentDataObject.ContainerCollection.Add(containerDataObject1);

			ShipmentDataObject.Order.ClientReference = "CUSTOMER";
			ShipmentDataObject.Order.TransportReference = "TRANS";
			ShipmentDataObject.Order.TotalUnits = 11m;

			var reader = GetNewReader(ShipmentDataObject, Logger);
			var whsDocketBO = reader.ReadIntoBusinessObject();

			AssertNotNull(whsDocketBO);

			CombineAssertions(delegate
			{
				AssertEquals("whsDocketBO.Containers.Count", 2, whsDocketBO.Containers.Count);

				var containerBO1 = whsDocketBO.Containers[0];
				AssertEquals("CNT123456", containerBO1.WC_ContainerNum);
				AssertEquals("s1111", containerBO1.WC_SealNum);

				var containerBO2 = whsDocketBO.Containers[1];
				AssertEquals("CNT666666", containerBO2.WC_ContainerNum);
				AssertEquals("s2222", containerBO2.WC_SealNum);
			});
		}

		public void TestWithContainers_PartialCollection()
		{
			var client = Data.CreateClientOrgCRAHOLSYDInDB();
			var warehouse = Data.GetOrCreateWarehouseInDB();

			var docketBOToLoad = GetNewDocket(client, warehouse);
			docketBOToLoad.WD_ExternalReference = "ORDERME1";
			docketBOToLoad.WD_CustomerReference = "CUSTOMER";
			docketBOToLoad.WD_TransportReference = "TRANS";
			docketBOToLoad.WD_TotalUnits = 10m;

			var containerBOToLoad = Factory.New<WhsDocketContainer>();
			containerBOToLoad.WC_ContainerNum = "CNT123456";
			containerBOToLoad.WC_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;
			containerBOToLoad.WC_SealNum = "R11111111";
			var containerBOToLoad1 = Factory.New<WhsDocketContainer>();
			containerBOToLoad1.WC_ContainerNum = "CNT654321";
			containerBOToLoad1.WC_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;
			containerBOToLoad1.WC_SealNum = "R2222222";

			docketBOToLoad.Containers.Add(containerBOToLoad);
			docketBOToLoad.Containers.Add(containerBOToLoad1);
			Factory.SaveForTesting();

			var containerDataObject = new Container(DefaultDataObjectWriterStrategy.TestInstance);
			containerDataObject.ContainerNumber = "CNT123456";
			containerDataObject.Seal = "s1111";
			containerDataObject.ContainerType = new ContainerType { Code = "20GP", Description = "Twenty foot general purpose", ISOCode = "22G0" };

			var containerDataObject1 = new Container(DefaultDataObjectWriterStrategy.TestInstance);
			containerDataObject1.ContainerNumber = "CNT666666";
			containerDataObject1.Seal = "s2222";
			containerDataObject1.ContainerType = new ContainerType { Code = "20GP", Description = "Twenty foot general purpose", ISOCode = "22G0" };

			ShipmentDataObject.SetContainerCollection(() => new DataObjectList<Container>());
			ShipmentDataObject.ContainerCollection.Content = CollectionContent.Partial;
			ShipmentDataObject.ContainerCollection.Add(containerDataObject);
			ShipmentDataObject.ContainerCollection.Add(containerDataObject1);

			ShipmentDataObject.Order.ClientReference = "CUSTOMER";
			ShipmentDataObject.Order.TransportReference = "TRANS";
			ShipmentDataObject.Order.TotalUnits = 11m;

			var reader = GetNewReader(ShipmentDataObject, Logger);
			var whsDocketBO = reader.ReadIntoBusinessObject();

			AssertNotNull(whsDocketBO);

			CombineAssertions(delegate
			{
				AssertEquals("whsDocketBO.Containers.Count", 3, whsDocketBO.Containers.Count);

				var containerBO1 = whsDocketBO.Containers[0];
				AssertEquals("CNT123456", containerBO1.WC_ContainerNum);
				AssertEquals("s1111", containerBO1.WC_SealNum);

				var containerBO2 = whsDocketBO.Containers[1];
				AssertEquals("CNT654321", containerBO2.WC_ContainerNum);
				AssertEquals("R2222222", containerBO2.WC_SealNum);

				var containerBO3 = whsDocketBO.Containers[2];
				AssertEquals("CNT666666", containerBO3.WC_ContainerNum);
				AssertEquals("s2222", containerBO3.WC_SealNum);
			});
		}

		#endregion

		#region TestCustomsSourceImportFinalisesDocket_RejectsImportIfUnableToFinalise_DPSMatched

		public void TestCustomsSource_ImportFinalisesDocket_RejectsImportIfUnableToFinalise_DPSMatched()
		{
			Globals.IsUserInteractive = false;
			Data.SetupProductsAndShipmentDataObjectForCustomsImportInDB(isWarehouseCreatedAsVirtual: true);

			var putawayEngineMock = PutawayEngineManagerForReceiveMockHandler.GetMockPutawayEngineManagerForReceive();

			Env.Security.DpsAllowUpdateToMatched.IsAllowed = false;
			using (OrganisationsDataRegistry.Instance.DPSFreightMovementRestrictions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "ALL"))
			using (WarehouseDataRegistry.Instance.AllowOrderToBeFinalisedWithoutCustomsClearance.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (ObjectFactory.Substitute(putawayEngineMock.Object))
			using (ObjectFactory.Substitute<IAllocationEngineManager>(new AllocateFIFOLegacyMock()))
			{
				ShipmentDataObject.ScreeningStatus = new UniversalCodeDescriptionPair { Code = ScreeningStatusesList.Codes.Matched, Description = ScreeningStatusesList.Descriptions.Matched };
				// warehouse exists matching address, should find it
				var reader = GetNewReader(ShipmentDataObject, Logger);
				Assert("Precondition", !Globals.CanShowDialogs);
				AssertExceptionThrown("Docket cannot be finalised thus the import should be rejected.",
					typeof(DataObjectReadFailureException),
					ExpectedDPSMatchedExceptionMessage,
					() => reader.ReadIntoBusinessObject());
				Assert("There should be no error message.", UnitTestUserNotification.Instance.LastMessage.WasNone);
			}
		}

		protected abstract string ExpectedDPSMatchedExceptionMessage { get; }

		#endregion

		//

		protected override bool SupportsClientReferenceImport => true;
		protected override bool SupportsTransportReferenceImport => true;

		#region Implementation

		#region GetDocketWithAllocateMock

		internal static TDocket GetDocketWithAllocateMock(TReader reader)
		{
			TDocket result;
			using (ObjectFactory.Substitute<IAllocationEngineManager>(new AllocateFIFOLegacyMock()))
			{
				result = reader.ReadIntoBusinessObject();
			}
			return result;
		}

		#endregion

		#region GetNewDocketDataObject

		protected override UniversalShipment GetNewDocketDataObject(TDocket docket)
		{
			var writer = GetNewWriter(docket);
			var result = writer.GetDataObject(docket);
			result.DataContext.AddDataTarget(((ITopLevelDataObjectWriter)writer).TopLevelDataContextType, docket.WD_DocketID);
			Logger.TopLevelDataObject = result;

			return result;
		}

		protected abstract WhsOrderAndReceiveDataObjectWriter<TDocket> GetNewWriter(TDocket docket);

		#endregion

		#region ChargeLines

		ChargeLine GetChargeLine(ZString? branchCode, ZString? chargeCode, ZString? costAPInvoiceNumber, ZDateTime? costDueDate, ZString? costGSTVATID, ZDateTime? costInvoiceDate,
			ZDecimal? costLocalAmount, ZDecimal? costOSAmount, ZString? costOSCurrency, ZDecimal? costOSGSTVATAmount, ZString? creditor, ZString? debtor, ZString? departmentCode,
			ZString? description, ZShort? displaySequence, ZString? sellGSTVATID, ZString? sellInvoiceType, ZDecimal? sellLocalAmount, ZDecimal? sellOSAmount,
			ZString? sellOSCurrency, ZDecimal? sellOSGSTVATAmount)
		{
			ChargeLine chargeLine = new ChargeLine(DefaultDataObjectWriterStrategy.TestInstance);

			if (branchCode.HasValue)
			{
				chargeLine.Branch = new Branch();
				chargeLine.Branch.Code = branchCode;
				chargeLine.Branch.Name = "EDIHQ";
			}

			if (chargeCode.HasValue)
			{
				chargeLine.ChargeCode = new ChargeCode();
				chargeLine.ChargeCode.Code = chargeCode;
				chargeLine.ChargeCode.Description = "International Freight";
			}

			chargeLine.CostAPInvoiceNumber = costAPInvoiceNumber;
			chargeLine.CostDueDate = costDueDate;

			if (costGSTVATID.HasValue)
			{
				chargeLine.CostGSTVATID = new TaxID();
				chargeLine.CostGSTVATID.TaxCode = costGSTVATID;
				chargeLine.CostGSTVATID.Description = "Cost Tax Description";
			}

			chargeLine.CostInvoiceDate = costInvoiceDate;
			chargeLine.CostLocalAmount = costLocalAmount;
			chargeLine.CostOSAmount = costOSAmount;

			if (costOSCurrency.HasValue)
			{
				chargeLine.CostOSCurrency = new Currency();
				chargeLine.CostOSCurrency.Code = costOSCurrency;
				chargeLine.CostOSCurrency.Description = "Cost Currency Description";
			}

			chargeLine.CostOSGSTVATAmount = costOSGSTVATAmount;

			if (creditor.HasValue)
			{
				chargeLine.Creditor = new OrganizationReference();
				chargeLine.Creditor.Key = creditor;
				chargeLine.Creditor.Type = nameof(DataContextType.Organization);
			}

			if (debtor.HasValue)
			{
				chargeLine.Debtor = new OrganizationReference();
				chargeLine.Debtor.Key = debtor;
				chargeLine.Debtor.Type = nameof(DataContextType.Organization);
			}

			if (departmentCode.HasValue)
			{
				chargeLine.Department = new Department();
				chargeLine.Department.Code = departmentCode;
				chargeLine.Department.Name = "Test Department";
			}

			chargeLine.Description = description;
			chargeLine.DisplaySequence = displaySequence;

			if (sellGSTVATID.HasValue)
			{
				chargeLine.SellGSTVATID = new TaxID();
				chargeLine.SellGSTVATID.TaxCode = sellGSTVATID;
				chargeLine.SellGSTVATID.Description = "Sell Tax Description";
			}

			chargeLine.SellInvoiceType = sellInvoiceType;
			chargeLine.SellLocalAmount = sellLocalAmount;
			chargeLine.SellOSAmount = sellOSAmount;

			if (sellOSCurrency.HasValue)
			{
				chargeLine.SellOSCurrency = new Currency();
				chargeLine.SellOSCurrency.Code = sellOSCurrency;
				chargeLine.SellOSCurrency.Description = "Sell Currency Description";
			}

			chargeLine.SellOSGSTVATAmount = sellOSGSTVATAmount;

			return chargeLine;
		}

		void AssertCharge(JobCharge charge, ChargeLine chargeLine)
		{
			if (chargeLine.Branch != null && chargeLine.Branch.Code.HasValue)
			{
				AssertNotNull("charge.Branch should not be null", charge.Branch);
				AssertEquals("Branch should be equal", chargeLine.Branch.Code, charge.Branch.GB_Code);
			}

			if (chargeLine.ChargeCode != null && chargeLine.ChargeCode.Code.HasValue)
			{
				AssertNotNull("charge.ChargeCode should not be null", charge.ChargeCode);
				AssertEquals("ChargeCode should be equal", chargeLine.ChargeCode.Code, charge.ChargeCode.AC_Code);
			}

			if (chargeLine.CostAPInvoiceNumber.HasValue)
			{
				AssertEquals("APInvoiceNum should be equal", chargeLine.CostAPInvoiceNumber, charge.JR_APInvoiceNum);
			}

			if (chargeLine.CostDueDate.HasValue)
			{
				AssertEquals("CostDueDate should be equal", chargeLine.CostDueDate, charge.JR_PaymentDate);
			}

			if (chargeLine.CostGSTVATID != null && chargeLine.CostGSTVATID.TaxCode.HasValue && !charge.JR_AT_CostGSTRate.IsEmpty)
			{
				AssertNotNull("CostGSTRate should not be null", charge.CostGSTRate);
				AssertEquals("CostGSTRate should be equal", chargeLine.CostGSTVATID.TaxCode, charge.CostGSTRate.AT_Code);
			}

			if (chargeLine.CostInvoiceDate.HasValue)
			{
				AssertEquals("CostInvoiceDate should be equal", chargeLine.CostInvoiceDate, charge.JR_APInvoiceDate);
			}

			if (chargeLine.CostLocalAmount.HasValue)
			{
				AssertEquals("Cost Local Amount should be equal", chargeLine.CostLocalAmount, charge.JR_LocalCostAmt);
			}

			if (chargeLine.CostOSAmount.HasValue)
			{
				AssertEquals("Cost Local Amount should be equal", chargeLine.CostOSAmount, charge.JR_OSCostAmt);
			}

			if (chargeLine.CostOSCurrency != null && chargeLine.CostOSCurrency.Code.HasValue)
			{
				AssertEquals("Cost Currency should be equal", chargeLine.CostOSCurrency.Code, charge.JR_RX_NKCostCurrency);
			}

			if (chargeLine.CostOSGSTVATAmount.HasValue && !charge.JR_AT_CostGSTRate.IsEmpty)
			{
				AssertEquals("Cost OS GST VAT Amount should be equal", chargeLine.CostOSGSTVATAmount, charge.JR_OSCostGSTAmt_Calc);
			}

			if (chargeLine.Creditor != null && chargeLine.Creditor.Key.HasValue)
			{
				AssertNotNull("CostAccount should not be null", charge.CostAccount);
				AssertEquals("Creditor should be equal", chargeLine.Creditor.Key, charge.CostAccount.OH_Code);
			}

			if (chargeLine.Debtor != null && chargeLine.Debtor.Key.HasValue)
			{
				AssertNotNull("SellAccount should not be null", charge.SellAccount);
				AssertEquals("Debtor should be equal", chargeLine.Debtor.Key, charge.SellAccount.OH_Code);
			}

			if (chargeLine.Department != null && chargeLine.Department.Code.HasValue)
			{
				AssertNotNull("Department should not be null", charge.Department);
				AssertEquals("Department should be equal", chargeLine.Department.Code, charge.Department.GE_Code);
			}

			if (chargeLine.Description.HasValue)
			{
				AssertEquals("Description should be equal", chargeLine.Description, charge.JR_Desc);
			}

			if (chargeLine.DisplaySequence.HasValue)
			{
				AssertEquals("DisplaySequence should be equal", chargeLine.DisplaySequence, charge.JR_DisplaySequence);
			}

			if (chargeLine.SellGSTVATID != null && chargeLine.SellGSTVATID.TaxCode.HasValue && !charge.JR_AT_SellGSTRate.IsEmpty)
			{
				AssertNotNull("SellGSTRate should not be null", charge.SellGSTRate);
				AssertEquals("SellGSTRate should be equal", chargeLine.SellGSTVATID.TaxCode, charge.SellGSTRate.AT_Code);
			}

			if (chargeLine.SellInvoiceType.HasValue)
			{
				AssertEquals("InvoiceType should be equal", chargeLine.SellInvoiceType, charge.JR_InvoiceType);
			}

			if (chargeLine.SellLocalAmount.HasValue)
			{
				AssertEquals("Sell Local Amount should be equal", chargeLine.SellLocalAmount, charge.JR_LocalSellAmt);
			}

			if (chargeLine.SellOSAmount.HasValue)
			{
				AssertEquals("Sell OS Amount should be equal", chargeLine.SellOSAmount, charge.JR_OSSellAmt);
			}

			if (chargeLine.SellOSCurrency != null && chargeLine.SellOSCurrency.Code.HasValue)
			{
				AssertEquals("Sell OS Currency should be equal", chargeLine.SellOSCurrency.Code, charge.JR_RX_NKSellCurrency);
			}

			if (chargeLine.SellOSGSTVATAmount.HasValue && !charge.JR_AT_SellGSTRate.IsEmpty)
			{
				AssertEquals("SellOSGSTVAT Amount should be equal", chargeLine.SellOSGSTVATAmount, charge.JR_OSSellGSTAmt_Calc);
			}
		}

		#endregion

		#region PickAndFinaliseInDB

		protected void PickAndFinaliseInDB(WhsOrder order)
		{
			PickOrderCore(order, isFinalisingPick: true);

			order.FinaliseDocketAlwaysFinalisingPick();
			AssertIsFinalisedPrecondition(order);
			AssertIsFinalisedPrecondition(order.Pick);

			Factory.SaveForTesting();
			AssertEquals("Precondition - Pick should be finalised in the DB.", false, order.Pick.HasChanges);
		}

		protected void PickOrder(WhsOrder order)
		{
			PickOrderCore(order, isFinalisingPick: false);
		}

		void PickOrderCore(WhsOrder order, bool isFinalisingPick)
		{
			AssertEquals("We should be finalising the Pick in a Virtual Warehouse.", true, !order.Warehouse.WW_IsVirtualWarehouse || isFinalisingPick);

			var bensDodgyField = Factory.BOFactory.GetType().GetField("saveAllowed", BindingFlags.NonPublic | BindingFlags.Instance);
			bensDodgyField.SetValue(Factory.BOFactory, true);

			Helper.CreatePickNew(order);
			AssertEquals("Precondition", true, order.IsAttachedToPickButNotFinalised);
		}

		#endregion

		#region GetNewDocketWithLine

		TDocket GetNewDocketWithLine(OrgHeader org, WhsWarehouse whs, ZString externalRef, OrgSupplierPart part, ZDecimal qty, bool finalise = false, byte externalRefSplit = 0)
		{
			var docket = GetNewDocketWithLineCore(org, whs, externalRef, part, qty);
			docket.WD_ExternalReferenceSplit = externalRefSplit;
			if (finalise)
			{
				FinaliseDocket(docket);
			}

			Factory.SaveForTesting();

			return docket;
		}

		protected TDocket GetDocketWithLineForCustoms(OrgHeader org, WhsWarehouse whs, ZString externalRef, ZString customsParentReference, OrgSupplierPart part, ZDecimal qty, ZString inwardsBondedEntryKey, ZShort inwardsEntryLineNo, ZString outwardsBondedEntryKey, ZShort outwardsEntryLineNo, bool finalise = false, byte externalRefSplit = 0)
		{
			var docket = GetNewDocketWithLine(org, whs, externalRef, part, qty, externalRefSplit: externalRefSplit);
			docket.WD_CustomsParentReference = customsParentReference;
			docket.WD_DocketSubType = "CUS";
			SetupDocketLineForCustoms((TDocketLine)docket.Lines[0], inwardsBondedEntryKey, inwardsEntryLineNo, outwardsBondedEntryKey, outwardsEntryLineNo);

			if (finalise)
			{
				FinaliseDocket(docket);
			}

			return docket;
		}

		protected abstract void FinaliseDocket(TDocket docket);
		protected abstract TDocket GetNewDocketWithLineCore(OrgHeader org, WhsWarehouse whs, ZString externalRef, OrgSupplierPart part, ZDecimal qty);
		protected abstract void SetupDocketLineForCustoms(TDocketLine line, ZString inwardsBondedEntryKey, ZShort inwardsEntryLineNo, ZString outwardsBondedEntryKey, ZShort outwardsEntryLineNo);
		protected abstract RecipientRoleType GetRecipientRoleType();

		#endregion

		#region CreateDocketWithStatusCode

		protected TDocket CreateDocketWithStatusCode(ZString statusCode)
		{
			var client = Data.CreateClientOrgCRAHOLSYDInDB();
			var whs = Data.GetOrCreateWarehouseInDB();

			var docketBOToLoad = GetNewDocket(client, whs, "2");
			docketBOToLoad.WD_DocketStatus = statusCode;
			docketBOToLoad.WD_DocketID = "W00000002";
			docketBOToLoad.WD_ExternalReference = "ORDER123";
			docketBOToLoad.WD_OH_Client = client.PK;
			docketBOToLoad.WD_TransportReference = "TRANS123";

			return docketBOToLoad;
		}

		#endregion

		#endregion
	}
}
