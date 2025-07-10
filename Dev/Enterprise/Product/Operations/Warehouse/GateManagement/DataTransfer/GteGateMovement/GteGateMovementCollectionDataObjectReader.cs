using CargoWise.Common;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.Warehouse.GateManagement.Business;
using Enterprise.ZArchitecture.Schema;
using ReferenceTypes = Enterprise.Core.Constants.GateManagementConstants.ReferenceTypes;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Warehouse.GateManagement.DataTransfer
{
	public class GteGateMovementCollectionDataObjectReader : DataObjectCollectionReader<UniversalShipment, GteGateMovement>
	{
		public GteGateMovementCollectionDataObjectReader(GteVehicleMovement vehicleMovement, GteBooking parentBooking, IXmlImportLogger logger, UniversalObjectFactory factory, UniversalShipment[] gateMovementsDataObjectCollection) : base(gateMovementsDataObjectCollection)
		{
			this.parentBooking = Argument.NotNull(parentBooking, nameof(parentBooking));
			this.logger = Argument.NotNull(logger, nameof(logger));
			this.factory = Argument.NotNull(factory, nameof(factory));

			gateMovementCollection = Argument.NotNull(vehicleMovement.GateMovements, nameof(vehicleMovement.GateMovements));
		}
		readonly GteBooking parentBooking;
		readonly IXmlImportLogger logger;
		readonly UniversalObjectFactory factory;
		readonly GteGateMovementCollection gateMovementCollection;

		protected override GteGateMovement[] BusinessObjects => gateMovementCollection.ToArray();

		protected override void AddToCollection(GteGateMovement businessObject) => gateMovementCollection.Add(businessObject);

		protected override void RemoveFromCollection(GteGateMovement businessObject)
		{
			gateMovementCollection.RemoveFromRelationship(businessObject);
			businessObject.Delete();
		}

		protected override GteGateMovement FindMatchingBusinessObject(UniversalShipment dataObject) => null;

		protected override GteGateMovement ReadIntoBusinessObject(UniversalShipment dataObject, GteGateMovement businessObject)
		{
			var movementBookingNumber = dataObject.GetAdditionalReferenceOrDefault(ReferenceTypes.Codes.MovementBookingNumber);
			GteGateMovementBooking movementBooking = null;
			if (!string.IsNullOrEmpty(movementBookingNumber))
			{
				movementBooking = factory.LoadTop1<GteGateMovementBooking>(new ZQuery(GteGateMovementBookingSchema.GBM_MovementBookingNumber, movementBookingNumber));
			}

			var movementBookingCreated = false;
			if (movementBooking == null)
			{
				var sourceReference = dataObject.GetAdditionalReferenceOrDefault(AdditionalReferenceTypes.Codes.BookingPartyReference);
				movementBookingCreated = true;
				movementBooking = factory.New<GteGateMovementBooking>();
				movementBooking.GBM_GBK_Booking = parentBooking.PK;
				movementBooking.GBM_SourceReferenceNumber = sourceReference ?? ZString.Empty;
				movementBooking.GBM_Source = !string.IsNullOrEmpty(sourceReference) ? "VBS" : "";
				movementBooking.GBM_MovementBookingNumber = movementBookingNumber ?? ZString.Empty;
			}

			var reader = new GteGateMovementDataObjectReader(movementBooking, dataObject, logger, factory);
			var gateMovement = reader.ReadIntoBusinessObject();

			if (movementBookingCreated)
			{
				CopyMovementDetailsToBooking(gateMovement, movementBooking);
			}

			return gateMovement;
		}

		void CopyMovementDetailsToBooking(GteGateMovement gateMovement, GteGateMovementBooking movementBooking)
		{
			movementBooking.GBM_TransportReference = gateMovement.GGM_TransportReference;
			movementBooking.GBM_IsPickup = gateMovement.GGM_IsPickup;
			movementBooking.GBM_RH_NKCargoType = gateMovement.GGM_RH_NKCargoType;
			movementBooking.GBM_F3_NKPackageType = gateMovement.GGM_F3_NKPackageType;
			movementBooking.GBM_RC_UnitType = gateMovement.GGM_RC_UnitType;
			movementBooking.GBM_UnitNumber = gateMovement.GGM_UnitNumber;
			movementBooking.GBM_WL_Dock = gateMovement.GGM_WL_Dock;
		}
	}
}
