using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Extensions;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business; 
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Management.Testing;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.GateManagement.Business;
using Enterprise.Warehouse.Integration.CodeLists;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Warehouse.GateManagement.DataTransfer.Test
{
	[TestedType(typeof(FacilityMatchingHelper))]
	class FacilityMatchingHelperTest : TestCaseWithFactoryAndMessagingHelpers
	{
		public void TestGivenUXMLContainsDepOrArrCFSAddress_ThenMatchTWHFacility()
		{
			var booking = Factory.NewWithValidTestData<GteBooking>();
			booking.Facility.WW_WarehouseType = WarehouseTypes.Codes.Transit;

			Factory.SaveForTesting();

			var dataObject = new GteBookingDataObjectWriter(new DataWritingManager(new ActionInfo(null, booking))).GetDataObject(booking);
			var arrAddressDataObject = dataObject.OrganizationAddressCollection.FirstOrDefault(a => a.AddressType.Value == nameof(DocAddressType.ArrivalCFSAddress));
			var depAddressDataObject = dataObject.OrganizationAddressCollection.FirstOrDefault(a => a.AddressType.Value == nameof(DocAddressType.DepartureCFSAddress));

			CombineAssertions("Addres should be populated under addressType: ", () =>
			{
				AssertNotNull("'ArrivalCFSAddress'", arrAddressDataObject);
				AssertNotNull("'DepartureCFSAddress'", depAddressDataObject);
			});

			var logger = new DummyLogger();
			var matchedFacilityRow = FacilityMatchingHelper.GetFacility(dataObject, Factory, logger);
			AssertEquals("Helper should match correct facility", booking.Facility.PK, matchedFacilityRow.GetValue(WhsWarehouseSchema.PK));
		}

		public void TestGivenUXMLContainerLocalCartageYard_ThenMatchCYDFacility()
		{
			var booking = Factory.NewWithValidTestData<GteBooking>();
			booking.Facility.WW_WarehouseType = WarehouseTypes.Codes.ContainerYard;

			Factory.SaveForTesting();

			var dataObject = new GteBookingDataObjectWriter(new DataWritingManager(new ActionInfo(null, booking))).GetDataObject(booking);
			var addressDataObject = dataObject.OrganizationAddressCollection.FirstOrDefault(a => a.AddressType.Value == nameof(DocAddressType.LocalCartageYard));
			AssertNotNull("Addres should be populated under addressType 'LocalCartageYard'", addressDataObject);

			var logger = new DummyLogger();
			var matchedFacilityRow = FacilityMatchingHelper.GetFacility(dataObject, Factory, logger);
			AssertEquals("Helper should match correct facility", booking.Facility.PK, matchedFacilityRow.GetValue(WhsWarehouseSchema.PK));
		}

		public void TestGivenUXMLWithOrganizationAddresses_WhenNoSupportedAddressTypes_ThenThrowException()
		{
			var address = Factory.NewWithValidTestData<OrgAddress>();
			var dataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			var writeManager = new DataWritingManager(new ActionInfo(null, Factory.New<DummyBusinessObject>()));

			Factory.SaveForTesting();

			dataObject.AddOrgAddress(writeManager, address, DocAddressType.TransportCompanyDocumentaryAddress);
			dataObject.AddOrgAddress(writeManager, address, DocAddressType.VanningLocationAddress);

			var logger = new DummyLogger();
			AssertExceptionThrown<DataObjectReadFailureException>(
				"FacilityMatchingHelper only supports 'LocalCartageYard', 'DepartureCFSAddress', and 'ArrivalCFSAddress",
				"No valid address was found when matching for a facility.",
				() =>
			{
				FacilityMatchingHelper.GetFacility(dataObject, Factory, logger);
			});
		}

		public void TestGivenUXMLWithValidOrganizationAddressType_WhenNoMatchingOrgAddress_ThenThrowException()
		{
			var dataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);

			var orgAddressDataObject = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance);
			orgAddressDataObject.AddressType = nameof(DocAddressType.LocalCartageYard);
			orgAddressDataObject.Address1 = "1 main st";
			orgAddressDataObject.City = "Sydney";
			orgAddressDataObject.Postcode = "2020";
			orgAddressDataObject.State = "NSW";
			orgAddressDataObject.Country = new Country();
			orgAddressDataObject.Country.Code = "AU";
			orgAddressDataObject.CompanyName = "Test Organization";
			orgAddressDataObject.Port = new UNLOCO() { Code = "AUSYD" };
			orgAddressDataObject.Email = "123TestAddress@gmail.com";

			dataObject.AddOrgAddress(orgAddressDataObject);

			var logger = new DummyLogger();
			AssertExceptionThrown<DataObjectReadFailureException>(
				"Expected exception thrown when OrganizationAddress does not match a valid address",
				"When matching for a facility via provided address, the address provided was not found.",
				() =>
			{
				FacilityMatchingHelper.GetFacility(dataObject, Factory, logger);
			});
		}

		public void TestGivenUXMLAddressIsLocalCartageYard_WhenFacilityIsTWH_ThenThrowException()
		{
			var booking = Factory.NewWithValidTestData<GteBooking>();
			booking.Facility.WW_WarehouseType = WarehouseTypes.Codes.Transit;
			var address = booking.Facility.WarehouseAddress;

			Factory.SaveForTesting();

			var dataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			var writeManager = new DataWritingManager(new ActionInfo(null, Factory.New<DummyBusinessObject>()));
			dataObject.AddOrgAddress(writeManager, address, DocAddressType.LocalCartageYard);

			var logger = new DummyLogger();
			AssertExceptionThrown<DataObjectReadFailureException>(
				"Expected exception thrown when Address type does not match with facility",
				"No CYD facility matched the provided address with address type (LocalCartageYard).",
				() =>
			{
				FacilityMatchingHelper.GetFacility(dataObject, Factory, logger);
			});
		}

		public void TestGivenUXMLAddressIsCFSAddress_WhenFacilityIsCYD_ThenThrowException()
		{
			var booking = Factory.NewWithValidTestData<GteBooking>();
			booking.Facility.WW_WarehouseType = WarehouseTypes.Codes.ContainerYard;
			var address = booking.Facility.WarehouseAddress;

			Factory.SaveForTesting();

			var dataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			var writeManager = new DataWritingManager(new ActionInfo(null, Factory.New<DummyBusinessObject>()));
			dataObject.AddOrgAddress(writeManager, address, DocAddressType.DepartureCFSAddress);

			var logger = new DummyLogger();
			AssertExceptionThrown<DataObjectReadFailureException>(
				"Expected exception thrown when Address type does not match with facility",
				"No TRW facility matched the provided address with address type (DepartureCFSAddress).",
				() =>
			{
				FacilityMatchingHelper.GetFacility(dataObject, Factory, logger);
			});
		}

		public void TestGivenValidUXML_WhenAddressMatchesMultipleActiveFacilities_ThenThrowException()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var warehouse1 = Factory.NewWithValidTestData<WhsWarehouse>();
			warehouse1.WW_OA_WarehouseAddress = orgHeader.MainAddress.PK;
			warehouse1.WW_WarehouseType = WarehouseTypes.Codes.ContainerYard;
			warehouse1.WW_IsActive = true;

			var warehouse2 = Factory.NewWithValidTestData<WhsWarehouse>();
			warehouse2.WW_OA_WarehouseAddress = orgHeader.MainAddress.PK;
			warehouse2.WW_WarehouseType = WarehouseTypes.Codes.ContainerYard;
			warehouse2.WW_IsActive = true;

			Factory.SaveForTesting();

			var dataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			var writeManager = new DataWritingManager(new ActionInfo(null, Factory.New<DummyBusinessObject>()));
			dataObject.AddOrgAddress(writeManager, orgHeader.MainAddress, DocAddressType.LocalCartageYard);

			var logger = new DummyLogger();
			AssertExceptionThrown<DataObjectReadFailureException>(
				"Expected exception thrown when Address type does not match with facility",
				"More than one CYD facility was found via the provided address with address type (LocalCartageYard).",
				() =>
				{
					FacilityMatchingHelper.GetFacility(dataObject, Factory, logger);
				});
		}
	}
}
