using System.Collections.Generic;
using System.Linq;
using Enterprise.Freight.Integration.QuotedBooking;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Freight.Integration
{
	public static class CCABookingValidationHelper
	{
		public static bool AnyNamedAccountMatchesBooking(IQuotedBooking booking, IReadOnlyCollection<IOrgHeader> namedAccounts)
		{
			return namedAccounts.Count == 0 || namedAccounts.Any(namedAccount =>
				booking.ControllingCustomer?.PK == namedAccount.PK ||
				booking.Client?.PK == namedAccount.PK ||
				booking.Consignor?.PK == namedAccount.PK ||
				booking.Consignee?.PK == namedAccount.PK);
		}
	}
}
