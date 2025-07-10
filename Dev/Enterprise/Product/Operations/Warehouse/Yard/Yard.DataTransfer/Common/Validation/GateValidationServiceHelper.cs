using System.Linq;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using static Enterprise.Core.Constants.GateManagementConstants;

namespace Enterprise.Warehouse.Yard.DataTransfer.Universal
{
	static class GateValidationServiceHelper
	{
		public static void ValidateTransportOrgAddress(Shipment dataObject)
		{
			if (dataObject.OrganizationAddressCollection.FirstOrDefault(nameof(DocAddressType.TransportCompanyDocumentaryAddress)) == null)
			{
				var errorMessage = Res.GetString("b9fcdb01-5460-4b35-9d77-209f47739c75", "Transport Company details are missing from UXML.");
				throw new DataObjectReadFailureException(errorMessage);
			}
		}

		public static void ValidateVehicle(Shipment dataObject)
		{
			var vehicle = dataObject.PreCarriageShipmentCollection?.FirstOrDefault()?.VehicleRun?.Vehicle ?? dataObject.VehicleRun?.Vehicle;
			if (vehicle == null || string.IsNullOrEmpty(vehicle.Registration?.Number))
			{
				var errorMessage = Res.GetString("a5323650-5fb8-45f9-8785-9e61d4e1c84d", "Vehicle Registration Number is not found.");
				throw new DataObjectReadFailureException(errorMessage);
			}
		}

		public static void ValidateWaitingBayLocation(Shipment dataObject)
		{
			if (string.IsNullOrEmpty(dataObject.WarehouseLocation))
			{
				var errorMessage = Res.GetString("0923387b-e888-4b71-ba9c-b9c3a959a4aa", "Yard location is missing from UXML.");
				throw new DataObjectReadFailureException(errorMessage);
			}
		}

		public static void ValidateContainerCollections(Shipment dataObject)
		{
			var containers = dataObject.SubShipmentCollection?.SelectMany(c => c.ContainerCollection);
			if (containers == null || !containers.Any())
			{
				var errorMessage = Res.GetString("e3ff2ddf-cf35-45f8-bd47-1fda0e1e472b", "There are no pickups and delivery details to import.");
				throw new DataObjectReadFailureException(errorMessage);
			}
		}

		public static void ValidateBookingConfirmationReference(Shipment dataObject)
		{
			foreach (var subShipment in dataObject.SubShipmentCollection)
			{
				if (string.IsNullOrEmpty(subShipment.BookingConfirmationReference))
				{
					var direction = subShipment.TransportBookingDirection.Code.GetValueOrDefault();
					if (direction == TransportBookingDirections.Codes.Delivery && !CYDTransportationUnitMatchingHelper.IsBooking(dataObject))
					{
						throw new DataObjectReadFailureException(Res.GetString("d9bd0f51-de3b-428a-be22-915f23d1fcca", "Booking Confirmation Reference is required in UXML for delivery."));
					}
					else if (direction == TransportBookingDirections.Codes.Pickup)
					{
						throw new DataObjectReadFailureException(Res.GetString("c07675f5-319f-4c4b-9407-78048b9b02b2", "Booking Confirmation Reference is required in UXML for pickup."));
					}
				}
			}
		}

		public static void ValidateContainerInfo(Shipment dataObject)
		{
			foreach (var subShipment in dataObject.SubShipmentCollection)
			{
				var direction = subShipment.TransportBookingDirection.Code.GetValueOrDefault();
				var container = subShipment.ContainerCollection.FirstOrDefault();

				if (direction == TransportBookingDirections.Codes.Delivery)
				{
					if (string.IsNullOrEmpty(container?.ContainerNumber))
					{
						throw new DataObjectReadFailureException(Res.GetString("02804a10-96d9-46ab-9f24-7569c0d4ecc1", "Container number is required in UXML for delivery."));
					}
				}
				else if (direction == TransportBookingDirections.Codes.Pickup)
				{
					if (string.IsNullOrEmpty(container?.ContainerType?.Code))
					{
						throw new DataObjectReadFailureException(Res.GetString("b819dbcf-2e54-4415-97cb-69357e9dd646", "Container type is required in UXML for pickup."));
					}
				}
			}
		}
	}
}
