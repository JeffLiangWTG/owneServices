using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Extensions;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.GateManagement.Business;
using Enterprise.ZArchitecture.Schema;
using GateManagementConstants = Enterprise.Core.Constants.GateManagementConstants;

namespace Enterprise.Warehouse.GateManagement.DataTransfer
{
	public class GteGateMovementDataObjectReader : ShipmentDataObjectReader<GteGateMovement>
	{
		public GteGateMovementDataObjectReader(GteGateMovementBooking movementBooking, Shipment dataObject, IXmlImportLogger logger, UniversalObjectFactory factory) : base(dataObject, logger, factory)
		{
			this.movementBooking = Argument.NotNull(movementBooking, nameof(movementBooking));
		}
		readonly GteGateMovementBooking movementBooking;
		bool existingMovementFound;

		public override DataContextType DataContextType => DataContextType.GateVehicleMovement;

		protected override IMatchingBusinessEntityFinder<GteGateMovement> GetCombinedReferenceMatcher() => null;

		protected override GteGateMovement GetExistingBusinessObjectUsingModuleSpecificBusinessRules()
		{
			var gateMovement = movementBooking.GateMovements.FirstOrDefault(gateMovement => gateMovement.GGM_CancelledReason == "");
			if (gateMovement != null)
			{
				existingMovementFound = true;
			}
			return gateMovement;
		}

		protected override void PopulateBusinessObject(GteGateMovement gateMovement)
		{
			SetValue(gateMovement, GteGateMovementSchema.GGM_GBM_MovementBooking, movementBooking.PK);

			PopulateTransportReference(gateMovement);
			PopulatePickupDirection(gateMovement);
			PopulateDock(gateMovement);
			PopulateCargoAndPackageType(gateMovement);
			PopulateUnitInformation(gateMovement);
		}

		void PopulateTransportReference(GteGateMovement gateMovement)
		{
			var reference = dataObject.GetAdditionalReferenceOrDefault(AdditionalReferenceTypes.Codes.TransportReference);
			if (!string.IsNullOrEmpty(reference))
			{
				SetValue(gateMovement, GteGateMovementSchema.GGM_TransportReference, reference);
			}
			else if (!string.IsNullOrEmpty(movementBooking.GBM_TransportReference))
			{
				SetValue(gateMovement, GteGateMovementSchema.GGM_TransportReference, movementBooking.GBM_TransportReference);
			}
		}

		void PopulatePickupDirection(GteGateMovement gateMovement)
		{
			var pickupCode = dataObject.TransportBookingDirection?.Code?.ToString();
			if (pickupCode == GateManagementConstants.TransportBookingDirections.Codes.Pickup)
			{
				SetValue(gateMovement, GteGateMovementSchema.GGM_IsPickup, true);
			}
			else if (pickupCode == GateManagementConstants.TransportBookingDirections.Codes.Delivery)
			{
				SetValue(gateMovement, GteGateMovementSchema.GGM_IsPickup, false);
			}
			else if (!existingMovementFound)
			{
				SetValue(gateMovement, GteGateMovementSchema.GGM_IsPickup, movementBooking.GBM_IsPickup);
			}
		}

		void PopulateDock(GteGateMovement gateMovement)
		{
			var locationString = dataObject.WarehouseLocation.ToString();
			try
			{
				var facility = FacilityMatchingHelper.GetFacility(dataObject, factory, logger);
				var warehouse = factory.LoadTop1<WhsWarehouse>(new ZQuery(WhsWarehouseSchema.PK, facility.GetValue(WhsWarehouseSchema.PK)));
				if (warehouse != null)
				{
					var location = warehouse.FindLocation(locationString);
					if (location != null)
					{
						SetValue(gateMovement, GteGateMovementSchema.GGM_WL_Dock, location.PK);
					}
				}
				else if (!existingMovementFound)
				{
					SetValue(gateMovement, GteGateMovementSchema.GGM_WL_Dock, movementBooking.GBM_WL_Dock);
				}
			}
			catch (Exception)
			{
			}
		}

		void PopulateCargoAndPackageType(GteGateMovement gateMovement)
		{
			var cargoTypeCode = dataObject.PackingLineCollection?.FirstOrDefault()?.Commodity?.Code ?? "";
			var packTypeCode = dataObject.PackingLineCollection?.FirstOrDefault()?.PackType?.Code ?? "";

			if (!string.IsNullOrEmpty(cargoTypeCode))
			{
				SetValue(gateMovement, GteGateMovementSchema.GGM_RH_NKCargoType, cargoTypeCode);
			}
			else if (!existingMovementFound)
			{
				SetValue(gateMovement, GteGateMovementSchema.GGM_RH_NKCargoType, movementBooking.GBM_RH_NKCargoType);
			}

			if (string.IsNullOrEmpty(gateMovement.GGM_RH_NKCargoType))
			{
				throw new DataObjectReadFailureException(Res.GetString("29fc72b7-022b-48ee-a11e-d997afc0ded2", "Gate Movement must have a Cargo Type"));
			}

			if (!string.IsNullOrEmpty(packTypeCode))
			{
				SetValue(gateMovement, GteGateMovementSchema.GGM_F3_NKPackageType, packTypeCode);
			}
			else if (!existingMovementFound)
			{
				SetValue(gateMovement, GteGateMovementSchema.GGM_F3_NKPackageType, movementBooking.GBM_F3_NKPackageType);
			}

			if (string.IsNullOrEmpty(gateMovement.GGM_F3_NKPackageType))
			{
				throw new DataObjectReadFailureException(Res.GetString("9e939696-12f3-4a12-bda8-0eaa3c4a2a4e", "Gate Movement must have a Package Type"));
			}
		}

		void PopulateUnitInformation(GteGateMovement gateMovement)
		{
			var unitTypeCode = dataObject.ContainerCollection?.FirstOrDefault()?.ContainerType?.Code ?? "";
			var unitNumber = dataObject.ContainerCollection?.FirstOrDefault()?.ContainerNumber ?? "";
			var unitType = factory.LoadFromUniqueKey<RefContainer>(RefContainerSchema.RC_Code, unitTypeCode);

			if (unitType != null)
			{
				SetValue(gateMovement, GteGateMovementSchema.GGM_RC_UnitType, unitType.PK);
			}
			else if (!existingMovementFound)
			{
				SetValue(gateMovement, GteGateMovementSchema.GGM_RC_UnitType, movementBooking.GBM_RC_UnitType);
			}

			if (!string.IsNullOrEmpty(unitNumber))
			{
				SetValue(gateMovement, GteGateMovementSchema.GGM_UnitNumber, unitNumber);
			}
			else if (!existingMovementFound)
			{
				SetValue(gateMovement, GteGateMovementSchema.GGM_UnitNumber, movementBooking.GBM_UnitNumber);
			}
		}
	}
}
