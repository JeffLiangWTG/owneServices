using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Extensions;
using CargoWise.Types;
using Enterprise.DataTransfer.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.Warehouse.Transit.Business;
using Enterprise.Warehouse.Transit.Business.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;

namespace Enterprise.Warehouse.Transit.DataTransfer.Universal.Testing
{
	class WhsTransitReceiveTransportationUnitDataObjectReaderForTruckTest : TransitUniversalTestCase
	{
		#region TestPopulateBizO_CreateNewHeaderForVehicle

		public void TestPopulateBizO_CreateNewHeaderForVehicle()
		{
			var warehouse = Data.Warehouse;
			var receiveASN = Helper.CreateReceiveASN("ASN0000001", warehouse.PK);
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
			var vehicle = shipment.GetVehicle();

			var reader = new WhsTransitReceiveTransportationUnitDataObjectReaderForTruck(shipment, Logger, Factory, warehouse, receiveASN, vehicle);
			var header = reader.ReadIntoBusinessObject();
			Factory.SaveForTesting();

			var pivotQuery = new ZQuery(WhsItemReceiveASNRTUPivotSchema.WAR_WRH_TransitReceiveTransportationUnit, header.PK);
			var pivot = Factory.Load<WhsItemReceiveASNRTUPivot>(pivotQuery);
			AssertEquals("Header must have point to the ASN passed into the reader",
				receiveASN.GetValue(WhsItemReceiveASNSchema.PK), pivot.SingleOrDefault().ReceiveASN.PK);
			AssertEquals("Header reference should be generated from number fountain.", "TR00000001", header.WRH_ReferenceNumber);
			AssertEquals("Warehouse must be set to the one that passed into the reader.", warehouse.GetValue(WhsWarehouseSchema.PK), header.WRH_WW_Warehouse);
			AssertEquals("Vehicle reference must be Vehicle Registration Number.", "ABC-123", header.WRH_VehicleReference);
			AssertEquals("Signed By must be the Driver name.", "DRIVER1", header.WRH_SignedBy);
			AssertEquals("WDH_UnitType of Vehicle is always VEH.", "VEH", header.WRH_UnitType);
			AssertNotNull("There must be a package extension for Vehicle.", header.PackageExtension);
			AssertEquals(PkgUnit.Unit, header.PackageExtension.Package.KP_F3_NKPackType);
			var vehiclePackageStates = Factory.Load<WhsItemPackageState>(new ZQuery(WhsItemPackageStateSchema.WPS_KP_Package, header.PackageExtension.KPN_KP_Package));
			AssertEquals("Package State must not be created for the Vehicle.", 0, vehiclePackageStates.Length);

			var links = Factory.Load<StmUniversalJobLink>(new ZQuery());
			AssertUniversalJobLink(links, null, header.PK, WhsItemReceiveTransportationUnitSchema.Constants.Prefix, DataContextType.GateBooking, "GTB001", "EDI", "DAT", "EDI");
			AssertUniversalJobLink(links, null, header.PK, WhsItemReceiveTransportationUnitSchema.Constants.Prefix, DataContextType.GateMovementBooking, "GMB001", "EDI", "DAT", "EDI");
			AssertGateRelatedBookingConfirmedLogs(header,
				"TR00000001|FAC=WHS|LOC=Johannesburg|MST=GateBooking|RFN=GTB001|TYP=WhsItemReceiveTransportationUnit|WHS=TWH",
				"TR00000001|FAC=WHS|LOC=Johannesburg|MST=GateMovementBooking|RFN=GMB001|TYP=WhsItemReceiveTransportationUnit|WHS=TWH");
		}

		void AssertGateRelatedBookingConfirmedLogs(WhsItemReceiveTransportationUnit rtu, ZString expectedGateBookingConfirmedLogRef, ZString expectedGateMovementBookingConfirmedLogRef)
		{
			var bookingConfirmedLogQuery = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.BookingConfirmed.Code);
			var allBookingConfirmedLogs = rtu.Logs.Find(bookingConfirmedLogQuery).ToList();
			var gateBookingConfirmedLog = allBookingConfirmedLogs.SingleOrDefault(log => log.SL_Reference.Contains(nameof(DataContextType.GateBooking)));
			var gateMovementBookingConfirmedLog = allBookingConfirmedLogs.SingleOrDefault(log => log.SL_Reference.Contains(nameof(DataContextType.GateMovementBooking)));

			AssertNotNull("Gate Booking Confirmed Log should exist", gateBookingConfirmedLog);
			AssertNotNull("Gate Movement Booking Confirmed Log should exist", gateMovementBookingConfirmedLog);
			AssertEquals(expectedGateBookingConfirmedLogRef, gateBookingConfirmedLog.SL_Reference);
			AssertEquals(expectedGateMovementBookingConfirmedLogRef, gateMovementBookingConfirmedLog.SL_Reference);
		}

		#endregion

		#region TestPopulateBizO_IsTransportProviderIsKnown

		#region Known address is different from Approved Location

		public void TestPopulateBizO_TransportProviderIsKnown_CertifiedHaulier() =>
			TestPopulateBizO_TransportProviderIsKnown("CH", ZDate.Empty, isKnown: true, "Is known when certificate type is CertifiedHaulier 'CH' and before Year 2027.");
		public void TestPopulateBizO_TransportProviderIsKnown_CertifiedHaulier_Expired() =>
			TestPopulateBizO_TransportProviderIsKnown("CH", ZDate.Today.AddDays(-2), isKnown: false, "Is not known if OrgCountryData has expired.");

		public void TestPopulateBizO_TransportProviderIsKnown_RegulatedAgentACENotice_ApprovedLocation() =>
			TestPopulateBizO_TransportProviderIsKnown("RA", ZDate.Empty, isKnown: true, "Is known when certificate type is RegulatedAgentACENotice 'RA', and Approved Location matches Company address.");

		public void TestPopulateBizO_TransportProviderIsKnown_RegulatedAgentACENotice_ApprovedLocation_Expired() =>
			TestPopulateBizO_TransportProviderIsKnown("RA", ZDate.Today.AddDays(-2), isKnown: false, "Is not known if OrgCountryData has expired.");

		public void TestPopulateBizO_TransportProviderIsKnown_ApprovedHaulier() =>
			TestPopulateBizO_TransportProviderIsKnown("AH", ZDate.Empty, isKnown: true, "Is known when certificate type is ApprovedHaulier 'AH'.");

		public void TestPopulateBizO_TransportProviderIsKnown_ApprovedHaulier_NotExpired() =>
			TestPopulateBizO_TransportProviderIsKnown("AH", ZDate.Today.AddDays(2), isKnown: true, "Is known when OrgCountryData has not expired.");

		public void TestPopulateBizO_TransportProviderIsKnown_ApprovedHaulier_Expired() =>
			TestPopulateBizO_TransportProviderIsKnown("AH", ZDate.Today.AddDays(-2), isKnown: false, "Is not known when OrgCountryData has expired.");

		protected void TestPopulateBizO_TransportProviderIsKnown(string exApprovedOrMajorExporter, ZDate expiredDateOffset, bool isKnown, string reason)
		{
			var checkCertifiedHaulier = DateTime.Now < new DateTime(2027, 01, 01);
			isKnown = isKnown && (!(exApprovedOrMajorExporter == "CH") || checkCertifiedHaulier);

			var warehouse = Data.Warehouse;
			warehouse.WW_TransitSecurityProcessingRequired = true;
			var receiveASN = Helper.CreateReceiveASN("ASN1", warehouse.PK);
			var shipment = Data.HeaderDataObject;
			var dataContext = shipment.DataContext;

			shipment = Data.SetupNewDataContextWithDataSource(shipment, gateBookingNumber: "GTB001");
			shipment = Data.SetupHeaderObjectForGateBooking(shipment, vehicleRegistrationNumber: "ABC-123", driverName: "DRIVER1");
			var subShipment = Data.CreateSubShipmentForGateBookingHeaderObject("GMB001", shipment.WayBillNumber.Value, true);
			shipment.SetSubShipmentCollection(() => new DataObjectList<Shipment>() { subShipment });

			var rtuTransportCompanyAddress = GetNewAddressData_CRAHOLSYD(nameof(DocAddressType.DepartureCFSLocalTransportAddress));
			rtuTransportCompanyAddress.Address1 = "2804 Fudrucker Way";
			rtuTransportCompanyAddress.AddressShortCode = rtuTransportCompanyAddress.Address1;
			shipment.OrganizationAddressCollection.Add(rtuTransportCompanyAddress);

			var crbAddress = GetNewAddressData_CRAHOLSYD(nameof(DocAddressType.ClientRequestedBillingParty));
			crbAddress.Address1 = "3804 Fudrucker Way";
			crbAddress.AddressShortCode = crbAddress.Address1;
			shipment.OrganizationAddressCollection.Add(crbAddress);

			var vehicle = shipment.GetVehicle();

			var org = Data.Orgs.CRAHOLSYD;
			var transportOrgAddress = org.Addresses.AddNew();
			transportOrgAddress.Address1 = rtuTransportCompanyAddress.Address1.Value;
			transportOrgAddress.OA_RN_NKCountryCode = rtuTransportCompanyAddress.Country.Code.Value;
			var crbOrgAddress = org.Addresses.AddNew();
			crbOrgAddress.Address1 = crbAddress.Address1.Value;
			crbOrgAddress.OA_RN_NKCountryCode = crbAddress.Country.Code.Value;

			// transportOrgAddress is the approved location
			var orgCountryData = transportOrgAddress.KnownShipperDetails.AddNew();
			orgCountryData.OV_OH_OrgHeader = org.PK;
			orgCountryData.OV_EXApprovedOrMajorExporter = exApprovedOrMajorExporter;
			orgCountryData.OV_EXApprovalExpiryDate = expiredDateOffset;

			WarehouseDataRegistry.Instance.DriverSecurityCertificationCheckingActivated.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var contact = Data.Orgs.CRAHOLSYD.Contacts.AddNew();
			contact.OC_ContactName = "Test Driver";
			var crew = new Crew() { FullName = contact.OC_ContactName, CrewType = CrewType.Driver };
			shipment.VehicleRun.SetCrewCollection(() => new List<Crew>() { crew });

			var cert1 = contact.Certificates.AddNew();
			cert1.XZ_ExpiryOrDueDate = DateTime.UtcNow.AddDays(2);
			cert1.XZ_Type = "BKG";
			var cert2 = contact.Certificates.AddNew();
			cert2.XZ_ExpiryOrDueDate = DateTime.UtcNow.AddDays(2);
			cert2.XZ_Type = "DTA";

			Factory.SaveForTesting();

			var header = new WhsTransitReceiveTransportationUnitDataObjectReaderForTruck(shipment, Logger, Factory, warehouse, receiveASN, vehicle).ReadIntoBusinessObject();
			var transportCompanyAddress = header.DocAddresses.FindByDocAddressType(DocAddressType.TransportCompanyDocumentaryAddress);
			AssertNotNull(transportCompanyAddress);
			AssertEquals(reason, isKnown, header.WRH_TransportProviderIsKnown);
		}

		#endregion

		#region Org Country Data

		public void TestPopulateBizO_TransportProviderIsKnown_Country_EU_EU() =>
			TestPopulateBizO_TransportProviderIsKnown_EUCountryCode(CountryCodes.EuropeanUnion, CountryCodes.France, isKnown: true, "Is known when Company is an EU country, and Org Country Data is European Union.");

		public void TestPopulateBizO_TransportProviderIsKnown_Country_EU_NonEU() =>
			TestPopulateBizO_TransportProviderIsKnown_EUCountryCode(CountryCodes.EuropeanUnion, CountryCodes.SouthAfrica, isKnown: false, "Is not known when Company is not an EU country, and Org Country Data is European Union.");

		public void TestPopulateBizO_TransportProviderIsKnown_Country_UK() =>
			TestPopulateBizO_TransportProviderIsKnown_EUCountryCode(CountryCodes.UnitedKingdom, CountryCodes.UnitedKingdom, isKnown: true, "Is known when UK Country Codes match.");

		public void TestPopulateBizO_TransportProviderIsKnown_Country_NonEU() =>
			TestPopulateBizO_TransportProviderIsKnown_EUCountryCode(CountryCodes.SouthAfrica, CountryCodes.SouthAfrica, isKnown: true, "Is known when Country Codes match.");

		public void TestPopulateBizO_TransportProviderIsKnown_EUCountryCode(string approvedCountry, string currentCountry, bool isKnown, string message)
		{
			var warehouse = Data.Warehouse;
			warehouse.WW_TransitSecurityProcessingRequired = true;
			GlbCompany.CurrentCompany.SetCountry(currentCountry);

			var receiveASN = Helper.CreateReceiveASN("ASN1", warehouse.PK);
			var shipment = Data.HeaderDataObject;
			var dataContext = shipment.DataContext;

			shipment = Data.SetupNewDataContextWithDataSource(shipment, gateBookingNumber: "GTB001");
			shipment = Data.SetupHeaderObjectForGateBooking(shipment, vehicleRegistrationNumber: "ABC-123", driverName: "DRIVER1");
			var subShipment = Data.CreateSubShipmentForGateBookingHeaderObject("GMB001", shipment.WayBillNumber.Value, true);
			shipment.SetSubShipmentCollection(() => new DataObjectList<Shipment>() { subShipment });

			var orgAddress = Data.Orgs.Warehouse_WUFSHIJNB;
			orgAddress.AddressType = nameof(DocAddressType.DepartureCFSLocalTransportAddress);
			shipment.OrganizationAddressCollection.Add(orgAddress);

			var vehicle = shipment.GetVehicle();

			var org = Data.Orgs.WUFSHIJNB;
			var orgCountryData = org.MainAddress.KnownShipperDetails.AddNew();
			orgCountryData.OV_OH_OrgHeader = org.PK;
			orgCountryData.OV_RN_NKClientCountryRelation = approvedCountry;
			orgCountryData.OV_EXApprovedOrMajorExporter = "AH";

			WarehouseDataRegistry.Instance.DriverSecurityCertificationCheckingActivated.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var contact = Data.Orgs.WUFSHIJNB.Contacts.AddNew();
			contact.OC_ContactName = "Test Driver";
			var crew = new Crew() { FullName = contact.OC_ContactName };
			shipment.VehicleRun.SetCrewCollection(() => new List<Crew>() { crew });

			var cert1 = contact.Certificates.AddNew();
			cert1.XZ_ExpiryOrDueDate = DateTime.UtcNow.AddDays(2);
			cert1.XZ_Type = "BKG";
			var cert2 = contact.Certificates.AddNew();
			cert2.XZ_ExpiryOrDueDate = DateTime.UtcNow.AddDays(2);
			cert2.XZ_Type = "DTA";

			Factory.SaveForTesting();

			var header = new WhsTransitReceiveTransportationUnitDataObjectReaderForTruck(shipment, Logger, Factory, warehouse, receiveASN, vehicle).ReadIntoBusinessObject();
			var transportCompanyAddress = header.DocAddresses.FindByDocAddressType(DocAddressType.TransportCompanyDocumentaryAddress);
			AssertNotNull(transportCompanyAddress);
			AssertEquals(message, isKnown, header.WRH_TransportProviderIsKnown);
		}

		#endregion

		public void TestPopulateBizO_TransportProviderIsKnown_DriverNotCertified()
		{
			var warehouse = Data.Warehouse;
			warehouse.WW_TransitSecurityProcessingRequired = true;
			var receiveASN = Helper.CreateReceiveASN("ASN1", warehouse.PK);
			var shipment = Data.HeaderDataObject;
			var dataContext = shipment.DataContext;

			shipment = Data.SetupNewDataContextWithDataSource(shipment, gateBookingNumber: "GTB001");
			shipment = Data.SetupHeaderObjectForGateBooking(shipment, vehicleRegistrationNumber: "ABC-123", driverName: "DRIVER1");
			var subShipment = Data.CreateSubShipmentForGateBookingHeaderObject("GMB001", shipment.WayBillNumber.Value, true);
			shipment.SetSubShipmentCollection(() => new DataObjectList<Shipment>() { subShipment });

			var orgAddress = Data.Orgs.Warehouse_WUFSHIJNB;
			orgAddress.AddressType = nameof(DocAddressType.DepartureCFSLocalTransportAddress);
			shipment.OrganizationAddressCollection.Add(orgAddress);

			var vehicle = shipment.GetVehicle();

			var org = Data.Orgs.WUFSHIJNB;
			var orgCountryData = org.MainAddress.KnownShipperDetails.AddNew();
			orgCountryData.OV_OH_OrgHeader = org.PK;
			orgCountryData.OV_EXApprovedOrMajorExporter = "AH";

			WarehouseDataRegistry.Instance.DriverSecurityCertificationCheckingActivated.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var contact = Data.Orgs.WUFSHIJNB.Contacts.AddNew();
			contact.OC_ContactName = "Test Driver";
			var crew = new Crew() { FullName = contact.OC_ContactName };
			shipment.VehicleRun.SetCrewCollection(() => new List<Crew>() { crew });

			Factory.SaveForTesting();

			var header = new WhsTransitReceiveTransportationUnitDataObjectReaderForTruck(shipment, Logger, Factory, warehouse, receiveASN, vehicle).ReadIntoBusinessObject();
			var transportCompanyAddress = header.DocAddresses.FindByDocAddressType(DocAddressType.TransportCompanyDocumentaryAddress);
			AssertNotNull(transportCompanyAddress);
			AssertEquals("Is not known when approval expiry date is in the past.", false, header.WRH_TransportProviderIsKnown);
		}

		#endregion

		#region TestPopulateBizO_CannotUpdateTransportCompanyAddress

		public void TestPopulateBizO_CannotUpdateDepartureCFSLocalTransportAddress() =>
			TestPopulateBizO_CannotUpdateTransportCompanyAddress(nameof(DocAddressType.DepartureCFSLocalTransportAddress));

		public void TestPopulateBizO_CannotUpdateArrivalCFSLocalTransportAddress() =>
			TestPopulateBizO_CannotUpdateTransportCompanyAddress(nameof(DocAddressType.ArrivalCFSLocalTransportAddress));

		void TestPopulateBizO_CannotUpdateTransportCompanyAddress(string addressType)
		{
			var warehouse = Data.Warehouse;
			var receiveASN = Helper.CreateReceiveASN("ASN0000001", warehouse.PK);
			var dataContext = DataContextFactory.New();
			var shipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				VoyageFlightNo = "V1",
				WayBillNumber = "WAYBILL01",
				DataContext = dataContext,
				BookingConfirmationReference = "WAYBILL01"
			};

			shipment = Data.SetupNewDataContextWithDataSource(shipment, gateBookingNumber: "GTB001", isArrival: addressType == "ArrivalCFSLocalTransportAddress");
			shipment = Data.SetupHeaderObjectForGateBooking(shipment, vehicleRegistrationNumber: "ABC-123", driverName: "DRIVER1");
			var subShipment = Data.CreateSubShipmentForGateBookingHeaderObject("GMB001", shipment.WayBillNumber.Value, true);
			shipment.SetSubShipmentCollection(() => new DataObjectList<Shipment>() { subShipment });

			var org = addressType == "ArrivalCFSLocalTransportAddress" ? Data.Orgs.Warehouse_INTHEMSYD : Data.Orgs.Warehouse_WUFSHIJNB;
			org.AddressType = addressType;
			shipment.OrganizationAddressCollection.Add(org);

			var vehicle = shipment.GetVehicle();

			var header = new WhsTransitReceiveTransportationUnitDataObjectReaderForTruck(shipment, Logger, Factory, warehouse, receiveASN, vehicle).ReadIntoBusinessObject();
			header.WRH_WL_StagingLocation = warehouse.GetValue(WhsWarehouseSchema.WW_DefaultInboundDockDoor);
			Helper.CreatePackageState(header, 1, "PLT", "PKG-1", TransitWarehouseStatuses.Codes.Arrived);

			Factory.SaveForTesting();

			shipment.OrganizationAddressCollection
				.Where(address => address.AddressType.GetValueOrDefault().ToString() == addressType).FirstOrDefault()
				.Address1 = "New Address";

			AssertExceptionThrown<DataObjectReadFailureException>("Cannot update the Truck Transport Company Address. Some packages have already been unloaded.",
				() => new WhsTransitReceiveTransportationUnitDataObjectReaderForTruck(shipment, Logger, Factory, warehouse, receiveASN, vehicle).ReadIntoBusinessObject());
		}

		#endregion

		#region TestPopulateBizO_CannotUpdateTransportCompanyDriverName

		public void TestPopulateBizO_CannotUpdateTransportCompanyDriverName()
		{
			var warehouse = Data.Warehouse;
			var receiveASN = Helper.CreateReceiveASN("ASN0000001", warehouse.PK);
			var dataContext = DataContextFactory.New();
			var shipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				VoyageFlightNo = "V1",
				WayBillNumber = "WAYBILL01",
				DataContext = dataContext,
				BookingConfirmationReference = "WAYBILL01"
			};

			shipment = Data.SetupNewDataContextWithDataSource(shipment, gateBookingNumber: "GTB001", isArrival: false);
			shipment = Data.SetupHeaderObjectForGateBooking(shipment, vehicleRegistrationNumber: "ABC-123", driverName: "DRIVER1");
			var subShipment = Data.CreateSubShipmentForGateBookingHeaderObject("GMB001", shipment.WayBillNumber.Value, true);
			shipment.SetSubShipmentCollection(() => new DataObjectList<Shipment>() { subShipment });

			var vehicle = shipment.GetVehicle();
			shipment.VehicleRun.CrewCollection.FirstOrDefault(cc => cc.FullName.HasValue).FullName = "Old Driver";

			var header = new WhsTransitReceiveTransportationUnitDataObjectReaderForTruck(shipment, Logger, Factory, warehouse, receiveASN, vehicle).ReadIntoBusinessObject();

			header.WRH_WL_StagingLocation = warehouse.GetValue(WhsWarehouseSchema.WW_DefaultInboundDockDoor);
			Helper.CreatePackageState(header, 1, "PLT", "PKG-1", TransitWarehouseStatuses.Codes.Arrived);

			Factory.SaveForTesting();

			shipment.VehicleRun.CrewCollection.FirstOrDefault(cc => cc.FullName.HasValue).FullName = "New Driver";

			AssertExceptionThrown<DataObjectReadFailureException>("Cannot update the Truck Driver Name. Some packages have already been unloaded.",
				() => new WhsTransitReceiveTransportationUnitDataObjectReaderForTruck(shipment, Logger, Factory, warehouse, receiveASN, vehicle).ReadIntoBusinessObject());
		}

		#endregion

		#region Utils

		WhsTransitTestHelper Helper => helper ?? (helper = new WhsTransitTestHelper(Factory.BOFactory));
		WhsTransitTestHelper helper;

		#endregion
	}
}
