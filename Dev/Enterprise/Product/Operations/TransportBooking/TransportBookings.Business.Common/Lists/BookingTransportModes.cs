using Enterprise.ZArchitecture.Core;

namespace Enterprise.TransportBookings.Shared.Lists
{
	public class BookingTransportModes
	{
		public static class Codes
		{
			public const string RoadTransport = "ROA";
			public const string RailTransport = "RAI";
			public const string InlandWaterways = "IWT";
		}

		public static class Descriptions
		{
			public static string RoadTransport
			{
				get { return Res.GetString("BookingTransportModes|Description|RoadTransport", "Road Transport"); }
			}

			public static string RailTransport
			{
				get { return Res.GetString("BookingTransportModes|Description|RailTransport", "Rail Transport"); }
			}

			public static string InlandWaterways
			{
				get { return Res.GetString("BookingTransportModes|Description|InlandWaterways", "Inland Waterways"); }
			}
		}

		public CodeDescriptionPairList List
		{
			get
			{
				if (list == null)
				{
					list = new CodeDescriptionPairList();

					list.AddPair(Codes.RoadTransport, Descriptions.RoadTransport);
					list.AddPair(Codes.RailTransport, Descriptions.RailTransport);
					list.AddPair(Codes.InlandWaterways, Descriptions.InlandWaterways);
				}

				return list;
			}
		}

		CodeDescriptionPairList list;
	}
}
