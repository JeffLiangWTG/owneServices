using CargoWise.Definitions;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.Warehouse.GateManagement.Business;
using static Enterprise.Core.Constants;
using GateManagementConstants = Enterprise.Core.Constants.GateManagementConstants;
using TransportDirections = Enterprise.Core.Constants.GateManagementConstants.TransportBookingDirections;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Warehouse.GateManagement.DataTransfer
{
	public class GteGateMovementDataObjectWriter : DataObjectWriter<GteGateMovement, UniversalShipment>
	{
		public GteGateMovementDataObjectWriter(IDataWritingManager writeManager) : base(writeManager)
		{
		}

		protected override UniversalShipment PopulateDataObject(GteGateMovement gateMovement)
		{
			var shipment = new UniversalShipment(writeManager.WriterStrategy);
			shipment.DataContext = DataContextFactory.New();

			PopulateDataSourceCollection(gateMovement, shipment);
			PopulateBookingConfirmationReference(gateMovement, shipment);
			PopulateAdditionalReferenceCollection(gateMovement, shipment);
			PopulateTransportBookingDirection(gateMovement, shipment);
			PopulateWarehouseLocation(gateMovement, shipment);
			PopulatePackingLineCollection(gateMovement, shipment);
			PopulateContainerCollection(gateMovement, shipment);

			return shipment;
		}

		void PopulateDataSourceCollection(GteGateMovement gateMovement, UniversalShipment shipment)
		{
			if (!string.IsNullOrEmpty(gateMovement.GateMovementBooking.GBM_MovementBookingNumber))
			{
				shipment.DataContext.AddDataSource(DataContextType.GateMovementBooking, gateMovement.GateMovementBooking.GBM_MovementBookingNumber);
				shipment.DataContext.AddDataSource(DataContextType.GateMovement, gateMovement.GateMovementBooking.GBM_MovementBookingNumber);
			}

			if (!string.IsNullOrEmpty(gateMovement.GateMovementBooking.Booking.GBK_ReferenceNumber))
			{
				shipment.DataContext.AddDataSource(DataContextType.GateBooking, gateMovement.GateMovementBooking.Booking.GBK_ReferenceNumber);
			}
		}

		void PopulateBookingConfirmationReference(GteGateMovement gateMovement, UniversalShipment shipment)
		{
			if (!string.IsNullOrEmpty(gateMovement.GateMovementBooking.GBM_BookingReferenceNumber))
			{
				shipment.BookingConfirmationReference = gateMovement.GateMovementBooking.GBM_BookingReferenceNumber;
			}
		}

		void PopulateContainerCollection(GteGateMovement gateMovement, UniversalShipment shipment)
		{
			var hasUnitType = gateMovement.UnitType != null;
			var hasUnitNumber = !string.IsNullOrEmpty(gateMovement.GGM_UnitNumber);

			shipment.FacilityJobType = new CodeDescriptionPair()
			{
				Code = (hasUnitType && hasUnitNumber) ? FacilityJobType.Codes.Container : FacilityJobType.Codes.Cargo,
				Description = (hasUnitType && hasUnitNumber) ? FacilityJobType.Descriptions.Container : FacilityJobType.Descriptions.Cargo
			};

			if (hasUnitType || hasUnitNumber)
			{
				shipment.SetContainerCollection(() => new DataObjectList<Container>() { new Container() });

				if (hasUnitType)
				{
					shipment.ContainerCollection[0].ContainerType = new ContainerType() { Code = gateMovement.UnitType.RC_Code };
				}

				if (hasUnitNumber)
				{
					shipment.ContainerCollection[0].ContainerNumber = gateMovement.GGM_UnitNumber;
				}
			}
		}

		void PopulatePackingLineCollection(GteGateMovement gateMovement, UniversalShipment shipment)
		{
			if (gateMovement.PackageType != null || gateMovement.CargoType != null)
			{
				shipment.SetPackingLineCollection(() => new DataObjectList<PackingLine>()
				{
					new PackingLine()
				});

				if (!string.IsNullOrEmpty(gateMovement.GGM_RH_NKCargoType))
				{
					shipment.PackingLineCollection[0].Commodity = new Commodity() { Code = gateMovement.GGM_RH_NKCargoType };
				}

				if (!string.IsNullOrEmpty(gateMovement.GGM_F3_NKPackageType))
				{
					shipment.PackingLineCollection[0].PackType = new PackageType() { Code = gateMovement.GGM_F3_NKPackageType };
				}
			}
		}

		void PopulateWarehouseLocation(GteGateMovement gateMovement, UniversalShipment shipment)
		{
			if (gateMovement.Dock != null)
			{
				shipment.WarehouseLocation = gateMovement.Dock.ToLocationString();
				var address = gateMovement.Dock.Warehouse.WarehouseAddress;
				shipment.AddOrgAddress(new OrganizationAddress()
				{
					AddressType = nameof(DocAddressType.LocalCartageYard),
					Address1 = address.Address1,
					Address2 = address.Address2,
					City = address.City,
					Postcode = address.Postcode,
					AddressShortCode = address.OA_Code,
					OrganizationCode = address.Header.OH_Code
				});
			}
		}

		void PopulateTransportBookingDirection(GteGateMovement gateMovement, UniversalShipment shipment)
		{
			shipment.TransportBookingDirection = new TransportBookingDirection()
			{
				Code = gateMovement.GGM_IsPickup ? TransportDirections.Codes.Pickup : TransportDirections.Codes.Delivery,
				Description = gateMovement.GGM_IsPickup ? TransportDirections.Descriptions.Pickup : TransportDirections.Descriptions.Delivery
			};
		}

		void PopulateAdditionalReferenceCollection(GteGateMovement gateMovement, UniversalShipment shipment)
		{
			if (!string.IsNullOrEmpty(gateMovement.GateMovementBooking.GBM_SourceReferenceNumber))
			{
				shipment.AddAdditionalReference(AdditionalReferenceTypes.Codes.BookingPartyReference, AdditionalReferenceTypes.Descriptions.BookingPartyReference, gateMovement.GateMovementBooking.GBM_SourceReferenceNumber);
			}

			if (!string.IsNullOrEmpty(gateMovement.GGM_TransportReference))
			{
				shipment.AddAdditionalReference(AdditionalReferenceTypes.Codes.TransportReference, AdditionalReferenceTypes.Descriptions.TransportReference, gateMovement.GGM_TransportReference);
			}

			shipment.AddAdditionalReference(
				GateManagementConstants.ReferenceTypes.Codes.MovementBookingNumber,
				GateManagementConstants.ReferenceTypes.Descriptions.MovementBookingNumber,
				gateMovement.GateMovementBooking.GBM_MovementBookingNumber
			);
		}
	}
}
