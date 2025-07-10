using System;
using System.Linq;
using CargoWise.Definitions;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Environment.Business.Testing;
using Enterprise.Warehouse.GateManagement.Business;
using Enterprise.Warehouse.Integration.CodeLists;
using NUnit.Framework;

namespace Enterprise.Warehouse.GateManagement.DataTransfer.Test
{
	[TestedType(typeof(GteVehicleMovementDataObjectWriter))]
	public class GteVehicleMovementDataObjectWriterTest : TestCaseWithFactory
	{
		WhsTestHelperFunctionsEnv helper;
		protected WhsTestHelperFunctionsEnv Helper
		{
			get
			{
				helper ??= new WhsTestHelperFunctionsEnv(Factory);
				return helper;
			}
		}

		readonly ZDateTimeOffset EntryTime = new ZDateTimeOffset(new DateTime(2024, 06, 06, 12, 00, 00), DateTimeKind.Local);
		readonly ZDateTimeOffset ExitTime = new ZDateTimeOffset(new DateTime(2024, 06, 06, 15, 00, 00), DateTimeKind.Local);

		public void TestGivenVehicleMovementWithNoVehicleEntries_WhenGenerateUXML_ThenFieldsAreCorrectlySet()
		{
			var vehicleMovement = Factory.NewWithValidTestData<GteVehicleMovement>();
			vehicleMovement.GVM_VehicleRegistration = "REG-123";
			vehicleMovement.GVM_RC_VehicleType = Factory.NewWithValidTestData<RefContainer>().PK;
			vehicleMovement.VehicleType.RC_Code = "RTRK";
			vehicleMovement.GVM_WL_Location = Factory.NewWithValidTestData<WhsLocation>().PK;
			vehicleMovement.WarehouseLocation.Warehouse.WW_WarehouseType = WarehouseTypes.Codes.ContainerYard;
			vehicleMovement.WarehouseLocation.Warehouse.WarehouseAddress.Address1 = "Address 1";
			vehicleMovement.WarehouseLocation.Warehouse.WarehouseAddress.Address2 = "Address 2";
			vehicleMovement.WarehouseLocation.Warehouse.WarehouseAddress.City = "SYD";
			vehicleMovement.WarehouseLocation.Warehouse.WarehouseAddress.Postcode = "0000";
			vehicleMovement.WarehouseLocation.Warehouse.WarehouseAddress.OA_Code = "BLANK SYD";
			vehicleMovement.WarehouseLocation.Warehouse.WarehouseAddress.Header.OH_Code = "ZZ";

			var gateMovement = Factory.NewWithValidTestData<GteGateMovement>();
			gateMovement.GGM_GVM_VehicleMovement = vehicleMovement.PK;

			var gateMovementBooking = gateMovement.GateMovementBooking;
			gateMovementBooking.GBM_SourceReferenceNumber = "VBS-001";

			var booking = gateMovementBooking.Booking;

			var transportCompany = booking.TransportCompany;
			transportCompany.OH_Code = "ABC";

			var shipment = new GteVehicleMovementDataObjectWriter(new DataWritingManager(new ActionInfo(null, vehicleMovement))).GetDataObject(vehicleMovement);
			var address = shipment.OrganizationAddressCollection?.FirstOrDefault(nameof(DocAddressType.LocalCartageYard));
			CombineAssertions(() =>
			{
				AssertEquals("Expected DataContext to be GateVehicleMovement", nameof(DataContextType.GateVehicleMovement), shipment.DataContext.DataSourceCollection.FirstOrDefault().Type);
				AssertEquals("Expected OrganizationCode to match", transportCompany.OH_Code, shipment.OrganizationAddressCollection.FirstOrDefault().OrganizationCode);
				AssertEquals("Expected WarehouseLocation to match", vehicleMovement.WarehouseLocation.ToLocationString(), shipment.WarehouseLocation);
				AssertEquals("Expected Warehouse to match", vehicleMovement.WarehouseLocation.Warehouse.WarehouseAddress.OA_Code, address.AddressShortCode);
				AssertEquals("Expected Warehouse to match", vehicleMovement.WarehouseLocation.Warehouse.WarehouseAddress.Address1, address.Address1);
				AssertEquals("Expected Warehouse to match", vehicleMovement.WarehouseLocation.Warehouse.WarehouseAddress.Address2, address.Address2);
				AssertEquals("Expected Warehouse to match", vehicleMovement.WarehouseLocation.Warehouse.WarehouseAddress.City, address.City);
				AssertEquals("Expected Warehouse to match", vehicleMovement.WarehouseLocation.Warehouse.WarehouseAddress.Postcode, address.Postcode);
				AssertEquals("Expected Warehouse to match", vehicleMovement.WarehouseLocation.Warehouse.WarehouseAddress.Header.OH_Code, address.OrganizationCode);
				AssertEquals("Expected VehicleRegistration to match", vehicleMovement.GVM_VehicleRegistration, shipment.VehicleRun.Vehicle.Registration.Number);
				AssertEquals("Expected VehicleType to match", vehicleMovement.VehicleType.RC_Code, shipment.VehicleRun.Vehicle.VehicleType.Code);
				AssertNull("Expected there to be no DateCollection", shipment.DateCollection);
				AssertNull("Expected there to be no RelatedShipmentCollection", shipment.RelatedShipmentCollection);
				AssertNotNull("Expected there to be a SubShipmentCollection", shipment.SubShipmentCollection);
				AssertEquals("Expected there to be a single SubShipment", 1, shipment.SubShipmentCollection.Count);
				AssertEquals("Expected SubShipment BookingPartyReference to match", gateMovementBooking.GBM_SourceReferenceNumber, shipment.SubShipmentCollection[0].GetAdditionalReferenceOrDefault(AdditionalReferenceTypes.Codes.BookingPartyReference));
			});
		}

		public void TestGivenVehicleMovementWithGateIn_WhenGenerateUXML_ThenFieldsAreCorrectlySet()
		{
			var vehicleMovement = Factory.NewWithValidTestData<GteVehicleMovement>();
			vehicleMovement.GVM_VehicleRegistration = "REG-123";
			vehicleMovement.GVM_RC_VehicleType = Factory.NewWithValidTestData<RefContainer>().PK;
			vehicleMovement.VehicleType.RC_Code = "RTRK";
			vehicleMovement.GVM_WL_Location = Factory.NewWithValidTestData<WhsLocation>().PK;
			vehicleMovement.WarehouseLocation.Warehouse.WW_WarehouseType = WarehouseTypes.Codes.ContainerYard;
			vehicleMovement.WarehouseLocation.Warehouse.WarehouseAddress.Address1 = "Address 1";
			vehicleMovement.WarehouseLocation.Warehouse.WarehouseAddress.Address2 = "Address 2";
			vehicleMovement.WarehouseLocation.Warehouse.WarehouseAddress.City = "SYD";
			vehicleMovement.WarehouseLocation.Warehouse.WarehouseAddress.Postcode = "0000";
			vehicleMovement.WarehouseLocation.Warehouse.WarehouseAddress.OA_Code = "BLANK SYD";
			vehicleMovement.WarehouseLocation.Warehouse.WarehouseAddress.Header.OH_Code = "ZZ";

			var gateMovement = Factory.NewWithValidTestData<GteGateMovement>();
			gateMovement.GGM_GVM_VehicleMovement = vehicleMovement.PK;

			var gateMovementBooking = gateMovement.GateMovementBooking;
			gateMovementBooking.GBM_SourceReferenceNumber = "VBS-001";

			var booking = gateMovementBooking.Booking;
			var transportCompany = booking.TransportCompany;
			transportCompany.OH_Code = "ABC";

			var vehicleEntry = Factory.NewWithValidTestData<GteVehicleEntry>();
			vehicleEntry.GVE_IsIncoming = true;
			vehicleEntry.GVE_EntryTime = EntryTime;
			vehicleEntry.GVE_GVM_VehicleMovement = vehicleMovement.PK;

			var shipment = new GteVehicleMovementDataObjectWriter(new DataWritingManager(new ActionInfo(null, vehicleMovement))).GetDataObject(vehicleMovement);
			var address = shipment.OrganizationAddressCollection?.FirstOrDefault(nameof(DocAddressType.LocalCartageYard));
			CombineAssertions(() =>
			{
				AssertEquals("Expected DataContext to be GateVehicleMovement", nameof(DataContextType.GateVehicleMovement), shipment.DataContext.DataSourceCollection.FirstOrDefault().Type);
				AssertEquals("Expected OrganizationCode to match", transportCompany.OH_Code, shipment.OrganizationAddressCollection.FirstOrDefault().OrganizationCode);
				AssertEquals("Expected WarehouseLocation to match", vehicleMovement.WarehouseLocation.ToLocationString(), shipment.WarehouseLocation);
				AssertEquals("Expected Warehouse to match", vehicleMovement.WarehouseLocation.Warehouse.WarehouseAddress.OA_Code, address.AddressShortCode);
				AssertEquals("Expected Warehouse to match", vehicleMovement.WarehouseLocation.Warehouse.WarehouseAddress.Address1, address.Address1);
				AssertEquals("Expected Warehouse to match", vehicleMovement.WarehouseLocation.Warehouse.WarehouseAddress.Address2, address.Address2);
				AssertEquals("Expected Warehouse to match", vehicleMovement.WarehouseLocation.Warehouse.WarehouseAddress.City, address.City);
				AssertEquals("Expected Warehouse to match", vehicleMovement.WarehouseLocation.Warehouse.WarehouseAddress.Postcode, address.Postcode);
				AssertEquals("Expected Warehouse to match", vehicleMovement.WarehouseLocation.Warehouse.WarehouseAddress.Header.OH_Code, address.OrganizationCode);
				AssertEquals("Expected VehicleRegistration to match", vehicleMovement.GVM_VehicleRegistration, shipment.VehicleRun.Vehicle.Registration.Number);
				AssertEquals("Expected VehicleType to match", vehicleMovement.VehicleType.RC_Code, shipment.VehicleRun.Vehicle.VehicleType.Code);
				AssertEquals("Expected DateCollection has one element", 1, shipment.DateCollection.Count);
				AssertEquals("Expected DateCollection has matched Vehicle Gate In Time", vehicleEntry.GVE_EntryTime, shipment.DateCollection.FirstOrDefault(date => date.Type == DateType.Start)?.Value);
				AssertNotNull("Expected there to be a RelatedShipmentCollection", shipment.RelatedShipmentCollection);
				AssertEquals("Expected there to be single RelatedShipment", 1, shipment.RelatedShipmentCollection.Count);
				AssertEquals("Expected RelatedShipment to be incoming", bool.TrueString, shipment.RelatedShipmentCollection[0].GetAdditionalInfoOrDefault("IsIncoming"));
				AssertNotNull("Expected there to be a SubShipmentCollection", shipment.SubShipmentCollection);
				AssertEquals("Expected there to be a single SubShipment", 1, shipment.SubShipmentCollection.Count);
				AssertEquals("Expected SubShipment BookingPartyReference to match", gateMovementBooking.GBM_SourceReferenceNumber, shipment.SubShipmentCollection[0].GetAdditionalReferenceOrDefault(AdditionalReferenceTypes.Codes.BookingPartyReference));
			});
		}

		public void TestDraftGVM_WhenEntryTimeIsDefault_ThenDataSourceKeyIsTheDraftGateInVehicleEntry()
		{
			var vehicleMovement = Factory.NewWithValidTestData<GteVehicleMovement>();
			vehicleMovement.GVM_VehicleRegistration = "REG-123";
			vehicleMovement.GVM_RC_VehicleType = Factory.NewWithValidTestData<RefContainer>().PK;
			vehicleMovement.VehicleType.RC_Code = "RTRK";
			vehicleMovement.GVM_WL_Location = Factory.NewWithValidTestData<WhsLocation>().PK;

			var gateMovement = Factory.NewWithValidTestData<GteGateMovement>();
			gateMovement.GGM_GVM_VehicleMovement = vehicleMovement.PK;

			var lane = Factory.NewWithValidTestData<GteLane>();

			var vehicleEntry1 = Factory.New<GteVehicleEntry>();
			vehicleEntry1.GVE_IsIncoming = true;
			vehicleEntry1.GVE_GVM_VehicleMovement = vehicleMovement.PK;
			vehicleEntry1.GVE_GateActionNumber = "GVE001";
			vehicleEntry1.GVE_GLN_Lane = lane.PK;

			var shipment = new GteVehicleMovementDataObjectWriter(new DataWritingManager(new ActionInfo(null, vehicleMovement))).GetDataObject(vehicleMovement);

			CombineAssertions(() =>
			{
				AssertEquals("Expected default value for GVE_EntryTime as this denotes a draft gate-in", true, vehicleEntry1.GVE_EntryTime.IsDefault);
				AssertEquals("Expected DataContext to be GateVehicleMovement", nameof(DataContextType.GateVehicleMovement), shipment.DataContext.DataSourceCollection.FirstOrDefault().Type);
				AssertEquals("Expected DataContext key to be the draft gate-in", "GVE001", shipment.DataContext.DataSourceCollection.FirstOrDefault().Key);
			});
		}

		public void TestGivenVehicleMovementWithGateInAndGateOut_WhenGenerateUXML_ThenFieldsAreCorrectlySet()
		{
			var vehicleMovement = Factory.NewWithValidTestData<GteVehicleMovement>();
			vehicleMovement.GVM_VehicleRegistration = "REG-123";
			vehicleMovement.GVM_RC_VehicleType = Factory.NewWithValidTestData<RefContainer>().PK;
			vehicleMovement.VehicleType.RC_Code = "RTRK";
			vehicleMovement.GVM_WL_Location = Factory.NewWithValidTestData<WhsLocation>().PK;
			vehicleMovement.WarehouseLocation.Warehouse.WW_WarehouseType = WarehouseTypes.Codes.ContainerYard;
			vehicleMovement.WarehouseLocation.Warehouse.WarehouseAddress.Address1 = "Address 1";
			vehicleMovement.WarehouseLocation.Warehouse.WarehouseAddress.Address2 = "Address 2";
			vehicleMovement.WarehouseLocation.Warehouse.WarehouseAddress.City = "SYD";
			vehicleMovement.WarehouseLocation.Warehouse.WarehouseAddress.Postcode = "0000";
			vehicleMovement.WarehouseLocation.Warehouse.WarehouseAddress.OA_Code = "BLANK SYD";
			vehicleMovement.WarehouseLocation.Warehouse.WarehouseAddress.Header.OH_Code = "ZZ";

			var gateMovement = Factory.NewWithValidTestData<GteGateMovement>();
			gateMovement.GGM_GVM_VehicleMovement = vehicleMovement.PK;

			var gateMovementBooking = gateMovement.GateMovementBooking;
			gateMovementBooking.GBM_SourceReferenceNumber = "VBS-001";

			var booking = gateMovementBooking.Booking;
			var transportCompany = booking.TransportCompany;
			transportCompany.OH_Code = "ABC";

			var vehicleEntry = Factory.NewWithValidTestData<GteVehicleEntry>();
			vehicleEntry.GVE_IsIncoming = true;
			vehicleEntry.GVE_EntryTime = EntryTime;
			vehicleEntry.GVE_GVM_VehicleMovement = vehicleMovement.PK;

			var vehicleExit = Factory.NewWithValidTestData<GteVehicleEntry>();
			vehicleExit.GVE_IsIncoming = false;
			vehicleExit.GVE_EntryTime = ExitTime;
			vehicleExit.GVE_GVM_VehicleMovement = vehicleMovement.PK;

			var shipment = new GteVehicleMovementDataObjectWriter(new DataWritingManager(new ActionInfo(null, vehicleMovement))).GetDataObject(vehicleMovement);
			var address = shipment.OrganizationAddressCollection?.FirstOrDefault(nameof(DocAddressType.LocalCartageYard));
			CombineAssertions(() =>
			{
				AssertEquals("Expected DataContext to be GateVehicleMovement", nameof(DataContextType.GateVehicleMovement), shipment.DataContext.DataSourceCollection.FirstOrDefault().Type);
				AssertEquals("Expected OrganizationCode to match", transportCompany.OH_Code, shipment.OrganizationAddressCollection.FirstOrDefault().OrganizationCode);
				AssertEquals("Expected WarehouseLocation to match", vehicleMovement.WarehouseLocation.ToLocationString(), shipment.WarehouseLocation);
				AssertEquals("Expected Warehouse to match", vehicleMovement.WarehouseLocation.Warehouse.WarehouseAddress.OA_Code, address.AddressShortCode);
				AssertEquals("Expected Warehouse to match", vehicleMovement.WarehouseLocation.Warehouse.WarehouseAddress.Address1, address.Address1);
				AssertEquals("Expected Warehouse to match", vehicleMovement.WarehouseLocation.Warehouse.WarehouseAddress.Address2, address.Address2);
				AssertEquals("Expected Warehouse to match", vehicleMovement.WarehouseLocation.Warehouse.WarehouseAddress.City, address.City);
				AssertEquals("Expected Warehouse to match", vehicleMovement.WarehouseLocation.Warehouse.WarehouseAddress.Postcode, address.Postcode);
				AssertEquals("Expected Warehouse to match", vehicleMovement.WarehouseLocation.Warehouse.WarehouseAddress.Header.OH_Code, address.OrganizationCode);
				AssertEquals("Expected VehicleRegistration to match", vehicleMovement.GVM_VehicleRegistration, shipment.VehicleRun.Vehicle.Registration.Number);
				AssertEquals("Expected VehicleType to match", vehicleMovement.VehicleType.RC_Code, shipment.VehicleRun.Vehicle.VehicleType.Code);
				AssertEquals("Expected DateCollection has two elements", 2, shipment.DateCollection.Count);
				AssertEquals("Expected DateCollection has matched Vehicle Gate In Time", vehicleEntry.GVE_EntryTime, shipment.DateCollection.FirstOrDefault(date => date.Type == DateType.Start)?.Value);
				AssertEquals("Expected DateCollection has matched Vehicle Gate Out Time", vehicleExit.GVE_EntryTime, shipment.DateCollection.FirstOrDefault(date => date.Type == DateType.End)?.Value);
				AssertNotNull("Expected there to be a RelatedShipmentCollection", shipment.RelatedShipmentCollection);
				AssertEquals("Expected there to be two RelatedShipments", 2, shipment.RelatedShipmentCollection.Count);
				AssertNotNull("Expected a RelatedShipment to be incoming", shipment.RelatedShipmentCollection.FirstOrDefault(shipment => (string)shipment.GetAdditionalInfoOrDefault("IsIncoming") == bool.TrueString));
				AssertNotNull("Expected a RelatedShipment to be outgoing", shipment.RelatedShipmentCollection.FirstOrDefault(shipment => (string)shipment.GetAdditionalInfoOrDefault("IsIncoming") == bool.FalseString));
				AssertNotNull("Expected there to be a SubShipmentCollection", shipment.SubShipmentCollection);
				AssertEquals("Expected there to be a single SubShipment", 1, shipment.SubShipmentCollection.Count);
				AssertEquals("Expected SubShipment BookingPartyReference to match", gateMovementBooking.GBM_SourceReferenceNumber, shipment.SubShipmentCollection[0].GetAdditionalReferenceOrDefault(AdditionalReferenceTypes.Codes.BookingPartyReference));
			});
		}

		public void TestGivenVehicleMovementWithReversedGateIn_WhenGenerateUXML_ThenFieldsAreCorrectlySet()
		{
			var vehicleMovement = Factory.NewWithValidTestData<GteVehicleMovement>();
			vehicleMovement.GVM_VehicleRegistration = "REG-123";
			vehicleMovement.GVM_RC_VehicleType = Factory.NewWithValidTestData<RefContainer>().PK;
			vehicleMovement.VehicleType.RC_Code = "RTRK";
			vehicleMovement.GVM_WL_Location = Factory.NewWithValidTestData<WhsLocation>().PK;
			vehicleMovement.WarehouseLocation.Warehouse.WW_WarehouseType = WarehouseTypes.Codes.ContainerYard;
			vehicleMovement.WarehouseLocation.Warehouse.WarehouseAddress.Address1 = "Address 1";
			vehicleMovement.WarehouseLocation.Warehouse.WarehouseAddress.Address2 = "Address 2";
			vehicleMovement.WarehouseLocation.Warehouse.WarehouseAddress.City = "SYD";
			vehicleMovement.WarehouseLocation.Warehouse.WarehouseAddress.Postcode = "0000";
			vehicleMovement.WarehouseLocation.Warehouse.WarehouseAddress.OA_Code = "BLANK SYD";
			vehicleMovement.WarehouseLocation.Warehouse.WarehouseAddress.Header.OH_Code = "ZZ";

			var gateMovement = Factory.NewWithValidTestData<GteGateMovement>();
			gateMovement.GGM_GVM_VehicleMovement = vehicleMovement.PK;
			var gateMovementBooking = gateMovement.GateMovementBooking;
			gateMovementBooking.GBM_SourceReferenceNumber = "VBS-001";

			var booking = gateMovementBooking.Booking;
			var transportCompany = booking.TransportCompany;
			transportCompany.OH_Code = "ABC";

			var cancelledVehicleEntry = Factory.NewWithValidTestData<GteVehicleEntry>();
			cancelledVehicleEntry.GVE_IsIncoming = true;
			cancelledVehicleEntry.GVE_CancelledReason = "Test";
			cancelledVehicleEntry.GVE_CancelledTime = DateTime.Now;
			cancelledVehicleEntry.GVE_GS_NKCancelledBy = "~BP";
			cancelledVehicleEntry.GVE_GVM_VehicleMovement = vehicleMovement.PK;

			var shipment = new GteVehicleMovementDataObjectWriter(new DataWritingManager(new ActionInfo(null, vehicleMovement))).GetDataObject(vehicleMovement);
			var address = shipment.OrganizationAddressCollection?.FirstOrDefault(nameof(DocAddressType.LocalCartageYard));
			CombineAssertions(() =>
			{
				AssertEquals("Expected DataContext to be GateVehicleMovement", nameof(DataContextType.GateVehicleMovement), shipment.DataContext.DataSourceCollection.FirstOrDefault().Type);
				AssertEquals("Expected OrganizationCode to match", transportCompany.OH_Code, shipment.OrganizationAddressCollection.FirstOrDefault().OrganizationCode);
				AssertEquals("Expected WarehouseLocation to match", vehicleMovement.WarehouseLocation.ToLocationString(), shipment.WarehouseLocation);
				AssertEquals("Expected Warehouse to match", vehicleMovement.WarehouseLocation.Warehouse.WarehouseAddress.OA_Code, address.AddressShortCode);
				AssertEquals("Expected Warehouse to match", vehicleMovement.WarehouseLocation.Warehouse.WarehouseAddress.Address1, address.Address1);
				AssertEquals("Expected Warehouse to match", vehicleMovement.WarehouseLocation.Warehouse.WarehouseAddress.Address2, address.Address2);
				AssertEquals("Expected Warehouse to match", vehicleMovement.WarehouseLocation.Warehouse.WarehouseAddress.City, address.City);
				AssertEquals("Expected Warehouse to match", vehicleMovement.WarehouseLocation.Warehouse.WarehouseAddress.Postcode, address.Postcode);
				AssertEquals("Expected Warehouse to match", vehicleMovement.WarehouseLocation.Warehouse.WarehouseAddress.Header.OH_Code, address.OrganizationCode);
				AssertEquals("Expected VehicleRegistration to match", vehicleMovement.GVM_VehicleRegistration, shipment.VehicleRun.Vehicle.Registration.Number);
				AssertEquals("Expected VehicleType to match", vehicleMovement.VehicleType.RC_Code, shipment.VehicleRun.Vehicle.VehicleType.Code);
				AssertNull("Expected there to be no DateCollection", shipment.DateCollection);
				AssertNull("Expected there to be no RelatedShipmentCollection", shipment.RelatedShipmentCollection);
				AssertNotNull("Expected there to be a SubShipmentCollection", shipment.SubShipmentCollection);
				AssertEquals("Expected there to be a single SubShipment", 1, shipment.SubShipmentCollection.Count);
				AssertEquals("Expected SubShipment BookingPartyReference to match", gateMovementBooking.GBM_SourceReferenceNumber, shipment.SubShipmentCollection[0].GetAdditionalReferenceOrDefault(AdditionalReferenceTypes.Codes.BookingPartyReference));
			});
		}

		public void TestGivenVehicleMovementWithGateInAndReversedGateOut_WhenGenerateUXML_ThenFieldsAreCorrectlySet()
		{
			var vehicleMovement = Factory.NewWithValidTestData<GteVehicleMovement>();
			vehicleMovement.GVM_VehicleRegistration = "REG-123";
			vehicleMovement.GVM_RC_VehicleType = Factory.NewWithValidTestData<RefContainer>().PK;
			vehicleMovement.VehicleType.RC_Code = "RTRK";
			vehicleMovement.GVM_WL_Location = Factory.NewWithValidTestData<WhsLocation>().PK;
			vehicleMovement.WarehouseLocation.Warehouse.WW_WarehouseType = WarehouseTypes.Codes.ContainerYard;
			vehicleMovement.WarehouseLocation.Warehouse.WarehouseAddress.Address1 = "Address 1";
			vehicleMovement.WarehouseLocation.Warehouse.WarehouseAddress.Address2 = "Address 2";
			vehicleMovement.WarehouseLocation.Warehouse.WarehouseAddress.City = "SYD";
			vehicleMovement.WarehouseLocation.Warehouse.WarehouseAddress.Postcode = "0000";
			vehicleMovement.WarehouseLocation.Warehouse.WarehouseAddress.OA_Code = "BLANK SYD";
			vehicleMovement.WarehouseLocation.Warehouse.WarehouseAddress.Header.OH_Code = "ZZ";

			var gateMovement = Factory.NewWithValidTestData<GteGateMovement>();
			gateMovement.GGM_GVM_VehicleMovement = vehicleMovement.PK;
			var gateMovementBooking = gateMovement.GateMovementBooking;
			gateMovementBooking.GBM_SourceReferenceNumber = "VBS-001";

			var booking = gateMovementBooking.Booking;
			var transportCompany = booking.TransportCompany;
			transportCompany.OH_Code = "ABC";

			var vehicleEntry = Factory.NewWithValidTestData<GteVehicleEntry>();
			vehicleEntry.GVE_IsIncoming = true;
			vehicleEntry.GVE_EntryTime = EntryTime;
			vehicleEntry.GVE_GVM_VehicleMovement = vehicleMovement.PK;

			var cancelledVehicleExit = Factory.NewWithValidTestData<GteVehicleEntry>();
			cancelledVehicleExit.GVE_IsIncoming = false;
			cancelledVehicleExit.GVE_CancelledReason = "Test";
			cancelledVehicleExit.GVE_CancelledTime = DateTime.Now;
			cancelledVehicleExit.GVE_GS_NKCancelledBy = "~BP";
			cancelledVehicleExit.GVE_GVM_VehicleMovement = vehicleMovement.PK;

			var shipment = new GteVehicleMovementDataObjectWriter(new DataWritingManager(new ActionInfo(null, vehicleMovement))).GetDataObject(vehicleMovement);
			var address = shipment.OrganizationAddressCollection?.FirstOrDefault(nameof(DocAddressType.LocalCartageYard));
			CombineAssertions(() =>
			{
				AssertEquals("Expected DataContext to be GateVehicleMovement", nameof(DataContextType.GateVehicleMovement), shipment.DataContext.DataSourceCollection.FirstOrDefault().Type);
				AssertEquals("Expected OrganizationCode to match", transportCompany.OH_Code, shipment.OrganizationAddressCollection.FirstOrDefault().OrganizationCode);
				AssertEquals("Expected WarehouseLocation to match", vehicleMovement.WarehouseLocation.ToLocationString(), shipment.WarehouseLocation);
				AssertEquals("Expected Warehouse to match", vehicleMovement.WarehouseLocation.Warehouse.WarehouseAddress.OA_Code, address.AddressShortCode);
				AssertEquals("Expected Warehouse to match", vehicleMovement.WarehouseLocation.Warehouse.WarehouseAddress.Address1, address.Address1);
				AssertEquals("Expected Warehouse to match", vehicleMovement.WarehouseLocation.Warehouse.WarehouseAddress.Address2, address.Address2);
				AssertEquals("Expected Warehouse to match", vehicleMovement.WarehouseLocation.Warehouse.WarehouseAddress.City, address.City);
				AssertEquals("Expected Warehouse to match", vehicleMovement.WarehouseLocation.Warehouse.WarehouseAddress.Postcode, address.Postcode);
				AssertEquals("Expected Warehouse to match", vehicleMovement.WarehouseLocation.Warehouse.WarehouseAddress.Header.OH_Code, address.OrganizationCode);
				AssertEquals("Expected VehicleRegistration to match", vehicleMovement.GVM_VehicleRegistration, shipment.VehicleRun.Vehicle.Registration.Number);
				AssertEquals("Expected VehicleType to match", vehicleMovement.VehicleType.RC_Code, shipment.VehicleRun.Vehicle.VehicleType.Code);
				AssertEquals("Expected DateCollection has one element", 1, shipment.DateCollection.Count);
				AssertEquals("Expected DateCollection has matched Vehicle Gate In Time", vehicleEntry.GVE_EntryTime, shipment.DateCollection.FirstOrDefault(date => date.Type == DateType.Start)?.Value);
				AssertNotNull("Expected there to be a RelatedShipmentCollection", shipment.RelatedShipmentCollection);
				AssertEquals("Expected there to be one RelatedShipment", 1, shipment.RelatedShipmentCollection.Count);
				AssertNotNull("Expected a RelatedShipment to be incoming", shipment.RelatedShipmentCollection.FirstOrDefault(shipment => (string)shipment.GetAdditionalInfoOrDefault("IsIncoming") == bool.TrueString));
				AssertNotNull("Expected there to be a SubShipmentCollection", shipment.SubShipmentCollection);
				AssertEquals("Expected there to be a single SubShipment", 1, shipment.SubShipmentCollection.Count);
				AssertEquals("Expected SubShipment BookingPartyReference to match", gateMovementBooking.GBM_SourceReferenceNumber, shipment.SubShipmentCollection[0].GetAdditionalReferenceOrDefault(AdditionalReferenceTypes.Codes.BookingPartyReference));
			});
		}

		public void TestGivenVehicleMovementWithReversedGateInAndGateIn_WhenGenerateUXML_ThenFieldsAreCorrectlySet()
		{
			var vehicleMovement = Factory.NewWithValidTestData<GteVehicleMovement>();
			vehicleMovement.GVM_VehicleRegistration = "REG-123";
			vehicleMovement.GVM_RC_VehicleType = Factory.NewWithValidTestData<RefContainer>().PK;
			vehicleMovement.VehicleType.RC_Code = "RTRK";
			vehicleMovement.GVM_WL_Location = Factory.NewWithValidTestData<WhsLocation>().PK;
			vehicleMovement.WarehouseLocation.Warehouse.WW_WarehouseType = WarehouseTypes.Codes.ContainerYard;
			vehicleMovement.WarehouseLocation.Warehouse.WarehouseAddress.Address1 = "Address 1";
			vehicleMovement.WarehouseLocation.Warehouse.WarehouseAddress.Address2 = "Address 2";
			vehicleMovement.WarehouseLocation.Warehouse.WarehouseAddress.City = "SYD";
			vehicleMovement.WarehouseLocation.Warehouse.WarehouseAddress.Postcode = "0000";
			vehicleMovement.WarehouseLocation.Warehouse.WarehouseAddress.OA_Code = "BLANK SYD";
			vehicleMovement.WarehouseLocation.Warehouse.WarehouseAddress.Header.OH_Code = "ZZ";

			var gateMovement = Factory.NewWithValidTestData<GteGateMovement>();
			gateMovement.GGM_GVM_VehicleMovement = vehicleMovement.PK;
			var gateMovementBooking = gateMovement.GateMovementBooking;
			gateMovementBooking.GBM_SourceReferenceNumber = "VBS-001";

			var booking = gateMovementBooking.Booking;
			var transportCompany = booking.TransportCompany;
			transportCompany.OH_Code = "ABC";

			var cancelledVehicleEntry = Factory.NewWithValidTestData<GteVehicleEntry>();
			cancelledVehicleEntry.GVE_IsIncoming = true;
			cancelledVehicleEntry.GVE_CancelledReason = "Test";
			cancelledVehicleEntry.GVE_CancelledTime = DateTime.Now;
			cancelledVehicleEntry.GVE_GS_NKCancelledBy = "~BP";
			cancelledVehicleEntry.GVE_EntryTime = EntryTime.AddHours(-1);
			cancelledVehicleEntry.GVE_GVM_VehicleMovement = vehicleMovement.PK;

			var vehicleEntry = Factory.NewWithValidTestData<GteVehicleEntry>();
			vehicleEntry.GVE_IsIncoming = true;
			vehicleEntry.GVE_EntryTime = EntryTime;
			vehicleEntry.GVE_GVM_VehicleMovement = vehicleMovement.PK;

			var shipment = new GteVehicleMovementDataObjectWriter(new DataWritingManager(new ActionInfo(null, vehicleMovement))).GetDataObject(vehicleMovement);
			var address = shipment.OrganizationAddressCollection?.FirstOrDefault(nameof(DocAddressType.LocalCartageYard));
			CombineAssertions(() =>
			{
				AssertEquals("Expected DataContext to be GateVehicleMovement", nameof(DataContextType.GateVehicleMovement), shipment.DataContext.DataSourceCollection.FirstOrDefault().Type);
				AssertEquals("Expected OrganizationCode to match", transportCompany.OH_Code, shipment.OrganizationAddressCollection.FirstOrDefault().OrganizationCode);
				AssertEquals("Expected WarehouseLocation to match", vehicleMovement.WarehouseLocation.ToLocationString(), shipment.WarehouseLocation);
				AssertEquals("Expected Warehouse to match", vehicleMovement.WarehouseLocation.Warehouse.WarehouseAddress.OA_Code, address.AddressShortCode);
				AssertEquals("Expected Warehouse to match", vehicleMovement.WarehouseLocation.Warehouse.WarehouseAddress.Address1, address.Address1);
				AssertEquals("Expected Warehouse to match", vehicleMovement.WarehouseLocation.Warehouse.WarehouseAddress.Address2, address.Address2);
				AssertEquals("Expected Warehouse to match", vehicleMovement.WarehouseLocation.Warehouse.WarehouseAddress.City, address.City);
				AssertEquals("Expected Warehouse to match", vehicleMovement.WarehouseLocation.Warehouse.WarehouseAddress.Postcode, address.Postcode);
				AssertEquals("Expected Warehouse to match", vehicleMovement.WarehouseLocation.Warehouse.WarehouseAddress.Header.OH_Code, address.OrganizationCode);
				AssertEquals("Expected VehicleRegistration to match", vehicleMovement.GVM_VehicleRegistration, shipment.VehicleRun.Vehicle.Registration.Number);
				AssertEquals("Expected VehicleType to match", vehicleMovement.VehicleType.RC_Code, shipment.VehicleRun.Vehicle.VehicleType.Code);
				AssertEquals("Expected DateCollection has one element", 1, shipment.DateCollection.Count);
				AssertEquals("Expected DateCollection has matched Vehicle Gate In Time", vehicleEntry.GVE_EntryTime, shipment.DateCollection.FirstOrDefault(date => date.Type == DateType.Start)?.Value);
				AssertNotNull("Expected there to be a RelatedShipmentCollection", shipment.RelatedShipmentCollection);
				AssertEquals("Expected there to be one RelatedShipment", 1, shipment.RelatedShipmentCollection.Count);
				AssertNotNull("Expected a RelatedShipment to be incoming", shipment.RelatedShipmentCollection.FirstOrDefault(shipment => (string)shipment.GetAdditionalInfoOrDefault("IsIncoming") == bool.TrueString));
				AssertNotNull("Expected there to be a SubShipmentCollection", shipment.SubShipmentCollection);
				AssertEquals("Expected there to be a single SubShipment", 1, shipment.SubShipmentCollection.Count);
				AssertEquals("Expected SubShipment BookingPartyReference to match", gateMovementBooking.GBM_SourceReferenceNumber, shipment.SubShipmentCollection[0].GetAdditionalReferenceOrDefault(AdditionalReferenceTypes.Codes.BookingPartyReference));
			});
		}

		public void TestGivenVehicleMovementWithGateInAndReversedGateOutAndGateOut_WhenGenerateUXML_ThenFieldsAreCorrectlySet()
		{
			var vehicleMovement = Factory.NewWithValidTestData<GteVehicleMovement>();
			vehicleMovement.GVM_VehicleRegistration = "REG-123";
			vehicleMovement.GVM_RC_VehicleType = Factory.NewWithValidTestData<RefContainer>().PK;
			vehicleMovement.VehicleType.RC_Code = "RTRK";
			vehicleMovement.GVM_WL_Location = Factory.NewWithValidTestData<WhsLocation>().PK;
			vehicleMovement.WarehouseLocation.Warehouse.WW_WarehouseType = WarehouseTypes.Codes.ContainerYard;
			vehicleMovement.WarehouseLocation.Warehouse.WarehouseAddress.Address1 = "Address 1";
			vehicleMovement.WarehouseLocation.Warehouse.WarehouseAddress.Address2 = "Address 2";
			vehicleMovement.WarehouseLocation.Warehouse.WarehouseAddress.City = "SYD";
			vehicleMovement.WarehouseLocation.Warehouse.WarehouseAddress.Postcode = "0000";
			vehicleMovement.WarehouseLocation.Warehouse.WarehouseAddress.OA_Code = "BLANK SYD";
			vehicleMovement.WarehouseLocation.Warehouse.WarehouseAddress.Header.OH_Code = "ZZ";

			var gateMovement = Factory.NewWithValidTestData<GteGateMovement>();
			gateMovement.GGM_GVM_VehicleMovement = vehicleMovement.PK;
			var gateMovementBooking = gateMovement.GateMovementBooking;
			gateMovementBooking.GBM_SourceReferenceNumber = "VBS-001";

			var booking = gateMovementBooking.Booking;
			var transportCompany = booking.TransportCompany;
			transportCompany.OH_Code = "ABC";

			var vehicleEntry = Factory.NewWithValidTestData<GteVehicleEntry>();
			vehicleEntry.GVE_IsIncoming = true;
			vehicleEntry.GVE_EntryTime = EntryTime;
			vehicleEntry.GVE_GVM_VehicleMovement = vehicleMovement.PK;

			var cancelledVehicleExit = Factory.NewWithValidTestData<GteVehicleEntry>();
			cancelledVehicleExit.GVE_IsIncoming = false;
			cancelledVehicleExit.GVE_CancelledReason = "Test";
			cancelledVehicleExit.GVE_CancelledTime = DateTime.Now;
			cancelledVehicleExit.GVE_GS_NKCancelledBy = "~BP";
			cancelledVehicleExit.GVE_EntryTime = ExitTime.AddHours(-1);
			cancelledVehicleExit.GVE_GVM_VehicleMovement = vehicleMovement.PK;

			var vehicleExit = Factory.NewWithValidTestData<GteVehicleEntry>();
			vehicleExit.GVE_IsIncoming = false;
			vehicleExit.GVE_EntryTime = ExitTime;
			vehicleExit.GVE_GVM_VehicleMovement = vehicleMovement.PK;

			var shipment = new GteVehicleMovementDataObjectWriter(new DataWritingManager(new ActionInfo(null, vehicleMovement))).GetDataObject(vehicleMovement);
			var address = shipment.OrganizationAddressCollection?.FirstOrDefault(nameof(DocAddressType.LocalCartageYard));
			CombineAssertions(() =>
			{
				AssertEquals("Expected DataContext to be GateVehicleMovement", nameof(DataContextType.GateVehicleMovement), shipment.DataContext.DataSourceCollection.FirstOrDefault().Type);
				AssertEquals("Expected OrganizationCode to match", transportCompany.OH_Code, shipment.OrganizationAddressCollection.FirstOrDefault().OrganizationCode);
				AssertEquals("Expected WarehouseLocation to match", vehicleMovement.WarehouseLocation.ToLocationString(), shipment.WarehouseLocation);
				AssertEquals("Expected Warehouse to match", vehicleMovement.WarehouseLocation.Warehouse.WarehouseAddress.OA_Code, address.AddressShortCode);
				AssertEquals("Expected Warehouse to match", vehicleMovement.WarehouseLocation.Warehouse.WarehouseAddress.Address1, address.Address1);
				AssertEquals("Expected Warehouse to match", vehicleMovement.WarehouseLocation.Warehouse.WarehouseAddress.Address2, address.Address2);
				AssertEquals("Expected Warehouse to match", vehicleMovement.WarehouseLocation.Warehouse.WarehouseAddress.City, address.City);
				AssertEquals("Expected Warehouse to match", vehicleMovement.WarehouseLocation.Warehouse.WarehouseAddress.Postcode, address.Postcode);
				AssertEquals("Expected Warehouse to match", vehicleMovement.WarehouseLocation.Warehouse.WarehouseAddress.Header.OH_Code, address.OrganizationCode);
				AssertEquals("Expected VehicleRegistration to match", vehicleMovement.GVM_VehicleRegistration, shipment.VehicleRun.Vehicle.Registration.Number);
				AssertEquals("Expected VehicleType to match", vehicleMovement.VehicleType.RC_Code, shipment.VehicleRun.Vehicle.VehicleType.Code);
				AssertEquals("Expected DateCollection has two elements", 2, shipment.DateCollection.Count);
				AssertEquals("Expected DateCollection has matched Vehicle Gate In Time", vehicleEntry.GVE_EntryTime, shipment.DateCollection.FirstOrDefault(date => date.Type == DateType.Start)?.Value);
				AssertEquals("Expected DateCollection has matched Vehicle Gate Out Time", vehicleExit.GVE_EntryTime, shipment.DateCollection.FirstOrDefault(date => date.Type == DateType.End)?.Value);
				AssertNotNull("Expected there to be a RelatedShipmentCollection", shipment.RelatedShipmentCollection);
				AssertEquals("Expected there to be two RelatedShipments", 2, shipment.RelatedShipmentCollection.Count);
				AssertNotNull("Expected a RelatedShipment to be incoming", shipment.RelatedShipmentCollection.FirstOrDefault(shipment => (string)shipment.GetAdditionalInfoOrDefault("IsIncoming") == bool.TrueString));
				AssertNotNull("Expected a RelatedShipment to be outgoing", shipment.RelatedShipmentCollection.FirstOrDefault(shipment => (string)shipment.GetAdditionalInfoOrDefault("IsIncoming") == bool.FalseString));
				AssertNotNull("Expected there to be a SubShipmentCollection", shipment.SubShipmentCollection);
				AssertEquals("Expected there to be a single SubShipment", 1, shipment.SubShipmentCollection.Count);
				AssertEquals("Expected SubShipment BookingPartyReference to match", gateMovementBooking.GBM_SourceReferenceNumber, shipment.SubShipmentCollection[0].GetAdditionalReferenceOrDefault(AdditionalReferenceTypes.Codes.BookingPartyReference));
			});
		}

		public void TestGivenVehicleMovementWithoutVehicleRegistration_WhenGenerateUXML_ThenFieldsAreCorrectlySet()
		{
			var vehicleMovement = Factory.NewWithValidTestData<GteVehicleMovement>();
			vehicleMovement.GVM_VehicleRegistration = "";
			vehicleMovement.GVM_RC_VehicleType = Factory.NewWithValidTestData<RefContainer>().PK;
			vehicleMovement.VehicleType.RC_Code = "RTRK";
			vehicleMovement.GVM_WL_Location = Factory.NewWithValidTestData<WhsLocation>().PK;
			vehicleMovement.WarehouseLocation.Warehouse.WW_WarehouseType = WarehouseTypes.Codes.ContainerYard;
			vehicleMovement.WarehouseLocation.Warehouse.WarehouseAddress.Address1 = "Address 1";
			vehicleMovement.WarehouseLocation.Warehouse.WarehouseAddress.Address2 = "Address 2";
			vehicleMovement.WarehouseLocation.Warehouse.WarehouseAddress.City = "SYD";
			vehicleMovement.WarehouseLocation.Warehouse.WarehouseAddress.Postcode = "0000";
			vehicleMovement.WarehouseLocation.Warehouse.WarehouseAddress.OA_Code = "BLANK SYD";
			vehicleMovement.WarehouseLocation.Warehouse.WarehouseAddress.Header.OH_Code = "ZZ";

			var gateMovement = Factory.NewWithValidTestData<GteGateMovement>();
			gateMovement.GGM_GVM_VehicleMovement = vehicleMovement.PK;
			var gateMovementBooking = gateMovement.GateMovementBooking;
			gateMovementBooking.GBM_SourceReferenceNumber = "VBS-001";

			var booking = gateMovementBooking.Booking;
			var transportCompany = booking.TransportCompany;
			transportCompany.OH_Code = "ABC";

			var shipment = new GteVehicleMovementDataObjectWriter(new DataWritingManager(new ActionInfo(null, vehicleMovement))).GetDataObject(vehicleMovement);
			var address = shipment.OrganizationAddressCollection?.FirstOrDefault(nameof(DocAddressType.LocalCartageYard));
			CombineAssertions(() =>
			{
				AssertEquals("Expected DataContext to be GateVehicleMovement", nameof(DataContextType.GateVehicleMovement), shipment.DataContext.DataSourceCollection.FirstOrDefault().Type);
				AssertEquals("Expected OrganizationCode to match", transportCompany.OH_Code, shipment.OrganizationAddressCollection.FirstOrDefault().OrganizationCode);
				AssertEquals("Expected WarehouseLocation to match", vehicleMovement.WarehouseLocation.ToLocationString(), shipment.WarehouseLocation);
				AssertEquals("Expected Warehouse to match", vehicleMovement.WarehouseLocation.Warehouse.WarehouseAddress.OA_Code, address.AddressShortCode);
				AssertEquals("Expected Warehouse to match", vehicleMovement.WarehouseLocation.Warehouse.WarehouseAddress.Address1, address.Address1);
				AssertEquals("Expected Warehouse to match", vehicleMovement.WarehouseLocation.Warehouse.WarehouseAddress.Address2, address.Address2);
				AssertEquals("Expected Warehouse to match", vehicleMovement.WarehouseLocation.Warehouse.WarehouseAddress.City, address.City);
				AssertEquals("Expected Warehouse to match", vehicleMovement.WarehouseLocation.Warehouse.WarehouseAddress.Postcode, address.Postcode);
				AssertEquals("Expected Warehouse to match", vehicleMovement.WarehouseLocation.Warehouse.WarehouseAddress.Header.OH_Code, address.OrganizationCode);
				AssertNull("Expected VehicleRegistration to be null", shipment.VehicleRun?.Vehicle?.Registration?.Number);
				AssertEquals("Expected VehicleType to match", vehicleMovement.VehicleType.RC_Code, shipment.VehicleRun.Vehicle.VehicleType.Code);
				AssertNull("Expected there to be no DateCollection", shipment.DateCollection);
				AssertNull("Expected there to be no RelatedShipmentCollection", shipment.RelatedShipmentCollection);
				AssertNotNull("Expected there to be a SubShipmentCollection", shipment.SubShipmentCollection);
				AssertEquals("Expected there to be a single SubShipment", 1, shipment.SubShipmentCollection.Count);
				AssertEquals("Expected SubShipment BookingPartyReference to match", gateMovementBooking.GBM_SourceReferenceNumber, shipment.SubShipmentCollection[0].GetAdditionalReferenceOrDefault(AdditionalReferenceTypes.Codes.BookingPartyReference));
			});
		}

		public void TestGivenVehicleMovementWithoutVehicleType_WhenGenerateUXML_ThenFieldsAreCorrectlySet()
		{
			var vehicleMovement = Factory.New<GteVehicleMovement>();
			vehicleMovement.GVM_VehicleRegistration = "REG-123";
			vehicleMovement.GVM_WL_Location = Factory.NewWithValidTestData<WhsLocation>().PK;
			vehicleMovement.WarehouseLocation.Warehouse.WW_WarehouseType = WarehouseTypes.Codes.ContainerYard;
			vehicleMovement.WarehouseLocation.Warehouse.WarehouseAddress.Address1 = "Address 1";
			vehicleMovement.WarehouseLocation.Warehouse.WarehouseAddress.Address2 = "Address 2";
			vehicleMovement.WarehouseLocation.Warehouse.WarehouseAddress.City = "SYD";
			vehicleMovement.WarehouseLocation.Warehouse.WarehouseAddress.Postcode = "0000";
			vehicleMovement.WarehouseLocation.Warehouse.WarehouseAddress.OA_Code = "BLANK SYD";
			vehicleMovement.WarehouseLocation.Warehouse.WarehouseAddress.Header.OH_Code = "ZZ";

			var gateMovement = Factory.NewWithValidTestData<GteGateMovement>();
			gateMovement.GGM_GVM_VehicleMovement = vehicleMovement.PK;
			var gateMovementBooking = gateMovement.GateMovementBooking;
			gateMovementBooking.GBM_SourceReferenceNumber = "VBS-001";

			var booking = gateMovementBooking.Booking;
			var transportCompany = booking.TransportCompany;
			transportCompany.OH_Code = "ABC";

			var shipment = new GteVehicleMovementDataObjectWriter(new DataWritingManager(new ActionInfo(null, vehicleMovement))).GetDataObject(vehicleMovement);
			var address = shipment.OrganizationAddressCollection?.FirstOrDefault(nameof(DocAddressType.LocalCartageYard));
			CombineAssertions(() =>
			{
				AssertEquals("Expected DataContext to be GateVehicleMovement", nameof(DataContextType.GateVehicleMovement), shipment.DataContext.DataSourceCollection.FirstOrDefault().Type);
				AssertEquals("Expected OrganizationCode to match", transportCompany.OH_Code, shipment.OrganizationAddressCollection.FirstOrDefault().OrganizationCode);
				AssertEquals("Expected WarehouseLocation to match", vehicleMovement.WarehouseLocation.ToLocationString(), shipment.WarehouseLocation);
				AssertEquals("Expected Warehouse to match", vehicleMovement.WarehouseLocation.Warehouse.WarehouseAddress.OA_Code, address.AddressShortCode);
				AssertEquals("Expected Warehouse to match", vehicleMovement.WarehouseLocation.Warehouse.WarehouseAddress.Address1, address.Address1);
				AssertEquals("Expected Warehouse to match", vehicleMovement.WarehouseLocation.Warehouse.WarehouseAddress.Address2, address.Address2);
				AssertEquals("Expected Warehouse to match", vehicleMovement.WarehouseLocation.Warehouse.WarehouseAddress.City, address.City);
				AssertEquals("Expected Warehouse to match", vehicleMovement.WarehouseLocation.Warehouse.WarehouseAddress.Postcode, address.Postcode);
				AssertEquals("Expected Warehouse to match", vehicleMovement.WarehouseLocation.Warehouse.WarehouseAddress.Header.OH_Code, address.OrganizationCode);
				AssertEquals("Expected VehicleRegistration to match", vehicleMovement.GVM_VehicleRegistration, shipment.VehicleRun.Vehicle.Registration.Number);
				AssertNull("Expected VehicleType to be null", shipment.VehicleRun?.Vehicle?.VehicleType?.Code);
				AssertNull("Expected there to be no DateCollection", shipment.DateCollection);
				AssertNull("Expected there to be no RelatedShipmentCollection", shipment.RelatedShipmentCollection);
				AssertNotNull("Expected there to be a SubShipmentCollection", shipment.SubShipmentCollection);
				AssertEquals("Expected there to be a single SubShipment", 1, shipment.SubShipmentCollection.Count);
				AssertEquals("Expected SubShipment BookingPartyReference to match", gateMovementBooking.GBM_SourceReferenceNumber, shipment.SubShipmentCollection[0].GetAdditionalReferenceOrDefault(AdditionalReferenceTypes.Codes.BookingPartyReference));
			});
		}

		public void TestGivenVehicleMovementWithoutWarehouse_WhenGenerateUXML_ThenFieldsAreCorrectlySet()
		{
			var vehicleMovement = Factory.New<GteVehicleMovement>();
			vehicleMovement.GVM_VehicleRegistration = "REG-123";
			vehicleMovement.GVM_RC_VehicleType = Factory.NewWithValidTestData<RefContainer>().PK;
			vehicleMovement.VehicleType.RC_Code = "RTRK";

			var gateMovement = Factory.NewWithValidTestData<GteGateMovement>();
			gateMovement.GGM_GVM_VehicleMovement = vehicleMovement.PK;
			var gateMovementBooking = gateMovement.GateMovementBooking;
			gateMovementBooking.GBM_SourceReferenceNumber = "VBS-001";

			var booking = gateMovementBooking.Booking;
			var transportCompany = booking.TransportCompany;
			transportCompany.OH_Code = "ABC";

			var shipment = new GteVehicleMovementDataObjectWriter(new DataWritingManager(new ActionInfo(null, vehicleMovement))).GetDataObject(vehicleMovement);
			CombineAssertions(() =>
			{
				AssertEquals("Expected DataContext to be GateVehicleMovement", nameof(DataContextType.GateVehicleMovement), shipment.DataContext.DataSourceCollection.FirstOrDefault().Type);
				AssertEquals("Expected OrganizationCode to match", transportCompany.OH_Code, shipment.OrganizationAddressCollection.FirstOrDefault().OrganizationCode);
				AssertNull("Expected WarehouseLocation to be null", shipment.WarehouseLocation);
				AssertNull("Expected Warehouse to be null", shipment.OrganizationAddressCollection?.FirstOrDefault(nameof(DocAddressType.LocalCartageYard)));
				AssertEquals("Expected VehicleRegistration to match", vehicleMovement.GVM_VehicleRegistration, shipment.VehicleRun.Vehicle.Registration.Number);
				AssertEquals("Expected VehicleType to match", vehicleMovement.VehicleType.RC_Code, shipment.VehicleRun.Vehicle.VehicleType.Code);
				AssertNull("Expected there to be no DateCollection", shipment.DateCollection);
				AssertNull("Expected there to be no RelatedShipmentCollection", shipment.RelatedShipmentCollection);
				AssertNotNull("Expected there to be a SubShipmentCollection", shipment.SubShipmentCollection);
				AssertEquals("Expected there to be a single SubShipment", 1, shipment.SubShipmentCollection.Count);
				AssertEquals("Expected SubShipment BookingPartyReference to match", gateMovementBooking.GBM_SourceReferenceNumber, shipment.SubShipmentCollection[0].GetAdditionalReferenceOrDefault(AdditionalReferenceTypes.Codes.BookingPartyReference));
			});
		}

		public void TestGivenVehicleMovement_WhenWriteToUXML_ThenSetDataSources()
		{
			var booking = Factory.NewWithValidTestData<GteBooking>();
			booking.GBK_ReferenceNumber = "GB0001000";

			var gateMovementBooking = Factory.NewWithValidTestData<GteGateMovementBooking>();
			gateMovementBooking.GBM_GBK_Booking = booking.PK;
			gateMovementBooking.GBM_MovementBookingNumber = "GBM0001";

			var vehicleMovement = Factory.NewWithValidTestData<GteVehicleMovement>();
			vehicleMovement.GVM_VehicleRegistration = "REG-123";

			var gateMovement = Factory.NewWithValidTestData<GteGateMovement>();
			gateMovement.GGM_GVM_VehicleMovement = vehicleMovement.PK;
			gateMovement.GGM_GBM_MovementBooking = gateMovementBooking.PK;

			var vehicleEntry = Factory.NewWithValidTestData<GteVehicleEntry>();
			vehicleEntry.GVE_GVM_VehicleMovement = vehicleMovement.PK;

			var shipment = new GteVehicleMovementDataObjectWriter(new DataWritingManager(new ActionInfo(null, vehicleMovement))).GetDataObject(vehicleMovement);

			var subShipment1 = shipment.SubShipmentCollection.GetExactDataObjectFromSubShipmentCollection(gateMovementBooking.GBM_MovementBookingNumber);

			CombineAssertions(() =>
			{
				AssertEquals("Gate Vehicle Movement Test Shipment should have three data sources", 3, subShipment1.DataContext.DataSourceCollection.Count());
				AssertEquals("First Data source type should be GateMovementBooking", nameof(DataContextType.GateMovementBooking), subShipment1.DataContext.DataSourceCollection.FirstOrDefault().Type);
				AssertEquals("First Data source key should be set", "GBM0001", subShipment1.DataContext.DataSourceCollection.FirstOrDefault().Key);
				AssertEquals("Second Data source type should be GateMovement", nameof(DataContextType.GateMovement), subShipment1.DataContext.DataSourceCollection.ElementAtOrDefault(1).Type);
				AssertEquals("Second Data source key should be set", "GBM0001", subShipment1.DataContext.DataSourceCollection.ElementAtOrDefault(1).Key);
				AssertEquals("Third Data source type should be GateBooking", nameof(DataContextType.GateBooking), subShipment1.DataContext.DataSourceCollection.ElementAtOrDefault(2).Type);
				AssertEquals("Third Data source key should be set", "GB0001000", subShipment1.DataContext.DataSourceCollection.ElementAtOrDefault(2).Key);
			});
		}

		public void TestGivenVehicleMovementInMultipleBookings_WhenWriteToUXML_ThenSetDataSources()
		{
			var booking = Factory.NewWithValidTestData<GteBooking>();
			booking.GBK_ReferenceNumber = "GB0001000";

			var gateMovementBooking = Factory.NewWithValidTestData<GteGateMovementBooking>();
			gateMovementBooking.GBM_GBK_Booking = booking.PK;
			gateMovementBooking.GBM_MovementBookingNumber = "GBM0001";

			var booking2 = Factory.NewWithValidTestData<GteBooking>();
			booking2.GBK_ReferenceNumber = "GB0002000";

			var gateMovementBooking2 = Factory.NewWithValidTestData<GteGateMovementBooking>();
			gateMovementBooking2.GBM_GBK_Booking = booking2.PK;
			gateMovementBooking2.GBM_MovementBookingNumber = "GBM0002";

			var vehicleMovement = Factory.NewWithValidTestData<GteVehicleMovement>();
			vehicleMovement.GVM_VehicleRegistration = "REG-123";

			var vehicleEntry = Factory.NewWithValidTestData<GteVehicleEntry>();
			vehicleEntry.GVE_GVM_VehicleMovement = vehicleMovement.PK;

			var gateMovement = Factory.NewWithValidTestData<GteGateMovement>();
			gateMovement.GGM_GVM_VehicleMovement = vehicleMovement.PK;
			gateMovement.GGM_GBM_MovementBooking = gateMovementBooking.PK;

			var gateMovement2 = Factory.NewWithValidTestData<GteGateMovement>();
			gateMovement2.GGM_GVM_VehicleMovement = vehicleMovement.PK;
			gateMovement2.GGM_GBM_MovementBooking = gateMovementBooking2.PK;

			var shipment = new GteVehicleMovementDataObjectWriter(new DataWritingManager(new ActionInfo(null, vehicleMovement))).GetDataObject(vehicleMovement);

			var subShipment1 = shipment.SubShipmentCollection.GetExactDataObjectFromSubShipmentCollection(gateMovementBooking.GBM_MovementBookingNumber);
			var subShipment2 = shipment.SubShipmentCollection.GetExactDataObjectFromSubShipmentCollection(gateMovementBooking2.GBM_MovementBookingNumber);

			CombineAssertions(() =>
			{
				AssertEquals("Vehicle movement should have multiple SubShipments", 2, shipment.SubShipmentCollection.Count);
				AssertEquals("SubShipment1 should have three data sources", 3, subShipment1.DataContext.DataSourceCollection.Count());
				AssertEquals("The first Data source type for SubShipment1 should be GateMovementBooking", nameof(DataContextType.GateMovementBooking), subShipment1.DataContext.DataSourceCollection.FirstOrDefault().Type);
				AssertEquals("The first Data source key for SubShipment1 should be set", "GBM0001", subShipment1.DataContext.DataSourceCollection.FirstOrDefault().Key);
				AssertEquals("The second Data source type for SubShipment1 should be GateMovement", nameof(DataContextType.GateMovement), subShipment1.DataContext.DataSourceCollection.ElementAtOrDefault(1).Type);
				AssertEquals("The second Data source key for SubShipment1 should be set", "GBM0001", subShipment1.DataContext.DataSourceCollection.ElementAtOrDefault(1).Key);
				AssertEquals("The third Data source type for SubShipment1 should be GateBooking", nameof(DataContextType.GateBooking), subShipment1.DataContext.DataSourceCollection.ElementAtOrDefault(2).Type);
				AssertEquals("The third Data source key for SubShipment1 should be set", "GB0001000", subShipment1.DataContext.DataSourceCollection.ElementAtOrDefault(2).Key);
				AssertEquals("SubShipment 2 should have three data sources", 3, subShipment2.DataContext.DataSourceCollection.Count());
				AssertEquals("The first Data source type for SubShipment2 should be GateMovementBooking", nameof(DataContextType.GateMovementBooking), subShipment2.DataContext.DataSourceCollection.FirstOrDefault().Type);
				AssertEquals("The first Data source key for SubShipment2 should be set", "GBM0002", subShipment2.DataContext.DataSourceCollection.FirstOrDefault().Key);
				AssertEquals("The second Data source type for SubShipment2 should be GateMovement", nameof(DataContextType.GateMovement), subShipment2.DataContext.DataSourceCollection.ElementAtOrDefault(1).Type);
				AssertEquals("The second Data source key for SubShipment2 should be set", "GBM0002", subShipment2.DataContext.DataSourceCollection.ElementAtOrDefault(1).Key);
				AssertEquals("The third Data source type for SubShipment2 should be GateBooking", nameof(DataContextType.GateBooking), subShipment2.DataContext.DataSourceCollection.ElementAtOrDefault(2).Type);
				AssertEquals("The third Data source key for SubShipment2 should be set", "GB0002000", subShipment2.DataContext.DataSourceCollection.ElementAtOrDefault(2).Key);
			});
		}

		public void TestOrgAddressIsCorrectlyPopulatedDependingOnFacilityType()
		{
			var vehicleMovement = Factory.NewWithValidTestData<GteVehicleMovement>();
			vehicleMovement.GVM_VehicleRegistration = "REG-123";
			vehicleMovement.GVM_RC_VehicleType = Factory.NewWithValidTestData<RefContainer>().PK;
			vehicleMovement.VehicleType.RC_Code = "RTRK";
			vehicleMovement.GVM_WL_Location = Factory.NewWithValidTestData<WhsLocation>().PK;

			var gateMovement = Factory.NewWithValidTestData<GteGateMovement>();
			gateMovement.GGM_GVM_VehicleMovement = vehicleMovement.PK;

			var gateMovementBooking = gateMovement.GateMovementBooking;
			gateMovementBooking.GBM_SourceReferenceNumber = "VBS-001";

			var booking = gateMovementBooking.Booking;

			var transportCompany = booking.TransportCompany;
			transportCompany.OH_Code = "ABC";

			vehicleMovement.WarehouseLocation.Warehouse.WW_WarehouseType = WarehouseTypes.Codes.ContainerYard;
			var shipment = new GteVehicleMovementDataObjectWriter(new DataWritingManager(new ActionInfo(null, vehicleMovement))).GetDataObject(vehicleMovement);
			var cyAddress = shipment.OrganizationAddressCollection?.FirstOrDefault(nameof(DocAddressType.LocalCartageYard));

			AssertNotNull("Container Yard address should be populated under 'LocalCartageYard'", cyAddress);

			vehicleMovement.WarehouseLocation.Warehouse.WW_WarehouseType = WarehouseTypes.Codes.Transit;
			shipment = new GteVehicleMovementDataObjectWriter(new DataWritingManager(new ActionInfo(null, vehicleMovement))).GetDataObject(vehicleMovement);
			var arrivalTRWAddress = shipment.OrganizationAddressCollection?.FirstOrDefault(nameof(DocAddressType.ArrivalCFSAddress));
			var departureTRWAddress = shipment.OrganizationAddressCollection?.FirstOrDefault(nameof(DocAddressType.DepartureCFSAddress));

			CombineAssertions("Transit Warehouse address should be populated under both 'ArrivalCFSAddress' and 'DepartureCFSAddress'", () =>
			{
				AssertNotNull(arrivalTRWAddress);
				AssertNotNull(departureTRWAddress);
				AssertEquals("Expected both addresses to be the same", arrivalTRWAddress.ToString(), departureTRWAddress.ToString());
			});
		}

		public void TestGivenVehicleMovementWithWarehouseLocation_WhenGenerateUXML_ShouldRetainCorrectLength()
		{
			var expectedWarehouseLocationLength = 28;

			var warehouse = Helper.CreateWarehouse("WHS1");
			var row = Helper.CreateRowAndGenerateLocations(warehouse, "WEST YARD WAITING BAY B1", 2, 2);

			var vehicleMovement = Factory.NewWithValidTestData<GteVehicleMovement>();
			vehicleMovement.GVM_WL_Location = row.Locations.FirstOrDefault().PK;

			var gateMovement = Factory.NewWithValidTestData<GteGateMovement>();
			gateMovement.GGM_GVM_VehicleMovement = vehicleMovement.PK;

			var gateMovementBooking = gateMovement.GateMovementBooking;
			gateMovementBooking.GBM_SourceReferenceNumber = "VBS-001";

			var vehicleEntry = Factory.NewWithValidTestData<GteVehicleEntry>();
			vehicleEntry.GVE_IsIncoming = true;
			vehicleEntry.GVE_EntryTime = EntryTime;
			vehicleEntry.GVE_GVM_VehicleMovement = vehicleMovement.PK;
			vehicleEntry.GateLane.GLN_Code = "LNE";
			vehicleEntry.GateLane.Gate.GTE_Code = "GTE";

			var shipment = new GteVehicleMovementDataObjectWriter(new DataWritingManager(new ActionInfo(null, vehicleMovement))).GetDataObject(vehicleMovement);

			CombineAssertions(() =>
			{
				AssertEquals("Warehouse Location should match", "WEST YARD WAITING BAY B1-1-1", shipment.WarehouseLocation);
				AssertEquals("Warehouse Location length should be equal", expectedWarehouseLocationLength, shipment.WarehouseLocation.ToString().Length);
			});
		}
	}
}
