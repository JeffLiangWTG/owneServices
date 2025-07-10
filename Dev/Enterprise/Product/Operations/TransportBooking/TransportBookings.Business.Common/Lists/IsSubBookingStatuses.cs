using Enterprise.ZArchitecture.Core;

namespace Enterprise.TransportBookings.Shared.Lists
{
	public class IsSubBookingStatuses
	{
		public static class Codes
		{
			public const string All = "ALL";
			public const string IsSubBooking = "SUB";
			public const string NotSubBooking = "NOS";
		}

		public static class Descriptions
		{
			public static string All
			{
				get { return Res.GetString("IsSubBookingStatuses|Description|All", "Show ALL Bookings"); }
			}

			public static string IsSubBooking
			{
				get { return Res.GetString("IsSubBookingStatuses|Description|IsSubBooking", "Show Sub Bookings"); }
			}

			public static string NotSubBooking
			{
				get { return Res.GetString("IsSubBookingStatuses|Description|NotSubBooking", "Show NOT Sub Bookings"); }
			}
		}

		public CodeDescriptionPairList List
		{
			get
			{
				if (list == null)
				{
					list = new CodeDescriptionPairList();

					list.AddPair(Codes.All, Descriptions.All);
					list.AddPair(Codes.IsSubBooking, Descriptions.IsSubBooking);
					list.AddPair(Codes.NotSubBooking, Descriptions.NotSubBooking);
				}

				return list;
			}
		}

		CodeDescriptionPairList list;
	}
}
