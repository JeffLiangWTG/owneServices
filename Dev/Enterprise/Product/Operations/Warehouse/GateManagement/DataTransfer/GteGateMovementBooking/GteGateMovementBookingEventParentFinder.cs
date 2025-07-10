using CargoWise.EntityFramework;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.Warehouse.GateManagement.Business;
using Enterprise.ZArchitecture.Schema;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.Warehouse.GateManagement.DataTransfer
{
	public class GteGateMovementBookingEventParentFinder : EventParentFinder
	{
		public GteGateMovementBookingEventParentFinder(BusinessObjectFactory factory, IEventDataContextManager manager, IXmlImportLogger logger)
			: base(factory, manager, logger)
		{
		}

		protected override BusinessObject[] GetLogParentsForEventUsingContext(UniversalEvent xmlEvent)
		{
			if (xmlEvent.DataContext != null || xmlEvent.HasMatchingDataTarget(DataContextType.GateMovementBooking))
			{
				var eventValueObject = (IXmlEventValueObject)xmlEvent;
				var shippersReference = eventValueObject.Context.ShippersReference;
				if (shippersReference.IsEmpty)
				{
					return null;
				}

				var query = new ZQuery(GteGateMovementBookingSchema.GBM_SourceReferenceNumber, shippersReference);
				var matchedGteGateMovementBookings = factory.Load<GteGateMovementBooking>(query);

				if (matchedGteGateMovementBookings.Length == 0)
				{
					return null;
				}

				return matchedGteGateMovementBookings;
			}

			return null;
		}
	}
}
