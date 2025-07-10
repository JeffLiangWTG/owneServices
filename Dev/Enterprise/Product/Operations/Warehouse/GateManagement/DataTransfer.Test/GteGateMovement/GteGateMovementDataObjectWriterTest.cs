using System.Linq;
using CargoWise.Definitions;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.GateManagement.Business;
using static Enterprise.Core.Constants;

namespace Enterprise.Warehouse.GateManagement.DataTransfer.Test
{
	public class GteGateMovementDataObjectWriterTest : TestCaseWithFactory
	{
		public void TestGivenFullPickupGateMovement_WhenWriteToUXML_ThenAllFieldsSetCorrectly()
		{
			#region Setup test data

			var dock = Factory.NewWithValidTestData<WhsLocation>();
			dock.WLV_WA_PutawayArea = Factory.NewWithValidTestData<WhsArea>().PK;
			dock.Warehouse.WarehouseAddress.Address1 = "Address 1";
			dock.Warehouse.WarehouseAddress.Address2 = "Address 2";
			dock.Warehouse.WarehouseAddress.City = "SYD";
			dock.Warehouse.WarehouseAddress.Postcode = "0000";
			dock.Warehouse.WarehouseAddress.OA_Code = "BLANK SYD";
			dock.Warehouse.WarehouseAddress.Header.OH_Code = "ZZ";

			var vehicleMovement = Factory.NewWithValidTestData<GteVehicleMovement>();
			var gateMovement = Factory.NewWithValidTestData<GteGateMovement>();
			gateMovement.GGM_GVM_VehicleMovement = vehicleMovement.PK;
			gateMovement.GateMovementBooking.GBM_BookingReferenceNumber = "BKR-001";
			gateMovement.GateMovementBooking.GBM_SourceReferenceNumber = "VBS-001";
			gateMovement.GateMovementBooking.GBM_Source = "VBS";
			gateMovement.GGM_TransportReference = "TRF-001";
			gateMovement.GGM_IsPickup = true;
			gateMovement.GGM_WL_Dock = dock.PK;
			gateMovement.GGM_RH_NKCargoType = "AABT";
			gateMovement.GGM_F3_NKPackageType = "BAG";
			gateMovement.GGM_RC_UnitType = Factory.NewWithValidTestData<RefContainer>().PK;
			gateMovement.UnitType.RC_Code = "20FR";
			gateMovement.GGM_UnitNumber = "UNT-001";

			#endregion

			var shipment = new GteGateMovementDataObjectWriter(new DataWritingManager(new ActionInfo(null, vehicleMovement))).GetDataObject(gateMovement);
			var address = shipment.OrganizationAddressCollection?.FirstOrDefault(nameof(DocAddressType.LocalCartageYard));
			CombineAssertions(() =>
			{
				AssertEquals("BookingReferenceNumber should be set", gateMovement.GateMovementBooking.GBM_BookingReferenceNumber, shipment.BookingConfirmationReference);
				AssertEquals("SourceReferenceNumber should be set", gateMovement.GateMovementBooking.GBM_SourceReferenceNumber, shipment.GetAdditionalReferenceOrDefault(AdditionalReferenceTypes.Codes.BookingPartyReference));
				AssertEquals("TransportReference should be set", gateMovement.GGM_TransportReference, shipment.GetAdditionalReferenceOrDefault(AdditionalReferenceTypes.Codes.TransportReference));
				AssertEquals("IsPickup should be set", GateManagementConstants.TransportBookingDirections.Codes.Pickup, shipment.TransportBookingDirection.Code);
				AssertEquals("Dock should be set", gateMovement.Dock.ToLocationString(), shipment.WarehouseLocation);
				AssertEquals("Warehouse should be set", gateMovement.Dock.Warehouse.WarehouseAddress.OA_Code, address.AddressShortCode);
				AssertEquals("Warehouse should be set", gateMovement.Dock.Warehouse.WarehouseAddress.OA_Code, address.AddressShortCode);
				AssertEquals("Warehouse should be set", gateMovement.Dock.Warehouse.WarehouseAddress.Address1, address.Address1);
				AssertEquals("Warehouse should be set", gateMovement.Dock.Warehouse.WarehouseAddress.Address2, address.Address2);
				AssertEquals("Warehouse should be set", gateMovement.Dock.Warehouse.WarehouseAddress.City, address.City);
				AssertEquals("Warehouse should be set", gateMovement.Dock.Warehouse.WarehouseAddress.Postcode, address.Postcode);
				AssertEquals("Warehouse should be set", gateMovement.Dock.Warehouse.WarehouseAddress.Header.OH_Code, address.OrganizationCode);
				AssertEquals("CargoType should be set", gateMovement.GGM_RH_NKCargoType, shipment.PackingLineCollection[0].Commodity.Code);
				AssertEquals("PackType should be set", gateMovement.GGM_F3_NKPackageType, shipment.PackingLineCollection[0].PackType.Code);
				AssertEquals("UnitType should be set", gateMovement.UnitType.RC_Code, shipment.ContainerCollection[0].ContainerType.Code);
				AssertEquals("UnitNumber should be set", gateMovement.GGM_UnitNumber, shipment.ContainerCollection[0].ContainerNumber);
				AssertEquals("FacilityJobType should be set", FacilityJobType.Codes.Container, shipment.FacilityJobType.Code);
				AssertEquals("FacilityJobType should be set", FacilityJobType.Descriptions.Container, shipment.FacilityJobType.Description);
			});
		}

		public void TestGivenGateMovementWithoutBookingSourceReferenceNumber_WhenWriteToUXML_ThenAllFieldsSetCorrectly()
		{
			#region Setup test data

			var dock = Factory.NewWithValidTestData<WhsLocation>();
			dock.WLV_WA_PutawayArea = Factory.NewWithValidTestData<WhsArea>().PK;
			dock.WLV_WA_PutawayArea = Factory.NewWithValidTestData<WhsArea>().PK;
			dock.Warehouse.WarehouseAddress.Address1 = "Address 1";
			dock.Warehouse.WarehouseAddress.Address2 = "Address 2";
			dock.Warehouse.WarehouseAddress.City = "SYD";
			dock.Warehouse.WarehouseAddress.Postcode = "0000";
			dock.Warehouse.WarehouseAddress.OA_Code = "BLANK SYD";
			dock.Warehouse.WarehouseAddress.Header.OH_Code = "ZZ";

			var vehicleMovement = Factory.NewWithValidTestData<GteVehicleMovement>();
			var gateMovement = Factory.NewWithValidTestData<GteGateMovement>();
			gateMovement.GGM_GVM_VehicleMovement = vehicleMovement.PK;
			gateMovement.GGM_TransportReference = "TRF-001";
			gateMovement.GGM_IsPickup = true;
			gateMovement.GGM_WL_Dock = dock.PK;
			gateMovement.GGM_RH_NKCargoType = "AABT";
			gateMovement.GGM_F3_NKPackageType = "BAG";
			gateMovement.GGM_RC_UnitType = Factory.NewWithValidTestData<RefContainer>().PK;
			gateMovement.UnitType.RC_Code = "20FR";
			gateMovement.GGM_UnitNumber = "UNT-001";

			#endregion

			var shipment = new GteGateMovementDataObjectWriter(new DataWritingManager(new ActionInfo(null, vehicleMovement))).GetDataObject(gateMovement);
			var address = shipment.OrganizationAddressCollection?.FirstOrDefault(nameof(DocAddressType.LocalCartageYard));
			CombineAssertions(() =>
			{
				AssertEquals("SourceReferenceNumber should be null", null, shipment.GetAdditionalReferenceOrDefault(AdditionalReferenceTypes.Codes.BookingPartyReference));
				AssertEquals("TransportReference should be set", gateMovement.GGM_TransportReference, shipment.GetAdditionalReferenceOrDefault(AdditionalReferenceTypes.Codes.TransportReference));
				AssertEquals("IsPickup should be set", GateManagementConstants.TransportBookingDirections.Codes.Pickup, shipment.TransportBookingDirection.Code);
				AssertEquals("Dock should be set", gateMovement.Dock.ToLocationString(), shipment.WarehouseLocation);
				AssertEquals("Warehouse should be set", gateMovement.Dock.Warehouse.WarehouseAddress.OA_Code, address.AddressShortCode);
				AssertEquals("Warehouse should be set", gateMovement.Dock.Warehouse.WarehouseAddress.OA_Code, address.AddressShortCode);
				AssertEquals("Warehouse should be set", gateMovement.Dock.Warehouse.WarehouseAddress.Address1, address.Address1);
				AssertEquals("Warehouse should be set", gateMovement.Dock.Warehouse.WarehouseAddress.Address2, address.Address2);
				AssertEquals("Warehouse should be set", gateMovement.Dock.Warehouse.WarehouseAddress.City, address.City);
				AssertEquals("Warehouse should be set", gateMovement.Dock.Warehouse.WarehouseAddress.Postcode, address.Postcode);
				AssertEquals("Warehouse should be set", gateMovement.Dock.Warehouse.WarehouseAddress.Header.OH_Code, address.OrganizationCode);
				AssertEquals("CargoType should be set", gateMovement.GGM_RH_NKCargoType, shipment.PackingLineCollection[0].Commodity.Code);
				AssertEquals("PackType should be set", gateMovement.GGM_F3_NKPackageType, shipment.PackingLineCollection[0].PackType.Code);
				AssertEquals("UnitType should be set", gateMovement.UnitType.RC_Code, shipment.ContainerCollection[0].ContainerType.Code);
				AssertEquals("UnitNumber should be set", gateMovement.GGM_UnitNumber, shipment.ContainerCollection[0].ContainerNumber);
				AssertEquals("FacilityJobType should be set", FacilityJobType.Codes.Container, shipment.FacilityJobType.Code);
				AssertEquals("FacilityJobType should be set", FacilityJobType.Descriptions.Container, shipment.FacilityJobType.Description);
			});
		}

		public void TestGivenGateMovementWithoutDock_WhenWriteToUXML_ThenAllFieldsSetCorrectly()
		{
			#region Setup test data

			var vehicleMovement = Factory.NewWithValidTestData<GteVehicleMovement>();
			var gateMovement = Factory.NewWithValidTestData<GteGateMovement>();
			gateMovement.GGM_GVM_VehicleMovement = vehicleMovement.PK;
			gateMovement.GateMovementBooking.GBM_SourceReferenceNumber = "VBS-001";
			gateMovement.GateMovementBooking.GBM_Source = "VBS";
			gateMovement.GGM_TransportReference = "TRF-001";
			gateMovement.GGM_IsPickup = true;
			gateMovement.GGM_RH_NKCargoType = "AABT";
			gateMovement.GGM_F3_NKPackageType = "BAG";
			gateMovement.GGM_RC_UnitType = Factory.NewWithValidTestData<RefContainer>().PK;
			gateMovement.UnitType.RC_Code = "20FR";
			gateMovement.GGM_UnitNumber = "UNT-001";

			#endregion

			var shipment = new GteGateMovementDataObjectWriter(new DataWritingManager(new ActionInfo(null, vehicleMovement))).GetDataObject(gateMovement);
			var warehouse = shipment.OrganizationAddressCollection?.FirstOrDefault(nameof(DocAddressType.LocalCartageYard));
			CombineAssertions(() =>
			{
				AssertEquals("SourceReferenceNumber should be set", gateMovement.GateMovementBooking.GBM_SourceReferenceNumber, shipment.GetAdditionalReferenceOrDefault(AdditionalReferenceTypes.Codes.BookingPartyReference));
				AssertEquals("TransportReference should be set", gateMovement.GGM_TransportReference, shipment.GetAdditionalReferenceOrDefault(AdditionalReferenceTypes.Codes.TransportReference));
				AssertEquals("IsPickup should be set", GateManagementConstants.TransportBookingDirections.Codes.Pickup, shipment.TransportBookingDirection.Code);
				AssertEquals("Dock should be null", null, shipment.WarehouseLocation);
				AssertEquals("Warehouse should be null", null, warehouse);
				AssertEquals("CargoType should be set", gateMovement.GGM_RH_NKCargoType, shipment.PackingLineCollection[0].Commodity.Code);
				AssertEquals("PackType should be set", gateMovement.GGM_F3_NKPackageType, shipment.PackingLineCollection[0].PackType.Code);
				AssertEquals("UnitType should be set", gateMovement.UnitType.RC_Code, shipment.ContainerCollection[0].ContainerType.Code);
				AssertEquals("UnitNumber should be set", gateMovement.GGM_UnitNumber, shipment.ContainerCollection[0].ContainerNumber);
				AssertEquals("FacilityJobType should be set", FacilityJobType.Codes.Container, shipment.FacilityJobType.Code);
				AssertEquals("FacilityJobType should be set", FacilityJobType.Descriptions.Container, shipment.FacilityJobType.Description);
			});
		}

		public void TestGivenGateMovementWithoutUnitTypeOrUnitNumber_WhenWriteToUXML_ThenAllFieldsSetCorrectly()
		{
			#region Setup test data

			var dock = Factory.NewWithValidTestData<WhsLocation>();
			dock.WLV_WA_PutawayArea = Factory.NewWithValidTestData<WhsArea>().PK;
			dock.WLV_WA_PutawayArea = Factory.NewWithValidTestData<WhsArea>().PK;
			dock.Warehouse.WarehouseAddress.Address1 = "Address 1";
			dock.Warehouse.WarehouseAddress.Address2 = "Address 2";
			dock.Warehouse.WarehouseAddress.City = "SYD";
			dock.Warehouse.WarehouseAddress.Postcode = "0000";
			dock.Warehouse.WarehouseAddress.OA_Code = "BLANK SYD";
			dock.Warehouse.WarehouseAddress.Header.OH_Code = "ZZ";

			var vehicleMovement = Factory.NewWithValidTestData<GteVehicleMovement>();
			var gateMovement = Factory.NewWithValidTestData<GteGateMovement>();
			gateMovement.GGM_GVM_VehicleMovement = vehicleMovement.PK;
			gateMovement.GateMovementBooking.GBM_SourceReferenceNumber = "VBS-001";
			gateMovement.GateMovementBooking.GBM_Source = "VBS";
			gateMovement.GGM_TransportReference = "TRF-001";
			gateMovement.GGM_IsPickup = true;
			gateMovement.GGM_WL_Dock = dock.PK;
			gateMovement.GGM_RH_NKCargoType = "AABT";
			gateMovement.GGM_F3_NKPackageType = "BAG";

			#endregion

			var shipment = new GteGateMovementDataObjectWriter(new DataWritingManager(new ActionInfo(null, vehicleMovement))).GetDataObject(gateMovement);
			var address = shipment.OrganizationAddressCollection?.FirstOrDefault(nameof(DocAddressType.LocalCartageYard));
			CombineAssertions(() =>
			{
				AssertEquals("SourceReferenceNumber should be set", gateMovement.GateMovementBooking.GBM_SourceReferenceNumber, shipment.GetAdditionalReferenceOrDefault(AdditionalReferenceTypes.Codes.BookingPartyReference));
				AssertEquals("TransportReference should be set", gateMovement.GGM_TransportReference, shipment.GetAdditionalReferenceOrDefault(AdditionalReferenceTypes.Codes.TransportReference));
				AssertEquals("IsPickup should be set", GateManagementConstants.TransportBookingDirections.Codes.Pickup, shipment.TransportBookingDirection.Code);
				AssertEquals("Dock should be set", gateMovement.Dock.ToLocationString(), shipment.WarehouseLocation);
				AssertEquals("Warehouse should be set", gateMovement.Dock.Warehouse.WarehouseAddress.OA_Code, address.AddressShortCode);
				AssertEquals("Warehouse should be set", gateMovement.Dock.Warehouse.WarehouseAddress.OA_Code, address.AddressShortCode);
				AssertEquals("Warehouse should be set", gateMovement.Dock.Warehouse.WarehouseAddress.Address1, address.Address1);
				AssertEquals("Warehouse should be set", gateMovement.Dock.Warehouse.WarehouseAddress.Address2, address.Address2);
				AssertEquals("Warehouse should be set", gateMovement.Dock.Warehouse.WarehouseAddress.City, address.City);
				AssertEquals("Warehouse should be set", gateMovement.Dock.Warehouse.WarehouseAddress.Postcode, address.Postcode);
				AssertEquals("Warehouse should be set", gateMovement.Dock.Warehouse.WarehouseAddress.Header.OH_Code, address.OrganizationCode);
				AssertEquals("CargoType should be set", gateMovement.GGM_RH_NKCargoType, shipment.PackingLineCollection[0].Commodity.Code);
				AssertEquals("PackType should be set", gateMovement.GGM_F3_NKPackageType, shipment.PackingLineCollection[0].PackType.Code);
				AssertEquals("ContainerCollection should be null", null, shipment.ContainerCollection);
				AssertEquals("FacilityJobType should be set", FacilityJobType.Codes.Cargo, shipment.FacilityJobType.Code);
				AssertEquals("FacilityJobType should be set", FacilityJobType.Descriptions.Cargo, shipment.FacilityJobType.Description);
			});
		}

		public void TestGivenGateMovementWithoutUnitType_WhenWriteToUXML_ThenAllFieldsSetCorrectly()
		{
			#region Setup test data

			var dock = Factory.NewWithValidTestData<WhsLocation>();
			dock.WLV_WA_PutawayArea = Factory.NewWithValidTestData<WhsArea>().PK;
			dock.WLV_WA_PutawayArea = Factory.NewWithValidTestData<WhsArea>().PK;
			dock.Warehouse.WarehouseAddress.Address1 = "Address 1";
			dock.Warehouse.WarehouseAddress.Address2 = "Address 2";
			dock.Warehouse.WarehouseAddress.City = "SYD";
			dock.Warehouse.WarehouseAddress.Postcode = "0000";
			dock.Warehouse.WarehouseAddress.OA_Code = "BLANK SYD";
			dock.Warehouse.WarehouseAddress.Header.OH_Code = "ZZ";

			var vehicleMovement = Factory.NewWithValidTestData<GteVehicleMovement>();
			var gateMovement = Factory.NewWithValidTestData<GteGateMovement>();
			gateMovement.GGM_GVM_VehicleMovement = vehicleMovement.PK;
			gateMovement.GateMovementBooking.GBM_SourceReferenceNumber = "VBS-001";
			gateMovement.GateMovementBooking.GBM_Source = "VBS";
			gateMovement.GGM_TransportReference = "TRF-001";
			gateMovement.GGM_IsPickup = true;
			gateMovement.GGM_WL_Dock = dock.PK;
			gateMovement.GGM_RH_NKCargoType = "AABT";
			gateMovement.GGM_F3_NKPackageType = "BAG";
			gateMovement.GGM_UnitNumber = "UNT-001";

			#endregion

			var shipment = new GteGateMovementDataObjectWriter(new DataWritingManager(new ActionInfo(null, vehicleMovement))).GetDataObject(gateMovement);
			var address = shipment.OrganizationAddressCollection?.FirstOrDefault(nameof(DocAddressType.LocalCartageYard));
			CombineAssertions(() =>
			{
				AssertEquals("SourceReferenceNumber should be set", gateMovement.GateMovementBooking.GBM_SourceReferenceNumber, shipment.GetAdditionalReferenceOrDefault(AdditionalReferenceTypes.Codes.BookingPartyReference));
				AssertEquals("TransportReference should be set", gateMovement.GGM_TransportReference, shipment.GetAdditionalReferenceOrDefault(AdditionalReferenceTypes.Codes.TransportReference));
				AssertEquals("IsPickup should be set", GateManagementConstants.TransportBookingDirections.Codes.Pickup, shipment.TransportBookingDirection.Code);
				AssertEquals("Dock should be set", gateMovement.Dock.ToLocationString(), shipment.WarehouseLocation);
				AssertEquals("Warehouse should be set", gateMovement.Dock.Warehouse.WarehouseAddress.OA_Code, address.AddressShortCode);
				AssertEquals("Warehouse should be set", gateMovement.Dock.Warehouse.WarehouseAddress.OA_Code, address.AddressShortCode);
				AssertEquals("Warehouse should be set", gateMovement.Dock.Warehouse.WarehouseAddress.Address1, address.Address1);
				AssertEquals("Warehouse should be set", gateMovement.Dock.Warehouse.WarehouseAddress.Address2, address.Address2);
				AssertEquals("Warehouse should be set", gateMovement.Dock.Warehouse.WarehouseAddress.City, address.City);
				AssertEquals("Warehouse should be set", gateMovement.Dock.Warehouse.WarehouseAddress.Postcode, address.Postcode);
				AssertEquals("Warehouse should be set", gateMovement.Dock.Warehouse.WarehouseAddress.Header.OH_Code, address.OrganizationCode);
				AssertEquals("CargoType should be set", gateMovement.GGM_RH_NKCargoType, shipment.PackingLineCollection[0].Commodity.Code);
				AssertEquals("PackType should be set", gateMovement.GGM_F3_NKPackageType, shipment.PackingLineCollection[0].PackType.Code);
				AssertEquals("UnitType should be null", null, shipment.ContainerCollection[0].ContainerType);
				AssertEquals("UnitNumber should be set", gateMovement.GGM_UnitNumber, shipment.ContainerCollection[0].ContainerNumber);
				AssertEquals("FacilityJobType should be set", FacilityJobType.Codes.Cargo, shipment.FacilityJobType.Code);
				AssertEquals("FacilityJobType should be set", FacilityJobType.Descriptions.Cargo, shipment.FacilityJobType.Description);
			});
		}

		public void TestGivenGateMovementWithoutUnitNumber_WhenWriteToUXML_ThenAllFieldsSetCorrectly()
		{
			#region Setup test data

			var dock = Factory.NewWithValidTestData<WhsLocation>();
			dock.WLV_WA_PutawayArea = Factory.NewWithValidTestData<WhsArea>().PK;
			dock.WLV_WA_PutawayArea = Factory.NewWithValidTestData<WhsArea>().PK;
			dock.Warehouse.WarehouseAddress.Address1 = "Address 1";
			dock.Warehouse.WarehouseAddress.Address2 = "Address 2";
			dock.Warehouse.WarehouseAddress.City = "SYD";
			dock.Warehouse.WarehouseAddress.Postcode = "0000";
			dock.Warehouse.WarehouseAddress.OA_Code = "BLANK SYD";
			dock.Warehouse.WarehouseAddress.Header.OH_Code = "ZZ";

			var vehicleMovement = Factory.NewWithValidTestData<GteVehicleMovement>();
			var gateMovement = Factory.NewWithValidTestData<GteGateMovement>();
			gateMovement.GGM_GVM_VehicleMovement = vehicleMovement.PK;
			gateMovement.GateMovementBooking.GBM_SourceReferenceNumber = "VBS-001";
			gateMovement.GateMovementBooking.GBM_Source = "VBS";
			gateMovement.GGM_TransportReference = "TRF-001";
			gateMovement.GGM_IsPickup = true;
			gateMovement.GGM_WL_Dock = dock.PK;
			gateMovement.GGM_RH_NKCargoType = "AABT";
			gateMovement.GGM_F3_NKPackageType = "BAG";
			gateMovement.GGM_RC_UnitType = Factory.NewWithValidTestData<RefContainer>().PK;
			gateMovement.UnitType.RC_Code = "20FR";

			#endregion

			var shipment = new GteGateMovementDataObjectWriter(new DataWritingManager(new ActionInfo(null, vehicleMovement))).GetDataObject(gateMovement);
			var address = shipment.OrganizationAddressCollection?.FirstOrDefault(nameof(DocAddressType.LocalCartageYard));
			CombineAssertions(() =>
			{
				AssertEquals("SourceReferenceNumber should be set", gateMovement.GateMovementBooking.GBM_SourceReferenceNumber, shipment.GetAdditionalReferenceOrDefault(AdditionalReferenceTypes.Codes.BookingPartyReference));
				AssertEquals("TransportReference should be set", gateMovement.GGM_TransportReference, shipment.GetAdditionalReferenceOrDefault(AdditionalReferenceTypes.Codes.TransportReference));
				AssertEquals("IsPickup should be set", GateManagementConstants.TransportBookingDirections.Codes.Pickup, shipment.TransportBookingDirection.Code);
				AssertEquals("Dock should be set", gateMovement.Dock.ToLocationString(), shipment.WarehouseLocation);
				AssertEquals("Warehouse should be set", gateMovement.Dock.Warehouse.WarehouseAddress.OA_Code, address.AddressShortCode);
				AssertEquals("Warehouse should be set", gateMovement.Dock.Warehouse.WarehouseAddress.OA_Code, address.AddressShortCode);
				AssertEquals("Warehouse should be set", gateMovement.Dock.Warehouse.WarehouseAddress.Address1, address.Address1);
				AssertEquals("Warehouse should be set", gateMovement.Dock.Warehouse.WarehouseAddress.Address2, address.Address2);
				AssertEquals("Warehouse should be set", gateMovement.Dock.Warehouse.WarehouseAddress.City, address.City);
				AssertEquals("Warehouse should be set", gateMovement.Dock.Warehouse.WarehouseAddress.Postcode, address.Postcode);
				AssertEquals("Warehouse should be set", gateMovement.Dock.Warehouse.WarehouseAddress.Header.OH_Code, address.OrganizationCode);
				AssertEquals("CargoType should be set", gateMovement.GGM_RH_NKCargoType, shipment.PackingLineCollection[0].Commodity.Code);
				AssertEquals("PackType should be set", gateMovement.GGM_F3_NKPackageType, shipment.PackingLineCollection[0].PackType.Code);
				AssertEquals("UnitType should be set", gateMovement.UnitType.RC_Code, shipment.ContainerCollection[0].ContainerType.Code);
				AssertEquals("UnitNumber should be null", null, shipment.ContainerCollection[0].ContainerNumber);
				AssertEquals("FacilityJobType should be set", FacilityJobType.Codes.Cargo, shipment.FacilityJobType.Code);
				AssertEquals("FacilityJobType should be set", FacilityJobType.Descriptions.Cargo, shipment.FacilityJobType.Description);
			});
		}

		public void TestGivenGateMovementWithBlankValues_WhenWriteToUXML_ThenAllFieldsSetCorrectly()
		{
			#region Setup test data

			var dock = Factory.NewWithValidTestData<WhsLocation>();
			dock.WLV_WA_PutawayArea = Factory.NewWithValidTestData<WhsArea>().PK;
			dock.WLV_WA_PutawayArea = Factory.NewWithValidTestData<WhsArea>().PK;
			dock.Warehouse.WarehouseAddress.Address1 = "Address 1";
			dock.Warehouse.WarehouseAddress.Address2 = "Address 2";
			dock.Warehouse.WarehouseAddress.City = "SYD";
			dock.Warehouse.WarehouseAddress.Postcode = "0000";
			dock.Warehouse.WarehouseAddress.OA_Code = "BLANK SYD";
			dock.Warehouse.WarehouseAddress.Header.OH_Code = "ZZ";

			var vehicleMovement = Factory.NewWithValidTestData<GteVehicleMovement>();
			var gateMovement = Factory.NewWithValidTestData<GteGateMovement>();
			gateMovement.GGM_GVM_VehicleMovement = vehicleMovement.PK;
			gateMovement.GateMovementBooking.GBM_BookingReferenceNumber = "";
			gateMovement.GateMovementBooking.GBM_SourceReferenceNumber = "";
			gateMovement.GateMovementBooking.GBM_Source = "";
			gateMovement.GGM_TransportReference = "";
			gateMovement.GGM_IsPickup = true;
			gateMovement.GGM_WL_Dock = dock.PK;
			gateMovement.GGM_RH_NKCargoType = "";
			gateMovement.GGM_F3_NKPackageType = "";
			gateMovement.GGM_RC_UnitType = Factory.NewWithValidTestData<RefContainer>().PK;
			gateMovement.UnitType.RC_Code = "20FR";
			gateMovement.GGM_UnitNumber = "";

			#endregion

			var shipment = new GteGateMovementDataObjectWriter(new DataWritingManager(new ActionInfo(null, vehicleMovement))).GetDataObject(gateMovement);
			var address = shipment.OrganizationAddressCollection?.FirstOrDefault(nameof(DocAddressType.LocalCartageYard));
			CombineAssertions(() =>
			{
				AssertEquals("BookingReferenceNumber should be null", null, shipment.BookingConfirmationReference);
				AssertEquals("SourceReferenceNumber should be null", null, shipment.GetAdditionalReferenceOrDefault(AdditionalReferenceTypes.Codes.BookingPartyReference));
				AssertEquals("TransportReference should be null", null, shipment.GetAdditionalReferenceOrDefault(AdditionalReferenceTypes.Codes.TransportReference));
				AssertEquals("IsPickup should be set", GateManagementConstants.TransportBookingDirections.Codes.Pickup, shipment.TransportBookingDirection.Code);
				AssertEquals("Dock should be set", gateMovement.Dock.ToLocationString(), shipment.WarehouseLocation);
				AssertEquals("Warehouse should be set", gateMovement.Dock.Warehouse.WarehouseAddress.OA_Code, address.AddressShortCode);
				AssertEquals("Warehouse should be set", gateMovement.Dock.Warehouse.WarehouseAddress.OA_Code, address.AddressShortCode);
				AssertEquals("Warehouse should be set", gateMovement.Dock.Warehouse.WarehouseAddress.Address1, address.Address1);
				AssertEquals("Warehouse should be set", gateMovement.Dock.Warehouse.WarehouseAddress.Address2, address.Address2);
				AssertEquals("Warehouse should be set", gateMovement.Dock.Warehouse.WarehouseAddress.City, address.City);
				AssertEquals("Warehouse should be set", gateMovement.Dock.Warehouse.WarehouseAddress.Postcode, address.Postcode);
				AssertEquals("Warehouse should be set", gateMovement.Dock.Warehouse.WarehouseAddress.Header.OH_Code, address.OrganizationCode);
				AssertEquals("CargoType should be null", null, shipment.PackingLineCollection);
				AssertEquals("PackType should be null", null, shipment.PackingLineCollection);
				AssertEquals("UnitType should be set", gateMovement.UnitType.RC_Code, shipment.ContainerCollection[0].ContainerType.Code);
				AssertEquals("UnitNumber should be null", null, shipment.ContainerCollection[0].ContainerNumber);
				AssertEquals("FacilityJobType should be set", FacilityJobType.Codes.Cargo, shipment.FacilityJobType.Code);
				AssertEquals("FacilityJobType should be set", FacilityJobType.Descriptions.Cargo, shipment.FacilityJobType.Description);
			});
		}

		public void TestGivenGateMovementWithSubShipment_WhenWriteToUXML_ThenDataSourceAndKeySetCorrectly()
		{
			#region Setup test data

			var booking = Factory.NewWithValidTestData<GteBooking>();
			booking.GBK_ReferenceNumber = "GB0001000";

			var gateMovementBooking = Factory.NewWithValidTestData<GteGateMovementBooking>();
			gateMovementBooking.GBM_GBK_Booking = booking.PK;
			gateMovementBooking.GBM_BookingReferenceNumber = "Test Shipment 1";
			gateMovementBooking.GBM_MovementBookingNumber = "GBM0001";

			var vehicleMovement = Factory.NewWithValidTestData<GteVehicleMovement>();
			var gateMovement = Factory.NewWithValidTestData<GteGateMovement>();
			gateMovement.GGM_GVM_VehicleMovement = vehicleMovement.PK;
			gateMovement.GGM_GBM_MovementBooking = gateMovementBooking.PK;

			#endregion

			var shipment = new GteGateMovementDataObjectWriter(new DataWritingManager(new ActionInfo(null, vehicleMovement))).GetDataObject(gateMovement);
			CombineAssertions(() =>
			{
				AssertEquals("Shipment should have three data sources", 3, shipment.DataContext.DataSourceCollection.Count());
				AssertEquals("First Data source type of Shipment should be GateMovementBooking", nameof(DataContextType.GateMovementBooking), shipment.DataContext.DataSourceCollection.FirstOrDefault().Type);
				AssertEquals("First Data source key of Shipment should be set", "GBM0001", shipment.DataContext.DataSourceCollection.FirstOrDefault().Key);
				AssertEquals("Second Data source type of Shipment should be GateMovement", nameof(DataContextType.GateMovement), shipment.DataContext.DataSourceCollection.ElementAtOrDefault(1).Type);
				AssertEquals("Second Data source key of Shipment should be set", "GBM0001", shipment.DataContext.DataSourceCollection.ElementAtOrDefault(1).Key);
				AssertEquals("Third Data source type of Shipment should be GateBooking", nameof(DataContextType.GateBooking), shipment.DataContext.DataSourceCollection.ElementAtOrDefault(2).Type);
				AssertEquals("Third Data source key of Shipment should be set", "GB0001000", shipment.DataContext.DataSourceCollection.ElementAtOrDefault(2).Key);
			});
		}

		public void TestGivenGateMovementWithMultipleSubShipments_WhenWriteToUXML_ThenDataSourceAndKeySetCorrectly()
		{
			#region Setup test data

			var booking = Factory.NewWithValidTestData<GteBooking>();
			booking.GBK_ReferenceNumber = "GB0001000";

			var gateMovementBooking = Factory.NewWithValidTestData<GteGateMovementBooking>();
			gateMovementBooking.GBM_GBK_Booking = booking.PK;
			gateMovementBooking.GBM_BookingReferenceNumber = "Test Shipment 1";
			gateMovementBooking.GBM_MovementBookingNumber = "GBM0001";

			var gateMovementBooking2 = Factory.NewWithValidTestData<GteGateMovementBooking>();
			gateMovementBooking2.GBM_GBK_Booking = booking.PK;
			gateMovementBooking2.GBM_BookingReferenceNumber = "Test Shipment 2";
			gateMovementBooking2.GBM_MovementBookingNumber = "GBM0002";

			var vehicleMovement = Factory.NewWithValidTestData<GteVehicleMovement>();
			var gateMovement = Factory.NewWithValidTestData<GteGateMovement>();
			gateMovement.GGM_GVM_VehicleMovement = vehicleMovement.PK;
			gateMovement.GGM_GBM_MovementBooking = gateMovementBooking.PK;

			var gateMovement2 = Factory.NewWithValidTestData<GteGateMovement>();
			gateMovement2.GGM_GVM_VehicleMovement = vehicleMovement.PK;
			gateMovement2.GGM_GBM_MovementBooking = gateMovementBooking2.PK;

			#endregion

			var gateMovements = vehicleMovement.GateMovements;
			var shipment1 = new GteGateMovementDataObjectWriter(new DataWritingManager(new ActionInfo(null, vehicleMovement))).GetDataObject(gateMovements.FirstOrDefault(s => s.GateMovementBooking.GBM_MovementBookingNumber == "GBM0001"));
			var shipment2 = new GteGateMovementDataObjectWriter(new DataWritingManager(new ActionInfo(null, vehicleMovement))).GetDataObject(gateMovements.FirstOrDefault(s => s.GateMovementBooking.GBM_MovementBookingNumber == "GBM0002"));

			CombineAssertions(() =>
			{
				AssertEquals("Vehicle Movement should have multiple gate movements", 2, gateMovements.Count);
				AssertEquals("Shipment1 should have three data sources", 3, shipment1.DataContext.DataSourceCollection.Count());
				AssertEquals("First Data source type of Shipment1 should be GateMovementBooking", nameof(DataContextType.GateMovementBooking), shipment1.DataContext.DataSourceCollection.FirstOrDefault().Type);
				AssertEquals("First Data source key of Test Shipment1 should be set", "GBM0001", shipment1.DataContext.DataSourceCollection.FirstOrDefault().Key);
				AssertEquals("Second Data source type of Shipment1 should be GateMovement", nameof(DataContextType.GateMovement), shipment1.DataContext.DataSourceCollection.ElementAtOrDefault(1).Type);
				AssertEquals("Second Data source key of Test Shipment1 should be set", "GBM0001", shipment1.DataContext.DataSourceCollection.ElementAtOrDefault(1).Key);
				AssertEquals("Third Data source type of Shipment1 should be GateBooking", nameof(DataContextType.GateBooking), shipment1.DataContext.DataSourceCollection.ElementAtOrDefault(2).Type);
				AssertEquals("Third Data source key of Test Shipment1 should be set", "GB0001000", shipment1.DataContext.DataSourceCollection.ElementAtOrDefault(2).Key);

				AssertEquals("Shipment2 should have three data sources", 3, shipment2.DataContext.DataSourceCollection.Count());
				AssertEquals("Data source type of Test Shipment2 should be GateMovementBooking", nameof(DataContextType.GateMovementBooking), shipment2.DataContext.DataSourceCollection.FirstOrDefault().Type);
				AssertEquals("Data source key of Test Shipment2 should be set", "GBM0002", shipment2.DataContext.DataSourceCollection.FirstOrDefault().Key);
				AssertEquals("Second Data source type of Shipment2 should be GateMovement", nameof(DataContextType.GateMovement), shipment2.DataContext.DataSourceCollection.ElementAtOrDefault(1).Type);
				AssertEquals("Second Data source key of Test Shipment2 should be set", "GBM0002", shipment2.DataContext.DataSourceCollection.ElementAtOrDefault(1).Key);
				AssertEquals("Third Data source type of Shipment1 should be GateBooking", nameof(DataContextType.GateBooking), shipment1.DataContext.DataSourceCollection.ElementAtOrDefault(2).Type);
				AssertEquals("Third Data source key of Test Shipment1 should be set", "GB0001000", shipment1.DataContext.DataSourceCollection.ElementAtOrDefault(2).Key);
			});
		}
	}
}
