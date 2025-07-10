using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.Warehouse.GateManagement.Business;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants.GateManagementConstants;
using Event = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.Warehouse.GateManagement.DataTransfer
{
	public class GteGateMovementDataContextManager : EventDataContextManager<GteGateMovement>
	{
		public override DataContextType DataContextType => DataContextType.GateMovement;

		public override ZString DataContextKey => ParentBO.GateMovementBooking.GBM_MovementBookingNumber;

		public override string DefaultOutputDirectory => null;

		protected override ZQuery GetDataContextKeyMatchingQuery(IDataContextMatchingKey matchingValues, BusinessObjectFactory factory, IXmlImportLogger logger)
		{
			var query = new ZDBOnlyQuery(typeof(GteGateMovement));
			query.AddToFilter(GteGateMovementSchema.GGM_CancelledReason, SQLComparisonOperator.Equal, ZString.Empty);
			query.OrderBy = GteGateMovementSchema.GGM_ConfirmedTime.Name;

			var subQuery = new ZDBOnlySubQuery(typeof(GteGateMovementBooking), GteGateMovementSchema.GGM_GBM_MovementBooking, GteGateMovementBookingSchema.PK);
			subQuery.AddToFilter(GteGateMovementBookingSchema.GBM_MovementBookingNumber, matchingValues.Key);
			subQuery.AddToFilter(GteGateMovementBookingSchema.GBM_CancelledReason, SQLComparisonOperator.Equal, ZString.Empty);

			query.AddSubQuery(subQuery, JoinCondition.And);
			return query;
		}

		protected override IEnumerable<KeyValuePair<TypeWithDescription, IZType>> GetEventContextValues()
		{
			var result = new List<KeyValuePair<TypeWithDescription, IZType>>();

			if (ParentBO != null)
			{
				var direction = ParentBO.GGM_IsPickup ? TransportBookingDirections.Codes.Pickup : TransportBookingDirections.Codes.Delivery;

				result.AddIfNotEmpty(Event.ContextTypes.VBSNotificationID, ParentBO.GateMovementBooking.GBM_SourceReferenceNumber);
				result.AddIfNotEmpty(Event.ContextTypes.GateBookingNumber, ParentBO.GateMovementBooking.Booking.GBK_ReferenceNumber);
				result.AddIfNotEmpty(Event.ContextTypes.MovementBookingNumber, ParentBO.GateMovementBooking.GBM_MovementBookingNumber);
				result.AddIfNotEmpty(Event.ContextTypes.Direction, (ZString)direction);
				result.AddIfNotEmpty(Event.ContextTypes.VehicleRegistration, ParentBO.VehicleMovement.GVM_VehicleRegistration);
			}

			return result;
		}

		protected override EventParentFinder GetEventParentFinder(BusinessObjectFactory factory, IXmlImportLogger logger) => new GteGateMovementEventParentFinder(this, factory, logger);
	}
}
