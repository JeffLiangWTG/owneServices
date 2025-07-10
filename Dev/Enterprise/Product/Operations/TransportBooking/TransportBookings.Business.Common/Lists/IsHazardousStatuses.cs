using Enterprise.ZArchitecture.Core;

namespace Enterprise.TransportBookings.Shared.Lists
{
	public class IsHazardousStatuses
	{
		public static class Codes
		{
			public const string All = "ALL";
			public const string Hazardous = "HAZ";
			public const string NotHazardous = "NOT";
		}

		public static class Descriptions
		{
			public static string All
			{
				get { return Res.GetString("IsHazardousStatuses|Description|All", "Show ALL Bookings"); }
			}

			public static string Hazardous
			{
				get { return Res.GetString("IsHazardousStatuses|Description|Hazardous", "Show Hazardous Bookings"); }
			}

			public static string NotHazardous
			{
				get { return Res.GetString("IsHazardousStatuses|Description|NotHazardous", "Show Bookings that are NOT Hazardous"); }
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
					list.AddPair(Codes.Hazardous, Descriptions.Hazardous);
					list.AddPair(Codes.NotHazardous, Descriptions.NotHazardous);
				}

				return list;
			}
		}

		CodeDescriptionPairList list;
	}
}
