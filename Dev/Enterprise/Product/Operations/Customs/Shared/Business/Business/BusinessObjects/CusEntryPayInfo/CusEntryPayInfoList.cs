using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.Business
{
	public class CusEntryPayInfoStatusList : CodeDescriptionPairList
	{
		public static class Codes
		{
			public const string Pending = "PEN";
			public const string Clear = "CLR";
			public const string AwaitingResponse = "AWR";
		}

		public static class Descriptions
		{
			public static string Pending
			{
				get { return Res.GetString("fbe40e38-ee6d-406d-8482-c3f79e9e820e", "Pending"); }
			}
			public static string Clear
			{
				get { return Res.GetString("480e449d-24f3-43fb-84c7-9b3ca0c31d60", "Clear"); }
			}
			public static string AwaitingResponse
			{
				get { return Res.GetString("FCF6BA52-1537-418C-A7DE-32562D3FE0F7", "Awaiting processing via further response"); }
			}
		}

		public CusEntryPayInfoStatusList()
		{
			AddPair(Codes.Pending, Descriptions.Pending);
			AddPair(Codes.Clear, Descriptions.Clear);
			AddPair(Codes.AwaitingResponse, Descriptions.AwaitingResponse);
		}
	}
}
