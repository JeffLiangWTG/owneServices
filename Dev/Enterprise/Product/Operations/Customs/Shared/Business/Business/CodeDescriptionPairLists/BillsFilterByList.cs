using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.Business
{
	public partial class BillFilterByList : CodeDescriptionPairList
	{
		public static class Codes
		{
			public const string All = "ALL";
			public const string HouseBill = BillTypeList.Codes.HouseBill;
			public const string LowestBills = "LB";
			public const string MasterBill = BillTypeList.Codes.MasterBill;
			public const string SubHouseBill = BillTypeList.Codes.SubHouseBill;
		}

		public static class Descriptions
		{
			public static string All
			{
				get { return Res.GetString("5d1da262-258c-45e9-a3cc-1845aaf3e3db", "All"); }
			}
			public static string HouseBill
			{
				get { return Res.GetString("7d03e720-6f3b-4883-8e02-6aa1776f7073", "House Bill Only"); }
			}
			public static string LowestBills
			{
				get { return Res.GetString("24b5595a-a5af-4a61-84c9-2fcac2e79643", "Lowest Bills Only"); }
			}
			public static string MasterBill
			{
				get { return Res.GetString("d54138f3-1859-40a1-a81b-e739e2b916ca", "Master Bill Only"); }
			}
			public static string SubHouseBill
			{
				get { return Res.GetString("956f80dd-3f0b-48df-a927-0128efd053bd", "Sub House Bill Only"); }
			}
		}

		public BillFilterByList()
		{
			AddPair(Codes.All, Descriptions.All);
			AddPair(Codes.MasterBill, Descriptions.MasterBill);
			AddPair(Codes.HouseBill, Descriptions.HouseBill);
			AddPair(Codes.SubHouseBill, Descriptions.SubHouseBill);
			AddPair(Codes.LowestBills, Descriptions.LowestBills);
		}
	}
}
