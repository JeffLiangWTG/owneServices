using Enterprise.ZArchitecture.Core;

namespace Enterprise.TransportBookings.Shared.Lists
{
	public class IsOverriddenStatuses
	{
		public static class Codes
		{
			public const string All = "ALL";
			public const string OverridenOrStandalone = "OVR";
			public const string NotOverridden = "NOT";
		}

		public static class Descriptions
		{
			public static string All
			{
				get { return Res.GetString("IsOverriddenStatuses|Description|All", "Show ALL Bookings"); }
			}

			public static string OverridenOrStandalone
			{
				get { return Res.GetString("IsOverriddenStatuses|Description|OverridenOrStandalone", "Show Overridden and Standalone Bookings"); }
			}

			public static string NotOverridden
			{
				get { return Res.GetString("IsOverriddenStatuses|Description|NotOverridden", "Show Bookings that are NOT Overridden"); }
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
					list.AddPair(Codes.OverridenOrStandalone, Descriptions.OverridenOrStandalone);
					list.AddPair(Codes.NotOverridden, Descriptions.NotOverridden);
				}

				return list;
			}
		}

		CodeDescriptionPairList list;
	}
}
