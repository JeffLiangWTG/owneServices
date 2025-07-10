using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public class DebtorRoleList : CodeDescriptionPairList
	{
		public static class Codes
		{
			public const string NotOwnGoods = "NOW";
			public const string Agent = "AGT";
			public const string NotConsignorOrConsignee = "NCX";
		}

		public static class Descriptions
		{
			public static MultilingualString NotOwnGoods { get { return ResString.GetMultilingualString("DebtorRoleList|NOW", "Does not own goods"); } }
			public static MultilingualString Agent { get { return ResString.GetMultilingualString("DebtorRoleList|AGT", "Flagged as Forwarder/Agent"); } }
			public static MultilingualString NotConsignorOrConsignee { get { return ResString.GetMultilingualString("DebtorRoleList|NCX", "Matches neither Consignor/Consignee"); } }
		}

		public DebtorRoleList()
		{
			AddPair(Codes.NotOwnGoods, Descriptions.NotOwnGoods);
			AddPair(Codes.Agent, Descriptions.Agent);
			AddPair(Codes.NotConsignorOrConsignee, Descriptions.NotConsignorOrConsignee);
		}
	}
}
