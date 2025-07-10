using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.TransportBookings.Shared;
using Enterprise.TransportCommon.Shared;

namespace Enterprise.TransportBookings.Business
{
	public class DtbBookingParentWrapper : NonPersistentBusinessObject
	{
		public DtbBookingParentWrapper(IDtbBookingParent parent, DtbBookingDirection direction)
		{
			Parent = Argument.NotNull(parent, nameof(parent));
			Direction = direction;
		}

		public IDtbBookingParent Parent { get; }
		public DtbBookingDirection Direction { get; }

		public DtbBookingConsolidation Consolidation => DtbBookingConsolidation.FindExistingTransportBookingConsolidation(Parent.Factory, Parent, Direction);

		public DtbBookingCollection Bookings
		{
			get
			{
				if (bookings == null)
				{
					var consolidation = Consolidation;
					bookings = consolidation != null ? new DtbBookingCollection(consolidation) : new DtbBookingCollection(Parent.Factory, ZQuery.NoResultQuery);
				}

				return bookings;
			}
		}

		DtbBookingCollection bookings;
	}
}
