using Enterprise.ZArchitecture.Core;

namespace Enterprise.Rating.Business
{
	/// <summary>
	/// You can use this instead of a tri-state checkbox.
	/// It becomes a Yes, No, (empty) list
	/// </summary>
	public class YesNoEmptyList : CodeDescriptionPairList
	{
		public static class Codes
		{
			public const string Empty = "";
			public const string No = "NO";
			public const string Yes = "YES";
		}

		public static class Descriptions
		{
			public static MultilingualString Empty { get { return ResString.GetMultilingualString("b6016cdd-b634-4c16-b85b-9e149b77de6d", ""); } }
			public static MultilingualString No { get { return ResString.GetMultilingualString("de2cf5f2-4455-4f3b-bc18-80e07128d072", "No"); } }
			public static MultilingualString Yes { get { return ResString.GetMultilingualString("b34964aa-43f8-4b81-8535-8a6c02b0c42c", "Yes"); } }
		}

		public YesNoEmptyList()
		{
			AddPair(Codes.Empty, Descriptions.Empty);
			AddPair(Codes.No, Descriptions.No);
			AddPair(Codes.Yes, Descriptions.Yes);
		}
	}
}
