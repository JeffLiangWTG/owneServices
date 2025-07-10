using Enterprise.ZArchitecture.Core;

namespace Enterprise.TransportBookings.Shared.Lists
{
	public class BookingConsolidatedStatuses
	{
		public static class Codes
		{
			public static string All
			{
				get { return Res.GetString("b79df621-f33e-408c-b3e9-85081e5b2b30", "All"); }
			}

			public const string Consolidated = "CON";
			public const string Unconsolidated = "UNC";
		}

		public static class Descriptions
		{
			public static string All
			{
				get { return Res.GetString("b79df621-f33e-408c-b3e9-85081e5b2b30", "All"); }
			}

			public static string Consolidated
			{
				get { return Res.GetString("5d5de57f-0941-4939-86a6-dc5b1748f1a3", "Consolidated"); }
			}

			public static string Unconsolidated
			{
				get { return Res.GetString("34253287-a9e3-4e92-b233-e7ff202b89d6", "Unconsolidated"); }
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
					list.AddPair(Codes.Consolidated, Descriptions.Consolidated);
					list.AddPair(Codes.Unconsolidated, Descriptions.Unconsolidated);
				}

				return list;
			}
		}

		CodeDescriptionPairList list;
	}
}
