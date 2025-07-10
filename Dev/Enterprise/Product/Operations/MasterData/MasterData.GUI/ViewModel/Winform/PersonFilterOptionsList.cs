using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterData.GUI
{
	public class PersonFilterOptionsList : CodeDescriptionPairList
	{
		public PersonFilterOptionsList()
		{
			AddPair(Descriptions.Contains.GetUnresolvedString(), Descriptions.Contains);
			AddPair(Descriptions.ExactMatch.GetUnresolvedString(), Descriptions.ExactMatch);
		}

		public static class Descriptions
		{
			public static MultilingualString Contains => TextConstant.Contains;
			public static MultilingualString ExactMatch => TextConstant.ExactMatch;
		}
	}
}
