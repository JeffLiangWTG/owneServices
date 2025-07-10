using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Extensions;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.DataTransfer.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.Warehouse.Integration;
using Enterprise.Warehouse.Transit.Business;
using Enterprise.Warehouse.Transit.Business.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using Moq;
using static Enterprise.Core.Constants;

namespace Enterprise.Warehouse.Transit.DataTransfer.Universal.Testing
{
	class WhsItemDispatchTransportationUnitDataObjectReaderForTruckTest : TransitUniversalTestCase
	{
		#region TestPopulateBizO_CreateNewHeader

		public void TestPopulateBizO_CreateNewHeader()
		{
			var warehouse = Data.Warehouse;
			var dispatchLoadList = Helper.CreateDispatchLoadList("DLL1", warehouse.PK, warehouse.DefaultLocation);

			var dataContext = DataContextFactory.New();
			dataContext.AddDataSource(DataContextType.TransportConsignmentRunSheet, "TR1");

			var reader = new WhsItemDispatchTransportationUnitDataObjectReaderForTruck(dispatchLoadList, warehouse, new Shipment { VoyageFlightNo = "V1", DataContext = dataContext }, Logger, Factory);
			var header = reader.ReadIntoBusinessObject();
			Factory.SaveForTesting();

			var pivotQuery = new ZQuery(WhsItemDispatchLoadListDTUPivotSchema.WLD_WDH_TransitDispatchTransportationUnit, header.PK);
			var pivot = Factory.Load<WhsItemDispatchLoadListDTUPivot>(pivotQuery);

			AssertEquals("Header must have point to the dispatch load list passed into the reader",
				dispatchLoadList.GetValue(WhsItemDispatchLoadListSchema.PK), pivot.SingleOrDefault().DispatchLoadList.PK);
			AssertEquals("Header reference should be generated from number fountain.", "TD00000001", header.WDH_ReferenceNumber);
			AssertEquals("Warehouse must be set to the one that passed into the reader.", warehouse.GetValue(WhsWarehouseSchema.PK), header.WDH_WW_Warehouse);
			AssertEquals("Vehicle reference must be the one passed in as VoyageFlightNo.", "V1", header.WDH_VehicleReference);
			AssertEquals("WDH_UnitType of Vehicle is always VEH.", "VEH", header.WDH_UnitType);
			AssertNotNull("There must be a package extension for Vehicle.", header.PackageExtension);
			AssertEquals(PkgUnit.Unit, header.PackageExtension.Package.KP_F3_NKPackType);
			var uldPackageState = Factory.LoadTop1<WhsItemPackageState>(new ZQuery(WhsItemPackageStateSchema.WPS_KP_Package, header.PackageExtension.KPN_KP_Package));
			AssertNull("No Package State must be created for Vehicles.", uldPackageState);
		}

		#endregion

		#region TestPopulateBizo_TestImportTransportCompany

		public void TestPopulateBizo_TestImportTransportCompany()
		{
			var warehouse = Data.Warehouse;
			var dispatchLoadList = Helper.CreateDispatchLoadList("DLL1", warehouse.PK, warehouse.DefaultLocation);

			var dataContext = DataContextFactory.New();
			dataContext.AddDataSource(DataContextType.TransportConsignmentRunSheet, "TR1");

			var org = GetNewAddressData_INTHEMSYD(nameof(DocAddressType.ArrivalCFSLocalTransportAddress));
			org.Port = new UNLOCO() { Code = "AUSYD" };

			var shipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance) { VoyageFlightNo = "V1", DataContext = dataContext };
			shipment.SetOrganizationAddressCollection(() => new List<OrganizationAddress> { org });

			var reader = new WhsItemDispatchTransportationUnitDataObjectReaderForTruck(dispatchLoadList, warehouse, shipment, Logger, Factory);
			var header = reader.ReadIntoBusinessObject();

			var addresses = Factory.Load<JobDocAddress>(new ZQuery(JobDocAddressSchema.E2_ParentID, header.PK));
			var transportDocAddress = addresses.Single(doc => doc.E2_AddressType == DocAddressTypes.Codes.TransportCompanyDocumentaryAddress);

			CombineAssertions("Transport company in DTU should be same as Shipment's ArrivalCFSLocalTransportAddress", () =>
			{
				AssertEquals(org.CompanyName, transportDocAddress.CompanyName);
				AssertEquals(org.Address1, transportDocAddress.Address1);
				AssertEquals(org.Address2, transportDocAddress.Address2);
				AssertEquals(org.Postcode, transportDocAddress.Postcode);
			});
		}

		#endregion

		#region TestPopulateBizO_PopulateBillToParty

		public void TestPopulateBizO_PopulateBillToParty_ArrivalWarehouse()
		{
			var warehouse = Data.Warehouse;

			var dataContext = DataContextFactory.New();
			dataContext.AddDataSource(DataContextType.TransportConsignmentRunSheet, "TR1");
			dataContext.SetWorkflowInfo(new WorkflowInfo { RecipientRoles = new[] { new RecipientRoleDetail { Type = RecipientRoleType.ATW } } });

			var org = GetNewAddressData_WUFSHIJNB(nameof(DocAddressType.ReceivingForwarderAddress));
			org.Port = new UNLOCO() { Code = "AUSYD" };
			org.AddressType = nameof(DocAddressType.ReceivingForwarderAddress);

			var shipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance) { VoyageFlightNo = "V1", DataContext = dataContext };
			shipment.SetOrganizationAddressCollection(() => new List<OrganizationAddress> { org });

			var dispatchLoadList = Helper.CreateDispatchLoadList("DLL1", warehouse.PK);
			var reader = new WhsItemDispatchTransportationUnitDataObjectReaderForTruck(dispatchLoadList, warehouse, shipment, Logger, Factory);
			var header = reader.ReadIntoBusinessObject();
			Factory.SaveForTesting();

			var billToPartyAddress = header.DocAddresses.FindByDocAddressType(DocAddressType.ClientRequestedBillingParty);
			AssertNotNull("Bill To Party in DTU in Arrival Warehouse should be same as Consol's ReceivingForwarderAddress", billToPartyAddress);
			AssertEquals("Bill To Party in DTU in Arrival Warehouse should be same as Consol's ReceivingForwarderAddress", Data.Orgs.WUFSHIJNB.MainAddress.PK, billToPartyAddress.E2_OA_Address);
			Factory.SaveForTesting();

			var newAddressPK = Data.Orgs.INTHEMSYD.MainAddress.PK;
			var overrideOrg = GetNewAddressData_INTHEMSYD(nameof(DocAddressType.ReceivingForwarderAddress));
			shipment.SetOrganizationAddressCollection(() => new List<OrganizationAddress> { overrideOrg });

			header = reader.ReadIntoBusinessObject();
			Factory.SaveForTesting();

			var billToPartyAddress2 = header.DocAddresses.FindByDocAddressType(DocAddressType.ClientRequestedBillingParty);
			AssertNotNull("Bill To Party in DTU in Arrival Warehouse should be same as Consol's ReceivingForwarderAddress", billToPartyAddress2);
			AssertEquals("Bill To Party in DTU in Arrival Warehouse should be overridden", newAddressPK, billToPartyAddress2.E2_OA_Address);
		}

		public void TestPopulateBizO_PopulateBillToParty_DepartureWarehouse()
		{
			var warehouse = Data.Warehouse;

			var dataContext = DataContextFactory.New();
			dataContext.AddDataSource(DataContextType.TransportConsignmentRunSheet, "TR1");
			dataContext.SetWorkflowInfo(new WorkflowInfo { RecipientRoles = new[] { new RecipientRoleDetail { Type = RecipientRoleType.DTW } } });

			var org = GetNewAddressData_WUFSHIJNB(nameof(DocAddressType.SendingForwarderAddress));
			org.Port = new UNLOCO() { Code = "AUSYD" };

			var shipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance) { VoyageFlightNo = "V1", DataContext = dataContext };
			shipment.SetOrganizationAddressCollection(() => new List<OrganizationAddress> { org });

			var dispatchLoadList = Helper.CreateDispatchLoadList("DLL1", warehouse.PK);
			var reader = new WhsItemDispatchTransportationUnitDataObjectReaderForTruck(dispatchLoadList, warehouse, shipment, Logger, Factory);
			var header = reader.ReadIntoBusinessObject();
			Factory.SaveForTesting();

			var billToPartyAddress = header.DocAddresses.FindByDocAddressType(DocAddressType.ClientRequestedBillingParty);
			AssertNotNull("Bill To Party in DTU in Departure Warehouse should be same as Consol's SendingForwarderAddress", billToPartyAddress);
			AssertEquals("Bill To Party in DTU in Departure Warehouse should be same as Consol's SendingForwarderAddress", Data.Orgs.WUFSHIJNB.MainAddress.PK, billToPartyAddress.E2_OA_Address);
			Factory.SaveForTesting();

			var newAddressPK = Data.Orgs.INTHEMSYD.MainAddress.PK;
			var overrideOrg = GetNewAddressData_INTHEMSYD(nameof(DocAddressType.SendingForwarderAddress));
			shipment.SetOrganizationAddressCollection(() => new List<OrganizationAddress> { overrideOrg });

			header = reader.ReadIntoBusinessObject();
			Factory.SaveForTesting();
			var billToPartyAddress2 = header.DocAddresses.FindByDocAddressType(DocAddressType.ClientRequestedBillingParty);
			AssertNotNull("Bill To Party in DTU in Departure Warehouse should be same as Consol's SendingForwarderAddress", billToPartyAddress2);
			AssertEquals("Bill To Party in DTU in Departure Warehouse should be overridden", newAddressPK, billToPartyAddress2.E2_OA_Address);
		}

		#endregion

		#region TestPopulateBizO_LinkMatchingHeader

		public void TestPopulateBizO_LinkMatchingHeader()
		{
			var businessObjectFactory = Factory.BOFactory;
			var referenceHelper = new WhsTransitAdditionalReferencesHelper(Logger, Factory);
			var warehouse = Data.Warehouse;
			var differentWarehouse = Data.WarehouseINTHEMSYD;
			var matchingHeader = CreateDispatchTransportationUnit(referenceHelper, warehouse, "1", "V2", "TR1");
			var alreadyLoadedCompletedHeader = CreateDispatchTransportationUnit(referenceHelper, warehouse, "2", "V1", "TR1", true);
			var headerForDifferentTransportReference = CreateDispatchTransportationUnit(referenceHelper, warehouse, "4", "V2", "TR2");
			var headerForDifferentWarehouse = CreateDispatchTransportationUnit(referenceHelper, differentWarehouse, "5", "V1", "TR1");

			Factory.SaveForTesting();

			var dispatchLoadList = Factory.NewWithValidTestData<WhsItemDispatchLoadList>();
			var dataContext = DataContextFactory.New();
			dataContext.AddDataSource(DataContextType.TransportConsignmentRunSheet, "TR1");
			var reader = new WhsItemDispatchTransportationUnitDataObjectReaderForTruck(dispatchLoadList, warehouse, new Shipment { VoyageFlightNo = "V3", DataContext = dataContext }, Logger, Factory);
			var header = reader.ReadIntoBusinessObject();

			Factory.SaveForTesting();

			AssertEquals("Existing header must be matched.", matchingHeader.PK, header.PK);
			AssertEquals("Reference Numberr cannot be overridden", "1", header.WDH_ReferenceNumber);
			AssertEquals("Matching header must not be load completed.", true, header.WDH_FinalisedTime.IsEmpty);
			AssertEquals("Header should be related to two dispatch load lists", 2, header.DispatchLoadLists.Count);
			AssertNotNull("Header should be related to new ispatch load list", header.DispatchLoadLists.Where(t => t.PK == dispatchLoadList.GetValue(WhsItemDispatchLoadListSchema.PK)));

			AssertEquals("Old warehouse must remain.", warehouse.PK, header.WDH_WW_Warehouse);
			Factory.SaveForTesting();

			var newWarehouse = DataObjectReader.GetColumnIndexerFromRow(Factory.RowFactory.New(WhsWarehouseSchema.Constants.TableName));
			var readerForNewWarehouse = new WhsItemDispatchTransportationUnitDataObjectReaderForTruck(dispatchLoadList, newWarehouse, new Shipment { VoyageFlightNo = "V3", DataContext = dataContext }, Logger, Factory);
			var newHeader = readerForNewWarehouse.ReadIntoBusinessObject();
			AssertNotEquals("Should create a new header since the warehouse is different.", matchingHeader.PK, newHeader.PK);

			var additionalReferences = Factory.BOFactory.Load<CusEntryNumber>(new ZQuery(CusEntryNumSchema.CE_ParentID, matchingHeader.PK)).ToArray();
			AssertEquals("Only 1 Additional reference should be populated.", 1, additionalReferences.Length);
			var additionalReferenceNumber = additionalReferences.Single();
			AssertEquals("Additional reference number should be Run Sheet Number.", WarehouseAdditionalReferenceTypes.Codes.RunSheetNumber, additionalReferenceNumber.CE_EntryType);
			AssertEquals("Run Sheet number must be populated as additional reference.", "TR1", additionalReferenceNumber.CE_EntryNum);
		}

		public void TestPopulateBizO_DoesNotLinkLoadCompletedHeader()
		{
			var referenceHelper = new WhsTransitAdditionalReferencesHelper(Logger, Factory);
			var warehouse = Data.Warehouse;
			var loadCompleteHeader = CreateDispatchTransportationUnit(referenceHelper, warehouse, "1", "V1", "TR1", true);

			Factory.SaveForTesting();

			var loadList = Factory.NewWithValidTestData<WhsItemDispatchLoadList>();
			var dataContext = DataContextFactory.New();
			dataContext.AddDataSource(DataContextType.TransportConsignmentRunSheet, "TR1");
			var reader = new WhsItemDispatchTransportationUnitDataObjectReaderForTruck(loadList, warehouse, new Shipment { VoyageFlightNo = "V3", DataContext = dataContext }, Logger, Factory);
			var header = reader.ReadIntoBusinessObject();
			Factory.SaveForTesting();
			AssertNotEquals("Existing header did not match because it is Load Complete, new one is created instead.", loadCompleteHeader.PK, header.PK);
		}

		WhsItemDispatchTransportationUnit CreateDispatchTransportationUnit(WhsTransitAdditionalReferencesHelper referenceHelper, IWhsWarehouse warehouse,
			string referenceNumber, string vehicleNumber, string runSheetNumber, bool isLoadComplete = false)
		{
			var itemDispatchLoadList = Factory.BOFactory.New<WhsItemDispatchLoadList>();
			itemDispatchLoadList.WDL_JobID = referenceNumber;
			itemDispatchLoadList.WDL_ReferenceNumber = referenceNumber;
			itemDispatchLoadList.WDL_WW_Warehouse = warehouse.PK;
			var header = Factory.New<WhsItemDispatchTransportationUnit>();
			var dateTimeNow = DateTimeOffset.Now;
			var dateTimeOffset = new DateTimeOffset(dateTimeNow.Year, dateTimeNow.Month, dateTimeNow.Day, dateTimeNow.Hour, dateTimeNow.Minute, 0, 0, dateTimeNow.Offset);
			var completeTime = isLoadComplete ? dateTimeOffset : ZDateTimeOffset.Empty;
			header.WDH_GateInTime = completeTime;
			header.WDH_LoadCompleteTime = completeTime;
			header.WDH_GateOutTime = completeTime;
			header.WDH_FinalisedTime = completeTime;
			header.WDH_WW_Warehouse = warehouse.PK;
			header.WDH_ReferenceNumber = referenceNumber;
			header.WDH_VehicleReference = vehicleNumber;

			var additionalReferences = new List<TransitAdditionalReferenceInfo>();
			referenceHelper.CollectTransitAdditionalReferenceInfo(additionalReferences, WarehouseAdditionalReferenceTypes.Codes.RunSheetNumber, runSheetNumber);
			new TransitWarehouseCusEntryNumReferenceCollectionReader(additionalReferences.ToArray(), header, Logger, Factory).ReadIntoCollectionRetainingUnmatchedElements();

			var pivot = Factory.NewWithValidTestData<WhsItemDispatchLoadListDTUPivot>();
			pivot.WLD_WDH_TransitDispatchTransportationUnit = header.PK;
			pivot.WLD_WDL_TransitDispatchLoadList = itemDispatchLoadList.PK;

			return header;
		}

		#endregion

		#region PackingGroupAndWarehouseFromConsolNotNull

		public void PackingGroupAndWarehouseFromConsolNotNull()
		{
			AssertExceptionThrown<ArgumentNullException>("Load List must not be null.", () => new WhsItemDispatchTransportationUnitDataObjectReaderForTruck(null, new Mock<IColumnIndexer>().Object, new Shipment(DefaultDataObjectWriterStrategy.TestInstance), Logger, Factory));
			AssertExceptionThrown<ArgumentNullException>("Warehouse must not be null.", () => new WhsItemDispatchTransportationUnitDataObjectReaderForTruck(new Mock<IColumnIndexer>().Object, null, new Shipment(DefaultDataObjectWriterStrategy.TestInstance), Logger, Factory));
		}

		#endregion

		#region TestPopulateBizO_CreateNewHeaderForVehicle

		public void TestPopulateBizO_CreateNewHeaderForVehicle()
		{
			var warehouse = Data.Warehouse;
			var dispatchLoadList = Helper.CreateDispatchLoadList("DLL1", warehouse.PK, warehouse.DefaultLocation);

			var dataContext = DataContextFactory.New();
			var shipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				VoyageFlightNo = "V1",
				WayBillNumber = "WAYBILL01",
				DataContext = dataContext,
				BookingConfirmationReference = "WAYBILL01"
			};
			shipment = Data.SetupNewDataContextWithDataSource(shipment, gateBookingNumber: "GTB001", isArrival: true);
			shipment = Data.SetupHeaderObjectForGateBooking(shipment, vehicleRegistrationNumber: "ABC-123", driverName: "DRIVER1");
			var subShipment = Data.CreateSubShipmentForGateBookingHeaderObject("GMB001", shipment.WayBillNumber.Value, true);
			shipment.SetSubShipmentCollection(() => new DataObjectList<Shipment>() { subShipment });

			var reader = new WhsItemDispatchTransportationUnitDataObjectReaderForTruck(dispatchLoadList, warehouse, shipment, Logger, Factory);
			var header = reader.ReadIntoBusinessObject();

			Factory.SaveForTesting();

			var pivotQuery = new ZQuery(WhsItemDispatchLoadListDTUPivotSchema.WLD_WDH_TransitDispatchTransportationUnit, header.PK);
			var pivot = Factory.Load<WhsItemDispatchLoadListDTUPivot>(pivotQuery);
			AssertEquals("Header must have point to the dispatch load list passed into the reader",
				dispatchLoadList.GetValue(WhsItemDispatchLoadListSchema.PK), pivot.SingleOrDefault().DispatchLoadList.PK);
			AssertEquals("Header reference should be generated from number fountain.", "TD00000001", header.WDH_ReferenceNumber);
			AssertEquals("Warehouse must be set to the one that passed into the reader.", warehouse.GetValue(WhsWarehouseSchema.PK), header.WDH_WW_Warehouse);
			AssertEquals("Vehicle reference must be Vehicle Registration Number.", "ABC-123", header.WDH_VehicleReference);
			AssertEquals("Signed By must be the Driver name.", "DRIVER1", header.WDH_SignedBy);
			AssertEquals("WDH_UnitType of Vehicle is always VEH.", "VEH", header.WDH_UnitType);
			AssertNotNull("There must be a package extension for Vehicle.", header.PackageExtension);
			AssertEquals(PkgUnit.Unit, header.PackageExtension.Package.KP_F3_NKPackType);
			var vehiclePackageState = Factory.LoadTop1<WhsItemPackageState>(new ZQuery(WhsItemPackageStateSchema.WPS_KP_Package, header.PackageExtension.KPN_KP_Package));
			AssertNull("No Package State must be created for Vehicles.", vehiclePackageState);
			var links = Factory.Load<StmUniversalJobLink>(new ZQuery());
			AssertUniversalJobLink(links, null, header.PK, WhsItemDispatchTransportationUnitSchema.Constants.Prefix, DataContextType.GateBooking, "GTB001", "EDI", "DAT", "EDI");
			AssertUniversalJobLink(links, null, header.PK, WhsItemDispatchTransportationUnitSchema.Constants.Prefix, DataContextType.GateMovementBooking, "GMB001", "EDI", "DAT", "EDI");
			AssertGateRelatedBookingConfirmedLogs(header,
				"TD00000001|FAC=WHS|LOC=Johannesburg|MST=GateBooking|RFN=GTB001|TYP=WhsItemDispatchTransportationUnit|WHS=TWH",
				"TD00000001|FAC=WHS|LOC=Johannesburg|MST=GateMovementBooking|RFN=GMB001|TYP=WhsItemDispatchTransportationUnit|WHS=TWH");
		}

		void AssertGateRelatedBookingConfirmedLogs(WhsItemDispatchTransportationUnit dtu, ZString expectedGateBookingConfirmedLogRef, ZString expectedGateMovementBookingConfirmedLogRef)
		{
			var bookingConfirmedLogQuery = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.BookingConfirmed.Code);
			var allBookingConfirmedLogs = dtu.Logs.Find(bookingConfirmedLogQuery).ToList();
			var gateBookingConfirmedLog = allBookingConfirmedLogs.SingleOrDefault(log => log.SL_Reference.Contains(nameof(DataContextType.GateBooking)));
			var gateMovementBookingConfirmedLog = allBookingConfirmedLogs.SingleOrDefault(log => log.SL_Reference.Contains(nameof(DataContextType.GateMovementBooking)));

			AssertNotNull("Gate Booking Confirmed Log should exist", gateBookingConfirmedLog);
			AssertNotNull("Gate Movement Booking Confirmed Log should exist", gateMovementBookingConfirmedLog);
			AssertEquals(expectedGateBookingConfirmedLogRef, gateBookingConfirmedLog.SL_Reference);
			AssertEquals(expectedGateMovementBookingConfirmedLogRef, gateMovementBookingConfirmedLog.SL_Reference);
		}

		#endregion

		WhsTransitTestHelper Helper => helper ?? (helper = new WhsTransitTestHelper(Factory.BOFactory));
		WhsTransitTestHelper helper;
	}
}
