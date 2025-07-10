using Enterprise.ZArchitecture.Core;

namespace Enterprise.TransportBookings.Shared.Lists
{
	public class RequiresRefrigerationStatuses
	{
		public static class Codes
		{
			public const string All = "ALL";
			public const string RequiresRefrigeration = "REF";
			public const string NotRequiresRefrigeration = "NOT";
		}

		public static class Descriptions
		{
			public static string All
			{
				get { return Res.GetString("RequiresRefrigerationStatuses|Description|All", "Show ALL Bookings"); }
			}

			public static string RequiresRefrigeration
			{
				get { return Res.GetString("RequiresRefrigerationStatuses|Description|RequiresRefrigeration", "Show Bookings that Require Refrigeration"); }
			}

			public static string NotRequiresRefrigeration
			{
				get { return Res.GetString("RequiresRefrigerationStatuses|Description|NotRequiresRefrigeration", "Show Bookings that Require NO Refrigeration"); }
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
					list.AddPair(Codes.RequiresRefrigeration, Descriptions.RequiresRefrigeration);
					list.AddPair(Codes.NotRequiresRefrigeration, Descriptions.NotRequiresRefrigeration);
				}

				return list;
			}
		}

		CodeDescriptionPairList list;
	}
}
