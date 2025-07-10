using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.Warehouse.GateManagement.Business;
using Enterprise.ZArchitecture.Schema;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Warehouse.GateManagement.DataTransfer
{
	public class GteVehicleMovementBookingDataObjectReader : ShipmentDataObjectReader<GteVehicleMovementBooking>
	{
		public GteVehicleMovementBookingDataObjectReader(UniversalShipment dataObject, GteVehicleMovementBooking existingBusinessObject, IXmlImportLogger logger, UniversalObjectFactory factory)
			: base(dataObject, logger, factory)
		{
			this.existingBusinessObject = existingBusinessObject;
		}
		readonly GteVehicleMovementBooking existingBusinessObject;

		public override DataContextType DataContextType => DataContextType.GateBooking;

		#region Matching

		protected override IMatchingBusinessEntityFinder<GteVehicleMovementBooking> GetCombinedReferenceMatcher() => null;

		protected override GteVehicleMovementBooking GetExistingBusinessObjectUsingModuleSpecificBusinessRules()
		{
			return existingBusinessObject;
		}

		#endregion Matching

		#region Populate

		protected override void PopulateBusinessObject(GteVehicleMovementBooking vehicleMovement)
		{
			var vehicle = dataObject.VehicleRun.Vehicle;

			if (vehicle.Registration != null && !string.IsNullOrEmpty(vehicle.Registration.Number))
			{
				vehicleMovement.GBV_VehicleRegistration = vehicle.Registration.Number.Value;
			}

			if (vehicle.VehicleType != null)
			{
				var refContainer = factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, (string)vehicle.VehicleType.Code);
				vehicleMovement.GBV_RC_VehicleType = refContainer?.PK ?? ZGuid.Empty;
			}
		}

		#endregion Populate
	}
}
