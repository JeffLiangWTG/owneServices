using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterData.GUI
{
	public class PersonFilterActivesList : CodeDescriptionPairList
	{
		public PersonFilterActivesList()
		{
			AddPair(Descriptions.All.GetUnresolvedString(), Descriptions.All);
			AddPair(Descriptions.Active.GetUnresolvedString(), Descriptions.Active);
			AddPair(Descriptions.Inactive.GetUnresolvedString(), Descriptions.Inactive);
		}

		public static class Descriptions
		{
			public static MultilingualString All => TextConstant.ActiveStatusAll;
			public static MultilingualString Active => TextConstant.ActiveStatusActive;
			public static MultilingualString Inactive => TextConstant.ActiveStatusInactive;
		}
	}
}
