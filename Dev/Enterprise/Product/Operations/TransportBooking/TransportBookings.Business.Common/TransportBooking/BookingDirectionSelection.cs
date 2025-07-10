using CargoWise.EntityFramework;
using Enterprise.TransportCommon.Shared;

namespace Enterprise.TransportBookings.Shared
{
	// no reason to test such a basic class, this is a NonPersistentBizO only to implement the heavy interface IBusiness
	[CargoWise.EntityFramework.Testing.TestExcludeBusinessObjectsAllHaveTestCases]
	public class BookingDirectionSelection : NonPersistentBusinessObject
	{
		public BookingDirectionSelection(IDtbBookingParent parent, DtbBookingDirection direction)
		{
			Parent = parent;
			Direction = direction;
		}

		public IDtbBookingParent Parent { get; }
		public DtbBookingDirection Direction { get; }
	}
}
