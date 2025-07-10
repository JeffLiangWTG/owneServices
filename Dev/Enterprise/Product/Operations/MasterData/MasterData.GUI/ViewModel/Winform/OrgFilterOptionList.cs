using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterData.GUI
{
	public class OrgFilterOptionList : CodeDescriptionPairList
	{
		public OrgFilterOptionList()
		{
			AddPair(Descriptions.Contains.GetUnresolvedString(), Descriptions.Contains);
			AddPair(Descriptions.ExactMatch.GetUnresolvedString(), Descriptions.ExactMatch);
			AddPair(Descriptions.StartsWith.GetUnresolvedString(), Descriptions.StartsWith);
			AddPair(Descriptions.NotContain.GetUnresolvedString(), Descriptions.NotContain.GetUnresolvedString());
			AddPair(Descriptions.NotEqual.GetUnresolvedString(), Descriptions.NotEqual);
			AddPair(Descriptions.NotStartWith.GetUnresolvedString(), Descriptions.NotStartWith);
		}

		public static class Descriptions
		{
			public static MultilingualString Contains => TextConstant.Contains;
			public static MultilingualString ExactMatch => TextConstant.ExactMatch;
			public static MultilingualString StartsWith => TextConstant.StartsWith;
			public static MultilingualString NotContain => TextConstant.NotContain;
			public static MultilingualString NotEqual => TextConstant.NotEqual;
			public static MultilingualString NotStartWith => TextConstant.NotStartWith;
		}
	}
}
