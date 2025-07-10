using Enterprise.ZArchitecture.Core;

namespace Enterprise.eTail.Business
{
	public static class HVLVReleaseStatus
	{
		public static CodeDescriptionPairList GetAll()
		{
			var result = new CodeDescriptionPairList();
			result.Add(new CodeDescriptionPair(None, NoneDescription));
			result.Add(new CodeDescriptionPair(Held, HeldDescription));
			result.Add(new CodeDescriptionPair(Cleared, ClearedDescription));
			return result;
		}

		public const string None = "NON";
		public const string Held = "HLD";
		public const string Cleared = "CLR";

		public static string NoneDescription => Res.GetString("DF6A16D3-946F-478D-8697-5EEF9D942BDA", "None/Not Available");
		public static string HeldDescription => Res.GetString("A6581409-F3B9-4344-9751-9B25EB4A2699", "Held");
		public static string ClearedDescription => Res.GetString("9B8D40F7-DB5D-471F-BE9E-D40F8ECA3312", "Cleared");

		public static class ShortVersion
		{
			public const string None = "N";
			public const string Held = "H";
			public const string Cleared = "C";
		}
	}
}
