using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.Business
{
	public class PaymentPartyCodeDescriptionList : CodeDescriptionPairList
	{
		public static class Codes
		{
			public const string Default = "DEF";
			public const string Broker = "BRK";
			public const string Importer = "IMP";
		}

		public static class Descriptions
		{
			public static string Default
			{
				get { return Res.GetString("6b98635b-1425-4357-9e19-ffb5977f434f", "Default"); }
			}
			public static string Broker
			{
				get { return Res.GetString("a06a57f2-f433-49f5-ac32-bf502fe529e9", "Broker"); }
			}
			public static string Importer
			{
				get { return Res.GetString("609f044f-d1f1-47fc-9203-6a934a1c6a17", "Importer"); }
			}
		}

		public PaymentPartyCodeDescriptionList()
		{
			AddPair(Codes.Default, Descriptions.Default);
			AddPair(Codes.Broker, Descriptions.Broker);
			AddPair(Codes.Importer, Descriptions.Importer);
		}
	}
}
