using System.Linq;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management.Testing;
using Enterprise.Warehouse.GateManagement.Business;
using Enterprise.Warehouse.Integration.CodeLists;
using NUnit.Framework;

namespace Enterprise.Warehouse.GateManagement.DataTransfer.Test
{
	[TestedType(typeof(GteBookingDataObjectWriter))]
	public class GteBookingDataObjectWriterTest : TestCaseWithFactoryAndMessagingHelpers
	{
		public void TestWriteToDataObject()
		{
			#region Setup Test Data

			var transportCompany = Factory.NewWithValidTestData<OrgHeader>();
			transportCompany.OH_Code = "ABC";

			var booking = Factory.NewWithValidTestData<GteBooking>();
			booking.GBK_ReferenceNumber = "BookingReference";
			booking.GBK_OH_TransportCompany = transportCompany.PK;

			var gateMovementBooking1 = Factory.NewWithValidTestData<GteGateMovementBooking>();
			gateMovementBooking1.GBM_GBK_Booking = booking.PK;
			gateMovementBooking1.GBM_BookingReferenceNumber = "gateMovementBooking1";
			gateMovementBooking1.GBM_SlotStartTime = new ZDateTimeOffset(2023, 1, 1, 10, 10, 10);

			var gateMovementBooking2 = Factory.NewWithValidTestData<GteGateMovementBooking>();
			gateMovementBooking2.GBM_GBK_Booking = booking.PK;
			gateMovementBooking2.GBM_BookingReferenceNumber = "gateMovementBooking2";
			gateMovementBooking2.GBM_SlotStartTime = new ZDateTimeOffset(2022, 1, 1, 10, 10, 10);

			#endregion Setup Test Data

			var dataObject = new GteBookingDataObjectWriter(new DataWritingManager(new ActionInfo(null, booking))).GetDataObject(booking);

			CombineAssertions(() =>
			{
				AssertEquals(nameof(DataContextType.GateBooking), dataObject.DataContext.DataSourceCollection.FirstOrDefault().Type);
				AssertEquals("BookingReference", dataObject.DataContext.DataSourceCollection.FirstOrDefault().Key);

				AssertEquals(transportCompany.OH_Code, dataObject.OrganizationAddressCollection.FirstOrDefault(o => o.AddressType.Value == nameof(DocAddressType.TransportCompanyDocumentaryAddress)).OrganizationCode);
				AssertEquals("Expected the earliest GteGateMovementBooking's GBM_SlotStartTime to map to shipment level <SlotDateTime>", new ZDateTime(2022, 1, 1, 10, 10, 10), dataObject.SlotDateTime);
				AssertEquals(2, dataObject.SubShipmentCollection.Count);
			});
		}

		public void TestGivenGteBookingHasBookingPartyAddress_ThenUXMLHasBookingPartyAddress()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.OH_Code = "OH_CCVBSTEST";
			orgHeader.OH_FullName = "Container Chain VBS Test";
			orgHeader.CustomsCodes.AddNew(OrgCusCode.CodeTypes.ContainerChainCommunityCode, "CC_VBS_TEST", string.Empty);

			var booking = Factory.NewWithValidTestData<GteBooking>();
			var bookingPartyJobDocAddress = JobDocAddress.New(booking, DocAddressType.BookingPartyDocumentaryAddress);
			bookingPartyJobDocAddress.E2_OA_Address = orgHeader.MainAddress.PK;
			booking.DocAddresses.Add(bookingPartyJobDocAddress);

			var dataObject = new GteBookingDataObjectWriter(new DataWritingManager(new ActionInfo(null, booking))).GetDataObject(booking);
			var bookingPartyOrganizationAddresses = dataObject.OrganizationAddressCollection.FindAll(o => o.AddressType?.ToString() == nameof(DocAddressType.BookingPartyDocumentaryAddress));
			AssertEquals("Expected only one BookingPartyDocumentaryAddress in UXML",1, bookingPartyOrganizationAddresses.Count);

			var bookingPartyOrganizationAddress = bookingPartyOrganizationAddresses.FirstOrDefault();

			var registrationNumberCollection  = bookingPartyOrganizationAddress.RegistrationNumberCollection;
			AssertEquals("Expected only one RegistrationNumber for the BookingPartyDocumentaryAddress in UXML",1, registrationNumberCollection.Count);
			var registrationNumber = registrationNumberCollection.FirstOrDefault();
			AssertEquals("Expected RegistrationNumber Type Code is CC1 in UXML", "CC1", registrationNumber.Type.Code);
			AssertEquals("Expected RegistrationNumber CC1 Value in UXML is the same as the CC1 value of OrgHeader", "CC_VBS_TEST", registrationNumber.Value);
		}

		public void TestGivenGteBookingHasNoBookingPartyAddress_ThenUXMLHasNoBookingPartyAddress()
		{
			var booking = Factory.NewWithValidTestData<GteBooking>();

			var dataObject = new GteBookingDataObjectWriter(new DataWritingManager(new ActionInfo(null, booking))).GetDataObject(booking);
			var bookingPartyDocumentaryAddresses = dataObject.OrganizationAddressCollection.FindAll(o => o.AddressType?.ToString() == nameof(DocAddressType.BookingPartyDocumentaryAddress));
			AssertEquals("Expected no BookingPartyDocumentaryAddress in UXML",0, bookingPartyDocumentaryAddresses.Count);
		}

		public void TestGivenGteBookingFacilityIsCYD_WhenWritingUXML_ThenOrgAddressIsPopulatedUnderLocalCartageYard()
		{
			var booking = Factory.NewWithValidTestData<GteBooking>();
			booking.Facility.WW_WarehouseType = WarehouseTypes.Codes.ContainerYard;

			var dataObject = new GteBookingDataObjectWriter(new DataWritingManager(new ActionInfo(null, booking))).GetDataObject(booking);
			var addressDataObject = dataObject.OrganizationAddressCollection.FirstOrDefault(a => a.AddressType.Value == nameof(DocAddressType.LocalCartageYard));

			AssertNotNull("Address should be populated under 'LocalCartageYard'", addressDataObject);

			var logger = new DummyLogger();
			var matchedAddress = new GateManagementOrganisationDataObjectReader(addressDataObject, logger, Factory).GetMatched();

			AssertEquals("Address should match the booking facility warehouse address", booking.Facility.WarehouseAddress, matchedAddress);
		}

		public void TestGivenGteBookingFacilityIsTWH_WhenWritingUXML_ThenOrgAddressIsPopulatedTwiceUnderDepArrCFSAddress()
		{
			var booking = Factory.NewWithValidTestData<GteBooking>();
			booking.Facility.WW_WarehouseType = WarehouseTypes.Codes.Transit;

			var dataObject = new GteBookingDataObjectWriter(new DataWritingManager(new ActionInfo(null, booking))).GetDataObject(booking);
			var arrAddressDataObject = dataObject.OrganizationAddressCollection.FirstOrDefault(a => a.AddressType.Value == nameof(DocAddressType.ArrivalCFSAddress));
			var depAddressDataObject = dataObject.OrganizationAddressCollection.FirstOrDefault(a => a.AddressType.Value == nameof(DocAddressType.DepartureCFSAddress));

			CombineAssertions("Address should be populated under both 'ArrivalCFSAddress' and 'DepartureCFSAddress'", () =>
			{
				AssertNotNull("ArrivalCFSAddress", arrAddressDataObject);
				AssertNotNull("DepartureCFSAddress", depAddressDataObject);
			});

			var logger = new DummyLogger();
			var matchedArrAddress = new GateManagementOrganisationDataObjectReader(arrAddressDataObject, logger, Factory).GetMatched();
			var matchedDepAddress = new GateManagementOrganisationDataObjectReader(depAddressDataObject, logger, Factory).GetMatched();

			CombineAssertions("Both addresses should be identical and match the booking facility warehouse address", () =>
			{
				AssertEquals("ArrivalCFSAddress", booking.Facility.WarehouseAddress, matchedArrAddress);
				AssertEquals("DepartureCFSAddress", booking.Facility.WarehouseAddress, matchedDepAddress);
			});
		}
	}
}
