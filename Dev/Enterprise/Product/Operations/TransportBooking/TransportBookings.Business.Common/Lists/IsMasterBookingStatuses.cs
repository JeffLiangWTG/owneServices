using Enterprise.ZArchitecture.Core;

namespace Enterprise.TransportBookings.Shared.Lists
{
	public class IsMasterBookingStatuses
	{
		public static class Codes
		{
			public const string All = "ALL";
			public const string IsMasterBooking = "MST";
			public const string NotMasterBooking = "NOT";
		}

		public static class Descriptions
		{
			public static string All
			{
				get { return Res.GetString("IsMasterBookingStatuses|Description|All", "Show ALL Bookings"); }
			}

			public static string IsMasterBooking
			{
				get { return Res.GetString("IsMasterBookingStatuses|Description|IsMasterBooking", "Show Master Bookings"); }
			}

			public static string NotMasterBooking
			{
				get { return Res.GetString("IsMasterBookingStatuses|Description|NotMasterBooking", "Show NOT Master Bookings"); }
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
					list.AddPair(Codes.IsMasterBooking, Descriptions.IsMasterBooking);
					list.AddPair(Codes.NotMasterBooking, Descriptions.NotMasterBooking);
				}

				return list;
			}
		}

		CodeDescriptionPairList list;
	}
}
