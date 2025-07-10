using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterData.GUI
{
	public class OrgFilterTypeList : CodeDescriptionPairList
	{
		public OrgFilterTypeList()
		{
			AddPair(Descriptions.MainUNLOCO.GetUnresolvedString(), Descriptions.MainUNLOCO);
			AddPair(Descriptions.Name.GetUnresolvedString(), Descriptions.Name);
			AddPair(Descriptions.OrgTypes.GetUnresolvedString(), Descriptions.OrgTypes);
			AddPair(Descriptions.Email.GetUnresolvedString(), Descriptions.Email);
		}

		public static class Descriptions
		{
			public static MultilingualString MainUNLOCO => TextConstant.MainUNLOCO;
			public static MultilingualString Name => TextConstant.Name;
			public static MultilingualString OrgTypes => TextConstant.OrganizationTypes;
			public static MultilingualString Email => TextConstant.Email;
		}
	}
}
