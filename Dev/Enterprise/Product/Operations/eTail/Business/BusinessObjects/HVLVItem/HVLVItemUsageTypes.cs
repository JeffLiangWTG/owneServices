using Enterprise.ZArchitecture.Core;

namespace Enterprise.eTail.Business
{
	public class HVLVItemUsageTypes : CodeDescriptionPairList
	{
		public HVLVItemUsageTypes()
		{
			AddPair(Codes.Plus, Descriptions.Plus);
			AddPair(Codes.Standard, Descriptions.Standard);
		}

		public static class Codes
		{
			public const string Plus = "P";
			public const string Standard = "S";
		}

		public static class Descriptions
		{
			public static string Plus => Res.GetString("85fb236c-cf79-469a-ae41-a73e7888c008", "Ecommerce Plus");
			public static string Standard => Res.GetString("0a0dbcf2-9d31-4a1c-a0ac-8507980e885f", "Ecommerce Standard");
		}
	}
}

