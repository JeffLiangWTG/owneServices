using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.Business
{
	public partial class BillTypeList : CodeDescriptionPairList
	{
		public static class Codes
		{
			public const string HouseBill = "HB";
			public const string MasterBill = "MB";
			public const string SubHouseBill = "SH";
		}

		public static class Descriptions
		{
			public static string HouseBill
			{
				get { return Res.GetString("84c504f2-b759-4c77-ad71-ec50e4c5b2eb", "House Bill"); }
			}
			public static string MasterBill
			{
				get { return Res.GetString("2a244e20-6a51-40db-b238-4c874d5081b0", "Master Bill"); }
			}
			public static string SubHouseBill
			{
				get { return Res.GetString("c948a677-5abd-42be-ada3-e161db141dc4", "Sub House Bill"); }
			}
		}

		public BillTypeList()
		{
			AddPair(Codes.MasterBill, Descriptions.MasterBill);
			AddPair(Codes.HouseBill, Descriptions.HouseBill);
			AddPair(Codes.SubHouseBill, Descriptions.SubHouseBill);
		}
	}
}
