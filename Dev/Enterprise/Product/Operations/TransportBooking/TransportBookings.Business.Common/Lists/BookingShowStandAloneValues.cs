using Enterprise.ZArchitecture.Core;

namespace Enterprise.TransportBookings.Shared
{
	public class BookingShowStandaloneValues
	{
		public static class Codes
		{
			public const string ShowStandaloneBookingsOnly = "INC";
			public const string ExcludeStandaloneBookings = "EXC";
			public const string All = "ALL";
		}

		public static class Descriptions
		{
			public static string ShowStandaloneBookingsOnly
			{
				get { return Res.GetString("009b8d59-5bb9-4257-9641-11ad911b7d35", "Show Standalone Bookings Only"); }
			}
			public static string ExcludeStandaloneBookings
			{
				get { return Res.GetString("8fd54af7-9881-4064-9017-354c47d87cf4", "Exclude Standalone Bookings"); }
			}
			public static string All
			{
				get { return Res.GetString("2fd2d520-a0d8-4671-a2ee-7196c8f4de08", "Show All"); }
			}
		}

		public CodeDescriptionPairList List
		{
			get
			{
				if (list == null)
				{
					list = new CodeDescriptionPairList();

					list.AddPair(Codes.ShowStandaloneBookingsOnly, Descriptions.ShowStandaloneBookingsOnly);
					list.AddPair(Codes.ExcludeStandaloneBookings, Descriptions.ExcludeStandaloneBookings);
					list.AddPair(Codes.All, Descriptions.All);
				}

				return list;
			}
		}

		CodeDescriptionPairList list;
	}
}
