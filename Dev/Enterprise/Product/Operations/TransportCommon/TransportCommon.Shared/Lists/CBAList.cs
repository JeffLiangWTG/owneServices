using Enterprise.ZArchitecture.Core;

namespace Enterprise.TransportCommon.Shared
{
	public class CBAList : CodeDescriptionPairList
	{
		public static class Codes
		{
			public const string SaaS = "SAA";
			public const string SmartFreight = "SMA";
			public const string Teknowlogi = "TEK";
			public const string TMS3G = "3GT";
			public const string Transtream = "TRA";
			public const string Trinium = "TRI";
		}

		public static class Descriptions
		{
			public static string SaaS => (NoResString)"SaaS Transportation";
			public static string SmartFreight => "SmartFreight";
			public static string Teknowlogi => (NoResString)"Teknowlogi";
			public static string TMS3G => "3GTMS";
			public static string Transtream => (NoResString)"Transtream (Pierbridge)";
			public static string Trinium => (NoResString)"Trinium";
		}

		public CBAList()
		{
			AddPair(Codes.TMS3G, Descriptions.TMS3G);
			AddPair(Codes.SaaS, Descriptions.SaaS);
			AddPair(Codes.SmartFreight, Descriptions.SmartFreight);
			AddPair(Codes.Teknowlogi, Descriptions.Teknowlogi);
			AddPair(Codes.Transtream, Descriptions.Transtream);
			AddPair(Codes.Trinium, Descriptions.Trinium);
		}
	}
}
