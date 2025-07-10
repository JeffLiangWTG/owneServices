using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.Warehouse.Yard.DataTransfer.Universal.Test;
using static Enterprise.Core.Constants.GateManagementConstants;

namespace Enterprise.Warehouse.Yard.DataTransfer.Test
{
	public class CYDGateFacilityValidationServiceTest : TestCaseWithFactory
	{
		public void TestValidation_AllPassed()
		{
			var shipment = UniversalTestShipmentBuilder.GetShipment(new TestShipmentOptions { IsDataSourceGateVehicleMovement = true, IsIncoming = true, IsForPickup = false });

			shipment.SetContainerCollection(() => new DataObjectList<Container>()
			{
				new Container() { ContainerNumber = "CON1", ContainerType = new ContainerType() { Code = "LD-1" }, IsEmptyContainer = true }
			});
			shipment.SetContainerCollection(() => new DataObjectList<Container>()
			{
				new Container() { ContainerNumber = "CON1", ContainerType = new ContainerType() { Code = "LD-2" }, IsEmptyContainer = true }
			});

			var validationProvider = new CYDGateFacilityValidationService();
			var errorMsg = validationProvider.ValidateMovement(shipment, FacilityValidationTypes.GateIn);
			AssertEquals(0, errorMsg.Length);
		}

		public void TestValidation_MissedTransportOrgAddress()
		{
			var shipment = UniversalTestShipmentBuilder.GetShipment(new TestShipmentOptions { IsDataSourceGateVehicleMovement = true, IsIncoming = true, BookingPartyIsNotFound = true });

			var validationProvider = new CYDGateFacilityValidationService();
			var errorMsg = validationProvider.ValidateMovement(shipment, FacilityValidationTypes.GateIn);
			AssertEquals("Transport Company details are missing from UXML.", errorMsg.Single());
		}

		public void TestValidation_MissedVehicle()
		{
			var shipment = UniversalTestShipmentBuilder.GetShipment(new TestShipmentOptions { IsDataSourceGateVehicleMovement = true, IsIncoming = true, VehicleNumbers = [] });

			var validationProvider = new CYDGateFacilityValidationService();
			var errorMsg = validationProvider.ValidateMovement(shipment, FacilityValidationTypes.GateIn);
			AssertEquals("Vehicle Registration Number is not found.", errorMsg.Single());
		}

		public void TestValidation_MissedWaitingBayLocation()
		{
			var shipment = UniversalTestShipmentBuilder.GetShipment(new TestShipmentOptions { IsDataSourceGateVehicleMovement = true, IsIncoming = true, WarehouseLocation = string.Empty });

			var validationProvider = new CYDGateFacilityValidationService();
			var errorMsg = validationProvider.ValidateMovement(shipment, FacilityValidationTypes.GateIn);
			AssertEquals("Yard location is missing from UXML.", errorMsg.Single());
		}

		public void TestValidation_MissedBookingConfirmationReference_Delivery()
		{
			var shipment = UniversalTestShipmentBuilder.GetShipment(new TestShipmentOptions { IsDataSourceGateVehicleMovement = true, IsIncoming = true, IsForPickup = false, BookingConfirmationReference = string.Empty });

			var validationProvider = new CYDGateFacilityValidationService();
			var errorMsg = validationProvider.ValidateMovement(shipment, FacilityValidationTypes.GateIn);
			AssertEquals("Booking Confirmation Reference is required in UXML for delivery.", errorMsg.Single());
		}

		public void TestValidation_MissedBookingConfirmationReference_Pickup()
		{
			var shipment = UniversalTestShipmentBuilder.GetShipment(new TestShipmentOptions { IsDataSourceGateVehicleMovement = true, IsIncoming = true, IsForPickup = true, BookingConfirmationReference = string.Empty });

			var validationProvider = new CYDGateFacilityValidationService();
			var errorMsg = validationProvider.ValidateMovement(shipment, FacilityValidationTypes.GateIn);
			AssertEquals("Booking Confirmation Reference is required in UXML for pickup.", errorMsg.Single());
		}

		public void TestValidation_MissedContainerCollections()
		{
			var shipment = UniversalTestShipmentBuilder.GetShipment(new TestShipmentOptions { IsDataSourceGateVehicleMovement = true, IsIncoming = true });
			shipment.SubShipmentCollection.First().SetContainerCollection(() => new DataObjectList<Container>());

			var validationProvider = new CYDGateFacilityValidationService();
			var errorMsg = validationProvider.ValidateMovement(shipment, FacilityValidationTypes.GateIn);
			AssertEquals("There are no pickups and delivery details to import.", errorMsg.First());
		}

		public void TestValidation_MissedContainerInfo_ContainerNumber()
		{
			var shipment = UniversalTestShipmentBuilder.GetShipment(new TestShipmentOptions { IsDataSourceGateVehicleMovement = true, IsIncoming = true, ContainerNumbers = [""] });

			var validationProvider = new CYDGateFacilityValidationService();
			var errorMsg = validationProvider.ValidateMovement(shipment, FacilityValidationTypes.GateIn);
			AssertEquals("Container number is required in UXML for delivery.", errorMsg.Single());
		}

		public void TestValidation_MissedContainerInfo_ContainerType()
		{
			var shipment = UniversalTestShipmentBuilder.GetShipment(new TestShipmentOptions { IsDataSourceGateVehicleMovement = true, IsIncoming = true, IsForPickup = true, ContainerTypeCode = string.Empty });

			var validationProvider = new CYDGateFacilityValidationService();
			var errorMsg = validationProvider.ValidateMovement(shipment, FacilityValidationTypes.GateIn);
			AssertEquals("Container type is required in UXML for pickup.", errorMsg.Single());
		}
	}
}
