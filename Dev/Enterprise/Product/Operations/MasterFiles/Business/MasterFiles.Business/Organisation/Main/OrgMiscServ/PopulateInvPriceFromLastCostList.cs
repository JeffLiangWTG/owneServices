using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public class PopulateInvPriceFromLastCostList : CodeDescriptionPairList
	{
		public static class Codes
		{
			public const string Yes = "YES";
			public const string No = "NO";
			public const string Blank = "";
		}

		public static class Description
		{
			public static string Yes
			{
				get { return Res.GetString("f5328fb8-6d6b-432c-bf8e-4ebaae020b4b", "The Unit Price of the Commercial Invoice Line defaults to a Product's Last Cost"); }
			}
			public static string No
			{
				get { return Res.GetString("a720c15c-442e-4ae5-9902-458dd4a5da0c", "Unit Price of the Commercial Invoice Line is calculated normally"); }
			}
			public const string Blank = "";
		}

		public PopulateInvPriceFromLastCostList()
		{
			AddPair(Codes.Yes, Description.Yes);
			AddPair(Codes.No, Description.No);
			AddPair(Codes.Blank, Description.Blank);
		}
	}
}
